using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Hosting;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine;
using RayvarzResend.Web.RuleEngine.Engines;
using RayvarzResend.Web.RuleEngine.Executor;
using RayvarzResend.Web.RuleEngine.Parser;
using RayvarzResend.Web.RuleEngine.Promotion;
using RayvarzResend.Web.RuleEngine.Store;
using RayvarzResend.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.AddHttpClient();
builder.Services.AddSingleton<FicheRepository>();
builder.Services.AddSingleton<FicheSendService>();
builder.Services.AddSingleton<UnsentFicheService>();
builder.Services.AddSingleton<TahatorSnapshotStore>();
builder.Services.AddSingleton<TahatorResendService>();
builder.Services.AddSingleton<SoapBuilder>();
builder.Services.AddSingleton<RayvarzClient>();
builder.Services.AddSingleton<MemberRuleRepository>();
builder.Services.AddSingleton<LegacyRuleEngine>();
builder.Services.AddSingleton<DynamicRuleEngine>();
builder.Services.AddSingleton<RuleEngineFactory>();
builder.Services.AddSingleton<RayvarzPayloadBuilder>();
builder.Services.AddSingleton<RuleEngineStore>();
builder.Services.AddSingleton<RuleHistoryChecker>();
builder.Services.AddSingleton<IOperationRegistry>(_ => SaraOperationBootstrap.CreateDefault());
builder.Services.AddSingleton<DslValidator>();
builder.Services.AddSingleton<DslExecutor>();
builder.Services.AddSingleton<RuleDslParserService>();
builder.Services.AddSingleton<RuleVersionManager>();
builder.Services.AddSingleton<GoldenDryRunService>();
builder.Services.AddSingleton<RuleCircuitBreakerService>();
builder.Services.AddSingleton<RulePromotionService>();
builder.Services.AddHostedService<RuleSyncBackgroundService>();

var app = builder.Build();

app.UseExceptionHandler(handler =>
{
    handler.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json; charset=utf-8";
        var ex = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        await context.Response.WriteAsJsonAsync(new { error = ex?.Message ?? "خطای داخلی سرور" });
    });
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/config", (IConfiguration config) => new
{
    dryRun = config.GetValue<bool>("Rayvarz:DryRun"),
    serviceUrl = RayvarzUrlNormalizer.Normalize(config, config["Rayvarz:ServiceUrl"]),
    serviceUrlMsb = RayvarzUrlNormalizer.Normalize(config, config["Rayvarz:ServiceUrlMsb"] ?? ""),
    wsAddressingTo = RayvarzUrlNormalizer.Normalize(config, config["Rayvarz:WsAddressingTo"] ?? config["Rayvarz:ServiceUrl"]),
    useHttp = config.GetValue("Rayvarz:UseHttp", true),
    soapEnvelopeStyle = RayvarzSoapHttp.ResolveEnvelopeStyle(config),
    soapVersion = RayvarzSoapHttp.SoapVersionLabel(RayvarzSoapHttp.ResolveSoapVersion(config)),
    refRowDocNoInDetail = config["Rayvarz:RefRowDocNoInDetail"] ?? "headerDocRow",
    allowInvalidSsl = config.GetValue<bool>("Rayvarz:AllowInvalidSsl"),
    sourceSystemId = config["Rayvarz:SourceSystemId"],
    payloadSource = config["Rayvarz:PayloadSource"] ?? "LegacyCSharp",
    ruleEngineNidMember = config.GetValue("RuleEngine:NidMemberRayvarzRun", 1388),
    uiVersion = "3",
    features = new { rayvarzPing = true, rayvarzPostTest = true, rayvarzPostMinimalSave = true, tahator = true, unsentBatch = true },
    tahator = new
    {
        dryRun = config.GetValue<bool?>("Tahator:DryRun") ?? config.GetValue("Rayvarz:DryRun", true),
        pollIntervalMs = config.GetValue("Tahator:PollIntervalMs", 2000),
        pollTimeoutSeconds = config.GetValue("Tahator:PollTimeoutSeconds", 60),
        note = "تهاتر: تک‌کد — بدون اکسل؛ مسیر جدول واسط Accounting_DocHeader"
    },
    branches = new[] {
        new { id = 201, name = "منطقه 1", fund = 200201012 },
        new { id = 202, name = "منطقه 2", fund = 200202012 },
        new { id = 203, name = "منطقه 3", fund = 200203013 },
        new { id = 204, name = "منطقه 4", fund = 200204017 },
        new { id = 205, name = "منطقه 5", fund = 200205008 },
        new { id = 206, name = "منطقه 6", fund = 200206006 },
        new { id = 207, name = "منطقه 7", fund = 200207009 },
        new { id = 208, name = "منطقه 8", fund = 200208010 },
        new { id = 209, name = "منطقه 9", fund = 200209004 },
        new { id = 210, name = "منطقه 10", fund = 200210020 },
        new { id = 211, name = "منطقه 11", fund = 200211007 },
        new { id = 212, name = "منطقه 12", fund = 212210016 },
        new { id = 218, name = "منطقه ثامن", fund = 200218011 }
    }
});

