using RayvarzResend.Web.Models;

namespace RayvarzResend.Tests;

public class UnsentSearchPaginationTests
{
    [Theory]
    [InlineData(0, 0, 50)]
    [InlineData(1, 25, 25)]
    [InlineData(2, 50, 50)]
    [InlineData(0, 500, 100)]
    [InlineData(3, 200, 100)]
    public void NormalizedPageSize_clamps_between_1_and_100(int page, int pageSize, int expected)
    {
        var req = new UnsentFicheSearchRequest { Page = page, PageSize = pageSize };
        Assert.Equal(expected, req.NormalizedPageSize);
    }

    [Fact]
    public void Offset_uses_normalized_page_and_size()
    {
        var req = new UnsentFicheSearchRequest { Page = 3, PageSize = 50 };
        Assert.Equal(100, req.Offset);
        Assert.Equal(3, req.NormalizedPage);
    }

    [Fact]
    public void TotalPages_computed_from_total_count_and_page_size()
    {
        var result = new UnsentFicheSearchResult { TotalCount = 101, PageSize = 50 };
        Assert.Equal(3, result.TotalPages);
    }
}
