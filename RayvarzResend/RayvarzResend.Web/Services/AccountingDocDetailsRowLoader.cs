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
            SELECT TRY_CONVERT(int, d.WrapperAccountNo) AS IncmNo,
                   d.Price AS Val,
                   ISNULL(d.AccountNoComments, '') AS IncmRowDsc
            FROM dbo.Accounting_DocDetails d
            WHERE d.FicheNo = @f
              AND TRY_CONVERT(int, d.WrapperAccountNo) > 0
              AND TRY_CONVERT(decimal(38, 0), d.Price) <> 0
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
                var incmNo = SqlDataReaderCoercion.ReadInt32(reader, "IncmNo");
                if (incmNo <= 0)
                    continue;

                var val = SqlDataReaderCoercion.ReadDecimal(reader, "Val");
                if (val == 0)
                    continue;

                rows.Add(new IncmRowDto
                {
                    IncmNo = incmNo,
                    Val = val,
                    IncmRowDsc = SqlDataReaderCoercion.ReadString(reader, "IncmRowDsc")
                });
            }
        }
        catch (SqlException)
        {
            return null;
        }
        catch (InvalidCastException)
        {
            return null;
        }

        return rows.Count > 0 ? rows : null;
    }
}
