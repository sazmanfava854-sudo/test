using System.Security.Claims;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public class UnsentFicheService
{
    private readonly FicheRepository _repo;
    private readonly FicheSendService _send;
    private readonly TahatorResendService _tahator;
    private readonly IConfiguration _config;

    public UnsentFicheService(
        FicheRepository repo,
        FicheSendService send,
        TahatorResendService tahator,
        IConfiguration config)
    {
        _repo = repo;
        _send = send;
        _tahator = tahator;
        _config = config;
    }

    public async Task<UnsentFicheSearchResult> SearchAsync(UnsentFicheSearchRequest req, CancellationToken ct = default)
    {
        var result = req.FicheKind == UnsentFicheKind.Duty
            ? await _repo.SearchUnsentDutyAsync(req, ct)
            : await _repo.SearchUnsentIncomeAsync(req, ct);
        foreach (var item in result.Items)
            item.SourceKind = req.FicheKind;
        return result;
    }

    public async Task<UnsentBillPayLookupResult> LookupByBillPayAsync(
        UnsentBillPayLookupRequest req,
        CancellationToken ct = default)
    {
        var validation = UnsentBillPayLookupHelper.ValidateRequest(req);
        if (validation != null)
            return new UnsentBillPayLookupResult { Error = validation };

        var pairs = UnsentBillPayLookupHelper.NormalizePairs(req.Pairs);
        if (pairs.Count == 0)
            return new UnsentBillPayLookupResult { Error = "فایل اکسل باید حداقل یک ردیف با شناسه قبض و شناسه پرداخت معتبر داشته باشد" };

        var incomeItems = await _repo.FindUnsentByBillPayAsync(UnsentFicheKind.Income, pairs, ct);
        var incomeKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in incomeItems)
        {
            var key = UnsentBillPayLookupHelper.MatchKey(item.BillId, item.PaymentId);
            if (key.Length > 0)
                incomeKeys.Add(key);
        }

        var incomeFoundPairs = pairs.Where(p => incomeKeys.Contains(UnsentBillPayLookupHelper.PairKey(p))).ToList();
        var dutyConflictProbe = incomeFoundPairs.Count > 0
            ? await _repo.FindUnsentByBillPayAsync(UnsentFicheKind.Duty, incomeFoundPairs, ct)
            : [];

        var remainingPairs = pairs
            .Where(p => !incomeKeys.Contains(UnsentBillPayLookupHelper.PairKey(p)))
            .ToList();
        var dutyItems = remainingPairs.Count > 0
            ? await _repo.FindUnsentByBillPayAsync(UnsentFicheKind.Duty, remainingPairs, ct)
            : [];

        return UnsentBillPayLookupHelper.BuildMixedLookupResult(
            pairs,
            incomeItems,
            dutyConflictProbe,
            dutyItems);
    }

    private static List<UnsentBatchFicheTarget> ResolveBatchTargets(UnsentBatchSendRequest req)
    {
        if (req.Targets is { Count: > 0 })
        {
            return req.Targets
                .Where(t => !string.IsNullOrWhiteSpace(t.FicheNo))
                .Select(t => new UnsentBatchFicheTarget
                {
                    FicheNo = t.FicheNo.Trim(),
                    SourceKind = t.SourceKind
                })
                .ToList();
        }

        return (req.FicheNos ?? [])
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => new UnsentBatchFicheTarget
            {
                FicheNo = n.Trim(),
                SourceKind = req.FicheKind
            })
            .ToList();
    }

    public Task<UnsentBatchPlanResult> PlanBatchAsync(
        UnsentBatchSendRequest req,
        ClaimsPrincipal user,
        CancellationToken ct = default) =>
        BuildPlanAsync(req, user, ct);

    public async Task<UnsentBatchSendResult> SendBatchAsync(
        UnsentBatchSendRequest req,
        ClaimsPrincipal user,
        CancellationToken ct = default)
    {
        var dryRun = _config.GetValue<bool>("Rayvarz:DryRun");
        var delayMs = _config.GetValue("Rayvarz:SendDelayMs", 2000);
        var targets = ResolveBatchTargets(req);
        var result = new UnsentBatchSendResult
        {
            DryRun = dryRun,
            Total = targets.Count
        };

        if (targets.Count == 0)
            return result;

        var index = 0;

        foreach (var target in targets.GroupBy(t => t.FicheNo, StringComparer.Ordinal).Select(g => g.First()))
        {
            var ficheNo = target.FicheNo;
            if (string.IsNullOrWhiteSpace(ficheNo))
                continue;

            if (index > 0 && delayMs > 0 && !dryRun)
                await Task.Delay(delayMs, ct);

            var item = new UnsentBatchSendItemResult { FicheNo = ficheNo };
            result.Results.Add(item);
            index++;

            try
            {
                var outcome = await ProcessOneAsync(req, target, user, ct);
                item.SendPath = outcome.SendPath;
                item.Success = outcome.Success;
                item.Skipped = outcome.Skipped;
                item.SkipReason = outcome.SkipReason;
                item.Message = EpayFichePresenceChecker.MapBatchMessage(outcome.Message);
                item.BillId = outcome.BillId;
                item.PaymentId = outcome.PaymentId;
                item.VerifiedInRayvarz = outcome.VerifiedInRayvarz;
                item.DocNotSentError = outcome.DocNotSentError;

                if (outcome.Skipped)
                    result.Skipped++;
                else if (outcome.Success)
                    result.Succeeded++;
                else
                    result.Failed++;
            }
            catch (Exception ex)
            {
                item.Message = EpayFichePresenceChecker.MapBatchMessage(ex.Message);
                result.Failed++;
            }
        }

        return result;
    }

    private async Task<UnsentBatchPlanResult> BuildPlanAsync(
        UnsentBatchSendRequest req,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var targets = ResolveBatchTargets(req);
        var plan = new UnsentBatchPlanResult { Total = targets.Count };
        if (targets.Count == 0)
            return plan;

        foreach (var target in targets.GroupBy(t => t.FicheNo, StringComparer.Ordinal).Select(g => g.First()))
        {
            if (string.IsNullOrWhiteSpace(target.FicheNo))
                continue;

            plan.Items.Add(await PlanOneAsync(req, target, user, ct));
        }

        return plan;
    }

    private async Task<UnsentBatchPlanItem> PlanOneAsync(
        UnsentBatchSendRequest req,
        UnsentBatchFicheTarget target,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var ficheNo = target.FicheNo;
        var sourceKind = target.SourceKind;
        var item = new UnsentBatchPlanItem { FicheNo = ficheNo };

        if (await _repo.ExistsInAccountingDocHeaderAsync(ficheNo, ct))
        {
            item.SendPath = "Skip";
            item.Detail = "Accounting_DocHeader";
            item.BlockReason = "فیش در Accounting_DocHeader موجود است";
            return item;
        }

        var fiche = await _repo.LoadAsync(IdentifierType.FicheNo, ficheNo, ct);
        if (fiche == null)
        {
            item.SendPath = "Skip";
            item.BlockReason = "فیش یافت نشد";
            return item;
        }

        var districtDenied = DistrictAccessService.GetAccessDeniedMessage(user, fiche);
        if (districtDenied != null)
        {
            item.SendPath = "Skip";
            item.BlockReason = districtDenied;
            return item;
        }

        item.BillId = fiche.BillIdRaw;
        item.PaymentId = fiche.PaymentIdRaw;
        if (string.IsNullOrWhiteSpace(item.BillId))
            item.BillId = fiche.BillId;
        if (string.IsNullOrWhiteSpace(item.PaymentId))
            item.PaymentId = fiche.PaymentId;

        if (sourceKind == UnsentFicheKind.Income && fiche.Category != FicheCategory.Income)
        {
            item.SendPath = "Skip";
            item.BlockReason = "نوع فیش با درآمد (شهرسازی/تهاتر) مطابقت ندارد";
            return item;
        }

        if (sourceKind == UnsentFicheKind.Duty
            && fiche.Category is not (FicheCategory.DutyNosazi or FicheCategory.DutySenfi))
        {
            item.SendPath = "Skip";
            item.BlockReason = "نوع فیش با نوسازی/صنفی مطابقت ندارد";
            return item;
        }

        if (TahatorRowBuilder.IsTahatorFiche(fiche))
        {
            item.SendPath = "Tahator";
            item.Detail = fiche.IncomeAccountGroup == TahatorRowBuilder.IncomeAccountGroupTahatorAmount
                ? "تهاتر مبلغ — فقط همین فیش"
                : "تهاتر درآمد — فقط همین فیش";
            item.CanSend = true;
            return item;
        }

        var validation = FicheSendService.ValidateSendable(fiche);
        if (validation != null)
        {
            item.SendPath = sourceKind == UnsentFicheKind.Duty ? "Duty" : "Income";
            item.BlockReason = validation;
            return item;
        }

        if (!FicheBranchResolver.TryResolve(fiche, out _, out _, out var branchError))
        {
            item.SendPath = sourceKind == UnsentFicheKind.Duty ? "Duty" : "Income";
            item.BlockReason = branchError;
            return item;
        }

        item.SendPath = sourceKind == UnsentFicheKind.Duty ? "Duty" : "Income";
        item.Detail = sourceKind == UnsentFicheKind.Duty ? "ارسال نوسازی/صنفی" : "ارسال درآمدی شهرسازی";
        item.CanSend = true;
        return item;
    }

    private sealed record ProcessOutcome(
        string SendPath,
        bool Success,
        bool Skipped,
        string Message,
        string? SkipReason = null,
        bool VerifiedInRayvarz = false,
        string? DocNotSentError = null,
        string BillId = "",
        string PaymentId = "");

    private async Task<ProcessOutcome> ProcessOneAsync(
        UnsentBatchSendRequest req,
        UnsentBatchFicheTarget target,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var plan = await PlanOneAsync(req, target, user, ct);
        var ficheNo = target.FicheNo;
        if (!plan.CanSend)
        {
            return new ProcessOutcome(
                plan.SendPath,
                Success: false,
                Skipped: true,
                Message: plan.BlockReason ?? "رد شد",
                SkipReason: plan.SendPath,
                BillId: plan.BillId,
                PaymentId: plan.PaymentId);
        }

        if (plan.SendPath == "Tahator")
        {
            var tahResult = await _tahator.SendAsync(new TahatorFicheRequest { FicheNo = ficheNo }, ct);
            return new ProcessOutcome(
                "Tahator",
                tahResult.Success,
                tahResult.Skipped,
                tahResult.Message ?? (tahResult.Success ? "ارسال تهاتر موفق" : "ارسال تهاتر ناموفق"),
                tahResult.SkipReason,
                DocNotSentError: tahResult.DocNotSentError,
                BillId: plan.BillId,
                PaymentId: plan.PaymentId);
        }

        var fiche = await _repo.LoadAsync(IdentifierType.FicheNo, ficheNo, ct);
        if (fiche == null)
            return new ProcessOutcome(plan.SendPath, false, true, "فیش یافت نشد", "NotFound", BillId: plan.BillId, PaymentId: plan.PaymentId);

        if (!FicheBranchResolver.TryResolve(fiche, out var branch, out var fund, out var branchError))
            return new ProcessOutcome(plan.SendPath, false, true, branchError ?? FicheBranchResolver.RegionNotResolvedMessage, "RegionUnresolved", BillId: plan.BillId, PaymentId: plan.PaymentId);

        var sendReq = new SendFicheRequest
        {
            Fiche = fiche,
            Branch = branch,
            Fund = fund,
            DocDate = DateHelper.ToShamsiSlashDate(fiche.RayvarzDocDate),
            ActDate = DateHelper.ToShamsiSlashDate(fiche.RayvarzActDate),
            DueDate = DateHelper.ToShamsiSlashDate(fiche.RayvarzDueDate)
        };

        var sendResult = await _send.SendAsync(sendReq, ct);
        return new ProcessOutcome(
            plan.SendPath,
            sendResult.Success,
            Skipped: false,
            sendResult.Message ?? (sendResult.Success ? "ارسال موفق" : "ارسال ناموفق"),
            VerifiedInRayvarz: sendResult.VerifiedInRayvarz,
            DocNotSentError: sendResult.DocNotSentError,
            BillId: plan.BillId,
            PaymentId: plan.PaymentId);
    }
}
