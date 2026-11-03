namespace RayvarzResend.Web.Services;

/// <summary>کد پیگیری مؤثر از dbo.Installment_List — فقط ستون‌های DB.</summary>
public static class InstallmentListTrackingSql
{
    /// <summary>عبارت SQL برای alias <c>il</c> — خروجی AS trackingno.</summary>
    public const string EffectiveTrackingSelect = """
        NULLIF(LTRIM(RTRIM(CAST(
          COALESCE(
            NULLIF(LTRIM(RTRIM(CAST(il.TrackingNo AS varchar(32)))), ''),
            NULLIF(LTRIM(RTRIM(CAST(il.CheckRahgiriCode AS varchar(32)))), '')
          ) AS varchar(32)))), '') AS trackingno
        """;
}
