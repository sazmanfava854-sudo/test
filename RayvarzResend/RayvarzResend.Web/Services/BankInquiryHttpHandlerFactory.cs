using System.Net;
using System.Net.Security;
using System.Security.Authentication;

namespace RayvarzResend.Web.Services;

internal static class BankInquiryHttpHandlerFactory
{
    public static SocketsHttpHandler Create(IConfiguration config, BankInquiryConfirmOptions options)
    {
        var proxyUrl = FirstNonEmpty(options.ProxyUrl, config["Rayvarz:ProxyUrl"]);
        var useSystemProxy = options.UseSystemProxy || config.GetValue<bool>("Rayvarz:UseSystemProxy");
        var allowInvalidSsl = options.AllowInvalidSsl || config.GetValue<bool>("Rayvarz:AllowInvalidSsl");

        var handler = new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            SslOptions = new SslClientAuthenticationOptions
            {
                EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
            }
        };

        if (!string.IsNullOrWhiteSpace(proxyUrl))
        {
            handler.Proxy = new WebProxy(proxyUrl);
            handler.UseProxy = true;
        }
        else if (useSystemProxy)
        {
            handler.Proxy = HttpClient.DefaultProxy;
            handler.UseProxy = true;
            handler.DefaultProxyCredentials = CredentialCache.DefaultCredentials;
        }
        else
        {
            handler.UseProxy = false;
        }

        if (allowInvalidSsl)
            handler.SslOptions.RemoteCertificateValidationCallback = static (_, _, _, _) => true;

        return handler;
    }

    private static string? FirstNonEmpty(string? primary, string? fallback)
    {
        if (!string.IsNullOrWhiteSpace(primary))
            return primary.Trim();
        if (!string.IsNullOrWhiteSpace(fallback))
            return fallback.Trim();
        return null;
    }
}
