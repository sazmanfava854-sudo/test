using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace RuleTrace
{
    /// <summary>
    /// Sara formula compiler (vbc) references DLLs from c:\dll10 on the server.
    /// Sync DllPath → LegacyCompileDllPath before RunRule so BC2017 does not fail.
    /// </summary>
    internal static class CompileDllPathSync
    {
        public static void Sync(string sourceDllPath)
        {
            string target = ConfigurationManager.AppSettings["LegacyCompileDllPath"];
            if (string.IsNullOrWhiteSpace(target))
                target = @"c:\dll10";

            if (string.IsNullOrWhiteSpace(sourceDllPath) || !Directory.Exists(sourceDllPath))
            {
                Console.WriteLine("WARN: DllPath missing — cannot sync compile references to {0}", target);
                return;
            }

            PatchFormulaDesignerDllPath(target);

            try
            {
                if (!Directory.Exists(target))
                    Directory.CreateDirectory(target);

                int copied = 0;
                foreach (string file in Directory.GetFiles(sourceDllPath))
                {
                    File.Copy(file, Path.Combine(target, Path.GetFileName(file)), true);
                    copied++;
                }

                Console.WriteLine("Compile DLL  : synced {0} file(s) {1} -> {2}", copied, sourceDllPath, target);

                string bizSc = Path.Combine(target, "BIZ.SC.DLL");
                if (!File.Exists(bizSc) && !File.Exists(Path.Combine(target, "BIZ.SC.dll")))
                    Console.WriteLine("WARN: BIZ.SC.DLL not found under {0} after sync", target);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("WARN: no permission to create {0}", target);
                Console.WriteLine("      Run once as Administrator, or manually:");
                Console.WriteLine("        mkdir c:\\dll10");
                Console.WriteLine("        xcopy /Y \"{0}\\*\" \"c:\\dll10\\\"", sourceDllPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("WARN: compile DLL sync failed for {0}: {1}", target, ex.Message);
            }
        }

        private static void PatchFormulaDesignerDllPath(string targetPath)
        {
            string[] types =
            {
                "SafaClassDesingerNew.ClsCommon",
                "SafaClassDesingerNew.ClsClass",
                "SafaClassDesingerNew.ClsRunRuleResult",
            };

            string[] fields = { "DllPath", "PathDll", "LibPath", "ReferencePath", "AssemblyPath", "DllFolder" };

            foreach (string typeName in types)
            {
                Type type = Type.GetType(typeName + ", SafaClassDesingerNew");
                if (type == null) continue;

                foreach (string fieldName in fields)
                {
                    FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (field != null && field.FieldType == typeof(string))
                    {
                        try { field.SetValue(null, targetPath); } catch { /* optional */ }
                    }
                }
            }
        }
    }
}
