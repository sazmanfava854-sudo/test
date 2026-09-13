using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using SafaClassDesingerNew;
using FormulaClsCommon = SafaClassDesingerNew.ClsCommon;

namespace RuleTrace
{
    /// <summary>
    /// Sara server keeps pre-compiled formula assemblies on disk. A dev PC has no cache,
    /// so RunRule re-merges all Member XML and BC30269 fails. Copy server cache folder here.
    /// </summary>
    internal static class FormulaCacheSync
    {
        public static void Apply(int nidRuleClass, Guid cityGuid)
        {
            string localRoot = ConfigurationManager.AppSettings["FormulaCachePath"];
            if (string.IsNullOrWhiteSpace(localRoot))
                localRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SafaFormulaCache");

            string source = ConfigurationManager.AppSettings["FormulaCacheSource"];
            if (!string.IsNullOrWhiteSpace(source))
                CopyTree(source, localRoot);

            PatchEngineCachePaths(localRoot);
            Console.WriteLine("Formula cache: {0}", localRoot);
            if (string.IsNullOrWhiteSpace(source))
                Console.WriteLine("TIP           : set FormulaCacheSource to server cache folder (ask DBA, near c:\\dll10)");
        }

        public static void DumpEnginePaths()
        {
            Console.WriteLine("=== SafaClassDesingerNew static paths ===");
            DumpTypePaths(typeof(FormulaClsCommon).Assembly, "SafaClassDesingerNew.ClsCommon");
            DumpTypePaths(typeof(FormulaClsCommon).Assembly, "SafaClassDesingerNew.ClsClass");
            DumpTypePaths(typeof(ClsRunRuleResult).Assembly, "SafaClassDesingerNew.ClsRunRuleResult");
        }

        private static void DumpTypePaths(Assembly assembly, string typeName)
        {
            Type type = assembly.GetType(typeName, false);
            if (type == null) return;

            Console.WriteLine("[{0}]", typeName);
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (field.FieldType != typeof(string)) continue;
                object val = field.GetValue(null);
                if (val == null) continue;
                string text = val.ToString();
                if (string.IsNullOrWhiteSpace(text)) continue;
                if (text.IndexOf("\\", StringComparison.Ordinal) >= 0
                    || text.IndexOf("path", StringComparison.OrdinalIgnoreCase) >= 0
                    || text.IndexOf("cache", StringComparison.OrdinalIgnoreCase) >= 0
                    || field.Name.IndexOf("path", StringComparison.OrdinalIgnoreCase) >= 0
                    || field.Name.IndexOf("cache", StringComparison.OrdinalIgnoreCase) >= 0)
                    Console.WriteLine("  {0} = {1}", field.Name, text);
            }
        }

        private static void PatchEngineCachePaths(string localRoot)
        {
            if (string.IsNullOrWhiteSpace(localRoot)) return;

            try
            {
                if (!Directory.Exists(localRoot))
                    Directory.CreateDirectory(localRoot);
            }
            catch
            {
                return;
            }

            string[] types =
            {
                "SafaClassDesingerNew.ClsCommon",
                "SafaClassDesingerNew.ClsClass",
                "SafaClassDesingerNew.ClsRunRuleResult",
            };

            string[] fields =
            {
                "PathCompile", "CompilePath", "CachePath", "AssemblyCache", "AssemblyCachePath",
                "FormulaCachePath", "FormulaPath", "TempPath", "PathTemp", "DllPath",
            };

            foreach (string typeName in types)
            {
                Type type = Type.GetType(typeName + ", SafaClassDesingerNew");
                if (type == null) continue;

                foreach (string fieldName in fields)
                {
                    FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (field != null && field.FieldType == typeof(string))
                    {
                        try { field.SetValue(null, localRoot); } catch { /* optional */ }
                    }
                }
            }
        }

        private static void CopyTree(string source, string target)
        {
            if (!Directory.Exists(source))
            {
                Console.WriteLine("WARN: FormulaCacheSource not found: {0}", source);
                return;
            }

            if (!Directory.Exists(target))
                Directory.CreateDirectory(target);

            int files = 0;
            foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                string relative = file.Substring(source.Length).TrimStart('\\', '/');
                string dest = Path.Combine(target, relative);
                string destDir = Path.GetDirectoryName(dest);
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);
                File.Copy(file, dest, true);
                files++;
            }

            Console.WriteLine("Formula cache: copied {0} file(s) from {1}", files, source);
        }
    }
}
