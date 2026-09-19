using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>ساخت ردیف‌های Accounting_DocHeader/Details — parity Member 1388 ClsAccounting.Save.</summary>
public static class AccountingDocRowBuilder
{
    public const byte ObjOnPriceIncome = 1;
    public const byte ObjOnPriceNosazi = 2;
    public const byte ObjOnPriceSenfi = 3;

    public const byte ObjInDocumentFiche = 2;
    public const byte DocumentingCauseConfirm = 1;

    public const int PhasTypeRayvarz = 7;

    public sealed class AccountingDocHeaderDraft
    {
        public Guid GidDocHeader { get; init; }
        public string AccountingNo { get; init; } = "";
        public decimal SaraPrice { get; init; }
        public string DocDate { get; init; } = "";
        public string DocTime { get; init; } = "";
        public byte EumObjOnPrice { get; init; }
        public byte EumAccountingObjInDocument { get; init; } = ObjInDocumentFiche;
        public byte EumAccountingDocumentingCause { get; init; } = DocumentingCauseConfirm;
        public int DocRow { get; init; }
        public int PhasType { get; init; } = PhasTypeRayvarz;
        public string SubSystem { get; init; } = "Rayvarz";
        public string FicheNo { get; init; } = "";
        public Guid NidFiche { get; init; }
    }

    public sealed class AccountingDocDetailDraft
    {
        public Guid GidDocDetails { get; init; }
        public decimal Price { get; set; }
        public string FicheNo { get; init; } = "";
        public string BillId { get; init; } = "";
        public string PaymentId { get; init; } = "";
        public int PaymentDate { get; init; }
        public string BankCode { get; init; } = "18";
        public string AccountNo { get; init; } = "";
        public string AccountNoComments { get; init; } = "";
        public int WrapperAccountNo { get; init; }
        public int IncmRow { get; init; }
    }

    public static (AccountingDocHeaderDraft Header, List<AccountingDocDetailDraft> Details) Build(
        FicheHeaderDto fiche,
        RayvarzDocMeta? rayMeta,
        string? pursuitDocNo)
    {
        var details = BuildDetails(fiche);
        ReconcileDetailPrices(details, fiche.Payable);

        var accountingNo = BuildAccountingNo(fiche, rayMeta, pursuitDocNo);
        var now = DateTime.Now;
        var pc = new System.Globalization.PersianCalendar();

        var header = new AccountingDocHeaderDraft
        {
            GidDocHeader = Guid.NewGuid(),
            AccountingNo = accountingNo,
            SaraPrice = fiche.Payable,
            DocDate = $"{pc.GetYear(now):0000}/{pc.GetMonth(now):00}/{pc.GetDayOfMonth(now):00}",
            DocTime = now.ToString("HH:mm:ss"),
            EumObjOnPrice = ResolveObjOnPrice(fiche),
            DocRow = ResolveDocRow(fiche),
            FicheNo = fiche.FicheNo.Trim(),
            NidFiche = fiche.NidFiche
        };

        return (header, details);
    }

    public static List<AccountingDocDetailDraft> BuildDetails(FicheHeaderDto fiche)
    {
        var paymentDate = ResolvePaymentDateCompact(fiche);
        var bankCode = ResolveBankCode(fiche);
        var billId = FirstSegment(fiche.BillId);
        var paymentId = FirstSegment(fiche.PaymentId);
        var accountNo = string.IsNullOrWhiteSpace(fiche.BnkAcntNo) ? "" : fiche.BnkAcntNo.Trim();
        var ficheNo = fiche.FicheNo.Trim();

        var details = new List<AccountingDocDetailDraft>();
        var incmRow = 1;
        foreach (var row in fiche.Rows.Where(r => r.Val != 0))
        {
            details.Add(new AccountingDocDetailDraft
            {
                GidDocDetails = Guid.NewGuid(),
                Price = Math.Round(row.Val, 0),
                FicheNo = ficheNo,
                BillId = billId,
                PaymentId = paymentId,
                PaymentDate = paymentDate,
                BankCode = bankCode,
                AccountNo = accountNo,
                AccountNoComments = row.IncmRowDsc?.Trim() ?? "",
                WrapperAccountNo = row.IncmNo,
                IncmRow = incmRow++
            });
        }

        return details;
    }

