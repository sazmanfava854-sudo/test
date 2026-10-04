using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RayvarzResend.Web;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine;
using RayvarzResend.Web.Services;

WebApplicationBuilder builder;
try
{
    AppSettingsJsonGuard.ValidateOrThrow(Directory.GetCurrentDirectory());
    builder = WebApplication.CreateBuilder(args);
}
catch (Exception ex) when (AppSettingsJsonGuard.IsLoadError(ex))
{
    var message = AppSettingsJsonGuard.Describe(ex, Directory.GetCurrentDirectory());
    Console.OutputEncoding = System.Text.Encoding.UTF8;
    Console.Error.WriteLine(message);
    System.Diagnostics.Debug.WriteLine(message);
    Environment.Exit(1);
    throw;
}
AppSettingsConfiguration.UseSingleAppSettingsJsonOnly(builder.Configuration);
if (builder.Configuration is IConfigurationRoot configRoot)
    configRoot.Reload();

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.AddHttpClient();
builder.Services.AddHttpClient(MashhadSsoApiClient.HttpClientName)
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseProxy = false });
builder.Services.Configure<ShimasAuthOptions>(builder.Configuration.GetSection(ShimasAuthOptions.SectionName));
builder.Services.PostConfigure<ShimasAuthOptions>(o =>
    ShimasAuthConfiguration.ApplyMashhadAliases(builder.Configuration, o));
builder.Services.Configure<BankInquiryConfirmOptions>(builder.Configuration.GetSection(BankInquiryConfirmOptions.SectionName));
builder.Services.AddHttpClient(BankInquiryApiClient.HttpClientName)
    .ConfigurePrimaryHttpMessageHandler(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<BankInquiryConfirmOptions>>().Value;
        return BankInquiryHttpHandlerFactory.Create(config, options);
    });
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "RayvarzResend.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        // HTTP داخلی (مثلاً 5.252.216.140:8070/login.html) — کوکی Secure فقط وقتی درخواست HTTPS است
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(builder.Configuration.GetValue("Auth:SessionHours", 8));
        options.Events.OnRedirectToLogin = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/api"))
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            var shimas = ctx.HttpContext.RequestServices.GetRequiredService<ShimasAuthService>();
            ctx.Response.Redirect(shimas.ResolveLoginRedirectPath(ctx.Request));
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/api"))
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            var shimas = ctx.HttpContext.RequestServices.GetRequiredService<ShimasAuthService>();
            ctx.Response.Redirect(shimas.ResolveLoginRedirectPath(ctx.Request));
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
builder.Services.AddSingleton<AppPermissionService>();
builder.Services.AddSingleton<AppAuthService>();
builder.Services.AddSingleton<MashhadSsoApiClient>();
builder.Services.AddSingleton<ShimasAuthService>();
builder.Services.AddSingleton<FicheRepository>();
builder.Services.AddSingleton<AccountingDocWriter>();
builder.Services.AddSingleton<FicheSendService>();
builder.Services.AddSingleton<UnsentFicheService>();
builder.Services.AddSingleton<TahatorResendService>();
builder.Services.AddSingleton<SoapBuilder>();
builder.Services.AddSingleton<RayvarzClient>();
builder.Services.AddSingleton<MemberRuleRepository>();
builder.Services.AddSingleton<SaraBridgeStubService>();
builder.Services.AddSingleton<RayvarzPayloadBuilder>();
builder.Services.AddSingleton<InstallmentCheckService>();
builder.Services.AddSingleton<FicheDateChangeService>();
builder.Services.AddSingleton<BankInquiryApiClient>();
builder.Services.AddSingleton<EpayFichePresenceChecker>();
builder.Services.AddSingleton<BankInquiryConfirmService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedProto
        | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

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

app.UseAuthentication();
app.UseAuthorization();

async Task<IResult?> TryCompleteSsoCallbackAsync(HttpContext http, CancellationToken ct)
{
    var shimas = http.RequestServices.GetRequiredService<ShimasAuthService>();
    var auth = http.RequestServices.GetRequiredService<AppAuthService>();

    var callback = shimas.ParseCallbackQuery(http.Request.Query);
    if (!shimas.ValidateReturnedState(http, callback.State))
    {
        shimas.ClearSsoFlowCookies(http);
        var error = Uri.EscapeDataString("state بازگشت SSO معتبر نیست — دوباره وارد شوید");
        return Results.Redirect($"/login.html?error={error}");
    }

    var validation = await shimas.ValidateAsync(
        callback.Username,
        callback.RefreshToken,
        callback.Domain,
        ct);
    if (!validation.Success)
    {
        shimas.ClearSsoFlowCookies(http);
        var error = Uri.EscapeDataString(validation.Error ?? "ورود ناموفق");
        return Results.Redirect($"/login.html?error={error}");
    }

    if (!string.IsNullOrWhiteSpace(callback.Domain))
        validation.Profile.Domain = callback.Domain;

    var user = await shimas.ResolveOrCreateUserAsync(validation.Profile, ct);
    if (user == null)
    {
        shimas.ClearSsoFlowCookies(http);
        var hint = Uri.EscapeDataString(
            "کاربر در دستیار مالی ثبت نشده یا غیرفعال است — کد ملی/دامین را در مدیریت کاربران اضافه کنید (AutoProvisionUsers=false).");
        return Results.Redirect($"/login.html?error={hint}");
    }

    await auth.SignInAsync(http, user, ct);
    shimas.ClearSsoFlowCookies(http);
    return Results.Redirect(shimas.ResolvePostLoginRedirect(http));
}

