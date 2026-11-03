using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public static class AppUserUpdateRules
{
    public const string AdminOnlyChangeMessage = "فقط مدیر سیستم (Admin) می‌تواند وضعیت ادمین یا حساب‌های ادمین را تغییر دهد";

    public static void Apply(AppUserRecord user, UpdateAppUserRequest req)
    {
        var wasAdmin = user.IsAdmin;
        if (req.IsAdmin.HasValue)
            user.IsAdmin = req.IsAdmin.Value;
        if (req.IsActive.HasValue)
            user.IsActive = req.IsActive.Value;
        if (req.Domain != null)
        {
            var domain = AppUserDomainNormalizer.NormalizeList(req.Domain);
            if (!AppUserDomainNormalizer.IsValidList(domain))
                throw new ArgumentException("دامین الزامی است (مثلاً hoseine-sh یا hoseine-sh,sadathoseini-sh)");
            user.Domain = domain;
        }

        if (user.IsAdmin)
            user.DirectPermissions = AppUserDirectPermissions.AllGranted();
        else if (req.DirectPermissions != null)
            user.DirectPermissions = req.DirectPermissions;
        else if (wasAdmin)
            // AllGranted was written while admin; without this the demoted user keeps every permission.
            user.DirectPermissions = new AppUserDirectPermissions();
    }

    /// <summary>کاربر غیرادمین با مجوز «مدیریت کاربران» نباید ادمین بسازد/حذف کند یا حساب ادمین را ویرایش کند.</summary>
    public static string? DenyAdminChangeByNonAdmin(bool callerIsAdmin, bool targetIsAdmin, bool? requestedIsAdmin)
    {
        if (callerIsAdmin)
            return null;
        if (targetIsAdmin)
            return AdminOnlyChangeMessage;
        return requestedIsAdmin == true ? AdminOnlyChangeMessage : null;
    }
}
