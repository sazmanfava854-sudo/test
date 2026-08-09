using Microsoft.Data.SqlClient;
using System.Data;

namespace RayvarzResend.Web.RuleEngine.Store;

public sealed class RuleEngineStore
{
    private readonly string? _cs;

    public RuleEngineStore(IConfiguration config)
    {
        _cs = config.GetConnectionString("RayvarzRuleEngine");
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_cs);

    /// <summary>آیا جداول فاز ۰ (مثلاً RuleSyncState) روی RayvarzRuleEngine ساخته شده‌اند؟</summary>
    public async Task<bool> IsSchemaReadyAsync(CancellationToken ct = default)
    {
        if (!IsConfigured) return false;

        const string sql = """
            SELECT 1 FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'RuleSyncState'
            """;

        try
        {
            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null;
        }
        catch
        {
            return false;
        }
    }

    public string? ConfiguredDatabaseName
    {
        get
        {
            if (!IsConfigured) return null;
            try { return new SqlConnectionStringBuilder(_cs).InitialCatalog; }
            catch { return null; }
        }
    }

    public string? ConfiguredServerName
    {
        get
        {
            if (!IsConfigured) return null;
            try { return new SqlConnectionStringBuilder(_cs).DataSource; }
            catch { return null; }
        }
    }

    public async Task<RuleEngineDiagnostics> GetDiagnosticsAsync(CancellationToken ct = default)
    {
        var diag = new RuleEngineDiagnostics
        {
            ConnectionConfigured = IsConfigured,
            ConfiguredServer = ConfiguredServerName,
            ConfiguredDatabase = ConfiguredDatabaseName
        };

        if (!IsConfigured)
        {
            diag.ConnectionOk = false;
            diag.Message = "ConnectionStrings:RayvarzRuleEngine تنظیم نشده";
            return diag;
        }

        try
        {
            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync(ct);
            diag.ConnectionOk = true;
            diag.ActualServer = conn.DataSource;
            diag.ActualDatabase = conn.Database;

            const string tablesSql = """
                SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME IN (
                    'RuleSyncState','RuleGoldenFiche','RuleGoldenExpectedRow','RuleCandidate','RuleDslSnapshot')
                ORDER BY TABLE_NAME
                """;
            await using var tablesCmd = new SqlCommand(tablesSql, conn);
            await using var reader = await tablesCmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                diag.ExistingTables.Add(reader.GetString(0));
            await reader.CloseAsync();

            diag.SchemaReady = diag.ExistingTables.Contains("RuleSyncState", StringComparer.OrdinalIgnoreCase);

            if (diag.SchemaReady)
            {
                await using var countCmd = new SqlCommand(
                    "SELECT (SELECT COUNT(*) FROM dbo.RuleGoldenFiche),(SELECT COUNT(*) FROM dbo.RuleSyncState)", conn);
                await using var countReader = await countCmd.ExecuteReaderAsync(ct);
                if (await countReader.ReadAsync(ct))
                {
                    diag.GoldenFicheCount = countReader.GetInt32(0);
                    diag.SyncStateCount = countReader.GetInt32(1);
                }
                diag.Message = "Schema OK";
            }
            else
            {
                diag.Message =
                    $"اتصال به database '{diag.ActualDatabase}' برقرار است ولی RuleSyncState وجود ندارد. " +
                    "اسکریپت database/01_RayvarzRuleEngine_Schema.sql را روی RayvarzRuleEngine اجرا کنید. " +
                    "Database در connection string باید RayvarzRuleEngine باشد نه DbRuleEngein.";
            }
        }
        catch (Exception ex)
        {
            diag.ConnectionOk = false;
            diag.Message = ex.Message;
        }

        return diag;
    }

    private async Task<bool> GuardSchemaAsync(CancellationToken ct)
    {
        if (!IsConfigured) return false;
        return await IsSchemaReadyAsync(ct);
    }