// بازگشت SSO روی ریشه (https://city.mashhad.ir:5065?refresh_token=...) قبل از هدایت به /auth/login
app.Use(async (context, next) =>
{
    var shimas = context.RequestServices.GetRequiredService<ShimasAuthService>();
    if (!shimas.IsSsoCallbackHttpRequest(context.Request))
    {
        if (shimas.Options.DebugSigning)
        {
            var probe = shimas.ProbeSsoCallbackHttpRequest(context.Request);
            if (probe.PathMatchesCallback && probe.RefreshTokenLength > 0 && !probe.IsSsoCallbackHttpRequest)
            {
                var log = context.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("ShimasSsoCallback");
                log.LogWarning(
                    "درخواست شبیه callback روی {Path} رد شد: {Reason} (keys: {Keys})",
                    probe.EffectivePath,
                    probe.RejectionReasonFa,
                    string.Join(", ", probe.QueryKeys));
            }
        }

        await next();
        return;
    }

    try
    {
        var result = await TryCompleteSsoCallbackAsync(context, context.RequestAborted);
        if (result != null)
            await result.ExecuteAsync(context);
    }
    catch (SqlException ex)
    {
        var dbResult = AuthDatabaseError(ex);
        await dbResult.ExecuteAsync(context);
    }
});

static bool RequiresAuthenticatedShell(string path) =>
    path.Equals("/", StringComparison.OrdinalIgnoreCase)
    || path.Equals("/index.html", StringComparison.OrdinalIgnoreCase)
    || ManagementHubPaths.IsHubPage(path);

// index.html و صفحهٔ مدیریت بدون لاگین سرو نشود — جلوگیری از فلش UI قبل از redirect کلاینت
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "";
    var authenticated = context.User?.Identity?.IsAuthenticated == true;

    if (RequiresAuthenticatedShell(path) && !authenticated)
    {
        var shimas = context.RequestServices.GetRequiredService<ShimasAuthService>();
        var probe = shimas.ProbeSsoCallbackHttpRequest(context.Request);
        if (probe.PathMatchesCallback && probe.RefreshTokenLength > 0 && !probe.IsSsoCallbackHttpRequest)
        {
            var error = Uri.EscapeDataString(probe.RejectionReasonFa ?? "بازگشت SSO ناقص است");
            context.Response.Redirect($"/login.html?error={error}");
            return;
        }

        var login = shimas.ResolveLoginRedirectPath(context.Request);
        var target = ManagementHubPaths.IsHubPage(path)
            ? $"{login}?returnUrl={Uri.EscapeDataString(ManagementHubPaths.CanonicalWithSlash)}"
            : login;
        context.Response.Redirect(target);
        return;
    }

    // /management ، /MANAGMENT و ... → /management/ (فایل‌های استاتیک با حروف کوچک)
    if (ManagementHubPaths.NeedsCanonicalRedirect(path))
    {
        var rest = ManagementHubPaths.IsRoot(path) ? "" : path[(path.IndexOf('/', 1) + 1)..];
        context.Response.Redirect(ManagementHubPaths.CanonicalWithSlash + rest + context.Request.QueryString);
        return;
    }

    await next();
});

app.UseDefaultFiles();
app.UseStaticFiles();

var authenticated = AuthPolicies.Authenticated;
var adminOnly = AuthPolicies.AdminOnly;

app.MapGet("/api/auth/mode", (HttpContext http, ShimasAuthService shimas) =>
    Results.Ok(shimas.GetStatus(http.Request))).AllowAnonymous();

app.MapGet("/api/auth/sso-outbound-map", (ShimasAuthService shimas) =>
{
    var map = SsoOutboundFieldMap.Describe(shimas.Options);
    return Results.Ok(new
    {
        map,
        correctExampleFa = new
        {
            header_apiName = "FinancialAssistant",
            body_ClientId = "53db42619cf3C333b13a18D34fbd9111",
            appsettings = new
            {
                ApiName_or_SSOUserName = "FinancialAssistant",
                ClientId_or_LKey = "53db42619cf3C333b13a18D34fbd9111",
                ClientSecret = "(SecretKey کامل)",
                LoginKeyBodyClientIdIsApiName = false
            }
        }
    });
}).AllowAnonymous();

// دیباگ hash: همان apiName / requestTime / apiSecret و بدنه Time/Hash/ClientId که اپ می‌فرستد (بدون ClientSecret)
app.MapGet("/api/auth/sso-signing-preview", async (ShimasAuthService shimas, CancellationToken ct) =>
{
    var preview = await shimas.PreviewLoginKeySigningAsync(ct);
    return Results.Ok(preview);
}).AllowAnonymous();

app.MapGet("/api/auth/sso-callback-probe", (HttpContext http, ShimasAuthService shimas) =>
{
    var probe = shimas.ProbeSsoCallbackHttpRequest(http.Request);
    return Results.Ok(new
    {
        probe,
        hintFa = probe.IsSsoCallbackHttpRequest
            ? "این درخواست باید در middleware قبل از await next() به TryCompleteSsoCallbackAsync بخورد."
            : "اگر بعد از لاگین مشهد این را می‌بینید، همان URL را در مرورگر باز کنید یا در VS روی Path/QueryString بریک‌پوینت بگذارید — " + (probe.RejectionReasonFa ?? "")
    });
}).AllowAnonymous();

app.MapGet("/api/auth/sso-return-url", (HttpContext http, ShimasAuthService shimas) =>
{
    var callback = shimas.BuildCallbackAbsoluteUrl(http.Request);
    return Results.Ok(new
    {
        registerInSsoPortal = callback,
        descriptionFa = "این آدرس را در پورتال SSO برای FinancialAssistant در فیلد «برگشت آدرس» ثبت کنید. بعد از لاگین، SSO کاربر را به این URL با ?username&refresh_token&state برمی‌گرداند.",
        postLoginAppPath = shimas.Options.PostLoginDefaultPath,
        sampleLoginUrl = shimas.DescribeLoginStartForPortal(http.Request),
        clientIdLength = shimas.Options.EffectiveClientId.Length,
        clientIdConfigured = shimas.Options.EffectiveClientId.Length >= 4,
        debugLoginCheckUrl = $"{http.Request.Scheme}://{http.Request.Host}/auth/login?debug=1",
        noteFa = "برگشت آدرس SSO باید https://city.mashhad.ir:5065/management باشد؛ اگر هنوز Profile.aspx می‌بینید، ورود را از city.mashhad.ir:5065 شروع کنید و در Login.aspx پارامتر returnUrl را ببینید."
    });
}).AllowAnonymous();

