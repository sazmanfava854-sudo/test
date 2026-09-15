using System;
using System.Collections.Generic;
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
            foreach (int need in new[] { 336, 342, 344, 345, 432 })
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
            if (BuildInfo.Label.IndexOf("web", StringComparison.OrdinalIgnoreCase) < 0)
            {
                Console.Error.WriteLine("FAIL: BuildInfo.Label should be v22-web, got " + BuildInfo.Label);
                return 1;
            }
            return 0;
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
            fail += (Json.Str(back, "s") ?? string.Empty).IndexOf("چیدمان", StringComparison.Ordinal) >= 0 ? 0 : FailMsg("json unicode");
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
            fail += Expect(html, "/api/run", "run endpoint");
            fail += Expect(html, "/api/history", "history endpoint");
            fail += Expect(html, "تاریخچه فرمول", "history tab");
            fail += Expect(html, "بررسی فرمول از DB", "db-first button");
            fail += Expect(html, "dir=\"rtl\"", "rtl");
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
                        fail += Expect(boot, "v22c-web-history", "bootstrap label");
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
