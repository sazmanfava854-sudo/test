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

    /// <summary>
    /// ستون Domain می‌تواند چند دامین با کاما داشته باشد (مثلاً hoseine-sh,sadathoseini-sh)؛
    /// ورود SSO با هر کدام به همان کاربر می‌رسد.
    /// </summary>
    public static string NormalizeList(string? value)
    {
        var parts = SplitList(value);
        return string.Join(",", parts);
    }

    public static bool IsValidList(string? value)
    {
        var parts = SplitList(value);
        return parts.Count > 0 && parts.All(IsValid);
    }

    public static bool ListContains(string? list, string? domain)
    {
        var target = Normalize(domain);
        return target.Length > 0
            && SplitList(list).Any(d => d.Equals(target, StringComparison.OrdinalIgnoreCase));
    }

    private static List<string> SplitList(string? value)
    {
        var result = new List<string>();
        foreach (var raw in (value ?? "").Split([',', ';', '،'], StringSplitOptions.RemoveEmptyEntries))
        {
            var item = Normalize(raw);
            if (item.Length > 0 && !result.Contains(item, StringComparer.OrdinalIgnoreCase))
                result.Add(item);
        }

        return result;
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