app.MapGet("/auth/login", async (HttpContext http, ShimasAuthService shimas, CancellationToken ct) =>
{
    // روی localhost با PublicBaseUrl=city معمولاً به city redirect می‌شود — با ?debug=1 همان‌جا بمان تا F10 روی PC ممکن شود.
    if (shimas.UsesPublicSsoLoginUrl(http.Request) && !http.Request.Query.ContainsKey("debug"))
    {
        var target = shimas.ResolveLoginPath(http.Request);
        var ret = http.Request.Query["returnUrl"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(ret))
            target += "?returnUrl=" + Uri.EscapeDataString(ret);
        return Results.Redirect(target);
    }

    if (!shimas.Options.PreferSsoLoginForHost(http.Request.Host.Host))
        return Results.Redirect("/login.html");

    if (!shimas.Options.SsoReady)
    {
        if (shimas.Options.AllowLocalLoginFallback)
            return Results.Redirect("/login.html");
        return Results.Content("SSO پیکربندی نشده — ClientId و ClientSecret را در Auth:Shimas تنظیم کنید.", "text/plain; charset=utf-8", statusCode: 503);
    }

    try
    {
        var returnPath = http.Request.Query["returnUrl"].FirstOrDefault()
            ?? shimas.Options.PostLoginDefaultPath;
        shimas.RememberPostLoginReturn(http, returnPath);
        var callbackUrl = shimas.BuildCallbackAbsoluteUrl(http.Request);
        var clientId = shimas.Options.EffectiveClientId;
        if (clientId.Length < 4)
        {
            var logger = http.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("ShimasAuth");
            logger.LogWarning("ClientId/lkey در appsettings خالی یا خیلی کوتاه است ({Length} کاراکتر)", clientId.Length);
        }

        var loginUrl = await shimas.BuildExternalLoginUrlAsync(callbackUrl, http, ct);
        if (http.Request.Query.ContainsKey("debug"))
        {
            var encoded = Uri.EscapeDataString(callbackUrl);
            var diag = await shimas.DiagnoseLoginKeyAsync(callbackUrl, ct);
            var flow = loginUrl.Contains("/Authentication/Start/", StringComparison.OrdinalIgnoreCase)
                ? "loginKey → Authentication/Start/{loginKey} (روش سند SSO؛ برگشت به آدرس ثبت‌شده در پورتال)"
                : "Login.aspx?lkey&returnUrl (روش قدیمی؛ اگر SSO روی Profile.aspx بماند، این روش را قبول نمی‌کند)";
            var body = $"""
                برگشت آدرس (ثبت SSO) = همان آدرسی که به پورتال دادید:
                {callbackUrl}

                این مقدار باید داخل آدرس ورود به login.mashhad.ir باشد (پارامتر returnUrl):
                returnUrl={encoded}

                روش ورود فعلی: {flow}

                آدرس کامل ورود (کپی در مرورگر و قبل از لاگین چک کنید returnUrl هست):
                {loginUrl}

                ClientId/lkey تنظیم شده: بله (طول {clientId.Length} کاراکتر)
                ApiName (هدر apiName): {shimas.Options.SigningApiName}
                ClientId بدنه loginKey: {shimas.Options.EffectiveLoginKeyBodyClientId}
                LoginKeyBodyClientIdIsApiName: {shimas.Options.LoginKeyBodyClientIdIsApiName}
                جابه‌جایی احتمالی در appsettings: {SsoOutboundFieldMap.LikelyApiNameAndClientIdSwapped(shimas.Options)}
                نقشه کامل: /api/auth/sso-outbound-map
                hash دقیق (هدر+بدنه): /api/auth/sso-signing-preview
                لاگ hash روی هر درخواست SSO: Auth:Shimas:DebugSigning = true
                UseLoginKeyOnRedirect: {shimas.Options.UseLoginKeyOnRedirect}

                ---- تست loginKey با SSO ({diag.ApiBaseUrl}) ----
                getCurrentTime: {(diag.GetCurrentTimeOk ? "OK" : "ناموفق")}
                loginKey: {(diag.LoginKeyOk ? "OK" : $"ناموفق (code {diag.LoginKeyErrorCode}: {diag.LoginKeyErrorMessage})")}
                ClientSecret طول: {diag.ClientSecretLength} کاراکتر
                نتیجه: {diag.Verdict}
                {(diag.StartUrlSample != null ? "نمونه Start URL: " + diag.StartUrlSample : "")}
                """;
            return Results.Content(body, "text/plain; charset=utf-8");
        }

        return Results.Redirect(loginUrl);
    }
    catch (Exception ex)
    {
        return Results.Content($"خطا در آماده‌سازی ورود SSO: {ex.Message}", "text/plain; charset=utf-8", statusCode: 503);
    }
}).AllowAnonymous();

app.MapGet("/api/auth/sso-loginkey-check", async (HttpContext http, ShimasAuthService shimas, CancellationToken ct) =>
{
    var callback = shimas.BuildCallbackAbsoluteUrl(http.Request);
    var diag = await shimas.DiagnoseLoginKeyAsync(callback, ct);
    return Results.Ok(new
    {
        diag.ApiBaseUrl,
        diag.ApiName,
        diag.ClientIdMasked,
        diag.ClientIdLength,
        diag.ClientSecretLength,
        diag.UseLoginKeyOnRedirect,
        diag.ReturnUrlRegisteredInPortal,
        diag.GetCurrentTimeOk,
        diag.LoginKeyOk,
        diag.LoginKeyErrorCode,
        diag.LoginKeyErrorMessage,
        diag.StartUrlSample,
        verdictFa = diag.Verdict
    });
}).AllowAnonymous();

