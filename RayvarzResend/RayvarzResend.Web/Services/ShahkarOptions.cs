namespace RayvarzResend.Web.Services;

public sealed class ShahkarOptions
{
    public const string SectionName = "Shahkar";

    public bool DryRun { get; set; }
    public string ConnectionStringName { get; set; } = "Security";
    /// <summary>اختیاری — اگر در JSON نمی‌خواهید Server=IP\\instance بنویسید.</summary>
    public string? SqlServer { get; set; }
    /// <summary>مثلاً sql2012 — با SqlServer یا برای اصلاح نام instance چسبیده در ConnectionString.</summary>
    public string? SqlInstance { get; set; }
    public string Schema { get; set; } = "dbo";
    public string TableName { get; set; } = "Users";
    public int MaxSearchRows { get; set; } = 200;
    public string ShahkarOkValue { get; set; } = "1";
}
