namespace HRPerformance.Application.Common;

public static class DatabaseErrorHelper
{
    public static bool IsDatabaseError(Exception ex)
    {
        var name = ex.GetBaseException().GetType().Name;
        return name is "SqlException" or "DbUpdateException";
    }

    public static string GetPersianMessage(Exception ex)
    {
        var root = ex.GetBaseException();
        var name = root.GetType().Name;
        var message = root.Message;

        if (name == "SqlException")
        {
            if (message.Contains("login failed", StringComparison.OrdinalIgnoreCase))
            {
                if (message.Contains("Integrated Security", StringComparison.OrdinalIgnoreCase)
                    || message.Contains("trusted connection", StringComparison.OrdinalIgnoreCase)
                    || message.Contains("NT AUTHORITY", StringComparison.OrdinalIgnoreCase)
                    || message.Contains('\\'))
                    return "اتصال SQL با Windows Auth ناموفق بود. در IIS: App Pool Identity (مثلاً ITC\\sys-hoseine-sh) باید Login در SQL Server داشته باشد — database/17_GrantSqlAccess_WindowsUser.sql را اجرا کنید. تنظیمات: appsettings.Production.json (IIS) نه Development.json.";

                return "اتصال به SQL Server ناموفق بود. User Id/Password را در appsettings.Production.json (IIS) یا web.config بررسی کنید.";
            }

            if (message.Contains("cannot open database", StringComparison.OrdinalIgnoreCase))
                return "پایگاه HRPerformanceDB یافت نشد. اسکریپت‌های database/01 تا 08 را اجرا کنید.";

            if (message.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase))
                return "جداول سیستم ناقص است. اسکریپت‌های database/01 تا 11 را اجرا کنید.";

            return "خطا در اتصال به SQL Server. appsettings.Production.json (IIS) یا App Pool Identity را بررسی کنید.";
        }

        if (name == "DbUpdateException")
            return "خطا در ذخیره اطلاعات. اسکریپت database/11_RepairAuthentication.sql را اجرا کنید.";

        return "خطا در ورود. اتصال SQL و اسکریپت‌های database را بررسی کنید.";
    }
}
