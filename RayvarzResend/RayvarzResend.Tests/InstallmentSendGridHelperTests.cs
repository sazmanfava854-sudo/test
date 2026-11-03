using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class InstallmentSendGridHelperTests
{
    [Fact]
    public void BuildDocNumberWarnings_single_fiche_in_group_no_warning()
    {
        var items = new[]
        {
            Preview("809552", "1111111111111111", found: true, matches: true)
        };
        var selected = new HashSet<string> { "1111111111111111" };
        Assert.Empty(InstallmentSendGridHelper.BuildDocNumberWarnings(items, selected));
    }

    [Fact]
    public void BuildDocNumberWarnings_multi_fiche_one_selected_no_warning()
    {
        var items = new[]
        {
            Preview("809552", "1111111111111111", found: true, matches: true),
            Preview("809552", "2222222222222222", found: true, matches: true)
        };
        var selected = new HashSet<string> { "1111111111111111" };
        Assert.Empty(InstallmentSendGridHelper.BuildDocNumberWarnings(items, selected));
    }

    [Fact]
    public void BuildDocNumberWarnings_multi_fiche_two_selected_warns()
    {
        var items = new[]
        {
            Preview("809552", "1111111111111111", found: true, matches: true),
            Preview("809552", "2222222222222222", found: true, matches: true)
        };
        var selected = new HashSet<string> { "1111111111111111", "2222222222222222" };
        var warnings = InstallmentSendGridHelper.BuildDocNumberWarnings(items, selected);
        Assert.Single(warnings);
        Assert.Equal("809552", warnings[0].NoDocument);
        Assert.Equal(2, warnings[0].TotalInGrid);
        Assert.Equal(2, warnings[0].SelectedCount);
    }

    [Fact]
    public void FindDuplicateTrackingInGrid_detects_duplicate()
    {
        var items = new[]
        {
            Preview("1", "9907200614003461", found: true, matches: true),
            Preview("2", "9907200614003461", found: true, matches: true)
        };
        Assert.Equal("9907200614003461", InstallmentSendGridHelper.FindDuplicateTrackingInGrid(items));
    }

    [Fact]
    public void ParseSelectedTrackingNos_rejects_duplicate_selection()
    {
        var (keys, error) = InstallmentSendGridHelper.ParseSelectedTrackingNos([
            "9907200614003461",
            "9907200614003461"
        ]);
        Assert.Empty(keys);
        Assert.Equal(InstallmentSendGridHelper.DuplicateTrackingSelectionError, error);
    }

    [Fact]
    public void FormatDocWarningConfirmBody_joins_multiple_groups()
    {
        var body = InstallmentSendGridHelper.FormatDocWarningConfirmBody([
            new InstallmentSendGridHelper.DocSendWarning("100", 3, 2),
            new InstallmentSendGridHelper.DocSendWarning("200", 2, 2)
        ]);
        Assert.Contains("شماره سند 100", body);
        Assert.Contains("شماره سند 200", body);
        Assert.Contains("معمولاً فقط یک فیش", body);
    }

    private static InstallmentCheckPreviewItem Preview(
        string noDocument,
        string trackingNo,
        bool found,
        bool matches) =>
        new()
        {
            Found = found,
            DataMatches = matches,
            NoDocument = noDocument,
            TrackingNo = trackingNo
        };
}
