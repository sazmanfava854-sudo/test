using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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

                if (HasFlag(args, "--test-db"))
                    return TestDatabaseConnections(args);

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

            Guid runRuleGuid = ResolveRunRuleGuid(options.CityGuid);
            Console.WriteLine("Formula     : {0} (NidRuleClass={1})", options.Formula, nidRuleClass);
            Console.WriteLine("RunRuleGuid : {0}", runRuleGuid);
            Console.WriteLine("ReCompile   : {0}", options.ReCompile);
            PrintFormulaMemberStats(nidRuleClass, options.ReCompile);

            // ── 1. Compile / cache formula assembly ──
            if (options.ReCompile)
                Console.WriteLine("Compiling   : FULL recompile of all Member XML for NidClass {0} — large formulas can take 5–20 min...", nidRuleClass);
            else
                Console.WriteLine("Compiling   : loading cached assembly for NidClass {0} (fast — omit --recompile unless VB code changed)...", nidRuleClass);
            var compileStarted = DateTime.UtcNow;
            ClsRunRuleResult result = FormulaClsCommon.RunRule(nidRuleClass, runRuleGuid, options.ReCompile);
            Console.WriteLine("Compiling   : done in {0:0.0}s", (DateTime.UtcNow - compileStarted).TotalSeconds);

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

        private static void PrintFormulaMemberStats(int nidRuleClass, bool reCompile)
        {
            string ruleEngine = ConfigurationManager.ConnectionStrings["RuleEngine"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(ruleEngine))
                return;

            string[] queries =
            {
                @"SELECT COUNT(*) AS Cnt, ISNULL(SUM(DATALENGTH(XmlBody)),0) AS TotalBytes
                  FROM dbo.Member WHERE NidClass = @nid",
                @"SELECT COUNT(*) AS Cnt, ISNULL(SUM(DATALENGTH(Body)),0) AS TotalBytes
                  FROM dbo.Member WHERE NidClass = @nid",
                @"SELECT COUNT(*) AS Cnt, 0 AS TotalBytes FROM dbo.Member WHERE NidClass = @nid",
            };

            foreach (string sql in queries)
            {
                try
                {
                    using (var conn = new SqlConnection(ruleEngine))
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@nid", nidRuleClass);
                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                                return;

                            int count = reader.GetInt32(0);
                            long totalBytes = reader.GetInt64(1);
                            string size = totalBytes > 0 ? FormatBytes(totalBytes) : "(unknown)";
                            Console.WriteLine("Member rows : {0} in DbRuleEngein.dbo.Member (total XML ~{1})", count, size);

                            if (reCompile && count >= 5)
                                Console.WriteLine("TIP         : --recompile rebuilds ALL {0} XML bodies — for trace/debug drop --recompile after first successful run.", count);
                            else if (!reCompile)
                                Console.WriteLine("TIP         : using compile cache — add --recompile only when formula VB in Member table changed.");
                            return;
                        }
                    }
                }
                catch
                {
                    // try next column name
                }
            }
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("0.#") + " KB";
            return (bytes / (1024.0 * 1024.0)).ToString("0.#") + " MB";
        }

        private static int TestDatabaseConnections(string[] args)
        {
            Console.WriteLine("=== RuleTrace DB test ===");
            Console.WriteLine("Config file : {0}", AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            _dllPath = GetArg(args, "--dll-path") ?? ConfigurationManager.AppSettings["DllPath"];
            if (!string.IsNullOrWhiteSpace(_dllPath) && Directory.Exists(_dllPath))
            {
                AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
                ConnectionBootstrap.Apply(_dllPath);
            }
            else
            {
                Console.WriteLine("DllPath     : (skipped — testing App.config only)");
            }

            int failures = 0;
            failures += TestSqlConnection("RuleEngine", ConfigurationManager.ConnectionStrings["RuleEngine"]?.ConnectionString);
            failures += TestSqlConnection("Sara", ConfigurationManager.ConnectionStrings["Sara"]?.ConnectionString);

            if (failures == 0)
            {
                Console.WriteLine();
                Console.WriteLine("OK — both databases reachable with debugger login.");
                return 0;
            }

            Console.WriteLine();
            Console.Error.WriteLine("FAILED — {0} connection(s) could not open.", failures);
            return 5;
        }

        private static int TestSqlConnection(string name, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.Error.WriteLine("[{0}] MISSING connection string in config", name);
                return 1;
            }

            if (connectionString.IndexOf("hService", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Console.Error.WriteLine("[{0}] FAIL — config still uses hService", name);
                return 1;
            }

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT DB_NAME(), SUSER_SNAME()", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            Console.WriteLine("[{0}] OK — db={1}, login={2}", name, reader.GetString(0), reader.GetString(1));
                        else
                            Console.WriteLine("[{0}] OK — connected", name);
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[{0}] FAIL — {1}", name, ex.Message);
                return 1;
            }
        }

        private static void SetupConnections()
        {
            Console.WriteLine("Config file : {0}", AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            ConnectionBootstrap.Apply(_dllPath);
            CompileDllPathSync.Sync(_dllPath);

            string encryptionCode = ConfigurationManager.AppSettings["CityGuid"]
                ?? ConfigurationManager.AppSettings["RootGUID"]
                ?? ConfigurationManager.AppSettings["FormulaEncryptionCode"]
                ?? Guid.Empty.ToString();
            TrySetStaticString("BIZ.SC.ClsConnection", "FormulaEncryptionCode", encryptionCode);
            TrySetStaticString("BIZ.SC.ClsProxyHelper", "FormulaEncryptionCode", encryptionCode);
        }

        /// <summary>
        /// 2nd parameter of ClsCommon.RunRule — in many Sara installs this is the city GUID (NidCity),
        /// not a web.config key named RootGUID (often missing in Sara10).
        /// </summary>
        private static Guid ResolveRunRuleGuid(Guid? cityGuidOverride = null)
        {
            if (cityGuidOverride.HasValue && cityGuidOverride.Value != Guid.Empty)
            {
                Console.WriteLine("RunRule key : --city-guid");
                return cityGuidOverride.Value;
            }

            foreach (string key in new[] { "CityGuid", "RootGUID", "FormulaEncryptionCode", "NidCity" })
            {
                string value = ConfigurationManager.AppSettings[key];
                if (TryParseNonEmptyGuid(value, out Guid guid))
                {
                    Console.WriteLine("RunRule key : appSettings:{0}", key);
                    return guid;
                }
            }

            if (TryResolveCityGuidFromDatabase(out Guid cityGuid))
            {
                Console.WriteLine("RunRule key : dbo.CI_City / DefaultCity");
                return cityGuid;
            }

            Console.WriteLine("WARN: RunRuleGuid not set — using Guid.Empty (OK on some Sara10 installs).");
            Console.WriteLine("      Optional: appSettings:CityGuid or --city-guid <GUID>");
            return Guid.Empty;
        }

        private static bool TryParseNonEmptyGuid(string value, out Guid guid)
        {
            guid = Guid.Empty;
            if (string.IsNullOrWhiteSpace(value))
                return false;
            if (!Guid.TryParse(value, out guid) || guid == Guid.Empty)
                return false;
            return true;
        }

        private static bool TryResolveCityGuidFromDatabase(out Guid cityGuid)
        {
            cityGuid = Guid.Empty;
            string cityIdText = ConfigurationManager.AppSettings["DefaultCity"] ?? "2";
            if (!int.TryParse(cityIdText, out int cityId))
                return false;

            string sara = ConfigurationManager.ConnectionStrings["Sara"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(sara))
                return false;

            string[] queries =
            {
                "SELECT TOP 1 NidCity FROM dbo.CI_City WHERE ID = @id",
                "SELECT TOP 1 NidCity FROM dbo.Base_City WHERE ID = @id",
                "SELECT TOP 1 NidProc FROM dbo.CI_City WHERE ID = @id",
            };

            foreach (string sql in queries)
            {
                try
                {
                    using (var conn = new SqlConnection(sara))
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", cityId);
                        conn.Open();
                        object scalar = cmd.ExecuteScalar();
                        if (scalar != null && scalar != DBNull.Value && Guid.TryParse(scalar.ToString(), out Guid g) && g != Guid.Empty)
                        {
                            cityGuid = g;
                            Console.WriteLine("CityGuid    : {0} (DefaultCity={1})", cityGuid, cityId);
                            return true;
                        }
                    }
                }
                catch
                {
                    // try next query shape
                }
            }

            return false;
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

            string cityGuid = GetArg(args, "--city-guid");
            if (!string.IsNullOrWhiteSpace(cityGuid))
                options.CityGuid = Guid.Parse(cityGuid);

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
  --recompile             Force full recompile of ALL Member XML (slow: 20+ large files = minutes)
  --district <int>        ClsObjectFactory._District
  --city-guid <GUID>      RunRule city GUID (if not in web.config — often NidCity for Mashhad)
  --request-guid <GUID>   Security_RequestGuid (default: empty)
  --encrypt <code>        Security_EncryptCode (default: empty)
  --param Name=Value      Formula parameter (ClsRunRuleResult.SetParam)
  --factory-param K=V     ClsObjectFactory.ParameterList entry
  --all-params            Print all ParametersValue after run
  --dll-path <folder>     Override appSettings:DllPath
  --test-db               Test RuleEngine + Sara SQL login only (no formula run)

CONFIG (App.config):
  connectionStrings:RuleEngine  → ClsCommon.CnRuleString
  connectionStrings:Sara        → used by BIZ.SA / BIZ.SC at runtime
  appSettings:CityGuid / DefaultCity → 2nd arg of ClsCommon.RunRule (RootGUID often missing in Sara10)
  appSettings:DllPath           → folder with BIZ.SC.DLL, SafaClassDesingerNew.dll, ...

EXAMPLE:
  RuleTrace.exe --test-db
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
            public Guid? CityGuid { get; set; }
            public bool ReCompile { get; set; }
            public bool ShowAllParams { get; set; }
            public Dictionary<string, string> Parameters { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, string> FactoryParameters { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