app.MapGet("/api/auth/sso-credential-compare", (ShimasAuthService shimas, IConfiguration config) =>
{
    static (string source, string apiName, string clientId, string secret) ReadRow(
        string label, string? apiName, string? clientId, string? secret) =>
        (label, (apiName ?? "").Trim(), (clientId ?? "").Trim(), (secret ?? "").Trim());

    static object ToDto((string source, string apiName, string clientId, string secret) row) => new
    {
        source = row.source,
        apiName = row.apiName,
        clientIdMasked = SsoCredentialMask.MaskId(row.clientId),
        clientIdLength = row.clientId.Length,
        secretLength = row.secret.Length,
        secretLooksShort = SsoCredentialMask.SecretLooksTooShort(row.secret)
    };

    var shimasRow = ReadRow(
        "Auth:Shimas (فعال در اپ)",
        shimas.Options.SigningApiName,
        shimas.Options.EffectiveClientId,
        shimas.Options.ClientSecret);

    var settingsRow = ReadRow(
        "Settings (RuleEngine)",
        config["Settings:SSOUserName"],
        config["Settings:SSOClientId"],
        config["Settings:SSOSecret"]);

    var sameApiName = string.Equals(shimasRow.apiName, settingsRow.apiName, StringComparison.Ordinal);
    var sameClientId = shimas.Options.EffectiveClientId.Trim()
        == (config["Settings:SSOClientId"] ?? "").Trim();
    var settingsSecretLen = (config["Settings:SSOSecret"] ?? "").Trim().Length;
    var shimasSecretLen = (shimas.Options.ClientSecret ?? "").Trim().Length;
    var sameSecretLength = settingsSecretLen > 0 && settingsSecretLen == shimasSecretLen;

    return Results.Ok(new
    {
        shimas = ToDto(shimasRow),
        settings = ToDto(settingsRow),
        aligned = sameApiName && sameClientId && (settingsSecretLen == 0 || sameSecretLength),
        noteFa = "اگر loginKey 403 است و همهٔ فرمول‌های probe شکست خورد، دیباگ کد کمکی نمی‌کند — trio apiName/ClientId/SecretKey را با پورتال SSO یکی کنید. "
            + "اگر RuleEngine روی همان سرور SSO دارد، `sso-loginkey-probe?profile=settings` را بزنید؛ اگر آن OK شد، مقادیر Settings را در Auth:Shimas کپی کنید. "
            + "دیباگر Visual Studio فقط با Attach به w3wp همان سایت 5065 هنگام باز کردن این URLها breakpoint می‌خورد، نه F5 لوکال."
    });
}).AllowAnonymous();

// تشخیص 403 Client info missmatched:
//   /api/auth/sso-loginkey-probe                → اعتبار FinancialAssistant (Auth:Shimas) با همهٔ فرمول‌های هش
//   /api/auth/sso-loginkey-probe?profile=settings → اعتبار RuleEngine از بلوک Settings (SSOUserName/SSOClientId/SSOSecret)
//   &apiName=<نام>                                → همان تست با apiName دیگر (بدون تغییر appsettings) — Secret هرگز از URL گرفته نمی‌شود
app.MapGet("/api/auth/sso-loginkey-probe", async (HttpContext http, ShimasAuthService shimas, IConfiguration config, CancellationToken ct) =>
{
    var profile = (http.Request.Query["profile"].FirstOrDefault() ?? "").Trim().ToLowerInvariant();
    var apiNameOverride = (http.Request.Query["apiName"].FirstOrDefault() ?? "").Trim();
    string apiName, clientId, secret, source;
    if (profile == "settings")
    {
        source = "Settings (RuleEngine)";
        apiName = (config["Settings:SSOUserName"] ?? "").Trim();
        clientId = (config["Settings:SSOClientId"] ?? "").Trim();
        secret = (config["Settings:SSOSecret"] ?? "").Trim();
        if (apiName.Length == 0 || clientId.Length == 0 || secret.Length == 0)
            return Results.Json(new
            {
                error = "بلوک Settings کامل نیست — SSOUserName / SSOClientId / SSOSecret همان RuleEngine را در ریشهٔ appsettings.json بگذارید.",
                sample = new { Settings = new { SSOUserName = "<apiName از پورتال>", SSOClientId = "<ClientId>", SSOSecret = "<SecretKey>" } }
            }, statusCode: 400);
    }
    else
    {
        source = "Auth:Shimas (FinancialAssistant)";
        apiName = shimas.Options.SigningApiName;
        clientId = shimas.Options.EffectiveClientId;
        secret = (shimas.Options.ClientSecret ?? "").Trim();
    }

    var configuredApiName = apiName;
    if (apiNameOverride.Length > 0)
        apiName = apiNameOverride;

    var results = await shimas.ProbeLoginKeyVariantsAsync(apiName, clientId, secret, ct);
    var winner = results.FirstOrDefault(r => r.Ok);
    var ssoUnreachable = winner == null && results.Count == 1 && results[0].Variant == "getCurrentTime";
    var allClientInfoMismatch = winner == null && !ssoUnreachable
        && results.All(r => r.ErrorCode == 403 && (r.ErrorMessage ?? "").Contains("missmatch", StringComparison.OrdinalIgnoreCase));
    return Results.Ok(new
    {
        source,
        apiName,
        apiNameFromQuery = apiNameOverride.Length > 0 ? apiNameOverride : null,
        configuredApiName,
        clientIdMasked = SsoCredentialMask.MaskId(clientId),
        clientIdLength = clientId.Length,
        secretLength = secret.Length,
        currentFormula = $"sha256(secret+time) hex {(string.Equals(shimas.Options.HashEncoding, "upper", StringComparison.OrdinalIgnoreCase) ? "upper" : "lower")} (RuleEngine / Auth:Shimas:ApiSecretConcatOrder)",
        anyOk = winner != null,
        ssoReachable = !ssoUnreachable,
        workingFormula = winner?.Variant,
        verdictFa = winner != null
            ? (winner.Variant.StartsWith("sha256(secret+time) hex", StringComparison.Ordinal)
                ? (apiNameOverride.Length > 0
                    ? $"با apiName «{apiName}» OK شد — همین را در Auth:Shimas:ApiName (SSOUserName) بگذارید."
                    : "فرمول فعلی درست است — پس مشکل از اعتبار (ClientId/Secret/apiName) است، نه کد.")
                : winner.Variant.StartsWith("apiName = ClientId", StringComparison.Ordinal)
                    ? "SSO وقتی apiName = ClientId بود OK شد — Auth:Shimas:ApiName را همان ClientId بگذارید."
                    : $"SSO با «{winner.Variant}» جواب داد — شکل درخواست/فرمول در کد باید همین شود.")
            : ssoUnreachable
                ? "getCurrentTime جواب نداد — SSO از این سرور در دسترس نیست (شبکه/فایروال)؛ هیچ فرمولی آزموده نشد."
                : allClientInfoMismatch
                    ? "همهٔ فرمول‌ها عیناً «Client info missmatched» — طبق سند SSO (صفحه ۲۰) یعنی apiName (نام کاربری کاربردی برنامه) یا ClientId/SecretKey با ثبت پورتال یکی نیست؛ نام نمایشی برنامه (مثل FinancialAssistant) apiName نیست. اول خارج از برنامه تست کنید: scripts/test-mashhad-sso-loginkey.ps1 روی سرور؛ بعد &apiName=<نام کاربری پورتال> در همین URL."
                    : "هیچ فرمولی قبول نشد — apiName/ClientId/SecretKey را با پورتال تطبیق دهید یا scripts/test-mashhad-sso-loginkey.ps1 را روی سروری که به login.mashhad.ir دسترسی دارد اجرا کنید.",
        results
    });
}).AllowAnonymous();

