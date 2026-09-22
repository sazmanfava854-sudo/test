namespace RayvarzResend.Web.Services;

/// <summary>نام کاربری ویندوز / دامین لاگین یکپارچه — مثلاً hoseine-sh یا MASHHAD\hoseine-sh.</summary>
public static class AppUserDomainNormalizer
{
    public static string Normalize(string? value)
    {
        var s = (value ?? "").Trim();
        if (s.Length == 0)
            return "";

        var slash = s.LastIndexOf('\\');
        if (slash >= 0 && slash < s.Length - 1)
            s = s[(slash + 1)..];

        var at = s.IndexOf('@');
        if (at > 0)
            s = s[..at];

        return s.Trim();
    }

    public static bool IsValid(string? value)
    {
        var s = Normalize(value);
        if (s.Length is < 2 or > 100)
            return false;

        foreach (var ch in s)
        {
            if (char.IsLetterOrDigit(ch) || ch is '-' or '_' or '.')
                continue;
            return false;
        }

        return true;
    }
}
