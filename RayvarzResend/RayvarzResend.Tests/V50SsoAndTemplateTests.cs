using System.IO.Compression;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class V50SsoAndTemplateTests
{
    private static string WebFile(params string[] parts) =>
        Path.GetFullPath(Path.Combine(
            new[] { AppContext.BaseDirectory, "..", "..", "..", "..", "RayvarzResend.Web" }.Concat(parts).ToArray()));

    [Fact]
    public void Fixed_login_state_is_default_like_v47()
    {
        Assert.True(new ShimasAuthOptions().UseFixedLoginState);
        var json = File.ReadAllText(WebFile("appsettings.json"));
        Assert.Contains("\"UseFixedLoginState\": true", json);
    }

    [Fact]
    public void Sso_callback_applies_returned_domain_before_user_lookup()
    {
        var program = File.ReadAllText(WebFile("Program.cs"));
        var start = program.IndexOf("async Task<IResult?> TryCompleteSsoCallbackAsync", StringComparison.Ordinal);
        Assert.True(start >= 0);
        var body = program[start..];
        var domainIdx = body.IndexOf("validation.Profile.Domain = callback.Domain;", StringComparison.Ordinal);
        var lookupIdx = body.IndexOf("ResolveOrCreateUserAsync(validation.Profile", StringComparison.Ordinal);
        Assert.True(domainIdx >= 0, "callback domain must be applied to the SSO profile");
        Assert.True(domainIdx < lookupIdx);
    }

    [Fact]
    public void Unsent_excel_template_has_combined_bill_pay_column()
    {
        using var zip = ZipFile.OpenRead(WebFile("wwwroot", "templates", "unsent-bill-pay-template.xlsx"));
        var text = string.Concat(zip.Entries
            .Where(e => e.FullName.StartsWith("xl/", StringComparison.Ordinal) && e.FullName.EndsWith(".xml", StringComparison.Ordinal))
            .Select(e =>
            {
                using var reader = new StreamReader(e.Open());
                return reader.ReadToEnd();
            }));
        Assert.Contains("شناسه قبض و پرداخت", text);
        Assert.Contains("شماره فیش", text);
        Assert.DoesNotContain(">شناسه قبض<", text);
        Assert.DoesNotContain(">شناسه پرداخت<", text);
    }
}
