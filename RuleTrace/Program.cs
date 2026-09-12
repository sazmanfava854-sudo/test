using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using BIZ.SC;
using BIZ.SA;
using SafaClassDesingerNew;
using FormulaClsCommon = SafaClassDesingerNew.ClsCommon;

namespace RuleTrace
{
    internal static class Program
    {
        private static string _dllPath;

        private static readonly Dictionary<string, int> FormulaMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Global"]           = 432,
            ["Rule"]             = 336,
            ["Income"]           = 337,
            ["Takhalofat"]       = 338,
            ["CommissionFine"]   = 335,
            ["Solh"]             = 344,
            ["Tavafogh"]         = 345,
            ["Commission"]       = 340,
            ["Validtion"]        = 341,
            ["ZabetehConvert"]   = 342,
            ["Nosazi_Calculate"] = 339,
        };

        static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0 || HasFlag(args, "--help") || HasFlag(args, "-h"))
                {
                    PrintHelp();
                    return 0;
                }

                _dllPath = GetArg(args, "--dll-path") ?? ConfigurationManager.AppSettings["DllPath"];
                if (string.IsNullOrWhiteSpace(_dllPath) || !Directory.Exists(_dllPath))
                {
                    Console.Error.WriteLine("ERROR: DllPath not found. Set appSettings:DllPath or --dll-path");
                    return 2;
                }

                AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
                SetupConnections();

                var options = ParseOptions(args);
                return RunFormula(options);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("FATAL: " + ex);
                return 99;
            }
        }

        private static int RunFormula(TraceOptions options)
        {
            if (!FormulaMap.TryGetValue(options.Formula, out int nidRuleClass) || nidRuleClass == 0)
            {
                Console.Error.WriteLine("ERROR: Unknown formula '{0}'. Known: {1}",
                    options.Formula, string.Join(", ", FormulaMap.Keys));
                return 2;
            }

            Guid rootGuid = ParseRootGuid();
            Console.WriteLine("Formula     : {0} (NidRuleClass={1})", options.Formula, nidRuleClass);
            Console.WriteLine("RootGUID    : {0}", rootGuid);
            Console.WriteLine("ReCompile   : {0}", options.ReCompile);

            // ── 1. Compile / cache formula assembly ──
            ClsRunRuleResult result = FormulaClsCommon.RunRule(nidRuleClass, rootGuid, options.ReCompile);

            if (result == null)
            {
                Console.Error.WriteLine("ERROR: ClsCommon.RunRule returned null (check RuleEngine connection / CnRuleString).");
                return 3;
            }

            if (result.CompilerErrors != null && result.CompilerErrors.HasErrors)
            {
                Console.Error.WriteLine("=== COMPILE ERRORS ===");
                foreach (var err in result.CompilerErrors)
                    Console.Error.WriteLine("  {0}", err);
                return 4;
            }

            Console.WriteLine("Compile     : OK");
            if (result.ClassDesinger != null)
            {
                Console.WriteLine("Class       : {0} (FormulaGroup={1})",
                    result.ClassDesinger.Name, result.ClassDesinger.FormulaGroup);
            }

            // ── 2. Build Info8 context (ClsObjectFactory) ──
            var factory = BuildObjectFactory(options);
            result.SetMyInfo(factory);

            // ── 3. Inject formula parameters from command line (--param Name=Value) ──
            foreach (var kv in options.Parameters)
            {
                result.SetParam(kv.Key, kv.Value);
                Console.WriteLine("SetParam    : {0} = {1}", kv.Key, kv.Value);
            }

            // ── 4. Run entry point ──
            string entryPoint = options.EntryPoint;
            if (string.IsNullOrWhiteSpace(entryPoint))
            {
                entryPoint = result.ClassDesinger?.UpdatedFunctionList?.FirstOrDefault() ?? "Map_Function";
            }

            Console.WriteLine();
            Console.WriteLine("=== RUN {0} ===", entryPoint);
            object runResult = result.Run(entryPoint);
            if (runResult != null)
                Console.WriteLine("Return      : {0}", runResult);

            // ── 5. Trace output (replaces Logfilefj + UI) ──
            Console.WriteLine();
            Console.WriteLine("=== TRACE (AddError / BizErrors) ===");
            int errorCount = PrintBizErrors(factory.ErrorResult, options.Watch);

            if (!string.IsNullOrWhiteSpace(options.Watch) && result.ParametersValue != null)
            {
                if (result.ParametersValue.ContainsKey(options.Watch))
                    Console.WriteLine("ParametersValue[{0}] = {1}", options.Watch, result.ParametersValue[options.Watch]);
            }

            if (options.ShowAllParams && result.ParametersValue != null)
            {
                Console.WriteLine();
                Console.WriteLine("=== ALL ParametersValue ===");
                foreach (DictionaryEntry entry in result.ParametersValue)
                    Console.WriteLine("  {0} = {1}", entry.Key, entry.Value);
            }

            Console.WriteLine();
            Console.WriteLine("Done. BizErrors={0}", errorCount);
            return HasStopError(factory.ErrorResult) ? 1 : 0;
        }

        private static ClsObjectFactory BuildObjectFactory(TraceOptions options)
        {
            var parameterList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(options.NidProc))
                parameterList["NidProc"] = options.NidProc;

            foreach (var kv in options.FactoryParameters)
                parameterList[kv.Key] = kv.Value;

            var factory = new ClsObjectFactory
            {
                ParameterList = parameterList,
                Security_RequestGuid = options.RequestGuid,
                Security_EncryptCode = options.EncryptCode ?? string.Empty,
                _District = options.District,
                ErrorResult = new ClsErrorResult(),
                ObjectList = new List<DtoClsOperation>(),
            };

            if (parameterList.Count > 0)
            {
                Console.WriteLine("NidProc     : {0}", parameterList.ContainsKey("NidProc") ? parameterList["NidProc"] : "(not set)");
                factory.LoadObj(factory.ParameterList, factory.ObjectList);
            }
            else
            {
                Console.WriteLine("WARN: NidProc not set — GetPeace/GetZabeteh may fail for Solh.");
            }

            return factory;
        }

        private static void SetupConnections()
        {
            string ruleEngine = ConfigurationManager.ConnectionStrings["RuleEngine"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(ruleEngine))
                throw new InvalidOperationException("connectionStrings:RuleEngine is missing in App.config");

            FormulaClsCommon.CnRuleString = ruleEngine;

            // FormulaEncryptionCode — used by ClsFormula / ClsConnection wrappers
            string rootGuid = ConfigurationManager.AppSettings["RootGUID"] ?? Guid.Empty.ToString();
            TrySetStaticString("BIZ.SC.ClsConnection", "FormulaEncryptionCode", rootGuid);
            TrySetStaticString("BIZ.SC.ClsProxyHelper", "FormulaEncryptionCode", rootGuid);
        }

        private static Guid ParseRootGuid()
        {
            string root = ConfigurationManager.AppSettings["RootGUID"];
            if (string.IsNullOrWhiteSpace(root))
                return Guid.Empty;
            return Guid.TryParse(root, out Guid g) ? g : Guid.Empty;
        }

        private static int PrintBizErrors(ClsErrorResult errors, string watch)
        {
            if (errors?.BizErrors == null || errors.BizErrors.Count == 0)
            {
                Console.WriteLine("(no BizErrors)");
                return 0;
            }

            int shown = 0;
            foreach (var err in errors.BizErrors)
            {
                bool match = string.IsNullOrWhiteSpace(watch)
                    || string.Equals(err.ErrorKey, watch, StringComparison.OrdinalIgnoreCase)
                    || (err.ErrorTitel != null && err.ErrorTitel.IndexOf(watch, StringComparison.OrdinalIgnoreCase) >= 0);

                if (!match)
                    continue;

                Console.WriteLine("[{0}] {1}: {2}", err.ErrorAction, err.ErrorKey, err.ErrorTitel);
                shown++;
            }

            if (!string.IsNullOrWhiteSpace(watch) && shown == 0)
            {
                Console.WriteLine("(no match for watch='{0}' — total BizErrors={1})", watch, errors.BizErrors.Count);
                foreach (var err in errors.BizErrors.Take(20))
                    Console.WriteLine("  [{0}] {1}: {2}", err.ErrorAction, err.ErrorKey, err.ErrorTitel);
            }

            return errors.BizErrors.Count;
        }

        private static bool HasStopError(ClsErrorResult errors)
        {
            return errors?.BizErrors != null
                && errors.BizErrors.Any(e => e.ErrorAction == EumErrorAction.Stop);
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string name = new AssemblyName(args.Name).Name;
            string[] extensions = { ".dll", ".DLL" };

            foreach (var ext in extensions)
            {
                string path = Path.Combine(_dllPath, name + ext);
                if (File.Exists(path))
                    return Assembly.LoadFrom(path);
            }

            return null;
        }

        private static void TrySetStaticString(string typeName, string fieldName, string value)
        {
            try
            {
                Type t = Type.GetType(typeName + ", BIZ.SC");
                if (t == null) return;
                FieldInfo f = t.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
                if (f != null && f.FieldType == typeof(string))
                    f.SetValue(null, value);
            }
            catch
            {
                // optional — ClsConnection may not exist in all BIZ.SC versions
            }
        }

        private static TraceOptions ParseOptions(string[] args)
        {
            var options = new TraceOptions
            {
                Formula = GetArg(args, "--formula") ?? "Solh",
                NidProc = GetArg(args, "--nidproc"),
                Watch = GetArg(args, "--watch"),
                EntryPoint = GetArg(args, "--entry"),
                EncryptCode = GetArg(args, "--encrypt") ?? string.Empty,
                ReCompile = HasFlag(args, "--recompile"),
                ShowAllParams = HasFlag(args, "--all-params"),
            };

            string district = GetArg(args, "--district");
            if (!string.IsNullOrWhiteSpace(district))
                options.District = int.Parse(district);

            string requestGuid = GetArg(args, "--request-guid");
            options.RequestGuid = string.IsNullOrWhiteSpace(requestGuid)
                ? Guid.Empty
                : Guid.Parse(requestGuid);

            // --param SatheEshghal=120.5  (repeatable)
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--param" && i + 1 < args.Length)
                {
                    string[] parts = args[i + 1].Split(new[] { '=' }, 2);
                    if (parts.Length == 2)
                        options.Parameters[parts[0].Trim()] = parts[1].Trim();
                }
                if (args[i] == "--factory-param" && i + 1 < args.Length)
                {
                    string[] parts = args[i + 1].Split(new[] { '=' }, 2);
                    if (parts.Length == 2)
                        options.FactoryParameters[parts[0].Trim()] = parts[1].Trim();
                }
            }

            return options;
        }

        private static string GetArg(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            }
            return null;
        }

        private static bool HasFlag(string[] args, string name)
        {
            return args.Any(a => string.Equals(a, name, StringComparison.OrdinalIgnoreCase));
        }

        private static void PrintHelp()
        {
            Console.WriteLine(@"
RuleTrace — standalone Sara formula debugger (replaces Logfilefj workflow)

USAGE:
  RuleTrace.exe --nidproc <GUID> --formula Solh [--watch Calc_Chandganeh]

REQUIRED (for Solh / Peace formulas):
  --nidproc <GUID>        Sh_Request.NidProc (from UI or SQL)

OPTIONS:
  --formula <name>        Rule | Solh | Income | Takhalofat | ...  (default: Solh)
  --watch <name>          Filter BizErrors by ErrorKey or ErrorTitel
  --entry <method>        VB entry point (default: first in UpdatedFunctionList)
  --recompile             Force recompile (PReCompile=true)
  --district <int>        ClsObjectFactory._District
  --request-guid <GUID>   Security_RequestGuid (default: empty)
  --encrypt <code>        Security_EncryptCode (default: empty)
  --param Name=Value      Formula parameter (ClsRunRuleResult.SetParam)
  --factory-param K=V     ClsObjectFactory.ParameterList entry
  --all-params            Print all ParametersValue after run
  --dll-path <folder>     Override appSettings:DllPath

CONFIG (App.config):
  connectionStrings:RuleEngine  → ClsCommon.CnRuleString
  connectionStrings:Sara        → used by BIZ.SA / BIZ.SC at runtime
  appSettings:RootGUID          → 2nd arg of ClsCommon.RunRule
  appSettings:DllPath           → folder with BIZ.SC.DLL, SafaClassDesingerNew.dll, ...

EXAMPLE:
  RuleTrace.exe --nidproc a1b2c3d4-.... --formula Solh --watch Calc_Chandganeh --recompile

SQL to find NidProc:
  SELECT TOP 5 r.NidProc, nc.NosaziCode
  FROM Sara8M03.dbo.Sh_Request r
  JOIN Sara8M03.dbo.Base_NosaziCode nc ON nc.NidNosaziCode = r.NidNosaziCode
  WHERE nc.NosaziCode LIKE '%YOUR_CODE%'
  ORDER BY r.ModifyDate DESC;
");
        }

        private sealed class TraceOptions
        {
            public string Formula { get; set; }
            public string NidProc { get; set; }
            public string Watch { get; set; }
            public string EntryPoint { get; set; }
            public Guid RequestGuid { get; set; }
            public string EncryptCode { get; set; }
            public int District { get; set; }
            public bool ReCompile { get; set; }
            public bool ShowAllParams { get; set; }
            public Dictionary<string, string> Parameters { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, string> FactoryParameters { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
