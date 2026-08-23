using Microsoft.Data.SqlClient;

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
                : reader.GetDateTime(reader.GetOrdinal("VersionDateTime")),
            Source = "DbRuleEngein.Member"
        };
    }

    /// <summary>آخرین رکورد MemberHistory برای تشخیص تغییر قانون.</summary>
    public async Task<MemberHistoryRecord?> LoadLatestHistoryAsync(int nidMember, CancellationToken ct = default)
    {
        var cs = _config.GetConnectionString("RuleEngine") ?? _config["RuleEngine:ConnectionString"];
        if (string.IsNullOrWhiteSpace(cs))
            return null;

        const string sql = """
            SELECT TOP 1
                h.NidHistory,
                h.NidClass,
                h.NidMember,
                h.XmlBody,
                h.Modifyer,
                h.ModifyDesc,
                h.ModifyDate,
                h.ModifyTime
            FROM dbo.MemberHistory h
            WHERE h.NidMember = @nid
            ORDER BY h.NidHistory DESC
            """;

        await using var conn = new SqlConnection(cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidMember);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;

        var xmlBody = reader.IsDBNull(reader.GetOrdinal("XmlBody"))
            ? ""
            : reader.GetString(reader.GetOrdinal("XmlBody"));

        var modifyDate = reader.IsDBNull(reader.GetOrdinal("ModifyDate"))
            ? null
            : reader.GetValue(reader.GetOrdinal("ModifyDate"));
        var modifyTime = reader.IsDBNull(reader.GetOrdinal("ModifyTime"))
            ? null
            : reader.GetValue(reader.GetOrdinal("ModifyTime"));

        return new MemberHistoryRecord
        {
            NidHistory = Convert.ToInt64(reader.GetValue(reader.GetOrdinal("NidHistory"))),
            NidClass = reader.IsDBNull(reader.GetOrdinal("NidClass")) ? 0 : reader.GetInt32(reader.GetOrdinal("NidClass")),
            NidMember = reader.GetInt32(reader.GetOrdinal("NidMember")),
            XmlBody = xmlBody,
            Modifyer = reader.IsDBNull(reader.GetOrdinal("Modifyer")) ? null : reader.GetString(reader.GetOrdinal("Modifyer")),
            ModifyDesc = reader.IsDBNull(reader.GetOrdinal("ModifyDesc")) ? null : reader.GetString(reader.GetOrdinal("ModifyDesc")),
            ModifyDateRaw = modifyDate?.ToString(),
            ModifyTimeRaw = modifyTime?.ToString(),
            ModifyDateTime = MemberHistoryDateParser.CombineModifyDateTime(modifyDate, modifyTime)
        };
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

public sealed class MemberHistoryRecord
{
    public long NidHistory { get; init; }
    public int NidClass { get; init; }
    public int NidMember { get; init; }
    public string XmlBody { get; init; } = "";
    public string? Modifyer { get; init; }
    public string? ModifyDesc { get; init; }
    public string? ModifyDateRaw { get; init; }
    public string? ModifyTimeRaw { get; init; }
    public DateTime ModifyDateTime { get; init; }
}
