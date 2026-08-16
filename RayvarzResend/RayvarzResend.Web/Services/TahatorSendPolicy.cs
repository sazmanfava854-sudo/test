namespace RayvarzResend.Web.Services;

/// <summary>
/// تصمیم ارسال تهاتر — ابزار resend فقط وجود در رایورز را معیار skip می‌گیرد،
/// نه Accounting_DocHeader (ممکن است فیش از incmdocsys حذف شده ولی در واسط مانده باشد).
/// </summary>
public static class TahatorSendPolicy
{
    public static bool NeedsSend(bool inRayvarz) => !inRayvarz;

    /// <summary>اگر query رایورز fail شده، NeedsSend نباید true شود (outage ≠ «نیاز به ارسال»).</summary>
    public static bool NeedsSend(bool inRayvarz, bool rayvarzCheckFailed) =>
        rayvarzCheckFailed ? false : NeedsSend(inRayvarz);

    public static bool ShouldSendMember(bool force, bool inRayvarz) => force || !inRayvarz;
}