app.MapGet("/api/db-test", async (IConfiguration config) =>
{
    var results = new List<object>();
    foreach (var name in new[] { "Sara", "Rayvarz", "RuleEngine", "RayvarzRuleEngine" })
    {
        var cs = config.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(cs))
        {
            results.Add(new { name, ok = false, error = "Connection string تنظیم نشده" });
            continue;
        }
        try
        {
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();
            if (name == "RayvarzRuleEngine")
            {
                var schemaSql = """
                    SELECT CASE WHEN EXISTS (
                        SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                        WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'RuleSyncState')
                    THEN 1 ELSE 0 END
                    """;
                await using var schemaCmd = new SqlCommand(schemaSql, conn);
                var schemaReady = Convert.ToInt32(await schemaCmd.ExecuteScalarAsync()) == 1;
                if (!schemaReady)
                {
                    results.Add(new
                    {
                        name,
                        ok = false,
                        server = conn.DataSource,
                        database = conn.Database,
                        error = "Invalid object name 'dbo.RuleSyncState'",
                        hint = "اتصال SQL برقرار است ولی جداول ساخته نشده‌اند. در SSMS روی سرور 232 اجرا کنید: database/01_RayvarzRuleEngine_Schema.sql سپس 02_RuleGolden_Seed.sql — Database باید RayvarzRuleEngine باشد نه DbRuleEngein."
                    });
                    continue;
                }
            }

            var sql = name switch
            {
                "Sara" => "SELECT TOP 1 FicheNo FROM dbo.Duty_Fiche",
                "Rayvarz" => "SELECT TOP 1 Ref FROM ray.incmdocsys",
                "RuleEngine" => "SELECT TOP 1 NidMember FROM dbo.Member WHERE NidMember = 1388",
                "RayvarzRuleEngine" => "SELECT TOP 1 NidMember FROM dbo.RuleSyncState",
                _ => "SELECT 1"
            };
            await using var cmd = new SqlCommand(sql, conn);
            var sample = (await cmd.ExecuteScalarAsync())?.ToString();
            results.Add(new { name, ok = true, server = conn.DataSource, database = conn.Database, sample });
        }
        catch (Exception ex)
        {
            var hint = ConnectionHint(name, cs, ex);
            results.Add(new { name, ok = false, error = ex.Message, inner = ex.InnerException?.Message, hint });
        }
    }
    return Results.Ok(new { connections = results });
});

app.MapGet("/api/rayvarz-ping", async (RayvarzClient client, CancellationToken ct) =>
    Results.Ok(await client.PingAsync(ct)));

app.MapGet("/api/rayvarz-post-test", async (RayvarzClient client, CancellationToken ct) =>
    Results.Ok(await client.PostProbeAsync(ct)));

app.MapGet("/api/rayvarz-post-minimal-save", async (RayvarzClient client, CancellationToken ct) =>
    Results.Ok(await client.PostMinimalSaveDocumentAsync(ct)));

