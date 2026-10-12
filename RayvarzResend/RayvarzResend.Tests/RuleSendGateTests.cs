using Microsoft.Extensions.Logging.Abstractions;
using RayvarzResend.Web.RuleEngine.Sync;
using Xunit;

namespace RayvarzResend.Tests;

public class RuleSendGateTests
{
    private const string Approved = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string Changed = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

    [Fact]
    public void BlockedMessages_SeparateUserAndAdminText()
    {
        Assert.Equal("باید برنامه به‌روزرسانی شود", RuleSendGate.UserBlockedMessage);
        Assert.Equal("نیاز به به روز رسانی هست", RuleSendGate.AdminBlockedMessage);
        Assert.Equal(RuleSendGate.UserBlockedMessage, new RuleSendGateBlockedException().Message);
    }

    [Fact]
    public void Policy_Baseline_OpensWithCurrentHash()
    {
        var d = RuleSendGatePolicy.Evaluate(null, null, null, Approved, isBaseline: true);
        Assert.Equal(RuleSendGateStates.Open, d.Gate);
        Assert.Equal(Approved, d.ApprovedHash);
        Assert.Equal(RulePromotionActions.SendApproved, d.Action);
    }

    [Fact]
    public void Policy_BlocksImmediately_WhenMemberDiffersFromApproved()
    {
        var d = RuleSendGatePolicy.Evaluate(RuleSendGateStates.Open, Approved, Approved, Changed, isBaseline: false);
        Assert.Equal(RuleSendGateStates.Blocked, d.Gate);
        Assert.Equal(Approved, d.ApprovedHash);
        Assert.Equal(RulePromotionActions.SendBlocked, d.Action);
    }

    [Fact]
    public void Policy_StaysBlocked_WithoutNewLog_UntilAdminApproves()
    {
        var d = RuleSendGatePolicy.Evaluate(RuleSendGateStates.Blocked, Approved, Changed, Changed, isBaseline: false);
        Assert.Equal(RuleSendGateStates.Blocked, d.Gate);
        Assert.False(d.Changed);
    }

    [Fact]
    public void Policy_StableAfter72h_DoesNotReopen_WhenApprovedHashIsOld()
    {
        var d = RuleSendGatePolicy.Evaluate(RuleSendGateStates.Blocked, Approved, lastStableHash: Changed, Changed, isBaseline: false);
        Assert.Equal(RuleSendGateStates.Blocked, d.Gate);
    }

    [Fact]
    public void Policy_StaysOpen_AfterAdminApprovedCurrentHash()
    {
        var d = RuleSendGatePolicy.Evaluate(RuleSendGateStates.Open, Changed, Approved, Changed.ToUpperInvariant() + " ", isBaseline: false);
        Assert.Equal(RuleSendGateStates.Open, d.Gate);
        Assert.False(d.Changed);
    }

    [Fact]
    public void Policy_Reopens_WhenMemberRevertsToApproved()
    {
        var d = RuleSendGatePolicy.Evaluate(RuleSendGateStates.Blocked, Approved, Approved, Approved, isBaseline: false);
        Assert.Equal(RuleSendGateStates.Open, d.Gate);
        Assert.Equal(RulePromotionActions.SendReopened, d.Action);
    }

    [Fact]
    public void Policy_InitialisesApprovedFromStable_ForPreExistingRows()
    {
        var open = RuleSendGatePolicy.Evaluate(null, null, Approved, Approved, isBaseline: false);
        Assert.Equal(RuleSendGateStates.Open, open.Gate);
        Assert.Equal(Approved, open.ApprovedHash);
        Assert.True(open.Changed);

        var blocked = RuleSendGatePolicy.Evaluate(null, "", Approved, Changed, isBaseline: false);
        Assert.Equal(RuleSendGateStates.Blocked, blocked.Gate);
        Assert.Equal(Approved, blocked.ApprovedHash);
    }

