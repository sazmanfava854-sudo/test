using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RuleTrace
{
    /// <summary>
    /// Read-only Solh case dump for one NidProc: Member 1296/1297 source + Sara row values.
    /// Does not write dbo.Member. Does not compile or run VB.
    /// </summary>
    internal static class SolhNidDebug
    {
        public const int SolhRunMember = 1296;
        public const int SolhInitMember = 1297;

        private static readonly string[] TableHints =
        {
            "Request", "Peace", "Solh", "Zabete", "Zabeteh", "Chid", "Nosazi",
            "Edge", "Front", "Melk", "Parcel", "Property", "RuleResult", "Using",
        };

        private static readonly string[] SkipTypes = { "image", "varbinary", "binary", "timestamp", "rowversion" };

        private static readonly string[] VbSkip =
        {
            "If", "Then", "Else", "End", "And", "Or", "Not", "Is", "Nothing", "True", "False",
            "Dim", "As", "New", "Me", "My", "To", "In", "For", "Each", "Next", "While", "Loop",
            "Select", "Case", "Try", "Catch", "Finally", "Return", "Exit", "Sub", "Function",
            "Public", "Private", "Info8", "BIZ", "SA", "SC", "EumErrorAction", "Stop", "warning",
            "AddError", "ToString", "tostring", "GetType", "Integer", "Double", "String", "Boolean",
            "Object", "Decimal", "Long", "Date", "Of", "List",
        };

        public static Dictionary<string, object> Run(string sara, string nidProc, IList<MemberSource> sources, Action<string> log)
        {
            if (log == null) log = m => { };
            nidProc = (nidProc ?? "").Trim();
            log("SolhNid      : CRUD=Read — dbo.Member نوشته نمی‌شود، VB اجرا/کامپایل نمی‌شود");
            log("SolhNid      : NidProc=" + (nidProc.Length == 0 ? "(خالی — اول جستجو کنید)" : nidProc));

            var members = (sources ?? new List<MemberSource>())
                .Where(s => s != null && (s.NidClass == 344 || s.NidClass == 342 || s.NidClass == 345 || s.NidMember == SolhRunMember || s.NidMember == SolhInitMember))
                .ToList();
            log("SolhNid      : Solh-related members loaded=" + members.Count);

            MemberSource run = FirstMember(sources, 344, SolhRunMember) ?? FirstMember(sources, 0, SolhRunMember);
            MemberSource init = FirstMember(sources, 344, SolhInitMember) ?? FirstMember(sources, 0, SolhInitMember);
            if (run != null)
                log("SolhNid      : Member " + run.NidMember + " " + run.Name + " codeLen=" + (run.Code == null ? 0 : run.Code.Length));
            else
                log("SolhNid      : WARN Member 1296 Run پیدا نشد");
            if (init != null)
                log("SolhNid      : Member " + init.NidMember + " " + init.Name + " codeLen=" + (init.Code == null ? 0 : init.Code.Length));

            var stop = ExtractStop(run != null ? run.Code : null, "عدم اعلام ضابطه");
            if (stop == null)
                stop = ExtractStop(run != null ? run.Code : null, "صلحنامه");
            if (stop != null)
            {
                log("SolhNid      : توقف در Member " + (run == null ? SolhRunMember : run.NidMember)
                    + " L" + stop.Line + " Key=" + stop.Key);
                log("SolhNid      : شرط:");
                foreach (string line in stop.Block.Split('\n'))
                    log("SolhNid      :   " + line.TrimEnd());
            }
            else log("SolhNid      : بلوک Stop عدم‌اعلام در 1296 پیدا نشد");

            var maz = ExtractStop(init != null ? init.Code : null, "عرض معبر");
            if (maz != null)
                log("SolhNid      : توقف عرض معبر Member 1297 L" + maz.Line + "  " + Trunc(maz.Text, 90));

            var names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            CollectDimNames(run != null ? run.Code : null, names);
            CollectDimNames(init != null ? init.Code : null, names);
            CollectNames(Head(run != null ? run.Code : null, 250), names);
            CollectNames(Head(init != null ? init.Code : null, 120), names);
            if (stop != null) CollectNames(stop.Block, names);
            if (maz != null) CollectNames(maz.Block, names);
            log("SolhNid      : شناسه‌های VB در Run/meghdardehi=" + names.Count);

            var vars = new List<Dictionary<string, object>>();
            if (nidProc.Length == 0)
            {
                log("Vars         : NidProc خالی است — مقدار پرونده خوانده نشد");
            }
            else
            {
                vars = ReadSaraVars(sara, nidProc, names, log);
            }

            string diagnosis = Diagnose(stop, maz, vars, log);
            log("SolhNid      : بخش مشکوک: " + diagnosis);

            return new Dictionary<string, object>
            {
                { "nidProc", nidProc },
                { "diagnosis", diagnosis },
                { "stopLine", stop == null ? 0 : stop.Line },
                { "stopKey", stop == null ? "" : stop.Key },
                { "stopBlock", stop == null ? "" : stop.Block },
                { "mazArzLine", maz == null ? 0 : maz.Line },
                { "vars", vars },
                { "vbNames", names.Take(80).ToList() },
            };
        }

        internal static StopHit ExtractStop(string code, string needle)
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(needle)) return null;
            string[] lines = code.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            int hit = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                string t = lines[i].Trim();
                if (t.StartsWith("'")) continue;
                if (t.IndexOf(needle, StringComparison.OrdinalIgnoreCase) < 0) continue;
                if (t.IndexOf("AddError", StringComparison.OrdinalIgnoreCase) < 0 && needle != "عرض معبر") continue;
                hit = i;
                break;
            }
            if (hit < 0) return null;
            int from = hit;
            for (int i = hit; i >= 0 && i >= hit - 25; i--)
            {
                string t = lines[i].Trim();
                if (Regex.IsMatch(t, @"^\s*If\b", RegexOptions.IgnoreCase)) { from = i; break; }
            }
            int to = hit;
            for (int i = hit; i < lines.Length && i <= hit + 8; i++)
            {
                to = i;
                if (Regex.IsMatch(lines[i].Trim(), @"^End\s+If\b", RegexOptions.IgnoreCase)) break;
            }
            var sb = new StringBuilder();
            for (int i = from; i <= to; i++) sb.AppendLine((i + 1).ToString().PadLeft(5) + "| " + lines[i]);
            return new StopHit
            {
                Line = hit + 1,
                Text = lines[hit].Trim(),
                Key = ErrorKey(lines[hit]),
                Block = sb.ToString().TrimEnd(),
            };
        }

        internal static void CollectDimNames(string code, ISet<string> names)
        {
            if (string.IsNullOrWhiteSpace(code) || names == null) return;
            foreach (Match m in Regex.Matches(code, @"(?:^|\n)\s*(?:Dim|Private|Public)\s+(?!Sub\b|Function\b|Property\b|Class\b)([A-Za-z_][A-Za-z0-9_]*)", RegexOptions.IgnoreCase))
                names.Add(m.Groups[1].Value);
        }

        private static string Head(string code, int lineCount)
        {
            if (string.IsNullOrEmpty(code) || lineCount <= 0) return "";
            string[] lines = code.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            int n = Math.Min(lineCount, lines.Length);
            return string.Join("\n", lines, 0, n);
        }

        internal static void CollectNames(string code, ISet<string> names)
        {
            if (string.IsNullOrWhiteSpace(code) || names == null) return;
            foreach (Match m in Regex.Matches(code, @"\b[A-Za-z_][A-Za-z0-9_]*\b"))
            {
                string n = m.Value;
                if (n.Length < 2 || n.Length > 48) continue;
                if (VbSkip.Any(s => s.Equals(n, StringComparison.OrdinalIgnoreCase))) continue;
                if (n.StartsWith("L", StringComparison.Ordinal) && n.Length <= 3) continue;
                names.Add(n);
            }
        }

        internal sealed class StopHit
        {
            public int Line;
            public string Text;
            public string Key;
            public string Block;
        }

        private static MemberSource FirstMember(IList<MemberSource> sources, int nidClass, int nidMember)
        {
            if (sources == null) return null;
            foreach (MemberSource s in sources)
            {
                if (s == null || s.NidMember != nidMember) continue;
                if (nidClass == 0 || s.NidClass == nidClass) return s;
            }
            return null;
        }

        private static string ErrorKey(string line)
        {
            var m = Regex.Match(line ?? "", "AddError\\([^,]+,\\s*\"([^\"]+)\"", RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value : "";
        }

        private static List<Dictionary<string, object>> ReadSaraVars(string sara, string nidProc, ISet<string> vbNames, Action<string> log)
        {
            var vars = new List<Dictionary<string, object>>();
            if (string.IsNullOrWhiteSpace(sara))
            {
                log("Vars         : اتصال Sara خالی است");
                return vars;
            }

            var tables = DiscoverTables(sara, log);
            int tableHits = 0;
            foreach (string table in tables)
            {
                if (tableHits >= 12) break;
                try
                {
                    int n = DumpTable(sara, table, nidProc, vbNames, vars, log);
                    if (n > 0) tableHits++;
                }
                catch (Exception ex)
                {
                    log("Vars         : " + table + " skip — " + FirstLine(ex.Message));
                }
            }
            log("Vars         : " + vars.Count + " مقدار از " + tableHits + " جدول برای NidProc");
            return vars;
        }

        private static List<string> DiscoverTables(string cs, Action<string> log)
        {
            var list = new List<string>();
            const string sql = @"
SELECT DISTINCT c.TABLE_SCHEMA, c.TABLE_NAME
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.COLUMN_NAME IN ('NidProc','NidWorkItem')
ORDER BY c.TABLE_NAME";
            try
            {
                using (var c = new SqlConnection(cs))
                using (var cmd = new SqlCommand(sql, c) { CommandTimeout = 30 })
                {
                    c.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            string schema = Convert.ToString(r.GetValue(0));
                            string name = Convert.ToString(r.GetValue(1));
                            if (string.IsNullOrWhiteSpace(name)) continue;
                            int score = TableScore(name);
                            if (score <= 0) continue;
                            list.Add("[" + schema + "].[" + name + "]|" + score);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log("Vars         : discover failed — " + FirstLine(ex.Message));
                return PreferKnown();
            }
            var ordered = list
                .Select(s => s.Split('|'))
                .OrderByDescending(p => int.Parse(p[1], CultureInfo.InvariantCulture))
                .Select(p => p[0])
                .Distinct()
                .Take(18)
                .ToList();
            if (ordered.Count == 0) ordered = PreferKnown();
            log("Vars         : جداول کاندید NidProc=" + ordered.Count);
            return ordered;
        }

        private static List<string> PreferKnown()
        {
            return new List<string>
            {
                "[dbo].[Sh_RequestInfo]",
                "[dbo].[Sh_Request]",
                "[dbo].[Base_NosaziCode]",
            };
        }

        private static int TableScore(string name)
        {
            if (string.IsNullOrEmpty(name)) return 0;
            int s = 0;
            foreach (string h in TableHints)
                if (name.IndexOf(h, StringComparison.OrdinalIgnoreCase) >= 0) s += 3;
            if (name.IndexOf("Request", StringComparison.OrdinalIgnoreCase) >= 0) s += 4;
            if (name.IndexOf("Peace", StringComparison.OrdinalIgnoreCase) >= 0) s += 8;
            if (name.IndexOf("Solh", StringComparison.OrdinalIgnoreCase) >= 0) s += 8;
            if (name.IndexOf("Zabete", StringComparison.OrdinalIgnoreCase) >= 0) s += 7;
            if (name.IndexOf("Chid", StringComparison.OrdinalIgnoreCase) >= 0) s += 7;
            if (name.StartsWith("sys", StringComparison.OrdinalIgnoreCase)) return 0;
            if (name.IndexOf("Log", StringComparison.OrdinalIgnoreCase) >= 0) s -= 2;
            return s;
        }

        private static int DumpTable(string cs, string table, string nidText, ISet<string> vbNames, List<Dictionary<string, object>> vars, Action<string> log)
        {
            HashSet<string> cols;
            Dictionary<string, string> types;
            ColumnMeta(cs, table, out cols, out types);
            string nidCol = cols.Contains("NidProc") ? "NidProc"
                : cols.Contains("NidWorkItem") ? "NidWorkItem"
                : null;
            if (nidCol == null) return 0;

            var select = new List<string>();
            foreach (string col in cols)
            {
                string t;
                types.TryGetValue(col, out t);
                if (SkipTypes.Contains((t ?? "").ToLowerInvariant())) continue;
                select.Add("[" + col.Replace("]", "]]") + "]");
                if (select.Count >= 40) break;
            }
            if (select.Count == 0) return 0;

            string sql = "SELECT TOP 3 " + string.Join(", ", select) + " FROM " + table
                + " WHERE CAST([" + nidCol + "] AS NVARCHAR(50))=@t";
            int rows = 0;
            using (var c = new SqlConnection(cs))
            using (var cmd = new SqlCommand(sql, c) { CommandTimeout = 45 })
            {
                cmd.Parameters.AddWithValue("@t", nidText);
                c.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        rows++;
                        for (int i = 0; i < r.FieldCount; i++)
                        {
                            string col = r.GetName(i);
                            if (r.IsDBNull(i)) continue;
                            string val = Trunc(Convert.ToString(r.GetValue(i)), 80);
                            if (string.IsNullOrWhiteSpace(val)) continue;
                            bool interesting = vbNames != null && vbNames.Contains(col);
                            interesting = interesting || KeyColumn(col);
                            if (!interesting && vars.Count > 80) continue;
                            if (!interesting && !KeyColumn(col) && rows > 1) continue;
                            vars.Add(new Dictionary<string, object>
                            {
                                { "name", col },
                                { "value", val },
                                { "table", table },
                                { "match", interesting ? "VB/کلید" : "ستون" },
                            });
                        }
                    }
                }
            }
            if (rows > 0)
                log("Vars         : " + table + " rows=" + rows + " via " + nidCol);
            return rows;
        }

        private static bool KeyColumn(string col)
        {
            if (string.IsNullOrEmpty(col)) return false;
            string[] keys =
            {
                "NidProc", "NidWorkItem", "NidNosaziCode", "NosaziCode", "Masahat", "MazArz", "Arz",
                "Tarh", "Karbari", "PlanType", "UsingType", "UsingGroup", "UsingArea", "FrontArea",
                "Bar", "Requester", "Workflow", "RequestDate", "Peace", "Solh", "Zabete", "Chid",
                "M_Tarh", "M_Karbari", "FloorNo", "KolParking",
            };
            foreach (string k in keys)
                if (col.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        private static void ColumnMeta(string cs, string table, out HashSet<string> cols, out Dictionary<string, string> types)
        {
            cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            types = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string name = table.Replace("[", "").Replace("]", "");
            string schema = "dbo";
            string tbl = name;
            int dot = name.IndexOf('.');
            if (dot > 0) { schema = name.Substring(0, dot); tbl = name.Substring(dot + 1); }
            using (var c = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=@s AND TABLE_NAME=@t", c))
            {
                cmd.Parameters.AddWithValue("@s", schema);
                cmd.Parameters.AddWithValue("@t", tbl);
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

        private static string Diagnose(StopHit stop, StopHit maz, IList<Dictionary<string, object>> vars, Action<string> log)
        {
            bool hasZabete = HasTable(vars, "Zabete") || HasName(vars, "Zabete") || HasName(vars, "GetZabeteh");
            bool hasChid = HasTable(vars, "Chid") || HasName(vars, "Chidman") || HasName(vars, "InsertChidman");
            bool hasPeace = HasTable(vars, "Peace") || HasTable(vars, "Solh");
            string mazVal = FindVal(vars, "MazArz") ?? FindVal(vars, "ArzMaabar") ?? FindVal(vars, "Arz");
            double mazN;
            bool wide = double.TryParse(mazVal, NumberStyles.Any, CultureInfo.InvariantCulture, out mazN) && mazN > 8;

            if (stop != null && stop.Text.IndexOf("عدم اعلام", StringComparison.Ordinal) >= 0 && !hasZabete && !hasChid)
            {
                log("Vars         : ردیف ضابطه/چیدمان برای این Nid دیده نشد");
                return "اعلام ضابطه/چیدمان قبل از صلح (Member 1296 L" + stop.Line + " — عدم اعلام ضابطه). جدول Zabeteh/Chidman برای این NidProc خالی است یا خوانده نشد.";
            }
            if (wide && maz != null)
                return "عرض معبر (Member 1297 L" + maz.Line + ") MazArz=" + mazVal + " > 8";
            if (stop != null && !hasZabete)
                return "شرط Stop صلح در Member 1296 L" + stop.Line + " — مقدار GetZabeteh/ضابطه در DB پیدا نشد.";
            if (hasPeace)
                return "رکورد صلح/توافق برای این Nid هست؛ ادامه را در Member 1296 بعد از L270 ببینید.";
            if (stop != null)
                return "اولین Stop صلح: Member 1296 L" + stop.Line + " «" + stop.Key + "»";
            return "کد صلح خوانده شد؛ برای مقدار زنده NidProc را پر کنید و دوباره بزنید.";
        }

        private static bool HasTable(IList<Dictionary<string, object>> vars, string hint)
        {
            if (vars == null) return false;
            foreach (var v in vars)
            {
                object t;
                if (v.TryGetValue("table", out t) && Convert.ToString(t).IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private static bool HasName(IList<Dictionary<string, object>> vars, string hint)
        {
            if (vars == null) return false;
            foreach (var v in vars)
            {
                object n;
                if (v.TryGetValue("name", out n) && Convert.ToString(n).IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private static string FindVal(IList<Dictionary<string, object>> vars, string name)
        {
            if (vars == null) return null;
            foreach (var v in vars)
            {
                object n, val;
                if (!v.TryGetValue("name", out n) || !v.TryGetValue("value", out val)) continue;
                if (string.Equals(Convert.ToString(n), name, StringComparison.OrdinalIgnoreCase))
                    return Convert.ToString(val);
            }
            return null;
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