app.MapPost("/api/fiche/load", async (LoadFicheRequest? req, FicheRepository repo, CancellationToken ct) =>
{
    if (req == null || string.IsNullOrWhiteSpace(req.IdentifierValue))
        return Results.BadRequest(new { error = "شناسه فیش خالی است" });

    try
    {
        var fiche = await repo.LoadAsync(req.IdentifierType, req.IdentifierValue.Trim(), ct);
        if (fiche == null)
            return Results.NotFound(new { error = "فیش در Income_Fiche یا Duty_Fiche یافت نشد" });

        try
        {
            var yr = DateHelper.ExtractShamsiYear(fiche.RayvarzDocDate);
            fiche.ExistsInRayvarz = await repo.ExistsInRayvarzAsync(fiche.FicheNo, yr > 0 ? yr : null, ct);
        }
        catch (Exception rayEx)
        {
            fiche.ExistsInRayvarz = false;
            fiche.StatusMessage = $"فیش بارگذاری شد — اتصال رایورز ناموفق: {rayEx.Message}";
            return Results.Ok(fiche);
        }

        if (fiche.ExistsInRayvarz)
            fiche.StatusMessage = "تکراری — در رایورز موجود است";
        else if (fiche.Payable <= 0)
            fiche.StatusMessage = "مبلغ قابل پرداخت صفر است";
        else if (fiche.Rows.Count == 0)
            fiche.StatusMessage = "ردیف IncmNo یافت نشد";
        else
            fiche.StatusMessage = "آماده ارسال";

        return Results.Ok(fiche);
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = $"خطا در بارگذاری: {ex.Message}" }, statusCode: 500);
    }
});

app.MapGet("/api/rule/schema-diagnostics", async (RuleEngineStore store, CancellationToken ct) =>
    Results.Ok(await store.GetDiagnosticsAsync(ct)));

app.MapGet("/api/rule/sync/state", async (RuleVersionManager mgr, RuleEngineStore store, CancellationToken ct) =>
{
    if (!store.IsConfigured)
        return Results.Json(new { error = "ConnectionStrings:RayvarzRuleEngine تنظیم نشده" }, statusCode: 503);
    if (!await store.IsSchemaReadyAsync(ct))
        return Results.Json(new
        {
            error = "جداول RayvarzRuleEngine ساخته نشده‌اند",
            hint = "روی سرور 232 اجرا کنید: database/01_RayvarzRuleEngine_Schema.sql و 02_RuleGolden_Seed.sql",
            database = store.ConfiguredDatabaseName
        }, statusCode: 503);

    var state = await store.GetSyncStateAsync(mgr.NidMember, ct);
    if (state == null)
        return Results.Ok(new { nidMember = mgr.NidMember, activeEngine = "Legacy", note = "RuleSyncState row missing — run POST /api/rule/sync/run after seed" });
    return Results.Ok(state);
});

app.MapPost("/api/rule/sync/run", async (RuleVersionManager mgr, CancellationToken ct) =>
{
    var state = await mgr.InitializeAsync(ct);
    return Results.Ok(new { ok = true, state });
});

app.MapGet("/api/rule/history/latest", async (MemberRuleRepository repo, IConfiguration config, CancellationToken ct) =>
{
    var nid = config.GetValue("RuleEngine:NidMemberRayvarzRun", 1388);
    var latest = await repo.LoadLatestHistoryAsync(nid, ct);
    if (latest == null)
        return Results.NotFound(new { error = "MemberHistory یافت نشد — ConnectionStrings:RuleEngine را چک کنید." });
    return Results.Ok(new
    {
        latest.NidHistory,
        latest.NidMember,
        latest.NidClass,
        latest.ModifyDateTime,
        latest.ModifyDateRaw,
        latest.ModifyTimeRaw,
        latest.Modifyer,
        latest.ModifyDesc,
        xmlBodyLength = latest.XmlBody.Length
    });
});

app.MapGet("/api/rule/golden", async (RuleEngineStore store, IConfiguration config, CancellationToken ct) =>
{
    var nid = config.GetValue("RuleEngine:NidMemberRayvarzRun", 1388);
    if (!store.IsConfigured)
        return Results.Json(new { error = "ConnectionStrings:RayvarzRuleEngine تنظیم نشده" }, statusCode: 503);

    var fiches = await store.GetActiveGoldenFichesAsync(nid, ct);
    var withRows = new List<object>();
    foreach (var g in fiches)
    {
        var rows = await store.GetExpectedRowsAsync(g.GoldenFicheId, ct);
        withRows.Add(new { g.GoldenFicheId, g.Name, g.FicheNo, g.NidFiche, g.Scenario, g.ExpectedRowCount, expectedRows = rows });
    }
    return Results.Ok(new { count = withRows.Count, fiches = withRows });
});

app.MapPost("/api/rule/golden/dry-run", async (
    GoldenDryRunService dryRun,
    IConfiguration config,
    CancellationToken ct) =>
{
    var summary = await dryRun.RunAllAsync(compareExpectedRows: true, ct);
    return Results.Ok(new
    {
        summary.EngineName,
        summary.Total,
        summary.Passed,
        summary.AllPassed,
        forceEngine = config["RuleEngine:ForceEngine"],
        cases = summary.Cases
    });
});

