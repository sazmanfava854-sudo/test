using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public class UnsentFicheService
{
    private readonly FicheRepository _repo;
    private readonly FicheSendService _send;
    private readonly IConfiguration _config;

    public UnsentFicheService(FicheRepository repo, FicheSendService send, IConfiguration config)
    {
        _repo = repo;
        _send = send;
        _config = config;
    }

    public Task<UnsentFicheSearchResult> SearchAsync(UnsentFicheSearchRequest req, CancellationToken ct = default) =>
        req.FicheKind == UnsentFicheKind.Duty
            ? _repo.SearchUnsentDutyAsync(req, ct)
            : _repo.SearchUnsentIncomeAsync(req, ct);

    public async Task<UnsentBatchSendResult> SendBatchAsync(UnsentBatchSendRequest req, CancellationToken ct = default)
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
                if (await _repo.ExistsInAccountingDocHeaderAsync(ficheNo, ct))
                {
                    item.Skipped = true;
                    item.SkipReason = "InDocHeader";
                    item.Message = "فیش در Accounting_DocHeader موجود است";
                    result.Skipped++;
                    continue;
                }

                var fiche = await _repo.LoadAsync(IdentifierType.FicheNo, ficheNo, ct);
                if (fiche == null)
                {
                    item.Message = "فیش در Income_Fiche یا Duty_Fiche یافت نشد";
                    result.Failed++;
                    continue;
                }

                if (req.FicheKind == UnsentFicheKind.Income && fiche.Category != FicheCategory.Income)
                {
                    item.Skipped = true;
                    item.SkipReason = "WrongKind";
                    item.Message = "نوع فیش با شهرسازی انتخاب‌شده مطابقت ندارد";
                    result.Skipped++;
                    continue;
                }

                if (req.FicheKind == UnsentFicheKind.Duty
                    && fiche.Category is not (FicheCategory.DutyNosazi or FicheCategory.DutySenfi))
                {
                    item.Skipped = true;
                    item.SkipReason = "WrongKind";
                    item.Message = "نوع فیش با نوسازی/صنفی انتخاب‌شده مطابقت ندارد";
                    result.Skipped++;
                    continue;
                }

                var validation = FicheSendService.ValidateSendable(fiche);
                if (validation != null)
                {
                    item.Skipped = true;
                    item.SkipReason = "NotSendable";
                    item.Message = validation;
                    result.Skipped++;
                    continue;
                }

                var (branch, fund) = FicheBranchResolver.Resolve(fiche);
                var sendReq = new SendFicheRequest
                {
                    Fiche = fiche,
                    Branch = branch,
                    Fund = fund,
                    DocDate = DateHelper.ToShamsiSlashDate(fiche.RayvarzDocDate),
                    ActDate = DateHelper.ToShamsiSlashDate(fiche.RayvarzActDate),
                    DueDate = DateHelper.ToShamsiSlashDate(fiche.RayvarzDueDate),
                    ResetStatus = req.ResetStatus
                };

                var sendResult = await _send.SendAsync(sendReq, ct);
                item.Success = sendResult.Success;
                item.VerifiedInRayvarz = sendResult.VerifiedInRayvarz;
                item.DocNotSentError = sendResult.DocNotSentError;
                item.Message = sendResult.Message ?? (sendResult.Success ? "ارسال موفق" : "ارسال ناموفق");

                if (sendResult.Success)
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
}
