using System.Text.RegularExpressions;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>تب «مرکز هزینه» — به‌روزرسانی Deposit و CreditorPapers در dbo.Income_Fiche.</summary>
public static class CostCenterHelper
{
    private static readonly Regex FicheNoSplit = new(@"[,;،\s]+", RegexOptions.Compiled);

    public static IReadOnlyList<string> ParseFicheNoTokens(string? raw)
    {
        var text = (raw ?? "").Trim();
        if (text.Length == 0)
            return Array.Empty<string>();

        return FicheNoSplit
            .Split(text)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToList();
    }

    public static IReadOnlyList<string> ExpandSearchFicheKeys(string? raw)
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var token in ParseFicheNoTokens(raw))
        {
            foreach (var key in UnsentBillPayLookupHelper.ExpandFicheNoKeys(token))
                keys.Add(key);
        }

        return keys.Count == 0 ? Array.Empty<string>() : keys.ToList();
    }

    public static string? ValidateSearchRequest(CostCenterSearchRequest req)
    {
        if (ExpandSearchFicheKeys(req.FicheNo).Count == 0)
            return "شماره فیش را وارد کنید";
        return null;
    }

    public static (string WhereSql, List<(string Name, object Value)> Parameters) BuildSearchWhere(
        CostCenterSearchRequest req)
    {
        var keys = ExpandSearchFicheKeys(req.FicheNo);
        var parameters = new List<(string, object)>();
        var names = new List<string>(keys.Count);
        for (var i = 0; i < keys.Count; i++)
        {
            var name = $"@ficheNo{i}";
            names.Add(name);
            parameters.Add((name, keys[i]));
        }

        var whereSql = names.Count > 0
            ? "f.FicheNo IN (" + string.Join(", ", names) + ")"
            : "1=0";
        return (whereSql, parameters);
    }

    public static string? ValidateUpdateRequest(CostCenterUpdateRequest req)
    {
        var ficheNos = (req.FicheNos ?? [])
            .Select(s => (s ?? "").Trim())
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (ficheNos.Count == 0)
            return "حداقل یک فیش انتخاب کنید";

        if (!TryParseCenterValue(req.CenterOne, out _))
            return "مرکز یک باید عدد باشد";
        if (!TryParseCenterValue(req.CenterTwo, out _))
            return "مرکز دو باید عدد باشد";

        return null;
    }

    public static bool TryParseCenterValue(string? raw, out long value)
    {
        value = 0;
        var text = IdentifierText.Normalize(raw);
        if (text.Length == 0 || !text.All(char.IsDigit))
            return false;
        return long.TryParse(text, out value);
    }
}
