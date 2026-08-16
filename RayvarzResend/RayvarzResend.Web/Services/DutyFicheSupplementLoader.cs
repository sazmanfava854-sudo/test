using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>بارگذاری dbo.Duty_OddmentAccount از Sara8M03 — join با Duty_FicheSub از طریق NidFK.</summary>
public static class DutyFicheSupplementLoader
{
    public static async Task EnrichAsync(
        FicheHeaderDto fiche,
        string saraConnectionString,
        int eumDutyType,
        CancellationToken ct = default)
    {
        if (fiche.Category is not (FicheCategory.DutyNosazi or FicheCategory.DutySenfi))
            return;

        fiche.DutyOddments = await LoadOddmentsAsync(
            saraConnectionString,
            fiche.NidFiche,
            eumDutyType,
            ct);
    }

    public static async Task<List<DutyOddmentDto>> LoadOddmentsAsync(
        string connectionString,
        Guid nidFiche,
        int eumDutyType,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT o.CI_DutyFormula AS DutyFormula,
                   ISNULL(o.CI_DutyOddmentFor, 0) AS DutyFormulaFiche,
                   o.Price,
                   o.CI_OddmentType AS OddmentType,
                   NULLIF(LTRIM(RTRIM(o.FicheNo)), '') AS FicheNo,
                   o.CI_DutyYear AS DutyYear
            FROM dbo.Duty_OddmentAccount o
            INNER JOIN (
                SELECT DISTINCT fs.NidFK
                FROM dbo.Duty_FicheSub fs
                WHERE fs.NidFiche = @nidFiche
            ) fk ON fk.NidFK = o.NidFK
            WHERE ISNULL(o.IsCancel, 0) = 0
              AND (o.EumDutyType IS NULL OR o.EumDutyType = 0 OR o.EumDutyType = @dutyType)
            """;

        var list = new List<DutyOddmentDto>();
        try
        {
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nidFiche", nidFiche);
            cmd.Parameters.AddWithValue("@dutyType", eumDutyType);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                list.Add(new DutyOddmentDto
                {
                    DutyFormula = ReadInt32(reader, "DutyFormula"),
                    DutyFormulaFiche = ReadInt32(reader, "DutyFormulaFiche"),
                    Price = ReadDecimal(reader, "Price"),
                    OddmentType = ReadInt32(reader, "OddmentType"),
                    FicheNo = reader.IsDBNull(reader.GetOrdinal("FicheNo"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("FicheNo")),
                    DutyYear = reader.IsDBNull(reader.GetOrdinal("DutyYear"))
                        ? null
                        : ReadInt32(reader, "DutyYear")
                });
            }
        }
        catch (SqlException)
        {
            return [];
        }

        return list;
    }

    private static int ReadInt32(SqlDataReader reader, string column)
    {
        var ord = reader.GetOrdinal(column);
        return reader.IsDBNull(ord) ? 0 : Convert.ToInt32(reader.GetValue(ord));
    }

    private static decimal ReadDecimal(SqlDataReader reader, string column)
    {
        var ord = reader.GetOrdinal(column);
        return reader.IsDBNull(ord) ? 0 : Convert.ToDecimal(reader.GetValue(ord));
    }
}
