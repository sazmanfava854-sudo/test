using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace RuleTrace
{
    /// <summary>
    /// Replaces the Logfilefj + Sara message-window workflow.
    /// logfilefj("name", expr) is Info8.AddError(Warning, name, value) gated by UserGuid.
    /// Hover maps those probes (and identifiers) to BizErrors / ParametersValue without opening Sara UI.
    /// </summary>
    internal sealed class HoverItem
    {
        public int Line;
        public string Name;
        public string Expr;
        public string Value;
        public string Source;
    }

    internal static class HoverDebug
    {
        public const string Goal =
            "به‌جای logfilefj و باز کردن فرم سارا، موس را روی هر خط نگه دارید تا مقدار متغیر مثل دیباگر دیده شود.";

        public const string NoInstance =
            "Instanc ساخته نشد — کش DLL این فرم خالی است یا CompilerErrors. یک‌بار همان فرم را در سارا باز کنید تا کامپایل شود، بعد اینجا اجرا کنید. ClearCache را تیک نزنید. RuleTrace VB را کامپایل نمی‌کند.";

        private static readonly Regex RxLog = new Regex(
            @"logfilefj\s*\(\s*(?:""([^""]*)""|'([^']*)')\s*(?:,\s*(.*?))?\s*\)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex RxAdd = new Regex(
            @"AddError\s*\(\s*[^,\n]+,\s*(?:""([^""]*)""|'([^']*)')\s*(?:,\s*(.*?))?\s*\)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex RxIdent = new Regex(
            @"[A-Za-z_][A-Za-z0-9_]*",
            RegexOptions.CultureInvariant);

        private static readonly HashSet<string> Skip = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Sub", "Function", "Dim", "As", "If", "Then", "Else", "ElseIf", "End", "For", "Next",
            "While", "Wend", "Do", "Loop", "Select", "Case", "To", "New", "True", "False", "Nothing",
            "Me", "My", "Not", "And", "Or", "Mod", "Integer", "Double", "String", "Boolean", "Object",
            "Public", "Private", "Protected", "Friend", "Shared", "ByVal", "ByRef", "Optional",
            "Return", "Exit", "Call", "Set", "Get", "Let", "With", "Is", "Like", "Xor", "AndAlso",
            "OrElse", "Try", "Catch", "Finally", "Throw", "Imports", "Class", "Module", "Const",
            "Info8", "AddError", "logfilefj", "Logfilefj", "logfileFJ", "BIZ", "SA", "EumErrorAction",
            "warning", "Warning", "Stop", "UCase", "ToString", "UserGuid", "User", "Val", "CInt",
            "CDbl", "CStr", "CBool", "CDate", "IIf", "IsNothing", "IsDBNull", "True", "False",
        };

        public static List<HoverItem> Parse(string code)
        {
            var list = new List<HoverItem>();
            if (string.IsNullOrEmpty(code)) return list;
            string[] lines = code.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string raw = lines[i] ?? "";
                string t = raw.Trim();
                if (t.Length == 0) continue;
                if (t.StartsWith("'")) continue;
                if (Regex.IsMatch(t, @"\bSub\s+Logfilefj\b", RegexOptions.IgnoreCase)) continue;

                foreach (Match m in RxLog.Matches(raw))
                {
                    string name = FirstGroup(m, 1, 2);
                    string expr = (m.Groups[3].Success ? m.Groups[3].Value : "").Trim();
                    expr = StripComment(expr);
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    list.Add(new HoverItem
                    {
                        Line = i + 1,
                        Name = name.Trim(),
                        Expr = expr,
                        Source = "logfilefj",
                    });
                }

                if (t.IndexOf("AddError", StringComparison.OrdinalIgnoreCase) >= 0
                    && t.IndexOf("logfilefj", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    foreach (Match m in RxAdd.Matches(raw))
                    {
                        string name = FirstGroup(m, 1, 2);
                        string expr = (m.Groups[3].Success ? m.Groups[3].Value : "").Trim();
                        expr = StripComment(expr);
                        if (string.IsNullOrWhiteSpace(name)) continue;
                        if (name.Equals("A", StringComparison.OrdinalIgnoreCase) && expr.Equals("B", StringComparison.OrdinalIgnoreCase))
                            continue;
                        list.Add(new HoverItem
                        {
                            Line = i + 1,
                            Name = name.Trim(),
                            Expr = expr,
                            Source = "AddError",
                        });
                    }
                }
            }
            return list;
        }

        public static void Bind(IList<HoverItem> items, IList<TraceEvent> trace, IDictionary<string, string> parms, IList<Dictionary<string, object>> vars)
        {
            if (items == null) return;
            foreach (HoverItem it in items)
            {
                string fromLog = FromTrace(trace, it.Name);
                if (fromLog != null)
                {
                    it.Value = fromLog;
                    it.Source = "logfilefj";
                    continue;
                }
                string v = FromMap(parms, it.Name);
                string src = "ParametersValue";
                if (v == null && !string.IsNullOrEmpty(it.Expr))
                    v = FromMap(parms, it.Expr);
                if (v == null)
                {
                    v = FromVars(vars, it.Name);
                    src = "Sara";
                }
                if (v == null && !string.IsNullOrEmpty(it.Expr))
                    v = FromVars(vars, IdentTail(it.Expr));
                if (v == null) continue;
                it.Value = v;
                it.Source = src;
            }
        }

        public static List<Dictionary<string, object>> PackLines(IList<HoverItem> items)
        {
            var map = new Dictionary<int, List<Dictionary<string, object>>>();
            var order = new List<int>();
            if (items == null) return new List<Dictionary<string, object>>();
            foreach (HoverItem it in items)
            {
                List<Dictionary<string, object>> bucket;
                if (!map.TryGetValue(it.Line, out bucket))
                {
                    bucket = new List<Dictionary<string, object>>();
                    map[it.Line] = bucket;
                    order.Add(it.Line);
                }
                bucket.Add(new Dictionary<string, object>
                {
                    { "name", it.Name ?? "" },
                    { "expr", it.Expr ?? "" },
                    { "value", it.Value ?? "" },
                    { "source", it.Source ?? "" },
                });
            }
            var list = new List<Dictionary<string, object>>();
            foreach (int line in order)
            {
                list.Add(new Dictionary<string, object>
                {
                    { "line", line },
                    { "items", map[line] },
                });
            }
            return list;
        }

        public static List<string> Idents(string line)
        {
            var list = new List<string>();
            if (string.IsNullOrEmpty(line)) return list;
            foreach (Match m in RxIdent.Matches(line))
            {
                string n = m.Value;
                if (Skip.Contains(n)) continue;
                bool dup = false;
                foreach (string x in list)
                    if (string.Equals(x, n, StringComparison.OrdinalIgnoreCase)) { dup = true; break; }
                if (!dup) list.Add(n);
            }
            return list;
        }

        public static int ProbeCount(IList<HoverItem> items)
        {
            return items == null ? 0 : items.Count;
        }

        public static int BoundCount(IList<HoverItem> items)
        {
            if (items == null) return 0;
            int n = 0;
            foreach (HoverItem it in items)
                if (!string.IsNullOrEmpty(it.Value)) n++;
            return n;
        }

        private static string FromTrace(IList<TraceEvent> trace, string name)
        {
            if (trace == null || string.IsNullOrEmpty(name)) return null;
            string last = null;
            foreach (TraceEvent e in trace)
            {
                if (e == null) continue;
                if (string.Equals(e.Key, name, StringComparison.OrdinalIgnoreCase))
                    last = e.Title;
            }
            return string.IsNullOrWhiteSpace(last) ? null : last;
        }

        private static string FromMap(IDictionary<string, string> map, string name)
        {
            if (map == null || string.IsNullOrWhiteSpace(name)) return null;
            string v;
            if (map.TryGetValue(name.Trim(), out v) && !string.IsNullOrWhiteSpace(v)) return v;
            return null;
        }

        private static string FromVars(IList<Dictionary<string, object>> vars, string name)
        {
            if (vars == null || string.IsNullOrWhiteSpace(name)) return null;
            string last = null;
            foreach (var v in vars)
            {
                object n, val;
                if (!v.TryGetValue("name", out n) || !v.TryGetValue("value", out val)) continue;
                if (!string.Equals(Convert.ToString(n), name, StringComparison.OrdinalIgnoreCase)) continue;
                string s = Convert.ToString(val, CultureInfo.InvariantCulture);
                if (!string.IsNullOrWhiteSpace(s)) last = s;
            }
            return last;
        }

        private static string IdentTail(string expr)
        {
            if (string.IsNullOrEmpty(expr)) return expr;
            int dot = expr.LastIndexOf('.');
            string t = dot >= 0 ? expr.Substring(0, dot) : expr;
            Match m = RxIdent.Match(t.Trim());
            return m.Success ? m.Value : expr.Trim();
        }

        private static string FirstGroup(Match m, int a, int b)
        {
            if (m.Groups[a].Success && m.Groups[a].Value.Length > 0) return m.Groups[a].Value;
            if (m.Groups[b].Success) return m.Groups[b].Value;
            return "";
        }

        private static string StripComment(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            int q = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '"') q++;
                if (s[i] == '\'' && q % 2 == 0) return s.Substring(0, i).Trim().TrimEnd(')');
            }
            return s.Trim().TrimEnd(')');
        }
    }
}
