using System;
using System.Configuration;
using System.Data.SqlClient;

namespace RuleTrace
{
    internal static class MemberAnalyzer
    {
        public static void Print(int nidRuleClass)
        {
            string ruleEngine = ConfigurationManager.ConnectionStrings["RuleEngine"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(ruleEngine))
            {
                Console.Error.WriteLine("ERROR: RuleEngine connection string missing");
                return;
            }

            Console.WriteLine("=== Member analysis NidClass={0} ===", nidRuleClass);

            TryQuery(ruleEngine, @"
SELECT COUNT(*) AS MemberCount,
       SUM(CASE WHEN CAST(XmlBody AS NVARCHAR(MAX)) LIKE '%M_Out%' THEN 1 ELSE 0 END) AS RowsWithM_Out
FROM dbo.Member WHERE NidClass = @nid", nidRuleClass);

            if (ColumnExists(ruleEngine, "Member", "NidCity"))
            {
                TryQuery(ruleEngine, @"
SELECT NidCity, COUNT(*) AS Cnt
FROM dbo.Member WHERE NidClass = @nid
GROUP BY NidCity", nidRuleClass);
            }

            TryQuery(ruleEngine, @"
SELECT TOP 20
       DATALENGTH(XmlBody) AS XmlBytes,
       CASE WHEN CAST(XmlBody AS NVARCHAR(200)) LIKE '%M_Out%' THEN 1 ELSE 0 END AS HasM_Out
FROM dbo.Member WHERE NidClass = @nid", nidRuleClass);

            Console.WriteLine();
            Console.WriteLine("If multiple NidCity groups: RunRule must filter by your CityGuid.");
            Console.WriteLine("If RowsWithM_Out = MemberCount: each XML has M_Out — local compile needs correct SafaClassDesingerNew.dll from server.");
        }

        private static bool ColumnExists(string connectionString, string table, string column)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(
                    "SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=@t AND COLUMN_NAME=@c", conn))
                {
                    cmd.Parameters.AddWithValue("@t", table);
                    cmd.Parameters.AddWithValue("@c", column);
                    conn.Open();
                    return cmd.ExecuteScalar() != null;
                }
            }
            catch { return false; }
        }

        private static void TryQuery(string connectionString, string sql, int nidRuleClass)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@nid", nidRuleClass);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        int fieldCount = reader.FieldCount;
                        bool header = true;
                        while (reader.Read())
                        {
                            if (header)
                            {
                                for (int i = 0; i < fieldCount; i++)
                                    Console.Write(reader.GetName(i) + "\t");
                                Console.WriteLine();
                                header = false;
                            }
                            for (int i = 0; i < fieldCount; i++)
                                Console.Write((reader.IsDBNull(i) ? "" : reader.GetValue(i).ToString()) + "\t");
                            Console.WriteLine();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SQL skipped: {0}", ex.Message);
            }
        }
    }
}
