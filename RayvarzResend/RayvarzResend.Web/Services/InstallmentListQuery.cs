namespace RayvarzResend.Web.Services;

/// <summary>کوئری جستجوی ردیف تقسیط برای اعتبارسنجی اکسل.</summary>
public static class InstallmentListQuery
{
    public const string NosaziCodeSql = """
        CAST(b.District AS varchar(5)) + '-' + CAST(b.Region AS varchar(5)) + '-' +
        CAST(b.Block AS varchar(5)) + '-' + CAST(b.House AS varchar(5)) + '-' +
        '0' + '-' + '-' + CAST(b.Apartment AS varchar(5)) + '-' + CAST(b.Shop AS varchar(5))
        """;

    public static string BuildExcelLookupSql(string installmentListColumn) => $"""
        SELECT il.NoDocument,
               il.TrackingNo AS trackingno,
               il.PaymentCost,
               CAST(il.PaymentDate AS varchar(20)) AS PaymentDate,
               CAST(r.NidWorkItem AS nvarchar(50)) AS nidworkitem,
               {NosaziCodeSql} AS NosaziCode,
               CAST(il.CI_InstallmentStatus AS varchar(20)) AS CI_InstallmentStatus,
               il.EndStateDesc,
               il.EndStateCode,
               il.Comments
        FROM dbo.Income i
        INNER JOIN dbo.Income_Fiche f ON i.NidIncome = f.NidIncome
        INNER JOIN dbo.Installment ins ON f.NidFiche = ins.NidFiche
        INNER JOIN dbo.Installment_List il ON ins.NidInstallment = il.NidInstallment
        INNER JOIN dbo.Sh_RequestInfo r ON i.NidProc = r.NidProc
        INNER JOIN dbo.Base_NosaziCode b ON b.NidNosaziCode = r.NidNosaziCode
        WHERE il.{installmentListColumn} = @v
        """;
}
