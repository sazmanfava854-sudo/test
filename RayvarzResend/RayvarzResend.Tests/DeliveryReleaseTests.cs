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
}
