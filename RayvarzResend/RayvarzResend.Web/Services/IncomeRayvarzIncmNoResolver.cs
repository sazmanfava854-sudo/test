namespace RayvarzResend.Web.Services;

/// <summary>
/// نگاشت CI_IncomeCalculation → کد درآمد رایورز (IncmNo) — parity GetCI_IncomeCalculation.IncomeCode در VB.
/// </summary>
public static class IncomeRayvarzIncmNoResolver
{
    public static int Resolve(int ciIncomeCalculation, string? incomeCodeRaw)
    {
        if (IncomeExcludedCodes.Codes.Contains(ciIncomeCalculation))
            return 0;

        if (!string.IsNullOrWhiteSpace(incomeCodeRaw))
        {
            var code = incomeCodeRaw.Trim();
            if (code is "0" or "1")
                return 0;
            if (int.TryParse(code, out var parsed) && parsed > 0)
                return parsed;
        }

        return ciIncomeCalculation > 0 ? ciIncomeCalculation : 0;
    }

    public static bool IsMetaIncomeCode(string? incomeCodeRaw)
    {
        if (string.IsNullOrWhiteSpace(incomeCodeRaw))
            return false;
        var code = incomeCodeRaw.Trim();
        return code is "0" or "1";
    }
}
