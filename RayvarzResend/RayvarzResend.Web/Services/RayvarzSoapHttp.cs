using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace RayvarzResend.Web.Services;

internal enum RayvarzSoapVersion
{
    Soap12,
    Soap11
}

/// <summary>HTTP/SOAP transport — هم‌راستا با کلاینت‌های WCF قدیمی (SOAP 1.1 + SOAPAction، بدون Expect: 100-continue).</summary>
internal static class RayvarzSoapHttp
{
  public static bool IsItcDirectWcf(IConfiguration config)
    {
        var url = config["Rayvarz:ServiceUrl"] ?? "";
        return url.Contains("mdc-rayvarzsvc", StringComparison.OrdinalIgnoreCase)
               || url.Contains("safa_shahrsazi", StringComparison.OrdinalIgnoreCase);
    }

    public static RayvarzSoapVersion ResolveSoapVersion(IConfiguration config)
    {
        var raw = config["Rayvarz:SoapVersion"];
        if (string.IsNullOrWhiteSpace(raw))
            return IsItcDirectWcf(config) ? RayvarzSoapVersion.Soap11 : RayvarzSoapVersion.Soap12;

        raw = raw.Trim().ToLowerInvariant();
        return raw is "soap11" or "1.1" or "11" or "text/xml"
            ? RayvarzSoapVersion.Soap11
            : RayvarzSoapVersion.Soap12;
    }

    public static string ResolveEnvelopeStyle(IConfiguration config)
    {
        var raw = config["Rayvarz:SoapEnvelopeStyle"];
        if (string.IsNullOrWhiteSpace(raw))
            return IsItcDirectWcf(config) ? "empty-header" : "addressing";
        return raw.Trim().ToLowerInvariant();
    }

    public static string SoapVersionLabel(RayvarzSoapVersion version) =>
        version == RayvarzSoapVersion.Soap11 ? "soap11" : "soap12";

    public static HttpContent CreateSoapContent(string soapXml, string action, RayvarzSoapVersion version)
    {
        if (version == RayvarzSoapVersion.Soap11)
            return new StringContent(soapXml, Encoding.UTF8, "text/xml");

        var content = new StringContent(soapXml, Encoding.UTF8, "application/soap+xml");
        content.Headers.ContentType!.Parameters.Add(new NameValueHeaderValue("action", $"\"{action}\""));
        return content;
    }

    public static async Task<HttpResponseMessage> PostAsync(
        HttpClient client,
        string url,
        string soapXml,
        string action,
        RayvarzSoapVersion version,
        CancellationToken ct)
    {
        using var content = CreateSoapContent(soapXml, action, version);
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content,
            Version = HttpVersion.Version11,
            VersionPolicy = HttpVersionPolicy.RequestVersionExact
        };
        request.Headers.ExpectContinue = false;
        if (version == RayvarzSoapVersion.Soap11)
            request.Headers.TryAddWithoutValidation("SOAPAction", $"\"{action}\"");

        return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
    }
}
