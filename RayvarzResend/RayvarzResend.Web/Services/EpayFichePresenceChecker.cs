using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public enum EpayPresenceKind
{
    Found,
    NotFound,
    ServiceError
}

public sealed class EpayFichePresenceResult
{
    public EpayPresenceKind Kind { get; init; }
    public string Message { get; init; } = "";

    public bool Exists => Kind == EpayPresenceKind.Found;

    public string UserMessage =>
        Kind == EpayPresenceKind.NotFound
            ? EpayFichePresenceChecker.NotFoundMessage
            : Message;
}

/// <summary>
/// استعلام وجود فیش در epay_FindFichesByBillIDPayID.
/// Paid و NotPaid یعنی فیش در سامانه هست؛ RecordNotFound یعنی نیست.
/// </summary>
public sealed class EpayFichePresenceChecker
{
    public const string NotFoundMessage = "فیش مورد نظر در سامانه epay یافت نشد";
    public const string BulkNotFoundMessage = "در سامانه epay یافت نشد";
    public const string MissingIdsMessage = "شناسه قبض و شناسه پرداخت برای استعلام epay الزامی است";

    private readonly BankInquiryApiClient _api;
    private readonly ILogger<EpayFichePresenceChecker> _logger;

    public EpayFichePresenceChecker(
        BankInquiryApiClient api,
        ILogger<EpayFichePresenceChecker> logger)
    {
        _api = api;
        _logger = logger;
    }

    public static (string BillId, string PaymentId) ResolveIds(FicheHeaderDto fiche)
    {
        var bill = FirstNonEmpty(fiche.BillIdRaw, fiche.BillId);
        var pay = FirstNonEmpty(fiche.PaymentIdRaw, fiche.PaymentId);
        return (bill, pay);
    }

    public async Task<EpayFichePresenceResult> CheckAsync(
        string? billId,
        string? paymentId,
        CancellationToken ct = default)
    {
        var normalizedBill = BankInquiryConfirmHelper.NormalizeBillOrPayId(billId);
        var normalizedPay = BankInquiryConfirmHelper.NormalizeBillOrPayId(paymentId);

        try
        {
            var step = await _api.CheckFichePresenceAsync(billId ?? "", paymentId ?? "", ct);
            var result = FromLookupStep(step);
            if (result.Exists)
            {
                _logger.LogInformation(
                    "epay presence confirmed billId={BillId} payId={PayId}",
                    normalizedBill, normalizedPay);
            }
            else
            {
                _logger.LogWarning(
                    "epay presence check failed billId={BillId} payId={PayId} kind={Kind} message={Message}",
                    normalizedBill, normalizedPay, result.Kind, result.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "epay presence check threw billId={BillId} payId={PayId}",
                normalizedBill, normalizedPay);
            return new EpayFichePresenceResult
            {
                Kind = EpayPresenceKind.ServiceError,
                Message = BankInquiryApiClient.BuildUserErrorMessage(
                    ex, BankInquiryResponseParser.FicheLookupSourceLabel)
            };
        }
    }

    public async Task EnsurePresentOrThrowAsync(
        string? billId,
        string? paymentId,
        CancellationToken ct = default)
    {
        var result = await CheckAsync(billId, paymentId, ct);
        if (result.Exists)
            return;

        throw new InvalidOperationException(result.UserMessage);
    }

    public static EpayFichePresenceResult FromLookupStep(BankInquiryParsedStep step) =>
        step.Kind switch
        {
            BankInquiryStepKind.Paid or BankInquiryStepKind.NotPaid => new EpayFichePresenceResult
            {
                Kind = EpayPresenceKind.Found,
                Message = string.IsNullOrWhiteSpace(step.Message) ? "فیش در سامانه epay یافت شد" : step.Message
            },
            BankInquiryStepKind.RecordNotFound => new EpayFichePresenceResult
            {
                Kind = EpayPresenceKind.NotFound,
                Message = NotFoundMessage
            },
            BankInquiryStepKind.ServiceError => new EpayFichePresenceResult
            {
                Kind = EpayPresenceKind.ServiceError,
                Message = string.IsNullOrWhiteSpace(step.Message)
                    ? "خطا در استعلام سامانه epay"
                    : step.Message
            },
            _ => new EpayFichePresenceResult
            {
                Kind = EpayPresenceKind.ServiceError,
                Message = "نتیجه استعلام epay نامشخص است"
            }
        };

    public static string MapBatchMessage(string? message)
    {
        if (string.Equals(message, NotFoundMessage, StringComparison.Ordinal))
            return BulkNotFoundMessage;
        return message ?? "";
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return "";
    }
}
