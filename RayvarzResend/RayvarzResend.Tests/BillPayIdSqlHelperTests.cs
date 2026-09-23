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
}