    public async Task<RuleSyncStateRow?> GetSyncStateAsync(int nidMember, CancellationToken ct = default)
    {
        if (!IsConfigured || !await GuardSchemaAsync(ct)) return null;

        const string sql = """
            SELECT NidMember, NidClass, LastSeenNidHistory, LastSeenModifyAt,
                   LastStableNidHistory, LastStableModifyAt, LastStableXmlHash,
                   ActiveDslVersion, ActiveEngine, ActiveSnapshotId, UpdatedAtUtc,
                   ISNULL(ConsecutiveDynamicFailures, 0), CircuitBreakerOpenUntilUtc
            FROM dbo.RuleSyncState WHERE NidMember = @nid
            """;

        try
        {
            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nid", nidMember);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            if (!await r.ReadAsync(ct)) return null;

            return new RuleSyncStateRow
            {
                NidMember = r.GetInt32(0),
                NidClass = r.GetInt32(1),
                LastSeenNidHistory = r.IsDBNull(2) ? null : r.GetInt64(2),
                LastSeenModifyAt = r.IsDBNull(3) ? null : r.GetDateTime(3),
                LastStableNidHistory = r.IsDBNull(4) ? null : r.GetInt64(4),
                LastStableModifyAt = r.IsDBNull(5) ? null : r.GetDateTime(5),
                LastStableXmlHash = r.IsDBNull(6) ? null : r.GetString(6).Trim(),
                ActiveDslVersion = r.GetInt32(7),
                ActiveEngine = r.GetString(8),
                ActiveSnapshotId = r.IsDBNull(9) ? null : r.GetInt64(9),
                UpdatedAtUtc = r.GetDateTime(10),
                ConsecutiveDynamicFailures = r.FieldCount > 11 && !r.IsDBNull(11) ? r.GetInt32(11) : 0,
                CircuitBreakerOpenUntilUtc = r.FieldCount > 12 && !r.IsDBNull(12) ? r.GetDateTime(12) : null
            };
        }
        catch (SqlException ex) when (ex.Number is 208 or 207)
        {
            return await GetSyncStateLegacyAsync(nidMember, ct);
        }
    }

    private async Task<RuleSyncStateRow?> GetSyncStateLegacyAsync(int nidMember, CancellationToken ct)
    {
        const string sql = """
            SELECT NidMember, NidClass, LastSeenNidHistory, LastSeenModifyAt,
                   LastStableNidHistory, LastStableModifyAt, LastStableXmlHash,
                   ActiveDslVersion, ActiveEngine, ActiveSnapshotId, UpdatedAtUtc
            FROM dbo.RuleSyncState WHERE NidMember = @nid
            """;

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        if (!await r.ReadAsync(ct)) return null;

        return new RuleSyncStateRow
        {
            NidMember = r.GetInt32(0),
            NidClass = r.GetInt32(1),
            LastSeenNidHistory = r.IsDBNull(2) ? null : r.GetInt64(2),
            LastSeenModifyAt = r.IsDBNull(3) ? null : r.GetDateTime(3),
            LastStableNidHistory = r.IsDBNull(4) ? null : r.GetInt64(4),
            LastStableModifyAt = r.IsDBNull(5) ? null : r.GetDateTime(5),
            LastStableXmlHash = r.IsDBNull(6) ? null : r.GetString(6).Trim(),
            ActiveDslVersion = r.GetInt32(7),
            ActiveEngine = r.GetString(8),
            ActiveSnapshotId = r.IsDBNull(9) ? null : r.GetInt64(9),
            UpdatedAtUtc = r.GetDateTime(10)
        };
    }

