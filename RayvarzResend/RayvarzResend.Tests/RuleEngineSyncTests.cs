using RayvarzResend.Web.RuleEngine.Sync;
using Xunit;

namespace RayvarzResend.Tests;

public class RuleEngineSyncTests
{
    private const string ValidXml = """
        <ClsFunction>
          <NidClass>360</NidClass>
          <Name>Test</Name>
          <Body>&lt;DisplayName("محاسبه")&gt; Public Function Calc(ByVal x As Integer) As Integer</Body>
        </ClsFunction>
        """;

    private readonly RuleHistoryChecker _checker = new();

    [Fact]
    public void XmlHasher_IsDeterministic_AndIgnoresWhitespaceNoise()
    {
        var h1 = XmlBodyCanonicalHash.Compute("<root><a>1</a></root>");
        var h2 = XmlBodyCanonicalHash.Compute("  <root><a>1</a></root>\r\n");
        Assert.Equal(h1, h2);
        Assert.Equal(64, h1.Length);
    }

    [Fact]
    public void Decide_Baseline_WhenNoStableHash()
    {
        var state = new RuleSyncStateRow { LastStableXmlHash = "" };
        Assert.Equal(RuleSyncDecision.Baseline, _checker.Decide(state, "abc", null));
    }

    [Fact]
    public void Decide_Unchanged_WhenHashMatchesStable_IncludingCharPadding()
    {
        var hash = XmlBodyCanonicalHash.Compute("<m/>");
        var state = new RuleSyncStateRow { LastStableXmlHash = hash.ToUpperInvariant() + "  " };
        Assert.Equal(RuleSyncDecision.Unchanged, _checker.Decide(state, hash, null));
    }

    [Fact]
    public void Decide_Tracking_WhenOpenCandidateHasSameHash()
    {
        var state = new RuleSyncStateRow { LastStableXmlHash = "old" };
        var open = new RuleCandidateRow { CandidateId = 7, CanonicalXmlHash = "new" };
        Assert.Equal(RuleSyncDecision.TrackingOpenCandidate, _checker.Decide(state, "new", open));
    }

    [Fact]
    public void Decide_NewCandidate_WhenContentDiffersFromStableAndOpen()
    {
        var state = new RuleSyncStateRow { LastStableXmlHash = "old" };
        var open = new RuleCandidateRow { CandidateId = 7, CanonicalXmlHash = "mid" };
        Assert.Equal(RuleSyncDecision.NewCandidate, _checker.Decide(state, "newest", open));
        Assert.Equal(RuleSyncDecision.NewCandidate, _checker.Decide(state, "newest", null));
    }

    [Theory]
    [InlineData("Detected", true)]
    [InlineData("Parsed", true)]
    [InlineData("Validated", true)]
    [InlineData("Stable", false)]
    [InlineData("Promoted", false)]
    [InlineData("Rejected", false)]
    public void PendingStatuses_MatchLegacyVocabulary(string status, bool pending)
    {
        Assert.Equal(pending, RuleCandidateStatuses.IsPending(status));
    }

    [Fact]
    public void Validator_Passes_ForParsableClsFunction()
    {
        var outcome = new RuleCandidateValidator().Validate(new RuleCandidateRow { XmlBody = ValidXml });
        Assert.True(outcome.Success);
        Assert.Equal(1, outcome.FunctionCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("<broken")]
    public void Validator_Fails_ForEmptyOrInvalidXml(string xml)
    {
        var outcome = new RuleCandidateValidator().Validate(new RuleCandidateRow { XmlBody = xml });
        Assert.False(outcome.Success);
        Assert.False(string.IsNullOrEmpty(outcome.ErrorMessage));
    }
}
