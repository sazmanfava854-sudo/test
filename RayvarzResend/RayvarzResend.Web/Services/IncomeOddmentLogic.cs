using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>سازگاری با مسیر قدیمی — منطق در RuleEngine.Executor.</summary>
public static class IncomeOddmentLogic
{
    public static void ApplyToRows(IList<IncmRowDto> rows, IReadOnlyList<IncomeOddmentDto> oddments, Guid? nidIncome) =>
        RuleEngine.Executor.IncomeOddmentLogic.ApplyToRows(rows, oddments, nidIncome);

    public static decimal ApplyNetAdjustment(decimal baseValue, IReadOnlyList<IncomeOddmentDto> oddments, int incmNo) =>
        RuleEngine.Executor.IncomeOddmentLogic.ApplyNetAdjustment(baseValue, oddments, incmNo);

    public static void AppendMissingOddmentRows(IList<IncmRowDto> rows, IReadOnlyList<IncomeOddmentDto> oddments) =>
        RuleEngine.Executor.IncomeOddmentLogic.AppendMissingOddmentRows(rows, oddments);
}
