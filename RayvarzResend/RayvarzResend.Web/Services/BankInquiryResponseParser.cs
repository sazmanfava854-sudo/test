using System.Text.Json;

namespace RayvarzResend.Web.Services;

/// <summary>تفسیر پاسخ epay_EstelamOnLineBank.</summary>
public static class BankInquiryResponseParser
{
    private static readonly string[] PaidMessageHints =
    [
        "پرداخت شده",
        "پرداخت موفق",
        "تایید شد",
        "موفق"
    ];

    private static readonly string[] UnpaidMessageHints =
    [
        "پرداخت نشده",
        "پرداخت نگردیده",
        "پرداخت نکرده",
        "یافت نشد",
        "وجود ندارد",
        "نامعتبر",
        "not found",
        "not paid",
        "unpaid"
    ];

    public static BankInquiryApiResult Parse(string? rawJson, int httpStatusCode)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
            return BankInquiryApiResult.Failed("پاسخ خالی از سرویس استعلام بانک", rawJson);

        if (httpStatusCode < 200 || httpStatusCode >= 300)
            return BankInquiryApiResult.Failed($"خطای HTTP {httpStatusCode} از سرویس استعلام بانک", rawJson);

        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            return ParseElement(doc.RootElement, rawJson);
        }
        catch (JsonException)
        {
            return BankInquiryApiResult.Failed("پاسخ سرویس استعلام بانک JSON معتبر نیست", rawJson);
        }
    }

    internal static BankInquiryApiResult ParseElement(JsonElement root, string? rawJson = null)
    {
        var message = ReadMessage(root);
        var paymentDate = ReadPaymentDate(root);

        if (TryReadExplicitPaid(root, out var explicitPaid))
        {
            return explicitPaid
                ? BankInquiryApiResult.Paid(paymentDate, message)
                : BankInquiryApiResult.NotPaid(string.IsNullOrWhiteSpace(message) ? null : message);
        }

        if (ContainsAny(message, UnpaidMessageHints))
            return BankInquiryApiResult.NotPaid(message);

        if (ContainsAny(message, PaidMessageHints) || !string.IsNullOrWhiteSpace(paymentDate))
            return BankInquiryApiResult.Paid(paymentDate, message);

        if (TryReadNumericSuccess(root, out var numericPaid))
        {
            return numericPaid
                ? BankInquiryApiResult.Paid(paymentDate, message)
                : BankInquiryApiResult.NotPaid(string.IsNullOrWhiteSpace(message) ? null : message);
        }

        if (HasPaidDataObject(root))
            return BankInquiryApiResult.Paid(paymentDate, message);

        return BankInquiryApiResult.NotPaid(
            string.IsNullOrWhiteSpace(message) ? null : message);
    }

    private static bool TryReadExplicitPaid(JsonElement root, out bool isPaid)
    {
        isPaid = false;
        foreach (var name in new[] { "success", "isSuccess", "IsSuccess", "paid", "isPaid", "IsPaid" })
        {
            if (!TryGetPropertyIgnoreCase(root, name, out var value))
                continue;

            if (value.ValueKind == JsonValueKind.True)
            {
                isPaid = true;
                return true;
            }

            if (value.ValueKind == JsonValueKind.False)
            {
                isPaid = false;
                return true;
            }
        }

        return false;
    }

    private static bool TryReadNumericSuccess(JsonElement root, out bool isPaid)
    {
        isPaid = false;
        foreach (var name in new[] { "result", "resultCode", "code", "status", "Status", "errorCode" })
        {
            if (!TryGetPropertyIgnoreCase(root, name, out var value))
                continue;

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var code))
            {
                isPaid = code == 0 || code == 1;
                return true;
            }

            if (value.ValueKind == JsonValueKind.String)
            {
                var text = value.GetString() ?? "";
                if (text.Equals("OK", StringComparison.OrdinalIgnoreCase)
                    || text.Equals("Success", StringComparison.OrdinalIgnoreCase)
                    || text == "0"
                    || text == "1")
                {
                    isPaid = true;
                    return true;
                }

                if (text.Equals("Fail", StringComparison.OrdinalIgnoreCase)
                    || text.Equals("Error", StringComparison.OrdinalIgnoreCase))
                {
                    isPaid = false;
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasPaidDataObject(JsonElement root)
    {
        foreach (var name in new[] { "data", "result", "Data", "Result" })
        {
            if (!TryGetPropertyIgnoreCase(root, name, out var value))
                continue;

            if (value.ValueKind != JsonValueKind.Object)
                continue;

            if (!string.IsNullOrWhiteSpace(ReadPaymentDate(value)))
                return true;

            foreach (var child in value.EnumerateObject())
            {
                if (child.Name.Contains("pay", StringComparison.OrdinalIgnoreCase)
                    && child.Value.ValueKind == JsonValueKind.True)
                    return true;
            }
        }

        return false;
    }

    private static string ReadMessage(JsonElement root)
    {
        foreach (var name in new[]
                 {
                     "message", "Message", "errorMessage", "ErrorMessage",
                     "description", "Description", "msg", "Msg"
                 })
        {
            if (TryGetPropertyIgnoreCase(root, name, out var value)
                && value.ValueKind == JsonValueKind.String)
            {
                var text = value.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                    return text.Trim();
            }
        }

        return "";
    }

    private static string? ReadPaymentDate(JsonElement root)
    {
        foreach (var name in new[]
                 {
                     "paymentDate", "PaymentDate", "payDate", "PayDate",
                     "bankPaymentDate", "BankPaymentDate", "transactionDate", "TransactionDate"
                 })
        {
            if (TryGetPropertyIgnoreCase(root, name, out var value)
                && value.ValueKind == JsonValueKind.String)
            {
                var text = value.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                    return text.Trim();
            }
        }

        foreach (var containerName in new[] { "data", "result", "Data", "Result" })
        {
            if (!TryGetPropertyIgnoreCase(root, containerName, out var container)
                || container.ValueKind != JsonValueKind.Object)
                continue;

            var nested = ReadPaymentDate(container);
            if (!string.IsNullOrWhiteSpace(nested))
                return nested;
        }

        return null;
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string name, out JsonElement value)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            value = default;
            return false;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static bool ContainsAny(string text, IEnumerable<string> hints)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        foreach (var hint in hints)
        {
            if (text.Contains(hint, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
