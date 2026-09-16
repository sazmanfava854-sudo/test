using System;
using System.Collections.Generic;
using System.Globalization;

namespace RuleTrace
{
    /// <summary>
    /// One permit step at a time. Do not dump Solh/chidman/docs/history until step 1 (اعلام ضابطه) passes.
    /// Read-only. Does not write dbo.Member. Does not compile VB.
    /// </summary>
    internal static class PermitSteps
    {
        public static readonly string[] Names = { "ضابطه", "صلح", "تحلیل", "کمیسیون ماده ۱۰۰", "درآمد" };

        public sealed class Result
        {
            public int Number;
            public string Name;
            public string Status;
            public string Verdict;
            public string NextAction;
        }

        /// <summary>Step 1 only needs overlay vs Active. Later steps stay blocked until this passes.</summary>
        public static Result ClassifyZabeteh(string workItem, string active, string overlayId, string planType)
        {
            var r = new Result { Number = 1, Name = Names[0] };
            if (PermitPipeline.IsIgnoredWorkItem(workItem))
            {
                r.Status = "SKIP";
                r.Verdict = "WorkItem " + PermitPipeline.IgnoreWorkItem + " بررسی نمی‌شود";
                r.NextAction = "WorkItem " + PermitPipeline.SampleWorkItem + " را جستجو کنید (پروانه تجدید بنا)";
                return r;
            }
            bool emptyActive = ZabetehCase.IsEmptyGuid(active);
            bool hasOverlay = !ZabetehCase.IsEmptyGuid(overlayId);
            if (emptyActive && hasOverlay)
            {
                r.Status = "FAIL";
                r.Verdict = "ضابطه هست، اعلام نشده. روکش=" + overlayId
                    + (string.IsNullOrEmpty(planType) ? "" : " PlanType=" + planType);
                r.NextAction = "در گردش پروانه همین روکش را اعلام کنید تا ActiveNidZabeteh=" + overlayId
                    + " شود. Member 1296 را عوض نکنید. گام‌های ۲–۵ را اجرا نکنید.";
                return r;
            }
            if (emptyActive)
            {
                r.Status = "FAIL";
                r.Verdict = "نه روکش Zabeteh نه Active — ضابطه برای این ملک پیدا نشد";
                r.NextAction = "join " + ZabetehCase.JoinOn + " را در SSMS چک کنید. Member 1296 را عوض نکنید.";
                return r;
            }
            r.Status = "PASS";
            r.Verdict = "ضابطه اعلام شده Active=" + Trunc(active, 36);
            r.NextAction = "گام ۲ صلح را جدا بزنید";
            return r;
        }

        public static Dictionary<string, object> RunUntilFail(string sara, string ruleEngine, string nidProc, Action<string> log)
        {
            if (log == null) log = m => { };
            nidProc = (nidProc ?? "").Trim();
            log("Step        : رویکرد گام‌به‌گام — فقط گام ردشده. چیدمان/مستند/تاریخچه/موتور اجرا نشد");
            if (nidProc.Length == 0)
            {
                log("Step        : NidProc خالی — اول جستجو کنید");
                return Pack(1, "FAIL", "NidProc خالی", "اول پرونده را جستجو کنید", 1);
            }

            var vars = ZabetehCase.ReadStep1(sara, nidProc, log);
            string work = PermitPipeline.FindWorkItem(vars);
            string active = SolhNidDebug.ReadActive(vars);
            string overlay = FirstFrom(vars, "[dbo].[Zabeteh]", "NidZabeteh") ?? FirstFrom(vars, "Zabeteh", "NidZabeteh");
            string plan = FirstFrom(vars, "[dbo].[Zabeteh]", "CI_PlanType") ?? FirstFrom(vars, "CI_PlanType", "ID")
                ?? FirstFrom(vars, "CI_PlanType", "Id");

            Result step1 = ClassifyZabeteh(work, active, overlay, plan);
            Write(log, step1);
            int code = step1.Status == "PASS" ? 0 : (step1.Status == "SKIP" ? 0 : 1);
            if (step1.Status != "PASS")
            {
                for (int n = 2; n <= 5; n++)
                    log("Step        : " + n + "/5 " + Names[n - 1] + " — اجرا نشد (گام ۱ قبول نشد)");
                log("Exit code    : " + code);
                return Pack(step1, SlimVars(vars), code);
            }

            log("Step        : 2/5 صلح — PASS (Active پر است؛ L270 شلیک نمی‌شود)");
            log("Step        : 3/5 تحلیل — موقوف تا Instanc زنده در سارا");
            log("Step        : 4/5 کمیسیون ماده ۱۰۰ — موقوف تا Instanc زنده در سارا");
            log("Step        : 5/5 درآمد — موقوف تا Instanc زنده در سارا");
            log("Step        : کار بعدی: گام ۳–۵ را در خود سارا بعد از اعلام ضابطه اجرا کنید");
            log("Exit code    : 0");
            return Pack(step1, SlimVars(vars), 0);
        }

