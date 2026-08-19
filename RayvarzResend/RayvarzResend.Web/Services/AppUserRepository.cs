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

    public async Task<AppUserRecord> CreateUserAsync(CreateAppUserRequest req, CancellationToken ct = default)
    {
        if (_useInMemory)
            return _memory.Add(req);

        await EnsureSchemaAsync(ct);
        var username = (req.Username ?? "").Trim();
        if (username.Length < 3)
            throw new ArgumentException("نام کاربری حداقل ۳ کاراکتر باشد");
        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6)
            throw new ArgumentException("رمز عبور حداقل ۶ کاراکتر باشد");
        if (!req.IsAdmin && string.IsNullOrWhiteSpace(req.District))
            throw new ArgumentException("برای کاربر منطقه‌ای، انتخاب منطقه الزامی است");

        var user = new AppUserRecord
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = PasswordHasherUtil.Hash(req.Password!),
            FirstName = (req.FirstName ?? "").Trim(),
            LastName = (req.LastName ?? "").Trim(),
            NationalId = (req.NationalId ?? "").Trim(),
            Position = (req.Position ?? "").Trim(),
            District = (req.District ?? "").Trim(),
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
            throw new InvalidOperationException("نام کاربری تکراری است");
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
