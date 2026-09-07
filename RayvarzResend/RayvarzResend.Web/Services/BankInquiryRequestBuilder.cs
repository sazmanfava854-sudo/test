using System.Text.Json;

namespace RayvarzResend.Web.Services;

public static class BankInquiryRequestBuilder
{
    /// <summary>
    /// epay_FindEpayFichesByBillIdPayId — بدنه تخت (بدون request)، فقط userName/password/billId/payId.
    /// </summary>
    public static object BuildBillPayEnvelope(
        string userName,
        string password,
        string billId,
        string payId) =>
        new Dictionary<string, object>
        {
            ["userName"] = userName,
            ["password"] = password,
            ["billId"] = billId,
            ["payId"] = payId
        };

    /// <summary>
    /// epay_EstelamOnLineBank — بدنه تخت (بدون request) + bankCode.
    /// </summary>
    public static object BuildEnvelope(
        string userName,
        string password,
        string billId,
        string payId,
        int bankCode) =>
        new Dictionary<string, object>
        {
            ["userName"] = userName,
            ["password"] = password,
            ["billId"] = billId,
            ["payId"] = payId,
            ["bankCode"] = bankCode
        };

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
}
