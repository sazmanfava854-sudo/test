namespace RayvarzResend.Web.Services;

public sealed class ShahkarOptions
{
    public const string SectionName = "Shahkar";

    public bool DryRun { get; set; }
    public string ConnectionStringName { get; set; } = "Security";
    public string Schema { get; set; } = "dbo";
    public string TableName { get; set; } = "Users";
    public int MaxSearchRows { get; set; } = 200;
    public string ShahkarOkValue { get; set; } = "1";
}
