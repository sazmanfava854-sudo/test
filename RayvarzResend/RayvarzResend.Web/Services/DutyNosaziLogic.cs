using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>منطق Biz.Communication — تابع Nosazi() (فایل nosazo.vb).</summary>
public static class DutyNosaziLogic
{
    public sealed record DutySubAmounts(decimal Afzodeh, decimal Atash, decimal Garbage, decimal MainLine);

    /// <summary>همان محاسبات خطوط ۲۱۹–۲۵۲ nosazo.vb</summary>
    public static DutySubAmounts CalculateSubAmounts(
        IReadOnlyList<(int Formula, int Fiche, decimal Price)> subs,
        decimal payablePrice)
    {
        const int garbageFormula = 3;
        const int atashFormula = 5;
        const int afzodehFiche = 16;

        var afzodeh = subs.Where(s => s.Formula == garbageFormula && s.Fiche == afzodehFiche).Sum(s => s.Price);

        var atashF0 = subs.Where(s => s.Formula == atashFormula && s.Fiche == 0).Sum(s => s.Price);
        var atashNonZero = subs.Where(s => s.Formula == atashFormula && s.Fiche != 0).Sum(s => s.Price);
        var atash = atashF0 - atashNonZero;

        var garbageF0 = subs.Where(s => s.Formula == garbageFormula && s.Fiche == 0).Sum(s => s.Price);
        var garbageNonZero = subs.Where(s => s.Formula == garbageFormula && s.Fiche != 0).Sum(s => s.Price);
        var garbage = garbageF0 - garbageNonZero;

        var mainLine = payablePrice - atash - garbage - afzodeh;
        return new DutySubAmounts(afzodeh, atash, garbage, mainLine);
    }

    public static List<IncmRowDto> BuildIncmRows(
        DutySubAmounts amounts,
        bool isSenfi,
        int exportType)
    {
        var mainIncm = isSenfi switch
        {
            true when exportType == 14 => 2005,
            true => 100062,
            _ => 2003
        };
        var mainDsc = mainIncm switch
        {
            2005 => "عوارض ساليانه بانک ها و موسسات اعتباري",
            100062 => "صنفي",
            _ => "نوسازی"
        };

        var rows = new List<IncmRowDto>();
        if (amounts.MainLine != 0)
            rows.Add(new IncmRowDto { IncmNo = mainIncm, Val = amounts.MainLine, IncmRowDsc = mainDsc });
        if (amounts.Atash != 0)
            rows.Add(new IncmRowDto { IncmNo = 100002, Val = amounts.Atash, IncmRowDsc = "آتش نشانی" });
        if (amounts.Garbage != 0)
            rows.Add(new IncmRowDto { IncmNo = 100003, Val = amounts.Garbage, IncmRowDsc = "پسماند" });
        if (amounts.Afzodeh != 0)
            rows.Add(new IncmRowDto { IncmNo = 206098003, Val = amounts.Afzodeh, IncmRowDsc = "مالیات برارزش افزوده" });

        return rows;
    }

    /// <summary>TmpDate — وضعیت ۱: PaymentDate وگرنه BankPaymentDate (خط ۳۰۰–۳۰۴).</summary>
    public static string ResolvePaymentDateRay(int eumDutyFicheStatus, string paymentDateRay, string bankPaymentDateRay) =>
        eumDutyFicheStatus == 1
            ? FicheDateResolver.FirstRayvarzDate(paymentDateRay, bankPaymentDateRay)
            : FicheDateResolver.FirstRayvarzDate(bankPaymentDateRay, paymentDateRay);

    /// <summary>DocDate و Due = امروز شمسی؛ RowDate/ActDate = TmpDate (خط ۴۴۵–۴۵۰، ۵۳۵–۵۳۷).</summary>
    public static void ApplyRayvarzDates(FicheHeaderDto dto, int eumDutyFicheStatus, string paymentDateRay, string bankPaymentDateRay)
    {
        var tmpDate = ResolvePaymentDateRay(eumDutyFicheStatus, paymentDateRay, bankPaymentDateRay);
        var today = DateHelper.CurrentShamsiRayvarzDate();
        dto.RayvarzDocDate = today;
        dto.RayvarzDueDate = today;
        dto.RayvarzActDate = tmpDate;
        dto.RowDate = tmpDate;
    }

    /// <summary>BillID/PaymentID با / — ادغام دو بخش اول (خط ۲۷۸–۲۸۱).</summary>
    public static string NormalizeMergedId(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "";
        var parts = raw.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 2)
            return parts[0] + parts[1];
        return raw.Trim();
    }

    public static string DefaultBankCode(string? confirmBankCode) =>
        string.IsNullOrWhiteSpace(confirmBankCode) ? "18" : confirmBankCode.Trim();
}
