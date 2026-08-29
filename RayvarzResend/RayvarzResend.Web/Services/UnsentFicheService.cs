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

        var processedTahatorPairs = new HashSet<Guid>();
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
                var outcome = await ProcessOneAsync(req, ficheNo, processedTahatorPairs, user, ct);
                item.SendPath = outcome.SendPath;
                item.Success = outcome.Success;
                item.Skipped = outcome.Skipped;
                item.SkipReason = outcome.SkipReason;
                item.Message = outcome.Message;
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
                item.Message = ex.Message;
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

        var processedTahatorPairs = new HashSet<Guid>();

        foreach (var rawNo in req.FicheNos.Distinct(StringComparer.Ordinal))
        {
            var ficheNo = rawNo.Trim();
            if (string.IsNullOrWhiteSpace(ficheNo))
                continue;

            plan.Items.Add(await PlanOneAsync(req, ficheNo, processedTahatorPairs, user, ct));
        }

        return plan;
    }

    private async Task<UnsentBatchPlanItem> PlanOneAsync(
        UnsentBatchSendRequest req,
        string ficheNo,
        HashSet<Guid> processedTahatorPairs,
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
            var pair = await _repo.ResolveTahatorPairAsync(ficheNo, ct);
            if (pair == null)
            {
                item.SendPath = "Tahator";
                item.Detail = "جفت تهاتر ناقص";
                item.BlockReason = "جفت ۱۵۷+۱۵۸ کامل نیست";
                return item;
            }

            if (processedTahatorPairs.Contains(pair.NidIncome))
            {
                item.SendPath = "Tahator";
                item.Detail = "همراه جفت تهاتر";
                item.BlockReason = "جفت تهاتر قبلاً در همین دسته پردازش می‌شود";
                return item;
            }

            processedTahatorPairs.Add(pair.NidIncome);
            item.SendPath = "Tahator";
            item.TahatorPairFicheNo = string.Equals(pair.AmountFicheNo, ficheNo, StringComparison.Ordinal)
                ? pair.IncomeFicheNo
                : pair.AmountFicheNo;
            item.Detail = $"جفت ۱۵۷={pair.AmountFicheNo} → ۱۵۸={pair.IncomeFicheNo}";
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
        string? DocNotSentError = null);

    private async Task<ProcessOutcome> ProcessOneAsync(
        UnsentBatchSendRequest req,
        string ficheNo,
        HashSet<Guid> processedTahatorPairs,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var plan = await PlanOneAsync(req, ficheNo, processedTahatorPairs, user, ct);
        if (!plan.CanSend)
        {
            return new ProcessOutcome(
                plan.SendPath,
                Success: false,
                Skipped: true,
                Message: plan.BlockReason ?? "رد شد",
                SkipReason: plan.SendPath);
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
                DocNotSentError: tahResult.DocNotSentError);
        }

        var fiche = await _repo.LoadAsync(IdentifierType.FicheNo, ficheNo, ct);
        if (fiche == null)
            return new ProcessOutcome(plan.SendPath, false, true, "فیش یافت نشد", "NotFound");

        if (!FicheBranchResolver.TryResolve(fiche, out var branch, out var fund, out var branchError))
            return new ProcessOutcome(plan.SendPath, false, true, branchError ?? FicheBranchResolver.RegionNotResolvedMessage, "RegionUnresolved");

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
            DocNotSentError: sendResult.DocNotSentError);
    }
}
