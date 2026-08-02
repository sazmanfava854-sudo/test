using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>ثبت operationهای فاز ۳ — رفتار تقلیدی Sara/nosazo.vb (C# baseline v16).</summary>
public static class SaraOperationBootstrap
{
    public static OperationRegistry CreateDefault()
    {
        var registry = new OperationRegistry();
        RegisterCommunication(registry);
        RegisterInfo8(registry);
        RegisterAccounting(registry);
        RegisterDuty(registry);
        RegisterDistrict(registry);
        RegisterDate(registry);
        RegisterFiche(registry);
        RegisterRef(registry);
        RegisterNosazi(registry);
        RegisterIncome(registry);
        RegisterValidation(registry);
        RegisterCollectionNoOps(registry);
        return registry;
    }

    private static void RegisterCollectionNoOps(OperationRegistry r)
    {
        OperationHandler addNoOp = (ctx, _) =>
        {
            ctx.Variables["collectionAddSkipped"] = true;
            return null;
        };

        // صریح — از XmlBody واقعی Member 1388
        r.Register("TmpAccounting_DocDetailsList.Add", addNoOp);
        r.Register("ListRefP.Add", addNoOp);
        r.Register("ListRefP.add", addNoOp);
        r.Register("ListAcc.Add", addNoOp);
        r.Register("ListAcc.add", addNoOp);
        r.Register("PParamName.Add", addNoOp);
        r.Register("PParamValue.Add", addNoOp);
    }

    private static void RegisterCommunication(OperationRegistry r)
    {
        r.Register("Biz.Communication.DutyFicheResultListCount", (ctx, _) =>
            ctx.Fiche.Category is FicheCategory.DutyNosazi or FicheCategory.DutySenfi ? 1 : 0);
        r.Register("Communication.DutyFicheResultListCount", (ctx, _) =>
            ctx.Fiche.Category is FicheCategory.DutyNosazi or FicheCategory.DutySenfi ? 1 : 0);

        r.Register("Biz.Communication.IncomeFicheResultListCount", (ctx, _) =>
            ctx.Fiche.Category == FicheCategory.Income ? 1 : 0);
        r.Register("Communication.IncomeFicheResultListCount", (ctx, _) =>
            ctx.Fiche.Category == FicheCategory.Income ? 1 : 0);
    }

    private static void RegisterInfo8(OperationRegistry r)
    {
        r.Register("Info8.GetAccountingDocCreateParameter", (ctx, _) =>
        {
            ctx.Variables["param"] = new { ctx.Fiche.FicheNo, ctx.Fiche.Payable };
            return ctx.Variables["param"];
        });

        // VB error channel — در DryRun فقط لاگ/متغیر
        OperationHandler addError = (ctx, args) =>
        {
            var msg = args.Count > 0 ? args[0] : "";
            ctx.Variables["lastInfo8Error"] = msg;
            return null;
        };
        r.Register("Info8.AddError", addError);
        r.Register("Info8.addError", addError);
        r.Register("Info8.ClearError", (ctx, _) =>
        {
            ctx.Variables.Remove("lastInfo8Error");
            return null;
        });
    }

    private static void RegisterAccounting(OperationRegistry r)
    {
        OperationHandler saveHandler = (ctx, _) =>
        {
            if (ctx.DryRun)
            {
                ctx.Variables["saveSkipped"] = true;
                ctx.Variables["result"] = "DryRun-SaveSkipped";
                return ctx.Variables["result"];
            }

            throw new InvalidOperationException("ClsAccounting.Save فقط در حالت غیر DryRun مجاز است.");
        };

        r.Register("ClsAccounting.Save", saveHandler);
        r.Register("TmpDocument.Save", saveHandler);
    }

    private static void RegisterDuty(OperationRegistry r)
    {
        r.Register("Duty.CalculateSubAmounts", (ctx, _) =>
        {
            if (ctx.Variables.TryGetValue("dutySubs", out var subsObj)
                && subsObj is IReadOnlyList<(int Formula, int Fiche, decimal Price)> subs)
            {
                return DutyNosaziLogic.CalculateSubAmounts(subs, ctx.Fiche.Payable);
            }

            return DutyNosaziLogic.CalculateSubAmounts(Array.Empty<(int, int, decimal)>(), ctx.Fiche.Payable);
        });

        r.Register("Duty.BuildIncmRows", (ctx, args) =>
        {
            var amounts = ctx.Variables.TryGetValue("amounts", out var a) && a is DutyNosaziLogic.DutySubAmounts d
                ? d
                : DutyNosaziLogic.CalculateSubAmounts(Array.Empty<(int, int, decimal)>(), ctx.Fiche.Payable);

            var isSenfi = ctx.Fiche.Category == FicheCategory.DutySenfi;
            var exportType = ctx.Fiche.DutyExportType ?? 0;
            if (args.Count > 0 && bool.TryParse(args[0], out var senfiArg))
                isSenfi = senfiArg;
            if (args.Count > 1 && int.TryParse(args[1], out var exportArg))
                exportType = exportArg;

            var rows = DutyNosaziLogic.BuildIncmRows(amounts, isSenfi, exportType);
            ctx.Rows.Clear();
            ctx.Rows.AddRange(rows);
            return rows;
        });

        r.Register("Duty.ApplyRayvarzDates", (ctx, args) =>
        {
            var status = args.Count > 0 && int.TryParse(args[0], out var s) ? s : ctx.Fiche.CurrentStatus;
            var payment = args.Count > 1 ? args[1] : ctx.Fiche.RayvarzActDate;
            var bank = args.Count > 2 ? args[2] : ctx.Fiche.RayvarzDocDate;
            DutyNosaziLogic.ApplyRayvarzDates(ctx.Fiche, status, payment, bank);
            return null;
        });

        r.Register("Duty.NormalizeMergedId", (_, args) =>
            DutyNosaziLogic.NormalizeMergedId(args.Count > 0 ? args[0] : null));

        r.Register("Duty.DefaultBankCode", (_, args) =>
            DutyNosaziLogic.DefaultBankCode(args.Count > 0 ? args[0] : null));

        r.Register("Duty.IsSenfiObjOnPrice", (_, args) =>
        {
            var dutyType = args.Count > 0 && int.TryParse(args[0], out var t) ? t : 0;
            return DutyNosaziLogic.IsSenfiObjOnPrice(dutyType);
        });
    }

    private static void RegisterDistrict(OperationRegistry r)
    {
        r.Register("District.ResolveBranch", (ctx, args) =>
        {
            var bill = args.Count > 0 ? args[0] : ctx.Fiche.BillIdRaw;
            var payment = args.Count > 1 ? args[1] : ctx.Fiche.PaymentIdRaw;
            return DutyDistrictBranchResolver.ResolveBranch(bill, payment);
        });

        r.Register("District.ResolveFund", (ctx, args) =>
        {
            var branch = args.Count > 0 && int.TryParse(args[0], out var b)
                ? b
                : ctx.Fiche.ResolvedDistrictBranch ?? ctx.Branch;
            var bank = args.Count > 1 ? args[1] : ctx.Fiche.BankCode ?? "18";
            return DutyDistrictBranchResolver.ResolveFund(branch, bank);
        });
    }

    private static void RegisterDate(OperationRegistry r)
    {
        r.Register("Date.CurrentShamsiRayvarz", (_, _) => DateHelper.CurrentShamsiRayvarzDate());
        r.Register("Date.FirstRayvarzDate", (_, args) =>
        {
            var a = args.Count > 0 ? args[0] : "";
            var b = args.Count > 1 ? args[1] : "";
            return FicheDateResolver.FirstRayvarzDate(a, b);
        });
        r.Register("Date.ResolvePaymentDateRay", (_, args) =>
        {
            var status = args.Count > 0 && int.TryParse(args[0], out var s) ? s : 1;
            var payment = args.Count > 1 ? args[1] : "";
            var bank = args.Count > 2 ? args[2] : "";
            return DutyNosaziLogic.ResolvePaymentDateRay(status, payment, bank);
        });
    }

    private static void RegisterFiche(OperationRegistry r)
    {
        r.Register("Fiche.GetPayable", (ctx, _) => ctx.Fiche.Payable);
        r.Register("Fiche.GetExportType", (ctx, _) => ctx.Fiche.DutyExportType ?? 0);
        r.Register("Fiche.GetCategory", (ctx, _) => ctx.Fiche.Category.ToString());
        r.Register("Fiche.GetDutyStatus", (ctx, _) => ctx.Fiche.CurrentStatus);
        r.Register("Fiche.GetFicheNo", (ctx, _) => ctx.Fiche.FicheNo);
    }

    private static void RegisterRef(OperationRegistry r)
    {
        r.Register("Ref.NormalizeBillId", (ctx, _) =>
            DutyNosaziLogic.NormalizeMergedId(ctx.Fiche.BillIdRaw));
        r.Register("Ref.NormalizePaymentId", (ctx, _) =>
            DutyNosaziLogic.NormalizeMergedId(ctx.Fiche.PaymentIdRaw));
        r.Register("Ref.GetReconstructionNo", (ctx, _) => ctx.Fiche.RefReconstructionNo ?? "");
    }

    private static void RegisterNosazi(OperationRegistry r)
    {
        r.Register("Nosazi.BuildDutyRows", (ctx, _) =>
        {
            if (ctx.Fiche.Category is not (FicheCategory.DutyNosazi or FicheCategory.DutySenfi))
                throw new InvalidOperationException($"Nosazi.BuildDutyRows برای {ctx.Fiche.Category} مجاز نیست.");

            ctx.Rows.Clear();
            ctx.Rows.AddRange(ctx.Fiche.Rows);
            ctx.Variables["rowsBuilt"] = ctx.Rows.Count;
            return ctx.Rows;
        });
    }

    private static void RegisterIncome(OperationRegistry r)
    {
        OperationHandler buildIncome = (ctx, _) =>
        {
            if (ctx.Fiche.Category != FicheCategory.Income)
                throw new InvalidOperationException($"Income.BuildIncomeRows برای {ctx.Fiche.Category} مجاز نیست.");

            if (TahatorRowBuilder.IsTahatorFiche(ctx.Fiche) || ctx.Fiche.DocTyp is 14 or 15)
            {
                // تهاتر: ردیف منفی + Centers (اگر هنوز از Calculation باشند، دوباره بساز)
                if (ctx.Fiche.Rows.Count != 1
                    || ctx.Fiche.Rows[0].IncmNo is not (TahatorRowBuilder.IncmNoBank4 or TahatorRowBuilder.IncmNoOther))
                    TahatorRowBuilder.ApplyTahatorRows(ctx.Fiche);
            }
            else
            {
                // دفاعی: اگر ردیف‌ها هنوز ناخالص باشند، به Payable اسکیل کن (parity با SOAP)
                IncomeRowScaler.ScaleToPayable(ctx.Fiche.Rows, ctx.Fiche.Payable);
            }

            ctx.Rows.Clear();
            ctx.Rows.AddRange(ctx.Fiche.Rows);
            ctx.Variables["rowsBuilt"] = ctx.Rows.Count;
            ctx.Variables["incomeBuilt"] = true;
            return ctx.Rows;
        };

        r.Register("Income.BuildIncomeRows", buildIncome);
        r.Register("iNcOME.BuildIncomeRows", buildIncome);
        r.Register("iNcOME.BuildRows", buildIncome);
    }

    private static void RegisterValidation(OperationRegistry r)
    {
        r.Register("Validate.RowSumEqualsPayable", (ctx, _) =>
        {
            var sum = ctx.Rows.Count > 0 ? ctx.Rows.Sum(x => x.Val) : ctx.Fiche.Rows.Sum(x => x.Val);
            if (!TahatorRowBuilder.RowSumMatchesPayable(ctx.Fiche, sum))
                throw new InvalidOperationException($"جمع ردیف‌ها ({sum}) ≠ Payable ({ctx.Fiche.Payable})");
            return true;
        });
    }
}
