namespace RayvarzResend.Web.Services;

/// <summary>جستجوی مستقیم dbo.Installment_List (بدون JOIN) — تب تغییر تاریخ چک.</summary>
public static class InstallmentListDirectQuery
{
    public static string BuildSearchByTrackingNoSql() => $"""
        SELECT il.NidInstallmentList,
               il.NoDocument,
               il.TrackingNo,
               il.PaymentCost,
               CAST(il.PaymentDate AS varchar(20)) AS PaymentDate,
               CAST(il.CI_InstallmentStatus AS varchar(20)) AS CI_InstallmentStatus,
               il.EndStateDesc,
               il.EndStateCode,
               il.Comments
        FROM dbo.Installment_List il
        WHERE {InstallmentIdentifierDetector.BuildTrackingNoWhereClause("il.TrackingNo")}
        ORDER BY il.NidInstallmentList
        """;
}
