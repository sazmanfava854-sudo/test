using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RuleTrace
{
    /// <summary>
    /// Overall formula documentation in DbRuleEngeinDocument.
    /// Main catalog is dbo.MemberDocument (the SSMS TOP 1000 query) — not a Solh-only subset.
    /// Other dbo tables in the same catalog are listed and peeked read-only.
    /// </summary>
    internal static class RuleDocs
    {
        public const string Catalog = "DbRuleEngeinDocument";
        public const string MainTable = "MemberDocument";

        public static readonly string[] UserColumns =
        {
            "DocId", "Title", "NidMember", "MemberDocument", "LastEditOn", "Sort", "ParentDocId", "UserName",
        };

        public static readonly string[] TableHints =
        {
            "Zabeteh", "CI_PlanType", "CI_PlanUsingType", "CI_Zabeteh",
            "ZabeteStatic_Zabete", "ZabeteStatic_Info", "ZabeteStatic_Plan",
            "Sh_RequestInfo", "ActiveNidZabeteh", "NidProc",
            "Solh", "صلح", "ضابطه", "ماده 5", "ماده ۵", "کمیسیون", "چیدمان",
            "تحلیل", "تخلف", "درآمد", "ماده 100", "ماده صد", "پروانه", "تجدید",
        };

        private static readonly string[] SkipTypes = { "image", "varbinary", "binary", "timestamp", "rowversion" };

        /// <summary>Exact overall catalog the user posted — TOP 1000, no NidMember filter.</summary>
        public static string CatalogSql(string bodyExpr)
        {
            if (string.IsNullOrWhiteSpace(bodyExpr)) bodyExpr = DefaultBodyExpr();
            return
                "SELECT TOP (1000) [DocId], [Title], [NidMember], " +
                "CASE WHEN DATALENGTH([MemberDocument]) IS NULL THEN 0 ELSE DATALENGTH([MemberDocument]) END AS DocBytes, " +
                bodyExpr + " AS MemberDocument, " +
                "[LastEditOn], [Sort], [ParentDocId], [UserName] " +
                "FROM [dbo].[MemberDocument] " +
                "ORDER BY [ParentDocId], [Sort], [DocId]";
        }

        public static string DefaultBodyExpr()
        {
            return "LEFT(CONVERT(NVARCHAR(MAX), [MemberDocument]), 2000)";
        }

        public static Dictionary<string, object> Read(string ruleEngine, Action<string> log)
        {
            if (log == null) log = m => { };
            var tables = new List<Dictionary<string, object>>();
            var docs = new List<Dictionary<string, object>>();
            var matches = new List<Dictionary<string, object>>();
            var otherRows = new List<Dictionary<string, object>>();

            if (string.IsNullOrWhiteSpace(ruleEngine))
            {
                log("Doc         : RuleEngine خالی — " + Catalog + " خوانده نشد");
                return Pack(tables, docs, matches, otherRows);
            }

            string cs = ZabetehCase.WithCatalog(ruleEngine, Catalog);
            log("Doc         : catalog=" + Catalog + " user query = SELECT TOP (1000) DocId, Title, NidMember, MemberDocument, LastEditOn, Sort, ParentDocId, UserName");

            try
            {
                using (var c = new SqlConnection(cs))
                {
                    c.Open();
                    log("Doc         : db=" + c.Database + " login=" + CurrentLogin(c));
                    tables.AddRange(ListTables(c, log));
                    docs.AddRange(ReadMemberDocument(c, log));
                    MatchHints(docs, matches, log);
                    otherRows.AddRange(PeekOtherTables(c, tables, log));
                }
            }
            catch (Exception ex)
            {
                log("Doc         : " + FirstLine(ex.Message));
            }

            log("Doc         : tables=" + tables.Count + " MemberDocument=" + docs.Count + " matches=" + matches.Count + " otherPeek=" + otherRows.Count);
            return Pack(tables, docs, matches, otherRows);
        }

        public static void Probe(string ruleEngine, Action<string> log)
        {
            if (log == null) log = m => { };
            if (string.IsNullOrWhiteSpace(ruleEngine))
            {
                log("Doc         : RuleEngine خالی — Probe نشد");
                return;
            }
            string cs = ZabetehCase.WithCatalog(ruleEngine, Catalog);
            try
            {
                using (var c = new SqlConnection(cs))
                {
                    c.Open();
                    log("Doc         : [" + Catalog + "] OK db=" + c.Database + " login=" + CurrentLogin(c));
                    var tables = ListTables(c, log);
                    log("Doc         : " + tables.Count + " جدول dbo در " + Catalog);
                }
            }
            catch (Exception ex)
            {
                log("Doc         : Probe — " + FirstLine(ex.Message));
            }
        }

        public static void FlattenInto(List<Dictionary<string, object>> vars, Dictionary<string, object> pack)
        {
            if (vars == null || pack == null) return;
            object raw;
            if (pack.TryGetValue("docs", out raw))
            {
                var docs = raw as List<Dictionary<string, object>>;
                if (docs != null)
                {
                    foreach (var d in docs.Take(80))
                    {
                        vars.Add(new Dictionary<string, object>
                        {
                            { "name", "MemberDocument/" + Str(d, "nidMember") + "/" + Str(d, "docId") },
                            { "value", Trunc(Str(d, "title") + " — " + Str(d, "excerpt"), 220) },
                            { "table", "[" + Catalog + "].[dbo].[MemberDocument]" },
                            { "match", "مستند " + Str(d, "userName") },
                        });
                    }
                }
            }
            if (pack.TryGetValue("tables", out raw))
            {
                var tables = raw as List<Dictionary<string, object>>;
                if (tables != null)
                {
                    foreach (var t in tables)
                    {
                        vars.Add(new Dictionary<string, object>
                        {
                            { "name", "DocTable/" + Str(t, "name") },
                            { "value", Str(t, "cols") },
                            { "table", "[" + Catalog + "].[dbo].[" + Str(t, "name") + "]" },
                            { "match", "جدول مستند" },
                        });
                    }
                }
            }
            if (pack.TryGetValue("otherRows", out raw))
            {
                var other = raw as List<Dictionary<string, object>>;
                if (other != null)
                {
                    foreach (var o in other.Take(80))
                    {
                        vars.Add(new Dictionary<string, object>
                        {
                            { "name", Str(o, "name") },
                            { "value", Str(o, "value") },
                            { "table", "[" + Catalog + "].[dbo].[" + Str(o, "table") + "]" },
                            { "match", "جدول دیگر مستند" },
                        });
                    }
                }
            }
            if (pack.TryGetValue("matches", out raw))
            {
                var matches = raw as List<Dictionary<string, object>>;
                if (matches != null)
                {
                    foreach (var m in matches.Take(40))
                    {
                        vars.Add(new Dictionary<string, object>
                        {
                            { "name", "DocMatch/" + Str(m, "hint") },
                            { "value", Trunc(Str(m, "title") + " — " + Str(m, "excerpt"), 220) },
                            { "table", "[" + Catalog + "].[dbo].[MemberDocument]" },
                            { "match", "مستند جدول " + Str(m, "hint") },
                        });
                    }
                }
            }
        }

        private static Dictionary<string, object> Pack(
            List<Dictionary<string, object>> tables,
            List<Dictionary<string, object>> docs,
            List<Dictionary<string, object>> matches,
            List<Dictionary<string, object>> otherRows)
        {
            return new Dictionary<string, object>
            {
                { "catalog", Catalog },
                { "mainTable", MainTable },
                { "tables", tables },
                { "docs", docs },
                { "matches", matches },
                { "otherRows", otherRows },
            };
        }

        private static List<Dictionary<string, object>> ListTables(SqlConnection c, Action<string> log)
        {
            var list = new List<Dictionary<string, object>>();
            using (var cmd = new SqlCommand(@"
SELECT t.TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES t
WHERE t.TABLE_SCHEMA = 'dbo' AND t.TABLE_TYPE = 'BASE TABLE'
ORDER BY t.TABLE_NAME", c) { CommandTimeout = 30 })
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    string name = Convert.ToString(r.GetValue(0));
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    list.Add(new Dictionary<string, object> { { "name", name }, { "cols", "" } });
                }
            }
            foreach (var t in list)
            {
                string name = Convert.ToString(t["name"]);
                t["cols"] = string.Join(", ", TableColumns(c, name).Keys.Take(24));
                log("Detail      : جدول [" + name + "] cols=" + Convert.ToString(t["cols"]));
            }
            return list;
        }

        private static List<Dictionary<string, object>> ReadMemberDocument(SqlConnection c, Action<string> log)
        {
            var docs = new List<Dictionary<string, object>>();
            var cols = TableColumns(c, MainTable);
            foreach (string need in UserColumns)
            {
                if (!cols.ContainsKey(need))
                    log("Doc         : WARN ستون " + need + " در MemberDocument نیست");
            }
            string bodyType;
            cols.TryGetValue("MemberDocument", out bodyType);
            string bodyExpr = BodyExpr("MemberDocument", bodyType);
            string sql = CatalogSql(bodyExpr);
            try
            {
                using (var cmd = new SqlCommand(sql, c) { CommandTimeout = 45 })
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var row = new Dictionary<string, object>
                        {
                            { "docId", Cell(r, "DocId") },
                            { "title", Cell(r, "Title") },
                            { "nidMember", Cell(r, "NidMember") },
                            { "lastEditOn", Cell(r, "LastEditOn") },
                            { "sort", Cell(r, "Sort") },
                            { "parentDocId", Cell(r, "ParentDocId") },
                            { "userName", Cell(r, "UserName") },
                            { "bytes", Cell(r, "DocBytes") },
                            { "excerpt", Trunc(Cell(r, "MemberDocument"), 500) },
                        };
                        docs.Add(row);
                    }
                }
                log("Doc         : MemberDocument TOP 1000 → " + docs.Count + " ردیف (بدون فیلتر NidMember)");
            }
            catch (Exception ex)
            {
                log("Doc         : MemberDocument query — " + FirstLine(ex.Message));
            }
            return docs;
        }

        private static void MatchHints(List<Dictionary<string, object>> docs, List<Dictionary<string, object>> matches, Action<string> log)
        {
            foreach (var d in docs)
            {
                string blob = (Str(d, "title") + " " + Str(d, "excerpt")).ToLowerInvariant();
                foreach (string hint in TableHints)
                {
                    if (string.IsNullOrEmpty(hint)) continue;
                    if (blob.IndexOf(hint.ToLowerInvariant(), StringComparison.Ordinal) < 0) continue;
                    matches.Add(new Dictionary<string, object>
                    {
                        { "hint", hint },
                        { "docId", Str(d, "docId") },
                        { "nidMember", Str(d, "nidMember") },
                        { "title", Str(d, "title") },
                        { "excerpt", Trunc(Str(d, "excerpt"), 180) },
                    });
                    break;
                }
            }
            var byHint = matches.GroupBy(m => Str(m, "hint")).Select(g => g.Key + "=" + g.Count());
            log("Doc         : جدول‌های اشاره‌شده در مستند: " + string.Join(", ", byHint));
        }

        internal static bool SkipPeek(string name)
        {
            if (string.IsNullOrEmpty(name)) return true;
            if (name.Equals(MainTable, StringComparison.OrdinalIgnoreCase)) return true;
            if (name.Equals("sysdiagrams", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.StartsWith("AspNet", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.StartsWith("__EF", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.Equals("Users", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static List<Dictionary<string, object>> PeekOtherTables(SqlConnection c, List<Dictionary<string, object>> tables, Action<string> log)
        {
            var rows = new List<Dictionary<string, object>>();
            int peeked = 0;
            foreach (var t in tables)
            {
                string name = Str(t, "name");
                if (SkipPeek(name)) continue;
                if (peeked >= 20) break;
                peeked++;
                var cols = TableColumns(c, name);
                if (cols.Count == 0) continue;
                bool looksDoc = cols.ContainsKey("Title") || cols.ContainsKey("MemberDocument")
                    || cols.ContainsKey("Document") || cols.ContainsKey("Body") || cols.ContainsKey("DocText");
                var select = new List<string>();
                foreach (var kv in cols)
                {
                    if (SkipTypes.Contains((kv.Value ?? "").ToLowerInvariant())) continue;
                    select.Add("[" + kv.Key.Replace("]", "]]") + "]");
                    if (select.Count >= 20) break;
                }
                if (select.Count == 0) continue;
                int top = looksDoc ? 30 : 3;
                string sql = "SELECT TOP (" + top + ") " + string.Join(", ", select) + " FROM [dbo].[" + name.Replace("]", "") + "]";
                try
                {
                    using (var cmd = new SqlCommand(sql, c) { CommandTimeout = 30 })
                    using (var r = cmd.ExecuteReader())
                    {
                        int n = 0;
                        while (r.Read())
                        {
                            n++;
                            for (int i = 0; i < r.FieldCount; i++)
                            {
                                if (r.IsDBNull(i)) continue;
                                string val = Trunc(Convert.ToString(r.GetValue(i), CultureInfo.InvariantCulture), 160);
                                if (string.IsNullOrWhiteSpace(val)) continue;
                                rows.Add(new Dictionary<string, object>
                                {
                                    { "table", name },
                                    { "name", r.GetName(i) },
                                    { "value", val },
                                });
                            }
                        }
                        log("Detail      : [" + name + "] peek rows=" + n + (looksDoc ? " (مستندگونه)" : ""));
                    }
                }
                catch (Exception ex)
                {
                    log("Detail      : [" + name + "] " + FirstLine(ex.Message));
                }
            }
            return rows;
        }

        private static Dictionary<string, string> TableColumns(SqlConnection c, string table)
        {
            var cols = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new SqlCommand(
                "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=@t ORDER BY ORDINAL_POSITION", c))
            {
                cmd.Parameters.AddWithValue("@t", table);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        cols[r.GetString(0)] = r.GetString(1);
                }
            }
            return cols;
        }

        internal static string BodyExpr(string col, string dataType)
        {
            string t = (dataType ?? "").ToLowerInvariant();
            string wrapped = "[" + (col ?? "MemberDocument").Replace("]", "") + "]";
            if (t == "image" || t == "varbinary" || t == "binary")
                return "LEFT(CONVERT(NVARCHAR(MAX), CONVERT(VARBINARY(MAX), " + wrapped + ")), 2000)";
            return "LEFT(CONVERT(NVARCHAR(MAX), " + wrapped + "), 2000)";
        }

        private static string CurrentLogin(SqlConnection c)
        {
            try
            {
                using (var cmd = new SqlCommand("SELECT SUSER_SNAME()", c))
                {
                    object o = cmd.ExecuteScalar();
                    return o == null || o == DBNull.Value ? "" : Convert.ToString(o);
                }
            }
            catch { return ""; }
        }

        private static string Cell(IDataRecord r, string name)
        {
            try
            {
                object o = r[name];
                if (o == null || o == DBNull.Value) return "";
                return Convert.ToString(o, CultureInfo.InvariantCulture) ?? "";
            }
            catch { return ""; }
        }

        private static string Str(Dictionary<string, object> d, string key)
        {
            object o;
            if (d == null || !d.TryGetValue(key, out o) || o == null) return "";
            return Convert.ToString(o) ?? "";
        }

        private static string Trunc(string s, int n)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Replace("\r", " ").Replace("\n", " ");
            return s.Length <= n ? s : s.Substring(0, n) + "…";
        }

        private static string FirstLine(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            int i = s.IndexOf('\n');
            return i < 0 ? s : s.Substring(0, i);
        }
    }
}
