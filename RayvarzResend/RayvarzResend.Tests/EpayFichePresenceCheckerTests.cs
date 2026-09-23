using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class EpayFichePresenceCheckerTests
{
    [Fact]
    public void Paid_or_not_paid_means_fiche_exists_in_epay()
    {
        var paid = EpayFichePresenceChecker.FromLookupStep(new BankInquiryParsedStep
        {
            Kind = BankInquiryStepKind.Paid,
            Message = "قبض مورد نظر یافت شد"
        });
        var unpaid = EpayFichePresenceChecker.FromLookupStep(new BankInquiryParsedStep
        {
            Kind = BankInquiryStepKind.NotPaid,
            Message = "پرداخت نشده"
        });

        Assert.True(paid.Exists);
        Assert.Equal(EpayPresenceKind.Found, paid.Kind);
        Assert.True(unpaid.Exists);
        Assert.Equal(EpayPresenceKind.Found, unpaid.Kind);
    }

    [Fact]
    public void Record_not_found_uses_operator_message()
    {
        var result = EpayFichePresenceChecker.FromLookupStep(new BankInquiryParsedStep
        {
            Kind = BankInquiryStepKind.RecordNotFound,
            Message = "فیش در استعلام قبوض یافت نشد"
        });

        Assert.False(result.Exists);
        Assert.Equal(EpayPresenceKind.NotFound, result.Kind);
        Assert.Equal(EpayFichePresenceChecker.NotFoundMessage, result.UserMessage);
    }

    [Fact]
    public void Service_error_blocks_send_with_service_message()
    {
        var result = EpayFichePresenceChecker.FromLookupStep(new BankInquiryParsedStep
        {
            Kind = BankInquiryStepKind.ServiceError,
            Message = "زمان انتظار استعلام قبوض به پایان رسید"
        });

        Assert.False(result.Exists);
        Assert.Equal(EpayPresenceKind.ServiceError, result.Kind);
        Assert.Equal("زمان انتظار استعلام قبوض به پایان رسید", result.UserMessage);
    }

    [Fact]
    public void Batch_message_maps_single_not_found_text()
    {
        Assert.Equal(
            EpayFichePresenceChecker.BulkNotFoundMessage,
            EpayFichePresenceChecker.MapBatchMessage(EpayFichePresenceChecker.NotFoundMessage));
        Assert.Equal("خطای دیگر", EpayFichePresenceChecker.MapBatchMessage("خطای دیگر"));
    }
}