    [Fact]
    public async Task Gate_Throws_ExactMessage_WhenBlocked()
    {
        var gate = CreateGate(new FakeStore { Gate = "Blocked" });
        Assert.True(await gate.IsBlockedAsync());
        var ex = await Assert.ThrowsAsync<RuleSendGateBlockedException>(() => gate.EnsureOpenAsync());
        Assert.Equal("باید برنامه به‌روزرسانی شود", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Open")]
    [InlineData("open ")]
    public async Task Gate_Allows_WhenOpenOrNull(string? value)
    {
        var gate = CreateGate(new FakeStore { Gate = value });
        Assert.False(await gate.IsBlockedAsync());
        await gate.EnsureOpenAsync();
    }

    [Fact]
    public async Task Gate_IsInactive_WhenSyncOrGateDisabled()
    {
        var store = new FakeStore { Gate = "Blocked" };
        Assert.False(await CreateGate(store, o => o.Enabled = false).IsBlockedAsync());
        Assert.False(await CreateGate(store, o => o.SendGateEnabled = false).IsBlockedAsync());
        Assert.False(await CreateGate(new FakeStore { Gate = "Blocked", IsConfigured = false }).IsBlockedAsync());
        Assert.Equal(0, store.Reads);
    }

    [Fact]
    public async Task Gate_FailsOpen_WhenStoreThrows()
    {
        var gate = CreateGate(new FakeStore { Throw = true });
        Assert.False(await gate.IsBlockedAsync());
    }

    [Fact]
    public async Task Gate_CachesUntilExpiryOrInvalidate()
    {
        var now = new DateTime(2026, 10, 12, 8, 0, 0, DateTimeKind.Utc);
        var store = new FakeStore { Gate = "Open" };
        var gate = new RuleSendGate(store, Options(o => o.SendGateCacheSeconds = 15), NullLogger<RuleSendGate>.Instance, () => now);

        Assert.False(await gate.IsBlockedAsync());
        store.Gate = "Blocked";
        Assert.False(await gate.IsBlockedAsync());
        Assert.Equal(1, store.Reads);

        now = now.AddSeconds(16);
        Assert.True(await gate.IsBlockedAsync());
        Assert.Equal(2, store.Reads);

        store.Gate = "Open";
        gate.Invalidate();
        Assert.False(await gate.IsBlockedAsync());
        Assert.Equal(3, store.Reads);
    }

    [Theory]
    [InlineData("/api/fiche/send")]
    [InlineData("/api/tahator/send")]
    [InlineData("/api/unsent/send-batch")]
    public void SendEndpoints_CheckGate_BeforeAnyOtherValidation(string route)
    {
        var program = File.ReadAllText(Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "RayvarzResend.Web", "Program.cs")));
        var start = program.IndexOf($"app.MapPost(\"{route}\"", StringComparison.Ordinal);
        Assert.True(start >= 0, route);
        var bodyStart = program.IndexOf("=>\n{\n", start, StringComparison.Ordinal) + "=>\n{\n".Length;
        var firstStatement = program[bodyStart..program.IndexOf('\n', bodyStart)].Trim();
        Assert.Equal("if (await sendGate.IsBlockedAsync(ct))", firstStatement);
    }

    private static RuleEngineSyncOptions Options(Action<RuleEngineSyncOptions>? configure = null)
    {
        var options = new RuleEngineSyncOptions { Enabled = true, SendGateEnabled = true, SendGateCacheSeconds = 0 };
        configure?.Invoke(options);
        return options;
    }

    private static RuleSendGate CreateGate(FakeStore store, Action<RuleEngineSyncOptions>? configure = null) =>
        new(store, Options(configure), NullLogger<RuleSendGate>.Instance, () => DateTime.UtcNow);

    private sealed class FakeStore : IRuleSendGateStore
    {
        public string? Gate { get; set; }
        public bool Throw { get; set; }
        public bool IsConfigured { get; set; } = true;
        public int Reads { get; private set; }

        public Task<string?> GetSendGateAsync(int nidMember, CancellationToken ct = default)
        {
            Reads++;
            if (Throw)
                throw new InvalidOperationException("db down");
            return Task.FromResult(Gate);
        }
    }
}
