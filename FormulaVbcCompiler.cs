using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

namespace RuleTrace
{
    /// <summary>Compile merged formula VB with vbc/VBCodeProvider when SafaClassDesingerNew compile APIs are unavailable.</summary>
    internal static class FormulaVbcCompiler
    {
        public sealed class CompileOutcome
        {
            public Assembly Assembly;
            public string OutputPath;
            public Type FormulaType;
            public readonly List<string> Errors = new List<string>();
            public bool Ok { get { return Assembly != null && FormulaType != null; } }
        }

        public static CompileOutcome Compile(string mergedVb, string cacheFolder, string dllFolder, Action<string> log)
        {
            var outcome = new CompileOutcome();
            if (string.IsNullOrWhiteSpace(mergedVb) || mergedVb.Length < 200)
            {
                outcome.Errors.Add("merged VB too short");
                return outcome;
            }

            Directory.CreateDirectory(cacheFolder);
            string vbPath = Path.Combine(cacheFolder, "RuleTrace_merged.vb");
            string dllPath = Path.Combine(cacheFolder, "Solh_ruletrace.dll");
            File.WriteAllText(vbPath, mergedVb, Encoding.UTF8);

            List<string> refs = CollectReferences(dllFolder, log);
            log("VBC compile  : " + refs.Count + " reference DLL(s), source " + (mergedVb.Length / 1024) + " KB");

            outcome = TryCompileWithCodeDom(vbPath, dllPath, cacheFolder, refs, log);
            if (!outcome.Ok)
            {
                outcome = TryRemedyAndRecompile(ref mergedVb, outcome, vbPath, dllPath, cacheFolder, refs, log);
            }
            if (outcome.Ok) return outcome;

            log("VBC retry    : VBCodeProvider failed — trying vbc.exe ...");
            var exeOutcome = TryCompileWithVbcExe(vbPath, dllPath, dllFolder, log);
            if (!exeOutcome.Ok)
            {
                var missing = ExtractUndeclaredVariables(exeOutcome.Errors);
                if (missing.Count > 0)
                {
                    log("VBC auto-fix : " + missing.Count + " undeclared variable(s) from vbc.exe — adding property stubs...");
                    mergedVb = FormulaMerger.InjectPropertyStubs(mergedVb, missing);
                    File.WriteAllText(vbPath, mergedVb, Encoding.UTF8);
                    exeOutcome = TryCompileWithVbcExe(vbPath, dllPath, dllFolder, log);
                }
            }
            if (exeOutcome.Ok) return exeOutcome;

            foreach (string e in exeOutcome.Errors)
                if (!outcome.Errors.Contains(e)) outcome.Errors.Add(e);
            return outcome;
        }

        private static CompileOutcome TryRemedyAndRecompile(
            ref string mergedVb, CompileOutcome failedOutcome,
            string vbPath, string dllPath, string cacheFolder,
            List<string> refs, Action<string> log)
        {
            var outcome = failedOutcome;
            for (int attempt = 0; attempt < 3 && !outcome.Ok; attempt++)
            {
                var missing = ExtractUndeclaredVariables(outcome.Errors);
                if (missing.Count == 0) break;

                log("VBC auto-fix : " + missing.Count + " undeclared variable(s) found (" + string.Join(", ", missing.Take(8)) + (missing.Count > 8 ? "..." : "") + ") — adding property stubs...");
                mergedVb = FormulaMerger.InjectPropertyStubs(mergedVb, missing);
                File.WriteAllText(vbPath, mergedVb, Encoding.UTF8);

                outcome = TryCompileWithCodeDom(vbPath, dllPath, cacheFolder, refs, log);
                if (outcome.Ok)
                {
                    log("VBC auto-fix : compile OK after adding property stub(s)!");
                    return outcome;
                }
            }
            return outcome;
        }

        private static List<string> ExtractUndeclaredVariables(IEnumerable<string> errors)
        {
            var list = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (errors == null) return list.ToList();
            var rx = new Regex(@"'(\w+)'\s+is\s+not\s+declared", RegexOptions.IgnoreCase);
            foreach (string e in errors)
            {
                if (string.IsNullOrWhiteSpace(e)) continue;
                foreach (Match m in rx.Matches(e))
                {
                    string id = m.Groups[1].Value;
                    if (!string.IsNullOrWhiteSpace(id) && Regex.IsMatch(id, @"^[A-Za-z_]\w*$"))
                        list.Add(id);
                }
            }
            return list.ToList();
        }

