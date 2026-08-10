using System.Globalization;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Parser;

namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>توابع کمکی Member 1388 — پورت از member-1388-full-body.vb.</summary>
public static class Member1388HelperFunctions
{
    /// <summary>VB ChangeDate — تبدیل yyyy/MM/dd به yyyyMMdd.</summary>
    public static string ChangeDate(string d1)
    {
        if (string.IsNullOrWhiteSpace(d1) || d1.Length < 10)
            return "";

        return d1[..4] + d1.Substring(5, 2) + d1.Substring(8, 2);
    }

    /// <summary>VB GetSara8Workflow — NidProc → M_ShahrsaziArchiveGroup.</summary>
    public static int GetSara8Workflow(Guid? nidProc, DslExecutionContext context)
    {
        var group = Member1388WorkflowCatalog.ResolveArchiveGroup(nidProc);
        context.Variables["M_ShahrsaziArchiveGroup"] = group;
        return group;
    }

    /// <summary>VB GetDiffDate — Mood 1=ماه، 2=سال، 3=روز.</summary>
    public static int GetDiffDate(string date1, string date2, int mood)
    {
        if (!TryParseShamsi(date1, out var y1, out var m1, out var d1)
            || !TryParseShamsi(date2, out var y2, out var m2, out var d2))
            return 0;

        return mood switch
        {
            1 => DiffMonths(y1, m1, d1, y2, m2, d2),
            2 => DiffYears(y1, m1, m2, y2),
            3 => DiffDaysVbParity(y1, m1, d1, y2, m2, d2),
            _ => 0
        };
    }

    /// <summary>VB AddDateForHolidays — aa=1 تاریخ شمسی، در غیر این صورت DateTime میلادی.</summary>
    public static object AddDateForHolidays(
        string dateTime,
        int day,
        int aa = 0,
        IMember1388HolidayCalendar? holidayCalendar = null)
    {
        holidayCalendar ??= EmptyMember1388HolidayCalendar.Instance;
        var tmpD = dateTime;
        var i = 0;

        while (i < day)
        {
            tmpD = IncrementShamsiDay(tmpD);
            if (!holidayCalendar.IsHoliday(tmpD))
                i++;
        }

        if (aa == 1)
            return tmpD;

        var parts = tmpD.Split('/');
        var calendar = new PersianCalendar();
        return new DateTime(
            int.Parse(parts[0]),
            int.Parse(parts[1]),
            int.Parse(parts[2]),
            calendar);
    }

    /// <summary>VB FnSMS — در محیط Resend فقط trace؛ SMS واقعی ارسال نمی‌شود.</summary>
    public static void FnSms(string paramText, DslExecutionContext context)
    {
        context.HelperTrace.Add($"FnSMS: {paramText}");
    }

    /// <summary>VB Logfile — در محیط Resend فقط trace؛ فایل روی دیسک نوشته نمی‌شود.</summary>
    public static void Logfile(string name, string content, DslExecutionContext context)
    {
        context.HelperTrace.Add($"Logfile({name}): {content}");
    }

    /// <summary>VB Run — dispatch از AST در صورت وجود؛ fallback به catalog با warning.</summary>
    public static bool DispatchRun(DslExecutionContext context, IOperationRegistry registry, IList<string> trace)
    {
        context.Variables["M_ShahrsaziArchiveGroup"] = 0;

        if (context.Program is not null
            && Member1388AstRunInterpreter.TryExecute(context.Program, context, registry, trace, out var astDriven)
            && astDriven)
        {
            trace.Add("Run: زنجیره از AST");
            return context.FunctionsWithEffect.Count > 0 || context.InvokedFunctions.Count > 0;
        }

        var cause = context.Fiche.AccountingDocumentingCause ?? Member1388AccountingCause.Confirm;

        if (cause == Member1388AccountingCause.InstallmentCheck)
        {
            Member1388RunDispatcher.DispatchChild("iNcOMECheck", context, registry, trace);
            trace.Add("Run: AccountingDocumentingCause=7 → iNcOMECheck");
            return true;
        }

        if (cause != Member1388AccountingCause.Confirm)
        {
            trace.Add($"Run: AccountingDocumentingCause={cause} — بدون dispatch");
            return false;
        }

        if (context.Fiche.Category == FicheCategory.Income)
        {
            context.CompatibilityWarnings.Add("Run: AST در دسترس نیست — fallback به Member1388Catalog.ResolveIncomeCallOrder");
            foreach (var fn in Member1388Catalog.ResolveIncomeCallOrder(context.Fiche))
                Member1388RunDispatcher.DispatchChild(fn, context, registry, trace);
            trace.Add("Run: Confirm + Income → زنجیره catalog (fallback)");
            return context.FunctionsWithEffect.Count > 0;
        }

        if (context.Fiche.Category is FicheCategory.DutyNosazi or FicheCategory.DutySenfi)
        {
            Member1388RunDispatcher.DispatchChild("Nosazi", context, registry, trace);
            trace.Add("Run: Confirm + Duty → Nosazi");
            return true;
        }

        trace.Add("Run: Confirm — نوع فیش نامشخص");
        return false;
    }

