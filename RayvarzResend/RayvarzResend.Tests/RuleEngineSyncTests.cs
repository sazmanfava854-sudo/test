using RayvarzResend.Web.RuleEngine.Sync;
using Xunit;

namespace RayvarzResend.Tests;

public class RuleEngineSyncTests
{
    [Fact]
    public void XmlHasher_IsDeterministic_ForSameBody()
    {
        const string xml = "<root><a>1</a></root>";
        var h1 = XmlBodyCanonicalHash.Compute(xml);
        var h2 = XmlBodyCanonicalHash.Compute(xml);
        Assert.Equal(h1, h2);
        Assert.Equal(64, h1.Length);
    }

    [Fact]
    public void HistoryChecker_DetectsNewHistoryRow()
    {
        var checker = new RuleHistoryChecker();
        var state = new RuleSyncStateRow { NidMember = 1388, LastSeenNidHistory = 100 };
        var memberHash = XmlBodyCanonicalHash.Compute("<m/>");
        var history = new MemberHistoryHead
        {
            NidHistory = 101,
            NidMember = 1388,
            XmlBody = "<m2/>",
            ModifyUtc = DateTime.UtcNow
        };

        var a = checker.Assess(state, memberHash, history);
        Assert.True(a.ShouldOpenCandidate);
        Assert.True(a.HistoryAdvanced);
        Assert.Equal(101, a.LatestNidHistory);
    }

    [Fact]
    public void HistoryChecker_NoChange_WhenHistoryUnchanged()
    {
        var checker = new RuleHistoryChecker();
        const string xml = "<same/>";
        var hash = XmlBodyCanonicalHash.Compute(xml);
        var state = new RuleSyncStateRow
        {
            NidMember = 1388,
            LastSeenNidHistory = 101,
            LastStableXmlHash = hash
        };
        var history = new MemberHistoryHead
        {
            NidHistory = 101,
            NidMember = 1388,
            XmlBody = xml,
            ModifyUtc = DateTime.UtcNow
        };

        var a = checker.Assess(state, hash, history);
        Assert.False(a.ShouldOpenCandidate);
    }
}