app.MapGet("/api/rule/engine", async (RuleEngineFactory factory, RuleEngineStore store, IConfiguration config, CancellationToken ct) =>
{
    var resolved = await factory.ResolveEngineNameAsync(ct);
    string? activeEngine = null;
    long? activeSnapshotId = null;
    int? activeDslVersion = null;
    if (store.IsConfigured && await store.IsSchemaReadyAsync(ct))
    {
        var state = await store.GetSyncStateAsync(factory.NidMember, ct);
        activeEngine = state?.ActiveEngine;
        activeSnapshotId = state?.ActiveSnapshotId;
        activeDslVersion = state?.ActiveDslVersion;
    }

    // PayloadSource = منبع ساخت SOAP (این اپ vs SaraBridge)؛ موتور واقعی = resolvedEngine/ActiveEngine
    return Results.Ok(new
    {
        nidMember = factory.NidMember,
        activeEngine = activeEngine ?? "Legacy",
        resolvedEngine = resolved,
        activeSnapshotId,
        activeDslVersion,
        payloadSource = config["Rayvarz:PayloadSource"] ?? "LegacyCSharp",
        payloadSourceNote = "LegacyCSharp = ساخت SOAP داخل همین اپ از روی ActiveEngine/DSL؛ RuleEngineBridge = Sara خارجی",
        forceEngine = config["RuleEngine:ForceEngine"],
        dryRun = config.GetValue<bool>("Rayvarz:DryRun")
    });
});

app.MapGet("/api/rule/dsl/latest", async (RuleEngineStore store, RuleDslParserService parser, CancellationToken ct) =>
{
    if (!store.IsConfigured)
        return Results.Json(new { error = "ConnectionStrings:RayvarzRuleEngine تنظیم نشده" }, statusCode: 503);

    var snapshot = await store.GetLatestSnapshotAsync(parser.NidMember, ct);
    if (snapshot == null)
        return Results.NotFound(new { error = "RuleDslSnapshot یافت نشد — POST /api/rule/dsl/parse را اجرا کنید." });

    return Results.Ok(new
    {
        snapshot.SnapshotId,
        snapshot.NidMember,
        snapshot.DslVersion,
        snapshot.XmlHash,
        snapshot.ParserVersion,
        snapshot.EntryPoint,
        snapshot.IsActive,
        snapshot.CreatedAtUtc,
        dslJsonLength = snapshot.DslJson?.Length ?? 0
    });
});

app.MapPost("/api/rule/dsl/parse", async (bool? force, RuleVersionManager mgr, RuleEngineStore store, CancellationToken ct) =>
{
    if (!store.IsConfigured)
        return Results.Json(new { error = "ConnectionStrings:RayvarzRuleEngine تنظیم نشده" }, statusCode: 503);

    try
    {
        var result = await mgr.ParseActiveMemberSnapshotAsync(forceRebuild: force == true, ct);
        return Results.Ok(new
        {
            result.Stored,
            result.SkippedExisting,
            result.SnapshotId,
            result.DslVersion,
            result.XmlHash,
            result.Message,
            parseSuccess = result.Parse?.Success,
            parseError = result.Parse?.ErrorMessage,
            entryPoint = result.Parse?.Program?.EntryPoint,
            functionCount = result.Parse?.Program?.Functions.Count,
            unsupportedFunctions = result.Parse?.Program?.UnsupportedFunctions,
            warnings = result.Parse?.Program?.Warnings,
            dslJsonLength = result.Parse?.Program != null
                ? RuleDslParserService.SerializeProgram(result.Parse.Program).Length
                : (int?)null
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message, inner = ex.InnerException?.Message }, statusCode: 500);
    }
});

app.MapPost("/api/rule/dsl/preview", (RuleDslParsePreviewRequest? req, RuleDslParserService parser) =>
{
    string xml;
    if (!string.IsNullOrWhiteSpace(req?.XmlBody))
    {
        xml = req.XmlBody;
    }
    else
    {
        return Results.BadRequest(new { error = "XmlBody در body لازم است (preview بدون ذخیره DB)." });
    }

    var parsed = parser.Parse(xml, "preview");
    if (!parsed.Success || parsed.Program == null)
        return Results.Json(new { error = parsed.ErrorMessage ?? "Parse failed" }, statusCode: 400);

    return Results.Ok(new
    {
        parsed.Envelope?.XmlHash,
        parsed.Program.EntryPoint,
        parsed.Program.ParserVersion,
        functions = parsed.Program.Functions.Select(f => new
        {
            f.Name,
            f.DisplayName,
            f.IsSupported,
            role = SupportedDslFunctions.GetRole(f.Name, f.DisplayName).ToString(),
            statementCount = f.Body.Count
        }),
        parsed.Program.UnsupportedFunctions,
        parsed.Program.Warnings,
        dsl = parsed.Program
    });
});

