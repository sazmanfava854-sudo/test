using Microsoft.Data.SqlClient;
using RayvarzResend.Web.RuleEngine.Sync;

namespace RayvarzResend.Web.RuleEngine;

/// <summary>بارگذاری XmlBody از DbRuleEngein.dbo.Member یا فایل export (سرور 232).</summary>
public sealed class MemberRuleRepository
{
    private readonly IConfiguration _config;

    public MemberRuleRepository(IConfiguration config) => _config = config;

    public async Task<MemberRuleRecord?> LoadActiveMemberAsync(int nidMember, DateTime? asOf = null, CancellationToken ct = default)
    {
        var localPath = _config["RuleEngine:LocalXmlPath"];
        if (!string.IsNullOrWhiteSpace(localPath) && File.Exists(localPath))
        {
            var xml = await File.ReadAllTextAsync(localPath, ct);
            return new MemberRuleRecord
            {
                NidMember = nidMember,
                XmlBody = xml,
                Version = 0,
                VersionDateTime = File.GetLastWriteTimeUtc(localPath),
                Source = "LocalXmlPath"
            };
        }

        var cs = _config.GetConnectionString("RuleEngine") ?? _config["RuleEngine:ConnectionString"];
        if (string.IsNullOrWhiteSpace(cs))
            return null;

        const string sql = """
            SELECT TOP 1 NidMember, XmlBody, Body, Version, VersionDateTime, isActive, FromDate, ToDate
            FROM dbo.Member
            WHERE NidMember = @nid
              AND isActive = 1
              AND (@asOf IS NULL OR @asOf >= FromDate)
              AND (@asOf IS NULL OR ToDate IS NULL OR @asOf <= ToDate)
            ORDER BY Version DESC, VersionDateTime DESC
            """;

        await using var conn = new SqlConnection(cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);
        cmd.Parameters.AddWithValue("@asOf", (object?)asOf ?? DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;

        var xmlBody = reader.IsDBNull(reader.GetOrdinal("XmlBody"))
            ? ""
            : reader.GetString(reader.GetOrdinal("XmlBody"));
        if (string.IsNullOrWhiteSpace(xmlBody) && !reader.IsDBNull(reader.GetOrdinal("Body")))
            xmlBody = reader.GetString(reader.GetOrdinal("Body"));

        return new MemberRuleRecord
        {
            NidMember = reader.GetInt32(reader.GetOrdinal("NidMember")),
            XmlBody = xmlBody,
            Version = reader.IsDBNull(reader.GetOrdinal("Version")) ? 0 : Convert.ToInt32(reader.GetValue(reader.GetOrdinal("Version"))),
            VersionDateTime = reader.IsDBNull(reader.GetOrdinal("VersionDateTime"))
                ? null
                : SqlReaderValueCoercion.ToNullableDateTime(reader.GetValue(reader.GetOrdinal("VersionDateTime"))),
            Source = "DbRuleEngein"
        };
    }

    public async Task<MemberHistoryHead?> LoadLatestHistoryAsync(int nidMember, CancellationToken ct = default)
    {
        var cs = _config.GetConnectionString("RuleEngine") ?? _config["RuleEngine:ConnectionString"];
        if (string.IsNullOrWhiteSpace(cs))
            return null;

        const string sql = """
            SELECT TOP 1 NidHistory, NidMember, XmlBody, Body, ModifyDate, ModifyTime, Modifyer, ModifyDesc
            FROM dbo.MemberHistory
            WHERE NidMember = @nid
            ORDER BY NidHistory DESC
            """;

        try
        {
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nid", nidMember);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
                return null;

            var xmlBody = ReadOptionalString(reader, "XmlBody");
            if (string.IsNullOrWhiteSpace(xmlBody))
                xmlBody = ReadOptionalString(reader, "Body");

            return new MemberHistoryHead
            {
                NidHistory = Convert.ToInt64(reader.GetValue(reader.GetOrdinal("NidHistory"))),
                NidMember = reader.GetInt32(reader.GetOrdinal("NidMember")),
                XmlBody = xmlBody ?? "",
                ModifyUtc = TryReadModifyUtc(reader),
                Modifyer = ReadOptionalString(reader, "Modifyer"),
                ModifyDesc = ReadOptionalString(reader, "ModifyDesc")
            };
        }
        catch (SqlException)
        {
            return null;
        }
    }

    private static string? ReadOptionalString(SqlDataReader reader, string column)
    {
        try
        {
            var ord = reader.GetOrdinal(column);
            return reader.IsDBNull(ord) ? null : reader.GetString(ord);
        }
        catch (IndexOutOfRangeException)
        {
            return null;
        }
    }

    private static DateTime? TryReadModifyUtc(SqlDataReader reader)
    {
        try
        {
            var dateOrd = reader.GetOrdinal("ModifyDate");
            var date = reader.IsDBNull(dateOrd)
                ? null
                : SqlReaderValueCoercion.ToNullableDateTime(reader.GetValue(dateOrd));
            if (date is null)
                return null;

            TimeSpan? timeOfDay = null;
            try
            {
                var timeOrd = reader.GetOrdinal("ModifyTime");
                if (!reader.IsDBNull(timeOrd))
                    timeOfDay = SqlReaderValueCoercion.ToNullableTimeOfDay(reader.GetValue(timeOrd));
            }
            catch (IndexOutOfRangeException)
            {
                // ignore missing ModifyTime
            }

            return SqlReaderValueCoercion.CombineDateAndTime(date, timeOfDay);
        }
        catch (IndexOutOfRangeException)
        {
            return null;
        }
    }
}

public sealed class MemberRuleRecord
{
    public int NidMember { get; init; }
    public string XmlBody { get; init; } = "";
    public int Version { get; init; }
    public DateTime? VersionDateTime { get; init; }
    public string Source { get; init; } = "";
}
