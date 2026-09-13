namespace RayvarzResend.Web.Services;

/// <summary>فرمت‌های شناخته‌شده بدنه JSON برای سرویس‌های epay.</summary>
public enum BankInquiryRequestFormat
{
    CamelFlat,
    PascalFlat,
    EpayIdFlat,
    CamelWrapped,
    PascalWrapped
}
