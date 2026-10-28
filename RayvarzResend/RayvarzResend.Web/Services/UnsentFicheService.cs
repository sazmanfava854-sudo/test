using System.Security.Claims;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Sync;

namespace RayvarzResend.Web.Services;

public class UnsentFicheService
{
    private readonly FicheRepository _repo;
    private readonly FicheSendService _send;
    private readonly TahatorResendService _tahator;
    private readonly IConfiguration _config;
    private readonly RuleSendGate _sendGate;

    public UnsentFicheService(
        FicheRepository repo,
        FicheSendService send,
        TahatorResendService tahator,
        IConfiguration config,
        RuleSendGate sendGate)
    {
        _repo = repo;
        _send = send;
        _tahator = tahator;
        _config = config;
        _sendGate = sendGate;
    }

    public async Task<UnsentFicheSearchResult> SearchAsync(
        UnsentFicheSearchRequest req,
        ClaimsPrincipal? user,
        CancellationToken ct = default)
    {
        if (!ApplyUserDistrictScope(req, user))
            return new UnsentFicheSearchResult { FicheKind = req.FicheKind };

        var result = req.FicheKind == UnsentFicheKind.Duty
            ? await _repo.SearchUnsentDutyAsync(req, ct)
            : await _repo.SearchUnsentIncomeAsync(req, ct);
        foreach (var item in result.Items)
            item.SourceKind = req.FicheKind;
        return result;
    }

    /// <summary>
    /// کاربر منطقه‌ای فقط فیش‌های منطقهٔ خود را می‌بیند (همان قاعدهٔ ارسال)؛ false = هیچ نتیجه‌ای مجاز نیست.
    /// </summary>
    internal static bool ApplyUserDistrictScope(UnsentFicheSearchRequest req, ClaimsPrincipal? user)
    {
        req.RestrictIncomeAccountGroup = null;
        if (DistrictAccessService.MayOverrideBranch(user))
            return true;

        var userDistrict = DistrictAccessService.NormalizeDistrict(DistrictAccessService.GetUserDistrict(user));
        if (string.IsNullOrEmpty(userDistrict))
            return false;

        if (userDistrict == DistrictAccessService.CenterDistrictCode)
        {
            if (req.FicheKind == UnsentFicheKind.Duty)
                return false;
            req.District = null;
            req.RestrictIncomeAccountGroup = TahatorRowBuilder.IncomeAccountGroupTahatorAmount;
            return true;
        }

        req.District = userDistrict;
        return true;
    }

    public async Task<UnsentBillPayLookupResult> LookupByBillPayAsync(
        UnsentBillPayLookupRequest req,
        CancellationToken ct = default)
    {
        var validation = UnsentBillPayLookupHelper.ValidateRequest(req);
        if (validation != null)
            return new UnsentBillPayLookupResult { Error = validation };

        var pairs = UnsentBillPayLookupHelper.NormalizePairs(req.Pairs);
        var rawByKey = UnsentBillPayLookupHelper.IndexRawPairs(req.Pairs);

        var incomeBatch = await _repo.LookupBillPayBatchAsync(
            UnsentFicheKind.Income, pairs, diagnoseMisses: false, ct);
        var incomeKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in incomeBatch.Items)
        {
            var key = UnsentBillPayLookupHelper.MatchKey(item.BillId, item.PaymentId);
            if (key.Length > 0)
                incomeKeys.Add(key);
        }

        var incomeFoundPairs = pairs
            .Where(p => incomeKeys.Contains(UnsentBillPayLookupHelper.PairKey(p)))
            .ToList();
        var remainingPairs = pairs
            .Where(p => !incomeKeys.Contains(UnsentBillPayLookupHelper.PairKey(p)))
            .ToList();