app.MapGet("/auth/callback", async (HttpContext http, CancellationToken ct) =>
{
    try
    {
        return await TryCompleteSsoCallbackAsync(http, ct)
            ?? Results.Redirect("/login.html?error=" + Uri.EscapeDataString("پارامترهای بازگشت SSO ناقص است"));
    }
    catch (SqlException ex)
    {
        return AuthDatabaseError(ex);
    }
}).AllowAnonymous();

app.MapPost("/api/auth/login", async (LoginRequest? req, AppAuthService auth, ShimasAuthService shimas, HttpContext http, CancellationToken ct) =>
{
    if (req == null || string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { error = "نام کاربری و رمز عبور الزامی است" });

    try
    {
        var user = await auth.ValidateCredentialsAsync(req.Username, req.Password, ct);
        if (user == null)
            return Results.Json(new { error = "نام کاربری یا رمز عبور اشتباه است" }, statusCode: 401);

        if (!shimas.Options.LocalLoginAvailableForHost(http.Request.Host.Host))
        {
            if (!shimas.Options.AllowAdminLocalLoginOnPublicHost || !user.IsAdmin)
                return Results.Json(new { error = "ورود محلی غیرفعال است — از ورود سازمانی استفاده کنید" }, statusCode: 403);
        }

        var principal = AppAuthService.BuildPrincipal(user);
        await http.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            auth.CreateAuthProperties(persistent: true));
        return Results.Ok(await auth.ToSessionAsync(user, ct));
    }
    catch (SqlException ex)
    {
        return AuthDatabaseError(ex);
    }
}).AllowAnonymous();

app.MapPost("/api/auth/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok(new { ok = true });
}).RequireAuthorization(authenticated);

app.MapGet("/api/auth/me", async (HttpContext http, AppAuthService auth, CancellationToken ct) =>
{
    try
    {
        var session = await auth.GetSessionAsync(http.User, ct);
        return session == null
            ? Results.Json(new { error = "نشست منقضی شده" }, statusCode: 401)
            : Results.Ok(session);
    }
    catch (SqlException ex)
    {
        return AuthDatabaseError(ex);
    }
}).RequireAuthorization(authenticated);

app.MapGet("/api/admin/users", async (AppUserRepository users, AppPermissionService perms, HttpContext http, CancellationToken ct) =>
{
    var denied = await DenyUnlessManageUsers(http, perms, ct);
    if (denied != null) return denied;
    return Results.Ok(new { items = await users.ListUsersWithGroupsAsync(ct) });
}).RequireAuthorization(authenticated);

app.MapPost("/api/admin/users", async (CreateAppUserRequest? req, AppUserRepository users, AppPermissionService perms, HttpContext http, CancellationToken ct) =>
{
    var denied = await DenyUnlessManageUsers(http, perms, ct);
    if (denied != null) return denied;
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
                Domain = created.Domain,
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
}).RequireAuthorization(authenticated);