app.MapPost("/api/rule/dsl/validate", (RuleDslParsePreviewRequest? req, RuleDslParserService parser, DslValidator validator) =>
{
    if (string.IsNullOrWhiteSpace(req?.XmlBody))
        return Results.BadRequest(new { error = "XmlBody لازم است." });

    var parsed = parser.Parse(req.XmlBody, "validate");
    if (!parsed.Success || parsed.Program == null)
        return Results.Json(new { error = parsed.ErrorMessage }, statusCode: 400);

    var validation = validator.Validate(parsed.Program);
    return Results.Ok(new
    {
        validation.Success,
        validation.Errors,
        validation.Warnings,
        validation.UnknownOperations
    });
});

app.MapGet("/api/rule/promote/status", async (RulePromotionService promotion, CancellationToken ct) =>
    Results.Ok(await promotion.GetStatusAsync(ct)));

app.MapPost("/api/rule/promote/run", async (bool? force, RulePromotionService promotion, CancellationToken ct) =>
    Results.Ok(await promotion.EvaluatePromotionsAsync(forcePromote: force == true, ct)));

app.MapPost("/api/rule/promote/rollback", async (RulePromotionRollbackRequest? req, RulePromotionService promotion, CancellationToken ct) =>
    Results.Ok(await promotion.RollbackToLegacyAsync(req?.Reason, ct)));

app.MapGet("/api/rule/member/{nidMember:int}/meta", async (int nidMember, MemberRuleRepository repo, CancellationToken ct) =>
{
    try
    {
        var record = await repo.LoadActiveMemberAsync(nidMember, ct: ct);
        if (record == null || string.IsNullOrWhiteSpace(record.XmlBody))
            return Results.NotFound(new { error = "Member یا XmlBody یافت نشد — ConnectionStrings:RuleEngine یا RuleEngine:LocalXmlPath را تنظیم کنید." });

        var parsed = ClsFunctionParser.Parse(record.XmlBody);
        return Results.Ok(new
        {
            nidMember,
            record.Source,
            record.Version,
            record.VersionDateTime,
            parsed.NidClass,
            parsed.NidFunction,
            parsed.Name,
            parsed.DisplayText,
            parsed.IsActive,
            parsed.FormulaVersion,
            bodyLength = parsed.BodySource.Length,
            functionCount = parsed.FunctionNames.Count,
            functionsSample = parsed.FunctionNames.Take(25),
            hasNosazi = parsed.ContainsFunction("نوسازی") || parsed.ContainsFunction("Nosazi"),
            note = "XmlBody = ClsFunction با VB داخل Body؛ اجرا فقط در Sara یا SaraBridge."
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 500);
    }
});

app.MapPost("/api/fiche/preview", async (SendFicheRequest req, RayvarzPayloadBuilder payload, CancellationToken ct) =>
{
    var built = await payload.BuildAsync(req.Fiche, req.Branch, req.Fund, req.DocDate, req.ActDate, req.DueDate, ct);
    return Results.Ok(new
    {
        xml = built.Xml,
        payloadMode = built.Mode.ToString(),
        engineName = built.EngineName,
        warning = built.Warning,
        ruleMeta = built.RuleMeta
    });
});

app.MapPost("/api/tahator/check", async (TahatorFicheRequest? req, TahatorResendService tahator, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req?.FicheNo))
        return Results.BadRequest(new { error = "FicheNo تهاتر الزامی است (تک‌کد — بدون اکسل)." });
    try
    {
        return Results.Ok(await tahator.CheckAsync(req.FicheNo, ct));
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 500);
    }
});

app.MapPost("/api/tahator/send", async (TahatorFicheRequest? req, TahatorResendService tahator, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req?.FicheNo))
        return Results.BadRequest(new { error = "FicheNo تهاتر الزامی است (تک‌کد — بدون اکسل)." });
    try
    {
        return Results.Ok(await tahator.SendAsync(req, ct));
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message, steps = Array.Empty<string>() }, statusCode: 500);
    }
});

