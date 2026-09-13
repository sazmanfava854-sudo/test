using System;
using System.Data.SqlClient;
using System.Text;

namespace RuleTrace
{
    internal static class MemberAnalyzer
    {
        public static void Print(string ruleEngine, int nidRuleClass, Action<string> log)
        {
            log("=== Member analysis NidClass=" + nidRuleClass + " ===");

            Query(ruleEngine, log, @"
SELECT COUNT(*) AS MemberCount,
       SUM(CASE WHEN CAST(XmlBody AS NVARCHAR(MAX)) LIKE '%M_Out%' THEN 1 ELSE 0 END) AS RowsWithM_Out
FROM dbo.Member WHERE NidClass = @nid", nidRuleClass);

            if (ColumnExists(ruleEngine, "Member", "NidCity"))
            {
                Query(ruleEngine, log, @"
SELECT NidCity, COUNT(*) AS Cnt FROM dbo.Member WHERE NidClass = @nid GROUP BY NidCity", nidRuleClass);
            }

            Query(ruleEngine, log, @"
SELECT TOP 25 DATALENGTH(XmlBody) AS XmlBytes,
       CASE WHEN CAST(XmlBody AS NVARCHAR(MAX)) LIKE '%M_Out%' THEN 1 ELSE 0 END AS HasM_Out
FROM dbo.Member WHERE NidClass = @nid", nidRuleClass);

            Query(ruleEngine, log, @"
SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Member' ORDER BY ORDINAL_POSITION", nidRuleClass);

            log("");
            log("If RowsWithM_Out = MemberCount: every XML carries the class shell — engine must merge as Partial Class.");
            log("If NidCity groups > 1: RunRule must filter by your CityGuid.");
        }

        private static void Query(string cs, Action<string> log, string sql, int nid)
        {
            try
            {
                using (var c = new SqlConnection(cs))
                using (var cmd = new SqlCommand(sql, c))
                {
                    cmd.Parameters.AddWithValue("@nid", nid);
                    c.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        var sb = new StringBuilder();
                        for (int i = 0; i < r.FieldCount; i++) sb.Append(r.GetName(i)).Append('\t');
                        log(sb.ToString());
                        while (r.Read())
                        {
                            sb.Clear();
                            for (int i = 0; i < r.FieldCount; i++)
                                sb.Append(r.IsDBNull(i) ? "" : r.GetValue(i).ToString()).Append('\t');
                            log(sb.ToString());
                        }
                    }
                }
                log("");
            }
            catch (Exception ex)
            {
                log("SQL skipped: " + ex.Message);
            }
        }

        private static bool ColumnExists(string cs, string table, string column)
        {
            try
            {
                using (var c = new SqlConnection(cs))
                using (var cmd = new SqlCommand("SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=@t AND COLUMN_NAME=@c", c))
                {
                    cmd.Parameters.AddWithValue("@t", table);
                    cmd.Parameters.AddWithValue("@c", column);
                    c.Open();
                    return cmd.ExecuteScalar() != null;
                }
            }
            catch { return false; }
        }
    }
}
