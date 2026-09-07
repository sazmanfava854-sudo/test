namespace RayvarzResend.Web.Services;

public enum BankInquiryStepKind
{
    Paid,
    RecordNotFound,
    NotPaid,
    ServiceError
}

public sealed class BankInquiryParsedStep
{
    public BankInquiryStepKind Kind { get; init; }
    public string? PaymentDate { get; init; }
    public string Message { get; init; } = "";
    public string? RawResponse { get; init; }

    public BankInquiryApiResult ToApiResult(string inquirySource) => Kind switch
    {
        BankInquiryStepKind.Paid => BankInquiryApiResult.Paid(PaymentDate, Message, inquirySource),
        BankInquiryStepKind.NotPaid => BankInquiryApiResult.NotPaid(Message, inquirySource),
        BankInquiryStepKind.RecordNotFound => BankInquiryApiResult.NotPaid(Message, inquirySource),
        BankInquiryStepKind.ServiceError => BankInquiryApiResult.Failed(Message, RawResponse, inquirySource),
        _ => BankInquiryApiResult.Failed("نتیجه استعلام بانک نامشخص است", RawResponse, inquirySource)
    };
}
