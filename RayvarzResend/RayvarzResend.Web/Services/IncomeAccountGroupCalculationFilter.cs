using Microsoft.Data.SqlClient;

namespace RayvarzResend.Web.Services;

/// <summary>فیلتر Income_Calculation بر اساس ADP_IncomeAccountGroup — برای تهاتر درآمدی ۱۵۸.</summary>
public static class IncomeAccountGroupCalculationFilter
{
    public static async Task<HashSet<int>> LoadAllowedCalculationIdsAsync(
        string connectionString,
        int incomeAccountGroup,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT DISTINCT CI_IncomeCalculation
            FROM dbo.ADP_IncomeAccountGroup
            WHERE CI_IncomeAccountGroup = @g
            """;

        var set = new HashSet<int>();
        try
        {
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@g", incomeAccountGroup);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                if (reader.IsDBNull(0))
                    continue;
                var id = SqlDataReaderCoercion.ToInt32(reader.GetValue(0));
                if (id > 0)
                    set.Add(id);
            }
        }
        catch (SqlException)
        {
            return set;
        }

        return set;
    }
}
