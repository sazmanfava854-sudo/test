using System.Text.Json;
using System.Text.Json.Serialization;
using RayvarzResend.Web.RuleEngine.Store;

namespace RayvarzResend.Web.RuleEngine.Parser;

public sealed class RuleDslParserService
{
    public const string ParserVersion = "2.2.0";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    private readonly RuleEngineStore _store;
    private readonly MemberRuleRepository _members;
    private readonly IConfiguration _config;
    private readonly ILogger<RuleDslParserService> _logger;

    public RuleDslParserService(
        RuleEngineStore store,
        MemberRuleRepository members,
        IConfiguration config,
        ILogger<RuleDslParserService> logger)
    {
        _store = store;
        _members = members;
        _config = config;
        _logger = logger;
    }

    public int NidMember => _config.GetValue("RuleEngine:NidMemberRayvarzRun", 1388);

    public DslParseResult Parse(string xmlBody, string source = "xml")
    {
        try
        {
            var envelope = XmlEnvelopeReader.Read(xmlBody, source);
            var program = VbTranspiler.Transpile(envelope.Document);
            var success = program.HasEntryPoint;

            return new DslParseResult
            {
                Success = success,
                Envelope = envelope,
                Program = program,
                ErrorMessage = success ? null : "تابع Run در AST یافت نشد."
            };
        }
        catch (Exception ex)
        {
            return new DslParseResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<DslPersistResult> ParseAndStoreAsync(
        string xmlBody, string source = "xml", bool forceRebuild = false, CancellationToken ct = default)
    {
        var parsed = Parse(xmlBody, source);
        if (!parsed.Success || parsed.Envelope == null || parsed.Program == null)
        {
            return new DslPersistResult
            {
                Stored = false,
                Parse = parsed,
                Message = parsed.ErrorMessage ?? "Parse failed"
            };
        }

        if (!_store.IsConfigured || !await _store.IsSchemaReadyAsync(ct))
        {
            return new DslPersistResult
            {
                Stored = false,
                Parse = parsed,
                XmlHash = parsed.Envelope.XmlHash,
                Message = "RayvarzRuleEngine not configured or schema missing"
            };
        }

        var dslJson = JsonSerializer.Serialize(parsed.Program, JsonOptions);
        var existing = await _store.GetSnapshotByHashAsync(NidMember, parsed.Envelope.XmlHash, ct);
        if (existing != null && !forceRebuild)
        {
            return new DslPersistResult
            {
                Stored = false,
                SkippedExisting = true,
                SnapshotId = existing.SnapshotId,
                DslVersion = existing.DslVersion,
                XmlHash = existing.XmlHash,
                Parse = parsed,
                Message = "Snapshot already exists for this XmlHash — skipped rebuild (use force=true)"
            };
        }

        if (existing != null && forceRebuild)
        {
            await _store.UpdateDslSnapshotAsync(existing.SnapshotId, dslJson, ParserVersion, parsed.Program.EntryPoint, ct);
            _logger.LogInformation("DSL snapshot rebuilt SnapshotId={SnapshotId} ParserVersion={Version}",
                existing.SnapshotId, ParserVersion);
            return new DslPersistResult
            {
                Stored = true,
                SkippedExisting = false,
                SnapshotId = existing.SnapshotId,
                DslVersion = existing.DslVersion,
                XmlHash = existing.XmlHash,
                Parse = parsed,
                Message = "DSL snapshot rebuilt (force=true)"
            };
        }

        var dslVersion = await _store.GetNextDslVersionAsync(NidMember, ct);
        var snapshotId = await _store.InsertDslSnapshotAsync(new RuleDslSnapshotRow
        {
            NidMember = NidMember,
            DslVersion = dslVersion,
            XmlHash = parsed.Envelope.XmlHash,
            DslJson = dslJson,
            ParserVersion = ParserVersion,
            EntryPoint = parsed.Program.EntryPoint,
            IsActive = false
        }, ct);

        _logger.LogInformation(
            "DSL snapshot stored SnapshotId={SnapshotId} DslVersion={DslVersion} Hash={HashPrefix}… Functions={FnCount}",
            snapshotId, dslVersion, parsed.Envelope.XmlHash[..12], parsed.Program.Functions.Count);

        return new DslPersistResult
        {
            Stored = true,
            SnapshotId = snapshotId,
            DslVersion = dslVersion,
            XmlHash = parsed.Envelope.XmlHash,
            Parse = parsed,
            Message = "DSL snapshot stored (IsActive=0)"
        };
    }

    public async Task<DslPersistResult> ParseActiveMemberAsync(bool forceRebuild = false, CancellationToken ct = default)
    {
        var record = await _members.LoadActiveMemberAsync(NidMember, ct: ct);
        if (record == null || string.IsNullOrWhiteSpace(record.XmlBody))
        {
            return new DslPersistResult
            {
                Stored = false,
                Message = "Member XmlBody یافت نشد — ConnectionStrings:RuleEngine یا LocalXmlPath"
            };
        }

        return await ParseAndStoreAsync(record.XmlBody, record.Source, forceRebuild, ct);
    }

    public static string SerializeProgram(DslProgram program) =>
        JsonSerializer.Serialize(program, JsonOptions);

    public static DslProgram? DeserializeProgram(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        return JsonSerializer.Deserialize<DslProgram>(json, JsonOptions);
    }
}
