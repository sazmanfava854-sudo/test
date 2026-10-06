namespace RayvarzResend.Web.Services;

internal static class SsoCredentialMask
{
    public static string MaskId(string? value)
    {
        var text = (value ?? "").Trim();
        if (text.Length <= 6)
            return "***";
        return text[..4] + "…" + text[^4..];
    }

    public static bool SecretLooksTooShort(string? secret, int minLength = 8) =>
        (secret ?? "").Trim().Length < minLength;
}
