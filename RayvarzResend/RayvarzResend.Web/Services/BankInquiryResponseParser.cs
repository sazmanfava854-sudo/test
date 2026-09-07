using System.Text.Json;

namespace RayvarzResend.Web.Services;

/// <summary>تفسیر پاسخ سرویس‌های استعلام epay.</summary>
public static class BankInquiryResponseParser
{
    public const string FicheLookupSourceLabel = "استعلام قبوض";
    public const string OnlineBankSourceLabel = "استعلام آنی بانک";

    private static readonly string[] RecordNotFoundHints =
    [
        "یافت نشد",
        "وجود ندارد",
        "عدم وجود",
        "not found",
        "no record",
        "record not found"
    ];

    private static readonly string[] ServiceFailureHints =
    [
        "meaningful reply",
        "contract mismatch",
        "premature session shutdown",
        "internal server error",
        "خطا در ارتباط",
        "سرویس بانک",
        "در دسترس نیست"
    ];

    public static bool LooksLikeServiceFailure(string? message) => ContainsAny(message, ServiceFailureHints);
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

    /// <summary>epay_FindEpayFichesByBillIdPayId — ثبت فیش با تأخیر ~۱ روز.</summary>
    public static BankInquiryParsedStep ParseFicheLookupStep(string? rawJson, int httpStatusCode)
    {
        if (httpStatusCode < 200 || httpStatusCode >= 300)
            return ToServiceErrorStep(ParseHttpError(httpStatusCode, rawJson, FicheLookupSourceLabel));

        if (string.IsNullOrWhiteSpace(rawJson))
            return ServiceErrorStep("پاسخ خالی از سرویس استعلام قبوض", rawJson);

        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            var root = UnwrapResponseRoot(doc.RootElement);
            var message = ReadMessage(root, "strResult", "StrResult");
            var intResult = ReadInt(root, "intResult", "IntResult");
            var isPay = ReadInt(root, "isPay", "IsPay", "pay", "Pay");
            var fichesId = ReadInt(root, "fichesId", "FichesId");
            var paymentDate = ReadPaymentDate(root, "registerDate", "RegisterDate", "forgeDate", "ForgeDate");

            if (isPay == 1)
                return PaidStep(paymentDate, message);

            if (HasFicheLookupRecord(root, intResult, fichesId, isPay, paymentDate))
            {
                if (isPay == 0)
                    return NotPaidStep(message);

                if (!string.IsNullOrWhiteSpace(paymentDate))
                    return PaidStep(paymentDate, message);

                return NotPaidStep(message);
            }

            if (IsRecordNotFound(message, intResult, fichesId, isPay))
                return RecordNotFoundStep(message);

            return RecordNotFoundStep(
                string.IsNullOrWhiteSpace(message) ? "فیش در استعلام قبوض یافت نشد" : message);
        }
        catch (JsonException)
        {
            return ServiceErrorStep("پاسخ سرویس استعلام قبوض JSON معتبر نیست", rawJson);
        }
    }

    /// <summary>epay_EstelamOnLineBank — استعلام آنی پرداخت آنلاین.</summary>
    public static BankInquiryParsedStep ParseOnlineBankStep(string? rawJson, int httpStatusCode)
    {
        if (httpStatusCode < 200 || httpStatusCode >= 300)
            return ToServiceErrorStep(ParseHttpError(httpStatusCode, rawJson, OnlineBankSourceLabel));

        if (string.IsNullOrWhiteSpace(rawJson))
            return ServiceErrorStep("پاسخ خالی از سرویس استعلام آنی بانک", rawJson);

        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            var root = UnwrapResponseRoot(doc.RootElement);
            var message = ReadMessage(root, "strResualt", "StrResualt", "strResult", "StrResult");
            var intResult = ReadInt(root, "intResualt", "IntResualt", "intResult", "IntResult");
            var pay = ReadInt(root, "pay", "Pay");
            var paymentDate = ReadPaymentDate(root, "payDate", "PayDate");

            if (LooksLikeServiceFailure(message))
                return ServiceErrorStep(message, rawJson);

            if (pay == 1)
                return PaidStep(paymentDate, message);

            if (pay == 0 && (intResult == 0 || !intResult.HasValue))
                return NotPaidStep(message);

            if (IsRecordNotFound(message, intResult, null, pay))
                return NotPaidStep(message);

            if (intResult.HasValue && intResult.Value != 0)
            {
                if (ContainsAny(message, RecordNotFoundHints))
                    return NotPaidStep(message);

                return ServiceErrorStep(
                    string.IsNullOrWhiteSpace(message)
                        ? $"خطا در سرویس استعلام آنی بانک (کد {intResult.Value})"
                        : message,
                    rawJson);
            }

            return NotPaidStep(message);
        }
        catch (JsonException)
        {
            return ServiceErrorStep("پاسخ سرویس استعلام آنی بانک JSON معتبر نیست", rawJson);
        }
    }

    public static BankInquiryApiResult Parse(string? rawJson, int httpStatusCode)
    {
        if (httpStatusCode < 200 || httpStatusCode >= 300)
            return ParseHttpError(httpStatusCode, rawJson);

        if (string.IsNullOrWhiteSpace(rawJson))
            return BankInquiryApiResult.Failed("پاسخ خالی از سرویس استعلام بانک", rawJson);

        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;
            if (TryGetPropertyIgnoreCase(root, "response", out var response)
                && response.ValueKind == JsonValueKind.Object)
                return ParseElement(response, rawJson);
            if (TryGetPropertyIgnoreCase(root, "result", out var result)
                && result.ValueKind == JsonValueKind.Object)
                return ParseElement(result, rawJson);
            if (TryGetPropertyIgnoreCase(root, "data", out var data)
                && data.ValueKind == JsonValueKind.Object)
                return ParseElement(data, rawJson);

            return ParseElement(root, rawJson);
        }
        catch (JsonException)
        {
            return BankInquiryApiResult.Failed("پاسخ سرویس استعلام بانک JSON معتبر نیست", rawJson);
        }
    }

    public static BankInquiryApiResult ParseHttpError(
        int httpStatusCode,
        string? rawBody,
        string? serviceLabel = null)
    {
        var detail = ExtractErrorDetail(rawBody);
        var serviceName = string.IsNullOrWhiteSpace(serviceLabel) ? "سرویس استعلام بانک" : serviceLabel.Trim();
        var message = httpStatusCode switch
        {
            502 => $"خطای HTTP 502 از درگاه {serviceName} (Bad Gateway). "
                   + "معمولاً یعنی درگاه epay به سرویس پشتی وصل نشده یا UserName/Password/IP مجاز نیست. "
                   + "UseSystemProxy=true را امتحان کنید؛ با IT دسترسی به epayws.mashhad.ir را بررسی کنید.",
            503 => $"{serviceName} موقتاً در دسترس نیست (HTTP 503).",
            504 => $"زمان پاسخ {serviceName} تمام شد (HTTP 504).",
            400 => $"درخواست {serviceName} نامعتبر است (HTTP 400). "
                   + (serviceName == FicheLookupSourceLabel || serviceName == OnlineBankSourceLabel
                       ? "فرمت JSON یا فیلدهای userName/password/billId/payId را در بدنه اصلی (بدون request) بررسی کنید."
                       : "فرمت JSON یا فیلدهای userName/password/billId/payId/bankCode را بررسی کنید."),
            401 or 403 => $"احراز هویت {serviceName} رد شد — UserName/Password را بررسی کنید.",
            _ => $"خطای HTTP {httpStatusCode} از {serviceName}"
        };

        if (!string.IsNullOrWhiteSpace(detail))
            message += $" — {detail}";

        return BankInquiryApiResult.Failed(message, rawBody);
    }

    private static string ExtractErrorDetail(string? rawBody)
    {
        if (string.IsNullOrWhiteSpace(rawBody))
            return "";

        var trimmed = rawBody.Trim();
        if (trimmed.StartsWith('{') || trimmed.StartsWith('['))
        {
            try
            {
                using var doc = JsonDocument.Parse(trimmed);
                var message = ReadMessage(doc.RootElement);
                if (!string.IsNullOrWhiteSpace(message))
                    return message;
            }
            catch (JsonException)
            {
                // ignore HTML/non-JSON gateway pages
            }
        }

        var plain = trimmed
            .Replace("<br>", " ", StringComparison.OrdinalIgnoreCase)
            .Replace("<br/>", " ", StringComparison.OrdinalIgnoreCase)
            .Replace("<br />", " ", StringComparison.OrdinalIgnoreCase);
        if (plain.Contains('<') && plain.Contains('>'))
            plain = System.Text.RegularExpressions.Regex.Replace(plain, "<[^>]+>", " ");
        plain = System.Text.RegularExpressions.Regex.Replace(plain, "\\s+", " ").Trim();
        if (plain.Length > 180)
            plain = plain[..180] + "...";
        return plain;
    }

    private static JsonElement UnwrapResponseRoot(JsonElement root)
    {
        if (TryGetPropertyIgnoreCase(root, "response", out var response)
            && response.ValueKind == JsonValueKind.Object)
            return response;
        if (TryGetPropertyIgnoreCase(root, "result", out var result)
            && result.ValueKind == JsonValueKind.Object)
            return result;
        if (TryGetPropertyIgnoreCase(root, "data", out var data)
            && data.ValueKind == JsonValueKind.Object)
            return data;

        return root;
    }

    private static bool HasFicheLookupRecord(
        JsonElement root,
        int? intResult,
        int? fichesId,
        int? isPay,
        string? paymentDate)
    {
        if (fichesId is > 0)
            return true;

        if (!string.IsNullOrWhiteSpace(paymentDate))
            return true;

        if (ReadInt(root, "payAmount", "PayAmount") is > 0)
            return true;

        if (isPay == 0 && intResult == 0)
            return true;

        return IsRecordFoundInFicheLookup(root, intResult, fichesId, isPay);
    }

    private static bool IsRecordFoundInFicheLookup(
        JsonElement root,
        int? intResult,
        int? fichesId,
        int? isPay)
    {
        if (fichesId is > 0)
            return true;

        if (isPay == 0 && intResult == 0)
            return true;

        foreach (var name in new[] { "billId", "BillId", "payId", "PayId", "payAmount", "PayAmount" })
        {
            if (!TryGetPropertyIgnoreCase(root, name, out var value))
                continue;

            if (value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString()))
                return true;

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var number) && number != 0)
                return true;
        }

        return false;
    }

    private static bool IsRecordNotFound(string message, int? intResult, int? fichesId, int? payFlag)
    {
        if (ContainsAny(message, RecordNotFoundHints))
            return true;

        if (fichesId is 0 or null && payFlag is 0 or null && intResult is > 0)
            return true;

        return false;
    }

    private static BankInquiryParsedStep PaidStep(string? paymentDate, string message) => new()
    {
        Kind = BankInquiryStepKind.Paid,
        PaymentDate = paymentDate,
        Message = string.IsNullOrWhiteSpace(message) ? "پرداخت در بانک تایید شد" : message.Trim()
    };

    private static BankInquiryParsedStep NotPaidStep(string? message) => new()
    {
        Kind = BankInquiryStepKind.NotPaid,
        Message = string.IsNullOrWhiteSpace(message) ? BankInquiryConfirmHelper.UnpaidFicheMessage : message.Trim()
    };

    private static BankInquiryParsedStep RecordNotFoundStep(string? message) => new()
    {
        Kind = BankInquiryStepKind.RecordNotFound,
        Message = string.IsNullOrWhiteSpace(message) ? "فیش در استعلام قبوض یافت نشد" : message.Trim()
    };

    private static BankInquiryParsedStep ServiceErrorStep(string message, string? raw) => new()
    {
        Kind = BankInquiryStepKind.ServiceError,
        Message = message,
        RawResponse = raw
    };

    private static BankInquiryParsedStep ToServiceErrorStep(BankInquiryApiResult failed) => new()
    {
        Kind = BankInquiryStepKind.ServiceError,
        Message = failed.Message,
        RawResponse = failed.RawResponse
    };

    private static int? ReadInt(JsonElement root, params string[] names)
    {
        foreach (var name in names)
        {
            if (!TryGetPropertyIgnoreCase(root, name, out var value))
                continue;

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
                return number;

            if (value.ValueKind == JsonValueKind.String
                && int.TryParse(value.GetString(), out var parsed))
                return parsed;

            if (value.ValueKind == JsonValueKind.True)
                return 1;

            if (value.ValueKind == JsonValueKind.False)
                return 0;
        }

        return null;
    }

    private static string ReadMessage(JsonElement root, params string[] preferredNames)
    {
        foreach (var name in preferredNames)
        {
            if (TryGetPropertyIgnoreCase(root, name, out var value)
                && value.ValueKind == JsonValueKind.String)
            {
                var text = value.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                    return text.Trim();
            }
        }

        return ReadMessage(root);
    }

    private static string? ReadPaymentDate(JsonElement root, params string[] preferredNames)
    {
        foreach (var name in preferredNames)
        {
            if (TryGetPropertyIgnoreCase(root, name, out var value)
                && value.ValueKind == JsonValueKind.String)
            {
                var text = value.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                    return text.Trim();
            }
        }

        return ReadPaymentDate(root);
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
        foreach (var name in new[] { "success", "isSuccess", "IsSuccess", "paid", "isPaid", "IsPaid", "pay", "Pay", "isPay", "IsPay" })
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
