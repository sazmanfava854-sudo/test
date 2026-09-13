using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            File.WriteAllText(vbPath, mergedVb, System.Text.Encoding.UTF8);

            var refs = CollectReferences(dllFolder);
            log("VBC compile  : " + refs.Count + " reference DLL(s), source " + (mergedVb.Length / 1024) + " KB");

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

            if (cr.Errors.HasErrors)
            {
                int shown = 0;
                foreach (CompilerError e in cr.Errors)
                {
                    if (e.IsWarning) continue;
                    string line = "  vbc : " + e.ErrorText;
                    outcome.Errors.Add(line);
                    if (shown++ < 25) log(line);
                }
                if (shown > 25) log("  ... (" + shown + " vbc errors)");
                return outcome;
            }

            outcome.Assembly = cr.CompiledAssembly ?? (File.Exists(dllPath) ? Assembly.LoadFrom(dllPath) : null);
            outcome.OutputPath = dllPath;
            if (outcome.Assembly == null)
            {
                outcome.Errors.Add("vbc produced no assembly");
                return outcome;
            }

            outcome.FormulaType = outcome.Assembly.GetTypes()
                .FirstOrDefault(t => t.IsClass && !t.IsAbstract && t.IsPublic && t.GetMethods().Any(m => m.Name.Equals("Run", StringComparison.OrdinalIgnoreCase)))
                ?? outcome.Assembly.GetTypes().FirstOrDefault(t => t.IsClass && t.IsPublic && t.Name.IndexOf("Solh", StringComparison.OrdinalIgnoreCase) >= 0);

            if (outcome.FormulaType == null)
            {
                outcome.Errors.Add("no formula type with Run() in compiled assembly");
                log("VBC types    : " + string.Join(", ", outcome.Assembly.GetTypes().Select(t => t.FullName).Take(15)));
                return outcome;
            }

            log("VBC OK       : " + outcome.FormulaType.FullName + " -> " + dllPath);
            return outcome;
        }

        private static List<string> CollectReferences(string dllFolder)
        {
            var list = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (Directory.Exists(dllFolder))
            {
                foreach (string name in new[] { "BIZ.SC.dll", "BIZ.SA.dll", "SafaClassDesingerNew.dll", "Newtonsoft.Json.dll" })
                    AddRef(list, seen, Path.Combine(dllFolder, name));
                foreach (string f in Directory.GetFiles(dllFolder, "*.dll"))
                    AddRef(list, seen, f);
            }

            AddRef(list, seen, typeof(object).Assembly.Location);
            AddRef(list, seen, typeof(System.Data.DataTable).Assembly.Location);
            AddRef(list, seen, typeof(Microsoft.VisualBasic.Strings).Assembly.Location);
            AddRef(list, seen, Assembly.Load("System.Core").Location);
            AddRef(list, seen, Assembly.Load("System.Xml").Location);
            return list;
        }

        private static void AddRef(List<string> list, HashSet<string> seen, string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
            string full = Path.GetFullPath(path);
            if (seen.Add(full)) list.Add(full);
        }

        /// <summary>Wire compiled assembly into ClsRunRuleResult if possible; else return a lightweight host.</summary>
        public static object CreateRunHost(Assembly safa, object cls, CompileOutcome compiled, Action<string> log)
        {
            Type tResult = safa.GetType("SafaClassDesingerNew.ClsRunRuleResult", false);
            if (tResult != null)
            {
                try
                {
                    object result = Activator.CreateInstance(tResult);
                    Patch(result, cls, compiled.Assembly, compiled.FormulaType);
                    log("Run host     : ClsRunRuleResult wrapper");
                    return result;
                }
                catch (Exception ex) { log("Run host     : ClsRunRuleResult failed — " + ex.Message); }
            }

            log("Run host     : DirectFormulaHost (ClsRunRuleResult not wired)");
            return new DirectFormulaHost { ClassDesigner = cls, Assembly = compiled.Assembly, FormulaType = compiled.FormulaType };
        }

        private static void Patch(object result, object cls, Assembly asm, Type formulaType)
        {
            string[] clsNames = { "ClassDesinger", "ClassDesigner", "ClsClass", "M_Class" };
            string[] asmNames = { "Assembly", "MyAssembly", "CompiledAssembly", "FormulaAssembly", "Assem", "M_Assembly" };
            string[] typeNames = { "TypeClass", "FormulaType", "ClassType", "M_Type" };
            string[] instNames = { "Instance", "ClassInstance", "ObjClass", "M_Instance" };

            foreach (string n in clsNames) TrySet(result, n, cls);
            foreach (string n in asmNames) TrySet(result, n, asm);
            foreach (string n in typeNames) TrySet(result, n, formulaType);
            if (formulaType != null)
            {
                object inst = Activator.CreateInstance(formulaType);
                foreach (string n in instNames) TrySet(result, n, inst);
            }
        }

        private static void TrySet(object o, string name, object value)
        {
            Type t = o.GetType();
            const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            PropertyInfo p = t.GetProperty(name, f);
            if (p != null && p.CanWrite) { try { p.SetValue(o, value, null); } catch { } return; }
            FieldInfo fi = t.GetField(name, f);
            if (fi != null) try { fi.SetValue(o, value); } catch { }
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

        public void SetParam(string key, object value) { _params[key] = value; }

        public object Run(string entry)
        {
            if (_instance == null) SetMyInfo(_factory);
            MethodInfo m = FormulaType.GetMethod(entry ?? "Run", BindingFlags.Public | BindingFlags.Instance)
                           ?? FormulaType.GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(x => x.Name.Equals("Run", StringComparison.OrdinalIgnoreCase) && x.GetParameters().Length == 0);
            if (m == null) throw new MissingMethodException(FormulaType.FullName + "." + entry);
            return m.Invoke(_instance, null);
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
                return d;
            }
        }

        private static void TrySet(object o, string name, object value)
        {
            Type t = o.GetType();
            PropertyInfo p = t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (p != null && p.CanWrite) { try { p.SetValue(o, value, null); } catch { } return; }
            FieldInfo f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null) try { f.SetValue(o, value); } catch { }
        }
    }
}
