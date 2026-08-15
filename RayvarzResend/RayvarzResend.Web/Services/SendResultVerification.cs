namespace RayvarzResend.Web.Services;

/// <summary>قوانین پس از ارسال SOAP — Success فقط نتیجه رایورز است، نه تأیید incmdocsys.</summary>
public static class SendResultVerification
{
    public static string? BuildUnverifiedWarning(bool soapSuccess, bool verifiedInRayvarz, bool dryRun)
    {
        if (dryRun || !soapSuccess || verifiedInRayvarz)
            return null;
        return "SOAP موفق بود ولی فیش در incmdocsys تأیید نشد";
    }
}
