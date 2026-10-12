using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace RayvarzResend.Web.RuleEngine.Sync;

/// <summary>
/// جداول sync در RayvarzRuleEngine (اسکیمای database/01 + 03).
/// RuleCandidate: CandidateId IDENTITY، UNIQUE (NidMember, CanonicalXmlHash)، Source*/StableEligibleAtUtc NOT NULL.
/// </summary>
public sealed class RuleEngineStateRepository : IRuleSendGateStore
{
    private const string CandidateColumns = """
        CandidateId, NidMember, SourceNidHistory, SourceModifyAt, CanonicalXmlHash, XmlBody,
        Modifyer, ModifyDesc, Status, RejectReason, StableEligibleAtUtc, FirstSeenAtUtc, LastCheckedAtUtc
        """;

    private const string PendingStatusList = "('Detected', 'Parsed', 'Validated')";

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

        const string probe = """
            SELECT TOP 0 NidMember, LastStableXmlHash, LastSeenNidHistory, ConsecutiveDynamicFailures, CircuitBreakerOpenUntilUtc
            FROM dbo.RuleSyncState;
            SELECT TOP 0 CandidateId, CanonicalXmlHash, Status, StableEligibleAtUtc FROM dbo.RuleCandidate;
            SELECT TOP 0 LogId, Action, Reason FROM dbo.RulePromotionLog;
            """;
        try
        {
            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(probe, conn);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex,
                "Rule sync tables in RayvarzRuleEngine do not match database/01_RayvarzRuleEngine_Schema.sql + 03_Phase4_CircuitBreaker.sql");
            throw;
        }

        try
        {
            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(SendGateSchemaSql, conn);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex,
                "RuleSyncState.SendGate columns missing and could not be added — run database/12_RuleEngine_SendGate.sql");
            throw;
        }

        _schemaVerified = true;
    }

    /// <summary>هم‌ارز database/12_RuleEngine_SendGate.sql (idempotent).</summary>
    internal const string SendGateSchemaSql = """
        IF COL_LENGTH(N'dbo.RuleSyncState', N'ApprovedSendXmlHash') IS NULL
            ALTER TABLE dbo.RuleSyncState ADD ApprovedSendXmlHash CHAR(64) NULL;
        IF COL_LENGTH(N'dbo.RuleSyncState', N'SendGateChangedAtUtc') IS NULL
            ALTER TABLE dbo.RuleSyncState ADD SendGateChangedAtUtc DATETIME2(3) NULL;
        IF COL_LENGTH(N'dbo.RuleSyncState', N'SendGateChangedBy') IS NULL
            ALTER TABLE dbo.RuleSyncState ADD SendGateChangedBy NVARCHAR(200) NULL;
        IF COL_LENGTH(N'dbo.RuleSyncState', N'SendGate') IS NULL
        BEGIN
            ALTER TABLE dbo.RuleSyncState ADD SendGate VARCHAR(20) NULL;
            EXEC (N'
                UPDATE s SET
                    ApprovedSendXmlHash = s.LastStableXmlHash,
                    SendGate = CASE WHEN EXISTS (
                        SELECT 1 FROM dbo.RuleCandidate c
                        WHERE c.NidMember = s.NidMember AND c.Status IN (''Detected'', ''Parsed'', ''Validated''))
                        THEN ''Blocked'' ELSE ''Open'' END,
                    SendGateChangedAtUtc = SYSUTCDATETIME(),
                    SendGateChangedBy = N''schema-12''
                FROM dbo.RuleSyncState s;');
        END
        """;

    public async Task<string?> GetSendGateAsync(int nidMember, CancellationToken ct = default)
    {
        if (!IsConfigured)
            return null;
        await EnsureSchemaAsync(ct);
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand("SELECT SendGate FROM dbo.RuleSyncState WHERE NidMember = @nid", conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        var value = await cmd.ExecuteScalarAsync(ct);
        return value is null or DBNull ? null : Convert.ToString(value)?.Trim();
    }

    public async Task UpdateSendGateAsync(
        int nidMember,
        string gate,
        string? approvedHash,
        string changedBy,
        CancellationToken ct = default)
    {
        const string sql = """
            UPDATE dbo.RuleSyncState SET
                SendGate = @gate,
                ApprovedSendXmlHash = @approved,
                SendGateChangedAtUtc = SYSUTCDATETIME(),
                SendGateChangedBy = @by,
                UpdatedAtUtc = SYSUTCDATETIME()
            WHERE NidMember = @nid
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        cmd.Parameters.AddWithValue("@gate", gate);
        cmd.Parameters.AddWithValue("@approved",
            string.IsNullOrWhiteSpace(approvedHash) ? DBNull.Value : approvedHash.Trim());
        cmd.Parameters.AddWithValue("@by", Truncate(changedBy, 200)!);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<RuleSyncStateRow> GetOrCreateStateAsync(int nidMember, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        var existing = await GetStateAsync(nidMember, ct);
        if (existing != null)
            return existing;

        const string insert = """
            IF NOT EXISTS (SELECT 1 FROM dbo.RuleSyncState WHERE NidMember = @nid)
                INSERT INTO dbo.RuleSyncState (NidMember, NidClass, ActiveEngine, ActiveDslVersion)
                VALUES (@nid, @class, 'Legacy', 0);
            """;
        await using (var conn = new SqlConnection(_cs))
        {
            await conn.OpenAsync(ct);
            await using var ins = new SqlCommand(insert, conn);
            ins.Parameters.AddWithValue("@nid", nidMember);
            ins.Parameters.AddWithValue("@class", _options.NidClass);
            await ins.ExecuteNonQueryAsync(ct);
        }

        return await GetStateAsync(nidMember, ct)
            ?? throw new InvalidOperationException($"RuleSyncState row for NidMember={nidMember} could not be created");
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
                   ConsecutiveDynamicFailures, CircuitBreakerOpenUntilUtc,
                   SendGate, ApprovedSendXmlHash, SendGateChangedAtUtc, SendGateChangedBy
            FROM dbo.RuleSyncState WHERE NidMember = @nid
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? ReadState(reader) : null;
    }

    public async Task UpdateStateAsync(RuleSyncStateRow row, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE dbo.RuleSyncState SET
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
        cmd.Parameters.AddWithValue("@seenHist", (object?)row.LastSeenNidHistory ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@seenMod", SqlReaderValueCoercion.ToSqlDateTimeParameter(row.LastSeenModifyAt));
        cmd.Parameters.AddWithValue("@stableHist", (object?)row.LastStableNidHistory ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@stableMod", SqlReaderValueCoercion.ToSqlDateTimeParameter(row.LastStableModifyAt));
        cmd.Parameters.AddWithValue("@stableHash",
            string.IsNullOrWhiteSpace(row.LastStableXmlHash) ? DBNull.Value : row.LastStableXmlHash.Trim());
        cmd.Parameters.AddWithValue("@dslVer", row.ActiveDslVersion);
        cmd.Parameters.AddWithValue("@engine", string.IsNullOrWhiteSpace(row.ActiveEngine) ? "Legacy" : row.ActiveEngine);
        cmd.Parameters.AddWithValue("@snapshot", (object?)row.ActiveSnapshotId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@failures", row.ConsecutiveDynamicFailures);
        cmd.Parameters.AddWithValue("@breaker", SqlReaderValueCoercion.ToSqlDateTimeParameter(row.CircuitBreakerOpenUntilUtc));
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<RuleCandidateRow?> GetOpenCandidateAsync(int nidMember, CancellationToken ct = default)
    {
        if (!IsConfigured)
            return null;
        var sql = $"""
            SELECT TOP 1 {CandidateColumns}
            FROM dbo.RuleCandidate
            WHERE NidMember = @nid AND Status IN {PendingStatusList}
            ORDER BY FirstSeenAtUtc DESC, CandidateId DESC
            """;
        return await QuerySingleCandidateAsync(sql, cmd => cmd.Parameters.AddWithValue("@nid", nidMember), ct);
    }

    public Task<RuleCandidateRow?> GetCandidateByIdAsync(long candidateId, CancellationToken ct = default) =>
        QuerySingleCandidateAsync(
            $"SELECT {CandidateColumns} FROM dbo.RuleCandidate WHERE CandidateId = @id",
            cmd => cmd.Parameters.AddWithValue("@id", candidateId),
            ct);

    public Task<RuleCandidateRow?> GetCandidateByHashAsync(int nidMember, string hash, CancellationToken ct = default) =>
        QuerySingleCandidateAsync(
            $"SELECT {CandidateColumns} FROM dbo.RuleCandidate WHERE NidMember = @nid AND CanonicalXmlHash = @hash",
            cmd =>
            {
                cmd.Parameters.AddWithValue("@nid", nidMember);
                cmd.Parameters.AddWithValue("@hash", hash);
            },
            ct);

    public Task<RuleCandidateRow?> GetStableEligibleCandidateAsync(int nidMember, CancellationToken ct = default) =>
        QuerySingleCandidateAsync(
            $"""
            SELECT TOP 1 {CandidateColumns}
            FROM dbo.RuleCandidate
            WHERE NidMember = @nid
              AND Status IN {PendingStatusList}
              AND StableEligibleAtUtc <= SYSUTCDATETIME()
            ORDER BY FirstSeenAtUtc DESC, CandidateId DESC
            """,
            cmd => cmd.Parameters.AddWithValue("@nid", nidMember),
            ct);

    public async Task<long> InsertCandidateAsync(RuleCandidateRow row, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO dbo.RuleCandidate
                (NidMember, SourceNidHistory, SourceModifyAt, CanonicalXmlHash, XmlBody,
                 Modifyer, ModifyDesc, Status, StableEligibleAtUtc)
            OUTPUT INSERTED.CandidateId
            VALUES (@nid, @hist, @modAt, @hash, @xml, @modBy, @modDesc, @status, @stable)
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        AddCandidateContentParameters(cmd, row);
        cmd.Parameters.AddWithValue("@nid", row.NidMember);
        cmd.Parameters.AddWithValue("@hash", row.CanonicalXmlHash);
        var scalar = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt64(scalar);
    }

    /// <summary>هش قبلاً دیده شده (UNIQUE) — همان ردیف با ساعت ثبات جدید دوباره باز می‌شود.</summary>
    public async Task ReopenCandidateAsync(long candidateId, RuleCandidateRow row, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE dbo.RuleCandidate SET
                SourceNidHistory = @hist,
                SourceModifyAt = @modAt,
                XmlBody = @xml,
                Modifyer = @modBy,
                ModifyDesc = @modDesc,
                Status = @status,
                RejectReason = NULL,
                StableEligibleAtUtc = @stable,
                FirstSeenAtUtc = SYSUTCDATETIME(),
                LastCheckedAtUtc = SYSUTCDATETIME()
            WHERE CandidateId = @id
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        AddCandidateContentParameters(cmd, row);
        cmd.Parameters.AddWithValue("@id", candidateId);
        await cmd.ExecuteNonQueryAsync(ct);
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
        cmd.Parameters.AddWithValue("@reject", (object?)Truncate(rejectReason, 500) ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    /// <summary>candidateهای در انتظار قدیمی‌تر را Rejected می‌کند تا نسخه کهنه promote نشود.</summary>
    public async Task<IReadOnlyList<long>> SupersedePendingCandidatesAsync(
        int nidMember,
        long? exceptCandidateId,
        string reason,
        CancellationToken ct = default)
    {
        var sql = $"""
            UPDATE dbo.RuleCandidate SET
                Status = 'Rejected',
                RejectReason = @reason,
                LastCheckedAtUtc = SYSUTCDATETIME()
            OUTPUT INSERTED.CandidateId
            WHERE NidMember = @nid
              AND Status IN {PendingStatusList}
              AND (@except IS NULL OR CandidateId <> @except)
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        cmd.Parameters.AddWithValue("@except", (object?)exceptCandidateId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@reason", Truncate(reason, 500)!);
        var ids = new List<long>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            ids.Add(Convert.ToInt64(reader.GetValue(0)));
        return ids;
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
            INSERT INTO dbo.RulePromotionLog (NidMember, CandidateId, SnapshotId, Action, Reason)
            VALUES (@nid, @cid, @snap, @action, @reason)
            """;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        cmd.Parameters.AddWithValue("@cid", (object?)candidateId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@snap", (object?)snapshotId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@action", action);
        cmd.Parameters.AddWithValue("@reason", (object?)Truncate(reason, 500) ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private async Task<RuleCandidateRow?> QuerySingleCandidateAsync(
        string sql,
        Action<SqlCommand> bind,
        CancellationToken ct)
    {
        if (!IsConfigured)
            return null;
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        bind(cmd);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? ReadCandidate(reader) : null;
    }

    private static void AddCandidateContentParameters(SqlCommand cmd, RuleCandidateRow row)
    {
        cmd.Parameters.AddWithValue("@hist", row.SourceNidHistory);
        cmd.Parameters.AddWithValue("@modAt", NonNullSqlDate(row.SourceModifyAt));
        cmd.Parameters.AddWithValue("@xml", row.XmlBody ?? "");
        cmd.Parameters.AddWithValue("@modBy", (object?)Truncate(row.Modifyer, 200) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@modDesc", (object?)Truncate(row.ModifyDesc, 500) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@status", row.Status);
        cmd.Parameters.AddWithValue("@stable", NonNullSqlDate(row.StableEligibleAtUtc));
    }

    private static DateTime NonNullSqlDate(DateTime value) =>
        value.Year < 1753 ? DateTime.UtcNow : value;

    private static string? Truncate(string? value, int max) =>
        value is null || value.Length <= max ? value : value[..max];

    private static long? ReadNullableInt64(SqlDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? null : Convert.ToInt64(reader.GetValue(ordinal));

    private static int ReadInt32OrDefault(SqlDataReader reader, int ordinal, int fallback = 0) =>
        reader.IsDBNull(ordinal) ? fallback : Convert.ToInt32(reader.GetValue(ordinal));

    private static DateTime? ReadNullableDateTime(SqlDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? null : SqlReaderValueCoercion.ToNullableDateTime(reader.GetValue(ordinal));

    private static string ReadTrimmedString(SqlDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? "" : Convert.ToString(reader.GetValue(ordinal))?.Trim() ?? "";

    private static string? ReadNullableString(SqlDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? null : Convert.ToString(reader.GetValue(ordinal));

    private static RuleSyncStateRow ReadState(SqlDataReader reader) => new()
    {
        NidMember = Convert.ToInt32(reader.GetValue(0)),
        NidClass = ReadInt32OrDefault(reader, 1, 360),
        LastSeenNidHistory = ReadNullableInt64(reader, 2),
        LastSeenModifyAt = ReadNullableDateTime(reader, 3),
        LastStableNidHistory = ReadNullableInt64(reader, 4),
        LastStableModifyAt = ReadNullableDateTime(reader, 5),
        LastStableXmlHash = ReadTrimmedString(reader, 6),
        ActiveDslVersion = ReadInt32OrDefault(reader, 7),
        ActiveEngine = ReadTrimmedString(reader, 8) is { Length: > 0 } engine ? engine : "Legacy",
        ActiveSnapshotId = ReadNullableInt64(reader, 9),
        UpdatedAtUtc = ReadNullableDateTime(reader, 10) ?? DateTime.UtcNow,
        ConsecutiveDynamicFailures = ReadInt32OrDefault(reader, 11),
        CircuitBreakerOpenUntilUtc = ReadNullableDateTime(reader, 12),
        SendGate = RuleSendGateStates.IsBlocked(ReadTrimmedString(reader, 13))
            ? RuleSendGateStates.Blocked
            : RuleSendGateStates.Open,
        ApprovedSendXmlHash = ReadTrimmedString(reader, 14),
        SendGateChangedAtUtc = ReadNullableDateTime(reader, 15),
        SendGateChangedBy = ReadNullableString(reader, 16)
    };

    private static RuleCandidateRow ReadCandidate(SqlDataReader reader) => new()
    {
        CandidateId = Convert.ToInt64(reader.GetValue(0)),
        NidMember = Convert.ToInt32(reader.GetValue(1)),
        SourceNidHistory = ReadNullableInt64(reader, 2) ?? 0,
        SourceModifyAt = ReadNullableDateTime(reader, 3) ?? DateTime.UtcNow,
        CanonicalXmlHash = ReadTrimmedString(reader, 4),
        XmlBody = ReadNullableString(reader, 5) ?? "",
        Modifyer = ReadNullableString(reader, 6),
        ModifyDesc = ReadNullableString(reader, 7),
        Status = ReadTrimmedString(reader, 8),
        RejectReason = ReadNullableString(reader, 9),
        StableEligibleAtUtc = ReadNullableDateTime(reader, 10) ?? DateTime.UtcNow,
        FirstSeenAtUtc = ReadNullableDateTime(reader, 11) ?? DateTime.UtcNow,
        LastCheckedAtUtc = ReadNullableDateTime(reader, 12) ?? DateTime.UtcNow
    };
}