        var emptyBatch = new FicheRepository.BillPayBatchLookupResult(
            [],
            new Dictionary<string, BillPayMissDiagnostic>(StringComparer.Ordinal));
        var dutyConflictTask = incomeFoundPairs.Count > 0
            ? _repo.LookupBillPayBatchAsync(UnsentFicheKind.Duty, incomeFoundPairs, diagnoseMisses: false, ct)
            : Task.FromResult(emptyBatch);
        var dutyRemainingTask = remainingPairs.Count > 0
            ? _repo.LookupBillPayBatchAsync(UnsentFicheKind.Duty, remainingPairs, diagnoseMisses: false, ct)
            : Task.FromResult(emptyBatch);
        await Task.WhenAll(dutyConflictTask, dutyRemainingTask);
        var dutyConflictBatch = await dutyConflictTask;
        var dutyBatch = await dutyRemainingTask;

        var result = UnsentBillPayLookupHelper.BuildMixedLookupResult(
            pairs,
            incomeBatch.Items,
            dutyConflictBatch.Items,
            dutyBatch.Items);

        var missPairs = pairs
            .Where(p =>
            {
                var key = UnsentBillPayLookupHelper.PairKey(p);
                var conflict = result.Conflicts.Any(c =>
                    UnsentBillPayLookupHelper.MatchKey(c.BillId, c.PaymentId) == key);
                if (conflict) return false;
                return !result.Items.Any(i =>
                    UnsentBillPayLookupHelper.MatchKey(i.BillId, i.PaymentId) == key);
            })
            .ToList();
        var missDiagnostics = await _repo.DiagnoseBillPayMissesAsync(missPairs, ct);

        foreach (var item in result.Items)
        {
            var key = UnsentBillPayLookupHelper.MatchKey(item.BillId, item.PaymentId);
            if (key.Length > 0 && rawByKey.TryGetValue(key, out var raw))
                ApplyExcelDisplayIds(item, raw.RawBill, raw.RawPay);
        }

        foreach (var conflict in result.Conflicts)
        {
            var key = UnsentBillPayLookupHelper.MatchKey(conflict.BillId, conflict.PaymentId);
            if (key.Length > 0 && rawByKey.TryGetValue(key, out var raw))
            {
                if (!string.IsNullOrWhiteSpace(raw.RawBill)) conflict.BillId = raw.RawBill;
                if (!string.IsNullOrWhiteSpace(raw.RawPay)) conflict.PaymentId = raw.RawPay;
            }
        }

        for (var i = 0; i < result.Misses.Count; i++)
        {
            var miss = result.Misses[i];
            var lookupKey = UnsentBillPayLookupHelper.MatchKey(miss.BillId, miss.PaymentId);
            if (lookupKey.Length == 0)
                lookupKey = miss.BillId + "|" + miss.PaymentId;

            rawByKey.TryGetValue(lookupKey, out var raw);
            missDiagnostics.TryGetValue(lookupKey, out var diagnostic);

            var displayBill = string.IsNullOrEmpty(raw.RawBill) ? miss.BillId : raw.RawBill;
            var displayPay = string.IsNullOrEmpty(raw.RawPay) ? miss.PaymentId : raw.RawPay;
            result.Misses[i] = new UnsentBillPayMiss
            {
                BillId = displayBill,
                PaymentId = displayPay,
                Reason = UnsentBillPayLookupHelper.DescribeMiss(diagnostic, displayBill, displayPay)
            };
        }

