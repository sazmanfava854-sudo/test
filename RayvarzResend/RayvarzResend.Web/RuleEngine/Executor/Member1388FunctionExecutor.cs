using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Parser;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>اجرای منطق هر تابع Member 1388 — به‌جای defer بدنه VB.</summary>
public static class Member1388FunctionExecutor
{
    public static Member1388FunctionResult Execute(
        string functionName,
        DslExecutionContext context,
        IOperationRegistry registry)
    {
        var fiche = context.Fiche;
        var trace = new List<string> { $"Member1388.Execute({functionName})" };

        if (!Member1388Catalog.IsCatalogFunction(functionName))
            return Member1388FunctionResult.NotHandled(trace);

        context.Variables["ExistRayvarz"] = fiche.ExistsInRayvarz;
        SeedDistrictBranch(context, fiche);

        if (functionName.Equals("Nosazi", StringComparison.OrdinalIgnoreCase))
            return ExecuteNosazi(context, registry, trace);

        if (functionName.Equals("iNcOMECheck", StringComparison.OrdinalIgnoreCase))
            return ExecuteIncomeCheck(context, trace);

        if (SupportedDslFunctions.IsTahator(functionName))
            return ExecuteTahator(functionName, context, trace);

        if (functionName.Equals("BedeHi", StringComparison.OrdinalIgnoreCase))
            return ExecuteBedeHi(context, trace);

        if (functionName.Equals("Run", StringComparison.OrdinalIgnoreCase))
            return ExecuteRun(context, registry, trace);

        if (IsHelper(functionName))
            return ExecuteHelper(functionName, context, trace);

        if (fiche.Category != FicheCategory.Income)
            return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: false);

