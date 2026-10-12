using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace RayvarzResend.Web.RuleEngine.Sync;

public sealed class RuleEngineStateRepository
{
    private readonly string? _cs;
    private readonly ILogger<RuleEngineStateRepository> _logger;
    private bool _schemaEnsured;

    public RuleEngineStateRepository(IConfiguration config, IOptions<RuleEngineSyncOptions> options, ILogger<RuleEngineStateRepository> logger)
    {
        _logger = logger;
        var name = options.Value.StateConnectionStringName;
        _cs = config.GetConnectionString(name)
            ?? config.GetConnectionString("AppAuth")
            ?? config.GetConnectionString("RayvarzRuleEngine");
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_cs);

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        if (!IsConfigured || _schemaEnsured)
            return;

        const string sql = """
            IF OBJECT_ID(N'dbo.RuleSyncState', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.RuleSyncState (
                    NidMember INT NOT NULL CONSTRAINT PK_RuleSyncState PRIMARY KEY,
                    ActiveEngine NVARCHAR(32) NOT NULL CONSTRAINT DF_RuleSyncState_Engine DEFAULT (N'Legacy'),
                    ActiveXmlBodyHash NVARCHAR(64) NOT NULL CONSTRAINT DF_RuleSyncState_Hash DEFAULT (N''),
                    ActiveMemberVersion INT NOT NULL CONSTRAINT DF_RuleSyncState_Version DEFAULT (0),
                    LastSeenNidHistory BIGINT NULL,
                    LastHistoryModifyUtc DATETIME2(3) NULL,
                    LastMemberXmlBodyHash NVARCHAR(64) NOT NULL CONSTRAINT DF_RuleSyncState_MemberHash DEFAULT (N''),
                    LastCheckAtUtc DATETIME2(3) NULL,
                    LastChangeDetectedAtUtc DATETIME2(3) NULL,
                    LastPromotedAtUtc DATETIME2(3) NULL,
                    LastError NVARCHAR(2000) NULL,
                    UpdatedAtUtc DATETIME2(3) NOT NULL CONSTRAINT DF_RuleSyncState_Updated DEFAULT (SYSUTCDATETIME())
                );
            END

            IF OBJECT_ID(N'dbo.RuleCandidate', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.RuleCandidate (
                    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RuleCandidate PRIMARY KEY,
                    NidMember INT NOT NULL,
                    NidHistory BIGINT NULL,
                    XmlBodyHash NVARCHAR(64) NOT NULL,
                    MemberVersion INT NOT NULL CONSTRAINT DF_RuleCandidate_Version DEFAULT (0),
                    State NVARCHAR(32) NOT NULL,
                    DetectedAtUtc DATETIME2(3) NOT NULL CONSTRAINT DF_RuleCandidate_Detected DEFAULT (SYSUTCDATETIME()),
                    StableSinceUtc DATETIME2(3) NULL,
                    ParseOk BIT NOT NULL CONSTRAINT DF_RuleCandidate_Parse DEFAULT (0),
                    ValidationOk BIT NOT NULL CONSTRAINT DF_RuleCandidate_Val DEFAULT (0),
                    DryRunOk BIT NOT NULL CONSTRAINT DF_RuleCandidate_Dry DEFAULT (0),
                    Notes NVARCHAR(2000) NULL,
                    UpdatedAtUtc DATETIME2(3) NOT NULL CONSTRAINT DF_RuleCandidate_Updated DEFAULT (SYSUTCDATETIME())
                );
                CREATE INDEX IX_RuleCandidate_Member_State ON dbo.RuleCandidate (NidMember, State, DetectedAtUtc DESC);
            END

            IF OBJECT_ID(N'dbo.RulePromotionLog', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.RulePromotionLog (
                    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RulePromotionLog PRIMARY KEY,
                    NidMember INT NOT NULL,
                    CandidateId UNIQUEIDENTIFIER NULL,
                    FromEngine NVARCHAR(32) NOT NULL,
                    ToEngine NVARCHAR(32) NOT NULL,
                    XmlBodyHash NVARCHAR(64) NOT NULL,
                    NidHistory BIGINT NULL,
                    Outcome NVARCHAR(32) NOT NULL,
                    Detail NVARCHAR(2000) NULL,
                    CreatedAtUtc DATETIME2(3) NOT NULL CONSTRAINT DF_RulePromotionLog_Created DEFAULT (SYSUTCDATETIME())
                );
            END

            IF OBJECT_ID(N'dbo.RuleDryRunResult', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.RuleDryRunResult (
                    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RuleDryRunResult PRIMARY KEY,
                    CandidateId UNIQUEIDENTIFIER NULL,
                    NidMember INT NOT NULL,
                    GoldenFicheId INT NULL,
                    Passed BIT NOT NULL,
                    Summary NVARCHAR(2000) NULL,
                    CreatedAtUtc DATETIME2(3) NOT NULL CONSTRAINT DF_RuleDryRunResult_Created DEFAULT (SYSUTCDATETIME())
                );
            END
            """;

