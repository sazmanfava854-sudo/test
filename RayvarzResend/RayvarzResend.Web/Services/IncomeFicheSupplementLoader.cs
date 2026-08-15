using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>بارگذاری dbo.Income_OddmentAccount از Sara8M03.</summary>
public static class IncomeFicheSupplementLoader
{
    public static async Task EnrichAsync(
        FicheHeaderDto fiche,
        string saraConnectionString,
        CancellationToken ct = default)
    {
        if (fiche.Category != FicheCategory.Income || fiche.NidIncome is not { } nidIncome)
            return;

        fiche.Oddments = await LoadOddmentsAsync(saraConnectionString, nidIncome, ct);
    }

    public static async Task<List<IncomeOddmentDto>> LoadOddmentsAsync(
        string connectionString,
        Guid nidIncome,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT o.CI_IncomeCalculation AS IncmNo,
                   o.Value,
                   o.CI_OddmentType AS OddmentType
            FROM dbo.Income_OddmentAccount o
            WHERE o.NidIncome = @nid
            """;

        var list = new List<IncomeOddmentDto>();
        try
        {
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nid", nidIncome);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                list.Add(new IncomeOddmentDto
                {
                    IncmNo = ReadInt32(reader, "IncmNo"),
                    Value = ReadDecimal(reader, "Value"),
                    OddmentType = ReadInt32(reader, "OddmentType")
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
