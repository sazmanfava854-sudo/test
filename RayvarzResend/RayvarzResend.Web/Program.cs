using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RayvarzResend.Web;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine;
using RayvarzResend.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.AddHttpClient();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "RayvarzResend.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(builder.Configuration.GetValue("Auth:SessionHours", 8));
        options.Events.OnRedirectToLogin = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/api"))
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            ctx.Response.Redirect("/login.html");
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/api"))
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            ctx.Response.Redirect("/login.html");
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.Authenticated, p => p.RequireAuthenticatedUser());
    options.AddPolicy(AuthPolicies.AdminOnly, p =>
        p.RequireAuthenticatedUser().RequireRole("Admin"));
});
builder.Services.AddSingleton<InMemoryAppUserStore>();
builder.Services.AddSingleton<AppUserRepository>();
builder.Services.AddSingleton<AppAuthService>();
builder.Services.AddSingleton<FicheRepository>();
builder.Services.AddSingleton<FicheSendService>();
builder.Services.AddSingleton<UnsentFicheService>();
builder.Services.AddSingleton<TahatorResendService>();
builder.Services.AddSingleton<SoapBuilder>();
builder.Services.AddSingleton<RayvarzClient>();
builder.Services.AddSingleton<MemberRuleRepository>();
builder.Services.AddSingleton<SaraBridgeStubService>();
builder.Services.AddSingleton<RayvarzPayloadBuilder>();
builder.Services.AddSingleton<InstallmentCheckService>();

var app = builder.Build();

app.Services.GetRequiredService<RayvarzPayloadBuilder>();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var auth = scope.ServiceProvider.GetRequiredService<AppAuthService>();
        await auth.EnsureBootstrapAdminAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AppAuth");
        logger.LogError(ex, "Bootstrap admin failed — login may be unavailable until DB is configured");
    }
}

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
app.UseAuthentication();
app.UseAuthorization();

var authenticated = AuthPolicies.Authenticated;
var adminOnly = AuthPolicies.AdminOnly;

app.MapPost("/api/auth/login", async (LoginRequest? req, AppAuthService auth, HttpContext http, CancellationToken ct) =>
{
    if (req == null || string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { error = "نام کاربری و رمز عبور الزامی است" });

    var user = await auth.ValidateCredentialsAsync(req.Username, req.Password, ct);
    if (user == null)
        return Results.Json(new { error = "نام کاربری یا رمز عبور اشتباه است" }, statusCode: 401);

    var principal = AppAuthService.BuildPrincipal(user);
    await http.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal,
        auth.CreateAuthProperties(persistent: true));
    return Results.Ok(AppAuthService.ToSession(user));
}).AllowAnonymous();

app.MapPost("/api/auth/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok(new { ok = true });
}).RequireAuthorization(authenticated);

app.MapGet("/api/auth/me", async (HttpContext http, AppAuthService auth, CancellationToken ct) =>
{
    var session = await auth.GetSessionAsync(http.User, ct);
    return session == null
        ? Results.Json(new { error = "نشست منقضی شده" }, statusCode: 401)
        : Results.Ok(session);
}).RequireAuthorization(authenticated);

app.MapGet("/api/admin/users", async (AppUserRepository users, CancellationToken ct) =>
    Results.Ok(new { items = await users.ListUsersAsync(ct) }))
    .RequireAuthorization(adminOnly);

