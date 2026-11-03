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
        string? sqlInstanceOverride = null,
        int sqlPort = 0,
        string? databaseOverride = null,
        string? userIdOverride = null,
        string? passwordOverride = null)
    {
        var name = (connectionStringName ?? "").Trim();
        if (name.Length == 0)
            return null;

        var server = (sqlServerOverride ?? config["Shahkar:SqlServer"])?.Trim();
        var instance = (sqlInstanceOverride ?? config["Shahkar:SqlInstance"])?.Trim();
        var port = sqlPort > 0 ? sqlPort : config.GetValue("Shahkar:SqlPort", 0);
        var database = (databaseOverride ?? config["Shahkar:Database"])?.Trim();
        var userId = (userIdOverride ?? config["Shahkar:UserId"])?.Trim();
        var password = string.IsNullOrEmpty(passwordOverride) ? config["Shahkar:Password"] : passwordOverride;

        var cs = config.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(cs))
        {
            if (string.IsNullOrEmpty(server) && string.IsNullOrEmpty(userId))
                return null;
            cs = "";
        }

        var builder = new SqlConnectionStringBuilder(cs);
        ApplyServerAndInstance(builder, server, instance, port);
        if (!string.IsNullOrEmpty(database))
            builder.InitialCatalog = database;
        if (!string.IsNullOrEmpty(userId))
        {
            builder.UserID = userId;
            builder.IntegratedSecurity = false;
        }
        if (!string.IsNullOrEmpty(password))
        {
            builder.Password = password;
            builder.IntegratedSecurity = false;
        }
        EnsureLegacySqlDefaults(builder);
        builder.TrustServerCertificate = true;
        return builder.ConnectionString;
    }

    public static string WithDatabase(string connectionString, string database)
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = database
        };
        return builder.ConnectionString;
    }

    public static int PasswordLength(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return 0;
        return new SqlConnectionStringBuilder(connectionString).Password?.Length ?? 0;
    }

    public static string TargetLine(ConnectionStringSummary summary, int passwordLength) =>
        $"سرور مؤثر: {summary.DataSource} | پایگاه: {summary.Database} | کاربر: {summary.UserId} | طول رمز: {passwordLength}";

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
        string? instance,
        int port)
    {
        if (port > 0)
        {
            var host = !string.IsNullOrEmpty(server) ? server : HostOnly(builder.DataSource);
            if (!string.IsNullOrEmpty(host))
                builder.DataSource = host + "," + port;
            return;
        }

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

    private static string HostOnly(string? dataSource)
    {
        var ds = (dataSource ?? "").Trim();
        var slash = ds.IndexOf('\\');
        if (slash > 0)
            ds = ds[..slash];
        var comma = ds.IndexOf(',');
        if (comma > 0)
            ds = ds[..comma];
        return ds.Trim();
    }

    private static void EnsureLegacySqlDefaults(SqlConnectionStringBuilder builder)
    {
        // SqlClient 5 پیش‌فرض Encrypt=true است. روی SQL Server 2012 باید خاموش بماند،
        // و ContainsKey("Encrypt") حتی وقتی در رشته نیست true برمی‌گرداند.
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
