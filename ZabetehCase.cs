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
            "Zabeteh_Details",
            "CI_PlanType",
            "CI_PlanUsingType",
            "CI_Zabeteh",
            "ZabeteStatic_Info",
            "ZabeteStatic_Zabete",
            "ZabeteStatic_Plan",
        };

        private static readonly string[] SkipTypes = { "image", "varbinary", "binary", "timestamp", "rowversion" };

        /// <summary>
        /// Overlay is stored per property code, not per NidProc.
        /// Join: Zabeteh.NidNosaziCode = Sh_RequestInfo.NidNosaziCode
        /// Permit sample: WorkItem 300002275 (پروانه تجدید بنا). 5298603 is skipped.
        /// </summary>
        public const string JoinOn = "Zabeteh.NidNosaziCode = Sh_RequestInfo.NidNosaziCode";
        public const string StaticPkeyColumn = "P_Key";
        /// <summary>ZabeteStatic_Plan joins via Info, not CI_PlanType.</summary>
        public static readonly string[] StaticChildPrefer = { "NidZStatic_Info", StaticPkeyColumn };

        public static readonly string JoinSql =
            "SELECT TOP (10) a.NidZabeteh, a.NidNosaziCode, a.CI_PlanType, a.DateZabeteh, a.TimeZabeteh, a.UserName, " +
            "b.NidProc, b.NidWorkItem, b.ActiveNidZabeteh, b.RequesterName " +
            "FROM [dbo].[Zabeteh] a " +
            "INNER JOIN [dbo].[Sh_RequestInfo] b ON a.NidNosaziCode = b.NidNosaziCode " +
            "WHERE b.NidProc = @nidProc " +
            "ORDER BY a.DateZabeteh DESC";

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
            log("Zabeteh     : join = " + JoinOn + "  (نه NidProc)");
            var keys = new CaseKeys { NidProc = nidProc.Trim() };

            DumpNamed(sara, "Sh_RequestInfo", keys, vars, log, "NidProc");
            FillKeys(keys, vars);
            vars.Add(new Dictionary<string, object>
            {
                { "name", "JoinOn" },
                { "value", JoinOn },
                { "table", "join" },
                { "match", "کلید اتصال" },
            });
            log("Zabeteh     : NidWorkItem=" + (keys.NidWorkItem ?? "(خالی)")
                + " Workflow=" + (keys.Workflow ?? "(خالی)")
                + " NidNosaziCode=" + (keys.NidNosaziCode ?? "(خالی)")
                + " ActiveNidZabeteh=" + (IsEmptyGuid(keys.ActiveNidZabeteh) ? "(خالی)" : keys.ActiveNidZabeteh)
                + " P_Key=" + (keys.Pkey ?? "(خالی)"));
            if (PermitPipeline.IsIgnoredWorkItem(keys.NidWorkItem))
                log("Zabeteh     : WorkItem " + PermitPipeline.IgnoreWorkItem + " بررسی نمی‌شود");
            if (PermitPipeline.IsPermitSample(keys.NidWorkItem))
                log("Zabeteh     : پرونده " + PermitPipeline.SampleKind + " WorkItem=" + PermitPipeline.SampleWorkItem);

            if (IsEmptyGuid(keys.ActiveNidZabeteh))
                log("Zabeteh     : ActiveNidZabeteh خالی — اعلام فقط وقتی لازم است که این ملک صلح داشته باشد");

            DumpZabeteh(sara, keys, vars, log);
            FillKeys(keys, vars);

            LookupById(sara, "CI_PlanType", keys.PlanTypeId, vars, log);
            LookupById(sara, "CI_PlanUsingType", keys.PlanUsingTypeId, vars, log);
            LookupById(sara, "CI_Zabeteh", keys.CIZabetehId, vars, log);

            DumpStaticLayer(sara, keys, vars, log);

            int zabRows = CountTable(vars, "Zabeteh");
            int staticInfo = CountTable(vars, "ZabeteStatic_Info");
            log("Zabeteh     : rows Zabeteh=" + zabRows + " ZabeteStatic_Info=" + staticInfo);

            log("Zabeteh     : " + vars.Count + " مقدار از جداول ضابطه");
            return vars;
        }

        /// <summary>
        /// Step 1 only: request + overlay. No CI lookups, ماده ۵ static, docs, or Member.
        /// </summary>
        public static List<Dictionary<string, object>> ReadStep1(string sara, string nidProc, Action<string> log)
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

            var keys = new CaseKeys { NidProc = nidProc.Trim() };
            DumpNamed(sara, "Sh_RequestInfo", keys, vars, log, "NidProc");
            FillKeys(keys, vars);
            vars.Add(new Dictionary<string, object>
            {
                { "name", "JoinOn" },
                { "value", JoinOn },
                { "table", "join" },
                { "match", "کلید اتصال" },
            });
            log("Zabeteh     : NidWorkItem=" + (keys.NidWorkItem ?? "(خالی)")
                + " NidNosaziCode=" + (keys.NidNosaziCode ?? "(خالی)")
                + " ActiveNidZabeteh=" + (IsEmptyGuid(keys.ActiveNidZabeteh) ? "(خالی)" : keys.ActiveNidZabeteh));
            if (PermitPipeline.IsIgnoredWorkItem(keys.NidWorkItem))
                log("Zabeteh     : WorkItem " + PermitPipeline.IgnoreWorkItem + " بررسی نمی‌شود");
            if (PermitPipeline.IsPermitSample(keys.NidWorkItem))
                log("Zabeteh     : پرونده " + PermitPipeline.SampleKind + " WorkItem=" + PermitPipeline.SampleWorkItem);

            DumpZabeteh(sara, keys, vars, log);
            FillKeys(keys, vars);
            if (!IsEmptyGuid(keys.OverlayNidZabeteh))
                DumpNamed(sara, "Zabeteh_Details", keys, vars, log, "NidZabeteh");
            return vars;
        }

        /// <summary>
        /// Dump only the forms the user ticked — same as opening that form in Sara.
        /// Keys: NidNosaziCode → Base_Info.NidBase. Agreement/peace use Building='0'.
        /// </summary>
        public static List<Dictionary<string, object>> ReadSelected(string sara, string nidProc, IList<string> scopes, Action<string> log)
        {
            if (log == null) log = m => { };
            var selected = PermitScopes.Normalize(scopes);
            var vars = new List<Dictionary<string, object>>();
            if (selected.Count == 0)
            {
                log("Scope      : " + PermitScopes.MustPick);
                return vars;
            }
            if (string.IsNullOrWhiteSpace(nidProc))
            {
                log("Scope      : NidProc خالی");
                return vars;
            }
            if (string.IsNullOrWhiteSpace(sara))
            {
                log("Scope      : اتصال Sara خالی است");
                return vars;
            }

            var titles = new List<string>();
            foreach (string id in selected) titles.Add(PermitScopes.Title(id));
            log("Scope      : انتخاب = " + string.Join("، ", titles) + " — فقط همین فرم‌ها");

            var keys = new CaseKeys { NidProc = nidProc.Trim() };
            DumpNamed(sara, "Sh_RequestInfo", keys, vars, log, "NidProc");
            FillKeys(keys, vars);
            vars.Add(new Dictionary<string, object>
            {
                { "name", "JoinOn" },
                { "value", JoinOn },
                { "table", "join" },
                { "match", "کلید اتصال" },
            });
            log("Scope      : NidWorkItem=" + (keys.NidWorkItem ?? "(خالی)")
                + " NidNosaziCode=" + (keys.NidNosaziCode ?? "(خالی)")
                + " ActiveNidZabeteh=" + (IsEmptyGuid(keys.ActiveNidZabeteh) ? "(خالی)" : keys.ActiveNidZabeteh));

            DumpLandBase(sara, keys, vars, log);

            if (PermitScopes.Has(selected, PermitScopes.Zabeteh))
            {
                DumpZabeteh(sara, keys, vars, log);
                FillKeys(keys, vars);
                if (!IsEmptyGuid(keys.OverlayNidZabeteh))
                    DumpNamed(sara, "Zabeteh_Details", keys, vars, log, "NidZabeteh");
            }
            if (PermitScopes.Has(selected, PermitScopes.Solh))
            {
                DumpJoinBase(sara, "Sh_Peace", keys, vars, log, true);
                DumpJoinBase(sara, "Sh_PeaceLetter", keys, vars, log, true);
                LookupFromVars(sara, "CI_PeaceType", "CI_PeaceType", vars, log);
                DumpJoinBase(sara, "Base_Using", keys, vars, log, true);
            }
            if (PermitScopes.Has(selected, PermitScopes.Chidman))
            {
                DumpJoinBase(sara, "Base_Using", keys, vars, log, false);
                DumpJoinBase(sara, "Base_Front", keys, vars, log, false);
                LookupFromVars(sara, "CI_UsingType", "CI_UsingType", vars, log);
                LookupFromVars(sara, "CI_UsingGroup", "CI_UsingGroup", vars, log);
                LookupFromVars(sara, "CI_FrontPlace", "CI_FrontPlace", vars, log);
                LookupFromVars(sara, "CI_FrontType", "CI_FrontType", vars, log);
            }
            if (PermitScopes.Has(selected, PermitScopes.Tahlil))
            {
                AddEnumAnalysis(vars);
                DumpJoinBase(sara, "AnalysisBuilding", keys, vars, log, false);
                DumpJoinBase(sara, "AnalysisBuilding_Details", keys, vars, log, false);
                DumpJoinBase(sara, "GetAnalysisBuilding", keys, vars, log, false);
                LookupFromVars(sara, "CI_Penalty", "CI_Penalty", vars, log);
                AddMaxPenaltyDate(vars);
            }
            if (PermitScopes.Has(selected, PermitScopes.Tavafogh))
            {
                DumpJoinBase(sara, "Sh_Agreement", keys, vars, log, true);
                DumpJoinBase(sara, "Sh_AgreementLetter", keys, vars, log, true);
                LookupFromVars(sara, "CI_AgreementType", "CI_AgreementType", vars, log);
            }
            if (PermitScopes.Has(selected, PermitScopes.Commission))
                log("Scope      : کمیسیون ماده ۱۰۰ — جدول نام‌دار در این نسخه نیست. ملک می‌تواند کمیسیون نداشته باشد");
            if (PermitScopes.Has(selected, PermitScopes.Income))
                log("Scope      : درآمد — جدول نام‌دار در این نسخه نیست");

            log("Scope      : " + vars.Count + " مقدار از فرم‌های انتخاب‌شده");
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

            log("Scope      : جداول زمین/صلح/تحلیل/توافق نام‌دار");
            if (string.IsNullOrWhiteSpace(sara)) return;
            var extra = new List<string>(PermitScopes.LandTables);
            foreach (string id in PermitScopes.All)
                foreach (string t in PermitScopes.Tables(id))
                    if (!extra.Contains(t) && Array.IndexOf(SaraTables, t) < 0) extra.Add(t);
            foreach (string table in extra)
            {
                HashSet<string> cols;
                Dictionary<string, string> types;
                if (TryMeta(sara, table, out cols, out types))
                    log("Scope      : [" + table + "] OK cols=" + cols.Count);
                else
                    log("Scope      : [" + table + "] نیست یا قابل خواندن نیست");
            }
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
            public string NidWorkItem;
            public string Workflow;
            public string OverlayNidZabeteh;
            public string PlanTypeId;
            public string PlanUsingTypeId;
            public string CIZabetehId;
            public string NidZStaticInfo;
            public string NidBase;
            public string Building;
        }

        private static void FillKeys(CaseKeys k, List<Dictionary<string, object>> vars)
        {
            if (IsEmptyGuid(k.ActiveNidZabeteh))
                k.ActiveNidZabeteh = First(vars, "ActiveNidZabeteh", "NidActiveZabeteh") ?? k.ActiveNidZabeteh;
            if (string.IsNullOrEmpty(k.Pkey))
                k.Pkey = First(vars, "P_Key", "Pkey", "PKEY", "PKey", "MelkPkey", "PKEY_Melk");
            if (string.IsNullOrEmpty(k.NidZStaticInfo))
                k.NidZStaticInfo = First(vars, "NidZStatic_Info");
            if (string.IsNullOrEmpty(k.NidNosaziCode))
                k.NidNosaziCode = First(vars, "NidNosaziCode");
            if (string.IsNullOrEmpty(k.NidWorkItem))
                k.NidWorkItem = First(vars, "NidWorkItem", "WorkItem");
            if (string.IsNullOrEmpty(k.Workflow))
                k.Workflow = First(vars, "WorkflowTitel", "WorkflowTitle", "Workflow");
            if (IsEmptyGuid(k.OverlayNidZabeteh))
                k.OverlayNidZabeteh = FirstFromTable(vars, "[dbo].[Zabeteh]", "NidZabeteh")
                    ?? FirstFromTable(vars, "Zabeteh", "NidZabeteh");
            if (string.IsNullOrEmpty(k.PlanTypeId))
                k.PlanTypeId = First(vars, "CI_PlanType", "NidPlanType", "PlanType", "PlanTypeId");
            if (string.IsNullOrEmpty(k.PlanUsingTypeId))
                k.PlanUsingTypeId = First(vars, "CI_PlanUsingType", "NidPlanUsingType", "PlanUsingType");
            if (string.IsNullOrEmpty(k.CIZabetehId))
                k.CIZabetehId = First(vars, "CI_Zabeteh", "NidCIZabeteh", "NidZabetehType");
            if (string.IsNullOrEmpty(k.Building))
                k.Building = FirstFromTable(vars, "Base_NosaziCode", "Building") ?? First(vars, "Building");
            if (IsEmptyGuid(k.NidBase))
                k.NidBase = FirstFromTable(vars, "Base_Info", "NidBase") ?? First(vars, "NidBase");
            // Do not copy Zabeteh.NidZabeteh into ActiveNidZabeteh — L270 reads the request field only.
        }

        /// <summary>
        /// ماده ۵ / لایه ۸۳۶: ZabeteStatic_Info keyed by P_Key, children by NidZStatic_Info.
        /// Do not fall back to CI_PlanType — that dumps unrelated plan rows.
        /// </summary>
        private static void DumpStaticLayer(string sara, CaseKeys keys, List<Dictionary<string, object>> vars, Action<string> log)
        {
            if (string.IsNullOrEmpty(keys.Pkey))
                log("Zabeteh     : P_Key خالی — ZabeteStatic_Info (لایه ۸۳۶ / ماده ۵) برای این درخواست کلید نقشه ندارد");
            else
            {
                DumpNamed(sara, "ZabeteStatic_Info", keys, vars, log, StaticPkeyColumn, "Pkey", "PKEY", "PKey");
                FillKeys(keys, vars);
            }

            if (string.IsNullOrEmpty(keys.NidZStaticInfo) && string.IsNullOrEmpty(keys.Pkey))
                log("Zabeteh     : ZabeteStatic_Zabete/Plan رد شد — بدون NidZStatic_Info (بدون fallback روی CI_PlanType)");
            else
            {
                DumpNamed(sara, "ZabeteStatic_Zabete", keys, vars, log, StaticChildPrefer);
                DumpNamed(sara, "ZabeteStatic_Plan", keys, vars, log, StaticChildPrefer);
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

            int n = SelectWhere(cs, table, cols, types, whereCol, whereVal, vars, log, 5, null);
            log("Zabeteh     : [" + table + "] rows=" + n + " via " + whereCol);
        }

        /// <summary>
        /// Overlay is stored per property code, not per NidProc.
        /// User query: SELECT TOP 1 * FROM Zabeteh a JOIN Sh_RequestInfo b ON a.NidNosaziCode=b.NidNosaziCode
        /// </summary>
        private static void DumpZabeteh(string cs, CaseKeys keys, List<Dictionary<string, object>> vars, Action<string> log)
        {
            HashSet<string> cols;
            Dictionary<string, string> types;
            if (!TryMeta(cs, "Zabeteh", out cols, out types))
            {
                log("Zabeteh     : جدول Zabeteh در Sara نیست یا قابل خواندن نیست");
                return;
            }

            string order = cols.Contains("DateZabeteh") ? "[DateZabeteh] DESC" : null;
            int byNosazi = 0;
            int byActive = 0;

            if (cols.Contains("NidNosaziCode") && !string.IsNullOrEmpty(keys.NidNosaziCode))
            {
                byNosazi = SelectWhere(cs, "Zabeteh", cols, types, "NidNosaziCode", keys.NidNosaziCode, vars, log, 10, order);
                log("Zabeteh     : [Zabeteh] rows=" + byNosazi + " via NidNosaziCode (join با درخواست)");
            }
            else
                log("Zabeteh     : NidNosaziCode روی درخواست خالی است — join کاربر اجرا نشد");

            if (byNosazi > 0 && IsEmptyGuid(keys.ActiveNidZabeteh))
            {
                string nidZ = FirstFromTable(vars, "[dbo].[Zabeteh]", "NidZabeteh");
                string plan = FirstFromTable(vars, "[dbo].[Zabeteh]", "CI_PlanType");
                log("Zabeteh     : روکش ملک NidZabeteh=" + (nidZ ?? "(خالی)")
                    + " CI_PlanType=" + (plan ?? "(خالی)")
                    + " — ضابطه هست؛ Active خالی یعنی صلح روی این درخواست اعمال نشده (همه ملک‌ها صلح ندارند)");
            }

            if (cols.Contains("NidZabeteh") && !IsEmptyGuid(keys.ActiveNidZabeteh))
            {
                byActive = SelectWhere(cs, "Zabeteh", cols, types, "NidZabeteh", keys.ActiveNidZabeteh, vars, log, 3, null);
                log("Zabeteh     : [Zabeteh] rows=" + byActive + " via NidZabeteh=ActiveNidZabeteh (روکش اعلام‌شده)");
                string latest = FirstFromTable(vars, "[dbo].[Zabeteh]", "NidZabeteh");
                if (!IsEmptyGuid(latest) && !string.Equals(latest, keys.ActiveNidZabeteh, StringComparison.OrdinalIgnoreCase))
                    log("Zabeteh     : آخرین NidZabeteh=" + latest + " با ActiveNidZabeteh یکی نیست — اعلام‌شده همان Active است");
            }

            if (byNosazi == 0 && byActive == 0)
            {
                log("Zabeteh     : هیچ ردیف Zabeteh با NidNosaziCode/Active پیدا نشد — با NidProc جستجو نمی‌شود");
            }
        }

        /// <summary>
        /// Land row: Base_Info ⋈ Base_NosaziCode. Agreement/peace use Building='0'.
        /// </summary>
        private static void DumpLandBase(string cs, CaseKeys keys, List<Dictionary<string, object>> vars, Action<string> log)
        {
            HashSet<string> infoCols, nosaziCols;
            Dictionary<string, string> infoTypes, nosaziTypes;
            if (!TryMeta(cs, "Base_Info", out infoCols, out infoTypes))
            {
                log("Scope      : جدول Base_Info در Sara نیست یا قابل خواندن نیست");
                return;
            }
            bool hasNosazi = TryMeta(cs, "Base_NosaziCode", out nosaziCols, out nosaziTypes);
            if (!infoCols.Contains("NidNosaziCode") || string.IsNullOrEmpty(keys.NidNosaziCode))
            {
                log("Scope      : NidNosaziCode برای Base_Info خالی است");
                return;
            }

            var select = new List<string>();
            foreach (string col in infoCols)
            {
                string t;
                infoTypes.TryGetValue(col, out t);
                if (SkipTypes.Contains((t ?? "").ToLowerInvariant())) continue;
                select.Add("bi.[" + col.Replace("]", "]]") + "]");
                if (select.Count >= 40) break;
            }
            if (hasNosazi)
            {
                foreach (string col in new[] { "Building", "District", "Region", "Block", "House" })
                {
                    if (!nosaziCols.Contains(col)) continue;
                    select.Add("ni.[" + col + "]");
                }
            }
            if (select.Count == 0) return;

            string sql = "SELECT TOP (10) " + string.Join(", ", select)
                + " FROM [dbo].[Base_Info] bi";
            if (hasNosazi && nosaziCols.Contains("NidNosaziCode"))
            {
                sql += " INNER JOIN [dbo].[Base_NosaziCode] ni ON bi.[NidNosaziCode] = ni.[NidNosaziCode]";
                sql += " WHERE CAST(bi.[NidNosaziCode] AS NVARCHAR(50))=@t";
                if (nosaziCols.Contains("Building"))
                    sql += " ORDER BY CASE WHEN CAST(ni.[Building] AS NVARCHAR(20))='0' THEN 0 ELSE 1 END, ni.[Building]";
            }
            else
                sql += " WHERE CAST(bi.[NidNosaziCode] AS NVARCHAR(50))=@t";

            int rows = 0;
            string firstBase = null;
            string firstBuilding = null;
            try
            {
                using (var c = new SqlConnection(cs))
                using (var cmd = new SqlCommand(sql, c) { CommandTimeout = 45 })
                {
                    cmd.Parameters.AddWithValue("@t", keys.NidNosaziCode);
                    c.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            rows++;
                            AddRow(r, "[dbo].[Base_Info]", vars);
                            if (hasNosazi)
                            {
                                for (int i = 0; i < r.FieldCount; i++)
                                {
                                    if (r.IsDBNull(i)) continue;
                                    string col = r.GetName(i);
                                    if (col.Equals("Building", StringComparison.OrdinalIgnoreCase)
                                        || col.Equals("District", StringComparison.OrdinalIgnoreCase)
                                        || col.Equals("Region", StringComparison.OrdinalIgnoreCase)
                                        || col.Equals("Block", StringComparison.OrdinalIgnoreCase)
                                        || col.Equals("House", StringComparison.OrdinalIgnoreCase))
                                    {
                                        string val = Trunc(Convert.ToString(r.GetValue(i), CultureInfo.InvariantCulture), 120);
                                        if (string.IsNullOrWhiteSpace(val)) continue;
                                        vars.Add(new Dictionary<string, object>
                                        {
                                            { "name", col },
                                            { "value", val },
                                            { "table", "[dbo].[Base_NosaziCode]" },
                                            { "match", Flag(col) },
                                        });
                                        if (firstBuilding == null && col.Equals("Building", StringComparison.OrdinalIgnoreCase))
                                            firstBuilding = val;
                                    }
                                }
                            }
                            if (firstBase == null)
                            {
                                try
                                {
                                    int ord = r.GetOrdinal("NidBase");
                                    if (!r.IsDBNull(ord))
                                        firstBase = Convert.ToString(r.GetValue(ord), CultureInfo.InvariantCulture);
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log("Scope      : Base_Info join — " + FirstLine(ex.Message));
            }
            if (!IsEmptyGuid(firstBase)) keys.NidBase = firstBase;
            if (!string.IsNullOrEmpty(firstBuilding)) keys.Building = firstBuilding;
            FillKeys(keys, vars);
            log("Scope      : [Base_Info] rows=" + rows
                + " NidBase=" + (keys.NidBase ?? "(خالی)")
                + " Building=" + (keys.Building ?? "(خالی)")
                + " — توافق/صلح روی Building=0");
        }

        /// <summary>
        /// Child table via NidBase ← Base_Info.NidNosaziCode. Building='0' for agreement/peace.
        /// AnalysisBuilding_Details may join parent AnalysisBuilding when it has no NidBase.
        /// </summary>
        private static int DumpJoinBase(string cs, string table, CaseKeys keys, List<Dictionary<string, object>> vars, Action<string> log, bool buildingZero)
        {
            if (string.IsNullOrEmpty(keys.NidNosaziCode))
            {
                log("Scope      : " + table + " — NidNosaziCode خالی");
                return 0;
            }
            HashSet<string> cols;
            Dictionary<string, string> types;
            if (!TryMeta(cs, table, out cols, out types))
            {
                log("Scope      : جدول " + table + " در Sara نیست یا قابل خواندن نیست");
                return 0;
            }

            var select = new List<string>();
            foreach (string col in cols)
            {
                string t;
                types.TryGetValue(col, out t);
                if (SkipTypes.Contains((t ?? "").ToLowerInvariant())) continue;
                select.Add("t.[" + col.Replace("]", "]]") + "]");
                if (select.Count >= 50) break;
            }
            if (select.Count == 0) return 0;

            string sql;
            string via;
            if (cols.Contains("NidBase"))
            {
                sql = "SELECT TOP (20) " + string.Join(", ", select)
                    + " FROM [dbo].[" + table.Replace("]", "") + "] t"
                    + " INNER JOIN [dbo].[Base_Info] bi ON t.[NidBase] = bi.[NidBase]";
                via = "NidBase←Base_Info.NidNosaziCode";
                if (buildingZero)
                {
                    sql += " INNER JOIN [dbo].[Base_NosaziCode] ni ON bi.[NidNosaziCode] = ni.[NidNosaziCode] AND CAST(ni.[Building] AS NVARCHAR(20))='0'";
                    via += " Building=0";
                }
                sql += " WHERE CAST(bi.[NidNosaziCode] AS NVARCHAR(50))=@t";
            }
            else if (cols.Contains("NidAnalysisBuilding"))
            {
                sql = "SELECT TOP (20) " + string.Join(", ", select)
                    + " FROM [dbo].[" + table.Replace("]", "") + "] t"
                    + " INNER JOIN [dbo].[AnalysisBuilding] ab ON t.[NidAnalysisBuilding] = ab.[NidAnalysisBuilding]"
                    + " INNER JOIN [dbo].[Base_Info] bi ON ab.[NidBase] = bi.[NidBase]"
                    + " WHERE CAST(bi.[NidNosaziCode] AS NVARCHAR(50))=@t";
                via = "NidAnalysisBuilding←AnalysisBuilding.NidBase";
            }
            else if (cols.Contains("NidPeace"))
            {
                sql = "SELECT TOP (20) " + string.Join(", ", select)
                    + " FROM [dbo].[" + table.Replace("]", "") + "] t"
                    + " INNER JOIN [dbo].[Sh_Peace] sp ON t.[NidPeace] = sp.[NidPeace]"
                    + " INNER JOIN [dbo].[Base_Info] bi ON sp.[NidBase] = bi.[NidBase]";
                via = "NidPeace←Sh_Peace.NidBase";
                if (buildingZero)
                {
                    sql += " INNER JOIN [dbo].[Base_NosaziCode] ni ON bi.[NidNosaziCode] = ni.[NidNosaziCode] AND CAST(ni.[Building] AS NVARCHAR(20))='0'";
                    via += " Building=0";
                }
                sql += " WHERE CAST(bi.[NidNosaziCode] AS NVARCHAR(50))=@t";
            }
            else if (cols.Contains("NidAgreement"))
            {
                sql = "SELECT TOP (20) " + string.Join(", ", select)
                    + " FROM [dbo].[" + table.Replace("]", "") + "] t"
                    + " INNER JOIN [dbo].[Sh_Agreement] sa ON t.[NidAgreement] = sa.[NidAgreement]"
                    + " INNER JOIN [dbo].[Base_Info] bi ON sa.[NidBase] = bi.[NidBase]";
                via = "NidAgreement←Sh_Agreement.NidBase";
                if (buildingZero)
                {
                    sql += " INNER JOIN [dbo].[Base_NosaziCode] ni ON bi.[NidNosaziCode] = ni.[NidNosaziCode] AND CAST(ni.[Building] AS NVARCHAR(20))='0'";
                    via += " Building=0";
                }
                sql += " WHERE CAST(bi.[NidNosaziCode] AS NVARCHAR(50))=@t";
            }
            else if (!IsEmptyGuid(keys.NidBase) && cols.Contains("NidZabeteh"))
            {
                return SelectWhere(cs, table, cols, types, "NidZabeteh", keys.OverlayNidZabeteh, vars, log, 10, null);
            }
            else
            {
                log("Scope      : " + table + " — ستون NidBase/NidAnalysisBuilding برای join زمین نیست");
                return 0;
            }

            int rows = 0;
            try
            {
                using (var c = new SqlConnection(cs))
                using (var cmd = new SqlCommand(sql, c) { CommandTimeout = 45 })
                {
                    cmd.Parameters.AddWithValue("@t", keys.NidNosaziCode);
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
                log("Scope      : " + table + " query — " + FirstLine(ex.Message));
            }
            log("Scope      : [" + table + "] rows=" + rows + " via " + via);
            return rows;
        }

        private static void LookupFromVars(string cs, string table, string col, List<Dictionary<string, object>> vars, Action<string> log)
        {
            var ids = new List<string>();
            foreach (var v in vars)
            {
                object n, val;
                if (!v.TryGetValue("name", out n) || !v.TryGetValue("value", out val)) continue;
                if (!string.Equals(Convert.ToString(n), col, StringComparison.OrdinalIgnoreCase)) continue;
                string s = Convert.ToString(val, CultureInfo.InvariantCulture);
                if (string.IsNullOrWhiteSpace(s) || IsEmptyGuid(s)) continue;
                bool dup = false;
                foreach (string x in ids)
                    if (string.Equals(x, s, StringComparison.OrdinalIgnoreCase)) { dup = true; break; }
                if (!dup) ids.Add(s);
                if (ids.Count >= 8) break;
            }
            foreach (string id in ids)
                LookupById(cs, table, id, vars, log);
        }

        private static void AddEnumAnalysis(List<Dictionary<string, object>> vars)
        {
            vars.Add(new Dictionary<string, object>
            {
                { "name", "Parvaneh" }, { "value", "1" },
                { "table", "EumAnalysisBuildingType" }, { "match", "تحلیل پروانه" },
            });
            vars.Add(new Dictionary<string, object>
            {
                { "name", "Foul" }, { "value", "2" },
                { "table", "EumAnalysisBuildingType" }, { "match", "تعیین خلاف" },
            });
            vars.Add(new Dictionary<string, object>
            {
                { "name", "MovafeghatOsooli" }, { "value", "3" },
                { "table", "EumAnalysisBuildingType" }, { "match", "تحلیل موافقت اصولی" },
            });
            vars.Add(new Dictionary<string, object>
            {
                { "name", "AnaliysParvaneh_Date" }, { "value", "max(PenaltyDate)" },
                { "table", "AnalysisBuilding" }, { "match", "تاریخ تحلیل پروانه" },
            });
        }

        private static void AddMaxPenaltyDate(List<Dictionary<string, object>> vars)
        {
            string max = null;
            foreach (var v in vars)
            {
                object n, val, t;
                if (!v.TryGetValue("name", out n) || !v.TryGetValue("value", out val)) continue;
                if (!string.Equals(Convert.ToString(n), "PenaltyDate", StringComparison.OrdinalIgnoreCase)) continue;
                if (v.TryGetValue("table", out t))
                {
                    string table = Convert.ToString(t) ?? "";
                    if (table.IndexOf("AnalysisBuilding", StringComparison.OrdinalIgnoreCase) < 0
                        && table.IndexOf("Penalty", StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                }
                string s = Convert.ToString(val, CultureInfo.InvariantCulture);
                if (string.IsNullOrWhiteSpace(s)) continue;
                if (max == null || string.Compare(s, max, StringComparison.OrdinalIgnoreCase) > 0)
                    max = s;
            }
            if (max == null) return;
            foreach (var v in vars)
            {
                object n, t;
                if (!v.TryGetValue("name", out n) || !v.TryGetValue("table", out t)) continue;
                if (!string.Equals(Convert.ToString(n), "AnaliysParvaneh_Date", StringComparison.OrdinalIgnoreCase)) continue;
                if (Convert.ToString(t).IndexOf("AnalysisBuilding", StringComparison.OrdinalIgnoreCase) < 0) continue;
                v["value"] = max;
                return;
            }
            vars.Add(new Dictionary<string, object>
            {
                { "name", "AnaliysParvaneh_Date" },
                { "value", max },
                { "table", "AnalysisBuilding" },
                { "match", "تاریخ تحلیل پروانه" },
            });
        }

        private static string ValueFor(CaseKeys k, string col)
        {
            if (col.Equals("NidProc", StringComparison.OrdinalIgnoreCase)) return k.NidProc;
            if (col.IndexOf("ActiveNidZabeteh", StringComparison.OrdinalIgnoreCase) >= 0) return k.ActiveNidZabeteh;
            if (col.Equals("NidZabeteh", StringComparison.OrdinalIgnoreCase))
                return !IsEmptyGuid(k.OverlayNidZabeteh) ? k.OverlayNidZabeteh : k.ActiveNidZabeteh;
            if (col.Equals("P_Key", StringComparison.OrdinalIgnoreCase)
                || col.IndexOf("Pkey", StringComparison.OrdinalIgnoreCase) >= 0
                || col.Equals("PKEY", StringComparison.OrdinalIgnoreCase))
                return k.Pkey;
            if (col.Equals("NidZStatic_Info", StringComparison.OrdinalIgnoreCase)) return k.NidZStaticInfo;
            if (col.Equals("NidNosaziCode", StringComparison.OrdinalIgnoreCase)) return k.NidNosaziCode;
            if (col.Equals("NidBase", StringComparison.OrdinalIgnoreCase)) return k.NidBase;
            if (col.Equals("Building", StringComparison.OrdinalIgnoreCase)) return k.Building;
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
            int n = SelectWhere(cs, table, cols, types, pk, id, vars, log, 5, null);
            log("Zabeteh     : [" + table + "] lookup " + pk + "=" + id + " rows=" + n);
        }

        private static int SelectWhere(string cs, string table, HashSet<string> cols, Dictionary<string, string> types, string whereCol, string whereVal, List<Dictionary<string, object>> vars, Action<string> log, int top, string orderBy)
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
            if (top < 1) top = 5;
            string sql = "SELECT TOP (" + top + ") " + string.Join(", ", select) + " FROM [dbo].[" + table.Replace("]", "") + "]"
                + " WHERE CAST([" + whereCol.Replace("]", "") + "] AS NVARCHAR(50))=@t";
            if (!string.IsNullOrWhiteSpace(orderBy))
                sql += " ORDER BY " + orderBy;
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
            if (col.IndexOf("NidBase", StringComparison.OrdinalIgnoreCase) >= 0) return "NidBase ملک";
            if (col.Equals("Building", StringComparison.OrdinalIgnoreCase)) return "کد ساختمان (توافق/صلح=0)";
            if (col.IndexOf("Peace", StringComparison.OrdinalIgnoreCase) >= 0 || col.IndexOf("Solh", StringComparison.OrdinalIgnoreCase) >= 0) return "صلح";
            if (col.IndexOf("Agreement", StringComparison.OrdinalIgnoreCase) >= 0 || col.IndexOf("Tavafogh", StringComparison.OrdinalIgnoreCase) >= 0) return "توافق";
            if (col.IndexOf("Penalty", StringComparison.OrdinalIgnoreCase) >= 0 || col.IndexOf("AnalysisBuilding", StringComparison.OrdinalIgnoreCase) >= 0) return "تحلیل";
            if (col.IndexOf("Front", StringComparison.OrdinalIgnoreCase) >= 0 || col.IndexOf("UsingArea", StringComparison.OrdinalIgnoreCase) >= 0) return "چیدمان";
            if (col.IndexOf("Takhalof", StringComparison.OrdinalIgnoreCase) >= 0 || col.IndexOf("Tahlil", StringComparison.OrdinalIgnoreCase) >= 0) return "تحلیل";
            if (col.IndexOf("Commission", StringComparison.OrdinalIgnoreCase) >= 0 || col.IndexOf("Jarime", StringComparison.OrdinalIgnoreCase) >= 0) return "کمیسیون ماده ۱۰۰";
            if (col.IndexOf("Daramad", StringComparison.OrdinalIgnoreCase) >= 0 || col.Equals("Income", StringComparison.OrdinalIgnoreCase)) return "درآمد";
            if (col.IndexOf("Zabeteh", StringComparison.OrdinalIgnoreCase) >= 0) return "ضابطه";
            if (col.IndexOf("PlanType", StringComparison.OrdinalIgnoreCase) >= 0) return "طرح";
            if (col.IndexOf("PlanUsing", StringComparison.OrdinalIgnoreCase) >= 0 || col.IndexOf("Karbari", StringComparison.OrdinalIgnoreCase) >= 0) return "کاربری";
            if (col.Equals("P_Key", StringComparison.OrdinalIgnoreCase)
                || col.IndexOf("Pkey", StringComparison.OrdinalIgnoreCase) >= 0)
                return "P_Key ملک / ماده ۵";
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
