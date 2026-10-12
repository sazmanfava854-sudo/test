using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace RayvarzResend.Web.RuleEngine.Sync;

public sealed class RuleEngineStateRepository
{
    private readonly string? _cs;
    private readonly RuleEngineSyncOptions _options;
    private readonly ILogger<RuleEngineStateRepository> _logger;
    private bool _schemaVerified;

    public RuleEngineStateRepository(IConfiguration config, IOptions<RuleEngineSyncOptions> options, ILogger<RuleEngineStateRepository> logger)
    {
        _logger = logger;
        _options = options.Value;
        var name = _options.StateConnectionStringName;
        _cs = config.GetConnectionString(name)
            ?? config.GetConnectionString("AppAuth")
            ?? config.GetConnectionString("RayvarzRuleEngine");
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_cs);

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        if (!IsConfigured || _schemaVerified)
            return;

        const string probe = "SELECT TOP 0 LastStableXmlHash, LastSeenNidHistory FROM dbo.RuleSyncState";
        try
        {
            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(probe, conn);
            await cmd.ExecuteNonQueryAsync(ct);
            _schemaVerified = true;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex,
                "RuleSyncState schema mismatch — use existing RayvarzRuleEngine tables (LastStableXmlHash, RuleCandidate.CandidateId, …)");
            throw;
        }
    }

    public async Task<RuleSyncStateRow> GetOrCreateStateAsync(int nidMember, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        var existing = await GetStateAsync(nidMember, ct);
        if (existing != null)
            return existing;

        const string insert = """
            INSERT INTO dbo.RuleSyncState
                (NidMember, NidClass, ActiveEngine, LastStableXmlHash, ActiveDslVersion, ConsecutiveDynamicFailures, UpdatedAtUtc)
            VALUES
                (@nid, @class, N'Legacy', N'', NULL, 0, SYSUTCDATETIME())
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var ins = new SqlCommand(insert, conn);
        ins.Parameters.AddWithValue("@nid", nidMember);
        ins.Parameters.AddWithValue("@class", _options.NidClass);
        await ins.ExecuteNonQueryAsync(ct);

        return await GetStateAsync(nidMember, ct) ?? new RuleSyncStateRow { NidMember = nidMember, NidClass = _options.NidClass };
    }

    public async Task<RuleSyncStateRow?> GetStateAsync(int nidMember, CancellationToken ct = default)
    {
        if (!IsConfigured)
            return null;
        await EnsureSchemaAsync(ct);
        const string sql = """
            SELECT NidMember, NidClass, LastSeenNidHistory, LastSeenModifyAt,
                   LastStableNidHistory, LastStableModifyAt, LastStableXmlHash,
                   ActiveDslVersion, ActiveEngine, ActiveSnapshotId, UpdatedAtUtc,
                   ConsecutiveDynamicFailures, CircuitBreakerOpenUntilUtc
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
                NidClass = @class,
                LastSeenNidHistory = @seenHist,
                LastSeenModifyAt = @seenMod,
                LastStableNidHistory = @stableHist,
                LastStableModifyAt = @stableMod,
                LastStableXmlHash = @stableHash,
                ActiveDslVersion = @dslVer,
                ActiveEngine = @engine,
                ActiveSnapshotId = @snapshot,
                ConsecutiveDynamicFailures = @failures,
                CircuitBreakerOpenUntilUtc = @breaker,
                UpdatedAtUtc = SYSUTCDATETIME()
            WHERE NidMember = @nid
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", row.NidMember);
        cmd.Parameters.AddWithValue("@class", row.NidClass);
        cmd.Parameters.AddWithValue("@seenHist", (object?)row.LastSeenNidHistory ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@seenMod", SqlReaderValueCoercion.ToSqlDateTimeParameter(row.LastSeenModifyAt));
        cmd.Parameters.AddWithValue("@stableHist", (object?)row.LastStableNidHistory ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@stableMod", SqlReaderValueCoercion.ToSqlDateTimeParameter(row.LastStableModifyAt));
        cmd.Parameters.AddWithValue("@stableHash", row.LastStableXmlHash ?? "");
        cmd.Parameters.AddWithValue("@dslVer", (object?)row.ActiveDslVersion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@engine", row.ActiveEngine);
        cmd.Parameters.AddWithValue("@snapshot", (object?)row.ActiveSnapshotId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@failures", row.ConsecutiveDynamicFailures);
        cmd.Parameters.AddWithValue("@breaker", SqlReaderValueCoercion.ToSqlDateTimeParameter(row.CircuitBreakerOpenUntilUtc));
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<RuleCandidateRow?> GetOpenCandidateAsync(int nidMember, CancellationToken ct = default)
    {
        if (!IsConfigured)
            return null;
        const string sql = """
            SELECT TOP 1 CandidateId, NidMember, SourceNidHistory, SourceModifyAt, CanonicalXmlHash, XmlBody,
                   Modifyer, ModifyDesc, Status, RejectReason, StableEligibleAtUtc, FirstSeenAtUtc, LastCheckedAtUtc
            FROM dbo.RuleCandidate
            WHERE NidMember = @nid AND Status NOT IN (N'Promoted', N'Rejected')
            ORDER BY FirstSeenAtUtc DESC
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? ReadCandidate(reader) : null;
    }

    public async Task<RuleCandidateRow?> GetCandidateByIdAsync(long candidateId, CancellationToken ct = default)
    {
        if (!IsConfigured)
            return null;
        const string sql = """
            SELECT CandidateId, NidMember, SourceNidHistory, SourceModifyAt, CanonicalXmlHash, XmlBody,
                   Modifyer, ModifyDesc, Status, RejectReason, StableEligibleAtUtc, FirstSeenAtUtc, LastCheckedAtUtc
            FROM dbo.RuleCandidate WHERE CandidateId = @id
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", candidateId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? ReadCandidate(reader) : null;
    }

    public async Task<RuleCandidateRow?> GetStableEligibleCandidateAsync(int nidMember, int stabilityHours, CancellationToken ct = default)
    {
        if (!IsConfigured)
            return null;
        const string sql = """
            SELECT TOP 1 CandidateId, NidMember, SourceNidHistory, SourceModifyAt, CanonicalXmlHash, XmlBody,
                   Modifyer, ModifyDesc, Status, RejectReason, StableEligibleAtUtc, FirstSeenAtUtc, LastCheckedAtUtc
            FROM dbo.RuleCandidate
            WHERE NidMember = @nid
              AND Status = N'PendingStability'
              AND StableEligibleAtUtc IS NOT NULL
              AND StableEligibleAtUtc <= SYSUTCDATETIME()
            ORDER BY StableEligibleAtUtc ASC
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? ReadCandidate(reader) : null;
    }

    public async Task<long> InsertCandidateAsync(
        int nidMember,
        long sourceNidHistory,
        DateTime? sourceModifyAt,
        string canonicalXmlHash,
        string xmlBody,
        string? modifyer,
        string? modifyDesc,
        DateTime stableEligibleAtUtc,
        CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO dbo.RuleCandidate
                (NidMember, SourceNidHistory, SourceModifyAt, CanonicalXmlHash, XmlBody,
                 Modifyer, ModifyDesc, Status, RejectReason, StableEligibleAtUtc, FirstSeenAtUtc, LastCheckedAtUtc)
            OUTPUT INSERTED.CandidateId
            VALUES
                (@nid, @hist, @modAt, @hash, @xml, @modBy, @modDesc, @status, NULL, @stable, SYSUTCDATETIME(), SYSUTCDATETIME())
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        cmd.Parameters.AddWithValue("@hist", sourceNidHistory);
        cmd.Parameters.AddWithValue("@modAt", SqlReaderValueCoercion.ToSqlDateTimeParameter(sourceModifyAt));
        cmd.Parameters.AddWithValue("@hash", canonicalXmlHash);
        cmd.Parameters.AddWithValue("@xml", xmlBody ?? "");
        cmd.Parameters.AddWithValue("@modBy", (object?)modifyer ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@modDesc", (object?)modifyDesc ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@status", RuleCandidateStatuses.PendingStability);
        cmd.Parameters.AddWithValue("@stable", stableEligibleAtUtc);
        var scalar = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt64(scalar);
    }

    public async Task TouchCandidateCheckedAsync(long candidateId, CancellationToken ct = default)
    {
        const string sql = "UPDATE dbo.RuleCandidate SET LastCheckedAtUtc = SYSUTCDATETIME() WHERE CandidateId = @id";
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", candidateId);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task UpdateCandidateStatusAsync(long candidateId, string status, string? rejectReason, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE dbo.RuleCandidate SET
                Status = @status,
                RejectReason = @reject,
                LastCheckedAtUtc = SYSUTCDATETIME()
            WHERE CandidateId = @id
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", candidateId);
        cmd.Parameters.AddWithValue("@status", status);
        cmd.Parameters.AddWithValue("@reject", (object?)rejectReason ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task InsertPromotionLogAsync(
        int nidMember,
        long? candidateId,
        long? snapshotId,
        string action,
        string? reason,
        CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO dbo.RulePromotionLog (NidMember, CandidateId, SnapshotId, Action, Reason, CreatedAtUtc)
            VALUES (@nid, @cid, @snap, @action, @reason, SYSUTCDATETIME())
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        cmd.Parameters.AddWithValue("@cid", (object?)candidateId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@snap", (object?)snapshotId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@action", action);
        cmd.Parameters.AddWithValue("@reason", (object?)reason ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task InsertDryRunResultAsync(
        long? candidateId,
        long? snapshotId,
        int? goldenFicheId,
        string engineName,
        bool success,
        string? errorMessage,
        string? outputJson,
        CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO dbo.RuleDryRunResult
                (CandidateId, SnapshotId, GoldenFicheId, EngineName, Success, ErrorMessage, OutputJson, ExecutedAtUtc)
            VALUES (@cid, @snap, @gf, @engine, @success, @err, @out, SYSUTCDATETIME())
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@cid", (object?)candidateId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@snap", (object?)snapshotId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@gf", (object?)goldenFicheId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@engine", engineName);
        cmd.Parameters.AddWithValue("@success", success);
        cmd.Parameters.AddWithValue("@err", (object?)errorMessage ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@out", (object?)outputJson ?? DBNull.Value);
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
        NidClass = reader.GetInt32(1),
        LastSeenNidHistory = reader.IsDBNull(2) ? null : reader.GetInt64(2),
        LastSeenModifyAt = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
        LastStableNidHistory = reader.IsDBNull(4) ? null : reader.GetInt64(4),
        LastStableModifyAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
        LastStableXmlHash = reader.IsDBNull(6) ? "" : reader.GetString(6),
        ActiveDslVersion = reader.IsDBNull(7) ? null : reader.GetInt32(7),
        ActiveEngine = reader.IsDBNull(8) ? "Legacy" : reader.GetString(8),
        ActiveSnapshotId = reader.IsDBNull(9) ? null : reader.GetInt64(9),
        UpdatedAtUtc = reader.GetDateTime(10),
        ConsecutiveDynamicFailures = reader.IsDBNull(11) ? 0 : reader.GetInt32(11),
        CircuitBreakerOpenUntilUtc = reader.IsDBNull(12) ? null : reader.GetDateTime(12)
    };

    private static RuleCandidateRow ReadCandidate(SqlDataReader reader) => new()
    {
        CandidateId = reader.GetInt64(0),
        NidMember = reader.GetInt32(1),
        SourceNidHistory = reader.IsDBNull(2) ? null : reader.GetInt64(2),
        SourceModifyAt = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
        CanonicalXmlHash = reader.IsDBNull(4) ? "" : reader.GetString(4),
        XmlBody = reader.IsDBNull(5) ? "" : reader.GetString(5),
        Modifyer = reader.IsDBNull(6) ? null : reader.GetString(6),
        ModifyDesc = reader.IsDBNull(7) ? null : reader.GetString(7),
        Status = reader.IsDBNull(8) ? RuleCandidateStatuses.PendingStability : reader.GetString(8),
        RejectReason = reader.IsDBNull(9) ? null : reader.GetString(9),
        StableEligibleAtUtc = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
        FirstSeenAtUtc = reader.GetDateTime(11),
        LastCheckedAtUtc = reader.IsDBNull(12) ? null : reader.GetDateTime(12)
    };
}
