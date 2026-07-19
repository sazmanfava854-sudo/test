using HRPerformance.Domain.Models;

namespace HRPerformance.Infrastructure.Services.ExternalHr;

public static class MisShamsiQueryParser
{
    public static bool TryParseRange(
        string? fromCompact,
        string? toCompact,
        string? shamsiFromYear,
        string? shamsiFromMonth,
        string? shamsiFromDay,
        string? shamsiToYear,
        string? shamsiToMonth,
        string? shamsiToDay,
        out MisSyncDateRangeRequest request,
        out string errorMessage,
        int employeeLimit = 0)
    {
        if (!string.IsNullOrWhiteSpace(fromCompact) && !string.IsNullOrWhiteSpace(toCompact))
        {
            if (!TryParseCompactDate(fromCompact, out var fromParts, out errorMessage))
            {
                request = default!;
                return false;
            }

            if (!TryParseCompactDate(toCompact, out var toParts, out errorMessage))
            {
                request = default!;
                return false;
            }

            request = new MisSyncDateRangeRequest(
                fromParts.Year, fromParts.Month, fromParts.Day,
                toParts.Year, toParts.Month, toParts.Day,
                employeeLimit);
            errorMessage = string.Empty;
            return true;
        }

        if (!TryParsePart(shamsiFromYear, "shamsiFromYear", out var fy, out errorMessage))
        {
            request = default!;
            return false;
        }

        if (!TryParsePart(shamsiFromMonth, "shamsiFromMonth", out var fm, out errorMessage))
        {
            request = default!;
            return false;
        }

        if (!TryParsePart(shamsiFromDay, "shamsiFromDay", out var fd, out errorMessage))
        {
            request = default!;
            return false;
        }

        if (!TryParsePart(shamsiToYear, "shamsiToYear", out var ty, out errorMessage))
        {
            request = default!;
            return false;
        }

        if (!TryParsePart(shamsiToMonth, "shamsiToMonth", out var tm, out errorMessage))
        {
            request = default!;
            return false;
        }

        if (!TryParsePart(shamsiToDay, "shamsiToDay", out var td, out errorMessage))
        {
            request = default!;
            return false;
        }

        request = new MisSyncDateRangeRequest(fy, fm, fd, ty, tm, td, employeeLimit);
        errorMessage = string.Empty;
        return true;
    }

    private static bool TryParsePart(string? raw, string paramName, out int value, out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            value = 0;
            errorMessage =
                $"پارامتر {paramName} خالی است. " +
                "مثال: shamsiToDay=11 یا from=1404/04/10&to=1404/04/11";
            return false;
        }

        var trimmed = raw.Trim();
        if (!int.TryParse(trimmed, out value))
        {
            errorMessage =
                $"پارامتر {paramName} نامعتبر است: '{raw}'. " +
                "فقط عدد وارد کنید — بدون متن اضافه (مثال shamsiToDay=11).";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    private static bool TryParseCompactDate(string raw, out (int Year, int Month, int Day) parts, out string errorMessage)
    {
        parts = default;
        var normalized = raw.Trim().Replace('-', '/').Replace('.', '/');
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length != 3)
        {
            errorMessage =
                $"فرمت تاریخ '{raw}' نامعتبر است. از from=1404/04/10 و to=1404/04/11 استفاده کنید.";
            return false;
        }

        if (!int.TryParse(segments[0], out var year) ||
            !int.TryParse(segments[1], out var month) ||
            !int.TryParse(segments[2], out var day))
        {
            errorMessage = $"تاریخ '{raw}' باید فقط عدد باشد (مثال 1404/04/10).";
            return false;
        }

        parts = (year, month, day);
        errorMessage = string.Empty;
        return true;
    }
}