app.MapPut("/api/admin/users/{id:guid}", async (
    Guid id,
    UpdateAppUserRequest? req,
    AppUserRepository users,
    AppPermissionService perms,
    HttpContext http,
    CancellationToken ct) =>
{
    var denied = await DenyUnlessManageUsers(http, perms, ct);
    if (denied != null) return denied;
    if (req == null)
        return Results.BadRequest(new { error = "درخواست خالی است" });
    try
    {
        var updated = await users.UpdateUserAsync(id, req, ct);
        return Results.Ok(new { user = updated });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).RequireAuthorization(authenticated);

app.MapPost("/api/admin/users/{id:guid}/reset-password", async (
    Guid id,
    ResetAppUserPasswordRequest? req,
    AppUserRepository users,
    AppPermissionService perms,
    HttpContext http,
    CancellationToken ct) =>
{
    var denied = await DenyUnlessManageUsers(http, perms, ct);
    if (denied != null) return denied;
    if (req == null || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { error = "رمز عبور جدید الزامی است" });
    try
    {
        await users.ResetPasswordAsync(id, req.Password, ct);
        return Results.Ok(new { ok = true });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).RequireAuthorization(authenticated);

app.MapGet("/api/admin/groups", async (AppUserRepository users, AppPermissionService perms, HttpContext http, CancellationToken ct) =>
{
    var denied = await DenyUnlessManageUsers(http, perms, ct);
    if (denied != null) return denied;
    return Results.Ok(new { items = await users.ListGroupsAsync(ct) });
}).RequireAuthorization(authenticated);

app.MapPost("/api/admin/groups", async (
    CreateAppUserGroupRequest? req,
    AppUserRepository users,
    AppPermissionService perms,
    HttpContext http,
    CancellationToken ct) =>
{
    var denied = await DenyUnlessManageUsers(http, perms, ct);
    if (denied != null) return denied;
    if (req == null)
        return Results.BadRequest(new { error = "درخواست خالی است" });
    try
    {
        var group = await users.CreateGroupAsync(req, ct);
        return Results.Ok(new { group });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).RequireAuthorization(authenticated);

app.MapPut("/api/admin/groups/{id:guid}", async (
    Guid id,
    UpdateAppUserGroupRequest? req,
    AppUserRepository users,
    AppPermissionService perms,
    HttpContext http,
    CancellationToken ct) =>
{
    var denied = await DenyUnlessManageUsers(http, perms, ct);
    if (denied != null) return denied;
    if (req == null)
        return Results.BadRequest(new { error = "درخواست خالی است" });
    try
    {
        var group = await users.UpdateGroupAsync(id, req, ct);
        return Results.Ok(new { group });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).RequireAuthorization(authenticated);

app.MapGet("/api/config", (IConfiguration config, HttpContext http, ShimasAuthService shimas) => new
{
    releaseVersion = ReleaseInfo.Number,
    releaseLabel = ReleaseInfo.Label,
    releaseDisplayName = ReleaseInfo.DisplayName,
    contentRoot = app.Environment.ContentRootPath,
    appSettingsFile = Path.Combine(app.Environment.ContentRootPath, "appsettings.json"),
    dryRun = config.GetValue<bool>("Rayvarz:DryRun"),
    serviceUrl = RayvarzUrlNormalizer.Normalize(config, config["Rayvarz:ServiceUrl"]),
    serviceUrlMsb = RayvarzUrlNormalizer.Normalize(config, config["Rayvarz:ServiceUrlMsb"] ?? ""),
    wsAddressingTo = RayvarzUrlNormalizer.Normalize(config, config["Rayvarz:WsAddressingTo"] ?? config["Rayvarz:ServiceUrl"]),
    useHttp = config.GetValue("Rayvarz:UseHttp", true),
    soapEnvelopeStyle = RayvarzSoapHttp.ResolveEnvelopeStyle(config),
    soapVersion = RayvarzSoapHttp.SoapVersionLabel(RayvarzSoapHttp.ResolveSoapVersion(config)),
    refRowDocNoInDetail = config["Rayvarz:RefRowDocNoInDetail"] ?? "zero",
    allowInvalidSsl = config.GetValue<bool>("Rayvarz:AllowInvalidSsl"),
    sourceSystemId = string.IsNullOrWhiteSpace(config["Rayvarz:SourceSystemId"])
        ? SoapBuilder.DefaultSourceSystemId
        : config["Rayvarz:SourceSystemId"]!.Trim(),
    payloadSource = config["Rayvarz:PayloadSource"] ?? "LegacyCSharp",
    ruleEngineNidMember = config.GetValue("RuleEngine:NidMemberRayvarzRun", 1388),
    uiVersion = "5",
    auth = new
    {
        enabled = true,
        isAdmin = AppAuthService.IsAdmin(http.User),
        shimas = shimas.GetStatus()
    },
    features = new { rayvarzPing = true, rayvarzPostTest = true, rayvarzPostMinimalSave = true, tahator = true, unsentBatch = true, ruleEngineBridgeStub = true, auth = true, installmentCheck = true, ficheDateChange = true, bankInquiryConfirm = true },
    tahator = new
    {
        dryRun = config.GetValue<bool?>("Tahator:DryRun") ?? config.GetValue<bool>("Rayvarz:DryRun"),
        pollIntervalMs = config.GetValue("Tahator:PollIntervalMs", 2000),
        pollTimeoutSeconds = config.GetValue("Tahator:PollTimeoutSeconds", 60),
    },
    accountingDoc = new
    {
        dryRun = config.GetValue<bool?>("AccountingDoc:DryRun") ?? config.GetValue<bool>("Rayvarz:DryRun"),
        pollTimeoutSeconds = config.GetValue("AccountingDoc:PollTimeoutSeconds",
            config.GetValue("Tahator:PollTimeoutSeconds", 60)),
        pollIntervalMs = config.GetValue("AccountingDoc:PollIntervalMs",
            config.GetValue("Tahator:PollIntervalMs", 2000)),
    },
    installment = new
    {
        dryRun = config.GetValue<bool?>("Installment:DryRun") ?? config.GetValue("Rayvarz:DryRun", true),
        connection = "ConnectionStrings:Sara",
        database = "Sara8M03",
        table = "dbo.Installment_List"
    },
    ficheDateChange = new
    {
        dryRun = config.GetValue<bool?>("FicheDateChange:DryRun") ?? config.GetValue("Rayvarz:DryRun", true),
        connection = "ConnectionStrings:Sara",
        database = "Sara8M03",
        table = "dbo.Income_Fiche",
        defaultStatus = 1,
        statusLabels = FicheDateChangeHelper.FicheStatusLabels
    },
    bankInquiryConfirm = new
    {
        dryRun = config.GetValue<bool?>("BankInquiryConfirm:DryRun") ?? config.GetValue("Rayvarz:DryRun", true),
        serviceConfigured = !string.IsNullOrWhiteSpace(config["BankInquiryConfirm:ServiceUrl"])
            && !string.IsNullOrWhiteSpace(config["BankInquiryConfirm:FicheLookupServiceUrl"])
            && !string.IsNullOrWhiteSpace(config["BankInquiryConfirm:UserName"])
            && !string.IsNullOrWhiteSpace(config["BankInquiryConfirm:Password"]),
        ficheLookupServiceUrl = config["BankInquiryConfirm:FicheLookupServiceUrl"],
        onlineBankServiceUrl = config["BankInquiryConfirm:ServiceUrl"],
        bankCode = config.GetValue("BankInquiryConfirm:BankCode", 18),
        allowInvalidSsl = config.GetValue<bool?>("BankInquiryConfirm:AllowInvalidSsl")
            ?? config.GetValue<bool>("Rayvarz:AllowInvalidSsl"),
        useSystemProxy = config.GetValue<bool?>("BankInquiryConfirm:UseSystemProxy")
            ?? config.GetValue<bool>("Rayvarz:UseSystemProxy"),
        connection = "ConnectionStrings:Sara",
        database = "Sara8M03",
        table = "dbo.Income_Fiche",
        ficheStatus = BankInquiryConfirmHelper.ConfirmedFicheStatus,
        incomePaymentType = BankInquiryConfirmHelper.ConfirmedIncomePaymentType
    },
    ruleEngine = new
    {
        payloadSource = config["Rayvarz:PayloadSource"] ?? "LegacyCSharp",
        useLocalBridgeStub = config.GetValue("RuleEngine:UseLocalBridgeStub", false),
        saraBridgeUrl = config["RuleEngine:SaraBridgeUrl"],
        nidMember = config.GetValue("RuleEngine:NidMemberRayvarzRun", 1388),
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
        new { id = 212, name = "منطقه 12", fund = 200212004 },
        new { id = 218, name = "منطقه ثامن", fund = 200218011 },
        new { id = 102, name = "شعبه مرکز", fund = 0 }
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

app.MapPost("/api/unsent/search", async (
    UnsentFicheSearchRequest? req,
    UnsentFicheService unsent,
    AppPermissionService perms,
    HttpContext http,
    CancellationToken ct) =>
{
    var denied = await DenyUnlessUnsent(http, perms, ct);
    if (denied != null) return denied;
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
}).RequireAuthorization(authenticated);

app.MapPost("/api/unsent/plan-batch", async (
    UnsentBatchSendRequest? req,
    UnsentFicheService unsent,
    AppPermissionService perms,
    HttpContext http,
    CancellationToken ct) =>
{
    var denied = await DenyUnlessUnsent(http, perms, ct);
    if (denied != null) return denied;
    if (req == null || ((req.Targets == null || req.Targets.Count == 0) && (req.FicheNos == null || req.FicheNos.Count == 0)))
        return Results.BadRequest(new { error = "حداقل یک فیش انتخاب کنید" });
    try
    {
        return Results.Ok(await unsent.PlanBatchAsync(req, http.User, ct));
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 500);
    }
}).RequireAuthorization(authenticated);

app.MapPost("/api/unsent/send-batch", async (
    UnsentBatchSendRequest? req,
    UnsentFicheService unsent,
    AppPermissionService perms,
    HttpContext http,
    CancellationToken ct) =>
{
    var denied = await DenyUnlessUnsent(http, perms, ct);
    if (denied != null) return denied;
    if (req == null || ((req.Targets == null || req.Targets.Count == 0) && (req.FicheNos == null || req.FicheNos.Count == 0)))
        return Results.BadRequest(new { error = "حداقل یک فیش انتخاب کنید" });
    try
    {
        return Results.Ok(await unsent.SendBatchAsync(req, http.User, ct));
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 500);
    }
}).RequireAuthorization(authenticated);

app.MapPost("/api/unsent/lookup-by-bill-pay", async (
    UnsentBillPayLookupRequest? req,
    UnsentFicheService unsent,
    AppPermissionService perms,
    HttpContext http,
    CancellationToken ct) =>
{
    var denied = await DenyUnlessUnsent(http, perms, ct);
    if (denied != null) return denied;
    if (req == null)
        return Results.BadRequest(new { error = "درخواست خالی است" });
    var validation = UnsentBillPayLookupHelper.ValidateRequest(req);
    if (validation != null)
        return Results.BadRequest(new { error = validation });
    try
    {
        var result = await unsent.LookupByBillPayAsync(req, ct);
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

app.MapPost("/api/installment/preview", async (
    InstallmentCheckRequest? req,
    InstallmentCheckService installment,
    AppPermissionService perms,
    HttpContext http,
    CancellationToken ct) =>
{
    var denied = await DenyUnlessInstallment(http, perms, ct);
    if (denied != null) return denied;
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
    AppPermissionService perms,
    HttpContext http,
    CancellationToken ct) =>
{
    var denied = await DenyUnlessInstallment(http, perms, ct);
    if (denied != null) return denied;
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

app.MapGet("/api/fiche-date/account-groups", async (
    string? q,
    int? limit,
    FicheDateChangeService ficheDate,
    AppPermissionService perms,
    HttpContext http,
    CancellationToken ct) =>
{
    var denied = await DenyUnlessFicheDateChange(http, perms, ct);
    if (denied != null) return denied;
    try
    {
        var titles = await ficheDate.ListAccountGroupTitlesAsync(q, limit ?? 20, ct);
        return Results.Ok(new { titles });
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

app.MapPost("/api/fiche-date/search", async (
    FicheDateChangeSearchRequest? req,
    FicheDateChangeService ficheDate,
    AppPermissionService perms,
    HttpContext http,
    CancellationToken ct) =>
{
    var denied = await DenyUnlessFicheDateChange(http, perms, ct);
    if (denied != null) return denied;
    if (req == null)
        return Results.BadRequest(new { error = "درخواست خالی است" });
    try
    {
        var result = await ficheDate.SearchAsync(req, ct);
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

app.MapPost("/api/fiche-date/update", async (
    FicheDateChangeUpdateRequest? req,
    FicheDateChangeService ficheDate,
    AppPermissionService perms,
    HttpContext http,
    CancellationToken ct) =>
{
    var denied = await DenyUnlessFicheDateChange(http, perms, ct);
    if (denied != null) return denied;
    if (req == null)
        return Results.BadRequest(new { error = "درخواست خالی است" });
    req.PerformedByUser = AppAuthService.ResolveCommentUserName(http.User);
    try
    {
        var result = await ficheDate.UpdateAsync(req, ct);
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

app.MapPost("/api/bank-inquiry/search", async (
    BankInquirySearchRequest? req,
    BankInquiryConfirmService bankInquiry,
    AppPermissionService perms,
    HttpContext http,
    CancellationToken ct) =>
{
    var denied = await DenyUnlessBankInquiryConfirm(http, perms, ct);
    if (denied != null) return denied;
    if (req == null)
        return Results.BadRequest(new { error = "درخواست خالی است" });
    try
    {
        var result = await bankInquiry.SearchAsync(req, http.User, ct);
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

app.MapPost("/api/bank-inquiry/confirm", async (
    BankInquiryConfirmRequest? req,
    BankInquiryConfirmService bankInquiry,
    AppPermissionService perms,
    HttpContext http,
    CancellationToken ct) =>
{
    var denied = await DenyUnlessBankInquiryConfirm(http, perms, ct);
    if (denied != null) return denied;
    if (req == null)
        return Results.BadRequest(new { error = "درخواست خالی است" });
    req.PerformedByUser = AppAuthService.ResolveCommentUserName(http.User);
    try
    {
        var result = await bankInquiry.ConfirmAsync(req, http.User, ct);
        if (!string.IsNullOrWhiteSpace(result.Error))
            return Results.BadRequest(new { error = result.Error, result });
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

app.MapPost("/api/bank-inquiry/test", async (
    BankInquiryTestRequest? req,
    BankInquiryApiClient bankInquiry,
    AppPermissionService perms,
    HttpContext http,
    CancellationToken ct) =>
{
    var denied = await DenyUnlessBankInquiryConfirm(http, perms, ct);
    if (denied != null) return denied;
    if (req == null || string.IsNullOrWhiteSpace(req.BillId) || string.IsNullOrWhiteSpace(req.PaymentId))
        return Results.BadRequest(new { error = "billId و paymentId الزامی است" });

    try
    {
        var result = await bankInquiry.InquireAsync(req.BillId.Trim(), req.PaymentId.Trim(), ct);
        return Results.Ok(new
        {
            configured = bankInquiry.IsConfigured,
            isPaid = result.IsPaid,
            serviceError = result.ServiceError,
            message = result.Message,
            paymentDate = result.PaymentDate,
            inquirySource = result.InquirySource,
            rawResponse = result.RawResponse
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 500);
    }
}).RequireAuthorization(authenticated);

app.MapPost("/api/bank-inquiry/diagnose", async (
    BankInquiryTestRequest? req,
    BankInquiryApiClient bankInquiry,
    AppPermissionService perms,
    HttpContext http,
    CancellationToken ct) =>
{
    var denied = await DenyUnlessBankInquiryConfirm(http, perms, ct);
    if (denied != null) return denied;
    if (req == null || string.IsNullOrWhiteSpace(req.BillId) || string.IsNullOrWhiteSpace(req.PaymentId))
        return Results.BadRequest(new { error = "شناسه قبض و شناسه پرداخت الزامی است" });

    try
    {
        return Results.Ok(await bankInquiry.DiagnoseAsync(req.BillId, req.PaymentId, ct));
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 500);
    }
}).RequireAuthorization(authenticated);

static IResult AuthDatabaseError(SqlException ex) =>
    Results.Json(new
    {
        error = "خطا در اتصال به پایگاه کاربران (AppAuth). ConnectionStrings:AppAuth را بررسی کنید.",
        detail = ex.Message
    }, statusCode: 503);

static async Task<IResult?> DenyUnlessBankInquiryConfirm(HttpContext http, AppPermissionService perms, CancellationToken ct)
{
    var p = await perms.ResolveForPrincipalAsync(http.User, ct);
    if (!AppPermissionService.Allows(p, x => x.CanAccessBankInquiryConfirm))
        return Results.Json(new { error = "دسترسی به خدمات الکترونیک مجاز نیست" }, statusCode: 403);
    return null;
}

static async Task<IResult?> DenyUnlessManageUsers(HttpContext http, AppPermissionService perms, CancellationToken ct)
{
    var p = await perms.ResolveForPrincipalAsync(http.User, ct);
    if (!AppPermissionService.Allows(p, x => x.CanManageUsers))
        return Results.Json(new { error = "دسترسی به مدیریت کاربران مجاز نیست" }, statusCode: 403);
    return null;
}

static async Task<IResult?> DenyUnlessUnsent(HttpContext http, AppPermissionService perms, CancellationToken ct)
{
    var p = await perms.ResolveForPrincipalAsync(http.User, ct);
    if (!AppPermissionService.Allows(p, x => x.CanAccessUnsentFiches))
        return Results.Json(new { error = "دسترسی به فیش‌های جمعی مجاز نیست" }, statusCode: 403);
    return null;
}

static async Task<IResult?> DenyUnlessInstallment(HttpContext http, AppPermissionService perms, CancellationToken ct)
{
    var p = await perms.ResolveForPrincipalAsync(http.User, ct);
    if (!AppPermissionService.Allows(p, x => x.CanAccessInstallment))
        return Results.Json(new { error = "دسترسی به چک خزانه مجاز نیست" }, statusCode: 403);
    return null;
}

static async Task<IResult?> DenyUnlessFicheDateChange(HttpContext http, AppPermissionService perms, CancellationToken ct)
{
    var p = await perms.ResolveForPrincipalAsync(http.User, ct);
    if (!AppPermissionService.Allows(p, x => x.CanAccessFicheDateChange))
        return Results.Json(new { error = "دسترسی به تغییر تاریخ فیش مجاز نیست" }, statusCode: 403);
    return null;
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
