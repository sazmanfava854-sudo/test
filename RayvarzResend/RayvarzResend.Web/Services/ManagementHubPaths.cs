namespace RayvarzResend.Web.Services;

/// <summary>صفحهٔ مدیریت: https://city.mashhad.ir:5065/management — املای قدیمی /MANAGMENT هم پذیرفته می‌شود.</summary>
public static class ManagementHubPaths
{
    public const string Canonical = "/management";
    public const string CanonicalWithSlash = "/management/";

    private static readonly string[] Roots = new[] { "/management", "/managment" };

    /// <summary>ریشهٔ هاب بدون اسلش آخر (هر حالت حروف).</summary>
    public static bool IsRoot(string? path)
    {
        var value = (path ?? "").Trim();
        return Roots.Any(r => value.Equals(r, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>ریشه، ریشه با اسلش، یا index.html هاب.</summary>
    public static bool IsHubPage(string? path)
    {
        var value = (path ?? "").Trim();
        foreach (var root in Roots)
        {
            if (value.Equals(root, StringComparison.OrdinalIgnoreCase)
                || value.Equals(root + "/", StringComparison.OrdinalIgnoreCase)
                || value.Equals(root + "/index.html", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    /// <summary>هر مسیری زیر هاب که با املای canonical (حروف کوچک) نیست — باید به /management/ ریدایرکت شود.</summary>
    public static bool NeedsCanonicalRedirect(string? path)
    {
        var value = (path ?? "").Trim();
        if (IsRoot(value))
            return true;

        foreach (var root in Roots)
        {
            if (!value.StartsWith(root + "/", StringComparison.OrdinalIgnoreCase))
                continue;
            return !value.StartsWith(CanonicalWithSlash, StringComparison.Ordinal);
        }

        return false;
    }
}