        await AdjustTahatorExcelRowsAsync(result, ct);
        return result;
    }

    /// <summary>
    /// فیش تهاترِ موجود در رایورز از گرید ارسال حذف می‌شود.
    /// اگر مبلغ آن منفی باشد (۰۵۰۸۳۳۵۳۳۲۴۲) جفتِ غایب (۰۵۰۸۳۳۵۳۳۲۴۳) جای آن می‌آید.
    /// </summary>
    private async Task AdjustTahatorExcelRowsAsync(UnsentBillPayLookupResult result, CancellationToken ct)
    {
        var tahatorNos = result.Items.Where(i => i.IsTahator).Select(i => i.FicheNo).ToList();
        if (tahatorNos.Count == 0)
            return;

        var amounts = await _repo.GetRayvarzMinAmountByFicheNoAsync(tahatorNos, ct);
        if (amounts.Count == 0)
            return;

        var partnerByFiche = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var item in result.Items)
        {
            if (!item.IsTahator || !amounts.ContainsKey(item.FicheNo))
                continue;
            var pair = await _repo.ResolveTahatorPairAsync(item.FicheNo, ct);
            var partnerNo = OtherTahatorFicheNo(pair, item.FicheNo);
            if (!string.IsNullOrWhiteSpace(partnerNo))
                partnerByFiche[item.FicheNo] = partnerNo;
        }

        var partnerAmounts = await _repo.GetRayvarzMinAmountByFicheNoAsync(partnerByFiche.Values.ToList(), ct);
        var kept = new List<UnsentFicheListItem>();
        foreach (var item in result.Items)
        {
            if (!item.IsTahator || !amounts.TryGetValue(item.FicheNo, out var amount))
            {
                kept.Add(item);
                continue;
            }

            partnerByFiche.TryGetValue(item.FicheNo, out var partnerNo);
            var hasPartner = !string.IsNullOrWhiteSpace(partnerNo);
            var partnerInRayvarz = hasPartner && partnerAmounts.ContainsKey(partnerNo!);
            var action = UnsentBillPayLookupHelper.DecideTahatorExcelRow(
                item.IsTahator, true, amount, hasPartner, partnerInRayvarz);

            if (action == UnsentBillPayLookupHelper.TahatorExcelRowAction.Keep)
            {
                kept.Add(item);
                continue;
            }

            if (action == UnsentBillPayLookupHelper.TahatorExcelRowAction.ReplaceWithPartner)
            {
                var partner = await _repo.LoadIncomeListItemByFicheNoAsync(partnerNo!, unsentOnly: true, ct)
                    ?? await _repo.LoadIncomeListItemByFicheNoAsync(partnerNo!, unsentOnly: false, ct);
                if (partner != null)
                {
                    partner.SourceKind = UnsentFicheKind.Income;
                    kept.Add(partner);
                    continue;
                }
            }

            result.Misses.Add(new UnsentBillPayMiss
            {
                BillId = item.BillId,
                PaymentId = item.PaymentId,
                Reason = $"فیش {item.FicheNo} قبلاً در رایورز ثبت شده است"
            });
        }

        result.Items = kept
            .GroupBy(i => i.FicheNo, StringComparer.Ordinal)
            .Select(g => g.First())
            .ToList();
        result.Found = result.Items.Count;
        result.NotFound = result.Misses.Count;
    }

    private static List<UnsentBatchFicheTarget> ResolveBatchTargets(UnsentBatchSendRequest req)
    {
        if (req.Targets is { Count: > 0 })
        {
            return req.Targets
                .Where(t => !string.IsNullOrWhiteSpace(t.FicheNo))
                .Select(t => new UnsentBatchFicheTarget
                {
                    FicheNo = IdentifierText.Normalize(t.FicheNo),
                    SourceKind = t.SourceKind
                })
                .ToList();
        }

        return (req.FicheNos ?? [])
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => new UnsentBatchFicheTarget
            {
                FicheNo = IdentifierText.Normalize(n),
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
        await _sendGate.EnsureOpenAsync(ct);
        var dryRun = SendDryRun.Rayvarz(_config);
        var tahatorDryRun = SendDryRun.Tahator(_config);
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
        var tahatorPartnerByFiche = new Dictionary<string, string>(StringComparer.Ordinal);

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
                item.DryRun = outcome.SendPath == "Tahator" ? tahatorDryRun : dryRun;
                item.Success = outcome.Success;
                item.Skipped = outcome.Skipped;
                item.SkipReason = outcome.SkipReason;
                item.Message = EpayFichePresenceChecker.MapBatchMessage(outcome.Message);
                item.BillId = outcome.BillId;
                item.PaymentId = outcome.PaymentId;
                item.VerifiedInRayvarz = outcome.VerifiedInRayvarz;
                item.DocNotSentError = outcome.DocNotSentError;

                if (!string.IsNullOrWhiteSpace(outcome.PartnerFicheNo))
                    tahatorPartnerByFiche[ficheNo] = outcome.PartnerFicheNo;

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

        PromoteTahatorPairRegisteredInSameBatch(result, tahatorPartnerByFiche);
        return result;
    }

    /// <summary>
    /// اگر در همین ارسال دسته‌ای یکی از جفت تهاتر ثبت شود و فیش دیگر فقط به‌خاطر «قبلاً در رایورز» رد شود،
    /// آن را هم ثبت‌شده حساب کن — نه رد شده.
    /// </summary>
    public static void PromoteTahatorPairRegisteredInSameBatch(
        UnsentBatchSendResult result,
        IReadOnlyDictionary<string, string> partnerByFiche)
    {
        var sent = new HashSet<string>(
            result.Results.Where(r => r.Success && !r.Skipped).Select(r => r.FicheNo.Trim()),
            StringComparer.Ordinal);

        foreach (var item in result.Results)
        {
            if (!item.Skipped || !string.Equals(item.SkipReason, "AllInRayvarz", StringComparison.Ordinal))
                continue;
            if (!partnerByFiche.TryGetValue(item.FicheNo.Trim(), out var partner))
                continue;
            if (string.IsNullOrWhiteSpace(partner) || !sent.Contains(partner.Trim()))
                continue;

            item.Skipped = false;
            item.Success = true;
            item.SkipReason = null;
            item.VerifiedInRayvarz = true;
            item.Message = $"فیش {item.FicheNo.Trim()} در رایورز ثبت شد.";
            if (result.Skipped > 0)
                result.Skipped--;
            result.Succeeded++;
        }
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

        var shamsiYear = DateHelper.ExtractShamsiYear(fiche.RayvarzDocDate);
        var eligibility = await FicheSendEligibility.CheckAsync(
            _repo, fiche, ficheNo, shamsiYear > 0 ? shamsiYear : null, ct);
        if (eligibility.RayvarzQueryFailed)
        {
            item.SendPath = "Skip";
            item.Detail = "Rayvarz";
            item.BlockReason = "بررسی تکراری رایورز ناموفق — ارسال نشد";
            return item;
        }

        var duplicateBlock = FicheSendEligibility.BuildBlockReason(eligibility);
        if (duplicateBlock != null)
        {
            item.SendPath = TahatorRowBuilder.IsTahatorFiche(fiche) ? "Tahator" : "Skip";
            item.Detail = eligibility.InRayvarz && eligibility.InAccountingDocHeader
                ? "Rayvarz+Accounting_DocHeader"
                : eligibility.InRayvarz
                    ? "Rayvarz"
                    : "Accounting_DocHeader";
            item.BlockReason = duplicateBlock;
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
        string PaymentId = "",
        string? PartnerFicheNo = null);

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
                PaymentId: plan.PaymentId,
                PartnerFicheNo: OtherTahatorFicheNo(tahResult.Pair, ficheNo));
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

    private static string? OtherTahatorFicheNo(TahatorPairInfo? pair, string ficheNo)
    {
        if (pair == null)
            return null;
        if (string.Equals(ficheNo, pair.AmountFicheNo, StringComparison.Ordinal))
            return pair.IncomeFicheNo;
        if (string.Equals(ficheNo, pair.IncomeFicheNo, StringComparison.Ordinal))
            return pair.AmountFicheNo;
        return null;
    }

    private static void ApplyExcelDisplayIds(UnsentFicheListItem item, string rawBill, string rawPay)
    {
        if (!string.IsNullOrWhiteSpace(rawBill))
            item.BillId = rawBill.Trim();
        if (!string.IsNullOrWhiteSpace(rawPay))
            item.PaymentId = rawPay.Trim();
    }
}
