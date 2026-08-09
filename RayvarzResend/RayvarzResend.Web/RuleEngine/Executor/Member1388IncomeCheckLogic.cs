using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>اعتبارسنجی iNcOMECheck — VB member-1388-full-body.vb خطوط ۴۸۸۵–۴۹۶۲.</summary>
public static class Member1388IncomeCheckLogic
{
    private const string ExportWindowStart = "1399/10/22";
    private const string ExportWindowEnd = "1399/12/11";
    private const int MaxCashPaymentDays = 10;

    public static IncomeCheckResult Validate(FicheHeaderDto fiche, DslExecutionContext context)
    {
        context.Variables["ExistRayvarz"] = false;

        if (fiche.Category != FicheCategory.Income)
        {
            return IncomeCheckResult.Skipped("iNcOMECheck: نوع فیش Income نیست");
        }

        var status = fiche.CurrentStatus;
        if (status < 5 && status != 3)
        {
            return IncomeCheckResult.Blocked(
                "فیش تایید نشده است اطلاعات به رایورز ارسال نمیشود");
        }

        var bankPaymentDate = ResolveBankPaymentDate(fiche);
        if (string.IsNullOrWhiteSpace(bankPaymentDate) && fiche.Payable > 0)
        {
            return IncomeCheckResult.Blocked(
                "فیش تایید بانک نشده است اطلاعات به رایورز ارسال نمیشود");
        }

        if (string.IsNullOrWhiteSpace(bankPaymentDate) && fiche.Payable <= 0)
        {
            var today = ResolveCurrentShamsiDate(context);
            fiche.BankPaymentDate = today;
            fiche.PaymentDate = today;
            fiche.RayvarzActDate = today;
            bankPaymentDate = today;
        }

        Member1388HelperFunctions.GetSara8Workflow(fiche.NidProc, context);
        SeedDistrictBranch(context, fiche);

        if (fiche.Payable > 0)
        {
            var installmentError = ValidateInstallmentPaymentWindow(fiche, status, bankPaymentDate, context);
            if (installmentError is not null)
                return IncomeCheckResult.Blocked(installmentError);
        }

        fiche.CanSend = true;
        fiche.BlockReason = null;
        return IncomeCheckResult.Passed();
    }

    private static string? ValidateInstallmentPaymentWindow(
        FicheHeaderDto fiche,
        int status,
        string bankPaymentDate,
        DslExecutionContext context)
    {
        var exportDate = NormalizeDate(fiche.ExportPermanentDate);
        if (string.IsNullOrWhiteSpace(exportDate))
            return null;

        if (IsWithinLegacyExportWindow(exportDate))
        {
            var deadline = (string)Member1388HelperFunctions.AddDateForHolidays(
                exportDate, MaxCashPaymentDays, aa: 1, context.HolidayCalendar);
            if (string.CompareOrdinal(deadline, bankPaymentDate) < 0)
            {
                return "با توجه به اینکه فیش نقد خارج از مهلت پرداخت شده است امکان ارسال  اطلاعات تقسیط به سامانه مالی وجود ندارد ";
            }

            return null;
        }

        if (status == 5)
        {
            var molat = string.IsNullOrWhiteSpace(fiche.PaymentBreakDate)
                ? (string)Member1388HelperFunctions.AddDateForHolidays(
                    exportDate, MaxCashPaymentDays, aa: 1, context.HolidayCalendar)
                : NormalizeDate(fiche.PaymentBreakDate);

            if (string.CompareOrdinal(bankPaymentDate, molat) > 0)
            {
                return "با توجه به اینکه فیش نقد خارج از مهلت پرداخت شده است امکان ارسال  اطلاعات تقسیط به سامانه مالی وجود ندارد ";
            }

            return null;
        }

        var dayDiff = Member1388HelperFunctions.GetDiffDate(exportDate, bankPaymentDate, mood: 3);
        if (dayDiff > MaxCashPaymentDays)
        {
            return "با توجه به اینکه فیش نقد خارج از مهلت پرداخت شده است امکان ارسال  اطلاعات تقسیط به سامانه مالی وجود ندارد ";
        }

        return null;
    }

    private static bool IsWithinLegacyExportWindow(string exportDate) =>
        string.CompareOrdinal(exportDate, ExportWindowStart) > 0
        && string.CompareOrdinal(exportDate, ExportWindowEnd) < 0;

    private static string ResolveBankPaymentDate(FicheHeaderDto fiche)
    {
        if (!string.IsNullOrWhiteSpace(fiche.BankPaymentDate))
            return NormalizeDate(fiche.BankPaymentDate);
        if (!string.IsNullOrWhiteSpace(fiche.RayvarzActDate))
            return NormalizeDate(fiche.RayvarzActDate);
        return "";
    }

    private static string ResolveCurrentShamsiDate(DslExecutionContext context)
    {
        if (context.Variables.TryGetValue("CurrentShamsiDate", out var value)
            && value is string s
            && !string.IsNullOrWhiteSpace(s))
            return s;

        var now = DateTime.Now;
        return $"{now.Year}/{now.Month:00}/{now.Day:00}";
    }

    private static void SeedDistrictBranch(DslExecutionContext context, FicheHeaderDto fiche)
    {
        var branch = Member1388IncomeCenterResolver.ResolveDistrictBranch(fiche);
        if (branch > 0)
        {
            fiche.ResolvedDistrictBranch ??= branch;
            context.Variables["DistrickBranch"] = branch;
        }
    }

    private static string NormalizeDate(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "" : value.Trim().Replace('-', '/');
}

public sealed class IncomeCheckResult
{
    public bool Success { get; init; }
    public bool HadEffect { get; init; }
    public string? BlockReason { get; init; }
    public string? TraceMessage { get; init; }

    public static IncomeCheckResult Passed() =>
        new() { Success = true, HadEffect = true, TraceMessage = "iNcOMECheck: OK" };

    public static IncomeCheckResult Blocked(string reason) =>
        new() { Success = false, HadEffect = false, BlockReason = reason, TraceMessage = $"iNcOMECheck: {reason}" };

    public static IncomeCheckResult Skipped(string message) =>
        new() { Success = true, HadEffect = false, TraceMessage = message };
}
