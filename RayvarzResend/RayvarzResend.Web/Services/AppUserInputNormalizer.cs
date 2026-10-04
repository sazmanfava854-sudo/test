using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>اعتبارسنجی و نرمال‌سازی ثبت کاربر — ورود محلی همان دامین SSO (مثلاً alidoost-pa).</summary>
public static class AppUserInputNormalizer
{
    public const int NationalIdLength = 10;

    public static void ValidateAndApply(CreateAppUserRequest req)
    {
        req.FirstName = (req.FirstName ?? "").Trim();
        req.LastName = (req.LastName ?? "").Trim();
        req.NationalId = (req.NationalId ?? "").Trim();
        req.Position = (req.Position ?? "").Trim();
        req.District = (req.District ?? "").Trim();
        req.Domain = AppUserDomainNormalizer.NormalizeList(req.Domain);
        req.Username = ResolveLoginUsername(req);

        if (string.IsNullOrWhiteSpace(req.FirstName))
            throw new ArgumentException("نام الزامی است");
        if (string.IsNullOrWhiteSpace(req.LastName))
            throw new ArgumentException("نام خانوادگی الزامی است");
        if (!IsValidNationalId(req.NationalId))
            throw new ArgumentException("کد ملی باید ۱۰ رقم باشد");
        if (!AppUserDomainNormalizer.IsValidList(req.Domain))
            throw new ArgumentException("دامین الزامی است (مثلاً hoseine-sh)");
        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6)
            throw new ArgumentException("رمز عبور حداقل ۶ کاراکتر باشد");
        if (!req.IsAdmin && string.IsNullOrWhiteSpace(req.District))
            throw new ArgumentException("برای کاربر منطقه‌ای، انتخاب منطقه یا شعبه مرکز الزامی است");
    }

    /// <summary>ورود محلی همان SSO: اول دامین (alidoost-pa)، بعد کد ملی اگر Username خالی باشد.</summary>
    public static string ResolveLoginUsername(CreateAppUserRequest req)
    {
        var username = (req.Username ?? "").Trim();
        if (!string.IsNullOrEmpty(username))
            return username;

        var domainList = AppUserDomainNormalizer.NormalizeList(req.Domain);
        if (!string.IsNullOrEmpty(domainList))
        {
            var first = domainList.Split(',')[0].Trim();
            if (AppUserDomainNormalizer.IsValid(first))
                return first;
        }

        var nationalId = (req.NationalId ?? "").Trim();
        if (IsValidNationalId(nationalId))
            return nationalId;

        throw new ArgumentException("دامین سازمانی یا کد ملی برای ورود الزامی است");
    }

    public static bool IsValidNationalId(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && value.Trim().Length == NationalIdLength
        && value.Trim().All(char.IsDigit);
}
