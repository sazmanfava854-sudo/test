using System.Globalization;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Executor;
using RayvarzResend.Web.RuleEngine.Parser;
using Xunit;

namespace RayvarzResend.Tests;

public class Member1388HelperFunctionsTests
{
    [Theory]
    [InlineData("1399/01/15", "13990115")]
    [InlineData("1400/12/29", "14001229")]
    public void ChangeDate_converts_shamsi_to_compact(string input, string expected)
    {
        Assert.Equal(expected, Member1388HelperFunctions.ChangeDate(input));
    }

    [Fact]
    public void GetSara8Workflow_maps_nidproc_to_archive_group()
    {
        var ctx = new DslExecutionContext();
        var nid = Guid.Parse("53379B94-1411-4DC8-AEE1-012D4A9B43A7");

        var group = Member1388HelperFunctions.GetSara8Workflow(nid, ctx);

        Assert.Equal(2, group);
        Assert.Equal(2, ctx.Variables["M_ShahrsaziArchiveGroup"]);
    }

    [Fact]
    public void GetSara8Workflow_unknown_nidproc_returns_zero()
    {
        var ctx = new DslExecutionContext();
        Assert.Equal(0, Member1388HelperFunctions.GetSara8Workflow(Guid.NewGuid(), ctx));
    }

    [Theory]
    [InlineData("1399/01/01", "1399/01/11", 3, 10)]
    [InlineData("1399/01/01", "1399/01/01", 3, 0)]
    public void GetDiffDate_mood3_returns_day_difference(string d1, string d2, int mood, int expected)
    {
        Assert.Equal(expected, Member1388HelperFunctions.GetDiffDate(d1, d2, mood));
    }

    [Fact]
    public void AddDateForHolidays_adds_calendar_days_without_holidays()
    {
        var result = Member1388HelperFunctions.AddDateForHolidays("1399/01/01", 10, aa: 1);
        Assert.Equal("1399/01/11", result);
    }

    [Fact]
    public void AddDateForHolidays_skips_holidays()
    {
        var calendar = new TestHolidayCalendar("1399/01/05");
        var result = (string)Member1388HelperFunctions.AddDateForHolidays(
            "1399/01/01", 5, aa: 1, holidayCalendar: calendar);
        Assert.Equal("1399/01/07", result);
    }

    [Fact]
    public void AddDateForHolidays_returns_datetime_when_aa_not_1()
    {
        var result = Member1388HelperFunctions.AddDateForHolidays("1399/01/01", 1, aa: 0);
        Assert.IsType<DateTime>(result);
        var dt = (DateTime)result;
        var pc = new PersianCalendar();
        Assert.Equal(1399, pc.GetYear(dt));
        Assert.Equal(1, pc.GetMonth(dt));
        Assert.Equal(2, pc.GetDayOfMonth(dt));
    }

    [Fact]
    public void FnSms_and_Logfile_write_to_helper_trace_only()
    {
        var ctx = new DslExecutionContext();
        Member1388HelperFunctions.FnSms("test-sms", ctx);
        Member1388HelperFunctions.Logfile("ray", "payload", ctx);

        Assert.Equal(2, ctx.HelperTrace.Count);
        Assert.Contains(ctx.HelperTrace, t => t.Contains("FnSMS"));
        Assert.Contains(ctx.HelperTrace, t => t.Contains("Logfile(ray)"));
    }

    [Fact]
    public void Execute_Run_dispatches_income_chain_when_confirm()
    {
        var program = LoadFullProgram();
        var executor = new DslExecutor(SaraOperationBootstrap.CreateDefault());
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            AccountingDocumentingCause = Member1388AccountingCause.Confirm,
            IncomeAccountGroup = 150,
            Payable = 1_000_000m,
            BankCode = "18",
            Rows =
            {
                new IncmRowDto { IncmNo = 1025, Val = 600_000m },
                new IncmRowDto { IncmNo = 1271, Val = 400_000m }
            }
        };

        var result = executor.Execute(program, new DslExecutionContext
        {
            Fiche = fiche,
            DryRun = true,
            Member1388FullExecution = true
        });

        Assert.True(result.Success, result.ErrorMessage);
        Assert.Equal(Member1388Catalog.RunIncomeCallOrder, result.AppliedFunctions);
    }

    [Fact]
    public void Execute_Run_dispatches_income_check_when_cause_7()
    {
        var program = LoadFullProgram();
        var executor = new DslExecutor(SaraOperationBootstrap.CreateDefault());
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            AccountingDocumentingCause = Member1388AccountingCause.InstallmentCheck,
            CurrentStatus = 5,
            Payable = 500_000m,
            ExportPermanentDate = "1400/01/01",
            PaymentBreakDate = "1400/01/20",
            BankPaymentDate = "1400/01/10",
            Rows = { new IncmRowDto { IncmNo = 1025, Val = 500_000m } }
        };

        var result = executor.Execute(program, new DslExecutionContext
        {
            Fiche = fiche,
            DryRun = true,
            Member1388FullExecution = true
        });

        Assert.Contains("iNcOMECheck", result.AppliedFunctions);
        Assert.DoesNotContain("iNcOME", result.AppliedFunctions);
    }

    [Fact]
    public void Execute_Run_dispatches_nosazi_for_duty_fiche()
    {
        var program = LoadFullProgram();
        var executor = new DslExecutor(SaraOperationBootstrap.CreateDefault());
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.DutyNosazi,
            AccountingDocumentingCause = Member1388AccountingCause.Confirm,
            Payable = 100_000m,
            Rows = { new IncmRowDto { IncmNo = 1, Val = 100_000m } }
        };

        var result = executor.Execute(program, new DslExecutionContext
        {
            Fiche = fiche,
            DryRun = true,
            Member1388FullExecution = true
        });

        Assert.Contains("Nosazi", result.AppliedFunctions);
    }

    [Fact]
    public void Helper_executor_ChangeDate_sets_result_variable()
    {
        var ctx = new DslExecutionContext();
        ctx.Variables["HelperArg0"] = "1399/01/15";

        var result = Member1388FunctionExecutor.Execute("ChangeDate", ctx, SaraOperationBootstrap.CreateDefault());

        Assert.True(result.HadEffect);
        Assert.Equal("13990115", ctx.Variables["ChangeDateResult"]);
    }

    private static DslProgram LoadFullProgram()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "member-1388-full-body.vb");
        var vb = File.ReadAllText(path);
        var wrapped =
            "<?xml version=\"1.0\"?><ClsFunction><NidClass>360</NidClass><NidFunction>1388</NidFunction>" +
            $"<Name>Run</Name><Body>{System.Security.SecurityElement.Escape(vb)}</Body></ClsFunction>";
        return VbTranspiler.Transpile(XmlEnvelopeReader.Read(wrapped, "full-body").Document);
    }

    private sealed class TestHolidayCalendar : IMember1388HolidayCalendar
    {
        private readonly HashSet<string> _holidays;

        public TestHolidayCalendar(params string[] holidays) =>
            _holidays = new HashSet<string>(holidays, StringComparer.Ordinal);

        public bool IsHoliday(string shamsiDate) => _holidays.Contains(shamsiDate);
    }
}
