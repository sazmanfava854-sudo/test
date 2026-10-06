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
}
