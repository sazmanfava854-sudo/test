using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RuleTrace
{
    internal sealed class RunRequest
    {
        public string Formula = "Solh";
        public string NidProc;
        public string Watch;
        public string EntryPoint;
        public bool ReCompile;
        public bool ClearCache;
        public bool ShowAllParams;
        public int District;
        public Guid RequestGuid;
        public string EncryptCode = string.Empty;
        public Dictionary<string, string> Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> FactoryParameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>One AddError / BizErrors entry, in execution order — the unit the F10 stepper walks over.</summary>
    internal sealed class TraceEvent
    {
        public int Index;
        public string Action;
        public string Key;
        public string Title;
        public override string ToString() { return "[" + Action + "] " + Key + ": " + Title; }
    }

    /// <summary>VB source of one dbo.Member row (extracted from XmlBody) shown in the debug panel.</summary>
    internal sealed class MemberSource
    {
        public int NidMember;
        public string Name;
        public string Meta;
        public string Code;
        public override string ToString() { return NidMember + "  " + Name + "  " + Meta; }
    }

    /// <summary>
    /// Loads Sara DLLs (BIZ.SC, BIZ.SA, SafaClassDesingerNew) at runtime from DllPath and drives
    /// ClsCommon.RunRule / ClsRunRuleResult / ClsObjectFactory through reflection.
    /// No compile-time reference to Sara assemblies -> RuleTrace builds on any machine.
    /// </summary>
    internal sealed class FormulaEngine
    {
        public static readonly Dictionary<string, int> FormulaMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "Global", 432 },
            { "Rule", 336 },
            { "Income", 337 },
            { "Takhalofat", 338 },
            { "CommissionFine", 335 },
            { "Solh", 344 },
            { "Tavafogh", 345 },
            { "Commission", 340 },
            { "Validtion", 341 },
            { "ZabetehConvert", 342 },
            { "Nosazi_Calculate", 339 },
        };

        private const BindingFlags AnyStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        private const BindingFlags AnyInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static readonly object ResolverLock = new object();
        private static string _resolveDir;
        private static bool _resolverInstalled;

        private readonly UserSettings _s;
        private readonly Action<string> _log;
        private Assembly _safa;
        private Assembly _sc;
        private Assembly _sa;

        /// <summary>BizErrors of the last Run, in order (filled even when Watch filters the log).</summary>
        public readonly List<TraceEvent> LastTrace = new List<TraceEvent>();
        /// <summary>ParametersValue snapshot after the last Run.</summary>
        public readonly Dictionary<string, string> LastParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public FormulaEngine(UserSettings settings, Action<string> log)
        {
            _s = settings;
            _log = log ?? (m => { });
        }

        // ───────────────────────────── DLL discovery ─────────────────────────────

        public static string DetectDllPath(string preferred)
        {
            var candidates = new List<string>();
            if (!string.IsNullOrWhiteSpace(preferred)) candidates.Add(preferred);

            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
            candidates.Add(Path.Combine(desktop, "dll10"));
            candidates.Add(Path.Combine(desktop, "dll new", "dll10"));
            candidates.Add(Path.Combine(desktop, "dll new"));
            candidates.Add(Path.Combine(desktop, "dll"));
            candidates.Add(@"c:\dll10");
            candidates.Add(exeDir);
            candidates.Add(Path.Combine(exeDir, "dll10"));

            foreach (string dir in candidates)
            {
                if (IsDllFolder(dir)) return dir;
            }
            return null;
        }

        public static bool IsDllFolder(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir)) return false;
            return FindFile(dir, "SafaClassDesingerNew.dll") != null && FindFile(dir, "BIZ.SC.dll") != null;
        }

        private static string FindFile(string dir, string name)
        {
            try
            {
                return Directory.GetFiles(dir).FirstOrDefault(f =>
                    string.Equals(Path.GetFileName(f), name, StringComparison.OrdinalIgnoreCase));
            }
            catch { return null; }
        }

        // ───────────────────────────── Loading ─────────────────────────────

        public void LoadAssemblies()
        {
            if (!IsDllFolder(_s.DllPath))
                throw new InvalidOperationException("پوشه DLL معتبر نیست (SafaClassDesingerNew.dll / BIZ.SC.DLL پیدا نشد): " + _s.DllPath);

            InstallResolver(_s.DllPath);
            NeutralizeDllConfigs(_s.DllPath);
            NeutralizeDllConfigs(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            _safa = _safa ?? Assembly.LoadFrom(FindFile(_s.DllPath, "SafaClassDesingerNew.dll"));
            _sc = _sc ?? Assembly.LoadFrom(FindFile(_s.DllPath, "BIZ.SC.dll"));
            string sa = FindFile(_s.DllPath, "BIZ.SA.dll");
            if (sa != null && _sa == null) _sa = Assembly.LoadFrom(sa);

            _log("DLLs         : " + _s.DllPath);
            _log("  SafaClassDesingerNew " + _safa.GetName().Version);
            _log("  BIZ.SC               " + _sc.GetName().Version);
            if (_sa != null) _log("  BIZ.SA               " + _sa.GetName().Version);
            FormulaMerger.PatchEngineFlags(_safa, _log);
        }

        private static void InstallResolver(string dir)
        {
            lock (ResolverLock)
            {
                _resolveDir = dir;
                if (_resolverInstalled) return;
                _resolverInstalled = true;
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {
                    string name = new AssemblyName(args.Name).Name;
                    string path = FindFile(_resolveDir, name + ".dll") ?? FindFile(_resolveDir, name + ".exe");
                    return path == null ? null : Assembly.LoadFrom(path);
                };
            }
        }

        private void NeutralizeDllConfigs(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder)) return;
            foreach (string file in Directory.GetFiles(folder, "*.dll.config"))
            {
                try
                {
                    string bak = file + ".bak";
                    if (File.Exists(bak)) File.Delete(bak);
                    File.Move(file, bak);
                    _log("Neutralized  : " + Path.GetFileName(file) + " (may contain hService)");
                }
                catch (Exception ex)
                {
                    _log("WARN         : cannot rename " + Path.GetFileName(file) + ": " + ex.Message);
                }
            }
        }

        // ───────────────────────────── Connections ─────────────────────────────

        public void ApplyConnections()
        {
            Require(_s.RuleEngine, "RuleEngine");
            Require(_s.Sara, "Sara");

            foreach (string n in new[] { "RuleEngine", "DbRuleEngein", "DbRuleEngine", "RuleEngein", "RuleEngineConnection" })
                UpsertConnectionString(n, _s.RuleEngine);
            foreach (string n in new[] { "Sara", "Sara8M03", "SaraConnection", "Sara8M03Connection", "DefaultConnection" })
                UpsertConnectionString(n, _s.Sara);

            SetStaticString(_safa, "SafaClassDesingerNew.ClsCommon", "CnRuleString", _s.RuleEngine);
            PatchConnectionStatics(_safa, "SafaClassDesingerNew.ClsCommon");
            PatchConnectionStatics(_safa, "SafaClassDesingerNew.ClsClass");
            PatchConnectionStatics(_safa, "SafaClassDesingerNew.ClsConnection");
            PatchConnectionStatics(_sc, "BIZ.SC.ClsConnection");
            PatchConnectionStatics(_sc, "BIZ.SC.ClsCommon");
            if (_sa != null)
            {
                PatchConnectionStatics(_sa, "BIZ.SA.ClsCNManagment");
                PatchConnectionStatics(_sa, "BIZ.SA.ClsCNManagement");
                PatchConnectionStatics(_sa, "BIZ.SA.ClsCommon");
                PatchConnectionStatics(_sa, "BIZ.SA.ClsConnection");
            }
            PatchSettings(_safa);
            PatchSettings(_sc);
            PatchSettings(_sa);

            string code = string.IsNullOrWhiteSpace(_s.CityGuid) ? Guid.Empty.ToString() : _s.CityGuid;
            SetStaticString(_sc, "BIZ.SC.ClsConnection", "FormulaEncryptionCode", code);
            SetStaticString(_sc, "BIZ.SC.ClsProxyHelper", "FormulaEncryptionCode", code);

            _log("DB RuleEngine: " + Mask(_s.RuleEngine));
            _log("DB Sara      : " + Mask(_s.Sara));
        }

        private static void Require(string cs, string name)
        {
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("connection string " + name + " خالی است");
            if (cs.IndexOf("hService", StringComparison.OrdinalIgnoreCase) >= 0)
                throw new InvalidOperationException("connection string " + name + " هنوز hService دارد");
        }

        private static void UpsertConnectionString(string name, string value)
        {
            try
            {
                var all = ConfigurationManager.ConnectionStrings;
                MakeWritable(all);
                var existing = all[name];
                if (existing == null)
                {
                    all.Add(new ConnectionStringSettings(name, value, "System.Data.SqlClient"));
                }
                else
                {
                    MakeWritable(existing);
                    existing.ConnectionString = value;
                }
            }
            catch { /* best-effort */ }
        }

        private static void MakeWritable(ConfigurationElement element)
        {
            FieldInfo f = typeof(ConfigurationElement).GetField("_bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
            if (f != null) f.SetValue(element, false);
        }

        private void PatchConnectionStatics(Assembly asm, string typeName)
        {
            Type t = asm == null ? null : asm.GetType(typeName, false);
            if (t == null) return;

            foreach (FieldInfo f in t.GetFields(AnyStatic))
            {
                if (f.FieldType != typeof(string)) continue;
                string val = PickConnection(f.Name, f.GetValue(null) as string);
                if (val != null) Try(() => f.SetValue(null, val));
            }
            foreach (PropertyInfo p in t.GetProperties(AnyStatic))
            {
                if (p.PropertyType != typeof(string) || !p.CanWrite) continue;
                string cur = null;
                Try(() => cur = p.GetValue(null, null) as string);
                string val = PickConnection(p.Name, cur);
                if (val != null) Try(() => p.SetValue(null, val, null));
            }
        }

        private string PickConnection(string memberName, string current)
        {
            bool looksConn = !string.IsNullOrEmpty(current)
                && (current.IndexOf("Server=", StringComparison.OrdinalIgnoreCase) >= 0
                    || current.IndexOf("Data Source=", StringComparison.OrdinalIgnoreCase) >= 0
                    || current.IndexOf("hService", StringComparison.OrdinalIgnoreCase) >= 0);
            bool rule = memberName.IndexOf("Rule", StringComparison.OrdinalIgnoreCase) >= 0
                        || memberName.IndexOf("Engein", StringComparison.OrdinalIgnoreCase) >= 0;
            bool sara = memberName.IndexOf("Sara", StringComparison.OrdinalIgnoreCase) >= 0
                        || memberName.Equals("CnString", StringComparison.OrdinalIgnoreCase);
            bool cn = memberName.StartsWith("Cn", StringComparison.OrdinalIgnoreCase)
                      || memberName.IndexOf("Conn", StringComparison.OrdinalIgnoreCase) >= 0;

            if (!(looksConn || rule || sara || cn)) return null;
            if (rule) return _s.RuleEngine;
            if (sara) return _s.Sara;
            if (looksConn && current.IndexOf("RuleEng", StringComparison.OrdinalIgnoreCase) >= 0) return _s.RuleEngine;
            return _s.Sara;
        }

        private void PatchSettings(Assembly asm)
        {
            if (asm == null) return;
            Type t = asm.GetType(asm.GetName().Name + ".Properties.Settings", false);
            if (t == null) return;
            Try(() =>
            {
                object inst = t.GetProperty("Default", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
                foreach (PropertyInfo p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (p.PropertyType != typeof(string)) continue;
                    string cur = p.GetValue(inst, null) as string;
                    if (string.IsNullOrEmpty(cur) || cur.IndexOf("Server=", StringComparison.OrdinalIgnoreCase) < 0) continue;
                    string val = cur.IndexOf("RuleEng", StringComparison.OrdinalIgnoreCase) >= 0 ? _s.RuleEngine : _s.Sara;
                    Try(() => p.SetValue(inst, val, null));
                }
            });
        }

        private static void SetStaticString(Assembly asm, string typeName, string member, string value)
        {
            Type t = asm == null ? null : asm.GetType(typeName, false);
            if (t == null) return;
            FieldInfo f = t.GetField(member, AnyStatic);
            if (f != null && f.FieldType == typeof(string)) { Try(() => f.SetValue(null, value)); return; }
            PropertyInfo p = t.GetProperty(member, AnyStatic);
            if (p != null && p.PropertyType == typeof(string) && p.CanWrite) Try(() => p.SetValue(null, value, null));
        }

        private static void SetStaticByNames(Assembly asm, string typeName, string[] members, object value)
        {
            Type t = asm == null ? null : asm.GetType(typeName, false);
            if (t == null) return;
            foreach (string m in members)
            {
                FieldInfo f = t.GetField(m, AnyStatic);
                if (f != null && f.FieldType.IsInstanceOfType(value)) Try(() => f.SetValue(null, value));
                PropertyInfo p = t.GetProperty(m, AnyStatic);
                if (p != null && p.CanWrite && p.PropertyType.IsInstanceOfType(value)) Try(() => p.SetValue(null, value, null));
            }
        }

        // ───────────────────────────── DB helpers ─────────────────────────────

        public int TestDatabases()
        {
            int fail = 0;
            fail += TestOne("RuleEngine", _s.RuleEngine) ? 0 : 1;
            fail += TestOne("Sara", _s.Sara) ? 0 : 1;
            return fail;
        }

        private bool TestOne(string name, string cs)
        {
            try
            {
                Require(cs, name);
                using (var c = new SqlConnection(cs))
                using (var cmd = new SqlCommand("SELECT DB_NAME(), SUSER_SNAME()", c))
                {
                    c.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        r.Read();
                        _log("[" + name + "] OK — db=" + r.GetString(0) + ", login=" + r.GetString(1));
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _log("[" + name + "] FAIL — " + ex.Message);
                return false;
            }
        }

        public Guid ResolveCityGuid()
        {
            Guid g;
            if (Guid.TryParse(_s.CityGuid ?? string.Empty, out g) && g != Guid.Empty) return g;

            int cityId;
            if (!int.TryParse(_s.DefaultCity ?? "2", out cityId)) cityId = 2;
            string[] sqls =
            {
                "SELECT TOP 1 NidCity FROM dbo.CI_City WHERE ID = @id",
                "SELECT TOP 1 NidCity FROM dbo.CI_City WHERE CiCity = @id",
                "SELECT TOP 1 NidCity FROM dbo.Base_City WHERE ID = @id",
            };
            foreach (string sql in sqls)
            {
                try
                {
                    using (var c = new SqlConnection(_s.Sara))
                    using (var cmd = new SqlCommand(sql, c))
                    {
                        cmd.Parameters.AddWithValue("@id", cityId);
                        c.Open();
                        object o = cmd.ExecuteScalar();
                        if (o != null && o != DBNull.Value && Guid.TryParse(o.ToString(), out g) && g != Guid.Empty)
                        {
                            _s.CityGuid = g.ToString("D");
                            _log("CityGuid     : " + g + " (from CI_City ID=" + cityId + ")");
                            return g;
                        }
                    }
                }
                catch { }
            }
            _log("WARN         : CityGuid not resolved — using Guid.Empty");
            return Guid.Empty;
        }

        /// <summary>Find NidProc by NidWorkItem, NidProc text, or NosaziCode.</summary>
        public List<string[]> LookupCases(string text)
        {
            var rows = new List<string[]>();
            if (string.IsNullOrWhiteSpace(text)) return rows;
            text = text.Trim();

            string[] sqls =
            {
                @"SELECT TOP 30 CAST(r.NidProc AS NVARCHAR(50)), CAST(r.NidWorkItem AS NVARCHAR(50)), r.WorkflowTitel, r.RequestDate, r.RequesterName
                  FROM dbo.Sh_RequestInfo r
                  WHERE CAST(r.NidWorkItem AS NVARCHAR(50)) = @t OR CAST(r.NidProc AS NVARCHAR(50)) = @t
                  ORDER BY r.RequestDate DESC",
                @"SELECT TOP 30 CAST(r.NidProc AS NVARCHAR(50)), CAST(r.NidWorkItem AS NVARCHAR(50)), r.WorkflowTitel, r.RequestDate, nc.NosaziCode
                  FROM dbo.Sh_RequestInfo r
                  JOIN dbo.Base_NosaziCode nc ON nc.NidNosaziCode = r.NidNosaziCode
                  WHERE nc.NosaziCode LIKE '%' + @t + '%'
                  ORDER BY r.RequestDate DESC",
                @"SELECT TOP 30 CAST(r.NidProc AS NVARCHAR(50)), CAST(r.NidWorkItem AS NVARCHAR(50)), r.WorkflowTitel, r.RequestDate, r.RequesterName
                  FROM dbo.Sh_Request r
                  WHERE CAST(r.NidWorkItem AS NVARCHAR(50)) = @t OR CAST(r.NidProc AS NVARCHAR(50)) = @t",
            };

            foreach (string sql in sqls)
            {
                try
                {
                    using (var c = new SqlConnection(_s.Sara))
                    using (var cmd = new SqlCommand(sql, c))
                    {
                        cmd.Parameters.AddWithValue("@t", text);
                        c.Open();
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                var row = new string[r.FieldCount];
                                for (int i = 0; i < r.FieldCount; i++)
                                    row[i] = r.IsDBNull(i) ? string.Empty : r.GetValue(i).ToString();
                                rows.Add(row);
                            }
                        }
                    }
                    if (rows.Count > 0) return rows;
                }
                catch (Exception ex)
                {
                    _log("lookup skip  : " + FirstLine(ex.Message));
                }
            }
            return rows;
        }

        // ───────────────────────────── Compile support ─────────────────────────────

        private void SyncLegacyDllFolder()
        {
            string target = string.IsNullOrWhiteSpace(_s.LegacyDllPath) ? @"c:\dll10" : _s.LegacyDllPath;
            SetStaticByNames(_safa, "SafaClassDesingerNew.ClsCommon",
                new[] { "DllPath", "PathDll", "LibPath", "ReferencePath", "AssemblyPath", "DllFolder" }, target);

            if (string.Equals(Path.GetFullPath(target).TrimEnd('\\'), Path.GetFullPath(_s.DllPath).TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
            {
                _log("Compile refs : " + target + " (same as DllPath)");
                return;
            }
            try
            {
                Directory.CreateDirectory(target);
                int n = 0;
                foreach (string f in Directory.GetFiles(_s.DllPath))
                {
                    if (f.EndsWith(".config", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".bak", StringComparison.OrdinalIgnoreCase)) continue;
                    string dest = Path.Combine(target, Path.GetFileName(f));
                    if (!File.Exists(dest) || File.GetLastWriteTimeUtc(dest) != File.GetLastWriteTimeUtc(f))
                    {
                        File.Copy(f, dest, true);
                        n++;
                    }
                }
                _log("Compile refs : " + target + " (" + n + " file(s) updated)");
            }
            catch (UnauthorizedAccessException)
            {
                _log("WARN         : no permission for " + target + " — run once as Administrator:  xcopy /Y \"" + _s.DllPath + "\\*\" \"" + target + "\\\"");
            }
            catch (Exception ex)
            {
                _log("WARN         : sync " + target + " failed: " + ex.Message);
            }
        }

        private string PrepareCache(int nidRuleClass, Guid cityGuid, bool clear)
        {
            string root = string.IsNullOrWhiteSpace(_s.CachePath) ? Path.Combine(_s.DllPath, "SafaFormulaCache") : _s.CachePath;
            string folder = Path.Combine(root, cityGuid.ToString("D"), nidRuleClass.ToString());
            try
            {
                if (clear && Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                    _log("Cache        : cleared");
                }
                Directory.CreateDirectory(folder);
            }
            catch (Exception ex)
            {
                _log("WARN         : cache folder: " + ex.Message);
            }

            foreach (string typeName in new[] { "SafaClassDesingerNew.ClsCommon", "SafaClassDesingerNew.ClsClass", "SafaClassDesingerNew.ClsRunRuleResult" })
            {
                SetStaticByNames(_safa, typeName,
                    new[] { "PathCompile", "CompilePath", "CachePath", "AssemblyCache", "AssemblyCachePath", "FormulaCachePath", "FormulaPath", "TempPath", "PathTemp" },
                    folder);
                SetStaticByNames(_safa, typeName, new[] { "NidCity", "CityGuid", "RootGUID" }, cityGuid);
                SetStaticByNames(_safa, typeName, new[] { "NidCity", "CityGuid", "RootGUID" }, cityGuid.ToString("D"));
            }

            int files = Directory.Exists(folder) ? Directory.GetFiles(folder, "*", SearchOption.AllDirectories).Length : 0;
            _log("Cache        : " + folder + (files == 0 ? "  (empty — created after first successful compile)" : "  (" + files + " file(s))"));
            return folder;
        }

        public void PrintMemberStats(int nidRuleClass)
        {
            string[] sqls =
            {
                "SELECT COUNT(*), ISNULL(SUM(DATALENGTH(XmlBody)),0) FROM dbo.Member WHERE NidClass = @nid",
                "SELECT COUNT(*), ISNULL(SUM(DATALENGTH(Body)),0) FROM dbo.Member WHERE NidClass = @nid",
                "SELECT COUNT(*), 0 FROM dbo.Member WHERE NidClass = @nid",
            };
            foreach (string sql in sqls)
            {
                try
                {
                    using (var c = new SqlConnection(_s.RuleEngine))
                    using (var cmd = new SqlCommand(sql, c))
                    {
                        cmd.Parameters.AddWithValue("@nid", nidRuleClass);
                        c.Open();
                        using (var r = cmd.ExecuteReader())
                        {
                            if (!r.Read()) return;
                            int count = Convert.ToInt32(r.GetValue(0));
                            long bytes = Convert.ToInt64(r.GetValue(1));
                            _log("Member rows  : " + count + " in DbRuleEngein.dbo.Member" + (bytes > 0 ? " (~" + (bytes / 1024.0 / 1024.0).ToString("0.#") + " MB XML)" : ""));
                            return;
                        }
                    }
                }
                catch { }
            }
        }

        // ───────────────────────────── Run ─────────────────────────────

        /// <returns>0 ok, 1 stop-error in BizErrors, 3 null result, 4 compile errors</returns>
        public int Run(RunRequest r)
        {
            int nid;
            if (!FormulaMap.TryGetValue(r.Formula ?? string.Empty, out nid))
            {
                int.TryParse(r.Formula ?? string.Empty, out nid);
                if (nid == 0) throw new ArgumentException("فرمول ناشناخته: " + r.Formula);
            }

            Guid cityGuid = ResolveCityGuid();
            _log("Formula      : " + r.Formula + " (NidRuleClass=" + nid + ")");
            _log("CityGuid     : " + cityGuid);
            _log("ReCompile    : " + r.ReCompile);
            PrintMemberStats(nid);
            SyncLegacyDllFolder();
            string cacheFolder = PrepareCache(nid, cityGuid, r.ClearCache);

            Type clsCommon = _safa.GetType("SafaClassDesingerNew.ClsCommon", true);
            MethodInfo runRule = clsCommon.GetMethods(AnyStatic).FirstOrDefault(m => m.Name == "RunRule" && m.GetParameters().Length == 3);
            if (runRule == null) throw new MissingMethodException("SafaClassDesingerNew.ClsCommon.RunRule(int, Guid, bool) not found");

            _log(r.ReCompile
                ? "Compiling    : FULL recompile of all Member XML (may take several minutes)..."
                : "Compiling    : load/compile NidClass " + nid + " ...");
            DateTime t0 = DateTime.UtcNow;
            object result = runRule.Invoke(null, new object[] { nid, cityGuid, r.ReCompile });
            _log("Compiling    : done in " + (DateTime.UtcNow - t0).TotalSeconds.ToString("0.0") + "s");

            if (result == null)
            {
                _log("ERROR        : RunRule returned null (RuleEngine connection / CnRuleString?)");
                return 3;
            }

            object compilerErrors = Get(result, "CompilerErrors");
            if (HasCompilerErrors(compilerErrors))
            {
                if (FormulaMerger.IsMOutDuplicateError(compilerErrors))
                {
                    _log("");
                    _log("=== COMPILE ERRORS (M_Out duplicate — retry with XmlBody inject) ===");
                    LogCompilerErrors(compilerErrors, 12);
                    SaveMergedVb(compilerErrors, cacheFolder);
                    object retry = TryInjectedCompile(nid, cityGuid, r.ReCompile, cacheFolder);
                    if (retry != null && (retry is DirectFormulaHost || !HasCompilerErrors(Get(retry, "CompilerErrors"))))
                    {
                        result = retry;
                        _log("Retry        : compile OK (XmlBody + ToString1 shell + vbc)");
                    }
                }

                if (HasCompilerErrors(Get(result, "CompilerErrors")))
                {
                    _log("");
                    _log("=== COMPILE ERRORS ===");
                    LogCompilerErrors(Get(result, "CompilerErrors"), 40);
                    SaveMergedVb(Get(result, "CompilerErrors"), cacheFolder);
                    _log("");
                    _log("تشخیص: کد در XmlBody/<Body> موجود است (تحلیل Member) اما موتور هنگام merge بدنه را خالی می‌گذارد.");
                    _log("        EncryptXmlBody پر است — احتمالاً موتور از رمزنگاری می‌خواند و روی PC decrypt نمی‌شود.");
                    _log("        RuleTrace inject + merge را امتحان کرد؛ اگر باز خطا داد DLL دقیق سرور Sara لازم است.");
                    try { InspectClass(nid, cityGuid); }
                    catch (Exception ex) { _log("Inspect failed: " + ex.Message); }
                    return 4;
                }
            }

            _log("Compile      : OK");
            ReportCache(cacheFolder);

            object classDesigner = Get(result, "ClassDesinger");
            if (classDesigner == null && result is DirectFormulaHost)
                classDesigner = ((DirectFormulaHost)result).ClassDesigner;
            if (classDesigner != null)
                _log("Class        : " + Get(classDesigner, "Name") + " (FormulaGroup=" + Get(classDesigner, "FormulaGroup") + ")");

            object factory = BuildFactory(r);
            Invoke(result, "SetMyInfo", factory);

            foreach (var kv in r.Parameters)
            {
                Invoke(result, "SetParam", kv.Key, kv.Value);
                _log("SetParam     : " + kv.Key + " = " + kv.Value);
            }

            string entry = r.EntryPoint;
            if (string.IsNullOrWhiteSpace(entry))
            {
                var list = classDesigner == null ? null : Get(classDesigner, "UpdatedFunctionList") as IEnumerable;
                if (list != null) foreach (object o in list) { entry = o == null ? null : o.ToString(); break; }
                if (string.IsNullOrWhiteSpace(entry) && string.Equals(r.Formula, "Solh", StringComparison.OrdinalIgnoreCase)) entry = "Run";
                if (string.IsNullOrWhiteSpace(entry)) entry = "Map_Function";
            }

            _log("");
            _log("=== RUN " + entry + " ===");
            DateTime t1 = DateTime.UtcNow;
            object runResult = Invoke(result, "Run", entry);
            _log("Run          : done in " + (DateTime.UtcNow - t1).TotalSeconds.ToString("0.0") + "s");
            if (runResult != null) _log("Return       : " + runResult);

            _log("");
            _log("=== TRACE (AddError / BizErrors) ===");
            object errorResult = Get(factory, "ErrorResult");
            bool hasStop = PrintBizErrors(errorResult, r.Watch);

            var paramsValue = Get(result, "ParametersValue") as IDictionary;
            if (paramsValue == null && result is DirectFormulaHost)
                paramsValue = ((DirectFormulaHost)result).ParametersValue;
            LastParams.Clear();
            if (paramsValue != null)
            {
                foreach (DictionaryEntry de in paramsValue)
                    Try(() => LastParams[Convert.ToString(de.Key)] = Convert.ToString(de.Value));
                if (!string.IsNullOrWhiteSpace(r.Watch) && paramsValue.Contains(r.Watch))
                    _log("ParametersValue[" + r.Watch + "] = " + paramsValue[r.Watch]);
                if (r.ShowAllParams)
                {
                    _log("");
                    _log("=== ALL ParametersValue (" + paramsValue.Count + ") ===");
                    foreach (DictionaryEntry de in paramsValue) _log("  " + de.Key + " = " + de.Value);
                }
            }

            _log("");
            _log("Done.");
            return hasStop ? 1 : 0;
        }

        private object BuildFactory(RunRequest r)
        {
            var parameterList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(r.NidProc)) parameterList["NidProc"] = r.NidProc.Trim();
            foreach (var kv in r.FactoryParameters) parameterList[kv.Key] = kv.Value;

            Type tFactory = _sc.GetType("BIZ.SC.ClsObjectFactory", true);
            object factory = Activator.CreateInstance(tFactory);

            Set(factory, "ParameterList", parameterList);
            Set(factory, "Security_RequestGuid", r.RequestGuid);
            Set(factory, "Security_EncryptCode", r.EncryptCode ?? string.Empty);
            Set(factory, "_District", r.District);

            Type tErr = _sc.GetType("BIZ.SC.ClsErrorResult", false);
            if (tErr != null) Set(factory, "ErrorResult", Activator.CreateInstance(tErr));

            Type tDto = _sc.GetType("BIZ.SC.DtoClsOperation", false);
            if (tDto != null) Set(factory, "ObjectList", Activator.CreateInstance(typeof(List<>).MakeGenericType(tDto)));

            if (parameterList.Count > 0)
            {
                _log("NidProc      : " + (parameterList.ContainsKey("NidProc") ? parameterList["NidProc"] : "(not set)"));
                Invoke(factory, "LoadObj", Get(factory, "ParameterList"), Get(factory, "ObjectList"));
            }
            else
            {
                _log("WARN         : NidProc خالی است — GetPeace/GetZabeteh برای Solh ممکن است خطا بدهد");
            }
            return factory;
        }

        private bool PrintBizErrors(object errorResult, string watch)
        {
            LastTrace.Clear();
            var errors = errorResult == null ? null : Get(errorResult, "BizErrors") as IEnumerable;
            if (errors == null)
            {
                _log("(no BizErrors)");
                return false;
            }

            var all = errors.Cast<object>().ToList();
            if (all.Count == 0)
            {
                _log("(no BizErrors)");
                return false;
            }

            bool hasStop = false;
            int shown = 0;
            foreach (object e in all)
            {
                string action = Convert.ToString(Get(e, "ErrorAction"));
                string key = Convert.ToString(Get(e, "ErrorKey"));
                string title = Convert.ToString(Get(e, "ErrorTitel"));
                LastTrace.Add(new TraceEvent { Index = LastTrace.Count, Action = action, Key = key, Title = title });
                if (action == "Stop") hasStop = true;

                bool match = string.IsNullOrWhiteSpace(watch)
                    || string.Equals(key, watch, StringComparison.OrdinalIgnoreCase)
                    || (title != null && title.IndexOf(watch, StringComparison.OrdinalIgnoreCase) >= 0);
                if (!match) continue;
                _log("[" + action + "] " + key + ": " + title);
                shown++;
            }

            if (!string.IsNullOrWhiteSpace(watch) && shown == 0)
            {
                _log("(no match for watch='" + watch + "' — total BizErrors=" + all.Count + ", first 20:)");
                foreach (object e in all.Take(20))
                    _log("  [" + Get(e, "ErrorAction") + "] " + Get(e, "ErrorKey") + ": " + Get(e, "ErrorTitel"));
            }
            _log("BizErrors    : " + all.Count + " total, " + shown + " shown");
            return hasStop;
        }

        private void ReportCache(string folder)
        {
            if (!Directory.Exists(folder)) return;
            string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
            if (files.Length == 0) { _log("Cache        : engine did not write files here (uses its own location)"); return; }
            foreach (string f in files.Take(10)) _log("Cached       : " + f);
        }

        private static bool HasCompilerErrors(object compilerErrors)
        {
            return compilerErrors != null && Convert.ToBoolean(Get(compilerErrors, "HasErrors") ?? false);
        }

        private void LogCompilerErrors(object compilerErrors, int max)
        {
            int shown = 0;
            foreach (object e in (IEnumerable)compilerErrors)
            {
                if (shown++ < max) _log("  " + e);
            }
            if (shown > max) _log("  ... (" + shown + " errors total)");
        }

        /// <summary>Inject XmlBody bodies into ClsFunction, build single merged VB, call engine compile APIs.</summary>
        private object TryInjectedCompile(int nid, Guid cityGuid, bool recompile, string cacheFolder)
        {
            try
            {
                var sources = GetMemberSources(nid);
                if (sources.Count == 0 || sources.All(s => s.Code.Length < 50))
                {
                    _log("Retry skip   : no XmlBody <Body> text in Member table");
                    return null;
                }

                _log("Retry        : " + sources.Count + " member(s), " + (sources.Sum(s => (long)s.Code.Length) / 1024) + " KB VB from XmlBody");
                object cls = FormulaMerger.CreateClassWithBodies(_safa, nid, cityGuid, true, sources, _log);
                _log("After inject (ClsFunction.Body lengths):");
                FormulaMerger.LogFunctionBodies(cls, _log, 5);

                string merged = FormulaMerger.BuildMergedVb(cls, sources, _log);
                FormulaMerger.SaveMergedFile(merged, cacheFolder, _log);
                DateTime t0 = DateTime.UtcNow;
                _log("Retry compile: vbc (VBCodeProvider) on merged source...");
                var vbc = FormulaVbcCompiler.Compile(merged, cacheFolder, _s.DllPath, _log);
                object result = null;
                if (vbc.Ok)
                    result = FormulaVbcCompiler.CreateRunHost(_safa, cls, vbc, _log);
                else if (vbc.Errors.Count > 0)
                    _log("VBC FAILED   : see vbc errors above");
                if (result != null)
                    _log("Retry compile: done in " + (DateTime.UtcNow - t0).TotalSeconds.ToString("0.0") + "s");
                else
                    _log("Retry compile: vbc failed — send RuleTrace_merged.vb from cache folder");
                return result;
            }
            catch (Exception ex)
            {
                _log("Retry FAILED : " + ex.Message);
                return null;
            }
        }

        private void SaveMergedVb(object compilerErrors, string cacheFolder)
        {
            try
            {
                foreach (object e in (IEnumerable)compilerErrors)
                {
                    Match m = Regex.Match(e.ToString(), @"([A-Za-z]:\\[^(]+\.vb)\(", RegexOptions.IgnoreCase);
                    if (!m.Success || !File.Exists(m.Groups[1].Value)) continue;
                    string dir = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(cacheFolder)) ?? cacheFolder, "_failed");
                    Directory.CreateDirectory(dir);
                    string dest = Path.Combine(dir, "merged_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".vb");
                    File.Copy(m.Groups[1].Value, dest, true);
                    _log("Merged VB    : " + dest);
                    return;
                }
            }
            catch { }
        }

        // ───────────────────────────── Diagnostics: engine inspect ─────────────────────────────

        /// <summary>
        /// Builds SafaClassDesingerNew.ClsClass(nid, city, false) exactly like RunRule does and dumps what the
        /// engine parsed from dbo.Member (member list, names, body sizes) plus ClsCommon statics.
        /// Shows whether the engine reads member bodies at all (BC30269 = 20 empty shells).
        /// </summary>
        public void InspectClass(int nid, Guid cityGuid)
        {
            _log("");
            _log("=== ENGINE INSPECT (SafaClassDesingerNew " + _safa.GetName().Version + ") ===");

            Type tCommon = _safa.GetType("SafaClassDesingerNew.ClsCommon", false);
            if (tCommon != null) DumpStatics(tCommon);

            Type tCls = _safa.GetType("SafaClassDesingerNew.ClsClass", false);
            if (tCls == null) { _log("ClsClass     : type not found"); return; }

            foreach (ConstructorInfo c in tCls.GetConstructors(AnyInstance))
                _log("ClsClass ctor: (" + string.Join(", ", c.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")");
            foreach (MethodInfo m in tCls.GetMethods(AnyInstance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName).Take(60))
                _log("ClsClass meth: " + m.ReturnType.Name + " " + m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name)) + ")");

            ConstructorInfo ctor = tCls.GetConstructors(AnyInstance).FirstOrDefault(c => c.GetParameters().Length == 3)
                                   ?? tCls.GetConstructors(AnyInstance).OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
            if (ctor == null) { _log("ClsClass     : no constructor"); return; }

            var ps = ctor.GetParameters();
            var args = new object[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                Type pt = ps[i].ParameterType;
                if (pt == typeof(Guid)) args[i] = cityGuid;
                else if (pt == typeof(bool)) args[i] = false;
                else if (pt == typeof(string)) args[i] = cityGuid.ToString("D");
                else args[i] = Coerce(nid, pt);
            }

            object cls;
            DateTime t0 = DateTime.UtcNow;
            try
            {
                cls = ctor.Invoke(args);
            }
            catch (TargetInvocationException tie)
            {
                _log("ClsClass ctor: FAILED " + (tie.InnerException ?? tie).GetType().Name + ": " + (tie.InnerException ?? tie).Message);
                return;
            }
            _log("ClsClass     : loaded in " + (DateTime.UtcNow - t0).TotalSeconds.ToString("0.0") + "s");
            _log("Function bodies (ClsFunction.Body length — 0 = engine did not load code):");
            FormulaMerger.LogFunctionBodies(cls, _log, 20);
            int budget = 400;
            DumpObject(cls, "ClsClass", 0, ref budget);
        }

        private void DumpStatics(Type t)
        {
            foreach (FieldInfo f in t.GetFields(AnyStatic))
            {
                object v = null;
                Try(() => v = f.GetValue(null));
                _log("static       : " + t.Name + "." + f.Name + " = " + Short(v));
            }
        }

        private void DumpObject(object o, string prefix, int depth, ref int budget)
        {
            if (o == null || budget <= 0) return;
            Type t = o.GetType();
            foreach (MemberInfo mi in t.GetProperties(AnyInstance).Cast<MemberInfo>().Concat(t.GetFields(AnyInstance)))
            {
                if (budget-- <= 0) { _log("  ... (truncated)"); return; }
                var pi = mi as PropertyInfo;
                var fi = mi as FieldInfo;
                if (pi != null && pi.GetIndexParameters().Length > 0) continue;
                if (fi != null && fi.Name.EndsWith("BackingField")) continue;

                object v = null;
                string err = null;
                try { v = pi != null ? pi.GetValue(o, null) : fi.GetValue(o); }
                catch (Exception ex) { err = (ex.InnerException ?? ex).Message; }

                string name = prefix + "." + mi.Name;
                if (err != null) { _log("  " + name + " = <error: " + FirstLine(err) + ">"); continue; }
                if (v == null) { _log("  " + name + " = (null)"); continue; }

                Type vt = v.GetType();
                if (IsSimple(vt)) { _log("  " + name + " = " + Short(v)); continue; }

                var en = v as IEnumerable;
                if (en != null)
                {
                    var items = en.Cast<object>().Take(200).ToList();
                    _log("  " + name + " : " + vt.Name + " count=" + items.Count);
                    int shown = 0;
                    foreach (object item in items)
                    {
                        if (shown++ >= 40) { _log("      ... more items"); break; }
                        if (item == null) { _log("      [" + (shown - 1) + "] (null)"); continue; }
                        if (IsSimple(item.GetType())) { _log("      [" + (shown - 1) + "] " + Short(item)); continue; }
                        _log("      [" + (shown - 1) + "] " + Summary(item));
                    }
                    continue;
                }

                if (depth < 1 && vt.Assembly == _safa)
                {
                    _log("  " + name + " : " + vt.Name);
                    DumpObject(v, name, depth + 1, ref budget);
                }
                else
                {
                    _log("  " + name + " : " + vt.Name + " " + Summary(v));
                }
            }
        }

        /// <summary>Single-line view of an engine object: all simple-valued members (name/type/version/active/body length).</summary>
        private static string Summary(object item)
        {
            Type t = item.GetType();
            var parts = new List<string>();
            foreach (MemberInfo mi in t.GetProperties(AnyInstance).Cast<MemberInfo>().Concat(t.GetFields(AnyInstance)))
            {
                var pi = mi as PropertyInfo;
                var fi = mi as FieldInfo;
                if (pi != null && pi.GetIndexParameters().Length > 0) continue;
                if (fi != null && fi.Name.EndsWith("BackingField")) continue;
                object v = null;
                try { v = pi != null ? pi.GetValue(item, null) : fi.GetValue(item); } catch { continue; }
                if (v == null) continue;
                Type vt = v.GetType();
                if (vt == typeof(string))
                {
                    string s = (string)v;
                    parts.Add(mi.Name + "=" + (s.Length > 60 ? "\"" + FirstLine(s.Substring(0, 60)).Replace("\r", "") + "…\"(len " + s.Length + ")" : "\"" + s + "\""));
                }
                else if (IsSimple(vt)) parts.Add(mi.Name + "=" + v);
                else if (v is Array) parts.Add(mi.Name + "[" + ((Array)v).Length + "]");
                else if (v is ICollection) parts.Add(mi.Name + ".Count=" + ((ICollection)v).Count);
                if (parts.Count >= 14) break;
            }
            return t.Name + " { " + string.Join(", ", parts) + " }";
        }

        private static bool IsSimple(Type t)
        {
            return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal) || t == typeof(Guid) || t == typeof(DateTime) || t == typeof(TimeSpan);
        }

        private static string Short(object v)
        {
            if (v == null) return "(null)";
            string s = v as string;
            if (s == null) return v.ToString();
            s = Mask(s);
            return s.Length > 160 ? "\"" + s.Substring(0, 160) + "…\" (len " + ((string)v).Length + ")" : "\"" + s + "\"";
        }

        // ───────────────────────────── Diagnostics: member sources ─────────────────────────────

        /// <summary>Reads dbo.Member rows of a formula and extracts the VB code text from XmlBody for the debug panel.</summary>
        public List<MemberSource> GetMemberSources(int nid)
        {
            var list = new List<MemberSource>();
            string[] sqls =
            {
                "SELECT NidMember, EnumType, isActive, Version, FromDate, ToDate, CAST(XmlBody AS NVARCHAR(MAX)) FROM dbo.Member WHERE NidClass=@nid ORDER BY NidMember, Version",
                "SELECT NidMember, NULL, NULL, NULL, NULL, NULL, CAST(XmlBody AS NVARCHAR(MAX)) FROM dbo.Member WHERE NidClass=@nid ORDER BY NidMember",
            };
            Exception last = null;
            foreach (string sql in sqls)
            {
                try
                {
                    using (var c = new SqlConnection(_s.RuleEngine))
                    using (var cmd = new SqlCommand(sql, c) { CommandTimeout = 120 })
                    {
                        cmd.Parameters.AddWithValue("@nid", nid);
                        c.Open();
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                var m = new MemberSource { NidMember = Convert.ToInt32(r.GetValue(0)) };
                                string xml = r.IsDBNull(6) ? string.Empty : r.GetString(6);
                                string name;
                                m.Code = ExtractCode(xml, out name);
                                m.Name = name ?? "";
                                m.Meta = "v" + Str(r, 3) + " active=" + Str(r, 2) + " type=" + Str(r, 1) + " " + Str(r, 4).Trim() + "→" + Str(r, 5).Trim()
                                         + " (" + (m.Code.Length / 1024) + " KB)";
                                list.Add(m);
                            }
                        }
                    }
                    return list;
                }
                catch (Exception ex) { last = ex; list.Clear(); }
            }
            throw last ?? new InvalidOperationException("Member query failed");
        }

        private static string Str(IDataRecord r, int i)
        {
            try { return r.IsDBNull(i) ? "" : Convert.ToString(r.GetValue(i)); } catch { return ""; }
        }

        /// <summary>Largest text node in the member XML is the VB body; the first short Name/Title-like element is the member name.</summary>
        private static string ExtractCode(string xml, out string name)
        {
            name = null;
            if (string.IsNullOrWhiteSpace(xml)) return string.Empty;
            try
            {
                var doc = System.Xml.Linq.XDocument.Parse(xml, System.Xml.Linq.LoadOptions.PreserveWhitespace);
                var bodyEl = doc.Descendants().FirstOrDefault(e => e.Name.LocalName.Equals("Body", StringComparison.OrdinalIgnoreCase) && !e.HasElements && e.Value.Length > 0);
                if (bodyEl != null)
                {
                    var nameEl = doc.Descendants().FirstOrDefault(e => e.Name.LocalName.Equals("Name", StringComparison.OrdinalIgnoreCase) && !e.HasElements && e.Value.Length > 0 && e.Value.Length < 120);
                    if (nameEl != null) name = nameEl.Value.Trim();
                    return bodyEl.Value.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
                }
                string best = null;
                foreach (var el in doc.Descendants())
                {
                    if (el.HasElements) continue;
                    string v = el.Value;
                    string ln = el.Name.LocalName;
                    if (name == null && v.Length > 0 && v.Length < 120 && !v.Contains("\n") &&
                        (ln.IndexOf("Name", StringComparison.OrdinalIgnoreCase) >= 0 || ln.IndexOf("Title", StringComparison.OrdinalIgnoreCase) >= 0))
                        name = v.Trim();
                    if (best == null || v.Length > best.Length) best = v;
                }
                if (name == null)
                {
                    var attr = doc.Root == null ? null : doc.Root.Attributes().FirstOrDefault(a => a.Name.LocalName.IndexOf("Name", StringComparison.OrdinalIgnoreCase) >= 0);
                    if (attr != null) name = attr.Value;
                }
                return (best ?? xml).Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
            }
            catch
            {
                return xml;
            }
        }

        // ───────────────────────────── reflection helpers ─────────────────────────────

        private static object Get(object o, string name)
        {
            if (o == null) return null;
            Type t = o.GetType();
            PropertyInfo p = t.GetProperty(name, AnyInstance);
            if (p != null) return p.GetValue(o, null);
            FieldInfo f = t.GetField(name, AnyInstance);
            return f == null ? null : f.GetValue(o);
        }

        private void Set(object o, string name, object value)
        {
            Type t = o.GetType();
            PropertyInfo p = t.GetProperty(name, AnyInstance);
            if (p != null && p.CanWrite) { p.SetValue(o, Coerce(value, p.PropertyType), null); return; }
            FieldInfo f = t.GetField(name, AnyInstance);
            if (f != null) { f.SetValue(o, Coerce(value, f.FieldType)); return; }
            _log("WARN         : member " + t.Name + "." + name + " not found (DLL version differs)");
        }

        private static object Coerce(object value, Type target)
        {
            if (value == null || target.IsInstanceOfType(value)) return value;
            try { return Convert.ChangeType(value, target); } catch { return value; }
        }

        private static object Invoke(object o, string method, params object[] args)
        {
            Type t = o.GetType();
            MethodInfo m = t.GetMethods(AnyInstance).FirstOrDefault(x => x.Name == method && x.GetParameters().Length == args.Length);
            if (m == null) throw new MissingMethodException(t.FullName + "." + method + "(" + args.Length + " args)");
            var ps = m.GetParameters();
            var conv = new object[args.Length];
            for (int i = 0; i < args.Length; i++) conv[i] = Coerce(args[i], ps[i].ParameterType);
            try
            {
                return m.Invoke(o, conv);
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException ?? tie;
            }
        }

        private static void Try(Action a) { try { a(); } catch { } }

        private static string FirstLine(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            int i = s.IndexOf('\n');
            return i < 0 ? s : s.Substring(0, i).Trim();
        }

        public static string Mask(string cs)
        {
            if (string.IsNullOrEmpty(cs)) return "(empty)";
            return Regex.Replace(cs, @"(Password|Pwd)\s*=\s*[^;]*", "$1=***", RegexOptions.IgnoreCase);
        }
    }
}
