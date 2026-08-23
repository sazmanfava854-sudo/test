using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class InstallmentListQueryTests
{
    [Fact]
    public void BuildExcelLookupSql_uses_joined_tables_and_nosazi_formula()
    {
        var sql = InstallmentListQuery.BuildExcelLookupSql("NoDocument");

        Assert.Contains("FROM dbo.Income i", sql);
        Assert.Contains("INNER JOIN dbo.Income_Fiche f", sql);
        Assert.Contains("INNER JOIN dbo.Installment ins", sql);
        Assert.Contains("INNER JOIN dbo.Installment_List il", sql);
        Assert.Contains("INNER JOIN dbo.Sh_RequestInfo r", sql);
        Assert.Contains("INNER JOIN dbo.Base_NosaziCode b", sql);
        Assert.Contains("CAST(r.NidWorkItem AS nvarchar(50)) AS nidworkitem", sql);
        Assert.Contains("il.TrackingNo AS trackingno", sql);
        Assert.Contains("'0' + '-' + '-'", sql);
        Assert.Contains("WHERE il.NoDocument = @v", sql);
    }

    [Fact]
    public void BuildExcelLookupSql_supports_TrackingNo_column()
    {
        var sql = InstallmentListQuery.BuildExcelLookupSql("TrackingNo");
        Assert.Contains("'0' + @v", sql);
        Assert.DoesNotContain("WHERE il.TrackingNo = @v", sql);
    }
}
