using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine;
using RayvarzResend.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.AddHttpClient();
builder.Services.AddSingleton<FicheRepository>();
builder.Services.AddSingleton<SoapBuilder>();
builder.Services.AddSingleton<RayvarzClient>();
builder.Services.AddSingleton<MemberRuleRepository>();
builder.Services.AddSingleton<RayvarzPayloadBuilder>();

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
    refRowDocNoInDetail = config["Rayvarz:RefRowDocNoInDetail"] ?? "zero",
    allowInvalidSsl = config.GetValue<bool>("Rayvarz:AllowInvalidSsl"),
    sourceSystemId = config["Rayvarz:SourceSystemId"],
    payloadSource = config["Rayvarz:PayloadSource"] ?? "LegacyCSharp",
    ruleEngineNidMember = config.GetValue("RuleEngine:NidMemberRayvarzRun", 1388),
    uiVersion = "3",
    features = new { rayvarzPing = true, rayvarzPostTest = true, rayvarzPostMinimalSave = true },
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
    foreach (var name in new[] { "Sara", "Rayvarz" })
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
            var sql = name == "Sara"
                ? "SELECT TOP 1 FicheNo FROM dbo.Duty_Fiche"
                : "SELECT TOP 1 Ref FROM ray.incmdocsys";
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
    return Results.Ok(new { xml = built.Xml, payloadMode = built.Mode.ToString(), warning = built.Warning, ruleMeta = built.RuleMeta });
});

app.MapPost("/api/fiche/send", async (SendFicheRequest? req, FicheRepository repo, RayvarzPayloadBuilder payload, RayvarzClient client, IConfiguration config, CancellationToken ct) =>
{
    // اعتبارسنجی سمت سرور — همان شرط‌هایی که UI چک می‌کند (دور زدن UI نباید ارسال نامعتبر بسازد)
    if (req?.Fiche == null || string.IsNullOrWhiteSpace(req.Fiche.FicheNo))
        return Results.BadRequest(new { error = "فیش ارسال نشده یا شماره فیش خالی است — ارسال نشد" });

    if (req.Fiche.Payable <= 0)
        return Results.BadRequest(new { error = "مبلغ قابل پرداخت صفر است — ارسال نشد" });

    if (!req.Fiche.Rows.Any(r => r.Val != 0))
        return Results.BadRequest(new { error = "ردیف IncmNo یافت نشد — ارسال نشد" });

    var fiche = req.Fiche;
    var incmdocsysYear = ResolveIncmdocsysYear(req);

    bool existsInRayvarz;
    string? sendWarning = null;
    try
    {
        existsInRayvarz = fiche.ExistsInRayvarz
            || await repo.ExistsInRayvarzAsync(fiche.FicheNo, incmdocsysYear > 0 ? incmdocsysYear : null, ct);
    }
    catch (SqlException ex)
    {
        // پیش‌فرض امن: بدون چک تکراری در Ray_CityHall ارسال نکن (فقط با false صریح ادامه می‌یابد)
        if (config.GetValue("Rayvarz:RequireRayvarzDbForSend", true))
        {
            return Results.Json(new
            {
                error = $"اتصال SQL رایورز (Ray_CityHall) ناموفق: {ex.Message}",
                hint = ConnectionHint("Rayvarz", config.GetConnectionString("Rayvarz") ?? "", ex)
            }, statusCode: 503);
        }

        existsInRayvarz = fiche.ExistsInRayvarz;
        sendWarning = $"چک تکراری در Ray_CityHall انجام نشد — ارسال SOAP ادامه یافت: {ex.Message}";
    }

    if (existsInRayvarz)
        return Results.BadRequest(new { error = "فیش در رایورز موجود است — ارسال نشد" });

    if (req.ResetStatus)
    {
        try { await repo.ResetStatusAsync(fiche, ct); }
        catch (Exception ex) { return Results.Problem($"خطا در ریست وضعیت: {ex.Message}"); }
    }

    var built = await payload.BuildAsync(fiche, req.Branch, req.Fund, req.DocDate, req.ActDate, req.DueDate, ct);
    var xml = built.Xml;
    var dryRun = config.GetValue<bool>("Rayvarz:DryRun");
    var result = await client.SendAsync(xml, dryRun, ct);
    result.Warning = CombineWarnings(fiche.Warning, CombineWarnings(sendWarning, built.Warning));

    if (!dryRun && result.Success)
    {
        try
        {
            result.VerifiedInRayvarz = await repo.ExistsInRayvarzAsync(
                fiche.FicheNo, incmdocsysYear > 0 ? incmdocsysYear : null, ct);
        }
        catch (SqlException ex)
        {
            result.VerifiedInRayvarz = false;
            result.Warning = CombineWarnings(result.Warning,
                $"تأیید incmdocsys ممکن نشد (SQL رایورز): {ex.Message}");
        }

        if (!result.VerifiedInRayvarz)
        {
            try
            {
                result.DocNotSentError = await repo.GetDocNotSentErrorAsync(fiche.FicheNo, ct);
            }
            catch (SqlException ex)
            {
                result.DocNotSentError = $"Accounting_DocNotSent (Sara): {ex.Message}";
            }

            result.Warning = CombineWarnings(result.Warning,
                SendResultVerification.BuildUnverifiedWarning(result.Success, result.VerifiedInRayvarz, dryRun));
        }
    }

    return Results.Ok(result);
});

static int ResolveIncmdocsysYear(SendFicheRequest req)
{
    foreach (var d in new[] { req.DocDate, req.ActDate, req.DueDate })
    {
        var y = DateHelper.ExtractShamsiYear(d);
        if (y > 0) return y;
    }

    foreach (var d in new[] { req.Fiche.RayvarzDocDate, req.Fiche.RayvarzActDate, req.Fiche.RayvarzDueDate })
    {
        var y = DateHelper.ExtractShamsiYear(d);
        if (y > 0) return y;
    }

    return 0;
}

static string? CombineWarnings(string? a, string? b)
{
    if (string.IsNullOrWhiteSpace(a)) return b;
    if (string.IsNullOrWhiteSpace(b)) return a;
    return a + " | " + b;
}

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
    if (name == "Rayvarz" && msg.Contains("login failed"))
        return "User Id یا Password رایورز اشتباه است. اگر Password کاراکتر ; یا \" دارد، در JSON باید escape شود.";
    return null;
}

app.Run();