    private static bool TryParseShamsi(string value, out int y, out int m, out int d)
    {
        y = m = d = 0;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var parts = value.Trim().Split('/');
        if (parts.Length != 3)
            return false;

        return int.TryParse(parts[0], out y)
               && int.TryParse(parts[1], out m)
               && int.TryParse(parts[2], out d);
    }

    private static int DiffMonths(int y1, int m1, int d1, int y2, int m2, int d2)
    {
        if (y2 < y1)
            return 0;

        if (m2 <= m1 && y2 == y1)
            return 0;

        if (m2 > m1 && y2 == y1)
        {
            var outp = m2 - m1;
            if (d2 > d1) outp++;
            return outp;
        }

        if (y2 <= y1)
            return 0;

        if (m2 >= m1)
        {
            var outp = (y2 - y1) * 12 + (m2 - m1);
            if (d2 > d1) outp++;
            return outp;
        }

        var outAlt = (y2 - y1) * 12;
        outAlt = m2 + outAlt - m1;
        if (d2 > d1) outAlt++;
        return outAlt;
    }

    private static int DiffYears(int y1, int m1, int m2, int y2)
    {
        if (y2 <= y1)
            return 0;

        var outp = y2 - y1;
        if (m2 <= m1)
            outp--;
        return outp;
    }

    private static int DiffDaysVbParity(int y1, int m1, int d1, int y2, int m2, int d2)
    {
        var day1 = YearBaseDays(y1);
        day1 += MonthOffsetDays(m1) + d1;

        var day2 = YearBaseDaysForDate2(y1, y2, ref day1);
        day2 += MonthOffsetDays(m2) + d2;

        var diff = day2 - day1;
        return diff < 0 ? 0 : (int)diff;
    }

    private static long YearBaseDays(int year) => year switch
    {
        1388 => 0,
        1389 => 365,
        1390 => 365 * 2,
        1391 => 365 * 3,
        1392 => (365 * 3) + 366,
        1393 => (365 * 4) + 366,
        1394 => (365 * 5) + 366,
        1395 => (365 * 6) + 366,
        1396 => (365 * 7) + 366,
        1397 => (365 * 8) + 366,
        _ => 0
    };

    /// <summary>باگ VB در Case 1396/1397 برای Date2 — Day1 به‌جای Day2 ست می‌شود.</summary>
    private static long YearBaseDaysForDate2(int y1, int y2, ref long day1)
    {
        return y2 switch
        {
            1388 => 0,
            1389 => 365,
            1390 => 365 * 2,
            1391 => 365 * 3,
            1392 => (365 * 3) + 366,
            1393 => (365 * 4) + 366,
            1394 => (365 * 5) + 366,
            1395 => (365 * 6) + 366,
            1396 => day1 = (365 * 7) + 366,
            1397 => day1 = (365 * 8) + 366,
            _ => 0
        };
    }

    private static int MonthOffsetDays(int month) =>
        month < 7 ? (month - 1) * 31 : (6 * 31) + (month - 7) * 30;

    private static string IncrementShamsiDay(string shamsiDate)
    {
        var parts = shamsiDate.Split('/');
        var year = int.Parse(parts[0]);
        var month = int.Parse(parts[1]);
        var day = int.Parse(parts[2]);

        day++;
        if (day > 31 && month < 7)
        {
            month++;
            day -= 31;
        }
        else if (day > 30 && month > 6 && month != 12)
        {
            month++;
            day -= 30;
        }
        else if (day > 29 && month == 12)
        {
            month = 1;
            year++;
            day -= 29;
        }

        return $"{year:0000}/{month:00}/{day:00}";
    }
}

/// <summary>فراخوانی زنجیره Run از Member1388FunctionExecutor.</summary>
internal static class Member1388RunDispatcher
{
    public static void DispatchChild(
        string functionName,
        DslExecutionContext context,
        IOperationRegistry registry,
        IList<string> trace)
    {
        functionName = ResolveRunCallName(functionName, context.Fiche);

        if (!context.InvokedFunctions.Contains(functionName, StringComparer.OrdinalIgnoreCase))
            context.InvokedFunctions.Add(functionName);

        var role = SupportedDslFunctions.GetRole(functionName);
        trace.Add($"→ {functionName}() [اعمال قانون role={role}]");
        context.DispatchedFunction = functionName;

        var result = Member1388FunctionExecutor.Execute(functionName, context, registry);
        foreach (var line in result.Trace)
            trace.Add(line);

        if (result.HadEffect && !context.FunctionsWithEffect.Contains(functionName, StringComparer.OrdinalIgnoreCase))
            context.FunctionsWithEffect.Add(functionName);
    }

    /// <summary>VB Run همیشه BazAfarine() را صدا می‌زند؛ UseBazAfarineOld در C# جایگزینی است.</summary>
    internal static string ResolveRunCallName(string functionName, FicheHeaderDto fiche) =>
        fiche.UseBazAfarineOld
        && functionName.Equals("BazAfarine", StringComparison.OrdinalIgnoreCase)
            ? "BazAfarineOld"
            : functionName;
}
