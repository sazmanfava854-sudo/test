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
    public void NormalizePairs_converts_persian_fiche_number()
    {
        var pairs = UnsentBillPayLookupHelper.NormalizePairs(
        [
            new UnsentBillPayPair
            {
                BillId = "۰۵۰۸۳۳۵۳۳۲۴۲",
                PaymentId = "۰۵۰۸۳۳۵۳۳۲۴۲",
                FicheNo = "۰۵۰۸۳۳۵۳۳۲۴۲"
            }
        ]);

        Assert.Equal("050833533242", pairs[0].FicheNo);
        Assert.Equal("0050833533242", pairs[0].BillId);
    }

    [Fact]
    public void Identifier_search_converts_persian_and_arabic_digits()
    {
        Assert.Equal("050833533242", IdentifierText.Normalize("۰۵۰۸۳۳۵۳۳۲۴۲"));
        Assert.Equal("050833533242", IdentifierText.Normalize("٠٥٠٨٣٣٥٣٣٢٤٢"));
        Assert.Equal("101104/9881711", IdentifierText.Normalize("۱۰۱۱۰۴/۹۸۸۱۷۱۱"));

        var filter = FicheDateChangeHelper.BuildIdentifierFilter("۵۰۸۳۳۵۳۳۱۹۹");
        Assert.NotNull(filter);
        Assert.Contains("FicheNo IN", filter.Value.Clause);
        Assert.Contains(filter.Value.Parameters, p => p.Value == "050833533199");
        Assert.Contains(filter.Value.Parameters, p => p.Value == "50833533199");
    }

    [Fact]
    public void ExpandLookupKeys_matches_fiche_without_leading_zero()
    {
        var keys = UnsentBillPayLookupHelper.ExpandLookupKeys("۵۰۸۳۳۵۳۳۱۹۹");
        Assert.Contains("50833533199", keys);
        Assert.Contains("050833533199", keys);
        Assert.Contains("0050833533199", keys);
    }

    [Fact]
    public void NormalizePairs_accepts_fiche_number_without_bill_and_pay()
    {
        var pairs = UnsentBillPayLookupHelper.NormalizePairs(
        [
            new UnsentBillPayPair { FicheNo = "50833533243" },
            new UnsentBillPayPair { BillId = "50833533199", PaymentId = "" }
        ]);

        Assert.Single(pairs);
        Assert.Equal("50833533243", pairs[0].FicheNo);
        Assert.Equal("0050833533243", pairs[0].BillId);
    }

    [Fact]
    public void Negative_rayvarz_tahator_amount_is_replaced_by_partner()
    {
        Assert.Equal(
            UnsentBillPayLookupHelper.TahatorExcelRowAction.ReplaceWithPartner,
            UnsentBillPayLookupHelper.DecideTahatorExcelRow(true, true, -1027136660m, true, false));
        Assert.Equal(
            UnsentBillPayLookupHelper.TahatorExcelRowAction.RemoveAlreadyInRayvarz,
            UnsentBillPayLookupHelper.DecideTahatorExcelRow(true, true, 3183050000m, true, false));
        Assert.Equal(
            UnsentBillPayLookupHelper.TahatorExcelRowAction.RemoveAlreadyInRayvarz,
            UnsentBillPayLookupHelper.DecideTahatorExcelRow(true, true, -1m, true, true));
        Assert.Equal(
            UnsentBillPayLookupHelper.TahatorExcelRowAction.Keep,
            UnsentBillPayLookupHelper.DecideTahatorExcelRow(true, false, null, true, false));
    }

    [Fact]
    public void ValidateRequest_requires_valid_pairs_and_max()
    {
        Assert.Equal("درخواست خالی است", UnsentBillPayLookupHelper.ValidateRequest(null));
        Assert.Equal(
            "هر ردیف اکسل باید شماره فیش، یا شناسه قبض و شناسه پرداخت داشته باشد",
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
    public void IndexRawPairs_keeps_excel_values_for_miss_display()
    {
        var map = UnsentBillPayLookupHelper.IndexRawPairs(
        [
            new UnsentBillPayPair { BillId = "3091418652060", PaymentId = "1426270355" }
        ]);

        var key = "3091418652060|0001426270355";
        Assert.True(map.TryGetValue(key, out var raw));
        Assert.Equal("3091418652060", raw.RawBill);
        Assert.Equal("1426270355", raw.RawPay);
    }

    [Fact]
    public void DescribeMiss_explains_rayvarz_and_swapped_columns()
    {
        Assert.Equal(
            "فیش 123: قبلاً ارسال شده — شناسه قبض: 3091418652060، شناسه پرداخت: 1426270355",
            UnsentBillPayLookupHelper.DescribeMiss(
                new BillPayMissDiagnostic
                {
                    Found = true,
                    FicheNo = "123",
                    AlreadySent = true
                },
                "3091418652060",
                "1426270355"));

        Assert.Contains(
            "جابه‌جا",
            UnsentBillPayLookupHelper.DescribeMiss(new BillPayMissDiagnostic
            {
                Found = true,
                SwappedColumns = true
            },
            "3091418652060",
            "1426270355"));

        Assert.DoesNotContain(
            "جابه‌جا",
            UnsentBillPayLookupHelper.DescribeMiss(new BillPayMissDiagnostic
            {
                Found = true,
                SwappedColumns = true
            },
            "050833533236",
            "050833533236"));
    }

    [Fact]
    public void DescribeMiss_prefers_already_sent_over_swapped_flag()
    {
        var reason = UnsentBillPayLookupHelper.DescribeMiss(
            new BillPayMissDiagnostic
            {
                Found = true,
                FicheNo = "050833533236",
                AlreadySent = true,
                SwappedColumns = true
            },
            "050833533236",
            "050833533236");
        Assert.Contains("قبلاً ارسال شده", reason);
        Assert.DoesNotContain("جابه‌جا", reason);
    }

    [Fact]
    public void Pair_partner_already_in_rayvarz_is_counted_registered_not_rejected()
    {
        var result = new UnsentBatchSendResult
        {
            Total = 2,
            Succeeded = 1,
            Skipped = 1,
            Results =
            [
                new UnsentBatchSendItemResult
                {
                    FicheNo = "050833533199",
                    Success = true,
                    Skipped = true,
                    SkipReason = "AllInRayvarz",
                    Message = "این فیش قبلاً در رایورز ثبت شده است."
                },
                new UnsentBatchSendItemResult
                {
                    FicheNo = "050833533196",
                    Success = true,
                    Message = "فیش 050833533196 ارسال شد."
                }
            ]
        };

        UnsentFicheService.PromoteTahatorPairRegisteredInSameBatch(
            result,
            new Dictionary<string, string>
            {
                ["050833533199"] = "050833533196",
                ["050833533196"] = "050833533199"
            });

        Assert.Equal(2, result.Succeeded);
        Assert.Equal(0, result.Skipped);
        Assert.True(result.Results[0].Success);
        Assert.False(result.Results[0].Skipped);
        Assert.Contains("ثبت شد", result.Results[0].Message);
    }

    [Fact]
    public void ExpandLookupKeys_includes_stored_width_and_padded_form()
    {
        var keys = UnsentBillPayLookupHelper.ExpandLookupKeys("050833533236");
        Assert.Contains("050833533236", keys);
        Assert.Contains("0050833533236", keys);
        Assert.Contains("50833533236", keys);
    }

    [Fact]
    public void NormalizePairs_carries_optional_fiche_no()
    {
        var pairs = UnsentBillPayLookupHelper.NormalizePairs(
        [
            new UnsentBillPayPair
            {
                BillId = "050833533236",
                PaymentId = "050833533236",
                FicheNo = "050833533236"
            }
        ]);
        Assert.Single(pairs);
        Assert.Equal("050833533236", pairs[0].FicheNo);
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
