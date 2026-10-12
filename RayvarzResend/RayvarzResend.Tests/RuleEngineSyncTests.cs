using RayvarzResend.Web.RuleEngine.Sync;
using Xunit;

namespace RayvarzResend.Tests;

public class RuleEngineSyncTests
{
    [Fact]
    public void XmlBodyCanonicalHash_is_stable_for_whitespace()
    {
        var a = "<Root>  test\n</Root>";
        var b = "<Root> test </Root>";
        Assert.Equal(XmlBodyCanonicalHash.Compute(a), XmlBodyCanonicalHash.Compute(b));
    }

    [Fact]
    public void XmlBodyCanonicalHash_changes_when_content_changes()
    {
        var h1 = XmlBodyCanonicalHash.Compute("<Root>a</Root>");
        var h2 = XmlBodyCanonicalHash.Compute("<Root>b</Root>");
        Assert.NotEqual(h1, h2);
    }

    [Fact]
    public void RuleHistoryChecker_detects_hash_and_history_change()
    {
        var checker = new RuleHistoryChecker();
        var state = new RuleSyncStateRow
        {
            NidMember = 1388,
            ActiveXmlBodyHash = "abc",
            LastMemberXmlBodyHash = "abc",
            LastSeenNidHistory = 10
        };
        var assessment = checker.Assess(state, "def", new MemberHistoryHead { NidHistory = 11, NidMember = 1388 });
        Assert.True(assessment.MemberHashChanged);
        Assert.True(assessment.HistoryAdvanced);
        Assert.True(assessment.ShouldOpenCandidate);
    }

    [Fact]
    public void RuleHistoryChecker_no_candidate_when_unchanged()
    {
        var checker = new RuleHistoryChecker();
        var state = new RuleSyncStateRow
        {
            ActiveXmlBodyHash = "same",
            LastMemberXmlBodyHash = "same",
            LastSeenNidHistory = 99
        };
        var assessment = checker.Assess(state, "same", new MemberHistoryHead { NidHistory = 99 });
        Assert.False(assessment.MemberHashChanged);
        Assert.False(assessment.HistoryAdvanced);
        Assert.False(assessment.ShouldOpenCandidate);
    }
}
