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

    public Task<UnsentFicheSearchResult> SearchAsync(UnsentFicheSearchRequest req, CancellationToken ct = default) =>
        req.FicheKind == UnsentFicheKind.Duty
            ? _repo.SearchUnsentDutyAsync(req, ct)
            : _repo.SearchUnsentIncomeAsync(req, ct);

    public async Task<UnsentBillPayLookupResult> LookupByBillPayAsync(
        UnsentBillPayLookupRequest req,
        CancellationToken ct = default)
    {
        var validation = UnsentBillPayLookupHelper.ValidateRequest(req);
        if (validation != null)
            return new UnsentBillPayLookupResult { FicheKind = req.FicheKind, Error = validation };

        var pairs = UnsentBillPayLookupHelper.NormalizePairs(req.Pairs);
        var items = await _repo.FindUnsentByBillPayAsync(req.FicheKind, pairs, ct);
        var foundKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            var key = UnsentBillPayLookupHelper.MatchKey(item.BillId, item.PaymentId);
            if (key.Length > 0)
                foundKeys.Add(key);
        }

        var misses = pairs
            .Where(p => !foundKeys.Contains(p.BillId + "|" + p.PaymentId))
            .Select(p => new UnsentBillPayMiss
            {
                BillId = p.BillId,
                PaymentId = p.PaymentId,
                Reason = "در دیتابیس یافت نشد"
            })
            .ToList();

        return new UnsentBillPayLookupResult
        {
            FicheKind = req.FicheKind,
            Requested = pairs.Count,
            Found = items.Count,
            NotFound = misses.Count,
            Items = items,
            Misses = misses
        };
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
        var result = new UnsentBatchSendResult
        {
            DryRun = dryRun,
            Total = req.FicheNos?.Count ?? 0
        };

        if (req.FicheNos == null || req.FicheNos.Count == 0)
            return result;

        var index = 0;

        foreach (var rawNo in req.FicheNos.Distinct(StringComparer.Ordinal))
        {
            var ficheNo = rawNo.Trim();
            if (string.IsNullOrWhiteSpace(ficheNo))
                continue;

            if (index > 0 && delayMs > 0 && !dryRun)
                await Task.Delay(delayMs, ct);

            var item = new UnsentBatchSendItemResult { FicheNo = ficheNo };
            result.Results.Add(item);
            index++;

            try
            {
                var outcome = await ProcessOneAsync(req, ficheNo, user, ct);
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
        var plan = new UnsentBatchPlanResult { Total = req.FicheNos?.Count ?? 0 };
        if (req.FicheNos == null || req.FicheNos.Count == 0)
            return plan;

        foreach (var rawNo in req.FicheNos.Distinct(StringComparer.Ordinal))
        {
            var ficheNo = rawNo.Trim();
            if (string.IsNullOrWhiteSpace(ficheNo))
                continue;

            plan.Items.Add(await PlanOneAsync(req, ficheNo, user, ct));
        }

        return plan;
    }

    private async Task<UnsentBatchPlanItem> PlanOneAsync(
        UnsentBatchSendRequest req,
        string ficheNo,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
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

        if (req.FicheKind == UnsentFicheKind.Income && fiche.Category != FicheCategory.Income)
        {
            item.SendPath = "Skip";
            item.BlockReason = "نوع فیش با شهرسازی مطابقت ندارد";
            return item;
        }

        if (req.FicheKind == UnsentFicheKind.Duty
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
            item.SendPath = req.FicheKind == UnsentFicheKind.Duty ? "Duty" : "Income";
            item.BlockReason = validation;
            return item;
        }

        if (!FicheBranchResolver.TryResolve(fiche, out _, out _, out var branchError))
        {
            item.SendPath = req.FicheKind == UnsentFicheKind.Duty ? "Duty" : "Income";
            item.BlockReason = branchError;
            return item;
        }

        item.SendPath = req.FicheKind == UnsentFicheKind.Duty ? "Duty" : "Income";
        item.Detail = req.FicheKind == UnsentFicheKind.Duty ? "ارسال نوسازی/صنفی" : "ارسال درآمدی شهرسازی";
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
        string ficheNo,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var plan = await PlanOneAsync(req, ficheNo, user, ct);
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
