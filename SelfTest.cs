using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RuleTrace
{
    /// <summary>Headless checks. Run: RuleTrace.exe --self-test</summary>
    internal static class SelfTest
    {
        public static int Run()
        {
            int fail = 0;
            fail += ChidmanSolhGuard();
            fail += ChidmanRealSolhStop();
            fail += RelatedClassesSolh();
            fail += BannerNoRewrite();
            fail += JsonRoundtrip();
            fail += HistoryImageSql();
            fail += SolhNidExtract();
            fail += ZabetehNamedTables();
            fail += PickScope();
            fail += RuleDocsCatalog();
            fail += PasteSummaryFilter();
            fail += WebUiEmbedded();
            fail += WebHostRoundtrip();
            Console.WriteLine(fail == 0 ? "SELFTEST OK" : "SELFTEST FAIL " + fail);
            return fail == 0 ? 0 : 1;
        }

        private static int ChidmanSolhGuard()
        {
            var log = new List<string>();
            var sources = new List<MemberSource>
            {
                new MemberSource
                {
                    NidClass = 344,
                    NidMember = 1296,
                    Name = "Run",
                    Meta = "Solh",
                    Code = "Public Sub Run()\r\n  If True Then InsertChidman()\r\nEnd Sub\r\n",
                },
                new MemberSource
                {
                    NidClass = 342,
                    NidMember = 1288,
                    Name = "Chidman",
                    Meta = "ZabetehConvert",
                    Code =
                        "Public Sub InsertChidman()\r\n" +
                        "  If Solh Then\r\n" +
                        "    Exit Sub\r\n" +
                        "  End If\r\n" +
                        "  AddError(\"Chidman\", \"اعلام چیدمان\")\r\n" +
                        "  Calc_Chandganeh = 1\r\n" +
                        "End Sub\r\n",
                },
            };

            ChidmanAnalyzer.Report(sources, new List<TraceEvent>(), 1288, log.Add);
            string all = string.Join("\n", log);
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine(all);

            int fail = 0;
            fail += Expect(all, "Chidman      :", "summary prefix");
            fail += Expect(all, "342", "ZabetehConvert class");
            fail += Expect(all, "344", "Solh class");
            fail += Expect(all, "calls InsertChidman", "Solh calls chidman in 342");
            fail += Expect(all, "InsertChidman", "method name");
            fail += Expect(all, "If Solh Then", "Solh guard");
            fail += Expect(all, "Exit Sub", "early exit on Solh");
            fail += Expect(all, "AddError", "chidman AddError");
            fail += Expect(all, "Solh/صلح guards", "guard section");
            if (Regex.IsMatch(all, @"Sub/Function in this member:.*\bEnd\b"))
            {
                Console.Error.WriteLine("FAIL: End Sub parsed as a method");
                fail++;
            }
            if (all.IndexOf("vbc", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Console.Error.WriteLine("FAIL: static analysis still mentions vbc");
                fail++;
            }
            return fail;
        }

        private static int ChidmanRealSolhStop()
        {
            var log = new List<string>();
            var sources = new List<MemberSource>
            {
                new MemberSource
                {
                    NidClass = 344,
                    NidMember = 1296,
                    Name = "Run",
                    Meta = "Solh",
                    Code =
                        "Public Sub Run()\r\n" +
                        "  If Info8.GetRuleResultPeaceParameter().IsCallFromCrowd = True Then\r\n" +
                        "    Iscrowd = True\r\n" +
                        "  End If\r\n" +
                        "  Info8.AddError(BIZ.SA.EumErrorAction.Stop, \"صلحنامه\", \"به دلیل عدم اعلام ضابطه امکان محاسبه صلحنامه نمی باشد\")\r\n" +
                        "  InsertChidman()\r\n" +
                        "End Sub\r\n",
                },
                new MemberSource
                {
                    NidClass = 342,
                    NidMember = 1288,
                    Name = "Run",
                    Meta = "ZabetehConvert",
                    Code =
                        "Public Sub Run()\r\n" +
                        "  logfileFJ(\"noise\")\r\n" +
                        "  if tmpDto2.UsingArea>0 Then InsertChidman(tmpDto2)\r\n" +
                        "End Sub\r\n" +
                        "Public Sub InsertChidman(dto)\r\nEnd Sub\r\n",
                },
                new MemberSource
                {
                    NidClass = 336,
                    NidMember = 1148,
                    Name = "Run",
                    Meta = "Rule",
                    Code = "Public Sub Run()\r\n  logfileFJ(\"x\")\r\n  Info8.AddError(BIZ.SA.EumErrorAction.Stop, \"ضابطه\", \"ادرس برای ارسال به 137 معتبر نمی باشد\")\r\nEnd Sub\r\n",
                },
            };
            ChidmanAnalyzer.Report(sources, new List<TraceEvent>(), 1288, log.Add);
            string all = string.Join("\n", log);
            Console.WriteLine(all);
            int fail = 0;
            fail += Expect(all, "یافته", "findings header");
            fail += Expect(all, "صلحنامه", "solh stop key");
            fail += Expect(all, "عدم اعلام ضابطه", "missing regulation message");
            fail += Expect(all, "UsingArea", "InsertChidman UsingArea gate");
            fail += Expect(all, "1296", "Solh Run member");
            fail += Expect(all, "calls InsertChidman", "Solh calls InsertChidman");
            fail += Expect(all, "Solh/Tavafogh صدا می‌زند", "cross-class callers first");
            if (all.IndexOf("ادرس برای ارسال", StringComparison.Ordinal) >= 0)
            {
                Console.Error.WriteLine("FAIL: generic Rule/1148 ضابطه error leaked into findings");
                fail++;
            }
            if (all.IndexOf("calls logfileFJ", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Console.Error.WriteLine("FAIL: caller list still includes logfileFJ noise");
                fail++;
            }
            return fail;
        }

        private static int RelatedClassesSolh()
        {
            int[] rel = FormulaEngine.RelatedNidClasses(344);
            int fail = 0;
            foreach (int need in new[] { 336, 342, 344, 345, 338, 340, 335, 337, 432 })
            {
                bool ok = false;
                foreach (int n in rel) if (n == need) ok = true;
                if (!ok)
                {
                    Console.Error.WriteLine("FAIL: RelatedNidClasses(344) missing " + need);
                    fail++;
                }
            }
            if (FormulaEngine.ClassName(342) != "ZabetehConvert")
            {
                Console.Error.WriteLine("FAIL: class 342 should be ZabetehConvert, got " + FormulaEngine.ClassName(342));
                fail++;
            }
            return fail;
        }

        private static int BannerNoRewrite()
        {
            if (BuildInfo.Banner.IndexOf("بازنویسی", StringComparison.OrdinalIgnoreCase) < 0)
            {
                Console.Error.WriteLine("FAIL: banner does not describe no-VB-rewrite architecture: " + BuildInfo.Banner);
                return 1;
            }
            if (BuildInfo.Label.IndexOf("pick-scope", StringComparison.OrdinalIgnoreCase) < 0)
            {
                Console.Error.WriteLine("FAIL: BuildInfo.Label should be v23c-pick-scope, got " + BuildInfo.Label);
                return 1;
            }
            return 0;
        }

        private static int SolhNidExtract()
        {
            const string run =
                "Public Sub Run()\r\n" +
                "  Dim Masahat As Double = 0\r\n" +
                "  If Info8.GetRequest.Info.ActiveNidZabeteh = Guid.Empty Then\r\n" +
                "    Info8.AddError(BIZ.SA.EumErrorAction.Stop, \"صلحنامه\", \"به دلیل عدم اعلام ضابطه امکان محاسبه صلحنامه نمی باشد\")\r\n" +
                "    Exit Function\r\n" +
                "  End If\r\n" +
                "End Sub\r\n";
            var hit = SolhNidDebug.ExtractStop(run, "عدم اعلام ضابطه");
            int fail = 0;
            if (hit == null) return FailMsg("solh stop block");
            fail += hit.Line == 4 ? 0 : FailMsg("stop line");
            fail += Expect(hit.Key, "صلحنامه", "stop key");
            fail += Expect(hit.Block, "ActiveNidZabeteh", "if ActiveNidZabeteh");
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            SolhNidDebug.CollectDimNames(run, names);
            SolhNidDebug.CollectNames(hit.Block, names);
            fail += names.Contains("Masahat") ? 0 : FailMsg("dim Masahat");
            fail += names.Contains("ActiveNidZabeteh") ? 0 : FailMsg("ActiveNidZabeteh ident");
            var logs = new List<string>();
            var empty = SolhNidDebug.Run("", "", "", new List<MemberSource>
            {
                new MemberSource { NidClass = 344, NidMember = 1296, Name = "Run", Code = run },
            }, logs.Add);
            fail += Expect(Convert.ToString(empty["diagnosis"]), "Zabeteh", "diagnosis names dbo.Zabeteh");
            fail += Expect(Convert.ToString(empty["stopBlock"]), "عدم اعلام", "stopBlock returned");
            fail += logs.Any(l => l.IndexOf("توقف زنده", StringComparison.Ordinal) >= 0) ? 0 : FailMsg("empty Active logs fired L270");
            fail += logs.Any(l => l.IndexOf("توقف در Member", StringComparison.Ordinal) >= 0) ? FailMsg("old 'توقف در Member' wording") : 0;
            fail += Expect(SolhNidDebug.L270Headline(true, 270, null), "توقف زنده", "empty Active headline");
            fail += Expect(SolhNidDebug.L270Headline(false, 270, "EEA1F974-CC70-44CB-8386-21B8AAAA4B31"), "سورس است", "filled Active headline");
            return fail;
        }

        private static int ZabetehNamedTables()
        {
            int fail = 0;
            string joined = string.Join(",", ZabetehCase.SaraTables);
            foreach (string t in new[] { "Sh_RequestInfo", "Zabeteh", "Zabeteh_Details", "CI_PlanType", "CI_PlanUsingType", "CI_Zabeteh", "ZabeteStatic_Info", "ZabeteStatic_Zabete", "ZabeteStatic_Plan" })
                fail += Expect(joined, t, "named table " + t);
            fail += Expect(ZabetehCase.DocumentCatalog, "DbRuleEngeinDocument", "document catalog");
            fail += ZabetehCase.IsEmptyGuid(null) ? 0 : FailMsg("null guid empty");
            fail += ZabetehCase.IsEmptyGuid("") ? 0 : FailMsg("blank guid empty");
            fail += ZabetehCase.IsEmptyGuid("00000000-0000-0000-0000-000000000000") ? 0 : FailMsg("zero guid empty");
            fail += ZabetehCase.IsEmptyGuid("Guid.Empty") ? 0 : FailMsg("Guid.Empty token");
            fail += ZabetehCase.IsEmptyGuid("FA77A442-29CD-4DDC-ADEA-A3D3A6183F28") ? FailMsg("real nid should not be empty") : 0;
            string cs = ZabetehCase.WithCatalog(
                "Server=tcp:172.16.10.232;Database=DbRuleEngein;User Id=debugger;Password=x",
                ZabetehCase.DocumentCatalog);
            fail += Expect(cs, "DbRuleEngeinDocument", "WithCatalog switches InitialCatalog");
            if (cs.IndexOf("Database=DbRuleEngein;", StringComparison.OrdinalIgnoreCase) >= 0
                && cs.IndexOf("DbRuleEngeinDocument", StringComparison.OrdinalIgnoreCase) < 0)
            {
                Console.Error.WriteLine("FAIL: WithCatalog left RuleEngine catalog");
                fail++;
            }
            var log = new List<string>();
            var vars = ZabetehCase.Read("", "", "FA77A442-29CD-4DDC-ADEA-A3D3A6183F28", log.Add);
            fail += vars.Count == 0 ? 0 : FailMsg("no vars without Sara");
            fail += Expect(string.Join("\n", log), "اتصال Sara خالی", "empty Sara log");
            fail += Expect(ZabetehCase.JoinSql, "NidNosaziCode", "join sql nosazi");
            fail += Expect(ZabetehCase.JoinSql, "INNER JOIN", "join sql inner");
            fail += Expect(ZabetehCase.JoinOn, "NidNosaziCode", "join on nosazi not nidproc");
            if (ZabetehCase.JoinSql.IndexOf("a.NidProc", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Console.Error.WriteLine("FAIL: Zabeteh join must not be on a.NidProc");
                fail++;
            }
            fail += Expect(PermitPipeline.SampleWorkItem, "300002275", "permit sample workitem");
            fail += Expect(PermitPipeline.IgnoreWorkItem, "5298603", "ignored non-permit workitem");
            fail += Expect(PermitPipeline.PathFa, "ضابطه", "permit path starts with zabeteh");
            fail += Expect(PermitPipeline.PathFa, "درآمد", "permit path ends with income");
            fail += PermitPipeline.IsIgnoredWorkItem("5298603") ? 0 : FailMsg("ignore 5298603");
            fail += PermitPipeline.IsPermitSample("300002275") ? 0 : FailMsg("sample 300002275");
            fail += PermitPipeline.IsIgnoredWorkItem("300002275") ? FailMsg("permit sample not ignored") : 0;
            var skipLog = new List<string>();
            string skip = PermitPipeline.Report(new List<MemberSource>(), new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "name", "NidWorkItem" }, { "value", "5298603" }, { "table", "Sh_RequestInfo" } },
            }, skipLog.Add);
            fail += Expect(skip, "بررسی نمی‌شود", "skip diagnosis");
            fail += skipLog.Any(l => l.IndexOf("صلح نیست", StringComparison.Ordinal) >= 0) ? 0 : FailMsg("skip log");
            var overlayLog = new List<string>();
            var overlaySources = new List<MemberSource>
            {
                new MemberSource { NidClass = 338, NidMember = 1, Name = "Tahlil" },
                new MemberSource { NidClass = 340, NidMember = 2, Name = "Commission" },
                new MemberSource { NidClass = 335, NidMember = 3, Name = "Fine" },
                new MemberSource { NidClass = 337, NidMember = 4, Name = "Income" },
            };
            string unannounced = PermitPipeline.Report(overlaySources, new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "name", "NidWorkItem" }, { "value", "300002275" }, { "table", "Sh_RequestInfo" } },
                new Dictionary<string, object> { { "name", "NidNosaziCode" }, { "value", "bce2f9e5-f6bf-4e13-882f-804048fad548" }, { "table", "Sh_RequestInfo" } },
                new Dictionary<string, object> { { "name", "NidZabeteh" }, { "value", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee" }, { "table", "[dbo].[Zabeteh]" } },
            }, overlayLog.Add);
            fail += Expect(unannounced, "صلح ندارند", "solh not required for every property");
            fail += Expect(unannounced, "هست", "overlay present");
            if (unannounced.IndexOf("پیدا نشد", StringComparison.Ordinal) >= 0)
            {
                Console.Error.WriteLine("FAIL: unannounced overlay must not say zabeteh missing");
                fail++;
            }
            fail += overlayLog.Any(l => l.IndexOf("صلح ندارند", StringComparison.Ordinal) >= 0) ? 0 : FailMsg("permit peace line optional solh");
            var stepOverlay = PermitSteps.ClassifyZabeteh("300002275", "", "3fd00472-04da-4b5b-8ca8-701d9e82c4c0", "24");
            fail += stepOverlay.Status == "PASS" ? 0 : FailMsg("step1 pass when overlay exists even if Active empty");
            fail += Expect(stepOverlay.Verdict, "ضابطه ملک هست", "step1 overlay is enough");
            fail += Expect(stepOverlay.NextAction, "اجباری نیست", "solh not mandatory");
            var stepSolhSkip = PermitSteps.ClassifySolh("300002275", "", false);
            fail += stepSolhSkip.Status == "SKIP" ? 0 : FailMsg("step2 skip when no solh record");
            fail += Expect(stepSolhSkip.Verdict, "صلح ندارد", "step2 no solh");
            fail += PermitSteps.HasSolhRecord(new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "name", "NidPeace" }, { "value", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee" }, { "table", "[dbo].[Sh_Peace]" } },
            }) ? 0 : FailMsg("Sh_Peace counts as solh record");
            fail += PermitSteps.HasSolhRecord(new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "name", "NidZabeteh" }, { "value", "3fd00472-04da-4b5b-8ca8-701d9e82c4c0" }, { "table", "[dbo].[Zabeteh]" } },
            }) ? FailMsg("zabeteh overlay is not a solh record") : 0;
            var stepSolhFail = PermitSteps.ClassifySolh("300002275", "", true);
            fail += stepSolhFail.Status == "FAIL" ? 0 : FailMsg("step2 fail only if solh record and Active empty");
            var stepPass = PermitSteps.ClassifyZabeteh("300002275", "EEA1F974-CC70-44CB-8386-21B8AAAA4B31", "3fd00472-04da-4b5b-8ca8-701d9e82c4c0", "24");
            fail += stepPass.Status == "PASS" ? 0 : FailMsg("step1 pass when Active set");
            var stepSkip = PermitSteps.ClassifyZabeteh("5298603", "", "", "");
            fail += stepSkip.Status == "SKIP" ? 0 : FailMsg("step1 skip 5298603");
            var emptyStepLog = new List<string>();
            var emptyPack = PermitSteps.RunUntilFail("", "", "E3BB36F6-1D34-426B-BFA1-526A05C452BE", emptyStepLog.Add);
            fail += Convert.ToString(emptyPack["status"]) == "FAIL" ? 0 : FailMsg("empty sara step1 fail");
            fail += Convert.ToInt32(emptyPack["step"]) == 1 ? 0 : FailMsg("empty sara stays on step 1");
            fail += Expect(Convert.ToString(emptyPack["diagnosis"]), "پیدا نشد", "no overlay without Sara");
            fail += emptyStepLog.Any(l => l.IndexOf("Member 1288", StringComparison.Ordinal) >= 0) ? FailMsg("step run must not load chidman member") : 0;
            fail += emptyStepLog.Any(l => l.IndexOf("InsertChidman", StringComparison.OrdinalIgnoreCase) >= 0) ? FailMsg("step run must not analyze chidman") : 0;
            fail += emptyStepLog.Any(l => l.IndexOf("MemberDocument", StringComparison.OrdinalIgnoreCase) >= 0) ? FailMsg("step run must not load docs") : 0;
            fail += emptyStepLog.Any(l => l.IndexOf("CommissionFine", StringComparison.OrdinalIgnoreCase) >= 0) ? FailMsg("step run must not load income/commission members") : 0;
            fail += emptyStepLog.Any(l => l.IndexOf("2/5 صلح", StringComparison.Ordinal) >= 0 && l.IndexOf("اجرا نشد", StringComparison.Ordinal) >= 0) ? 0 : FailMsg("later steps blocked");
            fail += emptyStepLog.Any(l => l.IndexOf("ZabeteStatic", StringComparison.OrdinalIgnoreCase) >= 0) ? FailMsg("step1 must not dump ماده ۵") : 0;
            var slim = PermitSteps.SlimVars(new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "name", "NidZabeteh" }, { "value", "3fd00472-04da-4b5b-8ca8-701d9e82c4c0" }, { "table", "[dbo].[Zabeteh]" } },
                new Dictionary<string, object> { { "name", "DateZabeteh" }, { "value", "1404/01/01" }, { "table", "[dbo].[Zabeteh]" } },
                new Dictionary<string, object> { { "name", "UserName" }, { "value", "x" }, { "table", "[dbo].[Zabeteh]" } },
            });
            fail += slim.Count == 1 ? 0 : FailMsg("slim vars drops extra overlay columns");
            fail += Expect(string.Join(",", PermitPipeline.PermitClasses), "338", "takhalofat class");
            fail += Expect(string.Join(",", PermitPipeline.PermitClasses), "337", "income class");
            fail += Expect(ZabetehCase.StaticPkeyColumn, "P_Key", "static P_Key column");
            fail += Expect(string.Join(",", ZabetehCase.StaticChildPrefer), "NidZStatic_Info", "static child via Info");
            fail += Expect(string.Join(",", ZabetehCase.StaticChildPrefer), "P_Key", "static child also P_Key");
            if (string.Join(",", ZabetehCase.StaticChildPrefer).IndexOf("CI_PlanType", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Console.Error.WriteLine("FAIL: ZabeteStatic_Plan must not join on CI_PlanType");
                fail++;
            }
            return fail;
        }

        private static int PickScope()
        {
            int fail = 0;
            fail += PermitScopes.Has(PermitScopes.Normalize(new[] { "ضابطه", "صلح", "analysis" }), PermitScopes.Zabeteh) ? 0 : FailMsg("normalize zabeteh");
            fail += PermitScopes.Has(PermitScopes.Normalize(new[] { "صلح" }), PermitScopes.Solh) ? 0 : FailMsg("normalize solh");
            fail += PermitScopes.Has(PermitScopes.Normalize(new[] { "تحلیل" }), PermitScopes.Tahlil) ? 0 : FailMsg("normalize tahlil");
            fail += PermitScopes.Has(PermitScopes.Normalize(new[] { "توافق" }), PermitScopes.Tavafogh) ? 0 : FailMsg("normalize tavafogh");
            fail += PermitScopes.Has(PermitScopes.Normalize(new[] { "چیدمان" }), PermitScopes.Chidman) ? 0 : FailMsg("normalize chidman");
            fail += PermitScopes.Normalize(new[] { "nope", "zabeteh" }).Count == 1 ? 0 : FailMsg("unknown scope dropped");
            fail += Expect(string.Join(",", PermitScopes.Tables(PermitScopes.Tahlil)), "AnalysisBuilding", "tahlil named table");
            fail += Expect(string.Join(",", PermitScopes.Tables(PermitScopes.Solh)), "Sh_Peace", "solh named table");
            fail += Expect(string.Join(",", PermitScopes.Tables(PermitScopes.Tavafogh)), "Sh_Agreement", "agreement named table");
            fail += Expect(string.Join(",", PermitScopes.Tables(PermitScopes.Chidman)), "Base_Front", "chidman front");
            fail += PermitScopes.Tables(PermitScopes.Commission).Length == 0 ? 0 : FailMsg("commission has no named table");
            fail += PermitScopes.BuildingZero(PermitScopes.Solh) ? 0 : FailMsg("solh building 0");
            fail += PermitScopes.BuildingZero(PermitScopes.Tavafogh) ? 0 : FailMsg("agreement building 0");
            fail += PermitScopes.BuildingZero(PermitScopes.Tahlil) ? FailMsg("tahlil not building 0") : 0;
            fail += Expect(PermitScopes.MustPick, "کاربر باید انتخاب", "must pick copy");

            var emptyLog = new List<string>();
            var empty = PermitSteps.RunSelected("", "", "E3BB36F6-1D34-426B-BFA1-526A05C452BE", new string[0], emptyLog.Add);
            fail += Convert.ToString(empty["status"]) == "FAIL" ? 0 : FailMsg("empty pick fails");
            fail += Expect(Convert.ToString(empty["diagnosis"]), "کاربر باید انتخاب", "empty pick diagnosis");
            fail += emptyLog.Any(l => l.IndexOf("کاربر باید انتخاب", StringComparison.Ordinal) >= 0) ? 0 : FailMsg("empty pick log");
            fail += emptyLog.Any(l => l.IndexOf("Member 1296", StringComparison.Ordinal) >= 0) ? FailMsg("pick run must not load 1296") : 0;

            var tahlilPass = PermitSteps.ClassifyTahlil("300002275", true);
            fail += tahlilPass.Status == "PASS" ? 0 : FailMsg("tahlil pass when AnalysisBuilding");
            fail += Expect(tahlilPass.Verdict, "AnalysisBuilding", "tahlil names table");
            fail += Expect(tahlilPass.Verdict, "AnaliysParvaneh_Date", "tahlil max penalty date");
            var tahlilSkip = PermitSteps.ClassifyTahlil("300002275", false);
            fail += tahlilSkip.Status == "SKIP" ? 0 : FailMsg("tahlil skip when missing");
            var agrSkip = PermitSteps.ClassifyTavafogh("300002275", false);
            fail += agrSkip.Status == "SKIP" ? 0 : FailMsg("agreement skip when missing");
            fail += Expect(agrSkip.Verdict, "توافق ندارد", "agreement optional");
            var agrPass = PermitSteps.ClassifyTavafogh("300002275", true);
            fail += agrPass.Status == "PASS" ? 0 : FailMsg("agreement pass");
            fail += Expect(agrPass.Verdict, "Building=0", "agreement building 0");
            var comm = PermitSteps.ClassifyCommission("300002275");
            fail += comm.Status == "SKIP" ? 0 : FailMsg("commission skip no named table");
            fail += Expect(comm.Verdict, "کمیسیون نداشته باشد", "commission optional");
            var income = PermitSteps.ClassifyIncome("300002275");
            fail += income.Status == "SKIP" ? 0 : FailMsg("income skip");
            var chid = PermitSteps.ClassifyChidman("300002275", true);
            fail += chid.Status == "PASS" ? 0 : FailMsg("chidman pass");

            fail += PermitSteps.HasNamed(new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "name", "NidAnalysisBuilding" }, { "value", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee" }, { "table", "[dbo].[AnalysisBuilding]" } },
            }, "AnalysisBuilding") ? 0 : FailMsg("has analysis building");
            fail += PermitSteps.HasSolhRecord(new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "name", "NidAgreement" }, { "value", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee" }, { "table", "[dbo].[Sh_Agreement]" } },
            }) ? FailMsg("agreement is not solh") : 0;
            fail += PermitSteps.HasNamed(new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "name", "NidAgreement" }, { "value", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee" }, { "table", "[dbo].[Sh_Agreement]" } },
            }, "Sh_Agreement") ? 0 : FailMsg("has agreement");

            var pickLog = new List<string>();
            var pack = PermitSteps.RunSelected("", "", "E3BB36F6-1D34-426B-BFA1-526A05C452BE", new[] { "zabeteh", "tahlil" }, pickLog.Add);
            fail += Convert.ToInt32(pack["exitCode"]) == 1 ? 0 : FailMsg("empty sara zabeteh fail exit");
            fail += Expect(Convert.ToString(pack["diagnosis"]), "پیدا نشد", "selected zabeteh no overlay");
            fail += pickLog.Any(l => l.IndexOf("Scope", StringComparison.OrdinalIgnoreCase) >= 0) ? 0 : FailMsg("selected logs Scope");
            fail += pickLog.Any(l => l.IndexOf("2/5 صلح", StringComparison.Ordinal) >= 0) ? FailMsg("selected must not run linear solh gate") : 0;
            fail += pickLog.Any(l => l.IndexOf("InsertChidman", StringComparison.OrdinalIgnoreCase) >= 0) ? FailMsg("selected must not analyze chidman member") : 0;
            fail += pack.ContainsKey("scopes") ? 0 : FailMsg("pack has scopes");

            var readEmpty = ZabetehCase.ReadSelected("", "E3BB36F6-1D34-426B-BFA1-526A05C452BE", new[] { "tahlil" }, m => { });
            fail += readEmpty.Count == 0 ? 0 : FailMsg("readselected empty sara");
            return fail;
        }

        private static int RuleDocsCatalog()
        {
            int fail = 0;
            string sql = RuleDocs.CatalogSql(null);
            fail += Expect(sql, "TOP (1000)", "overall TOP 1000");
            foreach (string col in RuleDocs.UserColumns)
                fail += Expect(sql, "[" + col + "]", "catalog column " + col);
            if (sql.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Console.Error.WriteLine("FAIL: overall MemberDocument query must not filter NidMember");
                fail++;
            }
            fail += Expect(RuleDocs.Catalog, "DbRuleEngeinDocument", "docs catalog");
            fail += Expect(RuleDocs.MainTable, "MemberDocument", "main docs table");
            fail += Expect(RuleDocs.DefaultBodyExpr(), "MemberDocument", "body convert");
            fail += Expect(RuleDocs.BodyExpr("MemberDocument", "image"), "VARBINARY(MAX)", "image body via varbinary");
            fail += Expect(string.Join(",", RuleDocs.TableHints), "Zabeteh", "hint Zabeteh");
            fail += Expect(string.Join(",", RuleDocs.TableHints), "CI_PlanType", "hint CI_PlanType");
            fail += RuleDocs.SkipPeek("AspNetUsers") ? 0 : FailMsg("skip AspNetUsers peek");
            fail += RuleDocs.SkipPeek("__EFMigrationsHistory") ? 0 : FailMsg("skip EF peek");
            fail += RuleDocs.SkipPeek("Users") ? 0 : FailMsg("skip Users peek");
            fail += RuleDocs.SkipPeek("sysdiagrams") ? 0 : FailMsg("skip sysdiagrams peek");
            fail += RuleDocs.SkipPeek("MemberDocument") ? 0 : FailMsg("skip main catalog peek");
            fail += RuleDocs.SkipPeek("MemberDocumentLog") ? FailMsg("should peek MemberDocumentLog") : 0;
            fail += RuleDocs.SkipPeek("MemberVbCodeDocument") ? FailMsg("should peek MemberVbCodeDocument") : 0;
            var empty = RuleDocs.Read("", m => { });
            fail += empty.ContainsKey("docs") ? 0 : FailMsg("empty pack has docs");
            fail += empty.ContainsKey("tables") ? 0 : FailMsg("empty pack has tables");
            var vars = new List<Dictionary<string, object>>();
            RuleDocs.FlattenInto(vars, empty);
            fail += vars.Count == 0 ? 0 : FailMsg("flatten empty docs adds nothing");
            return fail;
        }

        private static int PasteSummaryFilter()
        {
            int fail = 0;
            fail += FormulaEngine.IsSummaryLine("NidProc      : 89DD8996-A448-4164-B0FD-74F8B5F71B1B") ? 0 : FailMsg("nidproc in summary");
            fail += FormulaEngine.IsSummaryLine("Zabeteh     : join = Zabeteh.NidNosaziCode = Sh_RequestInfo.NidNosaziCode") ? 0 : FailMsg("zabeteh join in summary");
            fail += FormulaEngine.IsSummaryLine("SolhNid      : بخش مشکوک: CRUD Read OK") ? 0 : FailMsg("solhnid in summary");
            fail += FormulaEngine.IsSummaryLine("Arch         : ClsFunction 1296 ? BodyLen=0") ? FailMsg("bodylen must not copy") : 0;
            fail += FormulaEngine.IsSummaryLine("Chidman      : L572 Key=طرح  Info8.AddError") ? FailMsg("adderror dump must not copy") : 0;
            fail += FormulaEngine.IsSummaryLine("Chidman      : توقف صلح اگر ضابطه/چیدمان اعلام نشده") ? 0 : FailMsg("l270 finding copies");
            fail += FormulaEngine.IsSummaryLine("Chidman      : ZabetehConvert/342 Member 1288 Run L866: InsertChidman(tmpDto)") ? FailMsg("insertchidman catalog skip") : 0;
            fail += FormulaEngine.IsSummaryLine("Chidman      : L864: If Just11 = True And tmpDto.CI_UsingGroup = 11") ? FailMsg("parking guard skip") : 0;
            fail += FormulaEngine.IsSummaryLine("SolhNid      : L270 سورس است نه توقف این Nid — ActiveNidZabeteh=eea1f974") ? 0 : FailMsg("guard-not-fired copies");
            fail += FormulaEngine.IsSummaryLine("SolhNid      : توقف زنده Member 1296 L270 — ActiveNidZabeteh خالی") ? 0 : FailMsg("fired L270 copies");
            fail += FormulaEngine.IsSummaryLine("History      : Rule/336 Member 1148 hist=856157") ? FailMsg("rule/336 history skip") : 0;
            fail += FormulaEngine.IsSummaryLine("History      : Solh/344 Member 1296 hist=856104") ? 0 : FailMsg("solh history copies");
            fail += FormulaEngine.IsSummaryLine("Doc         : جدول [AspNetUsers] cols=Id") ? FailMsg("aspnet docs skip") : 0;
            fail += FormulaEngine.IsSummaryLine("Doc         : [AspNetUsers] peek rows=1") ? FailMsg("aspnet peek skip") : 0;
            fail += FormulaEngine.IsSummaryLine("Doc         : MemberDocument TOP 1000 → 503 ردیف") ? 0 : FailMsg("memberdocument count copies");
            fail += FormulaEngine.IsSummaryLine("Permit      : مسیر پروانه = ضابطه → صلح → تحلیل") ? 0 : FailMsg("permit path copies");
            fail += FormulaEngine.IsSummaryLine("Zabeteh     : روکش اعلام‌نشده NidZabeteh=abc") ? 0 : FailMsg("unannounced overlay copies");
            fail += FormulaEngine.IsSummaryLine("Permit      : WorkItem=5298603 بررسی نمی‌شود") ? 0 : FailMsg("ignore workitem copies");
            fail += FormulaEngine.IsSummaryLine("Detail      : جدول [AspNetUsers] cols=Id") ? FailMsg("detail prefix never copies") : 0;
            fail += FormulaEngine.IsSummaryLine("Step        : 1/5 ضابطه — رد") ? 0 : FailMsg("step line copies");
            fail += FormulaEngine.IsSummaryLine("Scope      : تحلیل — قبول") ? 0 : FailMsg("scope line copies");
            bool prevStrict = FormulaEngine.StrictSummary;
            FormulaEngine.StrictSummary = true;
            fail += FormulaEngine.IsSummaryLine("Step        : 1/5 ضابطه — رد") ? 0 : FailMsg("strict step copies");
            fail += FormulaEngine.IsSummaryLine("Arch         : class 335 CommissionFine members=15") ? FailMsg("strict drops arch") : 0;
            fail += FormulaEngine.IsSummaryLine("Doc         : MemberDocument TOP 1000 → 503 ردیف") ? FailMsg("strict drops docs") : 0;
            fail += FormulaEngine.IsSummaryLine("Chidman      : توقف صلح اگر ضابطه/چیدمان اعلام نشده") ? FailMsg("strict drops chidman") : 0;
            fail += FormulaEngine.IsSummaryLine("Zabeteh     : روکش اعلام‌نشده NidZabeteh=3fd00472") ? 0 : FailMsg("strict overlay copies");
            fail += FormulaEngine.IsSummaryLine("Zabeteh     : Sara tables = Sh_RequestInfo, Zabeteh") ? FailMsg("strict drops table catalog") : 0;
            fail += FormulaEngine.IsSummaryLine("SolhNid      : Member 1296 Run codeLen=366093") ? FailMsg("strict drops solh members") : 0;
            fail += FormulaEngine.IsSummaryLine("History      : 40 change row(s)") ? FailMsg("strict drops history count") : 0;
            fail += FormulaEngine.IsSummaryLine("Scope      : انتخاب = ضابطه، تحلیل") ? 0 : FailMsg("strict scope copies");
            var flood = new[]
            {
                "Arch         : class 335 CommissionFine members=15 (853 KB)",
                "Doc         : MemberDocument TOP 1000 → 503 ردیف",
                "Chidman      : chidman AddError: 21 line(s) in Member 1288",
                "History      : 40 change row(s)",
                "SolhNid      : Member 1296 Run codeLen=366093",
                "Step        : 1/5 ضابطه — رد",
                "Step        : ضابطه هست، اعلام نشده. روکش=3fd00472-04da-4b5b-8ca8-701d9e82c4c0 PlanType=24",
                "Zabeteh     : روکش اعلام‌نشده NidZabeteh=3fd00472-04da-4b5b-8ca8-701d9e82c4c0 CI_PlanType=24",
                "Exit code    : 1",
            };
            int copied = 0;
            foreach (string line in flood)
                if (FormulaEngine.IsSummaryLine(line)) copied++;
            fail += copied == 4 ? 0 : FailMsg("strict flood copies only 4 step lines, got " + copied);
            FormulaEngine.StrictSummary = prevStrict;
            return fail;
        }

        private static int HistoryImageSql()
        {
            int fail = 0;
            string list = MemberHistory.ListBodyExpr("Body");
            fail += Expect(list, "DATALENGTH", "list uses DATALENGTH");
            if (list.IndexOf("NVARCHAR", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Console.Error.WriteLine("FAIL: list Body expr must not CAST to nvarchar");
                fail++;
            }
            string image = MemberHistory.BodyPayloadExpr("Body", "image");
            fail += Expect(image, "VARBINARY(MAX)", "image payload via varbinary");
            if (image.IndexOf("NVARCHAR", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Console.Error.WriteLine("FAIL: image payload must not CAST to nvarchar");
                fail++;
            }
            fail += MemberHistory.IsBinaryBody("image") ? 0 : FailMsg("image is binary body");
            fail += Expect(MemberHistory.BodyPayloadExpr("XmlBody", "nvarchar"), "VARBINARY(MAX)", "nvarchar also via varbinary");
            fail += Expect(MemberHistory.BodyPayloadExpr("Body", "ntext"), "NVARCHAR(MAX)", "ntext convert");
            byte[] utf16 = Encoding.Unicode.GetBytes("<Member><Body>Sub Run()</Body></Member>");
            fail += Expect(MemberHistory.DecodeBodyBytes(utf16), "Sub Run()", "utf16 xml decode");
            byte[] utf8 = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes("<x>ok</x>")).ToArray();
            fail += Expect(MemberHistory.DecodeBodyBytes(utf8), "<x>ok</x>", "utf8 bom decode");
            return fail;
        }

        private static int JsonRoundtrip()
        {
            var src = new System.Collections.Generic.Dictionary<string, object>
            {
                { "ok", true },
                { "n", 1288 },
                { "s", "چیدمان\nSolh" },
            };
            string json = Json.Encode(src);
            var back = Json.ParseObject(json);
            int fail = 0;
            fail += Expect(json, "1288", "json number");
            fail += Json.Bool(back, "ok") ? 0 : FailMsg("json bool");
            fail += Json.Int(back, "n") == 1288 ? 0 : FailMsg("json int");
            fail += Json.Long(new Dictionary<string, object> { { "nidHistory", 598688L } }, "nidHistory") == 598688L ? 0 : FailMsg("json long");
            fail += (Json.Str(back, "s") ?? string.Empty).IndexOf("چیدمان", StringComparison.Ordinal) >= 0 ? 0 : FailMsg("json unicode");
            var listed = Json.StrList(new Dictionary<string, object>
            {
                { "scopes", new List<object> { "zabeteh", "solh" } },
            }, "scopes");
            fail += listed.Count == 2 ? 0 : FailMsg("json strlist count");
            fail += listed.Contains("zabeteh") ? 0 : FailMsg("json strlist zabeteh");
            var csv = Json.StrList(new Dictionary<string, object> { { "scopes", "tahlil,tavafogh" } }, "scopes");
            fail += csv.Count == 2 ? 0 : FailMsg("json strlist csv");
            return fail;
        }

        private static int WebUiEmbedded()
        {
            string html;
            try { html = WebHost.LoadHtml(); }
            catch (Exception ex)
            {
                Console.Error.WriteLine("FAIL: WebUi.html missing: " + ex.Message);
                return 1;
            }
            int fail = 0;
            fail += Expect(html, "عیب‌یاب فرمول سارا", "persian title");
            fail += Expect(html, "id=\"btnRun\">اجرا</button>", "gold run button");
            fail += Expect(html, "/api/run", "run endpoint");
            fail += Expect(html, "/api/history", "history endpoint");
            fail += Expect(html, "/api/history-row", "history-row endpoint");
            fail += Expect(html, "تاریخچه فرمول", "history tab");
            fail += Expect(html, "بررسی فرمول از DB", "db-first button");
            fail += Expect(html, "دیباگ پروانه این Nid", "permit nid button");
            fail += Expect(html, "/api/solh-nid", "solh-nid endpoint");
            fail += Expect(html, "متغیرهای این Nid", "vars tab");
            fail += Expect(html, "showSummary(j.summary)", "chidman/history fill copy-summary");
            fail += Expect(html, "Zabeteh", "named Zabeteh table in vars hint");
            fail += Expect(html, "NidNosaziCode", "join key in vars hint");
            fail += Expect(html, "300002275", "permit sample workitem");
            fail += Expect(html, "5298603", "ignored workitem mentioned");
            fail += Expect(html, "تجدید بنا", "reconstruction permit");
            fail += Expect(html, "صلح ندارند", "solh not required for every property");
            fail += Expect(html, "data-scope=\"zabeteh\"", "scope checkbox zabeteh");
            fail += Expect(html, "data-scope=\"solh\"", "scope checkbox solh");
            fail += Expect(html, "data-scope=\"chidman\"", "scope checkbox chidman");
            fail += Expect(html, "data-scope=\"tahlil\"", "scope checkbox tahlil");
            fail += Expect(html, "data-scope=\"tavafogh\"", "scope checkbox tavafogh");
            fail += Expect(html, "کاربر باید انتخاب", "must pick hint");
            fail += Expect(html, "selectedScopes", "payload collects scopes");
            fail += Expect(html, "کدام بخش را دیباگ", "toast if no scope");
            fail += Expect(html, "AnalysisBuilding", "analysis table in vars hint");
            fail += Expect(html, "Sh_Peace", "peace table in vars hint");
            fail += Expect(html, "Sh_Agreement", "agreement table in vars hint");
            fail += Expect(html, "Building=0", "agreement building zero");
            fail += Expect(html, "ابزارهای بیشتر", "advanced tools collapsed");
            fail += Expect(html, "مستند کلی", "overall docs button");
            fail += Expect(html, "/api/docs", "docs endpoint");
            fail += Expect(html, "data-tab=\"docs\"", "docs tab");
            fail += Expect(html, "ParentDocId", "ParentDocId column");
            fail += Expect(html, "dir=\"rtl\"", "rtl");
            if (html.IndexOf("اجرای موتور (اختیاری)", StringComparison.Ordinal) >= 0)
            {
                Console.Error.WriteLine("FAIL: optional-engine label should not replace اجرا");
                fail++;
            }
            return fail;
        }

        private static int WebHostRoundtrip()
        {
            var app = new WebApp(new UserSettings
            {
                LastFormula = "Solh",
                LastWatch = "Calc_Chandganeh",
            });
            using (var host = new WebHost(app))
            {
                try { host.Start(17991); }
                catch (Exception ex)
                {
                    Console.WriteLine("WARN: HttpListener not available here (" + ex.Message + ") — HTML/API still compiled");
                    return 0;
                }
                try
                {
                    using (var wc = new System.Net.WebClient())
                    {
                        wc.Encoding = Encoding.UTF8;
                        string html = wc.DownloadString(host.Url);
                        string ping = wc.DownloadString(host.Url + "api/ping");
                        string boot = wc.DownloadString(host.Url + "api/bootstrap");
                        int fail = 0;
                        fail += Expect(html, "RuleTrace", "served html");
                        fail += Expect(ping, "\"ok\":true", "ping ok");
                        fail += Expect(boot, "Solh", "bootstrap formulas");
                        fail += Expect(boot, "v23c-pick-scope", "bootstrap label");
                        fail += Expect(boot, "mustPick", "bootstrap must-pick");
                        fail += Expect(boot, "zabeteh", "bootstrap scopes");
                        return fail;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("FAIL: web roundtrip: " + ex.Message);
                    return 1;
                }
                finally
                {
                    host.Stop();
                }
            }
        }

        private static int FailMsg(string label)
        {
            Console.Error.WriteLine("FAIL: " + label);
            return 1;
        }

        private static int Expect(string haystack, string needle, string label)
        {
            if (haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0) return 0;
            Console.Error.WriteLine("FAIL: expected " + label + " (" + needle + ")");
            return 1;
        }
    }
}
