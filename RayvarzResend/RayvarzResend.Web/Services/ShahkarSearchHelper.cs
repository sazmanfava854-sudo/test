using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public static class ShahkarSearchHelper
{
    public static string? ValidateSearchRequest(ShahkarSearchRequest? req)
    {
        if (req == null)
            return "درخواست خالی است";

        var userName = (req.UserName ?? "").Trim();
        var nationalCode = (req.NationalCode ?? "").Trim();
        if (userName.Length == 0 && nationalCode.Length == 0)
            return "نام کاربری یا کد ملی را وارد کنید";

        if (nationalCode.Length > 0 && nationalCode.Length != 10)
            return "کد ملی باید ۱۰ رقم باشد";

        return null;
    }

    public static (string WhereSql, Dictionary<string, object> Parameters) BuildSearchWhere(ShahkarSearchRequest req)
    {
        var userName = (req.UserName ?? "").Trim();
        var nationalCode = (req.NationalCode ?? "").Trim();
        var clauses = new List<string>();
        var parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        if (userName.Length > 0)
        {
            clauses.Add("UserName = @userName");
            parameters["userName"] = userName;
        }

        if (nationalCode.Length > 0)
        {
            clauses.Add("NationalCode = @nationalCode");
            parameters["nationalCode"] = nationalCode;
        }

        return (string.Join(" AND ", clauses), parameters);
    }

    public static string QualifyTable(ShahkarOptions options)
    {
        var schema = SanitizeIdentifier(options.Schema, "dbo");
        var table = SanitizeIdentifier(options.TableName, "Users");
        return $"[{schema}].[{table}]";
    }

    private static string SanitizeIdentifier(string? value, string fallback)
    {
        var s = (value ?? "").Trim();
        if (s.Length == 0)
            s = fallback;

        foreach (var ch in s)
        {
            if (!char.IsLetterOrDigit(ch) && ch != '_')
                throw new ArgumentException("نام schema/table نامعتبر است");
        }

        return s;
    }
}
