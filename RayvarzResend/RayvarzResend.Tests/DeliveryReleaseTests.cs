using RayvarzResend.Web;
using Xunit;

namespace RayvarzResend.Tests;

public class DeliveryReleaseTests
{
    [Fact]
    public void ReleaseInfo_is_v25_noskhe_akhar()
    {
        Assert.Equal(25, ReleaseInfo.Number);
        Assert.Equal("rayvarzresend-noskhe-akhar", ReleaseInfo.Tag);
        Assert.Equal("نسخه آخر", ReleaseInfo.DisplayName);
    }

    [Fact]
    public void VERSION_file_matches_ReleaseInfo()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "VERSION"));
        Assert.True(File.Exists(path), path);
        Assert.Equal(ReleaseInfo.Label, File.ReadAllText(path).Trim());
    }

    [Fact]
    public void DELIVERY_v25_doc_lists_noskhe_akhar()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "DELIVERY-v25.md"));
        Assert.True(File.Exists(path), path);
        var doc = File.ReadAllText(path);
        Assert.Contains("نسخه آخر", doc);
        Assert.Contains("Accounting_Doc", doc);
        Assert.Contains("411", doc);
    }

    [Fact]
    public void Bug14_ui_test_script_is_in_repo()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "scripts", "Bug14ValSummaryTests.mjs"));
        Assert.True(File.Exists(path), path);
        var script = File.ReadAllText(path);
        Assert.Contains("formatValMappingDetail", script);
        Assert.Contains("✓ جمع = Payable", script);
    }

    [Fact]
    public void appsettings_has_dry_run_default_false()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..",
            "RayvarzResend.Web", "appsettings.json"));
        Assert.True(File.Exists(path), path);
        var json = File.ReadAllText(path);
        Assert.Contains("\"Rayvarz\"", json);
        Assert.Contains("\"Installment\"", json);
        Assert.Contains("\"FicheDateChange\"", json);
        Assert.Contains("\"BankInquiryConfirm\"", json);
        Assert.Contains("\"ServiceUrl\"", json);
        Assert.Contains("epay_EstelamOnLineBank", json);
        Assert.DoesNotContain("\"DryRun\": true", json);
        Assert.Contains("\"DryRun\": false", json);
        Assert.Contains("\"AccountingDoc\"", json);
        Assert.Contains("\"Tahator\"", json);
        Assert.Contains("FinancialAssistant", json);
        Assert.Contains("https://city.mashhad.ir:5065", json);
    }

    [Fact]
    public void Program_cs_registers_RayvarzPayloadBuilder_for_preview_endpoint()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..",
            "RayvarzResend.Web", "Program.cs"));
        Assert.True(File.Exists(path), path);
        var program = File.ReadAllText(path);
        Assert.Contains("AddSingleton<RayvarzPayloadBuilder>", program);
        Assert.Contains("[FromServices] RayvarzPayloadBuilder", program);
    }

    [Fact]
    public void Unpublished_login_page_uses_national_id_copy_without_admin_hint()
    {
        var path = WebFile("wwwroot", "login.html");
        var html = File.ReadAllText(path);
        Assert.Contains("ورود با کد ملی و رمز عبور", html);
        Assert.Contains("کد ملی", html);
        Assert.Contains("رمز عبور", html);
        Assert.DoesNotContain("نام کاربری admin", html);
        Assert.DoesNotContain("ادمین می‌تواند", html);
    }

    [Fact]
    public void Unpublished_single_tab_uses_persian_summary_and_financial_assistant()
    {
        var html = File.ReadAllText(WebFile("wwwroot", "index.html"));
        Assert.Contains("id=\"sourceIdDisplay\"", html);
        Assert.Contains("value=\"FinancialAssistant\"", html);
        Assert.Contains("خلاصه ارسال", html);
        Assert.Contains("<th>کد درآمد</th>", html);
        Assert.Contains("<th>شرح</th>", html);
        Assert.Contains("<th>مبلغ (ریال)</th>", html);
        Assert.DoesNotContain("btnPreview", html);
        Assert.DoesNotContain("پیش نمایش xml", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("id=\"sendResultCard\"", html);
        Assert.DoesNotContain("id=\"resultBox\"", html);
    }

    [Fact]
    public void Unpublished_single_send_has_no_browser_confirm()
    {
        var js = File.ReadAllText(WebFile("wwwroot", "js", "app.js"));
        Assert.Contains("function showSendResult(", js);
        Assert.Contains("function fillSourceIdDisplay(", js);
        Assert.Contains("function showTahatorSendResult(", js);
        Assert.Contains("showAppInfo(`در حال ارسال فیش", js);
        Assert.DoesNotContain("if (!confirm(", js.Split("bindClick('btnSend'")[1].Split("bindClick('btnUnsentSearch'")[0]);
        Assert.DoesNotContain("if (!confirm(", js.Split("bindClick('btnUnsentSend'")[1].Split("bindClick('btnInstallmentPreview'")[0]);
    }

    [Fact]
    public void Unpublished_tahator_sends_only_requested_fiche()
    {
        var path = WebFile("Services", "TahatorResendService.cs");
        var src = File.ReadAllText(path);
        Assert.Contains("ResolveRequestedTahatorFiche", src);
        Assert.Contains("ارسال فقط فیش درخواستی", src);
        Assert.Contains("var ordered = new[] { targetFiche };", src);
        Assert.Contains("AccountingDocWriter", src);
    }

    private static string WebFile(params string[] parts)
    {
        var path = Path.GetFullPath(Path.Combine(
            new[] { AppContext.BaseDirectory, "..", "..", "..", "..", "RayvarzResend.Web" }
                .Concat(parts)
                .ToArray()));
        Assert.True(File.Exists(path), path);
        return path;
    }
}
