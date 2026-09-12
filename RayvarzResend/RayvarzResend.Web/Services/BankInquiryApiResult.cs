namespace RayvarzResend.Web.Services;

public sealed class BankInquiryDiagnosticsStep
{
    public string Service { get; set; } = "";
    public string Url { get; set; } = "";
    public string RequestBody { get; set; } = "";
    public int? HttpStatus { get; set; }
    public string? RawResponse { get; set; }
    public string Kind { get; set; } = "";
    public string Message { get; set; } = "";
    public string? PaymentDate { get; set; }
    public string? Exception { get; set; }
    public long ElapsedMs { get; set; }
}

public sealed class BankInquiryDiagnosticsResult
{
    public string BillId { get; set; } = "";
    public string PayId { get; set; } = "";
    public bool Configured { get; set; }
    public string UserName { get; set; } = "";
    public bool PasswordConfigured { get; set; }
    public string? Error { get; set; }
    public BankInquiryDiagnosticsStep? FicheLookup { get; set; }
    public BankInquiryDiagnosticsStep? OnlineBank { get; set; }
}

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
