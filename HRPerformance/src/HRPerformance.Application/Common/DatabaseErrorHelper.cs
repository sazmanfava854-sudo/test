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
                return "اتصال به SQL Server ناموفق بود. User Id یا Password در app\\appsettings.Development.json را بررسی کنید.";

            if (message.Contains("cannot open database", StringComparison.OrdinalIgnoreCase))
                return "پایگاه HRPerformanceDB یافت نشد. اسکریپت‌های database/01 تا 08 را اجرا کنید.";

            if (message.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase))
                return "جداول سیستم ناقص است. اسکریپت‌های database/01 تا 11 را اجرا کنید.";

            return "خطا در اتصال به SQL Server. سرور، پورت، و پسورد را در app\\appsettings.Development.json بررسی کنید.";
        }

        if (name == "DbUpdateException")
            return "خطا در ذخیره اطلاعات. اسکریپت database/11_RepairAuthentication.sql را اجرا کنید.";

        return "خطا در ورود. اتصال SQL و اسکریپت‌های database را بررسی کنید.";
    }
}
