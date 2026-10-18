using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>ردیف‌های از قبل ساخته‌شده در Sara — parity ClsAccounting.Save قبل از رایورز.</summary>
public static class AccountingDocDetailsRowLoader
{
    public static async Task<List<IncmRowDto>?> TryLoadAsync(
        string connectionString,
        string ficheNo,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(ficheNo))
            return null;

        const string sql = """
            SELECT d.WrapperAccountNo AS IncmNo,
                   d.Price AS Val,
                   ISNULL(d.AccountNoComments, '') AS IncmRowDsc
            FROM dbo.Accounting_DocDetails d
            WHERE d.FicheNo = @f
              AND d.WrapperAccountNo > 0
              AND d.Price <> 0
            ORDER BY d.IncmRow
            """;

        var rows = new List<IncmRowDto>();
        try
        {
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@f", ficheNo.Trim());
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                rows.Add(new IncmRowDto
                {
                    IncmNo = reader.GetInt32(reader.GetOrdinal("IncmNo")),
                    Val = reader.GetDecimal(reader.GetOrdinal("Val")),
                    IncmRowDsc = reader.GetString(reader.GetOrdinal("IncmRowDsc"))
                });
            }
        }
        catch (SqlException)
        {
            return null;
        }

        return rows.Count > 0 ? rows : null;
    }
}
