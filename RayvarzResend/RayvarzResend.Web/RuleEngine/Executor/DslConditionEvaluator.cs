namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>ارزیابی شرط‌های Run و RefParameter — مشترک بین DslExecutor و AST interpreter.</summary>
public static class DslConditionEvaluator
{
    public static bool Evaluate(string condition, DslExecutionContext context) =>
        DslExecutor.EvaluateCondition(condition, context);
}
