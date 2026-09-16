using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RuleTrace
{
    /// <summary>
    /// Every پروانه (building permit) walks: ضابطه → صلح → تحلیل → کمیسیون ماده ۱۰۰ → درآمد.
    /// WorkItem 300002275 is پروانه تجدید بنا. WorkItem 5298603 is not a permit/Solh case — skip it.
    /// Read-only. Does not write dbo.Member. Does not compile VB.
    /// </summary>
    internal static class PermitPipeline
    {
        public const string SampleWorkItem = "300002275";
        public const string SampleKind = "پروانه تجدید بنا";
        public const string IgnoreWorkItem = "5298603";

        public const string PathFa = "ضابطه → صلح (اگر باشد) → تحلیل → کمیسیون ماده ۱۰۰ → درآمد";

        public static readonly string[] StageNames = { "ضابطه", "صلح", "تحلیل", "کمیسیون ماده ۱۰۰", "درآمد" };

        /// <summary>FormulaMap ids that every permit must load (plus Global 432).</summary>
        public static readonly int[] PermitClasses =
        {
            336, // Rule / ضابطه
            342, // ZabetehConvert / چیدمان
            344, // Solh
            345, // Tavafogh
            338, // Takhalofat / تحلیل
            340, // Commission / ماده ۱۰۰
            335, // CommissionFine
            337, // Income / درآمد
            341, // Validtion
            339, // Nosazi_Calculate
            432, // Global
        };

        public static bool IsIgnoredWorkItem(string workItem)
        {
            return SameId(workItem, IgnoreWorkItem);
        }

        public static bool IsPermitSample(string workItem)
        {
            return SameId(workItem, SampleWorkItem);
        }

        public static string FindWorkItem(IList<Dictionary<string, object>> vars)
        {
            return First(vars, "NidWorkItem", "WorkItem", "NidRequest");
        }

        public static string FindWorkflow(IList<Dictionary<string, object>> vars)
        {
            return First(vars, "WorkflowTitel", "WorkflowTitle", "Workflow", "RequestType");
        }

        public static string Report(IList<MemberSource> sources, IList<Dictionary<string, object>> vars, Action<string> log)
        {
            if (log == null) log = m => { };
            string work = FindWorkItem(vars);
            string flow = FindWorkflow(vars);

            if (IsIgnoredWorkItem(work))
            {
                log("Permit      : WorkItem=" + IgnoreWorkItem + " بررسی نمی‌شود — این درخواست صلح نیست");
                log("Permit      : پرونده پروانه: WorkItem=" + SampleWorkItem + " (" + SampleKind + ")");
                return "این درخواست بررسی نمی‌شود. پرونده پروانه " + SampleKind + " را با WorkItem " + SampleWorkItem + " جستجو کنید.";
            }

            log("Permit      : مسیر پروانه = " + PathFa);
            if (IsPermitSample(work))
                log("Permit      : پرونده " + SampleKind + " WorkItem=" + SampleWorkItem);
            else if (!string.IsNullOrEmpty(work))
                log("Permit      : NidWorkItem=" + work + (string.IsNullOrEmpty(flow) ? "" : " گردش‌کار=" + Trunc(flow, 80)));
            else
                log("Permit      : NidWorkItem خالی — بعد از جستجو، همه پروانه‌ها همین پنج مرحله را دارند");

            if (!string.IsNullOrEmpty(flow))
                log("Permit      : WorkflowTitel=" + Trunc(flow, 120));

            string active = SolhNidDebug.ReadActive(vars);
            bool emptyActive = ZabetehCase.IsEmptyGuid(active);
            bool hasOverlay = HasName(vars, "NidZabeteh") || HasTable(vars, "Zabeteh");
            string nosazi = First(vars, "NidNosaziCode");

            int rule = CountClass(sources, 336);
            int convert = CountClass(sources, 342);
            int solh = CountClass(sources, 344);
            int tavafogh = CountClass(sources, 345);
            int tahlil = CountClass(sources, 338);
            int commission = CountClass(sources, 340);
            int fine = CountClass(sources, 335);
            int income = CountClass(sources, 337);

            log("Permit      : ضابطه    Rule/336=" + rule + " ZabetehConvert/342=" + convert
                + " overlay=" + (hasOverlay ? "هست" : "نیست")
                + " ActiveNidZabeteh=" + (emptyActive ? "(خالی)" : Trunc(active, 36))
                + " NidNosaziCode=" + (nosazi ?? "(خالی)"));
            log("Permit      : صلح      Solh/344=" + solh + " Tavafogh/345=" + tavafogh
                + (emptyActive
                    ? " — همه ملک‌ها صلح ندارند؛ L270 فقط اگر صلح محاسبه شود"
                    : " — L270 شلیک نمی‌شود"));
            log("Permit      : تحلیل    Takhalofat/338=" + tahlil
                + (tahlil == 0 ? " — فرمول تحلیل برای این بارگذاری نیامد" : ""));
            log("Permit      : کمیسیون  Commission/340=" + commission + " CommissionFine/335=" + fine
                + (commission + fine == 0 ? " — فرمول ماده ۱۰۰ برای این بارگذاری نیامد" : ""));
            log("Permit      : درآمد    Income/337=" + income
                + (income == 0 ? " — فرمول درآمد برای این بارگذاری نیامد" : ""));

            int hits = CountHints(vars);
            if (hits > 0)
                log("Permit      : ستون‌های مرحله‌ای در Sara=" + hits);

            return Summarize(work, flow, emptyActive, hasOverlay, tahlil, commission, fine, income);
        }

        private static string Summarize(string work, string flow, bool emptyActive, bool hasOverlay, int tahlil, int commission, int fine, int income)
        {
            string who = IsPermitSample(work)
                ? SampleKind + " " + SampleWorkItem
                : (string.IsNullOrEmpty(work) ? "این Nid" : "WorkItem " + work);
            if (!string.IsNullOrEmpty(flow))
                who += " / " + Trunc(flow, 40);

            var missing = new List<string>();
            if (tahlil == 0) missing.Add("تحلیل/338");
            if (commission == 0 && fine == 0) missing.Add("کمیسیون ماده ۱۰۰");
            if (income == 0) missing.Add("درآمد/337");

            string head = who + " مسیر " + PathFa + ". ";
            if (emptyActive && hasOverlay)
                head += "ضابطه برای ملک هست (Zabeteh با NidNosaziCode). اعلام Active فقط وقتی لازم است که این ملک صلح داشته باشد — همه ملک‌ها صلح ندارند. ";
            else if (emptyActive)
                head += "روکش Zabeteh برای این ملک پیدا نشد. join " + ZabetehCase.JoinOn + ". ";
            else
                head += "ضابطه اعلام شده. ";
            if (missing.Count > 0)
                head += "کمبود بارگذاری: " + string.Join("، ", missing) + ".";
            else if (!(emptyActive && hasOverlay))
                head += "مراحل فرمول بارگذاری شد.";
            else
                head += "صلح اجباری نیست؛ تحلیل/کمیسیون/درآمد را جدا ببینید.";
            return head;
        }

        private static int CountClass(IList<MemberSource> sources, int nid)
        {
            if (sources == null) return 0;
            int n = 0;
            foreach (var s in sources)
                if (s != null && s.NidClass == nid) n++;
            return n;
        }

        private static int CountHints(IList<Dictionary<string, object>> vars)
        {
            if (vars == null) return 0;
            int n = 0;
            foreach (var v in vars)
            {
                object name;
                if (!v.TryGetValue("name", out name)) continue;
                string s = Convert.ToString(name) ?? "";
                if (s.IndexOf("Takhalof", StringComparison.OrdinalIgnoreCase) >= 0
                    || s.IndexOf("Tahlil", StringComparison.OrdinalIgnoreCase) >= 0
                    || s.IndexOf("Commission", StringComparison.OrdinalIgnoreCase) >= 0
                    || s.IndexOf("Jarime", StringComparison.OrdinalIgnoreCase) >= 0
                    || s.IndexOf("Daramad", StringComparison.OrdinalIgnoreCase) >= 0
                    || s.IndexOf("Income", StringComparison.OrdinalIgnoreCase) >= 0
                    || s.IndexOf("Madeh", StringComparison.OrdinalIgnoreCase) >= 0)
                    n++;
            }
            return n;
        }

        private static bool HasName(IList<Dictionary<string, object>> vars, string name)
        {
            return !string.IsNullOrEmpty(First(vars, name));
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

        private static string First(IList<Dictionary<string, object>> vars, params string[] names)
        {
            if (vars == null) return null;
            foreach (string name in names)
            {
                foreach (var v in vars)
                {
                    object n, val;
                    if (!v.TryGetValue("name", out n) || !v.TryGetValue("value", out val)) continue;
                    if (!string.Equals(Convert.ToString(n), name, StringComparison.OrdinalIgnoreCase)) continue;
                    string s = Convert.ToString(val, CultureInfo.InvariantCulture);
                    if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
                }
            }
            return null;
        }

        private static bool SameId(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return false;
            return string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static string Trunc(string s, int n)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Replace("\r", " ").Replace("\n", " ");
            return s.Length <= n ? s : s.Substring(0, n) + "…";
        }
    }
}