        if (!Member1388AccountGroupRules.AppliesToFiche(functionName, fiche))
        {
            trace.Add(Member1388AccountGroupRules.SkipReason(functionName, fiche));
            return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: false);
        }

        context.Variables["ExistRayvarz"] = false;
        return ExecuteIncomeFunction(functionName, context, registry, trace);
    }

    private static Member1388FunctionResult ExecuteNosazi(
        DslExecutionContext context,
        IOperationRegistry registry,
        List<string> trace)
    {
        if (context.Fiche.Category is not (FicheCategory.DutyNosazi or FicheCategory.DutySenfi))
        {
            trace.Add("Nosazi: نوع فیش Duty نیست");
            return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: false);
        }

        context.Variables["ExistRayvarz"] = false;
        registry.Invoke("Nosazi.BuildDutyRows", context, Array.Empty<string>());
        RefParameterCollector.ApplyToFiche(context.Fiche, RefParameterCollector.GetOrCreateList(context));
        trace.Add("Nosazi: BuildDutyRows + RefParams");
        return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: true);
    }

    private static Member1388FunctionResult ExecuteTahator(
        string functionName,
        DslExecutionContext context,
        List<string> trace)
    {
        if (!Member1388AccountGroupRules.AppliesToFiche(functionName, context.Fiche))
        {
            trace.Add(Member1388AccountGroupRules.SkipReason(functionName, context.Fiche));
            return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: false);
        }

        if (functionName.Equals("Tahator1", StringComparison.OrdinalIgnoreCase))
            TahatorRowBuilder.ApplyTahatorAmountRows(context.Fiche);
        else
            TahatorRowBuilder.ApplyTahatorIncomeRows(context.Fiche);

        context.Rows.Clear();
        context.Rows.AddRange(context.Fiche.Rows);
        RefParameterCollector.ApplyToFiche(context.Fiche, RefParameterCollector.GetOrCreateList(context));
        trace.Add($"{functionName}: TahatorRowBuilder");
        return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: true);
    }

    private static Member1388FunctionResult ExecuteIncomeCheck(DslExecutionContext context, List<string> trace)
    {
        var result = Member1388IncomeCheckLogic.Validate(context.Fiche, context);
        trace.Add(result.TraceMessage ?? "iNcOMECheck");

        if (!result.Success && result.BlockReason is not null)
        {
            context.Fiche.CanSend = false;
            context.Fiche.BlockReason = result.BlockReason;
            context.ValidationErrors.Add(result.BlockReason);
        }

        return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: result.HadEffect);
    }

    private static Member1388FunctionResult ExecuteRun(
        DslExecutionContext context,
        IOperationRegistry registry,
        List<string> trace)
    {
        var hadEffect = Member1388HelperFunctions.DispatchRun(context, registry, trace);
        return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: hadEffect);
    }

    private static Member1388FunctionResult ExecuteHelper(
        string functionName,
        DslExecutionContext context,
        List<string> trace)
    {
        if (functionName.Equals("ChangeDate", StringComparison.OrdinalIgnoreCase))
        {
            var d1 = GetHelperArg(context, 0) ?? "";
            var result = Member1388HelperFunctions.ChangeDate(d1);
            context.Variables["ChangeDateResult"] = result;
            trace.Add($"ChangeDate: {d1} → {result}");
            return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: result.Length > 0);
        }

        if (functionName.Equals("GetSara8Workflow", StringComparison.OrdinalIgnoreCase))
        {
            var group = Member1388HelperFunctions.GetSara8Workflow(context.Fiche.NidProc, context);
            trace.Add($"GetSara8Workflow: group={group}");
            return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: group > 0);
        }

        if (functionName.Equals("GetDiffDate", StringComparison.OrdinalIgnoreCase))
        {
            var date1 = GetHelperArg(context, 0) ?? "";
            var date2 = GetHelperArg(context, 1) ?? "";
            var mood = int.TryParse(GetHelperArg(context, 2), out var m) ? m : 0;
            var diff = Member1388HelperFunctions.GetDiffDate(date1, date2, mood);
            context.Variables["GetDiffDateResult"] = diff;
            trace.Add($"GetDiffDate: {diff}");
            return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: true);
        }

        if (functionName.Equals("AddDateForHolidays", StringComparison.OrdinalIgnoreCase))
        {
            var date = GetHelperArg(context, 0) ?? "";
            var days = int.TryParse(GetHelperArg(context, 1), out var d) ? d : 0;
            var aa = int.TryParse(GetHelperArg(context, 2), out var a) ? a : 0;
            var result = Member1388HelperFunctions.AddDateForHolidays(
                date, days, aa, context.HolidayCalendar);
            context.Variables["AddDateForHolidaysResult"] = result;
            trace.Add($"AddDateForHolidays: {result}");
            return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: true);
        }

        if (functionName.Equals("FnSMS", StringComparison.OrdinalIgnoreCase))
        {
            var text = GetHelperArg(context, 0) ?? "";
            Member1388HelperFunctions.FnSms(text, context);
            trace.Add("FnSMS: traced");
            return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: !string.IsNullOrEmpty(text));
        }

        if (functionName.Equals("Logfile", StringComparison.OrdinalIgnoreCase))
        {
            var n1 = GetHelperArg(context, 0) ?? "";
            var n2 = GetHelperArg(context, 1) ?? "";
            Member1388HelperFunctions.Logfile(n1, n2, context);
            trace.Add($"Logfile: {n1}");
            return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: !string.IsNullOrEmpty(n1));
        }

        return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: false);
    }

    private static string? GetHelperArg(DslExecutionContext context, int index) =>
        context.Variables.TryGetValue($"HelperArg{index}", out var value) ? value?.ToString() : null;

    private static Member1388FunctionResult ExecuteBedeHi(DslExecutionContext context, List<string> trace)
    {
        var district = context.Variables.TryGetValue("DistrickBranch", out var branchObj)
                       && branchObj is int branchFromVar
            ? branchFromVar
            : Member1388IncomeCenterResolver.ResolveDistrictBranch(context.Fiche);

        var amount = Member1388OraghRowBuilder.ResolveBedeHiAmount(context.Fiche, district);
        context.Variables["BedeHiResult"] = amount;
        context.Fiche.PriorBedeHiAmount ??= amount;
        trace.Add($"BedeHi: {amount}");
        return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: amount > 0);
    }

    private static Member1388FunctionResult ExecuteIncomeFunction(
        string functionName,
        DslExecutionContext context,
        IOperationRegistry registry,
        List<string> trace)
    {
        var fiche = context.Fiche;
        ApplyIncomeRowsForFunction(functionName, fiche);
        ApplyIncomeCentersForFunction(functionName, fiche);

        registry.Invoke("Income.BuildIncomeRows", context, Array.Empty<string>());
        RefParameterCollector.ApplyToFiche(context.Fiche, RefParameterCollector.GetOrCreateList(context));
        context.Rows.Clear();
        context.Rows.AddRange(context.Fiche.Rows);
        trace.Add($"{functionName}: income rows + centers");
        return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: true);
    }

    private static void ApplyIncomeRowsForFunction(string functionName, FicheHeaderDto fiche)
    {
        if (functionName.Equals("iNcOMEOragh", StringComparison.OrdinalIgnoreCase))
        {
            Member1388OraghRowBuilder.Apply(fiche);
            return;
        }

        if (functionName.Equals("IncomeHoushmand", StringComparison.OrdinalIgnoreCase))
        {
            Member1388SpecialIncomeRowBuilder.ApplyHoushmand(fiche);
            return;
        }

        if (functionName.Equals("IncomeSrvElectronic", StringComparison.OrdinalIgnoreCase))
        {
            Member1388SpecialIncomeRowBuilder.ApplySrvElectronic(fiche);
            return;
        }

        IncomeRowScaler.ScaleToPayable(fiche.Rows, fiche.Payable);
    }

    private static void ApplyIncomeCentersForFunction(string functionName, FicheHeaderDto fiche)
    {
        if (functionName.Equals("IncomeHoushmand", StringComparison.OrdinalIgnoreCase)
            || functionName.Equals("IncomeSrvElectronic", StringComparison.OrdinalIgnoreCase))
            return;

        if (functionName.Equals("iNcOMEOragh", StringComparison.OrdinalIgnoreCase))
        {
            Member1388IncomeCenterResolver.ApplyOragh(fiche);
            return;
        }

        if (functionName.Equals("iNcOMEHavaleT", StringComparison.OrdinalIgnoreCase))
        {
            Member1388IncomeCenterResolver.ApplyHavaleT(fiche);
            return;
        }

        if (functionName.Equals("iNcOMEGhatar_Shahri", StringComparison.OrdinalIgnoreCase))
        {
            Member1388IncomeCenterResolver.ApplyGhatarShahri(fiche);
            return;
        }

        if (functionName.Equals("iNcOMESeprdeh", StringComparison.OrdinalIgnoreCase))
        {
            Member1388IncomeCenterResolver.ApplySeprdeh(fiche);
            return;
        }

        if (functionName.Equals("iNcOMEEshghal", StringComparison.OrdinalIgnoreCase)
            || functionName.Equals("iNcOMEBackSeprdeh", StringComparison.OrdinalIgnoreCase))
        {
            Member1388IncomeCenterResolver.ApplyEshghal(fiche);
            return;
        }

        if (functionName.Equals("BazAfarine", StringComparison.OrdinalIgnoreCase)
            || functionName.Equals("BazAfarineOld", StringComparison.OrdinalIgnoreCase))
        {
            Member1388IncomeCenterResolver.ApplyBazAfarine(fiche);
            return;
        }

        if (functionName.Equals("iNcOME", StringComparison.OrdinalIgnoreCase))
            ApplyStandardIncomeCenters(fiche);
    }

    private static void ApplyStandardIncomeCenters(FicheHeaderDto fiche)
    {
        Member1388IncomeCenterResolver.ApplyCenter1FromDeposit(fiche);

        var center3 = string.Equals(fiche.CheckNo?.Trim(), "5", StringComparison.Ordinal)
            ? TahatorRowBuilder.Center3CheckNo5
            : TahatorRowBuilder.Center3Default;
        foreach (var row in fiche.Rows)
            row.Center3 ??= center3;

        if (fiche.BankCode == "2" && fiche.CreditorPapers is > 0)
            fiche.Center = fiche.CreditorPapers;
    }

    private static void SeedDistrictBranch(DslExecutionContext context, FicheHeaderDto fiche)
    {
        if (fiche.ResolvedDistrictBranch is > 0)
        {
            context.Variables["DistrickBranch"] = fiche.ResolvedDistrictBranch.Value;
            return;
        }

        var branch = DutyDistrictBranchResolver.ResolveBranch(fiche.BillIdRaw, fiche.PaymentIdRaw);
        if (branch > 0)
        {
            fiche.ResolvedDistrictBranch = branch;
            context.Variables["DistrickBranch"] = branch;
        }
    }

    private static bool IsHelper(string name) =>
        name.Equals("ChangeDate", StringComparison.OrdinalIgnoreCase)
        || name.Equals("GetSara8Workflow", StringComparison.OrdinalIgnoreCase)
        || name.Equals("GetDiffDate", StringComparison.OrdinalIgnoreCase)
        || name.Equals("FnSMS", StringComparison.OrdinalIgnoreCase)
        || name.Equals("AddDateForHolidays", StringComparison.OrdinalIgnoreCase)
        || name.Equals("Logfile", StringComparison.OrdinalIgnoreCase);
}

public sealed class Member1388FunctionResult
{
    public bool WasHandled { get; init; }
    public bool SkipFunctionBody { get; init; }
    public bool HadEffect { get; init; }
    public IReadOnlyList<string> Trace { get; init; } = Array.Empty<string>();

    public static Member1388FunctionResult NotHandled(IReadOnlyList<string> trace) =>
        new() { WasHandled = false, Trace = trace };

    public static Member1388FunctionResult Handled(
        IReadOnlyList<string> trace,
        bool skipBody,
        bool hadEffect) =>
        new() { WasHandled = true, SkipFunctionBody = skipBody, HadEffect = hadEffect, Trace = trace };
}
