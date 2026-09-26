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

    [Fact]
    public void BuildMixedLookupResult_income_then_duty_without_conflict()
    {
        var requested = UnsentBillPayLookupHelper.NormalizePairs(
        [
            new UnsentBillPayPair { BillId = "111", PaymentId = "222" },
            new UnsentBillPayPair { BillId = "333", PaymentId = "444" }
        ]);
        var income = new List<UnsentFicheListItem>
        {
            new() { FicheNo = "I1", BillId = "0000000000111", PaymentId = "0000000000222" }
        };
        var duty = new List<UnsentFicheListItem>
        {
            new() { FicheNo = "D1", BillId = "0000000000333", PaymentId = "0000000000444" }
        };

        var result = UnsentBillPayLookupHelper.BuildMixedLookupResult(
            requested, income, [], duty);

        Assert.Equal(2, result.Found);
        Assert.Empty(result.Conflicts);
        Assert.Equal(UnsentFicheKind.Income, result.Items[0].SourceKind);
        Assert.Equal(UnsentFicheKind.Duty, result.Items[1].SourceKind);
    }

    [Fact]
    public void BuildMixedLookupResult_flags_income_and_duty_conflict()
    {
        var requested = UnsentBillPayLookupHelper.NormalizePairs(
        [
            new UnsentBillPayPair { BillId = "111", PaymentId = "222" }
        ]);
        var income = new List<UnsentFicheListItem>
        {
            new() { FicheNo = "I1", BillId = "0000000000111", PaymentId = "0000000000222" }
        };
        var dutyProbe = new List<UnsentFicheListItem>
        {
            new() { FicheNo = "D1", BillId = "0000000000111", PaymentId = "0000000000222" }
        };

        var result = UnsentBillPayLookupHelper.BuildMixedLookupResult(
            requested, income, dutyProbe, []);

        Assert.Empty(result.Items);
        Assert.Single(result.Conflicts);
        Assert.Equal(UnsentBillPayLookupHelper.IncomeDutyConflictReason, result.Conflicts[0].Reason);
        Assert.Empty(result.Misses);
    }
}
