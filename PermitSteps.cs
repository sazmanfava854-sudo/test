using System;
using System.Collections.Generic;
using System.Globalization;

namespace RuleTrace
{
    /// <summary>
    /// User picks forms (ضابطه/صلح/چیدمان/تحلیل/توافق/کمیسیون/درآمد). Independent — no linear gate.
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

        /// <summary>Step 1 is overlay on the property. ActiveNidZabeteh is only a Solh gate — not every property has Solh.</summary>
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
            bool hasOverlay = !ZabetehCase.IsEmptyGuid(overlayId);
            if (hasOverlay)
            {
                r.Status = "PASS";
                r.Verdict = "ضابطه ملک هست. روکش=" + overlayId
                    + (string.IsNullOrEmpty(planType) ? "" : " PlanType=" + planType)
                    + (ZabetehCase.IsEmptyGuid(active) ? " — Active خالی است ولی اعلام فقط برای صلح لازم است" : " Active=" + Trunc(active, 36));
                r.NextAction = "صلح برای همه ملک‌ها اجباری نیست. گام ۲ را جدا ببینید.";
                return r;
            }
            r.Status = "FAIL";
            r.Verdict = "روکش Zabeteh برای این ملک پیدا نشد";
            r.NextAction = "join " + ZabetehCase.JoinOn + " را در SSMS چک کنید.";
            return r;
        }

        /// <summary>Solh is optional. Empty Active means this file is not calculating Solh — not a missing overlay.</summary>
        public static Result ClassifySolh(string workItem, string active, bool hasSolhRecord)
        {
            var r = new Result { Number = 2, Name = Names[1] };
            if (PermitPipeline.IsIgnoredWorkItem(workItem))
            {
                r.Status = "SKIP";
                r.Verdict = "WorkItem " + PermitPipeline.IgnoreWorkItem + " صلح نیست";
                r.NextAction = "گام ۳ تحلیل را جدا ببینید";
                return r;
            }
            if (hasSolhRecord && ZabetehCase.IsEmptyGuid(active))
            {
                r.Status = "FAIL";
                r.Verdict = "این پرونده صلح دارد ولی ActiveNidZabeteh خالی است — L270 می‌ایستد";
                r.NextAction = "اگر صلح باید محاسبه شود، روکش را اعلام کنید. Member 1296 را عوض نکنید.";
                return r;
            }
            if (hasSolhRecord)
            {
                r.Status = "PASS";
                r.Verdict = "صلح اعمال می‌شود Active=" + Trunc(active, 36);
                r.NextAction = "گام ۳ تحلیل را جدا ببینید";
                return r;
            }
            r.Status = "SKIP";
            r.Verdict = "صلح ندارد — همه ملک‌ها صلح ندارند. L270 این پرونده را متوقف نمی‌کند";
            r.NextAction = "گام ۳ تحلیل را جدا ببینید. Member 1296 را عوض نکنید.";
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
            if (step1.Status == "FAIL")
            {
                for (int n = 2; n <= 5; n++)
                    log("Step        : " + n + "/5 " + Names[n - 1] + " — اجرا نشد (گام ۱ قبول نشد)");
                log("Exit code    : 1");
                return Pack(step1, SlimVars(vars), 1);
            }

            Result step2 = ClassifySolh(work, active, HasSolhRecord(vars));
            Write(log, step2);
            if (step2.Status == "FAIL")
            {
                for (int n = 3; n <= 5; n++)
                    log("Step        : " + n + "/5 " + Names[n - 1] + " — اجرا نشد (گام ۲ صلح رد شد)");
                log("Exit code    : 1");
                return Pack(step2, SlimVars(vars), 1);
            }

            log("Step        : 3/5 تحلیل — بعدی (Instanc زنده در سارا)");
            log("Step        : 4/5 کمیسیون ماده ۱۰۰ — بعد از تحلیل");
            log("Step        : 5/5 درآمد — بعد از کمیسیون");
            log("Step        : کار بعدی: گام ۳ تحلیل را در سارا ببینید. صلح اجباری نیست.");
            log("Exit code    : 0");
            return Pack(step1.Status == "PASS" && step2.Status == "SKIP" ? step2 : step1, SlimVars(vars), 0);
        }

        /// <summary>Independent forms. Empty pick → FAIL. Solh is Sh_Peace; agreement is Sh_Agreement; analysis is AnalysisBuilding.</summary>
        public static Dictionary<string, object> RunSelected(string sara, string ruleEngine, string nidProc, IList<string> scopes, Action<string> log)
        {
            if (log == null) log = m => { };
            var selected = PermitScopes.Normalize(scopes);
            nidProc = (nidProc ?? "").Trim();
            log("Scope      : مثل باز کردن فرم سارا — فقط بخش‌های تیک‌خورده. توافق و کمیسیون می‌تواند نباشد");
            if (selected.Count == 0)
            {
                log("Scope      : " + PermitScopes.MustPick);
                log("Exit code    : 1");
                return Pack(0, "FAIL", PermitScopes.MustPick, "تیک بزنید کدام بخش را دیباگ می‌کنید", 1);
            }
            if (nidProc.Length == 0)
            {
                log("Scope      : NidProc خالی — اول جستجو کنید");
                return Pack(0, "FAIL", "NidProc خالی", "اول پرونده را جستجو کنید", 1);
            }

            var vars = ZabetehCase.ReadSelected(sara, nidProc, selected, log);
            string work = PermitPipeline.FindWorkItem(vars);
            string active = SolhNidDebug.ReadActive(vars);
            string overlay = FirstFrom(vars, "[dbo].[Zabeteh]", "NidZabeteh") ?? FirstFrom(vars, "Zabeteh", "NidZabeteh");
            string plan = FirstFrom(vars, "[dbo].[Zabeteh]", "CI_PlanType") ?? FirstFrom(vars, "CI_PlanType", "ID")
                ?? FirstFrom(vars, "CI_PlanType", "Id");

            var results = new List<Result>();
            foreach (string id in selected)
            {
                Result r;
                if (id == PermitScopes.Zabeteh)
                    r = ClassifyZabeteh(work, active, overlay, plan);
                else if (id == PermitScopes.Solh)
                    r = ClassifySolh(work, active, HasSolhRecord(vars));
                else if (id == PermitScopes.Chidman)
                    r = ClassifyChidman(work, HasNamed(vars, "Base_Using") || HasNamed(vars, "Base_Front"));
                else if (id == PermitScopes.Tahlil)
                    r = ClassifyTahlil(work, HasNamed(vars, "AnalysisBuilding"));
                else if (id == PermitScopes.Tavafogh)
                    r = ClassifyTavafogh(work, HasNamed(vars, "Sh_Agreement"));
                else if (id == PermitScopes.Commission)
                    r = ClassifyCommission(work);
                else if (id == PermitScopes.Income)
                    r = ClassifyIncome(work);
                else
                    continue;
                r.Name = PermitScopes.Title(id);
                WriteScope(log, r);
                results.Add(r);
            }

            Result worst = PickWorst(results);
            log("Exit code    : " + (worst.Status == "FAIL" ? 1 : 0));
            var pack = Pack(worst, ScopeVars(vars, selected), worst.Status == "FAIL" ? 1 : 0);
            pack["scopes"] = selected;
            pack["sections"] = PackSections(results);
            return pack;
        }

        public static Result ClassifyChidman(string workItem, bool hasLayout)
        {
            var r = new Result { Number = 3, Name = PermitScopes.Title(PermitScopes.Chidman) };
            if (PermitPipeline.IsIgnoredWorkItem(workItem))
            {
                r.Status = "SKIP";
                r.Verdict = "WorkItem " + PermitPipeline.IgnoreWorkItem + " بررسی نمی‌شود";
                r.NextAction = "WorkItem " + PermitPipeline.SampleWorkItem + " را جستجو کنید";
                return r;
            }
            if (hasLayout)
            {
                r.Status = "PASS";
                r.Verdict = "چیدمان در Base_Using / Base_Front هست";
                r.NextAction = "متغیر UsingArea و CI_UsingType / CI_FrontPlace را در لاگ چک کنید";
                return r;
            }
            r.Status = "SKIP";
            r.Verdict = "چیدمان در Base_Using/Base_Front نیست";
            r.NextAction = "اگر این ملک چیدمان دارد، NidBase را در SSMS چک کنید";
            return r;
        }

        public static Result ClassifyTahlil(string workItem, bool hasAnalysis)
        {
            var r = new Result { Number = 3, Name = PermitScopes.Title(PermitScopes.Tahlil) };
            if (PermitPipeline.IsIgnoredWorkItem(workItem))
            {
                r.Status = "SKIP";
                r.Verdict = "WorkItem " + PermitPipeline.IgnoreWorkItem + " بررسی نمی‌شود";
                r.NextAction = "WorkItem " + PermitPipeline.SampleWorkItem + " را جستجو کنید";
                return r;
            }
            if (hasAnalysis)
            {
                r.Status = "PASS";
                r.Verdict = "تحلیل در AnalysisBuilding هست (Parvaneh=1 Foul=2 MovafeghatOsooli=3). AnaliysParvaneh_Date=max(PenaltyDate)";
                r.NextAction = "ریز را در AnalysisBuilding_Details و CI_Penalty ببینید";
                return r;
            }
            r.Status = "SKIP";
            r.Verdict = "تحلیل در AnalysisBuilding نیست";
            r.NextAction = "NidBase را از Base_Info با NidNosaziCode در SSMS چک کنید";
            return r;
        }

        public static Result ClassifyTavafogh(string workItem, bool hasAgreement)
        {
            var r = new Result { Number = 4, Name = PermitScopes.Title(PermitScopes.Tavafogh) };
            if (PermitPipeline.IsIgnoredWorkItem(workItem))
            {
                r.Status = "SKIP";
                r.Verdict = "WorkItem " + PermitPipeline.IgnoreWorkItem + " بررسی نمی‌شود";
                r.NextAction = "WorkItem " + PermitPipeline.SampleWorkItem + " را جستجو کنید";
                return r;
            }
            if (hasAgreement)
            {
                r.Status = "PASS";
                r.Verdict = "توافق در Sh_Agreement هست (Building=0)";
                r.NextAction = "نوع را در Sh_AgreementLetter / CI_AgreementType ببینید";
                return r;
            }
            r.Status = "SKIP";
            r.Verdict = "توافق ندارد — ملک می‌تواند توافق نداشته باشد";
            r.NextAction = "اگر توافق باید باشد، Building=0 و NidBase را چک کنید";
            return r;
        }

        public static Result ClassifyCommission(string workItem)
        {
            var r = new Result { Number = 4, Name = PermitScopes.Title(PermitScopes.Commission), Status = "SKIP" };
            if (PermitPipeline.IsIgnoredWorkItem(workItem))
            {
                r.Verdict = "WorkItem " + PermitPipeline.IgnoreWorkItem + " بررسی نمی‌شود";
                r.NextAction = "WorkItem " + PermitPipeline.SampleWorkItem + " را جستجو کنید";
                return r;
            }
            r.Verdict = "کمیسیون ماده ۱۰۰ جدول نام‌دار ندارد — ملک می‌تواند کمیسیون نداشته باشد";
            r.NextAction = "اگر رای ماده ۱۰۰ هست، جدول را نام ببرید تا به dump اضافه شود";
            return r;
        }

        public static Result ClassifyIncome(string workItem)
        {
            var r = new Result { Number = 5, Name = PermitScopes.Title(PermitScopes.Income), Status = "SKIP" };
            if (PermitPipeline.IsIgnoredWorkItem(workItem))
            {
                r.Verdict = "WorkItem " + PermitPipeline.IgnoreWorkItem + " بررسی نمی‌شود";
                r.NextAction = "WorkItem " + PermitPipeline.SampleWorkItem + " را جستجو کنید";
                return r;
            }
            r.Verdict = "درآمد جدول نام‌دار ندارد";
            r.NextAction = "اگر جدول درآمد مشخص شد، به dump اضافه می‌شود";
            return r;
        }

        internal static bool HasSolhRecord(IList<Dictionary<string, object>> vars)
        {
            if (HasNamed(vars, "Sh_Peace")) return true;
            if (vars == null) return false;
            foreach (var v in vars)
            {
                object n, t, val;
                v.TryGetValue("name", out n);
                v.TryGetValue("table", out t);
                v.TryGetValue("value", out val);
                string name = Convert.ToString(n) ?? "";
                string table = Convert.ToString(t) ?? "";
                string value = Convert.ToString(val, CultureInfo.InvariantCulture) ?? "";
                if (name.IndexOf("ActiveNidZabeteh", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                if (table.IndexOf("Zabeteh", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                if (table.IndexOf("Tavafogh", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                if (table.IndexOf("Sh_Agreement", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                bool nameHit = name.Equals("NidPeace", StringComparison.OrdinalIgnoreCase)
                    || name.Equals("NidSolh", StringComparison.OrdinalIgnoreCase);
                bool tableHit = table.IndexOf("Sh_Peace", StringComparison.OrdinalIgnoreCase) >= 0;
                if ((nameHit || tableHit) && !string.IsNullOrWhiteSpace(value) && !ZabetehCase.IsEmptyGuid(value))
                    return true;
            }
            return false;
        }

        internal static bool HasNamed(IList<Dictionary<string, object>> vars, string tableHint)
        {
            if (vars == null || string.IsNullOrEmpty(tableHint)) return false;
            foreach (var v in vars)
            {
                object t, val;
                if (!v.TryGetValue("table", out t) || !v.TryGetValue("value", out val)) continue;
                string table = Convert.ToString(t) ?? "";
                string value = Convert.ToString(val, CultureInfo.InvariantCulture) ?? "";
                if (table.IndexOf(tableHint, StringComparison.OrdinalIgnoreCase) < 0) continue;
                if (!string.IsNullOrWhiteSpace(value) && !ZabetehCase.IsEmptyGuid(value))
                    return true;
            }
            return false;
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

        private static void WriteScope(Action<string> log, Result r)
        {
            log("Scope      : " + r.Name + " — " + FaStatus(r.Status));
            if (!string.IsNullOrEmpty(r.Verdict))
                log("Scope      : " + r.Verdict);
            if (!string.IsNullOrEmpty(r.NextAction))
                log("Scope      : کار بعدی: " + r.NextAction);
        }

        private static Result PickWorst(List<Result> results)
        {
            if (results == null || results.Count == 0)
                return new Result { Number = 0, Name = "انتخاب", Status = "FAIL", Verdict = PermitScopes.MustPick, NextAction = "تیک بزنید" };
            Result fail = null, pass = null, skip = null;
            foreach (Result r in results)
            {
                if (r.Status == "FAIL" && fail == null) fail = r;
                else if (r.Status == "PASS" && pass == null) pass = r;
                else if (skip == null) skip = r;
            }
            if (fail != null)
            {
                var parts = new List<string>();
                foreach (Result r in results)
                    if (r.Status == "FAIL") parts.Add(r.Name + ": " + r.Verdict);
                fail.Verdict = string.Join(" | ", parts);
                return fail;
            }
            if (pass != null)
            {
                var parts = new List<string>();
                foreach (Result r in results) parts.Add(r.Name + " " + FaStatus(r.Status));
                pass.Verdict = string.Join("، ", parts) + ". " + (pass.Verdict ?? "");
                return pass;
            }
            skip.Verdict = (skip.Verdict ?? "") + " — بخش‌های انتخاب‌شده برای این ملک لازم نیست";
            return skip;
        }

        private static List<object> PackSections(List<Result> results)
        {
            var list = new List<object>();
            if (results == null) return list;
            foreach (Result r in results)
            {
                list.Add(new Dictionary<string, object>
                {
                    { "name", r.Name ?? "" },
                    { "status", r.Status ?? "" },
                    { "verdict", r.Verdict ?? "" },
                    { "nextAction", r.NextAction ?? "" },
                });
            }
            return list;
        }

        /// <summary>Keep keys plus columns from the ticked named tables so the vars tab matches the open form.</summary>
        internal static List<Dictionary<string, object>> ScopeVars(IList<Dictionary<string, object>> vars, IList<string> scopes)
        {
            var keepName = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ActiveNidZabeteh", "NidZabeteh", "NidWorkItem", "NidNosaziCode", "NidBase",
                "Building", "CI_PlanType", "NidProc", "JoinOn", "WorkflowTitel", "Workflow",
                "UsingArea", "PenaltyDate", "AnaliysParvaneh_Date", "Parvaneh", "Foul", "MovafeghatOsooli",
            };
            var keepTable = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Sh_RequestInfo", "Base_Info", "Base_NosaziCode", "EumAnalysisBuildingType", "join"
            };
            if (scopes != null)
            {
                foreach (string id in scopes)
                    foreach (string t in PermitScopes.Tables(id))
                        keepTable.Add(t);
            }
            var outVars = new List<Dictionary<string, object>>();
            if (vars == null) return outVars;
            foreach (var v in vars)
            {
                object n, t;
                v.TryGetValue("name", out n);
                v.TryGetValue("table", out t);
                string name = Convert.ToString(n) ?? "";
                string table = Convert.ToString(t) ?? "";
                bool hit = keepName.Contains(name);
                if (!hit)
                {
                    foreach (string hint in keepTable)
                        if (table.IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0) { hit = true; break; }
                }
                if (hit) outVars.Add(v);
            }
            return outVars;
        }

        private static string FaStatus(string s)
        {
            if (s == "PASS") return "قبول";
            if (s == "SKIP") return "لازم نیست";
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