    public async Task UpsertSyncStateAsync(RuleSyncStateRow state, CancellationToken ct = default)
    {
        if (!IsConfigured || !await GuardSchemaAsync(ct)) return;

        const string sql = """
            MERGE dbo.RuleSyncState AS t
            USING (SELECT @nid AS NidMember) AS s ON t.NidMember = s.NidMember
            WHEN MATCHED THEN UPDATE SET
                NidClass = @class,
                LastSeenNidHistory = @seenHist,
                LastSeenModifyAt = @seenAt,
                LastStableNidHistory = @stableHist,
                LastStableModifyAt = @stableAt,
                LastStableXmlHash = @stableHash,
                ActiveDslVersion = @dslVer,
                ActiveEngine = @engine,
                ActiveSnapshotId = @snapId,
                ConsecutiveDynamicFailures = @dynFail,
                CircuitBreakerOpenUntilUtc = @cbUntil,
                UpdatedAtUtc = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT (
                NidMember, NidClass, LastSeenNidHistory, LastSeenModifyAt,
                LastStableNidHistory, LastStableModifyAt, LastStableXmlHash,
                ActiveDslVersion, ActiveEngine, ActiveSnapshotId,
                ConsecutiveDynamicFailures, CircuitBreakerOpenUntilUtc)
            VALUES (@nid, @class, @seenHist, @seenAt, @stableHist, @stableAt, @stableHash, @dslVer, @engine, @snapId, @dynFail, @cbUntil);
            """;

        try
        {
            await ExecuteUpsertSyncStateAsync(state, sql, ct);
        }
        catch (SqlException ex) when (ex.Number == 207)
        {
            const string legacySql = """
                MERGE dbo.RuleSyncState AS t
                USING (SELECT @nid AS NidMember) AS s ON t.NidMember = s.NidMember
                WHEN MATCHED THEN UPDATE SET
                    NidClass = @class,
                    LastSeenNidHistory = @seenHist,
                    LastSeenModifyAt = @seenAt,
                    LastStableNidHistory = @stableHist,
                    LastStableModifyAt = @stableAt,
                    LastStableXmlHash = @stableHash,
                    ActiveDslVersion = @dslVer,
                    ActiveEngine = @engine,
                    ActiveSnapshotId = @snapId,
                    UpdatedAtUtc = SYSUTCDATETIME()
                WHEN NOT MATCHED THEN INSERT (
                    NidMember, NidClass, LastSeenNidHistory, LastSeenModifyAt,
                    LastStableNidHistory, LastStableModifyAt, LastStableXmlHash,
                    ActiveDslVersion, ActiveEngine, ActiveSnapshotId)
                VALUES (@nid, @class, @seenHist, @seenAt, @stableHist, @stableAt, @stableHash, @dslVer, @engine, @snapId);
                """;
            await ExecuteUpsertSyncStateAsync(state, legacySql, ct, includeCircuitBreaker: false);
        }
    }

