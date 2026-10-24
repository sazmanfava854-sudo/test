using Microsoft.Data.SqlClient;

namespace RayvarzResend.Web.Services;

public sealed class ShahkarDatabaseAccessException : Exception
{
    public ShahkarDatabaseAccessException(string message, string? hint = null)
        : base(message)
    {
        Hint = hint;
    }

    public string? Hint { get; }
}

public readonly record struct ShahkarDatabaseProbe(
    bool Found,
    string ExactName,
    string State,
    bool? HasAccess,
    bool CanViewAnyDatabase);

/// <summary>
/// خطای 4060 یعنی ورود قبول شده و خود پایگاه باز نشده است.
/// یک‌بار master را باز می‌کند تا نام واقعی پایگاه، وضعیت، یا instance اشتباه مشخص شود.
/// </summary>
public static class ShahkarConnectionOpener
{
    public static async Task<SqlConnection> OpenAsync(string connectionString, CancellationToken ct)
    {
        var summary = SqlConnectionStringResolver.Summarize(connectionString);
        var requested = summary.Database;
        var conn = new SqlConnection(connectionString);
        try
        {
            await conn.OpenAsync(ct);
            return conn;
        }
        catch (SqlException ex) when (IsCannotOpenDatabase(ex) && !string.IsNullOrWhiteSpace(requested))
        {
            await conn.DisposeAsync();
            return await RecoverFromDatabaseRefusalAsync(connectionString, summary, ct);
        }
    }