        private static Dictionary<string, object> Pack(Result step1, List<Dictionary<string, object>> vars, int code)
        {
            return Pack(step1.Number, step1.Status, step1.Verdict, step1.NextAction, code, vars);
        }

        private static Dictionary<string, object> Pack(int step, string status, string verdict, string next, int code)
        {
            return Pack(step, status, verdict, next, code, new List<Dictionary<string, object>>());
        }

        private static Dictionary<string, object> Pack(int step, string status, string verdict, string next, int code, List<Dictionary<string, object>> vars)
        {
            return new Dictionary<string, object>
            {
                { "step", step },
                { "status", status ?? "" },
                { "diagnosis", verdict ?? "" },
                { "nextAction", next ?? "" },
                { "exitCode", code },
                { "vars", vars ?? new List<Dictionary<string, object>>() },
            };
        }

        private static void Write(Action<string> log, Result r)
        {
            log("Step        : " + r.Number + "/5 " + r.Name + " — " + FaStatus(r.Status));
            if (!string.IsNullOrEmpty(r.Verdict))
                log("Step        : " + r.Verdict);
            if (!string.IsNullOrEmpty(r.NextAction))
                log("Step        : کار بعدی: " + r.NextAction);
        }

        private static string FaStatus(string s)
        {
            if (s == "PASS") return "قبول";
            if (s == "SKIP") return "رد شد از بررسی";
            return "رد";
        }

        private static string FirstFrom(IList<Dictionary<string, object>> vars, string tableHint, string name)
        {
            if (vars == null) return null;
            foreach (var v in vars)
            {
                object t, n, val;
                if (!v.TryGetValue("table", out t) || !v.TryGetValue("name", out n) || !v.TryGetValue("value", out val)) continue;
                if (Convert.ToString(t).IndexOf(tableHint, StringComparison.OrdinalIgnoreCase) < 0) continue;
                if (!string.Equals(Convert.ToString(n), name, StringComparison.OrdinalIgnoreCase)) continue;
                string s = Convert.ToString(val, CultureInfo.InvariantCulture);
                if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
            }
            return null;
        }

        /// <summary>Vars tab: only step-1 keys, not 23 overlay columns.</summary>
        internal static List<Dictionary<string, object>> SlimVars(IList<Dictionary<string, object>> vars)
        {
            var keep = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ActiveNidZabeteh", "NidZabeteh", "NidWorkItem", "NidNosaziCode",
                "CI_PlanType", "NidProc", "JoinOn", "WorkflowTitel", "Workflow",
            };
            var outVars = new List<Dictionary<string, object>>();
            if (vars == null) return outVars;
            foreach (var v in vars)
            {
                object n;
                if (!v.TryGetValue("name", out n)) continue;
                if (!keep.Contains(Convert.ToString(n))) continue;
                outVars.Add(v);
            }
            return outVars;
        }

        private static string Trunc(string s, int n)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= n ? s : s.Substring(0, n) + "…";
        }
    }
}