        try
        {
            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync(ct);
            _schemaEnsured = true;
        }
        catch (SqlException ex)
        {
            _logger.LogWarning(ex, "RuleEngine sync schema ensure failed — run database/11_RuleEngine_Sync_Schema.sql on RayvarzRuleEngine");
        }
    }

    public async Task<RuleSyncStateRow> GetOrCreateStateAsync(int nidMember, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        var existing = await GetStateAsync(nidMember, ct);
        if (existing != null)
            return existing;

        const string insert = """
            INSERT INTO dbo.RuleSyncState (NidMember) VALUES (@nid)
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using (var ins = new SqlCommand(insert, conn))
        {
            ins.Parameters.AddWithValue("@nid", nidMember);
            await ins.ExecuteNonQueryAsync(ct);
        }

        return await GetStateAsync(nidMember, ct) ?? new RuleSyncStateRow { NidMember = nidMember };
    }

    public async Task<RuleSyncStateRow?> GetStateAsync(int nidMember, CancellationToken ct = default)
    {
        if (!IsConfigured)
            return null;
        await EnsureSchemaAsync(ct);
        const string sql = """
            SELECT NidMember, ActiveEngine, ActiveXmlBodyHash, ActiveMemberVersion,
                   LastSeenNidHistory, LastHistoryModifyUtc, LastMemberXmlBodyHash,
                   LastCheckAtUtc, LastChangeDetectedAtUtc, LastPromotedAtUtc, LastError, UpdatedAtUtc
            FROM dbo.RuleSyncState WHERE NidMember = @nid
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;
        return ReadState(reader);
    }

    public async Task UpdateStateAsync(RuleSyncStateRow row, CancellationToken ct = default)
    {
        if (!IsConfigured)
            return;
        const string sql = """
            UPDATE dbo.RuleSyncState SET
                ActiveEngine = @engine,
                ActiveXmlBodyHash = @activeHash,
                ActiveMemberVersion = @version,
                LastSeenNidHistory = @nidHist,
                LastHistoryModifyUtc = @histUtc,
                LastMemberXmlBodyHash = @memberHash,
                LastCheckAtUtc = @checkUtc,
                LastChangeDetectedAtUtc = @changeUtc,
                LastPromotedAtUtc = @promotedUtc,
                LastError = @err,
                UpdatedAtUtc = SYSUTCDATETIME()
            WHERE NidMember = @nid
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", row.NidMember);
        cmd.Parameters.AddWithValue("@engine", row.ActiveEngine);
        cmd.Parameters.AddWithValue("@activeHash", row.ActiveXmlBodyHash ?? "");
        cmd.Parameters.AddWithValue("@version", row.ActiveMemberVersion);
        cmd.Parameters.AddWithValue("@nidHist", (object?)row.LastSeenNidHistory ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@histUtc", (object?)row.LastHistoryModifyUtc ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@memberHash", row.LastMemberXmlBodyHash ?? "");
        cmd.Parameters.AddWithValue("@checkUtc", (object?)row.LastCheckAtUtc ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@changeUtc", (object?)row.LastChangeDetectedAtUtc ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@promotedUtc", (object?)row.LastPromotedAtUtc ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@err", (object?)row.LastError ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<RuleCandidateRow?> GetOpenCandidateAsync(int nidMember, CancellationToken ct = default)
    {
        if (!IsConfigured)
            return null;
        const string sql = """
            SELECT TOP 1 Id, NidMember, NidHistory, XmlBodyHash, MemberVersion, State,
                   DetectedAtUtc, StableSinceUtc, ParseOk, ValidationOk, DryRunOk, Notes
            FROM dbo.RuleCandidate
            WHERE NidMember = @nid AND State NOT IN (N'Promoted', N'Rejected')
            ORDER BY DetectedAtUtc DESC
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? ReadCandidate(reader) : null;
    }

    public async Task InsertCandidateAsync(RuleCandidateRow row, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO dbo.RuleCandidate
                (Id, NidMember, NidHistory, XmlBodyHash, MemberVersion, State, DetectedAtUtc, StableSinceUtc,
                 ParseOk, ValidationOk, DryRunOk, Notes)
            VALUES
                (@id, @nid, @hist, @hash, @ver, @state, @detected, @stable, @parse, @val, @dry, @notes)
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", row.Id);
        cmd.Parameters.AddWithValue("@nid", row.NidMember);
        cmd.Parameters.AddWithValue("@hist", (object?)row.NidHistory ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@hash", row.XmlBodyHash);
        cmd.Parameters.AddWithValue("@ver", row.MemberVersion);
        cmd.Parameters.AddWithValue("@state", row.State.ToString());
        cmd.Parameters.AddWithValue("@detected", row.DetectedAtUtc);
        cmd.Parameters.AddWithValue("@stable", (object?)row.StableSinceUtc ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@parse", row.ParseOk);
        cmd.Parameters.AddWithValue("@val", row.ValidationOk);
        cmd.Parameters.AddWithValue("@dry", row.DryRunOk);
        cmd.Parameters.AddWithValue("@notes", (object?)row.Notes ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task UpdateCandidateAsync(RuleCandidateRow row, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE dbo.RuleCandidate SET
                State = @state, StableSinceUtc = @stable, ParseOk = @parse, ValidationOk = @val,
                DryRunOk = @dry, Notes = @notes, UpdatedAtUtc = SYSUTCDATETIME()
            WHERE Id = @id
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", row.Id);
        cmd.Parameters.AddWithValue("@state", row.State.ToString());
        cmd.Parameters.AddWithValue("@stable", (object?)row.StableSinceUtc ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@parse", row.ParseOk);
        cmd.Parameters.AddWithValue("@val", row.ValidationOk);
        cmd.Parameters.AddWithValue("@dry", row.DryRunOk);
        cmd.Parameters.AddWithValue("@notes", (object?)row.Notes ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task InsertPromotionLogAsync(
        int nidMember,
        Guid? candidateId,
        string fromEngine,
        string toEngine,
        string xmlBodyHash,
        long? nidHistory,
        string outcome,
        string? detail,
        CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO dbo.RulePromotionLog
                (Id, NidMember, CandidateId, FromEngine, ToEngine, XmlBodyHash, NidHistory, Outcome, Detail)
            VALUES (@id, @nid, @cid, @from, @to, @hash, @hist, @outcome, @detail)
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
        cmd.Parameters.AddWithValue("@nid", nidMember);
        cmd.Parameters.AddWithValue("@cid", (object?)candidateId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@from", fromEngine);
        cmd.Parameters.AddWithValue("@to", toEngine);
        cmd.Parameters.AddWithValue("@hash", xmlBodyHash);
        cmd.Parameters.AddWithValue("@hist", (object?)nidHistory ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@outcome", outcome);
        cmd.Parameters.AddWithValue("@detail", (object?)detail ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task InsertDryRunResultAsync(
        Guid? candidateId,
        int nidMember,
        int? goldenFicheId,
        bool passed,
        string? summary,
        CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO dbo.RuleDryRunResult (Id, CandidateId, NidMember, GoldenFicheId, Passed, Summary)
            VALUES (@id, @cid, @nid, @gf, @passed, @summary)
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
        cmd.Parameters.AddWithValue("@cid", (object?)candidateId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        cmd.Parameters.AddWithValue("@gf", (object?)goldenFicheId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@passed", passed);
        cmd.Parameters.AddWithValue("@summary", (object?)summary ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<int> CountActiveGoldensAsync(CancellationToken ct = default)
    {
        if (!IsConfigured)
            return 0;
        try
        {
            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand("SELECT COUNT(1) FROM dbo.RuleGoldenFiche WHERE IsActive = 1", conn);
            var scalar = await cmd.ExecuteScalarAsync(ct);
            return scalar == null ? 0 : Convert.ToInt32(scalar);
        }
        catch (SqlException)
        {
            return 0;
        }
    }

    private static RuleSyncStateRow ReadState(SqlDataReader reader) => new()
    {
        NidMember = reader.GetInt32(0),
        ActiveEngine = reader.GetString(1),
        ActiveXmlBodyHash = reader.GetString(2),
        ActiveMemberVersion = reader.GetInt32(3),
        LastSeenNidHistory = reader.IsDBNull(4) ? null : reader.GetInt64(4),
        LastHistoryModifyUtc = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
        LastMemberXmlBodyHash = reader.GetString(6),
        LastCheckAtUtc = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
        LastChangeDetectedAtUtc = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
        LastPromotedAtUtc = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
        LastError = reader.IsDBNull(10) ? null : reader.GetString(10),
        UpdatedAtUtc = reader.GetDateTime(11)
    };

    private static RuleCandidateRow ReadCandidate(SqlDataReader reader) => new()
    {
        Id = reader.GetGuid(0),
        NidMember = reader.GetInt32(1),
        NidHistory = reader.IsDBNull(2) ? null : reader.GetInt64(2),
        XmlBodyHash = reader.GetString(3),
        MemberVersion = reader.GetInt32(4),
        State = Enum.TryParse<RuleCandidateState>(reader.GetString(5), out var st) ? st : RuleCandidateState.Detected,
        DetectedAtUtc = reader.GetDateTime(6),
        StableSinceUtc = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
        ParseOk = reader.GetBoolean(8),
        ValidationOk = reader.GetBoolean(9),
        DryRunOk = reader.GetBoolean(10),
        Notes = reader.IsDBNull(11) ? null : reader.GetString(11)
    };
}
