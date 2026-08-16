namespace RayvarzResend.Web.Services;

/// <summary>
/// تصمیم ارسال تهاتر — ابزار resend فقط وجود در رایورز را معیار skip می‌گیرد،
/// نه Accounting_DocHeader (ممکن است فیش از incmdocsys حذف شده ولی در واسط مانده باشد).
/// </summary>
public static class TahatorSendPolicy
{
    public static bool NeedsSend(bool inRayvarz) => !inRayvarz;

    public static bool ShouldSendMember(bool force, bool inRayvarz) => force || !inRayvarz;
}
