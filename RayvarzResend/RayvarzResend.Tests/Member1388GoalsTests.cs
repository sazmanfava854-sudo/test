using Microsoft.Extensions.Configuration;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Executor;
using RayvarzResend.Web.RuleEngine.Parser;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class Member1388GoalsTests
{
    [Fact]
    public void RefParameterRegistry_maps_all_supported_names_to_fiche_and_rows()
    {
        var fiche = new FicheHeaderDto
        {
            FicheNo = "050733453546",
            Rows = { new IncmRowDto { IncmNo = 1, Val = 100m } }
        };
        var warnings = new List<string>();

        var result = RefParameterRegistry.ApplyAll(fiche,
        [
            new RefParameter { Name = "Center", Value = "910700001" },
            new RefParameter { Name = "Center1", Value = "335000046" },
            new RefParameter { Name = "Center2", Value = "800800007" },
            new RefParameter { Name = "Center3", Value = "910700001" },
            new RefParameter { Name = "Fund", Value = "200207009" },
            new RefParameter { Name = "DocDate", Value = "14050323" },
            new RefParameter { Name = "RowDate", Value = "1405/03/24" },
            new RefParameter { Name = "RowDocNo", Value = "DOC-99" },
            new RefParameter { Name = "RefReconstructionNo", Value = "REC-1" },
            new RefParameter { Name = "RefownrDsc", Value = "OWNER-1" },
            new RefParameter { Name = "Ref", Value = "ROW-REF" },
            new RefParameter { Name = "Ref2", Value = "R2" },
            new RefParameter { Name = "Ref3", Value = "R3" },
            new RefParameter { Name = "Ref6", Value = "R6" },
            new RefParameter { Name = "vchrtyp", Value = "0" },
            new RefParameter { Name = "PhasType", Value = "7" },
            new RefParameter { Name = "DUE", Value = "14050401" },
            new RefParameter { Name = "QTY", Value = "99" }
        ], warnings);

        Assert.True(result.AppliedCount >= 17, string.Join(", ", warnings));
        Assert.Empty(result.UnknownNames);
        Assert.Equal(910700001L, fiche.Center);
        Assert.Equal(335000046L, fiche.Rows[0].Center1);
        Assert.Equal(200207009, fiche.SuggestedFund);
        Assert.Equal("14050323", fiche.RayvarzDocDate);
        Assert.Equal("14050324", fiche.RayvarzActDate);
        Assert.Equal("DOC-99", fiche.RefRowDocNo);
        Assert.Equal("OWNER-1", fiche.RefOwnerDsc);
        Assert.Equal("ROW-REF", fiche.Rows[0].Ref);
    }

    [Fact]
    public void RefParameterRegistry_unknown_name_warns_without_failing()
    {
        var fiche = new FicheHeaderDto();
        var warnings = new List<string>();
        var result = RefParameterRegistry.ApplyAll(fiche,
        [
            new RefParameter { Name = "UnknownRefX", Value = "1" },
            new RefParameter { Name = "Fund", Value = "200207009" }
        ], warnings);

        Assert.Single(result.UnknownNames);
        Assert.Equal(1, result.AppliedCount);
        Assert.Contains(warnings, w => w.Contains("UnknownRefX"));
        Assert.Equal(200207009, fiche.SuggestedFund);
    }

    [Fact]
    public void RefParameters_flow_to_soap_xml()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Rayvarz:SoapAction"] = "http://tempuri.org/IReceiveIncmVchrServices/SaveDocument",
                ["Rayvarz:SourceSystemId"] = "1"
            })
            .Build();
        var soap = new SoapBuilder(config);
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            FicheNo = "050733453546",
            Payable = 1_000_000m,
            SuggestedFund = 200207009,
            RayvarzDocDate = "14050323",
            RayvarzActDate = "14050324",
            RefRowDocNo = "DOC-REF",
            RefOwnerDsc = "OWNER-SOAP",
            Center = 910700001,
            Rows = { new IncmRowDto { IncmNo = 1025, Val = 1_000_000m, Center1 = 335000046 } }
        };

        var xml = soap.Build(fiche, 207, 0, null, null, null);

        Assert.Contains("<b:Fund>200207009</b:Fund>", xml);
        Assert.Contains("<b:DocDate>14050323</b:DocDate>", xml);
        Assert.Contains("<b:RowDocNo>DOC-REF</b:RowDocNo>", xml);
        Assert.Contains("<b:RefownrDsc>OWNER-SOAP</b:RefownrDsc>", xml);
        Assert.Contains("<b:Center>910700001</b:Center>", xml);
    }

    [Fact]
    public void BazAfarineOld_uses_only_allowed_codes_not_full_exclusion_list()
    {
        var fiche = new FicheHeaderDto
        {
            Payable = 500_000m,
            Rows =
            {
                new IncmRowDto { IncmNo = 100098, Val = 1m },
                new IncmRowDto { IncmNo = 1025, Val = 499_999m },
                new IncmRowDto { IncmNo = 100107, Val = 1m }
            }
        };

        Member1388IncomeRowProfiles.ApplyBazAfarine(fiche);
        var bazRows = fiche.Rows.Select(r => r.IncmNo).OrderBy(x => x).ToList();

        fiche.Rows =
        [
            new IncmRowDto { IncmNo = 100098, Val = 1m },
            new IncmRowDto { IncmNo = 1025, Val = 499_999m },
            new IncmRowDto { IncmNo = 100107, Val = 1m }
        ];
        Member1388IncomeRowProfiles.ApplyBazAfarineOld(fiche);
        var oldRows = fiche.Rows.Select(r => r.IncmNo).OrderBy(x => x).ToList();

        Assert.Contains(1025, bazRows);
        Assert.DoesNotContain(1025, oldRows);
        Assert.Equal([100098, 100107], oldRows);
        Assert.All(fiche.Rows, r => Assert.Equal(500_000m, r.Val));
    }

    [Fact]
    public void BazAfarineOld_centers_match_BazAfarine_for_same_district()
    {
        var baseFiche = new FicheHeaderDto
        {
            ResolvedDistrictBranch = 7,
            Rows = { new IncmRowDto { IncmNo = 100098, Val = 100m } }
        };
        var oldFiche = new FicheHeaderDto
        {
            ResolvedDistrictBranch = 7,
            Rows = { new IncmRowDto { IncmNo = 100098, Val = 100m } }
        };

        Member1388IncomeCenterResolver.ApplyBazAfarine(baseFiche);
        Member1388IncomeCenterResolver.ApplyBazAfarineOld(oldFiche);

        Assert.Equal(baseFiche.Center, oldFiche.Center);
        Assert.Equal(baseFiche.Rows[0].Center1, oldFiche.Rows[0].Center1);
        Assert.Equal(baseFiche.Rows[0].Center2, oldFiche.Rows[0].Center2);
        Assert.Equal(baseFiche.Rows[0].Center3, oldFiche.Rows[0].Center3);
    }

    [Fact]
    public void Ast_run_interpreter_derives_call_order_from_program_not_catalog()
    {
        var program = BuildMinimalIncomeProgram(includeOragh: false);
        var executor = new DslExecutor(SaraOperationBootstrap.CreateDefault());
        var fiche = BuildIncomeFiche();

        var result = executor.Execute(program, new DslExecutionContext
        {
            Program = program,
            Fiche = fiche,
            DryRun = true,
            Member1388FullExecution = true
        });

        Assert.True(result.Success, result.ErrorMessage);
        Assert.Equal(["iNcOME", "BazAfarine"], result.AppliedFunctions);
        Assert.DoesNotContain("fallback", string.Join(" ", result.Trace), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Ast_run_change_in_vb_call_order_changes_applied_functions_without_csharp_edit()
    {
        var programWithoutOragh = BuildMinimalIncomeProgram(includeOragh: false);
        var programWithOragh = BuildMinimalIncomeProgram(includeOragh: true);
        var executor = new DslExecutor(SaraOperationBootstrap.CreateDefault());
        var fiche = BuildIncomeFiche();

        var without = executor.Execute(programWithoutOragh, Context(programWithoutOragh, fiche));
        var with = executor.Execute(programWithOragh, Context(programWithOragh, fiche));

        Assert.True(without.Success);
        Assert.True(with.Success);
        Assert.Equal(2, without.AppliedFunctions.Count);
        Assert.Equal(3, with.AppliedFunctions.Count);
        Assert.Contains("iNcOMEOragh", with.AppliedFunctions);
        Assert.DoesNotContain("iNcOMEOragh", without.AppliedFunctions);
    }

    [Fact]
    public void Ast_ref_replay_applies_fund_from_function_body_to_soap()
    {
        var program = BuildRefParamIncomeProgram();
        var executor = new DslExecutor(SaraOperationBootstrap.CreateDefault());
        var fiche = BuildIncomeFiche();

        var result = executor.Execute(program, Context(program, fiche));
        Assert.True(result.Success, result.ErrorMessage);
        Assert.Equal(200207009, fiche.SuggestedFund);
        Assert.Equal("14050505", fiche.RayvarzDocDate);
    }

    [Fact]
    public void Unsupported_run_statement_produces_compatibility_warning()
    {
        var program = new DslProgram
        {
            EntryPoint = "Run",
            Functions =
            [
                new DslFunction
                {
                    Name = "Run",
                    Body =
                    [
                        new DslUnsupportedStatement("SomeLegacyVB", "Call UnknownLegacyOp()"),
                        new DslCallFunctionStatement("iNcOME", [])
                    ]
                },
                IncomeFunctionStub("iNcOME")
            ]
        };

        var executor = new DslExecutor(SaraOperationBootstrap.CreateDefault());
        var result = executor.Execute(program, Context(program, BuildIncomeFiche()));

        Assert.True(result.Success);
        Assert.Contains(result.CompatibilityWarnings, w => w.Contains("Run defer"));
        Assert.Contains("iNcOME", result.AppliedFunctions);
    }

    private static DslExecutionContext Context(DslProgram program, FicheHeaderDto fiche) => new()
    {
        Program = program,
        Fiche = fiche,
        DryRun = true,
        Member1388FullExecution = true
    };

    private static FicheHeaderDto BuildIncomeFiche() => new()
    {
        Category = FicheCategory.Income,
        IncomeAccountGroup = 150,
        FicheNo = "050733453546",
        Payable = 1_000_000m,
        BankCode = "18",
        Rows =
        {
            new IncmRowDto { IncmNo = 1025, Val = 600_000m },
            new IncmRowDto { IncmNo = 1271, Val = 400_000m }
        }
    };

    private static DslProgram BuildMinimalIncomeProgram(bool includeOragh)
    {
        var calls = new List<DslStatement> { new DslCallFunctionStatement("iNcOME", []) };
        if (includeOragh)
            calls.Add(new DslCallFunctionStatement("iNcOMEOragh", []));
        calls.Add(new DslCallFunctionStatement("BazAfarine", []));

        return new DslProgram
        {
            EntryPoint = "Run",
            Functions =
            [
                new DslFunction
                {
                    Name = "Run",
                    Body =
                    [
                        new DslIfStatement(
                            "AccountingDocumentingCause = Confirm And ObjOnPrice = Income",
                            calls,
                            [],
                            null)
                    ]
                },
                IncomeFunctionStub("iNcOME"),
                IncomeFunctionStub("iNcOMEOragh"),
                IncomeFunctionStub("BazAfarine")
            ]
        };
    }

    private static DslProgram BuildRefParamIncomeProgram() => new()
    {
        EntryPoint = "Run",
        Functions =
        [
            new DslFunction
            {
                Name = "Run",
                Body =
                [
                    new DslIfStatement(
                        "AccountingDocumentingCause = Confirm And ObjOnPrice = Income",
                        [new DslCallFunctionStatement("iNcOME", [])],
                        [],
                        null)
                ]
            },
            new DslFunction
            {
                Name = "iNcOME",
                Body =
                [
                    new DslAssignStatement("RefFund.Name", "\"Fund\""),
                    new DslAssignStatement("RefFund.Value", "\"200207009\""),
                    new DslCallOperationStatement("Add", "ListRefP", ["RefFund"]),
                    new DslAssignStatement("RefP.Name", "\"DocDate\""),
                    new DslAssignStatement("RefP.Value", "\"14050505\""),
                    new DslCallOperationStatement("Add", "ListRefP", ["RefP"])
                ]
            }
        ]
    };

    private static DslFunction IncomeFunctionStub(string name) => new()
    {
        Name = name,
        Body = []
    };
}
