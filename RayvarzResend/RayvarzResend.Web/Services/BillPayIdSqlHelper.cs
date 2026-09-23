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
        PairMatchDirect(billColumn, paymentColumn, pairAlias);

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
