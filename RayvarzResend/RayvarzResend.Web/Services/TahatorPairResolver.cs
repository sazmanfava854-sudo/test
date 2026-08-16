namespace RayvarzResend.Web.Services;

/// <summary>انتخاب جفت فعال ۱۵۷+۱۵۸ وقتی چند نسخه روی یک NidIncome وجود دارد.</summary>
public static class TahatorPairResolver
{
    public sealed record Candidate(
        string FicheNo,
        int IncomeAccountGroup,
        int EumFicheStatus,
        Guid NidExportation,
        decimal Payable);

    public static (string AmountFicheNo, string IncomeFicheNo)? Resolve(
        IReadOnlyList<Candidate> candidates,
        string inputFicheNo,
        int inputGroup,
        Guid inputExportation,
        decimal inputPayable)
    {
        if (candidates.Count == 0) return null;

        var active = candidates.Where(c => c.EumFicheStatus != 4).ToList();
        if (active.Count == 0) return null;

        var anchor = active.FirstOrDefault(c =>
            string.Equals(c.FicheNo, inputFicheNo, StringComparison.Ordinal));
        if (anchor == null) return null;

        string? amountNo;
        string? incomeNo;

        if (inputGroup == TahatorRowBuilder.IncomeAccountGroupTahatorAmount)
        {
            amountNo = anchor.FicheNo;
            incomeNo = FindPartner(active, TahatorRowBuilder.IncomeAccountGroupTahatorIncome, anchor);
        }
        else if (inputGroup == TahatorRowBuilder.IncomeAccountGroupTahatorIncome)
        {
            incomeNo = anchor.FicheNo;
            amountNo = FindPartner(active, TahatorRowBuilder.IncomeAccountGroupTahatorAmount, anchor);
        }
        else
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(amountNo) || string.IsNullOrWhiteSpace(incomeNo))
            return null;

        return (amountNo, incomeNo);
    }

    private static string? FindPartner(
        IReadOnlyList<Candidate> active,
        int partnerGroup,
        Candidate anchor)
    {
        var partners = active.Where(c => c.IncomeAccountGroup == partnerGroup).ToList();
        if (partners.Count == 0) return null;

        if (anchor.NidExportation != Guid.Empty)
        {
            var byExport = partners
                .Where(c => c.NidExportation == anchor.NidExportation)
                .OrderByDescending(c => c.FicheNo, StringComparer.Ordinal)
                .FirstOrDefault();
            if (byExport != null) return byExport.FicheNo;
        }

        var byPayable = partners
            .Where(c => c.Payable == anchor.Payable)
            .OrderByDescending(c => c.FicheNo, StringComparer.Ordinal)
            .FirstOrDefault();
        if (byPayable != null) return byPayable.FicheNo;

        return partners.OrderByDescending(c => c.FicheNo, StringComparer.Ordinal).First().FicheNo;
    }
}
