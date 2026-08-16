namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>گزینه‌های ساخت ردیف درآمد Member 1388 — الگوی iNcOMEOragh.</summary>
public sealed class Member1388IncomeRowOptions
{
    public bool ApplyOddments { get; init; } = true;
    public bool ApplyBedeHi { get; init; }
    public int? BedeHiWhenAccountGroup { get; init; }
    public IReadOnlyList<int> ExcludeBedeHiWhenAccountGroups { get; init; } = Array.Empty<int>();
    public string? RowNum { get; init; }
    public int? RowNumWhenAccountGroup { get; init; }
    public bool RowNumFromDepositId { get; init; }
}