app.MapPost("/api/admin/users", async (CreateAppUserRequest? req, AppUserRepository users, CancellationToken ct) =>
{
    if (req == null)
        return Results.BadRequest(new { error = "درخواست خالی است" });
    try
    {
        var created = await users.CreateUserAsync(req, ct);
        return Results.Ok(new
        {
            user = new AppUserDto
            {
                Id = created.Id,
                Username = created.Username,
                FirstName = created.FirstName,
                LastName = created.LastName,
                NationalId = created.NationalId,
                Position = created.Position,
                District = created.District,
                IsAdmin = created.IsAdmin,
                IsActive = created.IsActive,
                CreatedAtUtc = created.CreatedAtUtc.ToString("O")
            }
        });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).RequireAuthorization(adminOnly);

app.MapGet("/api/config", (IConfiguration config, HttpContext http) => new
{
    releaseVersion = ReleaseInfo.Number,
    releaseLabel = ReleaseInfo.Label,
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
    uiVersion = "5",
    auth = new
    {
        enabled = true,
        isAdmin = AppAuthService.IsAdmin(http.User)
    },
    features = new { rayvarzPing = true, rayvarzPostTest = true, rayvarzPostMinimalSave = true, tahator = true, unsentBatch = true, ruleEngineBridgeStub = true, auth = true, installmentCheck = true },
    tahator = new
    {
        dryRun = config.GetValue<bool?>("Tahator:DryRun") ?? config.GetValue("Rayvarz:DryRun", true),
        pollIntervalMs = config.GetValue("Tahator:PollIntervalMs", 2000),
        pollTimeoutSeconds = config.GetValue("Tahator:PollTimeoutSeconds", 60),
    },
    installment = new
    {
        dryRun = config.GetValue<bool?>("Installment:DryRun") ?? config.GetValue("Rayvarz:DryRun", true),
        connection = "ConnectionStrings:Sara",
        database = "Sara8M03",
        table = "dbo.Installment_List"
    },
    ruleEngine = new
    {
        payloadSource = config["Rayvarz:PayloadSource"] ?? "LegacyCSharp",
        useLocalBridgeStub = config.GetValue("RuleEngine:UseLocalBridgeStub", false),
        saraBridgeUrl = config["RuleEngine:SaraBridgeUrl"],
        nidMember = config.GetValue("RuleEngine:NidMemberRayvarzRun", 1388),
    },
    branches = new[] {
        new { id = 102, name = "شعبه مرکز", fund = 0 },
        new { id = 201, name = "منطقه 1", fund = 200201012 },
        new { id = 202, name = "منطقه 2", fund = 200202012 },
        new { id = 203, name = "منطقه 3", fund = 200203013 },
        new { id = 204, name = "منطقه 4", fund = 200204017 },
        new { id = 205, name = "منطقه 5", fund = 200205008 },
        new { id = 206, name = "منطقه 6", fund = 200206006 },
        new { id = 207, name = "منطقه 7", fund = 200207009 },
        new { id = 208, name = "منطقه 8", fund = 200208010 },
        new { id = 209, name = "منطقه 9", fund = 200209008 },
        new { id = 210, name = "منطقه 10", fund = 200210020 },
        new { id = 211, name = "منطقه 11", fund = 200211007 },
        new { id = 212, name = "منطقه 12", fund = 212210016 },
        new { id = 218, name = "منطقه ثامن", fund = 200218011 }
    }
}).RequireAuthorization(authenticated);

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
}).RequireAuthorization(adminOnly);

app.MapGet("/api/rayvarz-ping", async (RayvarzClient client, CancellationToken ct) =>
    Results.Ok(await client.PingAsync(ct))).RequireAuthorization(adminOnly);

app.MapGet("/api/rayvarz-post-test", async (RayvarzClient client, CancellationToken ct) =>
    Results.Ok(await client.PostProbeAsync(ct))).RequireAuthorization(adminOnly);

app.MapGet("/api/rayvarz-post-minimal-save", async (RayvarzClient client, CancellationToken ct) =>
    Results.Ok(await client.PostMinimalSaveDocumentAsync(ct))).RequireAuthorization(adminOnly);

