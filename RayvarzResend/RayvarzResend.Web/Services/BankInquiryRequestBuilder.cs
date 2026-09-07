using System.Text.Json;

namespace RayvarzResend.Web.Services;

public static class BankInquiryRequestBuilder
{
    public static object BuildEnvelope(
        string userName,
        string password,
        string billId,
        string payId,
        int bankCode) =>
        new Dictionary<string, object>
        {
            ["request"] = new Dictionary<string, object>
            {
                ["UserName"] = userName,
                ["Password"] = password,
                ["BillId"] = billId,
                ["PayId"] = payId,
                ["BankCode"] = bankCode
            }
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
