using System.Reflection;
using Microsoft.CodeAnalysis.Scripting;

namespace Sara.RuleDebugger;

internal static class FaultFactory
{
    public static DebugFault From(
        Instruction inst,
        Exception ex,
        IReadOnlyList<DebugVariable> snapshot)
    {
        Exception root = Unwrap(ex);
        return new DebugFault
        {
            LineNumber = inst.Line,
            Column = inst.Column,
            FailingStatement = inst.Source,
            ExceptionType = root.GetType().FullName ?? root.GetType().Name,
            ErrorMessage = Explain(root, inst.Source),
            VariablesSnapshot = snapshot
        };
    }

    public static DebugFault Parse(int line, int column, string statement, string message)
    {
        return new DebugFault
        {
            LineNumber = line,
            Column = column,
            FailingStatement = statement,
            ExceptionType = "Microsoft.CodeAnalysis.Scripting.CompilationErrorException",
            ErrorMessage = "Script did not parse: " + message,
            VariablesSnapshot = Array.Empty<DebugVariable>()
        };
    }

    private static Exception Unwrap(Exception ex)
    {
        while (true)
        {
            if (ex is AggregateException agg && agg.InnerExceptions.Count == 1)
            {
                ex = agg.InnerExceptions[0];
                continue;
            }
            if (ex.InnerException != null && ex is TargetInvocationException or AggregateException)
            {
                ex = ex.InnerException;
                continue;
            }
            return ex;
        }
    }

    private static string Explain(Exception ex, string statement)
    {
        if (ex is DivideByZeroException)
            return "Division by zero at this statement: " + Quote(statement) + ". A denominator evaluated to 0.";
        if (ex is NullReferenceException)
            return "Null reference at this statement: " + Quote(statement) + ". A member was used on a null value.";
        if (ex is InvalidCastException or FormatException or OverflowException)
            return "Type mismatch at this statement: " + Quote(statement) + " — " + FirstLine(ex.Message);
        if (ex is Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
            return "Missing context property or member at this statement: " + Quote(statement) + " — " + FirstLine(ex.Message);
        if (ex is CompilationErrorException)
            return "The statement did not compile: " + Quote(statement) + " — " + FirstLine(ex.Message);
        if (ex is IndexOutOfRangeException or ArgumentOutOfRangeException)
            return "Index was out of range at this statement: " + Quote(statement) + " — " + FirstLine(ex.Message);
        return ex.GetType().Name + " at " + Quote(statement) + ": " + FirstLine(ex.Message);
    }

    private static string Quote(string s) => string.IsNullOrWhiteSpace(s) ? "(empty)" : "`" + s.Trim() + "`";

    private static string FirstLine(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        int i = s.IndexOf('\n');
        return (i < 0 ? s : s.Substring(0, i)).Trim();
    }
}
