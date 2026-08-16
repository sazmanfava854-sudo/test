using RayvarzResend.Web;
using Xunit;

namespace RayvarzResend.Tests;

public class DeliveryReleaseTests
{
    [Fact]
    public void ReleaseInfo_is_v22_final_delivery()
    {
        Assert.Equal(22, ReleaseInfo.Number);
        Assert.Equal("rayvarz-resend-v22", ReleaseInfo.Tag);
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
    public void DELIVERY_v22_doc_lists_noncritical_fixes()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "DELIVERY-v22.md"));
        Assert.True(File.Exists(path), path);
        var doc = File.ReadAllText(path);
        Assert.Contains("نسخه ۲۲", doc);
        Assert.Contains("canSend", doc);
        Assert.Contains("178 تست", doc);
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
}
