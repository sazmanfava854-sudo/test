namespace RayvarzResend.Web.Services;

public sealed class BankInquiryApiResult
{
    public bool IsPaid { get; set; }
    public string? PaymentDate { get; set; }
    public string Message { get; set; } = "";
    public string? RawResponse { get; set; }
    public bool ServiceError { get; set; }
    public string InquirySource { get; set; } = "";

    public static BankInquiryApiResult Paid(string? paymentDate = null, string? message = null, string? inquirySource = null) => new()
    {
        IsPaid = true,
        PaymentDate = paymentDate,
        Message = string.IsNullOrWhiteSpace(message) ? "پرداخت در بانک تایید شد" : message.Trim(),
        InquirySource = inquirySource ?? ""
    };

    public static BankInquiryApiResult NotPaid(string? message = null, string? inquirySource = null) => new()
    {
        IsPaid = false,
        Message = string.IsNullOrWhiteSpace(message) ? BankInquiryConfirmHelper.UnpaidFicheMessage : message.Trim(),
        InquirySource = inquirySource ?? ""
    };

    public static BankInquiryApiResult Failed(string message, string? raw = null, string? inquirySource = null) => new()
    {
        IsPaid = false,
        ServiceError = true,
        Message = message,
        RawResponse = raw,
        InquirySource = inquirySource ?? ""
    };
}
