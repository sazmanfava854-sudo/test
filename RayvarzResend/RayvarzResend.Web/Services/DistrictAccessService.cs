using System.Security.Claims;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>محدودیت دسترسی کاربر منطقه‌ای به فیش‌های همان منطقه.</summary>
public static class DistrictAccessService
{
    public static string? GetUserDistrict(ClaimsPrincipal? user) =>
        user?.FindFirst(AuthClaimTypes.District)?.Value;

    public static string NormalizeDistrict(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        var v = value.Trim();
        if (!int.TryParse(v, out var n))
            return v;

        if (n is >= 201 and <= 212)
            return (n - 200).ToString();
        if (n is 218 or 80)
            return "218";
        if (n is >= 1 and <= 12)
            return n.ToString();
        return v;
    }

    public static int? DistrictToBranchId(string? district)
    {
        var d = NormalizeDistrict(district);
        if (string.IsNullOrEmpty(d))
            return null;
        if (d == "218")
            return 218;
        if (int.TryParse(d, out var n) && n is >= 1 and <= 12)
            return 200 + n;
        return null;
    }

    public static string? ResolveFicheDistrict(FicheHeaderDto fiche)
    {
        if (fiche.ResolvedDistrictBranch is > 0)
            return NormalizeDistrict(fiche.ResolvedDistrictBranch.Value.ToString());

        var region = fiche.DutyRegion ?? fiche.IncomeRegion;
        if (!string.IsNullOrWhiteSpace(region))
            return NormalizeDistrict(region);

        return null;
    }

    public static bool CanAccessFiche(ClaimsPrincipal? user, FicheHeaderDto fiche)
    {
        if (AppAuthService.IsAdmin(user))
            return true;

        var userDistrict = NormalizeDistrict(GetUserDistrict(user));
        if (string.IsNullOrEmpty(userDistrict))
            return false;

        var ficheDistrict = ResolveFicheDistrict(fiche);
        if (string.IsNullOrEmpty(ficheDistrict))
            return false;

        return userDistrict == ficheDistrict;
    }

    public static string? GetAccessDeniedMessage(ClaimsPrincipal? user, FicheHeaderDto fiche)
    {
        if (CanAccessFiche(user, fiche))
            return null;

        if (AppAuthService.IsAdmin(user))
            return null;

        var userDistrict = NormalizeDistrict(GetUserDistrict(user));
        var ficheDistrict = ResolveFicheDistrict(fiche);
        if (string.IsNullOrEmpty(userDistrict))
            return "منطقه کاربر تنظیم نشده — با ادمین تماس بگیرید";

        if (string.IsNullOrEmpty(ficheDistrict))
            return "منطقه فیش مشخص نیست — ارسال مجاز نیست";

        return $"این فیش متعلق به منطقه {ficheDistrict} است و برای کاربر منطقه {userDistrict} قابل ارسال نیست";
    }
}