    public static void ReconcileDetailPrices(IList<AccountingDocDetailDraft> details, decimal payable)
    {
        if (details.Count == 0)
            return;

        var sum = details.Sum(d => d.Price);
        if (sum == payable)
            return;

        details[0].Price += payable - sum;
    }

    public static string BuildAccountingNo(FicheHeaderDto fiche, RayvarzDocMeta? rayMeta, string? pursuitDocNo)
    {
        var prefix = fiche.Category switch
        {
            FicheCategory.DutySenfi => "Sen",
            FicheCategory.DutyNosazi => "Nos",
            _ => "Incm"
        };

        var branch = rayMeta?.Branch ?? 0;
        var yr = rayMeta?.Yr ?? DateHelper.ExtractShamsiYear(fiche.RayvarzActDate);
        if (yr <= 0)
            yr = DateHelper.ExtractShamsiYear(fiche.RayvarzDocDate);
        if (yr <= 0)
            yr = DateHelper.CurrentShamsiYear();

        var docTyp = rayMeta?.DocTyp ?? fiche.DocTyp;
        if (docTyp <= 0)
            docTyp = fiche.Category is FicheCategory.DutySenfi ? 2 : fiche.Category is FicheCategory.DutyNosazi ? 1 : 3;

        var docSeq = FirstNonEmpty(pursuitDocNo, rayMeta is { Doc: > 0 } ? rayMeta.Doc.ToString() : null) ?? "0";
        return $"{prefix};{branch};{yr};{docTyp};{docSeq}";
    }

    private static byte ResolveObjOnPrice(FicheHeaderDto fiche) =>
        fiche.Category switch
        {
            FicheCategory.DutySenfi => ObjOnPriceSenfi,
            FicheCategory.DutyNosazi => ObjOnPriceNosazi,
            _ => ObjOnPriceIncome
        };

    /// <summary>Member 1388: درآمد PhasType=7؛ نوسازی/صنفی DocRow=1.</summary>
    private static int ResolveDocRow(FicheHeaderDto fiche) =>
        fiche.Category is FicheCategory.DutyNosazi or FicheCategory.DutySenfi ? 1 : PhasTypeRayvarz;

    private static int ResolvePaymentDateCompact(FicheHeaderDto fiche)
    {
        var slash = FicheDateResolver.ResolvePaymentDateByStatus(
            fiche.CurrentStatus,
            DateHelper.ToShamsiSlashDate(fiche.RayvarzDocDate),
            DateHelper.ToShamsiSlashDate(fiche.RayvarzActDate));

        if (string.IsNullOrWhiteSpace(slash))
            slash = DateHelper.ToShamsiSlashDate(fiche.RayvarzActDate);
        if (string.IsNullOrWhiteSpace(slash))
            slash = DateHelper.CurrentShamsiSlashDate();

        var digits = DateHelper.ToRayvarzDate(slash);
        return int.TryParse(digits, out var compact) ? compact : 0;
    }

    private static string ResolveBankCode(FicheHeaderDto fiche)
    {
        var code = FirstNonEmpty(fiche.PaymentBranch, fiche.BankCode)?.Trim();
        return string.IsNullOrWhiteSpace(code) ? "18" : code;
    }

    private static string FirstSegment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";
        var trimmed = value.Trim();
        var slash = trimmed.IndexOf('/');
        return slash >= 0 ? trimmed[..slash] : trimmed;
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var v in values)
        {
            if (!string.IsNullOrWhiteSpace(v))
                return v.Trim();
        }

        return null;
    }
}

public sealed class RayvarzDocMeta
{
    public int Branch { get; init; }
    public int Yr { get; init; }
    public int DocTyp { get; init; }
    public int Doc { get; init; }
    public int? Fund { get; init; }
}
