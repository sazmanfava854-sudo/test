using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>نگاشت مرکزی RefParameter (Name/Value) از DSL/VB به FicheHeaderDto و ردیف‌ها.</summary>
public static class RefParameterRegistry
{
    private static readonly HashSet<string> KnownNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Center", "Center1", "Center2", "Center3",
        "Fund", "DocDate", "RowDate", "RowDocNo",
        "RefReconstructionNo", "RefownrDsc",
        "Ref", "Ref2", "Ref3", "Ref6",
        "vchrtyp", "VchrTyp", "PhasType", "phasTyp",
        "DUE", "Due", "QTY", "Qty"
    };

    public static RefParameterApplyResult ApplyAll(
        FicheHeaderDto fiche,
        IEnumerable<RefParameter> refs,
        IList<string>? warnings = null)
    {
        var applied = 0;
        var unknown = new List<string>();

        foreach (var r in refs)
        {
            if (string.IsNullOrWhiteSpace(r.Name))
                continue;

            if (!KnownNames.Contains(r.Name))
            {
                unknown.Add(r.Name);
                warnings?.Add($"RefParameter ناشناخته: {r.Name}={r.Value}");
                continue;
            }

            if (string.IsNullOrWhiteSpace(r.Value))
                continue;

            if (TryApply(fiche, r.Name, r.Value.Trim()))
                applied++;
        }

        return new RefParameterApplyResult(applied, unknown);
    }

    public static bool IsKnown(string name) => KnownNames.Contains(name);

    private static bool TryApply(FicheHeaderDto fiche, string name, string value)
    {
        switch (name.ToUpperInvariant())
        {
            case "CENTER":
                if (long.TryParse(value, out var center))
                {
                    fiche.Center = center;
                    return true;
                }
                return false;

            case "CENTER1":
                if (long.TryParse(value, out var c1))
                {
                    ApplyToAllRows(fiche, row => row.Center1 = c1);
                    return true;
                }
                return false;

            case "CENTER2":
                if (long.TryParse(value, out var c2))
                {
                    ApplyToAllRows(fiche, row => row.Center2 = c2);
                    return true;
                }
                return false;

            case "CENTER3":
                if (long.TryParse(value, out var c3))
                {
                    ApplyToAllRows(fiche, row => row.Center3 = c3);
                    return true;
                }
                return false;

            case "FUND":
                if (int.TryParse(value, out var fund))
                {
                    fiche.SuggestedFund = fund;
                    return true;
                }
                return false;

            case "DOCDATE":
                fiche.RayvarzDocDate = DateHelper.ToRayvarzDate(value);
                return fiche.RayvarzDocDate.Length > 0;

            case "ROWDATE":
                fiche.RowDate = DateHelper.ToShamsiSlashDate(value);
                fiche.RayvarzActDate = DateHelper.ToRayvarzDate(value);
                return true;

            case "ROWDOCNO":
                fiche.RefRowDocNo = value;
                return true;

            case "REFRECONSTRUCTIONNO":
                fiche.RefReconstructionNo = value;
                return true;

            case "REFOWNRDSC":
                fiche.RefOwnerDsc = value;
                return true;

            case "REF":
                ApplyToAllRows(fiche, row => row.Ref = value);
                return true;

            case "REF2":
                fiche.Ref2Override = value;
                return true;

            case "REF3":
                fiche.Ref3Override = value;
                return true;

            case "REF6":
                fiche.Ref6Override = value;
                return true;

            case "VCHRTYP":
                fiche.SoapVchrTypCode = value;
                return true;

            case "PHASTYP":
                fiche.SoapPhasTypCode = value;
                return true;

            case "DUE":
                fiche.RayvarzDueDate = DateHelper.ToRayvarzDate(value);
                return fiche.RayvarzDueDate.Length > 0;

            case "QTY":
                fiche.SoapQtyOverride = value;
                return true;

            default:
                return false;
        }
    }

    private static void ApplyToAllRows(FicheHeaderDto fiche, Action<IncmRowDto> apply)
    {
        if (fiche.Rows.Count == 0)
            fiche.Rows.Add(new IncmRowDto());

        foreach (var row in fiche.Rows)
            apply(row);
    }
}

public sealed record RefParameterApplyResult(int AppliedCount, IReadOnlyList<string> UnknownNames);
