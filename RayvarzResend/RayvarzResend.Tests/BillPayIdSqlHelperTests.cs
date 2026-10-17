using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class BillPayIdSqlHelperTests
{
    [Fact]
    public void PairMatch_includes_swapped_columns_and_norm13()
    {
        var sql = BillPayIdSqlHelper.PairMatchOnTable("f.BillID", "f.PaymentID");
        Assert.Contains("f.BillID", sql);
        Assert.Contains("p.PaymentId", sql);
        Assert.Contains("p.BillId", sql);
        Assert.Contains("REPLICATE('0', 13)", sql);
    }

    [Fact]
    public void PairMatchStrict_omits_swapped_columns()
    {
        var strict = BillPayIdSqlHelper.PairMatchStrictOnTable("f.BillID", "f.PaymentID");
        var full = BillPayIdSqlHelper.PairMatchOnTable("f.BillID", "f.PaymentID");
        Assert.Contains("REPLICATE('0', 13)", strict);
        Assert.True(full.Length > strict.Length);
    }

    [Fact]
    public void PairMatchStrictForExcelLookup_includes_tahator_fiche_pattern()
    {
        var sql = BillPayIdSqlHelper.PairMatchStrictForExcelLookup("f.BillID", "f.PaymentID", "f.FicheNo");
        Assert.Contains("f.FicheNo", sql);
        Assert.Contains("p.BillId", sql);
        Assert.Contains("p.PaymentId", sql);
    }

    [Fact]
    public void SwappedColumnsCaseSql_excludes_equal_bill_and_payment()
    {
        var sql = BillPayIdSqlHelper.SwappedColumnsCaseSql("f.BillID", "f.PaymentID", "p.BillId", "p.PaymentId");
        Assert.Contains("AND NOT", sql);
    }
}
