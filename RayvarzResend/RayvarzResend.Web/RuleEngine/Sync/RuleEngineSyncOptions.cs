namespace RayvarzResend.Web.RuleEngine.Sync;

public sealed class RuleEngineSyncOptions
{
    public const string SectionName = "RuleEngineSync";

    /// <summary>فعال‌سازی polling و ذخیره state در RayvarzRuleEngine.</summary>
    public bool Enabled { get; set; }

    public int PollIntervalMinutes { get; set; } = 15;

    /// <summary>ثبات MemberHistory قبل از promote (پیش‌فرض ۷۲ ساعت).</summary>
    public int StabilityHours { get; set; } = 72;

    public int NidMember { get; set; } = 1388;

    /// <summary>RuleSyncState.NidClass (پیش‌فرض 360).</summary>
    public int NidClass { get; set; } = 360;

    /// <summary>اگر true باشد پس از شرط‌ها ActiveEngine به Dynamic تغییر می‌کند (پیش‌فرض false — فقط log).</summary>
    public bool AutoPromoteDynamic { get; set; }

    /// <summary>Connection string name for RuleSyncState tables (default AppAuth → RayvarzRuleEngine).</summary>
    public string StateConnectionStringName { get; set; } = "AppAuth";
}