        private static CompileOutcome TryCompileWithCodeDom(string vbPath, string dllPath, string cacheFolder, List<string> refs, Action<string> log)
        {
            var outcome = new CompileOutcome();
            try
            {
                var parms = new CompilerParameters
                {
                    GenerateExecutable = false,
                    GenerateInMemory = false,
                    OutputAssembly = dllPath,
                    CompilerOptions = "/optionstrict- /optioninfer+ /nowarn:42016,42017,42018,42019,42032",
                    TempFiles = new TempFileCollection(cacheFolder, false),
                };
                foreach (string r in refs) parms.ReferencedAssemblies.Add(r);

                CompilerResults cr;
                using (var prov = new VBCodeProvider())
                    cr = prov.CompileAssemblyFromFile(parms, vbPath);

                return FinishCompileOutcome(cr, dllPath, log, outcome);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null) msg += " — " + ex.InnerException.Message;
                outcome.Errors.Add("VBCodeProvider: " + msg);
                log("VBC warn     : " + msg);
                return outcome;
            }
        }

        private static CompileOutcome TryCompileWithVbcExe(string vbPath, string dllPath, string dllFolder, Action<string> log)
        {
            var outcome = new CompileOutcome();
            string vbc = FindVbcExe();
            if (string.IsNullOrEmpty(vbc))
            {
                outcome.Errors.Add("vbc.exe not found");
                log("VBC warn     : vbc.exe not found under .NET Framework");
                return outcome;
            }

            List<string> refs = CollectReferencesForVbcExe(dllFolder, log);
            var args = new StringBuilder();
            args.Append("/nologo /target:library /optionstrict- /optioninfer+ ");
            args.Append("/out:\"").Append(dllPath).Append("\" ");
            foreach (string r in refs)
                args.Append("/reference:\"").Append(r).Append("\" ");
            args.Append("\"").Append(vbPath).Append("\"");

            log("VBC exe      : " + vbc);
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = vbc,
                    Arguments = args.ToString(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };
                using (var p = Process.Start(psi))
                {
                    string stdout = p.StandardOutput.ReadToEnd();
                    string stderr = p.StandardError.ReadToEnd();
                    p.WaitForExit(300000);
                    if (p.ExitCode != 0)
                    {
                        int shown = 0;
                        foreach (string line in (stdout + "\n" + stderr).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            string t = line.Trim();
                            if (t.Length == 0) continue;
                            outcome.Errors.Add("vbc.exe : " + t);
                            if (shown++ < 25) log("  " + t);
                        }
                        if (outcome.Errors.Count == 0)
                        {
                            outcome.Errors.Add("vbc.exe exit " + p.ExitCode);
                            log("VBC FAILED   : vbc.exe exit " + p.ExitCode);
                        }
                        return outcome;
                    }
                }
            }
            catch (Exception ex)
            {
                outcome.Errors.Add("vbc.exe: " + ex.Message);
                log("VBC warn     : " + ex.Message);
                return outcome;
            }

            if (!File.Exists(dllPath))
            {
                outcome.Errors.Add("vbc.exe produced no DLL");
                return outcome;
            }

