using System.Text.RegularExpressions;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

internal static class RayvarzDiagnosticsHelper
{
    private static readonly Regex WsToRegex = new(
        @"<a:To[^>]*>([^<]*)</a:To>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static void ApplySoapRequestMeta(RayvarzTransportDiagnostics d, string soapXml, string envelopeStyle)
    {
        d.EnvelopeStyle = envelopeStyle;
        d.RequestBodyBytes = System.Text.Encoding.UTF8.GetByteCount(soapXml);
        d.HasWsAddressingHeader = soapXml.Contains("http://www.w3.org/2005/08/addressing", StringComparison.Ordinal)
            && soapXml.Contains("<a:Action", StringComparison.OrdinalIgnoreCase);
        var toMatch = WsToRegex.Match(soapXml);
        if (toMatch.Success)
            d.WsAddressingTo = toMatch.Groups[1].Value.Trim();
    }

    public static RayvarzTransportDiagnostics ClassifyFailure(
        Exception ex,
        string stage,
        long elapsedMs,
        RayvarzTransportDiagnostics baseInfo)
    {
        baseInfo.Stage = stage;
        baseInfo.ElapsedMs = elapsedMs;
        baseInfo.ExceptionChain = ExceptionChain(ex);

        var text = string.Join(" ", baseInfo.ExceptionChain).ToLowerInvariant();
        if (text.Contains("forcibly closed") || text.Contains("copying content to a stream"))
        {
            baseInfo.Category = "ConnectionReset";
            baseInfo.LikelyCause = baseInfo.HasWsAddressingHeader
                ? "اتصال در حین ارسال/دریافت قطع شد — شبکه، فایروال، یا رد شدن درخواست توسط MSB (گاهی هدر SOAP/آدرس To)."
                : "اتصال قطع شد — احتمال شبکه بالاست؛ اگر ping موفق است SoapEnvelopeStyle=addressing را امتحان کنید.";
            baseInfo.Hint = "GET /api/rayvarz-ping؛ مقایسه ping با send؛ WsAddressingTo و SoapEnvelopeStyle (addressing | empty-header).";
        }
        else if (text.Contains("ssl") || text.Contains("certificate") || text.Contains("tls") || text.Contains("authentication"))
        {
            baseInfo.Category = "TlsOrCertificate";
            baseInfo.LikelyCause = "خطای TLS یا گواهی SSL هنگام اتصال به MSB.";
            baseInfo.Hint = "AllowInvalidSsl=true فقط برای تست؛ VPN؛ اجرا از همان سرور شهرسازی.";
        }
        else if (text.Contains("timeout") || text.Contains("timed out"))
        {
            baseInfo.Category = "Timeout";
            baseInfo.LikelyCause = "مهلت اتصال تمام شد — مسیر شبکه کند یا سرویس پاسخ نمی‌دهد.";
            baseInfo.Hint = "فایروال/پروکسی؛ افزایش Timeout؛ rayvarz-ping.";
        }
        else if (ex is HttpRequestException)
        {
            baseInfo.Category = "HttpRequest";
            baseInfo.LikelyCause = "خطای HTTP در لایه انتقال.";
            baseInfo.Hint = "لاگ کنسول و Diagnostics.ExceptionChain را ببینید.";
        }
        else
        {
            baseInfo.Category = "Unknown";
            baseInfo.LikelyCause = "خطای نامشخص در ارسال SOAP.";
            baseInfo.Hint = "لاگ سطح Debug برای RayvarzClient را فعال کنید.";
        }

        return baseInfo;
    }

    public static RayvarzTransportDiagnostics ForSoapFault(long elapsedMs, RayvarzTransportDiagnostics baseInfo, int? httpStatus)
    {
        baseInfo.Category = "SoapFault";
        baseInfo.Stage = "ParseResponse";
        baseInfo.ElapsedMs = elapsedMs;
        baseInfo.HttpStatusCode = httpStatus;
        baseInfo.LikelyCause = "پاسخ SOAP Fault از MSB/رایورز — معمولاً محتوای Body یا فیلدهای سند.";
        baseInfo.Hint = "SoapResponse را بخوانید؛ XML Body را با مستندات مقایسه کنید (نه لزوماً هدر).";
        return baseInfo;
    }

    public static RayvarzTransportDiagnostics ForSuccess(long elapsedMs, RayvarzTransportDiagnostics baseInfo, int httpStatus, int responseBytes)
    {
        baseInfo.Category = "Success";
        baseInfo.Stage = "Complete";
        baseInfo.ElapsedMs = elapsedMs;
        baseInfo.HttpStatusCode = httpStatus;
        baseInfo.ResponseBodyBytes = responseBytes;
        return baseInfo;
    }

    private static List<string> ExceptionChain(Exception ex)
    {
        var list = new List<string>();
        for (var e = ex; e != null; e = e.InnerException)
            list.Add($"{e.GetType().Name}: {e.Message}");
        return list;
    }
}
