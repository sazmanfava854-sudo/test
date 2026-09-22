using System.Text;
using System.Text.Json;

namespace RayvarzResend.Web;

/// <summary>پیام خطای خوانا وقتی appsettings.json بعد از ویرایش ConnectionString خراب شود.</summary>
public static class AppSettingsJsonGuard
{
    public static void ValidateOrThrow(string? contentRoot = null)
    {
        var path = FindAppSettingsPath(contentRoot);
        if (path == null)
            return;

        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            _ = doc.RootElement.ValueKind;
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException(Describe(ex, contentRoot), ex);
        }
    }

    public static bool IsLoadError(Exception ex)
    {
        for (var cur = ex; cur != null; cur = cur.InnerException)
        {
            if (cur is InvalidDataException or FormatException or JsonException)
                return true;
        }
        return false;
    }

    public static string Describe(Exception ex, string? contentRoot = null)
    {
        var jsonEx = FindJsonException(ex);
        var path = FindAppSettingsPath(contentRoot);
        var line = jsonEx?.LineNumber is long n ? (int)n + 1 : (int?)null;
        var snippet = path != null && line is > 0 ? ReadLine(path, line.Value) : null;

        var sb = new StringBuilder();
        sb.AppendLine("خطا در خواندن appsettings.json — فایل JSON نامعتبر است.");
        if (path != null)
            sb.AppendLine("مسیر: " + path);
        if (line != null)
            sb.AppendLine($"خط {line}: {snippet?.Trim() ?? jsonEx?.Message}");
        sb.AppendLine();
        sb.AppendLine("علت رایج: بعد از بستن ConnectionStrings ویرگول جا مانده.");
        sb.AppendLine("درست:");
        sb.AppendLine("  },");
        sb.AppendLine("  \"Auth\": {");
        sb.AppendLine();
        sb.AppendLine("اگر رمز عبور کاراکتر \" یا \\ دارد، در JSON باید به صورت \\\" و \\\\ نوشته شود.");
        sb.AppendLine("بخش ConnectionStrings را از فایل zip دوباره کپی کنید و فقط مقدار Server/Password را عوض کنید.");
        return sb.ToString().TrimEnd();
    }

    private static JsonException? FindJsonException(Exception ex)
    {
        for (var cur = ex; cur != null; cur = cur.InnerException)
        {
            if (cur is JsonException json)
                return json;
        }
        return null;
    }

    private static string? FindAppSettingsPath(string? contentRoot)
    {
        foreach (var dir in new[] { contentRoot, Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            if (string.IsNullOrWhiteSpace(dir))
                continue;
            var path = Path.Combine(dir, "appsettings.json");
            if (File.Exists(path))
                return path;
        }
        return null;
    }

    private static string? ReadLine(string path, int lineNumber)
    {
        try
        {
            var n = 0;
            foreach (var line in File.ReadLines(path))
            {
                n++;
                if (n == lineNumber)
                    return line;
            }
        }
        catch
        {
            // ignore
        }
        return null;
    }
}
