using Microsoft.Extensions.Configuration;

namespace RayvarzResend.Web;

/// <summary>فقط appsettings.json — بدون appsettings.Production / Development (جلوگیری از دو فایل روی سرور).</summary>
public static class AppSettingsConfiguration
{
    public static void UseSingleAppSettingsJsonOnly(ConfigurationManager configuration)
    {
        for (var i = configuration.Sources.Count - 1; i >= 0; i--)
        {
            if (configuration.Sources[i] is not FileConfigurationSource fileSource)
                continue;

            var path = fileSource.Path;
            if (string.IsNullOrWhiteSpace(path))
                continue;

            if (path.Equals("appsettings.json", StringComparison.OrdinalIgnoreCase))
                continue;

            if (path.StartsWith("appsettings.", StringComparison.OrdinalIgnoreCase)
                && path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                configuration.Sources.RemoveAt(i);
            }
        }
    }
}