    private async Task ExecuteUpsertSyncStateAsync(
        RuleSyncStateRow state, string sql, CancellationToken ct, bool includeCircuitBreaker = true)
    {
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", state.NidMember);
        cmd.Parameters.AddWithValue("@class", state.NidClass);
        cmd.Parameters.AddWithValue("@seenHist", (object?)state.LastSeenNidHistory ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@seenAt", (object?)state.LastSeenModifyAt ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@stableHist", (object?)state.LastStableNidHistory ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@stableAt", (object?)state.LastStableModifyAt ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@stableHash", (object?)state.LastStableXmlHash ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@dslVer", state.ActiveDslVersion);
        cmd.Parameters.AddWithValue("@engine", state.ActiveEngine);
        cmd.Parameters.AddWithValue("@snapId", (object?)state.ActiveSnapshotId ?? DBNull.Value);
        if (includeCircuitBreaker)
        {
            cmd.Parameters.AddWithValue("@dynFail", state.ConsecutiveDynamicFailures);
            cmd.Parameters.AddWithValue("@cbUntil", (object?)state.CircuitBreakerOpenUntilUtc ?? DBNull.Value);
        }
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<IReadOnlyList<RuleGoldenFicheRow>> GetActiveGoldenFichesAsync(int nidMember, CancellationToken ct = default)
    {
        if (!IsConfigured || !await GuardSchemaAsync(ct)) return Array.Empty<RuleGoldenFicheRow>();

        const string sql = """
            SELECT GoldenFicheId, Name, FicheNo, NidFiche, NidMember, Scenario, ExpectedRowCount, IsActive, Notes
            FROM dbo.RuleGoldenFiche
            WHERE NidMember = @nid AND IsActive = 1
            ORDER BY GoldenFicheId
            """;

        var list = new List<RuleGoldenFicheRow>();
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            list.Add(new RuleGoldenFicheRow
            {
                GoldenFicheId = r.GetInt32(0),
                Name = r.GetString(1),
                FicheNo = r.GetString(2),
                NidFiche = r.GetGuid(3),
                NidMember = r.GetInt32(4),
                Scenario = r.GetString(5),
                ExpectedRowCount = r.GetInt32(6),
                IsActive = r.GetBoolean(7),
                Notes = r.IsDBNull(8) ? null : r.GetString(8)
            });
        }

        return list;
    }

    public async Task<IReadOnlyList<RuleGoldenExpectedRow>> GetExpectedRowsAsync(int goldenFicheId, CancellationToken ct = default)
    {
        if (!IsConfigured) return Array.Empty<RuleGoldenExpectedRow>();

        const string sql = """
            SELECT GoldenFicheId, IncmRow, IncmNo, ExpectedVal, IncmRowDsc, ExpectedBranch, ExpectedBank,
                   ExpectedCenter, ExpectedCenter1, ExpectedCenter2, ExpectedCenter3
            FROM dbo.RuleGoldenExpectedRow
            WHERE GoldenFicheId = @id
            ORDER BY IncmRow
            """;

        var list = new List<RuleGoldenExpectedRow>();
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", goldenFicheId);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            list.Add(new RuleGoldenExpectedRow
            {
                GoldenFicheId = r.GetInt32(0),
                IncmRow = r.GetInt32(1),
                IncmNo = r.GetInt32(2),
                ExpectedVal = r.GetDecimal(3),
                IncmRowDsc = r.IsDBNull(4) ? null : r.GetString(4),
                ExpectedBranch = r.IsDBNull(5) ? null : r.GetInt32(5),
                ExpectedBank = r.IsDBNull(6) ? null : r.GetInt32(6),
                ExpectedCenter = r.IsDBNull(7) ? null : Convert.ToInt64(r.GetValue(7)),
                ExpectedCenter1 = r.IsDBNull(8) ? null : Convert.ToInt64(r.GetValue(8)),
                ExpectedCenter2 = r.IsDBNull(9) ? null : Convert.ToInt64(r.GetValue(9)),
                ExpectedCenter3 = r.IsDBNull(10) ? null : Convert.ToInt64(r.GetValue(10))
            });
        }

