using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RuleTrace
{
    /// <summary>
    /// Read-only Solh case dump: Member 1296/1297 source + Sara ضابطه tables + MemberDocument.
    /// Does not write dbo.Member. Does not compile or run VB.
    /// </summary>
    internal static class SolhNidDebug
    {
        public const int SolhRunMember = 1296;
        public const int SolhInitMember = 1297;

        private static readonly string[] VbSkip =
        {
            "If", "Then", "Else", "End", "And", "Or", "Not", "Is", "Nothing", "True", "False",
            "Dim", "As", "New", "Me", "My", "To", "In", "For", "Each", "Next", "While", "Loop",
            "Select", "Case", "Try", "Catch", "Finally", "Return", "Exit", "Sub", "Function",
            "Public", "Private", "Info8", "BIZ", "SA", "SC", "EumErrorAction", "Stop", "warning",
            "AddError", "ToString", "tostring", "GetType", "Integer", "Double", "String", "Boolean",
            "Object", "Decimal", "Long", "Date", "Of", "List",
        };

        public static Dictionary<string, object> Run(string sara, string ruleEngine, string nidProc, IList<MemberSource> sources, Action<string> log)
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
            var maz = ExtractStop(init != null ? init.Code : null, "عرض معبر");

            var names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            CollectDimNames(run != null ? run.Code : null, names);
            CollectDimNames(init != null ? init.Code : null, names);
            CollectNames(Head(run != null ? run.Code : null, 250), names);
            CollectNames(Head(init != null ? init.Code : null, 120), names);
            if (stop != null) CollectNames(stop.Block, names);
            if (maz != null) CollectNames(maz.Block, names);
            log("SolhNid      : شناسه‌های VB در Run/meghdardehi=" + names.Count);

            var vars = ZabetehCase.Read(sara, ruleEngine, nidProc, log);
            var docs = RuleDocs.Read(ruleEngine, log);
            RuleDocs.FlattenInto(vars, docs);

            if (PermitPipeline.IsIgnoredWorkItem(PermitPipeline.FindWorkItem(vars)))
            {
                string skip = PermitPipeline.Report(sources, vars, log);
                log("SolhNid      : بخش مشکوک: " + skip);
                return Pack(nidProc, skip, stop, maz, vars, names, docs);
            }

            LogSourceGuards(stop, maz, vars, log);
            string permit = PermitPipeline.Report(sources, vars, log);
            Diagnose(stop, maz, vars, log);
            log("SolhNid      : بخش مشکوک: " + permit);
            return Pack(nidProc, permit, stop, maz, vars, names, docs);
        }

        private static Dictionary<string, object> Pack(string nidProc, string diagnosis, StopHit stop, StopHit maz, List<Dictionary<string, object>> vars, SortedSet<string> names, Dictionary<string, object> docs)
        {
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
                { "docs", docs.ContainsKey("docs") ? docs["docs"] : new List<Dictionary<string, object>>() },
                { "docTables", docs.ContainsKey("tables") ? docs["tables"] : new List<Dictionary<string, object>>() },
                { "docMatches", docs.ContainsKey("matches") ? docs["matches"] : new List<Dictionary<string, object>>() },
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

        internal static string L270Headline(bool emptyActive, int line, string active)
        {
            if (emptyActive)
                return "توقف زنده Member 1296 L" + line + " — ActiveNidZabeteh خالی";
            return "L" + line + " سورس است نه توقف این Nid — ActiveNidZabeteh=" + Trunc(active ?? "", 36);
        }

        private static void LogSourceGuards(StopHit stop, StopHit maz, IList<Dictionary<string, object>> vars, Action<string> log)
        {
            string active = ReadActive(vars);
            bool emptyActive = ZabetehCase.IsEmptyGuid(active);
            if (stop != null)
            {
                log("SolhNid      : " + L270Headline(emptyActive, stop.Line, active));
                if (emptyActive)
                {
                    log("SolhNid      : شرط:");
                    foreach (string line in stop.Block.Split('\n'))
                        log("SolhNid      :   " + line.TrimEnd());
                }
                else
                    log("SolhNid      : بلوک L270 در سورس ماند؛ Member 1296 را عوض نکنید");
            }
            else log("SolhNid      : بلوک Stop عدم‌اعلام در 1296 پیدا نشد");

            if (maz != null)
            {
                string mazVal = FindVal(vars, "MazArz") ?? FindVal(vars, "ArzMaabar");
                double mazN;
                bool wide = double.TryParse(mazVal, NumberStyles.Any, CultureInfo.InvariantCulture, out mazN) && mazN > 8;
                if (wide)
                    log("SolhNid      : توقف زنده عرض معبر Member 1297 L" + maz.Line + " MazArz=" + mazVal + " > 8");
                else
                    log("SolhNid      : سورس نامزد عرض معبر Member 1297 L" + maz.Line
                        + " — MazArz=" + (string.IsNullOrEmpty(mazVal) ? "در جداول نام‌دار نیست" : mazVal)
                        + " (بدون Instanc شلیک زنده معلوم نیست)");
            }
        }

        internal static string ReadActive(IList<Dictionary<string, object>> vars)
        {
            return FindValFromTable(vars, "Sh_RequestInfo", "ActiveNidZabeteh")
                ?? FindVal(vars, "ActiveNidZabeteh")
                ?? FindVal(vars, "NidActiveZabeteh");
        }

        private static string Diagnose(StopHit stop, StopHit maz, IList<Dictionary<string, object>> vars, Action<string> log)
        {
            string active = ReadActive(vars);
            bool emptyActive = ZabetehCase.IsEmptyGuid(active);
            bool hasZabetehRow = HasTable(vars, "[dbo].[Zabeteh]") || HasTable(vars, ".[Zabeteh]");
            bool hasStatic = HasTable(vars, "ZabeteStatic_Info") || HasTable(vars, "ZabeteStatic_Zabete");
            string nosazi = FindValFromTable(vars, "Sh_RequestInfo", "NidNosaziCode") ?? FindVal(vars, "NidNosaziCode");
            string overlay = FindValFromTable(vars, "[dbo].[Zabeteh]", "NidZabeteh");
            string mazVal = FindVal(vars, "MazArz") ?? FindVal(vars, "ArzMaabar");
            double mazN;
            bool wide = double.TryParse(mazVal, NumberStyles.Any, CultureInfo.InvariantCulture, out mazN) && mazN > 8;

            if (emptyActive)
            {
                log("Zabeteh     : ActiveNidZabeteh خالی است — همان شرط L270");
                if (hasZabetehRow)
                    return "ضابطه ملک هست (Zabeteh با NidNosaziCode). Active خالی یعنی صلح روی این درخواست اعمال نشده — همه ملک‌ها صلح ندارند (Member 1296 L" + (stop == null ? 270 : stop.Line) + ").";
                if (hasStatic)
                    return "ActiveNidZabeteh خالی است؛ ضابطه ایستا (ZabeteStatic_*) برای P_Key هست ولی به درخواست وصل نشده. صلح L270 درست می‌ایستد.";
                return "ActiveNidZabeteh=Guid.Empty — این درخواست ضابطه اعلام‌شده ندارد. join روی " + ZabetehCase.JoinOn + " است نه NidProc. Member 1296 L" + (stop == null ? 270 : stop.Line) + " را عوض نکنید.";
            }

            string crud = "CRUD Read OK: ActiveNidZabeteh=" + Trunc(active, 36)
                + " NidNosaziCode=" + Trunc(nosazi, 36)
                + " join " + ZabetehCase.JoinOn;
            if (!hasZabetehRow)
                return crud + " — کلید اعلام پر است ولی ردیف Zabeteh خوانده نشد.";
            if (!ZabetehCase.IsEmptyGuid(overlay) && !string.Equals(overlay, active, StringComparison.OrdinalIgnoreCase))
                log("Zabeteh     : NidZabeteh آخرین ردیف=" + overlay + " ≠ Active");
            if (wide && maz != null)
                return crud + ". L270 شلیک نمی‌شود. توقف بعدی: عرض معبر Member 1297 L" + maz.Line + " MazArz=" + mazVal + " > 8";
            return crud + ". L270 شلیک نمی‌شود."
                + (overlay == null ? "" : " overlay=" + Trunc(overlay, 36))
                + " نامزد بعدی سورس: عرض معبر 1297" + (maz == null ? "" : " L" + maz.Line)
                + " و طرح جنوب غرب. بدون Instanc شلیک زنده معلوم نیست.";
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

        private static string FindValFromTable(IList<Dictionary<string, object>> vars, string tableHint, string name)
        {
            if (vars == null) return null;
            foreach (var v in vars)
            {
                object t, n, val;
                if (!v.TryGetValue("table", out t) || !v.TryGetValue("name", out n) || !v.TryGetValue("value", out val)) continue;
                if (Convert.ToString(t).IndexOf(tableHint, StringComparison.OrdinalIgnoreCase) < 0) continue;
                if (!string.Equals(Convert.ToString(n), name, StringComparison.OrdinalIgnoreCase)) continue;
                string s = Convert.ToString(val);
                if (!string.IsNullOrWhiteSpace(s)) return s;
            }
            return null;
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
    }
}
