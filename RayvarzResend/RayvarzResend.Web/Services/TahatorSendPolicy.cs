namespace RayvarzResend.Web.Services;

/// <summary>
/// تصمیم ارسال تهاتر — فیش نباید هم در رایورز و هم در Accounting_DocHeader باشد.
/// </summary>
public static class TahatorSendPolicy
{
    public static bool NeedsSend(bool inRayvarz, bool inAccountingDocHeader, bool rayvarzCheckFailed = false) =>
        rayvarzCheckFailed ? false : !inRayvarz && !inAccountingDocHeader;

    /// <summary>اگر در رایورز یا واسط Sara باشد ارسال نمی‌شود (حتی با force).</summary>
    public static bool ShouldSendMember(bool force, bool inRayvarz, bool inAccountingDocHeader)
    {
        if (inRayvarz || inAccountingDocHeader)
            return false;
        return true;
    }
}
