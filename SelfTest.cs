using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace RuleTrace
{
    /// <summary>Headless checks for v21 architecture. Run: RuleTrace.exe --self-test</summary>
    internal static class SelfTest
    {
        public static int Run()
        {
            int fail = 0;
            fail += ChidmanSolhGuard();
            fail += BannerIsV21();
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
                    NidMember = 1,
                    Name = "Run",
                    Meta = "test",
                    Code = "Public Sub Run()\r\n  If True Then Map_Function()\r\nEnd Sub\r\n",
                },
                new MemberSource
                {
                    NidMember = 1288,
                    Name = "Chidman",
                    Meta = "test",
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

        private static int BannerIsV21()
        {
            if (BuildInfo.Label.IndexOf("v21", StringComparison.OrdinalIgnoreCase) < 0)
            {
                Console.Error.WriteLine("FAIL: BuildInfo.Label is " + BuildInfo.Label + " (expected v21-*)");
                return 1;
            }
            if (BuildInfo.Banner.IndexOf("بازنویسی", StringComparison.OrdinalIgnoreCase) < 0
                && BuildInfo.Banner.IndexOf("v21", StringComparison.OrdinalIgnoreCase) < 0)
            {
                Console.Error.WriteLine("FAIL: banner does not describe no-VB-rewrite architecture: " + BuildInfo.Banner);
                return 1;
            }
            return 0;
        }

        private static int Expect(string haystack, string needle, string label)
        {
            if (haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0) return 0;
            Console.Error.WriteLine("FAIL: expected " + label + " (" + needle + ")");
            return 1;
        }
    }
}