    public static bool IsCannotOpenDatabase(SqlException ex)
    {
        if (ex.Number == 4060)
            return true;
        foreach (SqlError err in ex.Errors)
        {
            if (err.Number == 4060)
                return true;
        }
        return ex.Message.Contains("Cannot open database", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<SqlConnection> RecoverFromDatabaseRefusalAsync(
        string connectionString,
        ConnectionStringSummary summary,
        CancellationToken ct)
    {
        var passwordLength = SqlConnectionStringResolver.PasswordLength(connectionString);
        var master = new SqlConnection(SqlConnectionStringResolver.WithDatabase(connectionString, "master"));
        try
        {
            await master.OpenAsync(ct);
        }
        catch (SqlException ex)
        {
            await master.DisposeAsync();
            throw new ShahkarDatabaseAccessException(
                ShahkarDatabaseMessages.DescribeMasterFailure(summary, passwordLength, ex.Message),
                SqlConnectionStringResolver.BuildLoginFailedHint("Security", summary, ex));
        }

        ShahkarDatabaseProbe probe;
        try
        {
            var canViewAny = await CanViewAnyDatabaseAsync(master, ct);
            probe = await ProbeAsync(master, summary.Database, canViewAny, ct);
        }
        finally
        {
            await master.DisposeAsync();
        }

        if (probe.Found
            && !string.Equals(probe.ExactName, summary.Database, StringComparison.Ordinal)
            && probe.HasAccess != false
            && probe.State.Equals("ONLINE", StringComparison.OrdinalIgnoreCase))
        {
            var retry = new SqlConnection(SqlConnectionStringResolver.WithDatabase(connectionString, probe.ExactName));
            try
            {
                await retry.OpenAsync(ct);
                return retry;
            }
            catch (SqlException)
            {
                await retry.DisposeAsync();
            }
        }

        var (message, hint) = ShahkarDatabaseMessages.DescribeRefusal(summary, passwordLength, probe);
        throw new ShahkarDatabaseAccessException(message, hint);
    }

    private static async Task<bool> CanViewAnyDatabaseAsync(SqlConnection master, CancellationToken ct)
    {
        try
        {
            await using var cmd = new SqlCommand(
                "SELECT HAS_PERMS_BY_NAME(NULL, NULL, 'VIEW ANY DATABASE')", master);
            var value = await cmd.ExecuteScalarAsync(ct);
            return value != null && value != DBNull.Value && Convert.ToInt32(value) == 1;
        }
        catch (SqlException)
        {
            return false;
        }
    }

    private static async Task<ShahkarDatabaseProbe> ProbeAsync(
        SqlConnection master,
        string database,
        bool canViewAny,
        CancellationToken ct)
    {
        const string collated = """
            SELECT TOP (1) name, state_desc, HAS_DBACCESS(name) AS has_access
            FROM sys.databases
            WHERE name COLLATE Latin1_General_CI_AI = @db COLLATE Latin1_General_CI_AI
            """;
        const string lowered = """
            SELECT TOP (1) name, state_desc, HAS_DBACCESS(name) AS has_access
            FROM sys.databases
            WHERE LOWER(name) = LOWER(@db)
            """;

        try
        {
            return await ReadProbeAsync(master, collated, database, canViewAny, ct);
        }
        catch (SqlException)
        {
            return await ReadProbeAsync(master, lowered, database, canViewAny, ct);
        }
    }

    private static async Task<ShahkarDatabaseProbe> ReadProbeAsync(
        SqlConnection master,
        string sql,
        string database,
        bool canViewAny,
        CancellationToken ct)
    {
        await using var cmd = new SqlCommand(sql, master);
        cmd.Parameters.AddWithValue("@db", database);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return new ShahkarDatabaseProbe(false, "", "", null, canViewAny);

        var name = reader.GetString(0);
        var state = reader.IsDBNull(1) ? "" : reader.GetString(1);
        bool? access = reader.IsDBNull(2) ? null : Convert.ToInt32(reader.GetValue(2)) == 1;
        return new ShahkarDatabaseProbe(true, name, state, access, canViewAny);
    }
}

public static class ShahkarDatabaseMessages
{
    public static string DescribeMasterFailure(ConnectionStringSummary summary, int passwordLength, string sqlMessage)
    {
        var target = SqlConnectionStringResolver.TargetLine(summary, passwordLength);
        return target + "\n"
            + "ورود به پایگاه master هم ناموفق بود. رمز یا instance با SSMS یکی نیست.\n"
            + "اگر طول رمز کمتر از رمز SSMS است، کاراکتر ; رشته ConnectionString را قطع کرده — رمز را در Shahkar:Password بگذارید.\n"
            + sqlMessage;
    }

    public static (string Message, string Hint) DescribeRefusal(
        ConnectionStringSummary summary,
        int passwordLength,
        ShahkarDatabaseProbe probe)
    {
        var target = SqlConnectionStringResolver.TargetLine(summary, passwordLength);
        var user = string.IsNullOrWhiteSpace(summary.UserId) ? "hService" : summary.UserId.Trim();
        var database = string.IsNullOrWhiteSpace(summary.Database) ? "security10" : summary.Database.Trim();

        if (!probe.Found)
        {
            var grant = GrantScript(user, database);
            var hidden = probe.CanViewAnyDatabase
                ? $"این login می‌تواند فهرست پایگاه‌ها را ببیند و {database} در این instance نیست."
                : $"این login فهرست همه پایگاه‌ها را نمی‌بیند؛ یا instance اشتباه است، یا کاربر داخل {database} ساخته نشده.";
            return (
                target + "\n"
                + $"ورود کاربر {user} به master موفق بود، اما پایگاه «{database}» برایش دیده نشد.\n"
                + hidden + "\n"
                + "در SSMS که وصل می‌شوید پورت را بگیرید و در Shahkar:SqlPort بگذارید:\n"
                + "SELECT local_tcp_port FROM sys.dm_exec_connections WHERE session_id = @@SPID;\n"
                + "اگر پورت همان است، اسکریپت دسترسی را با login مدیر اجرا کنید:\n"
                + grant,
                grant);
        }

        if (!probe.State.Equals("ONLINE", StringComparison.OrdinalIgnoreCase))
        {
            return (
                target + $"\nپایگاه «{probe.ExactName}» روی سرور هست ولی وضعیت آن {probe.State} است.",
                "تا پایگاه ONLINE نشود تب شاهکار وصل نمی‌شود.");
        }

        if (probe.HasAccess == true)
        {
            return (
                target + $"\nپایگاه «{probe.ExactName}» برای این login قابل‌دیدن است اما Open همان نام شکست.",
                "یک‌بار برنامه را ببندید و دوباره start.bat را اجرا کنید. اگر ماند، نام دقیق پایگاه را در Shahkar:Database بگذارید.");
        }

        var script = GrantScript(user, probe.ExactName);
        return (
            target + "\n"
            + $"رمز {user} قبول شد، ولی این login اجازه باز کردن پایگاه «{probe.ExactName}» را ندارد.\n"
            + "در SSMS اگر بدون زدن دیتابیس security10 وصل می‌شوید، فقط به سرور وصل شده‌اید نه به این پایگاه.\n"
            + "اسکریپت را با یک login مدیر (sa یا ویندوز admin) روی همان instance اجرا کنید:\n"
            + script,
            script);
    }

    public static string QuoteName(string name) =>
        "[" + (name ?? "").Replace("]", "]]", StringComparison.Ordinal) + "]";

    public static string GrantScript(string user, string database)
    {
        var quotedUser = QuoteName(user);
        var quotedDb = QuoteName(database);
        var literal = (user ?? "").Replace("'", "''", StringComparison.Ordinal);
        return $"""
            USE {quotedDb};
            IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'{literal}')
                CREATE USER {quotedUser} FOR LOGIN {quotedUser};
            ELSE
                ALTER USER {quotedUser} WITH LOGIN = {quotedUser};
            ALTER ROLE db_datareader ADD MEMBER {quotedUser};
            ALTER ROLE db_datawriter ADD MEMBER {quotedUser};
            """;
    }
}
