using System.Text.Json;

namespace RayvarzResend.Web.Services;

public static class BankInquiryRequestBuilder
{
    /// <summary>هر دو سرویس epay — بدنه تخت در ریشه JSON (بدون request).</summary>
    public static IReadOnlyList<BankInquiryRequestFormat> FlatFormats { get; } =
    [
        BankInquiryRequestFormat.CamelFlat,
        BankInquiryRequestFormat.PascalFlat,
        BankInquiryRequestFormat.EpayIdFlat
    ];

    /// <summary>استعلام قبوض — wrapped به HTTP 400 می‌خورد؛ فقط تخت.</summary>
    public static IReadOnlyList<BankInquiryRequestFormat> FicheLookupFormats => FlatFormats;

    public static IReadOnlyList<BankInquiryRequestFormat> AllFormats { get; } =
    [
        BankInquiryRequestFormat.CamelFlat,
        BankInquiryRequestFormat.PascalFlat,
        BankInquiryRequestFormat.EpayIdFlat,
        BankInquiryRequestFormat.CamelWrapped,
        BankInquiryRequestFormat.PascalWrapped
    ];

    public static string FormatLabel(BankInquiryRequestFormat format) => format switch
    {
        BankInquiryRequestFormat.CamelFlat => "camelCase تخت (billId/payId)",
        BankInquiryRequestFormat.PascalFlat => "PascalCase تخت (BillId/PayId)",
        BankInquiryRequestFormat.EpayIdFlat => "epay تخت (billID/payID)",
        BankInquiryRequestFormat.CamelWrapped => "camelCase داخل request",
        BankInquiryRequestFormat.PascalWrapped => "PascalCase داخل Request",
        _ => format.ToString()
    };

    /// <summary>epay_FindFichesByBillIDPayID — userName/password/billId/payId.</summary>
    public static object BuildBillPayEnvelope(
        string userName,
        string password,
        string billId,
        string payId,
        BankInquiryRequestFormat format = BankInquiryRequestFormat.CamelFlat) =>
        BuildEnvelopeCore(userName, password, billId, payId, bankCode: null, format);

    /// <summary>epay_EstelamOnLineBank — بدنه + bankCode.</summary>
    public static object BuildEnvelope(
        string userName,
        string password,
        string billId,
        string payId,
        int bankCode,
        BankInquiryRequestFormat format = BankInquiryRequestFormat.CamelFlat) =>
        BuildEnvelopeCore(userName, password, billId, payId, bankCode, format);

    public static string SerializeEnvelope(
        string userName,
        string password,
        string billId,
        string payId,
        int bankCode,
        JsonSerializerOptions? options = null)
    {
        options ??= new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(
            BuildEnvelope(userName, password, billId, payId, bankCode),
            options);
    }

    private static object BuildEnvelopeCore(
        string userName,
        string password,
        string billId,
        string payId,
        int? bankCode,
        BankInquiryRequestFormat format)
    {
        var fields = BuildFieldDictionary(userName, password, billId, payId, bankCode, format);
        return format is BankInquiryRequestFormat.CamelWrapped or BankInquiryRequestFormat.PascalWrapped
            ? new Dictionary<string, object>
            {
                [format == BankInquiryRequestFormat.CamelWrapped ? "request" : "Request"] = fields
            }
            : fields;
    }

    private static Dictionary<string, object> BuildFieldDictionary(
        string userName,
        string password,
        string billId,
        string payId,
        int? bankCode,
        BankInquiryRequestFormat format)
    {
        var (userKey, passKey, billKey, payKey, bankKey) = format switch
        {
            BankInquiryRequestFormat.PascalFlat or BankInquiryRequestFormat.PascalWrapped
                => ("UserName", "Password", "BillId", "PayId", "BankCode"),
            BankInquiryRequestFormat.EpayIdFlat
                => ("userName", "password", "billID", "payID", "bankCode"),
            _ => ("userName", "password", "billId", "payId", "bankCode")
        };

        var fields = new Dictionary<string, object>
        {
            [userKey] = userName,
            [passKey] = password,
            [billKey] = billId,
            [payKey] = payId
        };

        if (bankCode.HasValue)
            fields[bankKey] = bankCode.Value;

        return fields;
    }
}