        return list;
    }

    public async Task<long> InsertCandidateAsync(RuleCandidateRow row, CancellationToken ct = default)
    {
        if (!IsConfigured) return 0;

        const string sql = """
            INSERT INTO dbo.RuleCandidate (
                NidMember, SourceNidHistory, SourceModifyAt, CanonicalXmlHash, XmlBody,
                Modifyer, ModifyDesc, Status, StableEligibleAtUtc)
            OUTPUT INSERTED.CandidateId
            VALUES (@member, @hist, @modAt, @hash, @xml, @modBy, @modDesc, @status, @stableAt)
            """;

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@member", row.NidMember);
        cmd.Parameters.AddWithValue("@hist", row.SourceNidHistory);
        cmd.Parameters.AddWithValue("@modAt", row.SourceModifyAt);
        cmd.Parameters.AddWithValue("@hash", row.CanonicalXmlHash);
        AddNVarCharMax(cmd, "@xml", row.XmlBody);
        cmd.Parameters.AddWithValue("@modBy", (object?)row.Modifyer ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@modDesc", (object?)row.ModifyDesc ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@status", row.Status);
        cmd.Parameters.AddWithValue("@stableAt", row.StableEligibleAtUtc);
        var id = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt64(id);
    }

    public async Task<bool> CandidateExistsAsync(int nidMember, string hash, CancellationToken ct = default)
    {
        if (!IsConfigured) return false;

        const string sql = "SELECT 1 FROM dbo.RuleCandidate WHERE NidMember = @nid AND CanonicalXmlHash = @hash";
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        cmd.Parameters.AddWithValue("@hash", hash);
        var result = await cmd.ExecuteScalarAsync(ct);
        return result != null;
    }

    public async Task InsertDryRunResultAsync(
        long? candidateId, long? snapshotId, int goldenFicheId, string engine,
        bool success, string? error, string? outputJson, CancellationToken ct = default)
    {
        if (!IsConfigured) return;

        const string sql = """
            INSERT INTO dbo.RuleDryRunResult (CandidateId, SnapshotId, GoldenFicheId, EngineName, Success, ErrorMessage, OutputJson)
            VALUES (@cand, @snap, @golden, @engine, @ok, @err, @json)
            """;

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@cand", (object?)candidateId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@snap", (object?)snapshotId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@golden", goldenFicheId);
        cmd.Parameters.AddWithValue("@engine", engine);
        cmd.Parameters.AddWithValue("@ok", success);
        cmd.Parameters.AddWithValue("@err", (object?)error ?? DBNull.Value);
        AddNVarCharMax(cmd, "@json", outputJson);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task InsertPromotionLogAsync(int nidMember, long? candidateId, long? snapshotId, string action, string? reason, CancellationToken ct = default)
    {
        if (!IsConfigured) return;

        const string sql = """
            INSERT INTO dbo.RulePromotionLog (NidMember, CandidateId, SnapshotId, Action, Reason)
            VALUES (@nid, @cand, @snap, @action, @reason)
            """;

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        cmd.Parameters.AddWithValue("@cand", (object?)candidateId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@snap", (object?)snapshotId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@action", action);
        cmd.Parameters.AddWithValue("@reason", (object?)reason ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<RuleDslSnapshotRow?> GetSnapshotByHashAsync(int nidMember, string xmlHash, CancellationToken ct = default)
    {
        if (!IsConfigured || !await GuardSchemaAsync(ct)) return null;

        const string sql = """
            SELECT TOP 1 SnapshotId, NidMember, DslVersion, XmlHash, DslJson, ParserVersion, EntryPoint, CreatedAtUtc, IsActive
            FROM dbo.RuleDslSnapshot
            WHERE NidMember = @nid AND XmlHash = @hash
            ORDER BY DslVersion DESC
            """;

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        cmd.Parameters.AddWithValue("@hash", xmlHash);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        if (!await r.ReadAsync(ct)) return null;

        return ReadDslSnapshotRow(r);
    }

    public async Task<RuleDslSnapshotRow?> GetLatestSnapshotAsync(int nidMember, CancellationToken ct = default)
    {
        if (!IsConfigured || !await GuardSchemaAsync(ct)) return null;

        const string sql = """
            SELECT TOP 1 SnapshotId, NidMember, DslVersion, XmlHash, DslJson, ParserVersion, EntryPoint, CreatedAtUtc, IsActive
            FROM dbo.RuleDslSnapshot
            WHERE NidMember = @nid
            ORDER BY DslVersion DESC, SnapshotId DESC
            """;

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        if (!await r.ReadAsync(ct)) return null;

        return ReadDslSnapshotRow(r);
    }

    public async Task<int> GetNextDslVersionAsync(int nidMember, CancellationToken ct = default)
    {
        if (!IsConfigured || !await GuardSchemaAsync(ct)) return 1;

        const string sql = "SELECT ISNULL(MAX(DslVersion), 0) + 1 FROM dbo.RuleDslSnapshot WHERE NidMember = @nid";
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result);
    }

    public async Task<long> InsertDslSnapshotAsync(RuleDslSnapshotRow row, CancellationToken ct = default)
    {
        if (!IsConfigured) return 0;

        const string sql = """
            INSERT INTO dbo.RuleDslSnapshot (NidMember, DslVersion, XmlHash, DslJson, ParserVersion, EntryPoint, IsActive)
            OUTPUT INSERTED.SnapshotId
            VALUES (@member, @ver, @hash, @json, @parser, @entry, @active)
            """;

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@member", row.NidMember);
        cmd.Parameters.AddWithValue("@ver", row.DslVersion);
        cmd.Parameters.AddWithValue("@hash", row.XmlHash);
        AddNVarCharMax(cmd, "@json", row.DslJson);
        cmd.Parameters.AddWithValue("@parser", row.ParserVersion);
        cmd.Parameters.AddWithValue("@entry", row.EntryPoint);
        cmd.Parameters.AddWithValue("@active", row.IsActive);
        var id = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt64(id);
    }

    public async Task UpdateDslSnapshotAsync(
        long snapshotId, string dslJson, string parserVersion, string entryPoint, CancellationToken ct = default)
    {
        if (!IsConfigured) return;

        const string sql = """
            UPDATE dbo.RuleDslSnapshot
            SET DslJson = @json, ParserVersion = @parser, EntryPoint = @entry
            WHERE SnapshotId = @id
            """;

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", snapshotId);
        AddNVarCharMax(cmd, "@json", dslJson);
        cmd.Parameters.AddWithValue("@parser", parserVersion);
        cmd.Parameters.AddWithValue("@entry", entryPoint ?? "Run");
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task UpdateCandidateStatusAsync(long candidateId, string status, string? rejectReason = null, CancellationToken ct = default)
    {
        if (!IsConfigured) return;

        const string sql = """
            UPDATE dbo.RuleCandidate
            SET Status = @status, RejectReason = @reason, LastCheckedAtUtc = SYSUTCDATETIME()
            WHERE CandidateId = @id
            """;

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", candidateId);
        cmd.Parameters.AddWithValue("@status", status);
        cmd.Parameters.AddWithValue("@reason", (object?)rejectReason ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<IReadOnlyList<RuleCandidateRow>> GetCandidatesByStatusesAsync(
        int nidMember, IReadOnlyList<string> statuses, CancellationToken ct = default)
    {
        if (!IsConfigured || statuses.Count == 0) return Array.Empty<RuleCandidateRow>();

        var placeholders = string.Join(",", statuses.Select((_, i) => $"@s{i}"));
        var sql = $"""
            SELECT CandidateId, NidMember, SourceNidHistory, SourceModifyAt, CanonicalXmlHash,
                   XmlBody, Modifyer, ModifyDesc, Status, RejectReason, StableEligibleAtUtc
            FROM dbo.RuleCandidate
            WHERE NidMember = @nid AND Status IN ({placeholders})
            ORDER BY SourceModifyAt DESC
            """;

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        for (var i = 0; i < statuses.Count; i++)
            cmd.Parameters.AddWithValue($"@s{i}", statuses[i]);

        var list = new List<RuleCandidateRow>();
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
            list.Add(ReadCandidateRow(r));
        return list;
    }

    public async Task<RuleDslSnapshotRow?> GetActiveSnapshotAsync(int nidMember, CancellationToken ct = default)
    {
        if (!IsConfigured || !await GuardSchemaAsync(ct)) return null;

        const string sql = """
            SELECT TOP 1 SnapshotId, NidMember, DslVersion, XmlHash, DslJson, ParserVersion, EntryPoint, CreatedAtUtc, IsActive
            FROM dbo.RuleDslSnapshot
            WHERE NidMember = @nid AND IsActive = 1
            ORDER BY DslVersion DESC, SnapshotId DESC
            """;

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        if (!await r.ReadAsync(ct)) return null;
        return ReadDslSnapshotRow(r);
    }

    public async Task ActivateSnapshotAsync(int nidMember, long snapshotId, CancellationToken ct = default)
    {
        if (!IsConfigured) return;

        const string sql = """
            UPDATE dbo.RuleDslSnapshot SET IsActive = 0 WHERE NidMember = @nid;
            UPDATE dbo.RuleDslSnapshot SET IsActive = 1 WHERE SnapshotId = @snap AND NidMember = @nid;
            """;

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        cmd.Parameters.AddWithValue("@snap", snapshotId);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task SupersedeCandidatesAsync(int nidMember, long exceptCandidateId, CancellationToken ct = default)
    {
        if (!IsConfigured) return;

        const string sql = """
            UPDATE dbo.RuleCandidate
            SET Status = @superseded, RejectReason = 'Superseded by promotion'
            WHERE NidMember = @nid AND CandidateId <> @keep
              AND Status IN (@parsed, @validated, @dryRun, @stable, @detected)
            """;

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        cmd.Parameters.AddWithValue("@keep", exceptCandidateId);
        cmd.Parameters.AddWithValue("@superseded", RuleCandidateStatus.Superseded);
        cmd.Parameters.AddWithValue("@parsed", RuleCandidateStatus.Parsed);
        cmd.Parameters.AddWithValue("@validated", RuleCandidateStatus.Validated);
        cmd.Parameters.AddWithValue("@dryRun", RuleCandidateStatus.DryRunPassed);
        cmd.Parameters.AddWithValue("@stable", RuleCandidateStatus.Stable);
        cmd.Parameters.AddWithValue("@detected", RuleCandidateStatus.Detected);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<IReadOnlyList<RulePromotionLogRow>> GetRecentPromotionLogsAsync(int nidMember, int limit, CancellationToken ct = default)
    {
        if (!IsConfigured) return Array.Empty<RulePromotionLogRow>();

        const string sql = """
            SELECT TOP (@lim) LogId, NidMember, CandidateId, SnapshotId, Action, Reason, CreatedAtUtc
            FROM dbo.RulePromotionLog
            WHERE NidMember = @nid
            ORDER BY LogId DESC
            """;

        var list = new List<RulePromotionLogRow>();
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        cmd.Parameters.AddWithValue("@lim", limit);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            list.Add(new RulePromotionLogRow
            {
                LogId = r.GetInt64(0),
                NidMember = r.GetInt32(1),
                CandidateId = r.IsDBNull(2) ? null : r.GetInt64(2),
                SnapshotId = r.IsDBNull(3) ? null : r.GetInt64(3),
                Action = r.GetString(4),
                Reason = r.IsDBNull(5) ? null : r.GetString(5),
                CreatedAtUtc = r.GetDateTime(6)
            });
        }
        return list;
    }

    private static RuleCandidateRow ReadCandidateRow(SqlDataReader r) =>
        new()
        {
            CandidateId = r.GetInt64(0),
            NidMember = r.GetInt32(1),
            SourceNidHistory = r.GetInt64(2),
            SourceModifyAt = r.GetDateTime(3),
            CanonicalXmlHash = r.GetString(4).Trim(),
            XmlBody = r.GetString(5),
            Modifyer = r.IsDBNull(6) ? null : r.GetString(6),
            ModifyDesc = r.IsDBNull(7) ? null : r.GetString(7),
            Status = r.GetString(8),
            RejectReason = r.IsDBNull(9) ? null : r.GetString(9),
            StableEligibleAtUtc = r.GetDateTime(10)
        };

    public async Task DeactivateAllSnapshotsAsync(int nidMember, CancellationToken ct = default)
    {
        if (!IsConfigured) return;
        const string sql = "UPDATE dbo.RuleDslSnapshot SET IsActive = 0 WHERE NidMember = @nid";
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static void AddNVarCharMax(SqlCommand cmd, string name, string? value)
    {
        var p = cmd.Parameters.Add(name, SqlDbType.NVarChar, -1);
        p.Value = string.IsNullOrEmpty(value) ? DBNull.Value : value;
    }

    private static RuleDslSnapshotRow ReadDslSnapshotRow(SqlDataReader r) =>
        new()
        {
            SnapshotId = r.GetInt64(0),
            NidMember = r.GetInt32(1),
            DslVersion = r.GetInt32(2),
            XmlHash = r.GetString(3),
            DslJson = r.IsDBNull(4) ? null : r.GetString(4),
            ParserVersion = r.GetString(5),
            EntryPoint = r.GetString(6),
            CreatedAtUtc = r.GetDateTime(7),
            IsActive = r.GetBoolean(8)
        };
}

public sealed class RuleEngineDiagnostics
{
    public bool ConnectionConfigured { get; init; }
    public bool ConnectionOk { get; set; }
    public bool SchemaReady { get; set; }
    public string? ConfiguredServer { get; init; }
    public string? ConfiguredDatabase { get; init; }
    public string? ActualServer { get; set; }
    public string? ActualDatabase { get; set; }
    public List<string> ExistingTables { get; } = new();
    public int GoldenFicheCount { get; set; }
    public int SyncStateCount { get; set; }
    public string? Message { get; set; }
}
