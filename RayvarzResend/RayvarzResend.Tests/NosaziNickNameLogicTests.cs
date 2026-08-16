using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class NosaziNickNameLogicTests
{
    [Fact]
    public void ApplyLoadResult_sets_nick_and_source_when_found()
    {
        var dto = new FicheHeaderDto
        {
            BnkAcntNo = "fallback-from-xml",
            BnkAcntNoSource = "کد نوسازی — از Duty_Fiche.OtherFields (XML فیش)"
        };

        NosaziNickNameLogic.ApplyLoadResult(dto, "9-8-72-47-1-0-2-0", null);

        Assert.Equal("9-8-72-47-1-0-2-0", dto.BnkAcntNo);
        Assert.Equal(NosaziNickNameLogic.NickSource, dto.BnkAcntNoSource);
        Assert.Null(dto.Warning);
    }

    [Fact]
    public void ApplyLoadResult_sets_warning_on_sql_error_and_keeps_fallback_bnkAcntNo()
    {
        var dto = new FicheHeaderDto
        {
            BnkAcntNo = "9-1-2-3-4-5-6-7",
            BnkAcntNoSource = "کد نوسازی — از Duty_Fiche.OtherFields (XML فیش)"
        };

        var err = NosaziNickNameLogic.FormatSqlFailureWarning("Login failed for user");
        NosaziNickNameLogic.ApplyLoadResult(dto, null, err);

        Assert.Equal("9-1-2-3-4-5-6-7", dto.BnkAcntNo);
        Assert.Contains("GetNosaziNickName ناموفق", dto.Warning);
        Assert.Contains("OtherFields", dto.Warning);
    }

    [Fact]
    public void ApplyLoadResult_no_warning_when_nick_missing_without_error()
    {
        var dto = new FicheHeaderDto { BnkAcntNo = "xml-code" };
        NosaziNickNameLogic.ApplyLoadResult(dto, null, null);
        Assert.Null(dto.Warning);
        Assert.Equal("xml-code", dto.BnkAcntNo);
    }
}
