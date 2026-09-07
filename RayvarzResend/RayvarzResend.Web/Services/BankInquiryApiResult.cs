namespace RayvarzResend.Web.Services;

public sealed class BankInquiryApiResult
{
    public bool IsPaid { get; set; }
    public string? PaymentDate { get; set; }
    public string Message { get; set; } = "";
    public string? RawResponse { get; set; }
    public bool ServiceError { get; set; }

    public static BankInquiryApiResult Paid(string? paymentDate = null, string? message = null) => new()
    {
        IsPaid = true,
        PaymentDate = paymentDate,
        Message = string.IsNullOrWhiteSpace(message) ? "پرداخت در بانک تایید شد" : message.Trim()
    };

    public static BankInquiryApiResult NotPaid(string? message = null) => new()
    {
        IsPaid = false,
        Message = string.IsNullOrWhiteSpace(message) ? "فیش پرداخت نشده" : message.Trim()
    };

    public static BankInquiryApiResult Failed(string message, string? raw = null) => new()
    {
        IsPaid = false,
        ServiceError = true,
        Message = message,
        RawResponse = raw
    };
}
