using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>GetNosaziNickName — Duty_FicheSub.NidFK → Base_NosaziCode (۸ بخش با Shop).</summary>
public static class NosaziNickNameLogic
{
    public const string NickSource =
        "کد نوسازی — GetNosaziNickName (Duty_FicheSub.NidFK → Base_NosaziCode)";

    public static string FormatSqlFailureWarning(string sqlMessage) =>
        $"GetNosaziNickName ناموفق (SQL): {sqlMessage} — از OtherFields استفاده می‌شود";

    public static void ApplyLoadResult(FicheHeaderDto dto, string? nick, string? loadError)
    {
        if (!string.IsNullOrWhiteSpace(nick))
        {
            dto.BnkAcntNo = nick;
            dto.BnkAcntNoSource = NickSource;
            return;
        }

        if (!string.IsNullOrWhiteSpace(loadError))
            dto.Warning = loadError;
    }
}
