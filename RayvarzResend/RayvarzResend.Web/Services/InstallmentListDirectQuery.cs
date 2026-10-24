namespace RayvarzResend.Web.Services;

/// <summary>جستجوی مستقیم dbo.Installment_List (بدون JOIN) — تب تغییر تاریخ چک.</summary>
public static class InstallmentListDirectQuery
{
    public const string ListSelectColumns = """
        il.NidInstallment,
        il.NoDocument,
        il.TrackingNo,
        il.PaymentCost,
        CAST(il.PaymentDate AS varchar(20)) AS PaymentDate,
        CAST(il.CI_InstallmentStatus AS varchar(20)) AS CI_InstallmentStatus,
        il.EndStateDesc,
        il.EndStateCode,
        il.Comments
        """;

    public static string BuildSearchByTrackingNoSql() => $"""
        SELECT {ListSelectColumns}
        FROM dbo.Installment_List il
        WHERE {InstallmentIdentifierDetector.BuildTrackingNoWhereClause("il.TrackingNo")}
        ORDER BY il.NidInstallment, il.NoDocument
        """;

    /// <summary>یک ردیف — NidInstallment + NoDocument + TrackingNo (ستون PK جداگانه در Sara نیست).</summary>
    public static string BuildSelectOneRowSql() => $"""
        SELECT {ListSelectColumns}
        FROM dbo.Installment_List il
        WHERE il.NidInstallment = @nid
          AND LTRIM(RTRIM(CAST(il.NoDocument AS nvarchar(50)))) = @noDoc
          AND {InstallmentIdentifierDetector.BuildTrackingNoWhereClause("il.TrackingNo")}
        """;

    public static string BuildUpdatePaymentDateSql() => $"""
        UPDATE dbo.Installment_List
        SET PaymentDate = @paymentDate,
            Comments = @prefix + ISNULL(Comments, '')
        WHERE NidInstallment = @nid
          AND LTRIM(RTRIM(CAST(NoDocument AS nvarchar(50)))) = @noDoc
          AND {InstallmentIdentifierDetector.BuildTrackingNoWhereClause("TrackingNo")}
        """;
}