            try
            {
                outcome.Assembly = Assembly.LoadFrom(dllPath);
                outcome.OutputPath = dllPath;
                outcome.FormulaType = FindFormulaType(outcome.Assembly);
                if (outcome.FormulaType == null)
                {
                    outcome.Errors.Add("no formula type with Run() in vbc.exe output");
                    return outcome;
                }
                log("VBC OK       : " + outcome.FormulaType.FullName + " -> " + dllPath + " (vbc.exe)");
                return outcome;
            }
            catch (Exception ex)
            {
                outcome.Errors.Add("load DLL: " + ex.Message);
                log("VBC warn     : " + ex.Message);
                return outcome;
            }
        }

        private static CompileOutcome FinishCompileOutcome(CompilerResults cr, string dllPath, Action<string> log, CompileOutcome outcome)
        {
            if (cr.Errors.HasErrors)
            {
                int shown = 0;
                foreach (CompilerError e in cr.Errors)
                {
                    if (e.IsWarning) continue;
                    string line = "  vbc : (" + e.Line + "," + e.Column + ") " + e.ErrorText;
                    outcome.Errors.Add(line);
                    if (shown++ < 25) log(line);
                }
                if (shown > 25) log("  ... (" + shown + " vbc errors)");
                return outcome;
            }

            outcome.Assembly = cr.CompiledAssembly;
            if (outcome.Assembly == null && File.Exists(dllPath))
            {
                try { outcome.Assembly = Assembly.LoadFrom(dllPath); }
                catch (Exception ex) { outcome.Errors.Add("load DLL: " + ex.Message); return outcome; }
            }
            outcome.OutputPath = dllPath;
            if (outcome.Assembly == null)
            {
                outcome.Errors.Add("vbc produced no assembly");
                return outcome;
            }

            outcome.FormulaType = FindFormulaType(outcome.Assembly);
            if (outcome.FormulaType == null)
            {
                outcome.Errors.Add("no formula type with Run() in compiled assembly");
                log("VBC types    : " + string.Join(", ", outcome.Assembly.GetTypes().Select(t => t.FullName).Take(15)));
                return outcome;
            }

            log("VBC OK       : " + outcome.FormulaType.FullName + " -> " + dllPath);
            return outcome;
        }

        private static Type FindFormulaType(Assembly asm)
        {
            return asm.GetTypes()
                .FirstOrDefault(t => t.IsClass && !t.IsAbstract && t.IsPublic && t.GetMethods().Any(m => m.Name.Equals("Run", StringComparison.OrdinalIgnoreCase)))
                ?? asm.GetTypes().FirstOrDefault(t => t.IsClass && t.IsPublic && t.Name.IndexOf("Solh", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static string FindVbcExe()
        {
            string rt = RuntimeEnvironment.GetRuntimeDirectory();
            string path = Path.Combine(rt, "vbc.exe");
            if (File.Exists(path)) return path;

            string windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            foreach (string rel in new[]
            {
                @"Microsoft.NET\Framework64\v4.0.30319\vbc.exe",
                @"Microsoft.NET\Framework\v4.0.30319\vbc.exe",
            })
            {
                path = Path.Combine(windir, rel);
                if (File.Exists(path)) return path;
            }
            return null;
        }

        private static List<string> CollectReferences(string dllFolder, Action<string> log)
        {
            var list = new List<string>();
            var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Formula DLLs only — do NOT glob dll10/*.dll (duplicate System.ServiceModel etc. breaks vbc.exe BC2000).
            if (Directory.Exists(dllFolder))
            {
                foreach (string name in new[] { "BIZ.SC.dll", "BIZ.SA.dll", "SafaClassDesingerNew.dll", "Newtonsoft.Json.dll" })
                    AddRef(list, seenPaths, seenNames, Path.Combine(dllFolder, name));
            }

            AddTypeRef(list, seenPaths, seenNames, typeof(object));
            AddTypeRef(list, seenPaths, seenNames, typeof(System.Linq.Enumerable));
            AddTypeRef(list, seenPaths, seenNames, typeof(System.Data.DataTable));
            AddTypeRef(list, seenPaths, seenNames, typeof(System.Xml.XmlDocument));
            AddTypeRef(list, seenPaths, seenNames, typeof(System.Xml.Linq.XDocument));
            AddTypeRef(list, seenPaths, seenNames, typeof(System.ComponentModel.Component));
            AddTypeRef(list, seenPaths, seenNames, typeof(Microsoft.VisualBasic.Strings));
            AddTypeRef(list, seenPaths, seenNames, typeof(System.Runtime.Serialization.DataContractAttribute));
            AddTypeRef(list, seenPaths, seenNames, typeof(System.Configuration.ConfigurationManager));

            log("VBC refs     : " + list.Count + " assemblies (deduped by name)");
            if (list.Count < 5)
                log("VBC warn     : only " + list.Count + " refs resolved — check .NET Framework install");
            return list;
        }

        /// <summary>vbc.exe auto-references mscorlib/System/System.Core/System.Data — formula DLLs only.</summary>
        private static List<string> CollectReferencesForVbcExe(string dllFolder, Action<string> log)
        {
            var list = new List<string>();
            var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (Directory.Exists(dllFolder))
            {
                foreach (string name in new[] { "BIZ.SC.dll", "BIZ.SA.dll", "SafaClassDesingerNew.dll", "Newtonsoft.Json.dll" })
                    AddRef(list, seenPaths, seenNames, Path.Combine(dllFolder, name));
            }

            log("VBC exe refs : " + list.Count + " formula DLL(s) only (framework refs implicit)");
            return list;
        }

        private static void AddTypeRef(List<string> list, HashSet<string> seenPaths, HashSet<string> seenNames, Type t)
        {
            try
            {
                if (t == null) return;
                string loc = t.Assembly.Location;
                if (!string.IsNullOrEmpty(loc)) AddRef(list, seenPaths, seenNames, loc);
            }
            catch { }
        }

        private static void AddRef(List<string> list, HashSet<string> seenPaths, HashSet<string> seenNames, string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
            string full = Path.GetFullPath(path);
            if (!seenPaths.Add(full)) return;
            try
            {
                string asmName = AssemblyName.GetAssemblyName(full).Name;
                if (!seenNames.Add(asmName))
                {
                    seenPaths.Remove(full);
                    return;
                }
            }
            catch { }
            list.Add(full);
        }

        /// <summary>Run compiled formula via DirectFormulaHost (reliable; avoids broken ClsRunRuleResult after vbc).</summary>
        public static object CreateRunHost(Assembly safa, object cls, CompileOutcome compiled, Action<string> log)
        {
            log("Run host     : DirectFormulaHost -> " + compiled.FormulaType.FullName);
            return new DirectFormulaHost { ClassDesigner = cls, Assembly = compiled.Assembly, FormulaType = compiled.FormulaType };
        }

    }

    /// <summary>Fallback when ClsRunRuleResult cannot be constructed — runs compiled Solh via reflection.</summary>
    internal sealed class DirectFormulaHost
    {
        public object ClassDesigner;
        public Assembly Assembly;
        public Type FormulaType;
        private object _instance;
        private object _factory;
        private readonly Dictionary<string, object> _params = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public void SetMyInfo(object factory)
        {
            _factory = factory;
            _instance = Activator.CreateInstance(FormulaType);
            foreach (string n in new[] { "Info8", "Info", "MyInfo", "M_Info" })
                TrySet(_instance, n, factory);
        }

        public void SetParam(string key, object value)
        {
            _params[key] = value;
            if (_instance != null) TrySet(_instance, key, value);
        }

        public object Run(string entry)
        {
            if (_instance == null) SetMyInfo(_factory);
            foreach (var kv in _params) TrySet(_instance, kv.Key, kv.Value);
            string method = entry ?? "Run";
            MethodInfo m = FormulaType.GetMethod(method, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                           ?? FormulaType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                               .FirstOrDefault(x => x.Name.Equals(method, StringComparison.OrdinalIgnoreCase));
            if (m == null) throw new MissingMethodException(FormulaType.FullName + "." + method);
            var ps = m.GetParameters();
            object[] args = ps.Length == 0 ? null : new object[ps.Length];
            return m.Invoke(_instance, args);
        }

        public IDictionary ParametersValue
        {
            get
            {
                var d = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                if (_instance == null) return d;
                foreach (PropertyInfo p in FormulaType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (p.GetIndexParameters().Length > 0) continue;
                    try { d[p.Name] = p.GetValue(_instance, null); } catch { }
                }
                foreach (FieldInfo f in FormulaType.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    try { if (!d.ContainsKey(f.Name)) d[f.Name] = f.GetValue(_instance); } catch { }
                }
                return d;
            }
        }

        private static void TrySet(object o, string name, object value)
        {
            if (o == null || string.IsNullOrWhiteSpace(name)) return;
            Type t = o.GetType();
            PropertyInfo p = t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p != null && p.CanWrite)
            {
                try
                {
                    object v = value;
                    if (v != null && p.PropertyType != typeof(object) && !p.PropertyType.IsAssignableFrom(v.GetType()))
                        v = Convert.ChangeType(v, p.PropertyType);
                    p.SetValue(o, v, null);
                    return;
                }
                catch { }
            }
            FieldInfo f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (f != null)
            {
                try
                {
                    object v = value;
                    if (v != null && f.FieldType != typeof(object) && !f.FieldType.IsAssignableFrom(v.GetType()))
                        v = Convert.ChangeType(v, f.FieldType);
                    f.SetValue(o, v);
                }
                catch { }
            }
        }
    }
}
