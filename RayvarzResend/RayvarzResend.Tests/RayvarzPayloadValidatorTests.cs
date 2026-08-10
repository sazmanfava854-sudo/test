using Microsoft.Extensions.Configuration;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using RayvarzResend.Web.Validation;
using Xunit;

namespace RayvarzResend.Tests;

public class RayvarzPayloadValidatorTests
{
    private readonly RayvarzSoapPayloadValidator _validator = new();

    [Fact]
    public void Critical_missing_DocDate_blocks_send()
    {
        var fiche = BuildValidIncomeFiche();
        var xml = BuildSoapXml(fiche);

        var broken = xml.Replace("<b:DocDate>14050323</b:DocDate>", "<b:DocDate></b:DocDate>");
        var result = _validator.Validate(new RayvarzValidationInput
        {
            Fiche = fiche,
            SoapXml = broken,
            Branch = 207,
            Fund = 200207009
        });

        Assert.False(result.CanSend);
        var docDateIssue = Assert.Single(result.BlockingIssues, i => i.Code == "HDR_DOCDATE_REQUIRED");
        Assert.True(docDateIssue.Blocking);
    }

    [Fact]
    public void Compatibility_warning_does_not_block_send()
    {
        var fiche = BuildValidIncomeFiche();
        var xml = BuildSoapXml(fiche);

        var result = _validator.Validate(new RayvarzValidationInput
        {
            Fiche = fiche,
            SoapXml = xml,
            Branch = 207,
            Fund = 200207009,
            CompatibilityWarnings = ["Run defer: SomeLegacyVB line"]
        });

        Assert.True(result.CanSend);
        Assert.Contains(result.Warnings, w => w.Code == "DSL_COMPATIBILITY_DEFER");
        Assert.DoesNotContain(result.BlockingIssues, i => i.Code == "DSL_COMPATIBILITY_DEFER");
    }

    [Fact]
    public void Critical_row_sum_mismatch_blocks_send()
    {
        var fiche = BuildValidIncomeFiche();
        fiche.Rows[0].Val = 100m;

        var result = _validator.Validate(new RayvarzValidationInput { Fiche = fiche });

        Assert.False(result.CanSend);
        Assert.Contains(result.BlockingIssues, i => i.Code == "FIN_ROW_SUM_PAYABLE");
    }

    [Fact]
    public void Issue_records_code_field_operation_severity_blocking()
    {
        var fiche = BuildValidIncomeFiche();
        fiche.Payable = 0;

        var result = _validator.Validate(new RayvarzValidationInput { Fiche = fiche });

        var issue = Assert.Single(result.BlockingIssues, i => i.Code == "BIZ_PAYABLE_ZERO");
        Assert.Equal("Payable", issue.Field);
        Assert.Equal(RayvarzFieldRequirementCatalog.OpPreSend, issue.Operation);
        Assert.Equal(RayvarzValidationSeverity.Critical, issue.Severity);
        Assert.True(issue.Blocking);
    }

    [Fact]
    public void Valid_soap_passes_critical_checks()
    {
        var fiche = BuildValidIncomeFiche();
        var xml = BuildSoapXml(fiche);

        var result = _validator.Validate(new RayvarzValidationInput
        {
            Fiche = fiche,
            SoapXml = xml,
            Branch = 207,
            Fund = 200207009
        });

        Assert.True(result.CanSend, string.Join("; ", result.BlockingIssues.Select(i => i.Message)));
        Assert.Empty(result.BlockingIssues);
    }

    [Fact]
    public void Soap_inspector_parses_header_and_incm_rows()
    {
        var fiche = BuildValidIncomeFiche();
        var parsed = RayvarzSoapXmlInspector.TryParse(BuildSoapXml(fiche));

        Assert.NotNull(parsed);
        Assert.Equal("14050323", parsed!.DocDate);
        Assert.NotEmpty(parsed.IncmRows);
        Assert.Equal("1025", parsed.IncmRows[0].IncmNo);
    }

    [Fact]
    public void Soap_includes_source_system_id_marker_when_configured()
    {
        var fiche = BuildValidIncomeFiche();
        var xml = BuildSoapXml(fiche);
        Assert.Contains("<b:SourceId>RAYVARZ-RESEND</b:SourceId>", xml);
    }

    private static FicheHeaderDto BuildValidIncomeFiche() => new()
    {
        Category = FicheCategory.Income,
        IncomeAccountGroup = 150,
        FicheNo = "050733453546",
        Payable = 1_000_000m,
        BankCode = "18",
        BnkAcntNo = "9-1-1-0-0-0-0",
        DocTyp = 1,
        DocDsc = "فیش تست",
        DocTypDsc = "فیش",
        NidFiche = Guid.NewGuid(),
        RayvarzDocDate = "14050323",
        RayvarzActDate = "14050323",
        BillId = "123",
        PaymentId = "456",
        ResolvedDistrictBranch = 7,
        Rows =
        {
            new IncmRowDto { IncmNo = 1025, Val = 600_000m, IncmRowDsc = "ردیف ۱" },
            new IncmRowDto { IncmNo = 1271, Val = 400_000m, IncmRowDsc = "ردیف ۲" }
        }
    };

    private static string BuildSoapXml(FicheHeaderDto fiche)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Rayvarz:SoapAction"] = "http://tempuri.org/IReceiveIncmVchrServices/SaveDocument",
                ["Rayvarz:SourceSystemId"] = "RAYVARZ-RESEND",
                ["Rayvarz:TransactionIdMode"] = "nidFiche"
            })
            .Build();
        var soap = new SoapBuilder(config);
        return soap.Build(fiche, 207, 200207009, "14050323", "14050323", "14050324");
    }
}

public class FicheSendPreviewTests
{
    [Fact]
    public void Send_path_throws_when_validation_has_blocking_issues()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            FicheNo = "1",
            Payable = 0,
            Rows = { new IncmRowDto { IncmNo = 1, Val = 0 } }
        };

        var validator = new RayvarzSoapPayloadValidator();
        var result = validator.Validate(new RayvarzValidationInput { Fiche = fiche });

        Assert.False(result.CanSend);

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            if (!result.CanSend)
                throw new InvalidOperationException(
                    string.Join("; ", result.BlockingIssues.Select(i => $"[{i.Code}] {i.Message}")));
        });

        Assert.Contains("BIZ_PAYABLE_ZERO", ex.Message);
    }
}
