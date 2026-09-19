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
        if (partners.Count == 1) return partners[0].FicheNo;

        // چند فیش ۱۵۷/۱۵۸ روی یک NidIncome: اول مبلغ یکسان، بعد NidExportation (نه برعکس).
        var pool = partners;
        if (anchor.Payable > 0)
        {
            var payableMatches = partners.Where(c => c.Payable == anchor.Payable).ToList();
            if (payableMatches.Count > 0)
                pool = payableMatches;
        }

        if (anchor.NidExportation != Guid.Empty)
        {
            var byExport = pool
                .Where(c => c.NidExportation == anchor.NidExportation)
                .OrderByDescending(c => c.FicheNo, StringComparer.Ordinal)
                .FirstOrDefault();
            if (byExport != null) return byExport.FicheNo;
        }

        return pool.OrderByDescending(c => c.FicheNo, StringComparer.Ordinal).First().FicheNo;
    }
}
