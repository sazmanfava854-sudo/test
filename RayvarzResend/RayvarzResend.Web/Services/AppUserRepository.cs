using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public sealed class AppUserRepository
{
    private readonly string? _cs;
    private readonly bool _useInMemory;
    private readonly InMemoryAppUserStore _memory;
    private readonly ILogger<AppUserRepository> _logger;
    private bool _schemaEnsured;

    public AppUserRepository(IConfiguration config, InMemoryAppUserStore memory, ILogger<AppUserRepository> logger)
    {
        _useInMemory = config.GetValue("Auth:UseInMemoryStore", false);
        _cs = _useInMemory ? null : ResolveConnectionString(config);
        _memory = memory;
        _logger = logger;
    }

    public bool IsConfigured => _useInMemory || !string.IsNullOrWhiteSpace(_cs);

    private static string? ResolveConnectionString(IConfiguration config) =>
        config.GetConnectionString("AppAuth")
        ?? config.GetConnectionString("RayvarzRuleEngine")
        ?? config.GetConnectionString("Sara");

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        if (_useInMemory || !IsConfigured || _schemaEnsured)
            return;

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        const string sql = """
            IF OBJECT_ID(N'dbo.AppUser', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.AppUser (
                    Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AppUser PRIMARY KEY,
                    Username        NVARCHAR(100)    NOT NULL,
                    PasswordHash    NVARCHAR(500)    NOT NULL,
                    FirstName       NVARCHAR(100)    NOT NULL CONSTRAINT DF_AppUser_FirstName DEFAULT (N''),
                    LastName        NVARCHAR(100)    NOT NULL CONSTRAINT DF_AppUser_LastName DEFAULT (N''),
                    NationalId      NVARCHAR(20)     NOT NULL CONSTRAINT DF_AppUser_NationalId DEFAULT (N''),
                    Position        NVARCHAR(200)    NOT NULL CONSTRAINT DF_AppUser_Position DEFAULT (N''),
                    District        NVARCHAR(50)     NOT NULL CONSTRAINT DF_AppUser_District DEFAULT (N''),
                    IsAdmin         BIT              NOT NULL CONSTRAINT DF_AppUser_IsAdmin DEFAULT (0),
                    IsActive        BIT              NOT NULL CONSTRAINT DF_AppUser_IsActive DEFAULT (1),
                    CreatedAtUtc    DATETIME2(3)     NOT NULL CONSTRAINT DF_AppUser_Created DEFAULT (SYSUTCDATETIME()),
                    CONSTRAINT UQ_AppUser_Username UNIQUE (Username)
                );
                CREATE INDEX IX_AppUser_Active ON dbo.AppUser (IsActive) INCLUDE (Username, IsAdmin);
            END

            IF OBJECT_ID(N'dbo.AppUserGroup', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.AppUserGroup (
                    Id                      UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AppUserGroup PRIMARY KEY,
                    Name                    NVARCHAR(100)    NOT NULL,
                    CanAccessUnsentFiches   BIT              NOT NULL CONSTRAINT DF_AppUserGroup_Unsent DEFAULT (0),
                    CanAccessInstallment    BIT              NOT NULL CONSTRAINT DF_AppUserGroup_Installment DEFAULT (0),
                    CanManageUsers          BIT              NOT NULL CONSTRAINT DF_AppUserGroup_Users DEFAULT (0),
                    CreatedAtUtc            DATETIME2(3)     NOT NULL CONSTRAINT DF_AppUserGroup_Created DEFAULT (SYSUTCDATETIME())
                );
                CREATE UNIQUE INDEX UQ_AppUserGroup_Name ON dbo.AppUserGroup (Name);
            END

            IF OBJECT_ID(N'dbo.AppUserGroupMember', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.AppUserGroupMember (
                    UserId  UNIQUEIDENTIFIER NOT NULL,
                    GroupId UNIQUEIDENTIFIER NOT NULL,
                    CONSTRAINT PK_AppUserGroupMember PRIMARY KEY (UserId, GroupId),
                    CONSTRAINT FK_AppUserGroupMember_User FOREIGN KEY (UserId) REFERENCES dbo.AppUser (Id),
                    CONSTRAINT FK_AppUserGroupMember_Group FOREIGN KEY (GroupId) REFERENCES dbo.AppUserGroup (Id)
                );
            END
            """;
        await using var cmd = new SqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync(ct);
        _schemaEnsured = true;
        _logger.LogInformation("AppUser schema ensured");
    }

    public async Task<int> CountUsersAsync(CancellationToken ct = default)
    {
        if (_useInMemory)
            return _memory.Count;

        await EnsureSchemaAsync(ct);
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.AppUser", conn);
        var result = await cmd.ExecuteScalarAsync(ct);
        return result is int i ? i : Convert.ToInt32(result);
    }

    public async Task<AppUserRecord?> FindByUsernameAsync(string username, CancellationToken ct = default)
    {
        if (_useInMemory)
            return _memory.FindByUsername(username);

        await EnsureSchemaAsync(ct);
        const string sql = """
            SELECT TOP 1 Id, Username, PasswordHash, FirstName, LastName, NationalId, Position, District,
                   IsAdmin, IsActive, CreatedAtUtc
            FROM dbo.AppUser
            WHERE Username = @u
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@u", username.Trim());
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? ReadUser(reader) : null;
    }

    public async Task<AppUserRecord?> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (_useInMemory)
            return _memory.FindById(id);

        await EnsureSchemaAsync(ct);
        const string sql = """
            SELECT TOP 1 Id, Username, PasswordHash, FirstName, LastName, NationalId, Position, District,
                   IsAdmin, IsActive, CreatedAtUtc
            FROM dbo.AppUser
            WHERE Id = @id
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? ReadUser(reader) : null;
    }

    public async Task<List<AppUserDto>> ListUsersAsync(CancellationToken ct = default)
    {
        if (_useInMemory)
            return _memory.List();

        await EnsureSchemaAsync(ct);
        const string sql = """
            SELECT Id, Username, FirstName, LastName, NationalId, Position, District, IsAdmin, IsActive, CreatedAtUtc
            FROM dbo.AppUser
            ORDER BY CreatedAtUtc DESC, Username
            """;
        var list = new List<AppUserDto>();
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            list.Add(new AppUserDto
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                NationalId = reader.GetString(reader.GetOrdinal("NationalId")),
                Position = reader.GetString(reader.GetOrdinal("Position")),
                District = reader.GetString(reader.GetOrdinal("District")),
                IsAdmin = reader.GetBoolean(reader.GetOrdinal("IsAdmin")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")).ToString("O")
            });
        }

        return list;
    }

    public async Task<List<AppUserDto>> ListUsersWithGroupsAsync(CancellationToken ct = default)
    {
        var users = await ListUsersAsync(ct);
        if (users.Count == 0)
            return users;

        var memberships = await ListAllGroupMembershipsAsync(ct);
        foreach (var user in users)
            user.GroupIds = memberships.Where(m => m.UserId == user.Id).Select(m => m.GroupId).ToList();

        return users;
    }

    public async Task<List<AppUserGroupDto>> ListGroupsAsync(CancellationToken ct = default)
    {
        if (_useInMemory)
            return _memory.ListGroups();

        await EnsureSchemaAsync(ct);
        const string sql = """
            SELECT Id, Name, CanAccessUnsentFiches, CanAccessInstallment, CanManageUsers, CreatedAtUtc
            FROM dbo.AppUserGroup
            ORDER BY Name
            """;
        var list = new List<AppUserGroupDto>();
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            list.Add(ReadGroup(reader));
        return list;
    }

    public async Task<AppUserGroupDto> CreateGroupAsync(CreateAppUserGroupRequest req, CancellationToken ct = default)
    {
        ValidateGroupName(req.Name);
        if (_useInMemory)
            return _memory.AddGroup(req);

        await EnsureSchemaAsync(ct);
        var group = NewGroupRecord(req);
        const string sql = """
            INSERT INTO dbo.AppUserGroup
                (Id, Name, CanAccessUnsentFiches, CanAccessInstallment, CanManageUsers, CreatedAtUtc)
            VALUES (@id, @name, @unsent, @installment, @users, @created)
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", group.Id);
        cmd.Parameters.AddWithValue("@name", group.Name);
        cmd.Parameters.AddWithValue("@unsent", group.CanAccessUnsentFiches);
        cmd.Parameters.AddWithValue("@installment", group.CanAccessInstallment);
        cmd.Parameters.AddWithValue("@users", group.CanManageUsers);
        cmd.Parameters.AddWithValue("@created", group.CreatedAtUtc);
        try
        {
            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            throw new InvalidOperationException("گروه با این نام قبلاً ثبت شده است");
        }

        return ToGroupDto(group);
    }

    public async Task<AppUserGroupDto> UpdateGroupAsync(Guid id, UpdateAppUserGroupRequest req, CancellationToken ct = default)
    {
        ValidateGroupName(req.Name);
        if (_useInMemory)
            return _memory.UpdateGroup(id, req);

        await EnsureSchemaAsync(ct);
        const string sql = """
            UPDATE dbo.AppUserGroup
            SET Name = @name,
                CanAccessUnsentFiches = @unsent,
                CanAccessInstallment = @installment,
                CanManageUsers = @users
            WHERE Id = @id
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@name", req.Name!.Trim());
        cmd.Parameters.AddWithValue("@unsent", req.CanAccessUnsentFiches);
        cmd.Parameters.AddWithValue("@installment", req.CanAccessInstallment);
        cmd.Parameters.AddWithValue("@users", req.CanManageUsers);
        var affected = await cmd.ExecuteNonQueryAsync(ct);
        if (affected == 0)
            throw new InvalidOperationException("گروه یافت نشد");

        var groups = await ListGroupsAsync(ct);
        return groups.First(g => g.Id == id);
    }

    public async Task<List<Guid>> GetUserGroupIdsAsync(Guid userId, CancellationToken ct = default)
    {
        if (_useInMemory)
            return _memory.GetUserGroupIds(userId);

        await EnsureSchemaAsync(ct);
        const string sql = "SELECT GroupId FROM dbo.AppUserGroupMember WHERE UserId = @id";
        var ids = new List<Guid>();
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", userId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            ids.Add(reader.GetGuid(0));
        return ids;
    }

    public async Task SetUserGroupsAsync(Guid userId, IReadOnlyList<Guid> groupIds, CancellationToken ct = default)
    {
        if (_useInMemory)
        {
            _memory.SetUserGroups(userId, groupIds);
            return;
        }

        await EnsureSchemaAsync(ct);
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var del = new SqlCommand("DELETE FROM dbo.AppUserGroupMember WHERE UserId = @id", conn);
        del.Parameters.AddWithValue("@id", userId);
        await del.ExecuteNonQueryAsync(ct);

        foreach (var groupId in groupIds.Distinct())
        {
            await using var ins = new SqlCommand(
                "INSERT INTO dbo.AppUserGroupMember (UserId, GroupId) VALUES (@u, @g)", conn);
            ins.Parameters.AddWithValue("@u", userId);
            ins.Parameters.AddWithValue("@g", groupId);
            await ins.ExecuteNonQueryAsync(ct);
        }
    }

    public async Task<AppUserDto> UpdateUserAsync(Guid id, UpdateAppUserRequest req, CancellationToken ct = default)
    {
        if (_useInMemory)
            return _memory.UpdateUser(id, req);

        await EnsureSchemaAsync(ct);
        var user = await FindByIdAsync(id, ct);
        if (user == null)
            throw new InvalidOperationException("کاربر یافت نشد");

        if (req.IsAdmin.HasValue)
            user.IsAdmin = req.IsAdmin.Value;
        if (req.IsActive.HasValue)
            user.IsActive = req.IsActive.Value;

        const string sql = """
            UPDATE dbo.AppUser
            SET IsAdmin = @admin, IsActive = @active
            WHERE Id = @id
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@admin", user.IsAdmin);
        cmd.Parameters.AddWithValue("@active", user.IsActive);
        await cmd.ExecuteNonQueryAsync(ct);

        if (req.GroupIds != null)
            await SetUserGroupsAsync(id, req.GroupIds, ct);

        var dto = (await ListUsersWithGroupsAsync(ct)).First(u => u.Id == id);
        return dto;
    }

    public async Task ResetPasswordAsync(Guid id, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new ArgumentException("رمز عبور حداقل ۶ کاراکتر باشد");

        if (_useInMemory)
        {
            _memory.ResetPassword(id, password);
            return;
        }

        await EnsureSchemaAsync(ct);
        const string sql = "UPDATE dbo.AppUser SET PasswordHash = @hash WHERE Id = @id";
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@hash", PasswordHasherUtil.Hash(password));
        var affected = await cmd.ExecuteNonQueryAsync(ct);
        if (affected == 0)
            throw new InvalidOperationException("کاربر یافت نشد");
    }

    private async Task<List<(Guid UserId, Guid GroupId)>> ListAllGroupMembershipsAsync(CancellationToken ct)
    {
        if (_useInMemory)
            return _memory.ListMemberships();

        await EnsureSchemaAsync(ct);
        const string sql = "SELECT UserId, GroupId FROM dbo.AppUserGroupMember";
        var list = new List<(Guid, Guid)>();
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            list.Add((reader.GetGuid(0), reader.GetGuid(1)));
        return list;
    }

    private static void ValidateGroupName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("نام گروه الزامی است");
    }

    private static AppUserGroupRecord NewGroupRecord(CreateAppUserGroupRequest req) => new()
    {
        Id = Guid.NewGuid(),
        Name = req.Name!.Trim(),
        CanAccessUnsentFiches = req.CanAccessUnsentFiches,
        CanAccessInstallment = req.CanAccessInstallment,
        CanManageUsers = req.CanManageUsers,
        CreatedAtUtc = DateTime.UtcNow
    };

    private static AppUserGroupDto ToGroupDto(AppUserGroupRecord group) => new()
    {
        Id = group.Id,
        Name = group.Name,
        CanAccessUnsentFiches = group.CanAccessUnsentFiches,
        CanAccessInstallment = group.CanAccessInstallment,
        CanManageUsers = group.CanManageUsers,
        CreatedAtUtc = group.CreatedAtUtc.ToString("O")
    };

    private static AppUserGroupDto ReadGroup(SqlDataReader reader) => new()
    {
        Id = reader.GetGuid(reader.GetOrdinal("Id")),
        Name = reader.GetString(reader.GetOrdinal("Name")),
        CanAccessUnsentFiches = reader.GetBoolean(reader.GetOrdinal("CanAccessUnsentFiches")),
        CanAccessInstallment = reader.GetBoolean(reader.GetOrdinal("CanAccessInstallment")),
        CanManageUsers = reader.GetBoolean(reader.GetOrdinal("CanManageUsers")),
        CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")).ToString("O")
    };

    public async Task<AppUserRecord> CreateUserAsync(CreateAppUserRequest req, CancellationToken ct = default)
    {
        if (_useInMemory)
            return _memory.Add(req);

        await EnsureSchemaAsync(ct);
        AppUserInputNormalizer.ValidateAndApply(req);
        var username = req.Username!;

        var user = new AppUserRecord
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = PasswordHasherUtil.Hash(req.Password!),
            FirstName = req.FirstName,
            LastName = req.LastName,
            NationalId = req.NationalId,
            Position = req.Position,
            District = req.District,
            IsAdmin = req.IsAdmin,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        const string sql = """
            INSERT INTO dbo.AppUser
                (Id, Username, PasswordHash, FirstName, LastName, NationalId, Position, District, IsAdmin, IsActive, CreatedAtUtc)
            VALUES
                (@id, @u, @hash, @fn, @ln, @nid, @pos, @dist, @admin, 1, @created)
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", user.Id);
        cmd.Parameters.AddWithValue("@u", user.Username);
        cmd.Parameters.AddWithValue("@hash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@fn", user.FirstName);
        cmd.Parameters.AddWithValue("@ln", user.LastName);
        cmd.Parameters.AddWithValue("@nid", user.NationalId);
        cmd.Parameters.AddWithValue("@pos", user.Position);
        cmd.Parameters.AddWithValue("@dist", user.District);
        cmd.Parameters.AddWithValue("@admin", user.IsAdmin);
        cmd.Parameters.AddWithValue("@created", user.CreatedAtUtc);
        try
        {
            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            throw new InvalidOperationException("کاربر با این کد ملی قبلاً ثبت شده است");
        }

        return user;
    }

    private static AppUserRecord ReadUser(SqlDataReader reader) => new()
    {
        Id = reader.GetGuid(reader.GetOrdinal("Id")),
        Username = reader.GetString(reader.GetOrdinal("Username")),
        PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
        LastName = reader.GetString(reader.GetOrdinal("LastName")),
        NationalId = reader.GetString(reader.GetOrdinal("NationalId")),
        Position = reader.GetString(reader.GetOrdinal("Position")),
        District = reader.GetString(reader.GetOrdinal("District")),
        IsAdmin = reader.GetBoolean(reader.GetOrdinal("IsAdmin")),
        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
        CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
    };
}