app.MapGet("/api/tahator/pending", async (TahatorResendService tahator, CancellationToken ct) =>
{
    var items = await tahator.ListPendingAsync(ct);
    return Results.Ok(new { count = items.Count, items });
});

app.MapPost("/api/tahator/restore", async (TahatorFicheRequest? req, TahatorResendService tahator, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req?.FicheNo))
        return Results.BadRequest(new { error = "FicheNo برای بازگردانی الزامی است." });
    try
    {
        return Results.Ok(await tahator.RestorePendingAsync(req.FicheNo, ct));
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 500);
    }
});

app.MapPost("/api/unsent/search", async (UnsentFicheSearchRequest? req, UnsentFicheService unsent, CancellationToken ct) =>
{
    if (req == null)
        return Results.BadRequest(new { error = "پارامترهای جستجو الزامی است" });
    if (string.IsNullOrWhiteSpace(req.FromDate) || string.IsNullOrWhiteSpace(req.ToDate))
        return Results.BadRequest(new { error = "از تاریخ و تا تاریخ الزامی است" });
    try
    {
        return Results.Ok(await unsent.SearchAsync(req, ct));
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 500);
    }
});

app.MapPost("/api/unsent/send-batch", async (UnsentBatchSendRequest? req, UnsentFicheService unsent, CancellationToken ct) =>
{
    if (req?.FicheNos == null || req.FicheNos.Count == 0)
        return Results.BadRequest(new { error = "حداقل یک فیش برای ارسال انتخاب کنید" });
    try
    {
        return Results.Ok(await unsent.SendBatchAsync(req, ct));
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 500);
    }
});

app.MapPost("/api/fiche/send", async (SendFicheRequest req, FicheSendService send, CancellationToken ct) =>
{
    try
    {
        var result = await send.SendAsync(req, ct);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Message.Contains("Ray_CityHall", StringComparison.OrdinalIgnoreCase)
        || ex.Message.Contains("incmdocsys", StringComparison.OrdinalIgnoreCase))
    {
        return Results.Json(new { error = $"اتصال SQL رایورز ناموفق: {ex.Message}" }, statusCode: 503);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

static string? ConnectionHint(string name, string cs, Exception ex)
{
    var msg = (ex.Message + " " + (ex.InnerException?.Message ?? "")).ToLowerInvariant();
    var usesIntegrated = cs.Contains("Integrated Security", StringComparison.OrdinalIgnoreCase)
        || cs.Contains("Trusted_Connection", StringComparison.OrdinalIgnoreCase);
    var usesIp = System.Text.RegularExpressions.Regex.IsMatch(cs, @"Server=tcp:\d+\.\d+\.\d+\.\d+");

    if (msg.Contains("login failed") && usesIntegrated)
        return "Sara با Integrated Security: برنامه باید با همان کاربر ویندوزی/دامنه اجرا شود که به SQL دسترسی دارد. اگر با IP وصل می‌شوید، به‌جای IP از نام سرور استفاده کنید یا SQL User/Password بگذارید.";
    if (msg.Contains("sspi") || msg.Contains("kerberos"))
        return "خطای احراز هویت ویندوزی (SSPI/Kerberos). نام سرور را به‌جای IP امتحان کنید یا از SQL Authentication استفاده کنید.";
    if (msg.Contains("network-related") || msg.Contains("could not open") || msg.Contains("timeout"))
        return $"سرور SQL ({name}) از این ماشین در دسترس نیست — VPN/فایروال/پورت 1433 را چک کنید.";
    if (msg.Contains("json") || msg.Contains("configuration"))
        return "خطای خواندن appsettings.json — ویرگول/کاما/گیومه در Password یا ساختار JSON را چک کنید.";
    if (name == "RayvarzRuleEngine" && msg.Contains("invalid object name") && msg.Contains("rulesyncstate"))
        return "جداول RayvarzRuleEngine ساخته نشده — فایل database/01_RayvarzRuleEngine_Schema.sql را روی سرور 232 اجرا کنید. ConnectionStrings:RayvarzRuleEngine باید Database=RayvarzRuleEngine باشد (نه DbRuleEngein).";
    if (name == "Rayvarz" && msg.Contains("login failed"))
        return "User Id یا Password رایورز اشتباه است. اگر Password کاراکتر ; یا \" دارد، در JSON باید escape شود.";
    return null;
}

app.Run();
