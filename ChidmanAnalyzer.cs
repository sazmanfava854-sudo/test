using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RuleTrace
{
    /// <summary>Static + trace analysis for Solh chidman path (typically dbo.Member NidMember=1288).</summary>
    internal static class ChidmanAnalyzer
    {
        public const int DefaultChidmanMemberId = 1288;

        private static readonly string[] MethodHints =
        {
            "InsertChidman", "Chideman", "Chidman", "SetUsefulHeight", "insertRahPele",
            "insertFilterAsansor", "FnTafkikZamin", "FnArzZamin",
        };

        private static readonly string[] TraceKeyHints =
        {
            "chidman", "chid", "chandganeh", "چیدمان", "layout", "suggestion", "solh", "peace", "masir",
        };

        public static void Report(IList<MemberSource> sources, IList<TraceEvent> trace, int nidMember, Action<string> log)
        {
            Action<string> raw = log ?? (m => { });
            log = m => Prefix(raw, m);

            if (sources == null || sources.Count == 0)
            {
                log("Chidman      : (no Member sources — «بارگذاری کد از DB»)");
                return;
            }

            log("");
            log("=== تحلیل مسیر چیدمان (Member " + nidMember + ") ===");

            MemberSource focus = sources.FirstOrDefault(s => s.NidMember == nidMember);
            if (focus == null)
            {
                log("Member " + nidMember + " : NOT FOUND in dbo.Member for this formula");
                log("  Available NidMember: " + string.Join(", ", sources.Select(s => s.NidMember.ToString()).Take(25)));
                return;
            }

            log("Member " + nidMember + " : " + focus.Name + "  " + focus.Meta);

            var methods = ExtractMethodNames(focus.Code);
            log("  Sub/Function in this member: " + (methods.Count == 0 ? "(none parsed)" : string.Join(", ", methods.Take(12)) + (methods.Count > 12 ? " ..." : "")));

            var addErrors = FindAddErrorLines(focus);
            log("  AddError lines: " + addErrors.Count);
            foreach (var ae in addErrors.Take(15))
                log("    L" + ae.Line + " Key=" + ae.Key + "  " + Trunc(ae.Text, 100));
            if (addErrors.Count > 15) log("    ... (" + addErrors.Count + " total)");

            var chidmanAddErrors = addErrors.Where(a => MatchesHint(a.Key) || MatchesHint(a.Text)).ToList();
            if (chidmanAddErrors.Count == 0)
                log("  WARN         : no AddError with chidman/solh key in Member " + nidMember + " — اعلام چیدمان شاید در Member دیگر است");
            else
                log("  chidman AddError: " + chidmanAddErrors.Count + " line(s) in Member " + nidMember);

            ReportCallers(sources, focus, methods, log);
            ReportSolhGuards(focus, sources, log);
            ReportGuardsNearChidman(focus, log);
            ReportTrace(trace, sources, nidMember, log);
        }

        private static void ReportCallers(IList<MemberSource> sources, MemberSource focus, List<string> methods, Action<string> log)
        {
            var targets = new HashSet<string>(methods, StringComparer.OrdinalIgnoreCase);
            foreach (string h in MethodHints) targets.Add(h);

            var callers = new List<string>();
            foreach (MemberSource src in sources)
            {
                if (src == focus || string.IsNullOrWhiteSpace(src.Code)) continue;
                foreach (string name in targets)
                {
                    if (Regex.IsMatch(src.Code, @"\b" + Regex.Escape(name) + @"\s*\(", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                        callers.Add("Member " + src.NidMember + " " + src.Name + " calls " + name);
                }
            }

            log("");
            log("  Who calls Member " + focus.NidMember + " methods:");
            if (callers.Count == 0)
            {
                log("    (no call found in other Member XmlBody — ممکن است فقط موتور Sara این Member را جدا اجرا کند)");
                log("    اگر Run مستقیماً InsertChidman/Chideman را صدا نزند، چیدمان «اعمال» نمی‌شود.");
            }
            else
            {
                foreach (string c in callers.Distinct(StringComparer.OrdinalIgnoreCase).Take(20))
                    log("    " + c);
            }

            bool runCalls = sources.Any(s =>
                s.NidMember != focus.NidMember
                && Regex.IsMatch(s.Code ?? "", @"\bRun\s*\(", RegexOptions.IgnoreCase)
                && targets.Any(t => Regex.IsMatch(s.Code, @"\b" + Regex.Escape(t) + @"\s*\(", RegexOptions.IgnoreCase)));
            if (!runCalls)
            {
                var runMember = sources.FirstOrDefault(s => (s.Name ?? "").Equals("Run", StringComparison.OrdinalIgnoreCase)
                    || Regex.IsMatch(s.Code ?? "", @"\b(?:Public\s+)?Sub\s+Run\s*\(", RegexOptions.IgnoreCase));
                if (runMember != null)
                {
                    bool runHasChidman = MethodHints.Any(h =>
                        Regex.IsMatch(runMember.Code ?? "", @"\b" + Regex.Escape(h) + @"\s*\(", RegexOptions.IgnoreCase));
                    log("  Run member   : NidMember=" + runMember.NidMember + " " + runMember.Name
                        + (runHasChidman ? " — calls chidman helper(s)" : " — does NOT call InsertChidman/Chideman (مسیر چیدمان شاید قطع شده)"));
                }
            }
        }

        /// <summary>Lines where Solh/صلح/Peace may skip layout announcement — the original settlement-chidman bug.</summary>
        private static void ReportSolhGuards(MemberSource focus, IList<MemberSource> sources, Action<string> log)
        {
            log("");
            log("  Solh/صلح guards (may skip chidman announcement):");
            int shown = 0;
            shown += DumpSolhGuardLines(focus, "Member " + focus.NidMember, log, 18);
            var run = sources.FirstOrDefault(s => s != focus && (
                (s.Name ?? "").Equals("Run", StringComparison.OrdinalIgnoreCase)
                || Regex.IsMatch(s.Code ?? "", @"\b(?:Public\s+)?Sub\s+Run\s*\(", RegexOptions.IgnoreCase)));
            if (run != null)
                shown += DumpSolhGuardLines(run, "Run Member " + run.NidMember, log, 10);
            if (shown == 0)
                log("    (no If/Exit mentioning Solh/صلح/Peace/GetPeace — مسیر skip شاید با نام دیگر است)");
        }

        private static int DumpSolhGuardLines(MemberSource src, string label, Action<string> log, int max)
        {
            if (src == null || string.IsNullOrWhiteSpace(src.Code)) return 0;
            string[] lines = Normalize(src.Code).Split('\n');
            int shown = 0;
            bool header = false;
            for (int i = 0; i < lines.Length; i++)
            {
                string t = lines[i].Trim();
                if (t.Length == 0 || t.StartsWith("'")) continue;
                if (!LooksLikeSolhGuard(t)) continue;
                if (!header)
                {
                    log("    -- " + label + " --");
                    header = true;
                }
                int from = Math.Max(0, i);
                int to = Math.Min(lines.Length - 1, i + 3);
                for (int j = from; j <= to; j++)
                {
                    if (shown >= max)
                    {
                        log("    ... (truncated)");
                        return shown;
                    }
                    string mark = j == i ? ">>" : "  ";
                    log("    " + mark + " L" + (j + 1) + ": " + Trunc(lines[j].Trim(), 110));
                    shown++;
                }
            }
            return shown;
        }

        private static bool LooksLikeSolhGuard(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            bool isIf = line.StartsWith("If ", StringComparison.OrdinalIgnoreCase)
                || line.StartsWith("ElseIf ", StringComparison.OrdinalIgnoreCase)
                || line.IndexOf(" Exit ", StringComparison.OrdinalIgnoreCase) >= 0
                || line.StartsWith("Exit ", StringComparison.OrdinalIgnoreCase);
            if (!isIf) return false;
            string[] tokens = { "solh", "صلح", "peace", "getpeace", "chidman", "chideman", "chandganeh", "چیدمان", "masir", "مسیر" };
            string lower = line.ToLowerInvariant();
            foreach (string tok in tokens)
                if (lower.IndexOf(tok, StringComparison.Ordinal) >= 0) return true;
            return false;
        }

        private static void ReportGuardsNearChidman(MemberSource focus, Action<string> log)
        {
            log("");
            log("  If/Exit near chidman helpers (Member " + focus.NidMember + "):");
            string[] lines = Normalize(focus.Code).Split('\n');
            int shown = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string t = lines[i].Trim();
                if (t.Length == 0 || t.StartsWith("'")) continue;
                bool isChidmanLine = MethodHints.Any(h => t.IndexOf(h, StringComparison.OrdinalIgnoreCase) >= 0);
                bool isGuard = t.StartsWith("If ", StringComparison.OrdinalIgnoreCase)
                    || t.StartsWith("ElseIf ", StringComparison.OrdinalIgnoreCase)
                    || t.IndexOf(" Exit ", StringComparison.OrdinalIgnoreCase) >= 0
                    || t.StartsWith("Return", StringComparison.OrdinalIgnoreCase);
                if (!isChidmanLine && !isGuard) continue;

                int from = Math.Max(0, i - 2);
                int to = Math.Min(lines.Length - 1, i + 2);
                for (int j = from; j <= to; j++)
                {
                    if (shown++ >= 25) { log("    ... (truncated)"); return; }
                    string mark = j == i ? ">>" : "  ";
                    log("    " + mark + " L" + (j + 1) + ": " + Trunc(lines[j].Trim(), 110));
                }
            }
            if (shown == 0) log("    (no obvious If/Exit next to chidman calls — کل Member را در تب دیباگ ببینید)");
        }

        private static void ReportTrace(IList<TraceEvent> trace, IList<MemberSource> sources, int nidMember, Action<string> log)
        {
            log("");
            log("  BizErrors trace (chidman/solh related):");
            if (trace == null || trace.Count == 0)
            {
                log("    (no live trace — Instanc موتور Nothing است)");
                log("    عیب‌یابی بدون اجرا: If/Exit نزدیک InsertChidman را در همین Member ببینید.");
                log("    اجرای زنده وقتی ممکن است که UI سارا یک‌بار Solh را کامپایل کند و DLL در Cache باشد.");
                return;
            }

            var related = trace.Where(e => MatchesHint(e.Key) || MatchesHint(e.Title)).ToList();
            if (related.Count == 0)
            {
                log("    هیچ رویداد AddError مرتبط با چیدمان/صلح ثبت نشده.");
                log("    یعنی کد به خط AddError چیدمان در Member " + nidMember + " (یا جای دیگر) نرسیده است.");
            }
            else
            {
                foreach (TraceEvent e in related.Take(25))
                {
                    string loc = LocateKeyInMember(e.Key, sources, nidMember);
                    log("    [" + e.Action + "] " + e.Key + ": " + Trunc(e.Title, 80) + loc);
                }
            }

            var inMember = new List<TraceEvent>();
            foreach (TraceEvent e in trace)
            {
                if (KeyExistsInMember(e.Key, sources.First(s => s.NidMember == nidMember)))
                    inMember.Add(e);
            }
            if (sources.Any(s => s.NidMember == nidMember))
            {
                log("  Trace keys whose AddError text exists in Member " + nidMember + ": " + inMember.Count + "/" + trace.Count);
                if (inMember.Count == 0 && related.Count > 0)
                    log("  NOTE         : chidman-related trace از Member دیگر آمده — مسیر اجرا از 1288 عبور نکرده");
            }
        }

        private static void Prefix(Action<string> log, string m)
        {
            if (log == null) return;
            if (string.IsNullOrWhiteSpace(m))
            {
                log("Chidman      :");
                return;
            }
            string t = m.TrimStart();
            if (t.StartsWith("Chidman      :", StringComparison.OrdinalIgnoreCase)) log(t);
            else log("Chidman      : " + t);
        }

        private static string LocateKeyInMember(string key, IList<MemberSource> sources, int nidMember)
        {
            if (string.IsNullOrWhiteSpace(key)) return "";
            MemberSource m = sources.FirstOrDefault(s => s.NidMember == nidMember);
            if (m == null || !KeyExistsInMember(key, m)) return "";
            return "  (in Member " + nidMember + ")";
        }

        private static bool KeyExistsInMember(string key, MemberSource m)
        {
            if (m == null || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(m.Code)) return false;
            return Regex.IsMatch(m.Code, @"AddError\s*\(\s*""?" + Regex.Escape(key), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
                || m.Code.IndexOf("\"" + key + "\"", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool MatchesHint(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            string t = text.ToLowerInvariant();
            foreach (string h in TraceKeyHints)
                if (t.IndexOf(h, StringComparison.Ordinal) >= 0) return true;
            foreach (string h in MethodHints)
                if (t.IndexOf(h.ToLowerInvariant(), StringComparison.Ordinal) >= 0) return true;
            return false;
        }

        private sealed class AddErrLine
        {
            public int Line;
            public string Key;
            public string Text;
        }

        private static List<AddErrLine> FindAddErrorLines(MemberSource m)
        {
            var list = new List<AddErrLine>();
            if (m == null || string.IsNullOrWhiteSpace(m.Code)) return list;
            string[] lines = Normalize(m.Code).Split('\n');
            var rx = new Regex(@"AddError\s*\(\s*""([^""]+)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            for (int i = 0; i < lines.Length; i++)
            {
                Match match = rx.Match(lines[i]);
                if (!match.Success) continue;
                list.Add(new AddErrLine { Line = i + 1, Key = match.Groups[1].Value, Text = lines[i].Trim() });
            }
            return list;
        }

        private static List<string> ExtractMethodNames(string code)
        {
            var list = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(code)) return list.ToList();
            foreach (Match m in Regex.Matches(code, @"(?m)^\s*(?:Public|Private|Protected|Friend)?\s*(?:Sub|Function)\s+(\w+)", RegexOptions.IgnoreCase))
                list.Add(m.Groups[1].Value);
            return list.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static string Normalize(string s)
        {
            return (s ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n");
        }

        private static string Trunc(string s, int n)
        {
            if (s == null) return "";
            s = s.Replace("\r", " ").Replace("\n", " ");
            return s.Length <= n ? s : s.Substring(0, n) + "…";
        }
    }
}
