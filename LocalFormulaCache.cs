using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using FormulaClsCommon = SafaClassDesingerNew.ClsCommon;

namespace RuleTrace
{
    /// <summary>
    /// Build formula compile cache on the local machine (FormulaCachePath).
    /// </summary>
    internal static class LocalFormulaCache
    {
        public static string GetCacheRoot()
        {
            string root = ConfigurationManager.AppSettings["FormulaCachePath"];
            if (string.IsNullOrWhiteSpace(root))
                root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SafaFormulaCache");
            return root;
        }

        public static string GetCacheKeyFolder(int nidRuleClass, Guid cityGuid)
        {
            return Path.Combine(GetCacheRoot(), cityGuid.ToString("D"), nidRuleClass.ToString());
        }

        public static void Prepare(int nidRuleClass, Guid cityGuid, bool clearCache)
        {
            string folder = GetCacheKeyFolder(nidRuleClass, cityGuid);
            if (clearCache && Directory.Exists(folder))
            {
                try
                {
                    Directory.Delete(folder, true);
                    Console.WriteLine("Local cache  : cleared {0}", folder);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("WARN: could not clear cache {0}: {1}", folder, ex.Message);
                }
            }

            try
            {
                Directory.CreateDirectory(folder);
            }
            catch (Exception ex)
            {
                Console.WriteLine("WARN: could not create cache folder {0}: {1}", folder, ex.Message);
            }

            PatchMergeFlags();
            Console.WriteLine("Local cache  : {0}", folder);
        }

        public static bool TrySeedFromDatabase(int nidRuleClass, Guid cityGuid)
        {
            string ruleEngine = ConfigurationManager.ConnectionStrings["RuleEngine"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(ruleEngine))
                return false;

            string folder = GetCacheKeyFolder(nidRuleClass, cityGuid);
            Directory.CreateDirectory(folder);

            string[] blobColumns = { "Assembly", "AssemblyBody", "DllBody", "CompiledAssembly", "CacheBody", "AssemblyBin", "Dll" };
            foreach (string column in blobColumns)
            {
                try
                {
                    string sql = string.Format(
                        "SELECT TOP 1 [{0}] FROM dbo.RuleClass WHERE NidClass = @nid OR ID = @nid",
                        column);

                    using (var conn = new SqlConnection(ruleEngine))
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@nid", nidRuleClass);
                        conn.Open();
                        object value = cmd.ExecuteScalar();
                        if (value == null || value == DBNull.Value)
                            continue;

                        byte[] bytes = value as byte[];
                        if (bytes == null || bytes.Length == 0)
                            continue;

                        string dllPath = Path.Combine(folder, "RuleClass_" + nidRuleClass + ".dll");
                        File.WriteAllBytes(dllPath, bytes);
                        Console.WriteLine("Local cache  : seeded {0} bytes from RuleClass.{1}", bytes.Length, column);
                        return true;
                    }
                }
                catch
                {
                    // column may not exist
                }
            }

            return false;
        }

        public static void SaveCompileArtifacts(object compilerErrors)
        {
            if (compilerErrors == null) return;

            try
            {
                foreach (object err in (System.Collections.IEnumerable)compilerErrors)
                {
                    string text = err?.ToString() ?? string.Empty;
                    Match match = Regex.Match(text, @"([A-Za-z]:\\[^:]+\.vb)\(", RegexOptions.IgnoreCase);
                    if (!match.Success) continue;

                    string vbPath = match.Groups[1].Value;
                    if (!File.Exists(vbPath)) continue;

                    string destDir = Path.Combine(GetCacheRoot(), "_failed");
                    Directory.CreateDirectory(destDir);
                    string dest = Path.Combine(destDir, Path.GetFileName(vbPath));
                    File.Copy(vbPath, dest, true);
                    Console.Error.WriteLine("Saved merged VB: {0}", dest);
                    return;
                }
            }
            catch
            {
                // optional
            }
        }

        private static void PatchMergeFlags()
        {
            string[] types =
            {
                "SafaClassDesingerNew.ClsCommon",
                "SafaClassDesingerNew.ClsClass",
                "SafaClassDesingerNew.ClsRunRuleResult",
            };

            foreach (string typeName in types)
            {
                Type type = Type.GetType(typeName + ", SafaClassDesingerNew");
                if (type == null) continue;

                foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    string name = field.Name;
                    if (field.FieldType == typeof(bool)
                        && (name.IndexOf("Partial", StringComparison.OrdinalIgnoreCase) >= 0
                            || name.IndexOf("Merge", StringComparison.OrdinalIgnoreCase) >= 0
                            || name.IndexOf("BodyOnly", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        try { field.SetValue(null, true); } catch { /* optional */ }
                    }
                }
            }
        }
    }
}
