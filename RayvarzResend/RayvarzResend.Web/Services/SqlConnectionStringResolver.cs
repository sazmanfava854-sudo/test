using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace RayvarzResend.Web.Services;

/// <summary>
/// خواندن ConnectionString با پشتیبانی از نام instance جدا (جلوگیری از خطای JSON در 172.16.8.138\sql2012).
/// </summary>
public static class SqlConnectionStringResolver
{
    public static string? Get(
        IConfiguration config,
        string connectionStringName,
        string? sqlServerOverride = null,
        string? sqlInstanceOverride = null)
    {
        var name = (connectionStringName ?? "").Trim();
        if (name.Length == 0)
            return null;

        var server = (sqlServerOverride ?? config["Shahkar:SqlServer"])?.Trim();
        var instance = (sqlInstanceOverride ?? config["Shahkar:SqlInstance"])?.Trim();

        var cs = config.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(cs))
        {
            if (string.IsNullOrEmpty(server))
                return null;
            cs = "";
        }

        var builder = new SqlConnectionStringBuilder(cs);
        ApplyServerAndInstance(builder, server, instance);
        EnsureLegacySqlDefaults(builder);
        return builder.ConnectionString;
    }

    public static ConnectionStringSummary Summarize(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return new ConnectionStringSummary(false, "", "", false, "", false);

        var builder = new SqlConnectionStringBuilder(connectionString);
        return new ConnectionStringSummary(
            true,
            builder.DataSource,
            builder.InitialCatalog,
            builder.IntegratedSecurity,
            builder.IntegratedSecurity ? "" : builder.UserID,
            !builder.IntegratedSecurity && !string.IsNullOrEmpty(builder.Password));
    }

    public static string? BuildLoginFailedHint(
        string connectionStringName,
        ConnectionStringSummary summary,
        Exception ex)
    {
        var msg = (ex.Message + " " + (ex.InnerException?.Message ?? "")).ToLowerInvariant();
        if (!msg.Contains("login failed", StringComparison.Ordinal))
            return null;

        var hints = new List<string>
        {
            $"کلید تنظیم: ConnectionStrings:{connectionStringName}",
            "رمز عبور و User Id را در appsettings.json (همان پوشه start.bat) با SSMS مقایسه کنید.",
            "اگر رمز شامل ; یا \" یا \\ است، در JSON باید escape شود."
        };

        if (!summary.HasPassword && !summary.IntegratedSecurity)
            hints.Add("رمز عبور در ConnectionString خالی است — احتمالاً بعد از ; در Password رشته قطع شده.");

        if (!string.IsNullOrEmpty(summary.DataSource)
            && !summary.DataSource.Contains('\\', StringComparison.Ordinal)
            && !summary.DataSource.Contains(',', StringComparison.Ordinal)
            && summary.DataSource.Contains("sql", StringComparison.OrdinalIgnoreCase))
        {
            hints.Add(
                "نام instance احتمالاً بدون \\ در JSON چسبیده است (مثلاً 172.16.8.138sql2012). " +
                "در بخش Shahkar مقدار SqlServer و SqlInstance را تنظیم کنید یا در JSON از \\\\sql2012 استفاده کنید.");
        }
        else if (!string.IsNullOrEmpty(summary.DataSource)
                 && !summary.DataSource.Contains('\\', StringComparison.Ordinal)
                 && !summary.DataSource.Contains(',', StringComparison.Ordinal))
        {
            hints.Add(
                "برای SQL نام‌دار، در JSON از Server=IP\\\\instance استفاده کنید یا Shahkar:SqlServer و Shahkar:SqlInstance را پر کنید.");
        }

        return string.Join(" ", hints);
    }

    private static void ApplyServerAndInstance(
        SqlConnectionStringBuilder builder,
        string? server,
        string? instance)
    {
        if (!string.IsNullOrEmpty(server))
        {
            builder.DataSource = string.IsNullOrEmpty(instance) ? server : $"{server}\\{instance}";
            return;
        }

        if (string.IsNullOrEmpty(instance))
            return;

        var ds = (builder.DataSource ?? "").Trim();
        if (ds.Length == 0)
            return;

        if (ds.Contains('\\', StringComparison.Ordinal) || ds.Contains(',', StringComparison.Ordinal))
            return;

        if (ds.EndsWith(instance, StringComparison.OrdinalIgnoreCase) && ds.Length > instance.Length)
        {
            builder.DataSource = ds[..^instance.Length] + "\\" + instance;
            return;
        }

        builder.DataSource = ds + "\\" + instance;
    }

    private static void EnsureLegacySqlDefaults(SqlConnectionStringBuilder builder)
    {
        // SQL Server 2012 و پایین‌تر: Encrypt پیش‌فرض در SqlClient جدید گاهی مانع اتصال می‌شود.
        if (!builder.ContainsKey("Encrypt"))
            builder.Encrypt = false;
    }
}

public readonly record struct ConnectionStringSummary(
    bool IsConfigured,
    string DataSource,
    string Database,
    bool IntegratedSecurity,
    string UserId,
    bool HasPassword);
