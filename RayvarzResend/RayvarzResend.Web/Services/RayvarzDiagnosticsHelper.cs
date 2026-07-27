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
        var isPingOrTls = stage.Equals("GetWsdl", StringComparison.OrdinalIgnoreCase)
            || text.Contains("ssl connection could not be established");

        if (isPingOrTls && (text.Contains("ssl") || text.Contains("tls") || text.Contains("certificate")))
        {
            baseInfo.Category = "MsbTlsOrNetwork";
            baseInfo.LikelyCause =
                "اتصال TLS به msb.mashhad.ir برقرار نشد — این مرحله فقط WSDL است (بدون SOAP). مشکل XML/هدر/PhasTyp نیست.";
            baseInfo.Hint =
                "برنامه را روی همان سرور/شبکه‌ای اجرا کنید که سامانه شهرسازی از آن به MSB می‌زند؛ VPN سازمان؛ ProxyUrl یا UseSystemProxy؛ با IT خروجی به https://msb.mashhad.ir را باز کنید. AllowInvalidSsl معمولاً این خطا را حل نمی‌کند.";
        }
        else         if (text.Contains("forcibly closed") || text.Contains("copying content to a stream"))
        {
            baseInfo.Category = "ConnectionReset";
            var largePayload = baseInfo.RequestBodyBytes > 1500;
            if (baseInfo.HttpStatusCode == 502)
            {
                baseInfo.LikelyCause =
                    "HTTP 502 از سرور — درخواست رسید ولی پاسخ ناقص/قطع شد. روی ITC (mdc-rayvarzsvc) WinTestService با SOAP 1.1/basicHttp کار می‌کند؛ RayvarzResend با soap12+addressing اغلب 502 می‌گیرد.";
                baseInfo.Hint =
                    "appsettings: SoapVersion=soap11 و SoapEnvelopeStyle=empty-header (پیش‌فرض جدید برای ITC). برنامه را restart کنید؛ در پیش‌نمایش Header خالی باشد و Content-Type=text/xml.";
            }
            else
            {
                baseInfo.LikelyCause = stage.Equals("GetWsdl", StringComparison.OrdinalIgnoreCase)
                    ? "MSB اتصال را در handshake یا WSDL قطع کرد — فایروال، IP مجاز، یا مسیر شبکه."
                    : largePayload
                        ? "POST با بدنه بزرگ قطع شد — اگر POST Test کوچک OK بود: ساختار/فیلد SOAP، اندازه بدنه، یا رد محتوا توسط MSB/WAF (مبالغ منفی بستانکاری و کد بانک دیتابیس معتبرند)."
                        : baseInfo.HasWsAddressingHeader
                            ? "اتصال در حین ارسال/دریافت قطع شد — شبکه، فایروال، یا رد درخواست توسط MSB."
                            : "اتصال قطع شد — ابتدا Ping را از همان ماشین درست کنید.";
                baseInfo.Hint = stage.Equals("PostSoap", StringComparison.OrdinalIgnoreCase) && largePayload
                    ? "ترتیب تست: POST خالی → «تست SaveDocument حداقلی» → ارسال فیش. appsettings: SoapVersion=soap11؛ SoapEnvelopeStyle=empty-header؛ RefRowDocNoInDetail=headerDocRow (مثل شهرسازی)."
                    : stage.Equals("GetWsdl", StringComparison.OrdinalIgnoreCase)
                        ? "curl یا مرورگر از همان PC به ServiceUrl?wsdl؛ مقایسه با سرور اپلیکیشن شهرسازی."
                        : "پس از POST Test موفق: WsAddressingTo و SoapEnvelopeStyle را بررسی کنید.";
            }
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
