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
    public void Unpublished_login_page_has_no_national_id_hint()
    {
        var html = File.ReadAllText(WebFile("wwwroot", "login.html"));
        Assert.Contains("کد ملی", html);
        Assert.Contains("رمز عبور", html);
        Assert.DoesNotContain("ورود با کد ملی و رمز عبور", html);
        Assert.DoesNotContain("با کد ملی وارد شود", html);
        Assert.DoesNotContain("نام کاربری admin", html);
        Assert.DoesNotContain("ادمین می‌تواند", html);
    }

    [Fact]
    public void Unpublished_single_tab_hides_financial_assistant_and_keeps_persian_rows()
    {
        var html = File.ReadAllText(WebFile("wwwroot", "index.html"));
        Assert.DoesNotContain("sourceIdDisplay", html);
        Assert.DoesNotContain("FinancialAssistant", html);
        Assert.Contains("id=\"fund\"", html);
        Assert.Contains("خلاصه ارسال", html);
        Assert.Contains("field-label\">منبع", html);
        Assert.DoesNotContain("field-label\">صندوق", html);
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
        Assert.Contains("function fillFundSelect(", js);
        Assert.Contains("stat-label\">منبع", js);
        Assert.Contains("field: 'منبع'", js);
        Assert.DoesNotContain("field: 'صندوق'", js);
        Assert.Contains("function showTahatorSendResult(", js);
        Assert.Contains("showAppInfo(`در حال ارسال فیش", js);
        Assert.DoesNotContain("function fillSourceIdDisplay(", js);
        Assert.DoesNotContain("جفت مرجع", js.Split("function showTahatorSendResult")[1].Split("function formatTahatorCheck")[0]);
        Assert.DoesNotContain("جزئیات فیش", js.Split("function showTahatorSendResult")[1].Split("function formatTahatorCheck")[0]);
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
        Assert.Contains("فیش دیگر به‌صورت خودکار ارسال نمی‌شود", src);
        Assert.DoesNotContain("هر دو فیش با همان NidIncome لازم است", src);
        Assert.DoesNotContain("فقط فیش دیگر ارسال می‌شود", src);
        Assert.DoesNotContain("force=true برای ارسال اجباری", src);
        Assert.Contains("این فیش قبلاً در رایورز ثبت شده است.", src);
    }

    [Fact]
    public void Bulk_tahator_does_not_auto_send_or_skip_pair_partner()
    {
        var path = WebFile("Services", "UnsentFicheService.cs");
        var src = File.ReadAllText(path);
        Assert.DoesNotContain("processedTahatorPairs", src);
        Assert.DoesNotContain("جفت تهاتر قبلاً در همین دسته پردازش می‌شود", src);
        Assert.DoesNotContain("جفت ۱۵۷+۱۵۸ کامل نیست", src);
        Assert.Contains("فقط همین فیش", src);
    }

    [Fact]
    public void Operator_branch_fund_map_matches_district_table()
    {
        var program = File.ReadAllText(WebFile("Program.cs"));
        Assert.Contains("id = 201, name = \"منطقه 1\", fund = 200201012", program);
        Assert.Contains("id = 209, name = \"منطقه 9\", fund = 200209004", program);
        Assert.Contains("id = 212, name = \"منطقه 12\", fund = 200212004", program);
        Assert.Contains("id = 218, name = \"منطقه ثامن\", fund = 200218011", program);
        Assert.DoesNotContain("fund = 200209008", program);
        Assert.DoesNotContain("fund = 212210016", program);

        var json = File.ReadAllText(WebFile("appsettings.json"));
        Assert.Contains("\"209\": 200209004", json);
        Assert.Contains("\"212\": 200212004", json);
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
