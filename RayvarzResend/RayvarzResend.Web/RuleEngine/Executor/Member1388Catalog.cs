namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>فهرست توابع Member 1388 — همان ترتیب Run در فایل paste/VB.</summary>
public static class Member1388Catalog
{
    public const int NidMember = 1388;
    public const int NidFunction = 1388;

    public static readonly IReadOnlyList<string> RunIncomeCallOrder =
    [
        "iNcOME",
        "IncomeHoushmand",
        "IncomeSrvElectronic",
        "iNcOMESeprdeh",
        "iNcOMEEshghal",
        "iNcOMEGhatar_Shahri",
        "iNcOMEBackSeprdeh",
        "iNcOMEOragh",
        "iNcOMEHavaleT",
        "BazAfarine",
        "Tahator1",
        "Tahator"
    ];

    public static readonly IReadOnlyList<Member1388FunctionDef> AllFunctions =
    [
        new("Run", "اجراي فرمول", Member1388FunctionKind.Entry),
        new("Nosazi", "نوسازی", Member1388FunctionKind.Duty),
        new("iNcOME", "درآمد", Member1388FunctionKind.Income),
        new("iNcOMEOragh", "درآمد اوراق بهادار خزانه", Member1388FunctionKind.Income),
        new("iNcOMESeprdeh", "سپرده درآمد", Member1388FunctionKind.Income),
        new("iNcOMEEshghal", "اشغال معابر درآمد", Member1388FunctionKind.Income),
        new("iNcOMEGhatar_Shahri", "قطار شهری درآمد", Member1388FunctionKind.Income),
        new("iNcOMEBackSeprdeh", "برگشت از سپرده درآمد", Member1388FunctionKind.Income),
        new("iNcOMEHavaleT", "درآمد حواله تقسیط", Member1388FunctionKind.Income),
        new("BedeHi", "بدهی قبلی", Member1388FunctionKind.IncomeHelper),
        new("Logfile", null, Member1388FunctionKind.Helper),
        new("iNcOMECheck", "چک درآمد", Member1388FunctionKind.IncomeCheck),
        new("ChangeDate", null, Member1388FunctionKind.Helper),
        new("GetSara8Workflow", null, Member1388FunctionKind.Helper),
        new("GetDiffDate", null, Member1388FunctionKind.Helper),
        new("Tahator", "درآمدی تهاتر", Member1388FunctionKind.Tahator),
        new("BazAfarine", "باز افرینی", Member1388FunctionKind.Income),
        new("BazAfarineOld", "قدیم باز افرینی", Member1388FunctionKind.Income),
        new("Tahator1", "تهاتر تک مبلغی", Member1388FunctionKind.Tahator),
        new("FnSMS", null, Member1388FunctionKind.Helper),
        new("AddDateForHolidays", null, Member1388FunctionKind.Helper),
        new("IncomeHoushmand", "بهای هوشمندسازی خدمات شهری", Member1388FunctionKind.Income),
        new("IncomeSrvElectronic", "خدمات سرويس هاي الكترونيك شهرداري", Member1388FunctionKind.Income)
    ];

    public static bool IsCatalogFunction(string name) =>
        AllFunctions.Any(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}

public enum Member1388FunctionKind
{
    Entry,
    Duty,
    Income,
    IncomeHelper,
    IncomeCheck,
    Tahator,
    Helper
}

public sealed record Member1388FunctionDef(string Name, string? DisplayName, Member1388FunctionKind Kind);