app.MapPost("/api/fiche/load", async (LoadFicheRequest? req, FicheRepository repo, HttpContext http, CancellationToken ct) =>
{
    if (req == null || string.IsNullOrWhiteSpace(req.IdentifierValue))
        return Results.BadRequest(new { error = "شناسه فیش خالی است" });

    try
    {
        var value = req.IdentifierValue.Trim();
        FicheHeaderDto? fiche;
        IdentifierType usedType;

        if (req.FicheKind is { } kind)
        {
            (fiche, usedType) = await repo.LoadByKindWithAutoDetectAsync(kind, value, ct);
        }
        else
        {
            usedType = IdentifierDetector.Detect(value);
            fiche = await repo.LoadAsync(usedType, value, ct);
            if (fiche == null)
            {
                var alt = usedType == IdentifierType.FicheNo
                    ? IdentifierType.BillPaymentKey
                    : IdentifierType.FicheNo;
                fiche = await repo.LoadAsync(alt, value, ct);
                if (fiche != null) usedType = alt;
            }
        }

        if (fiche == null)
        {
            var table = req.FicheKind == UnsentFicheKind.Duty ? "Duty_Fiche" : req.FicheKind == UnsentFicheKind.Income ? "Income_Fiche" : "Income_Fiche یا Duty_Fiche";
            return Results.NotFound(new
            {
                error = $"فیش در {table} یافت نشد",
                detectedIdentifierType = usedType.ToString(),
                detectedIdentifierLabel = IdentifierDetector.Describe(usedType)
            });
        }

        var districtDenied = DistrictAccessService.GetAccessDeniedMessage(http.User, fiche);
        if (districtDenied != null)
            return Results.Json(new { error = districtDenied }, statusCode: 403);

        try
        {
            var yr = DateHelper.ExtractShamsiYear(fiche.RayvarzDocDate);
            fiche.ExistsInRayvarz = await repo.ExistsInRayvarzAsync(fiche.FicheNo, yr > 0 ? yr : null, ct);
        }
        catch (Exception rayEx)
        {
            fiche.ExistsInRayvarz = false;
            FicheSendService.ApplySendStatus(fiche);
            var rayWarn = $"بررسی تکراری رایورز ناموفق: {rayEx.Message}";
            fiche.StatusMessage = fiche.CanSend
                ? $"آماده ارسال — {rayWarn}"
                : $"{fiche.BlockReason} ({rayWarn})";
            return Results.Ok(fiche);
        }

        FicheSendService.ApplySendStatus(fiche);
        return Results.Ok(fiche);
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = $"خطا در بارگذاری: {ex.Message}" }, statusCode: 500);
    }
}).RequireAuthorization(authenticated);

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
}).RequireAuthorization(adminOnly);

app.MapGet("/api/rule/bridge/health", (IConfiguration config) => Results.Ok(new
{
    ok = true,
    mode = "LocalStub",
    contract = "POST /api/rule/bridge/build-save-document",
    payloadSource = config["Rayvarz:PayloadSource"] ?? "LegacyCSharp",
    useLocalBridgeStub = config.GetValue("RuleEngine:UseLocalBridgeStub", false),
    note = "Stub محلی — خروجی LegacyCSharp؛ Sara واقعی هنوز لازم است برای VB Member 1388."
})).RequireAuthorization(adminOnly);

app.MapPost("/api/rule/bridge/build-save-document", async (
    SaraBridgeBuildRequest? req,
    SaraBridgeStubService stub,
    CancellationToken ct) =>
{
    if (req == null)
        return Results.BadRequest(new SaraBridgeBuildResponse { Error = "بدنه درخواست خالی است." });
    try
    {
        var result = await stub.BuildAsync(req, ct);
        if (!string.IsNullOrWhiteSpace(result.Error))
            return Results.Json(result, statusCode: 404);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Json(new SaraBridgeBuildResponse { Error = ex.Message }, statusCode: 500);
    }
}).RequireAuthorization(adminOnly);

app.MapPost("/api/fiche/preview", async (
    [FromBody] SendFicheRequest req,
    [FromServices] RayvarzPayloadBuilder payloadBuilder,
    HttpContext http,
    CancellationToken ct) =>
{
    var districtDenied = DistrictAccessService.GetAccessDeniedMessage(http.User, req.Fiche);
    if (districtDenied != null)
        return Results.Json(new { error = districtDenied }, statusCode: 403);

    var blockReason = FicheSendService.ValidateSendable(req.Fiche);
    if (blockReason != null)
        return Results.BadRequest(new { error = blockReason });

    var built = await payloadBuilder.BuildAsync(req.Fiche, req.Branch, req.Fund, req.DocDate, req.ActDate, req.DueDate, ct);
    return Results.Ok(new { xml = built.Xml, payloadMode = built.Mode.ToString(), warning = built.Warning, ruleMeta = built.RuleMeta });
}).RequireAuthorization(adminOnly);

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
}).RequireAuthorization(adminOnly);

app.MapPost("/api/tahator/send", async (TahatorFicheRequest? req, TahatorResendService tahator, FicheRepository repo, HttpContext http, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req?.FicheNo))
        return Results.BadRequest(new { error = "FicheNo تهاتر الزامی است (تک‌کد — بدون اکسل)." });
    try
    {
        var loaded = await repo.LoadAsync(IdentifierType.FicheNo, req.FicheNo.Trim(), ct);
        if (loaded != null)
        {
            var districtDenied = DistrictAccessService.GetAccessDeniedMessage(http.User, loaded);
            if (districtDenied != null)
                return Results.Json(new { error = districtDenied }, statusCode: 403);
        }

        return Results.Ok(await tahator.SendAsync(req, ct));
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message, steps = Array.Empty<string>() }, statusCode: 500);
    }
}).RequireAuthorization(authenticated);

