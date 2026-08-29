using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

internal static class SsoUserProvisioningHelper
{
    public static (string FirstName, string LastName, string NationalId, string Position, string District)
        NormalizeProfile(ShimasUserProfile profile)
    {
        var username = (profile.Username ?? "").Trim();
        var firstName = (profile.FirstName ?? "").Trim();
        var lastName = (profile.LastName ?? "").Trim();
        var nationalId = (profile.NationalId ?? "").Trim();
        var position = (profile.Position ?? "").Trim();
        var district = (profile.District ?? "").Trim();

        if (string.IsNullOrEmpty(firstName))
        {
            var parts = username.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            firstName = parts.Length > 0 ? parts[0] : username;
        }

        if (string.IsNullOrEmpty(lastName))
            lastName = "کاربر";

        if (!AppUserInputNormalizer.IsValidNationalId(nationalId))
            nationalId = BuildPlaceholderNationalId(username);

        if (string.IsNullOrEmpty(position))
            position = "کاربر سازمان";

        return (firstName, lastName, nationalId, position, district);
    }

    private static string BuildPlaceholderNationalId(string username)
    {
        var hash = Math.Abs(StringComparer.Ordinal.GetHashCode(username)) % 1_000_000_000;
        return $"9{hash:D9}";
    }
}
