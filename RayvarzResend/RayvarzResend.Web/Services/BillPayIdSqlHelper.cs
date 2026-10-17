namespace RayvarzResend.Web.Services;

/// <summary>مقایسه شناسه قبض/پرداخت در SQL با نرمال epay (۱۳ رقم آخر + تطابق رشته).</summary>
public static class BillPayIdSqlHelper
{
    public const int EpayLength = BankInquiryConfirmHelper.EpayBillPayIdLength;

    public static string Norm13(string sqlExpr) =>
        $"RIGHT(REPLICATE('0', {EpayLength}) + LTRIM(RTRIM(CAST({sqlExpr} AS nvarchar(40)))), {EpayLength})";

    public static string PairMatchOnTable(string billColumn, string paymentColumn, string pairAlias = "p") =>
        $"""
        (
          {PairMatchDirect(billColumn, paymentColumn, pairAlias)}
          OR {PairMatchSwapped(billColumn, paymentColumn, pairAlias)}
        )
        """;

    /// <summary>فقط تطابق مستقیم قبض/پرداخت — برای افزودن به گرید (بدون جابه‌جایی ستون اکسل).</summary>
    public static string PairMatchStrictOnTable(string billColumn, string paymentColumn, string pairAlias = "p") =>
        $"""
        ({Norm13(billColumn)} = {Norm13($"{pairAlias}.BillId")} AND {Norm13(paymentColumn)} = {Norm13($"{pairAlias}.PaymentId")})
        """;

    /// <summary>
    /// تطابق اکسل: مستقیم + تهاتر (شناسه قبض=پرداخت=شماره فیش) + ستون اختیاری شماره فیش.
    /// </summary>
    public static string PairMatchStrictForExcelLookup(
        string billColumn,
        string paymentColumn,
        string ficheNoColumn,
        string pairAlias = "p") =>
        $"""
        (
          {PairMatchStrictOnTable(billColumn, paymentColumn, pairAlias)}
          OR {TahatorSameBillPayFicheMatch(billColumn, paymentColumn, ficheNoColumn, pairAlias)}
          OR {ExcelFicheNoColumnMatch(ficheNoColumn, billColumn, paymentColumn, pairAlias)}
        )
        """;

    /// <summary>ستون Swapped فقط وقتی واقعاً جابه‌جا شده — نه وقتی قبض و پرداخت یکسان‌اند (تهاتر).</summary>
    public static string SwappedColumnsCaseSql(
        string billColumn,
        string paymentColumn,
        string pairBillExpr,
        string pairPayExpr) =>
        $"""
        CASE WHEN (
              {Norm13(billColumn)} = {Norm13(pairPayExpr)}
              AND {Norm13(paymentColumn)} = {Norm13(pairBillExpr)}
              AND NOT (
                {Norm13(billColumn)} = {Norm13(pairBillExpr)}
                AND {Norm13(paymentColumn)} = {Norm13(pairPayExpr)}
              )
             ) THEN 1 ELSE 0 END
        """;

    private static string TahatorSameBillPayFicheMatch(
        string billColumn,
        string paymentColumn,
        string ficheNoColumn,
        string pairAlias) =>
        $"""
        (
          {Norm13($"{pairAlias}.BillId")} = {Norm13($"{pairAlias}.PaymentId")}
          AND {Norm13(billColumn)} = {Norm13(paymentColumn)}
          AND {Norm13(ficheNoColumn)} = {Norm13($"{pairAlias}.BillId")}
        )
        """;

    private static string ExcelFicheNoColumnMatch(
        string ficheNoColumn,
        string billColumn,
        string paymentColumn,
        string pairAlias) =>
        $"""
        (
          NULLIF(LTRIM(RTRIM(CAST({pairAlias}.FicheNo AS nvarchar(40)))), '') IS NOT NULL
          AND LTRIM(RTRIM(CAST({ficheNoColumn} AS nvarchar(40)))) = LTRIM(RTRIM(CAST({pairAlias}.FicheNo AS nvarchar(40))))
          AND {PairMatchStrictOnTable(billColumn, paymentColumn, pairAlias)}
        )
        """;

    private static string PairMatchDirect(string billColumn, string paymentColumn, string pairAlias) =>
        $"""
        (
          ({Norm13(billColumn)} = {Norm13($"{pairAlias}.BillId")} AND {Norm13(paymentColumn)} = {Norm13($"{pairAlias}.PaymentId")})
          OR (
            LTRIM(RTRIM(CAST({billColumn} AS nvarchar(40)))) IN ({pairAlias}.BillId, {pairAlias}.BillTrim)
            AND LTRIM(RTRIM(CAST({paymentColumn} AS nvarchar(40)))) IN ({pairAlias}.PaymentId, {pairAlias}.PayTrim)
          )
        )
        """;

    private static string PairMatchSwapped(string billColumn, string paymentColumn, string pairAlias) =>
        $"""
        (
          {Norm13(billColumn)} = {Norm13($"{pairAlias}.PaymentId")}
          AND {Norm13(paymentColumn)} = {Norm13($"{pairAlias}.BillId")}
        )
        """;
}
