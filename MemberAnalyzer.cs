using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Xml.Linq;

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

            // one row per Member: which rows are active / versions / where the code actually lives (Body vs XmlBody vs EncryptXmlBody)
            Query(ruleEngine, log, @"
SELECT NidMember, EnumType, isActive, Version, RTRIM(FromDate) AS FromDate, RTRIM(ToDate) AS ToDate,
       DATALENGTH(Body) AS BodyBytes, DATALENGTH(XmlBody) AS XmlBytes, DATALENGTH(EncryptXmlBody) AS EncBytes,
       CASE WHEN CAST(XmlBody AS NVARCHAR(MAX)) LIKE '%M_Out%' THEN 1 ELSE 0 END AS HasM_Out
FROM dbo.Member WHERE NidClass = @nid ORDER BY NidMember, Version", nidRuleClass);

            // duplicate NidMember (several versions) => engine must pick one version; if it takes all, BC30269 follows
            Query(ruleEngine, log, @"
SELECT NidMember, COUNT(*) AS Versions, SUM(CASE WHEN isActive = 1 THEN 1 ELSE 0 END) AS ActiveRows, MAX(Version) AS MaxVersion
FROM dbo.Member WHERE NidClass = @nid GROUP BY NidMember HAVING COUNT(*) > 1", nidRuleClass);

            Query(ruleEngine, log, @"
SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Member' ORDER BY ORDINAL_POSITION", nidRuleClass);

            DumpXmlStructure(ruleEngine, nidRuleClass, log);

            log("");
            log("RowsWithM_Out < MemberCount: only some members carry M_Out shell — engine must merge as ONE class + methods.");
            log("If XmlBytes large and <Body> has VB text but compile fails BC30269: engine reads EncryptXmlBody (EncBytes) not XmlBody.");
            log("If a NidMember has Versions > 1 with several ActiveRows: engine is not filtering versions => duplicates.");
        }

        /// <summary>Element layout of the first two XmlBody rows: shows where the VB code and member name live.</summary>
        private static void DumpXmlStructure(string cs, int nid, Action<string> log)
        {
            try
            {
                using (var c = new SqlConnection(cs))
                using (var cmd = new SqlCommand("SELECT TOP 2 NidMember, CAST(XmlBody AS NVARCHAR(MAX)) FROM dbo.Member WHERE NidClass=@nid ORDER BY DATALENGTH(XmlBody) DESC", c))
                {
                    cmd.Parameters.AddWithValue("@nid", nid);
                    c.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            log("--- XmlBody structure NidMember=" + r.GetValue(0) + " ---");
                            if (r.IsDBNull(1)) { log("(null)"); continue; }
                            XDocument doc;
                            try { doc = XDocument.Parse(r.GetString(1)); }
                            catch (Exception ex) { log("XML parse error: " + ex.Message); continue; }
                            if (doc.Root == null) continue;
                            log("<" + doc.Root.Name.LocalName + "> " + string.Join(" ", doc.Root.Attributes().Select(a => a.Name.LocalName + "=\"" + Trunc(a.Value, 40) + "\"")));
                            int shown = 0;
                            foreach (var el in doc.Root.Descendants())
                            {
                                if (shown++ >= 40) { log("  ..."); break; }
                                string indent = new string(' ', 2 * (el.Ancestors().Count()));
                                string val = el.HasElements ? "" : " = " + (el.Value.Length > 80 ? "(text " + el.Value.Length + " chars) \"" + Trunc(el.Value, 60) + "…\"" : "\"" + el.Value + "\"");
                                log(indent + "<" + el.Name.LocalName + ">" + string.Join("", el.Attributes().Select(a => " @" + a.Name.LocalName + "=" + Trunc(a.Value, 30))) + val);
                            }
                        }
                    }
                }
                log("");
            }
            catch (Exception ex)
            {
                log("XML structure skipped: " + ex.Message);
            }
        }

        private static string Trunc(string s, int n)
        {
            if (s == null) return "";
            s = s.Replace("\r", " ").Replace("\n", " ");
            return s.Length <= n ? s : s.Substring(0, n);
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
