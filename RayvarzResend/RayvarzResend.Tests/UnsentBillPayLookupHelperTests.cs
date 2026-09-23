using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class UnsentBillPayLookupHelperTests
{
    [Fact]
    public void NormalizePairs_pads_persian_digits_and_deduplicates()
    {
        var pairs = UnsentBillPayLookupHelper.NormalizePairs(
        [
            new UnsentBillPayPair { BillId = "۶۰۵۱۰۵۷۴", PaymentId = "123" },
            new UnsentBillPayPair { BillId = "0000060510574", PaymentId = "0000000000123" },
            new UnsentBillPayPair { BillId = "", PaymentId = "1" }
        ]);

        Assert.Single(pairs);
        Assert.Equal("0000060510574", pairs[0].BillId);
        Assert.Equal("0000000000123", pairs[0].PaymentId);
        Assert.Equal("60510574", pairs[0].BillIdTrim);
        Assert.Equal("123", pairs[0].PaymentIdTrim);
    }

    [Fact]
    public void ValidateRequest_requires_valid_pairs_and_max()
    {
        Assert.Equal("درخواست خالی است", UnsentBillPayLookupHelper.ValidateRequest(null));
        Assert.Equal(
            "فایل اکسل باید حداقل یک ردیف با شناسه قبض و شناسه پرداخت معتبر داشته باشد",
            UnsentBillPayLookupHelper.ValidateRequest(new UnsentBillPayLookupRequest()));

        var tooMany = new UnsentBillPayLookupRequest
        {
            Pairs = Enumerable.Range(1, UnsentBillPayLookupHelper.MaxPairs + 1)
                .Select(i => new UnsentBillPayPair
                {
                    BillId = i.ToString(),
                    PaymentId = (i + 1000).ToString()
                })
                .ToList()
        };
        Assert.Equal(
            $"حداکثر {UnsentBillPayLookupHelper.MaxPairs} ردیف در هر بارگذاری مجاز است",
            UnsentBillPayLookupHelper.ValidateRequest(tooMany));
    }

    [Fact]
    public void MatchKey_aligns_padded_and_unpadded_ids()
    {
        Assert.Equal(
            UnsentBillPayLookupHelper.MatchKey("60510574", "123"),
            UnsentBillPayLookupHelper.MatchKey("0000060510574", "0000000000123"));
    }
}
