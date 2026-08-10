using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Web.Validation;

/// <summary>
/// فیلدهای اجباری/اختیاری بر اساس مستند RayReceiveIncmVchr.dll —
/// SetHeaderDoc، AddDocItem، AddIncm.
/// </summary>
public static class RayvarzFieldRequirementCatalog
{
    public const string OpHeader = "SetHeaderDoc";
    public const string OpDocItem = "AddDocItem";
    public const string OpIncm = "AddIncm";
    public const string OpSave = "SaveDocument";
    public const string OpPreSend = "PreSend";
    public const string OpCompatibility = "Compatibility";

    public static bool RequiresFund(FicheHeaderDto fiche) =>
        fiche.Category is FicheCategory.DutyNosazi or FicheCategory.DutySenfi
        || TahatorRowBuilder.IsTahatorFiche(fiche)
        || fiche.SuggestedFund is > 0;

    public static bool RequiresDocumentCenter(FicheHeaderDto fiche) =>
        fiche.Center is > 0
        || fiche.IncomeAccountGroup is 156
        || TahatorRowBuilder.IsTahatorIncomeFiche(fiche);

    public static IncmRowFieldRequirements ResolveIncmRowRequirements(int incmNo, FicheHeaderDto fiche)
    {
        var req = new IncmRowFieldRequirements();

        if (TahatorRowBuilder.IsTahatorFiche(fiche))
        {
            req.RequiresCenter1 = true;
            req.RequiresRefRowDocNo = true;
            req.RequiresRefRowDate = true;
            return req;
        }

        if (fiche.Category is FicheCategory.DutyNosazi or FicheCategory.DutySenfi)
        {
            req.RequiresRef = true;
            // نوسازی/صنفی: Qty = Payable کل فیش در هر ردیف؛ Val = سهم همان IncmNo
            return req;
        }

        if (incmNo is 100098 or 100107 or 100108 || fiche.IncomeAccountGroup == 156)
        {
            req.RequiresCenter1 = true;
            req.RequiresCenter2 = true;
            req.RequiresCenter3 = true;
        }

        if (fiche.IncomeAccountGroup == 124)
            req.RequiresCenter2 = true;

        return req;
    }

    public sealed class IncmRowFieldRequirements
    {
        public bool RequiresCenter1 { get; set; }
        public bool RequiresCenter2 { get; set; }
        public bool RequiresCenter3 { get; set; }
        public bool RequiresRef { get; set; }
        public bool RequiresNum { get; set; }
        public bool RequiresQtyEqualsVal { get; set; }
        public bool RequiresRefRowDocNo { get; set; }
        public bool RequiresRefRowDate { get; set; }
    }
}
