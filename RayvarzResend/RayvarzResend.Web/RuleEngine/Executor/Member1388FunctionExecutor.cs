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

        if (IsHelper(functionName))
            return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: false);

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
        if (context.Fiche.Payable <= 0)
        {
            trace.Add("iNcOMECheck: مبلغ صفر");
            return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: false);
        }

        trace.Add("iNcOMECheck: OK");
        return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: true);
    }

    private static Member1388FunctionResult ExecuteBedeHi(DslExecutionContext context, List<string> trace)
    {
        context.Variables["BedeHiResult"] = 0m;
        trace.Add("BedeHi: 0");
        return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: false);
    }

    private static Member1388FunctionResult ExecuteIncomeFunction(
        string functionName,
        DslExecutionContext context,
        IOperationRegistry registry,
        List<string> trace)
    {
        var fiche = context.Fiche;
        IncomeRowScaler.ScaleToPayable(fiche.Rows, fiche.Payable);
        ApplyIncomeCentersForFunction(functionName, fiche);

        registry.Invoke("Income.BuildIncomeRows", context, Array.Empty<string>());
        RefParameterCollector.ApplyToFiche(context.Fiche, RefParameterCollector.GetOrCreateList(context));
        context.Rows.Clear();
        context.Rows.AddRange(context.Fiche.Rows);
        trace.Add($"{functionName}: income rows + centers");
        return Member1388FunctionResult.Handled(trace, skipBody: true, hadEffect: true);
    }

    private static void ApplyIncomeCentersForFunction(string functionName, FicheHeaderDto fiche)
    {
        if (functionName.Equals("IncomeHoushmand", StringComparison.OrdinalIgnoreCase))
        {
            fiche.ResolvedDistrictBranch = 682;
            return;
        }

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
        || name.Equals("Logfile", StringComparison.OrdinalIgnoreCase)
        || name.Equals("Run", StringComparison.OrdinalIgnoreCase);
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
