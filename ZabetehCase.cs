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
    /// Read-only Sara ضابطه tables for one NidProc.
    /// Named tables only — no INFORMATION_SCHEMA hunt for random *Zabeteh* names.
    /// Overall docs live in RuleDocs (DbRuleEngeinDocument.MemberDocument).
    /// </summary>
    internal static class ZabetehCase
    {
        public const string DocumentCatalog = "DbRuleEngeinDocument";

        public static readonly string[] SaraTables =
        {
            "Sh_RequestInfo",
            "Zabeteh",
            "CI_PlanType",
            "CI_PlanUsingType",
            "CI_Zabeteh",
            "ZabeteStatic_Info",
            "ZabeteStatic_Zabete",
            "ZabeteStatic_Plan",
        };

        private static readonly string[] SkipTypes = { "image", "varbinary", "binary", "timestamp", "rowversion" };

        public static List<Dictionary<string, object>> Read(string sara, string ruleEngine, string nidProc, Action<string> log)
        {
            if (log == null) log = m => { };
            var vars = new List<Dictionary<string, object>>();
            if (string.IsNullOrWhiteSpace(nidProc))
            {
                log("Zabeteh     : NidProc خالی");
                return vars;
            }
            if (string.IsNullOrWhiteSpace(sara))
            {
                log("Zabeteh     : اتصال Sara خالی است");
                return vars;
            }

            log("Zabeteh     : Sara tables = " + string.Join(", ", SaraTables));
            var keys = new CaseKeys { NidProc = nidProc.Trim() };

            DumpNamed(sara, "Sh_RequestInfo", keys, vars, log, "NidProc");
            FillKeys(keys, vars);
            log("Zabeteh     : ActiveNidZabeteh=" + (string.IsNullOrEmpty(keys.ActiveNidZabeteh) ? "(خالی)" : keys.ActiveNidZabeteh)
                + " Pkey=" + (keys.Pkey ?? "(خالی)"));

            if (IsEmptyGuid(keys.ActiveNidZabeteh))
                log("Zabeteh     : ActiveNidZabeteh خالی/Guid.Empty — صلح L270 باید بایستد");

            DumpNamed(sara, "Zabeteh", keys, vars, log, "NidProc", "NidZabeteh", "ActiveNidZabeteh");
            FillKeys(keys, vars);

            LookupById(sara, "CI_PlanType", keys.PlanTypeId, vars, log);
            LookupById(sara, "CI_PlanUsingType", keys.PlanUsingTypeId, vars, log);
            LookupById(sara, "CI_Zabeteh", keys.CIZabetehId, vars, log);

            DumpNamed(sara, "ZabeteStatic_Info", keys, vars, log, "Pkey", "PKEY", "PKey");
            FillKeys(keys, vars);
            DumpNamed(sara, "ZabeteStatic_Zabete", keys, vars, log, "Pkey", "Nid", "Code", "NidZabeteStatic");
            DumpNamed(sara, "ZabeteStatic_Plan", keys, vars, log, "Pkey", "Nid", "Code", "CI_PlanType", "CI_PlanUsingType");

            int zabRows = CountTable(vars, "Zabeteh");
            int staticInfo = CountTable(vars, "ZabeteStatic_Info");
            log("Zabeteh     : rows Zabeteh=" + zabRows + " ZabeteStatic_Info=" + staticInfo);

            log("Zabeteh     : " + vars.Count + " مقدار از جداول ضابطه");
            return vars;
        }

        /// <summary>Named-table ping for «تست اتصال» — no schema hunt.</summary>
        public static void Probe(string sara, string ruleEngine, Action<string> log)
        {
            if (log == null) log = m => { };
            log("Zabeteh     : جداول نام‌دار Sara = " + string.Join(", ", SaraTables));
            if (string.IsNullOrWhiteSpace(sara))
            {
                log("Zabeteh     : اتصال Sara خالی است — Probe نشد");
            }
            else
            {
                foreach (string table in SaraTables)
                {
                    HashSet<string> cols;
                    Dictionary<string, string> types;
                    if (TryMeta(sara, table, out cols, out types))
                        log("Zabeteh     : [" + table + "] OK cols=" + cols.Count);
                    else
                        log("Zabeteh     : [" + table + "] نیست یا قابل خواندن نیست");
                }
            }

            RuleDocs.Probe(ruleEngine, log);
        }

        public static bool IsEmptyGuid(string v)
        {
            if (string.IsNullOrWhiteSpace(v)) return true;
            v = v.Trim().Trim('{', '}');
            if (v.Equals("Guid.Empty", StringComparison.OrdinalIgnoreCase)) return true;
            Guid g;
            if (Guid.TryParse(v, out g) && g == Guid.Empty) return true;
            return false;
        }

        internal static string WithCatalog(string cs, string catalog)
        {
            if (string.IsNullOrWhiteSpace(cs) || string.IsNullOrWhiteSpace(catalog)) return cs ?? "";
            try
            {
                var b = new SqlConnectionStringBuilder(cs);
                b.InitialCatalog = catalog;
                return b.ConnectionString;
            }
            catch
            {
                return cs;
            }
        }

        private sealed class CaseKeys
        {
            public string NidProc;
            public string ActiveNidZabeteh;
            public string Pkey;
            public string NidNosaziCode;
            public string PlanTypeId;
            public string PlanUsingTypeId;
            public string CIZabetehId;
        }

        private static void FillKeys(CaseKeys k, List<Dictionary<string, object>> vars)
        {
            if (IsEmptyGuid(k.ActiveNidZabeteh))
                k.ActiveNidZabeteh = First(vars, "ActiveNidZabeteh", "NidActiveZabeteh") ?? k.ActiveNidZabeteh;
            if (string.IsNullOrEmpty(k.Pkey))
                k.Pkey = First(vars, "Pkey", "PKEY", "PKey", "MelkPkey", "PKEY_Melk");
            if (string.IsNullOrEmpty(k.NidNosaziCode))
                k.NidNosaziCode = First(vars, "NidNosaziCode");
            if (string.IsNullOrEmpty(k.PlanTypeId))
                k.PlanTypeId = First(vars, "CI_PlanType", "NidPlanType", "PlanType", "PlanTypeId");
            if (string.IsNullOrEmpty(k.PlanUsingTypeId))
                k.PlanUsingTypeId = First(vars, "CI_PlanUsingType", "NidPlanUsingType", "PlanUsingType");
            if (string.IsNullOrEmpty(k.CIZabetehId))
                k.CIZabetehId = First(vars, "CI_Zabeteh", "NidCIZabeteh", "NidZabetehType");
            if (IsEmptyGuid(k.ActiveNidZabeteh))
            {
                string z = FirstFromTable(vars, "Zabeteh", "NidZabeteh", "Nid");
                if (!IsEmptyGuid(z)) k.ActiveNidZabeteh = z;
            }
        }

        private static void DumpNamed(string cs, string table, CaseKeys keys, List<Dictionary<string, object>> vars, Action<string> log, params string[] preferCols)
        {
            HashSet<string> cols;
            Dictionary<string, string> types;
            if (!TryMeta(cs, table, out cols, out types))
            {
                log("Zabeteh     : جدول " + table + " در Sara نیست یا قابل خواندن نیست");
                return;
            }

            string whereCol = null;
            string whereVal = null;
            foreach (string c in preferCols ?? new string[0])
            {
                if (!cols.Contains(c)) continue;
                string val = ValueFor(keys, c);
                if (string.IsNullOrEmpty(val) || (IsGuidCol(c) && IsEmptyGuid(val))) continue;
                whereCol = c;
                whereVal = val;
                break;
            }
            if (whereCol == null && cols.Contains("NidProc") && !string.IsNullOrEmpty(keys.NidProc))
            {
                whereCol = "NidProc";
                whereVal = keys.NidProc;
            }
            if (whereCol == null)
            {
                log("Zabeteh     : " + table + " — کلید اتصال برای این Nid پیدا نشد (ستون‌ها: " + string.Join(",", cols.Take(12)) + ")");
                return;
            }

            int n = SelectWhere(cs, table, cols, types, whereCol, whereVal, vars, log);
            log("Zabeteh     : [" + table + "] rows=" + n + " via " + whereCol);
        }

        private static string ValueFor(CaseKeys k, string col)
        {
            if (col.Equals("NidProc", StringComparison.OrdinalIgnoreCase)) return k.NidProc;
            if (col.IndexOf("ActiveNidZabeteh", StringComparison.OrdinalIgnoreCase) >= 0) return k.ActiveNidZabeteh;
            if (col.Equals("NidZabeteh", StringComparison.OrdinalIgnoreCase)) return k.ActiveNidZabeteh;
            if (col.IndexOf("Pkey", StringComparison.OrdinalIgnoreCase) >= 0 || col.Equals("PKEY", StringComparison.OrdinalIgnoreCase))
                return k.Pkey;
            if (col.Equals("NidNosaziCode", StringComparison.OrdinalIgnoreCase)) return k.NidNosaziCode;
            if (col.IndexOf("PlanUsing", StringComparison.OrdinalIgnoreCase) >= 0) return k.PlanUsingTypeId;
            if (col.IndexOf("PlanType", StringComparison.OrdinalIgnoreCase) >= 0) return k.PlanTypeId;
            if (col.IndexOf("CI_Zabeteh", StringComparison.OrdinalIgnoreCase) >= 0) return k.CIZabetehId;
            return null;
        }

        private static bool IsGuidCol(string col)
        {
            return col.IndexOf("Nid", StringComparison.OrdinalIgnoreCase) >= 0 || col.IndexOf("Guid", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void LookupById(string cs, string table, string id, List<Dictionary<string, object>> vars, Action<string> log)
        {
            if (string.IsNullOrWhiteSpace(id)) return;
            HashSet<string> cols;
            Dictionary<string, string> types;
            if (!TryMeta(cs, table, out cols, out types))
            {
                log("Zabeteh     : " + table + " نیست");
                return;
            }
            string pk = FirstExisting(cols, "ID", "Id", "Nid", "Code", "Nid" + table);
            if (pk == null)
            {
                log("Zabeteh     : " + table + " ستون ID ندارد");
                return;
            }
            int n = SelectWhere(cs, table, cols, types, pk, id, vars, log);
            log("Zabeteh     : [" + table + "] lookup " + pk + "=" + id + " rows=" + n);
        }

        private static int SelectWhere(string cs, string table, HashSet<string> cols, Dictionary<string, string> types, string whereCol, string whereVal, List<Dictionary<string, object>> vars, Action<string> log)
        {
            var select = new List<string>();
            foreach (string col in cols)
            {
                string t;
                types.TryGetValue(col, out t);
                if (SkipTypes.Contains((t ?? "").ToLowerInvariant())) continue;
                select.Add("[" + col.Replace("]", "]]") + "]");
                if (select.Count >= 50) break;
            }
            if (select.Count == 0) return 0;
            string sql = "SELECT TOP 5 " + string.Join(", ", select) + " FROM [dbo].[" + table.Replace("]", "") + "]"
                + " WHERE CAST([" + whereCol.Replace("]", "") + "] AS NVARCHAR(50))=@t";
            int rows = 0;
            try
            {
                using (var c = new SqlConnection(cs))
                using (var cmd = new SqlCommand(sql, c) { CommandTimeout = 45 })
                {
                    cmd.Parameters.AddWithValue("@t", whereVal ?? "");
                    c.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            rows++;
                            AddRow(r, "[dbo].[" + table + "]", vars);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log("Zabeteh     : " + table + " query — " + FirstLine(ex.Message));
            }
            return rows;
        }

        private static void AddRow(IDataRecord r, string table, List<Dictionary<string, object>> vars)
        {
            for (int i = 0; i < r.FieldCount; i++)
            {
                if (r.IsDBNull(i)) continue;
                string col = r.GetName(i);
                string val = Trunc(Convert.ToString(r.GetValue(i), CultureInfo.InvariantCulture), 120);
                if (string.IsNullOrWhiteSpace(val)) continue;
                vars.Add(new Dictionary<string, object>
                {
                    { "name", col },
                    { "value", val },
                    { "table", table },
                    { "match", Flag(col) },
                });
            }
        }

        private static string Flag(string col)
        {
            if (col.IndexOf("ActiveNidZabeteh", StringComparison.OrdinalIgnoreCase) >= 0) return "کلید صلح L270";
            if (col.IndexOf("Zabeteh", StringComparison.OrdinalIgnoreCase) >= 0) return "ضابطه";
            if (col.IndexOf("PlanType", StringComparison.OrdinalIgnoreCase) >= 0) return "طرح";
            if (col.IndexOf("PlanUsing", StringComparison.OrdinalIgnoreCase) >= 0 || col.IndexOf("Karbari", StringComparison.OrdinalIgnoreCase) >= 0) return "کاربری";
            if (col.IndexOf("Pkey", StringComparison.OrdinalIgnoreCase) >= 0) return "Pkey ملک";
            if (col.IndexOf("NidProc", StringComparison.OrdinalIgnoreCase) >= 0) return "VB/کلید";
            return "ستون";
        }

        private static bool TryMeta(string cs, string table, out HashSet<string> cols, out Dictionary<string, string> types)
        {
            cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            types = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (var c = new SqlConnection(cs))
                using (var cmd = new SqlCommand(
                    "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=@t", c))
                {
                    cmd.Parameters.AddWithValue("@t", table);
                    c.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            string col = r.GetString(0);
                            cols.Add(col);
                            types[col] = r.GetString(1);
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return cols.Count > 0;
        }

        private static string FirstExisting(HashSet<string> cols, params string[] names)
        {
            foreach (string n in names)
                if (n != null && cols.Contains(n)) return n;
            return null;
        }

        private static string First(List<Dictionary<string, object>> vars, params string[] names)
        {
            foreach (string name in names)
            {
                foreach (var v in vars)
                {
                    object n, val;
                    if (!v.TryGetValue("name", out n) || !v.TryGetValue("value", out val)) continue;
                    if (string.Equals(Convert.ToString(n), name, StringComparison.OrdinalIgnoreCase))
                    {
                        string s = Convert.ToString(val);
                        if (!string.IsNullOrWhiteSpace(s)) return s;
                    }
                }
            }
            return null;
        }

        private static string FirstFromTable(List<Dictionary<string, object>> vars, string tableHint, params string[] names)
        {
            foreach (var v in vars)
            {
                object t, n, val;
                if (!v.TryGetValue("table", out t) || !v.TryGetValue("name", out n) || !v.TryGetValue("value", out val)) continue;
                if (Convert.ToString(t).IndexOf(tableHint, StringComparison.OrdinalIgnoreCase) < 0) continue;
                foreach (string name in names)
                    if (string.Equals(Convert.ToString(n), name, StringComparison.OrdinalIgnoreCase))
                    {
                        string s = Convert.ToString(val);
                        if (!IsEmptyGuid(s)) return s;
                    }
            }
            return null;
        }

        private static int CountTable(List<Dictionary<string, object>> vars, string hint)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var v in vars)
            {
                object t;
                if (v.TryGetValue("table", out t) && Convert.ToString(t).IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0)
                    set.Add(Convert.ToString(t));
            }
            if (set.Count == 0) return 0;
            int n = 0;
            foreach (var v in vars)
            {
                object t, name;
                if (!v.TryGetValue("table", out t) || !v.TryGetValue("name", out name)) continue;
                if (Convert.ToString(t).IndexOf(hint, StringComparison.OrdinalIgnoreCase) < 0) continue;
                if (Convert.ToString(name).IndexOf("Nid", StringComparison.OrdinalIgnoreCase) >= 0) n++;
            }
            return n;
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