app.MapPost("/api/fiche/send", async (SendFicheRequest? req, FicheSendService send, HttpContext http, CancellationToken ct) =>
{
    if (req?.Fiche == null || string.IsNullOrWhiteSpace(req.Fiche.FicheNo))
        return Results.BadRequest(new { error = "فیش ارسال نشده یا شماره فیش خالی است — ارسال نشد" });

    var districtDenied = DistrictAccessService.GetAccessDeniedMessage(http.User, req.Fiche);
    if (districtDenied != null)
        return Results.Json(new { error = districtDenied }, statusCode: 403);

    try
    {
        return Results.Ok(await send.SendAsync(req, ct));
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (SqlException ex)
    {
        return Results.Json(new
        {
            error = ex.Message,
            hint = ConnectionHint("Rayvarz", "", ex)
        }, statusCode: 503);
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 500);
    }
}).RequireAuthorization(authenticated);

app.MapPost("/api/unsent/search", async (UnsentFicheSearchRequest? req, UnsentFicheService unsent, CancellationToken ct) =>
{
    if (req == null)
        return Results.BadRequest(new { error = "درخواست جستجو خالی است" });
    if (req.HasPartialDateRange)
        return Results.BadRequest(new { error = "هر دو تاریخ از و تا را وارد کنید" });
    if (!req.HasDateRange)
        return Results.BadRequest(new { error = "بازه تاریخ (از و تا) برای جستجوی فیش‌های ارسال‌نشده الزامی است" });
    if (!req.HasAnyFilter)
        return Results.BadRequest(new { error = "حداقل یک فیلتر (تاریخ، فیش، قبض، پرداخت، منطقه) لازم است" });
    try
    {
        return Results.Ok(await unsent.SearchAsync(req, ct));
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 500);
    }
}).RequireAuthorization(adminOnly);

app.MapPost("/api/unsent/plan-batch", async (UnsentBatchSendRequest? req, UnsentFicheService unsent, CancellationToken ct) =>
{
    if (req?.FicheNos == null || req.FicheNos.Count == 0)
        return Results.BadRequest(new { error = "حداقل یک فیش انتخاب کنید" });
    try
    {
        return Results.Ok(await unsent.PlanBatchAsync(req, ct));
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 500);
    }
}).RequireAuthorization(adminOnly);

app.MapPost("/api/unsent/send-batch", async (UnsentBatchSendRequest? req, UnsentFicheService unsent, CancellationToken ct) =>
{
    if (req?.FicheNos == null || req.FicheNos.Count == 0)
        return Results.BadRequest(new { error = "حداقل یک فیش انتخاب کنید" });
    try
    {
        return Results.Ok(await unsent.SendBatchAsync(req, ct));
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 500);
    }
}).RequireAuthorization(adminOnly);

app.MapPost("/api/installment/preview", async (
    InstallmentCheckRequest? req,
    InstallmentCheckService installment,
    HttpContext http,
    CancellationToken ct) =>
{
    if (req == null)
        return Results.BadRequest(new { error = "درخواست خالی است" });
    req.PerformedByUser = AppAuthService.ResolveCommentUserName(http.User);
    try
    {
        var result = await installment.PreviewAsync(req, ct);
        if (!string.IsNullOrWhiteSpace(result.Error))
            return Results.BadRequest(new { error = result.Error });
        return Results.Ok(result);
    }
    catch (SqlException ex)
    {
        return Results.Json(new { error = ex.Message, hint = ConnectionHint("Sara", "", ex) }, statusCode: 503);
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 500);
    }
}).RequireAuthorization(authenticated);

app.MapPost("/api/installment/update", async (
    InstallmentCheckRequest? req,
    InstallmentCheckService installment,
    HttpContext http,
    CancellationToken ct) =>
{
    if (req == null)
        return Results.BadRequest(new { error = "درخواست خالی است" });
    req.PerformedByUser = AppAuthService.ResolveCommentUserName(http.User);
    try
    {
        var result = await installment.UpdateAsync(req, ct);
        if (!string.IsNullOrWhiteSpace(result.Error))
            return Results.BadRequest(new { error = result.Error });
        return Results.Ok(result);
    }
    catch (SqlException ex)
    {
        return Results.Json(new { error = ex.Message, hint = ConnectionHint("Sara", "", ex) }, statusCode: 503);
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 500);
    }
}).RequireAuthorization(authenticated);

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
