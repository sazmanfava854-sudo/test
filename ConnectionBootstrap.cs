using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using BIZ.SA;
using BIZ.SC;
using FormulaClsCommon = SafaClassDesingerNew.ClsCommon;

namespace RuleTrace
{
    /// <summary>
    /// Sara DLLs read connection strings from many places (CnRuleString, ConfigurationManager,
    /// ClsCNManagment static ctor, Properties.Settings, sidecar *.dll.config).
    /// This forces App.config credentials (debugger) everywhere before the formula engine runs.
    /// </summary>
    internal static class ConnectionBootstrap
    {
        private static readonly string[] RuleEngineNames =
        {
            "RuleEngine", "DbRuleEngein", "DbRuleEngine", "RuleEngein", "RuleEngineConnection",
            "SafaClassDesingerNew.Properties.Settings.DbRuleEngeinConnectionString",
            "SafaClassDesingerNew.Properties.Settings.RuleEngineConnectionString",
        };

        private static readonly string[] SaraNames =
        {
            "Sara", "Sara8M03", "SaraConnection", "Sara8M03Connection", "DefaultConnection",
            "SafaClassDesingerNew.Properties.Settings.SaraConnectionString",
            "SafaClassDesingerNew.Properties.Settings.Sara8M03ConnectionString",
            "BIZ.SA.Properties.Settings.SaraConnectionString",
            "BIZ.SC.Properties.Settings.SaraConnectionString",
        };

        public static void Apply(string dllPath = null)
        {
            string ruleEngine = Required("RuleEngine");
            string sara = Required("Sara");
            string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";

            NeutralizeDllConfigs(exeDir);
            if (!string.IsNullOrWhiteSpace(dllPath))
                NeutralizeDllConfigs(dllPath);

            EnsureConnectionStrings(ruleEngine, sara);

            // Primary hook used by SafaClassDesingerNew.ClsClass.LoadObj
            FormulaClsCommon.CnRuleString = ruleEngine;

            // Force-load Sara assemblies (static ctors may reset fields) then patch again
            TouchType("BIZ.SA.ClsCNManagment", "BIZ.SA");
            TouchType("BIZ.SA.ClsCommon", "BIZ.SA");
            TouchType("BIZ.SC.ClsConnection", "BIZ.SC");
            TouchType("SafaClassDesingerNew.ClsClass", "SafaClassDesingerNew");

            PatchStaticConnectionFields(ruleEngine, sara);
            PatchAssemblySettings(typeof(FormulaClsCommon).Assembly, ruleEngine, sara);
            PatchAssemblySettings(typeof(BIZ.SA.ClsCNManagment).Assembly, ruleEngine, sara);
            PatchAssemblySettings(typeof(BIZ.SC.ClsObjectFactory).Assembly, ruleEngine, sara);

            // Re-apply after static constructors
            FormulaClsCommon.CnRuleString = ruleEngine;
            PatchStaticConnectionFields(ruleEngine, sara);

            Console.WriteLine("DB RuleEngine : {0}", Mask(ruleEngine));
            Console.WriteLine("DB Sara       : {0}", Mask(sara));
            Console.WriteLine("CnRuleString  : {0}", Mask(FormulaClsCommon.CnRuleString ?? "(null)"));
        }

        private static string Required(string name)
        {
            string cs = ConfigurationManager.ConnectionStrings[name]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("connectionStrings:" + name + " is missing in App.config / RuleTrace.exe.config");
            if (cs.IndexOf("hService", StringComparison.OrdinalIgnoreCase) >= 0)
                throw new InvalidOperationException("connectionStrings:" + name + " still uses hService — edit RuleTrace.exe.config");
            return cs;
        }

        private static void EnsureConnectionStrings(string ruleEngine, string sara)
        {
            foreach (string name in RuleEngineNames)
                UpsertConnectionString(name, ruleEngine);
            foreach (string name in SaraNames)
                UpsertConnectionString(name, sara);
        }

        private static void UpsertConnectionString(string name, string connectionString)
        {
            var settings = ConfigurationManager.ConnectionStrings[name];
            if (settings == null)
            {
                try
                {
                    var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.ConnectionStrings.ConnectionStrings.Add(
                        new ConnectionStringSettings(name, connectionString, "System.Data.SqlClient"));
                    config.Save(ConfigurationSaveMode.Minimal);
                    ConfigurationManager.RefreshSection("connectionStrings");
                }
                catch
                {
                    // runtime add may fail on some hosts — Upsert is best-effort
                }
                return;
            }

            SetConnectionStringWritable(settings, connectionString);
        }

        private static void SetConnectionStringWritable(ConnectionStringSettings settings, string connectionString)
        {
            var readOnlyField = typeof(ConfigurationElement).GetField("_bReadOnly",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (readOnlyField != null)
                readOnlyField.SetValue(settings, false);
            settings.ConnectionString = connectionString;
        }

        private static void PatchStaticConnectionFields(string ruleEngine, string sara)
        {
            string[] types =
            {
                "SafaClassDesingerNew.ClsCommon", "SafaClassDesingerNew.ClsClass", "SafaClassDesingerNew.ClsConnection",
                "BIZ.SA.ClsCNManagment", "BIZ.SA.ClsCommon", "BIZ.SA.ClsConnection",
                "BIZ.SC.ClsConnection", "BIZ.SC.ClsCommon",
            };

            foreach (string typeName in types)
            {
                string assembly = typeName.StartsWith("BIZ.SC") ? "BIZ.SC" :
                                  typeName.StartsWith("BIZ.SA") ? "BIZ.SA" :
                                  "SafaClassDesingerNew";
                PatchTypeStaticStrings(Type.GetType(typeName + ", " + assembly), ruleEngine, sara);
            }
        }

        private static void PatchTypeStaticStrings(Type type, string ruleEngine, string sara)
        {
            if (type == null) return;

            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (field.FieldType != typeof(string)) continue;

                string name = field.Name;
                string current = field.GetValue(null) as string;
                bool looksLikeConn = !string.IsNullOrEmpty(current)
                    && (current.IndexOf("Server=", StringComparison.OrdinalIgnoreCase) >= 0
                        || current.IndexOf("hService", StringComparison.OrdinalIgnoreCase) >= 0);

                bool ruleField = name.IndexOf("Rule", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("Engein", StringComparison.OrdinalIgnoreCase) >= 0;
                bool saraField = name.IndexOf("Sara", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.Equals("CnString", StringComparison.OrdinalIgnoreCase);

                if (looksLikeConn || ruleField || saraField || name.IndexOf("Cn", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string value = saraField && !ruleField ? sara : ruleEngine;
                    try { field.SetValue(null, value); } catch { /* ignore */ }
                }
            }
        }

        private static void PatchAssemblySettings(Assembly assembly, string ruleEngine, string sara)
        {
            if (assembly == null) return;

            string settingsTypeName = assembly.GetName().Name + ".Properties.Settings";
            Type settingsType = assembly.GetType(settingsTypeName, false);
            if (settingsType == null) return;

            try
            {
                object settings = settingsType.GetProperty("Default", BindingFlags.Public | BindingFlags.Static)
                    ?.GetValue(null, null);
                if (settings == null) return;

                foreach (PropertyInfo prop in settingsType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (prop.PropertyType != typeof(string)) continue;
                    string val = prop.GetValue(settings, null) as string;
                    if (string.IsNullOrEmpty(val)) continue;
                    if (val.IndexOf("Server=", StringComparison.OrdinalIgnoreCase) < 0
                        && val.IndexOf("hService", StringComparison.OrdinalIgnoreCase) < 0)
                        continue;

                    string replacement = prop.Name.IndexOf("Sara", StringComparison.OrdinalIgnoreCase) >= 0 ? sara : ruleEngine;
                    prop.SetValue(settings, replacement, null);
                }
            }
            catch
            {
                // optional per DLL version
            }
        }

        private static void TouchType(string typeName, string assemblyName)
        {
            try
            {
                Type.GetType(typeName + ", " + assemblyName);
            }
            catch
            {
                // optional
            }
        }

        /// <summary>
        /// Sidecar *.dll.config next to copied DLLs often contain hService and confuse operators.
        /// Rename them so only RuleTrace.exe.config is used.
        /// </summary>
        private static void NeutralizeDllConfigs(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
                return;

            foreach (string file in Directory.GetFiles(folder, "*.dll.config"))
            {
                string backup = file + ".hService.bak";
                try
                {
                    if (File.Exists(backup))
                        File.Delete(backup);
                    File.Move(file, backup);
                    Console.WriteLine("Neutralized : {0}", Path.GetFileName(file));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("WARN: could not neutralize {0}: {1}", Path.GetFileName(file), ex.Message);
                }
            }
        }

        private static string Mask(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) return "(empty)";
            int idx = connectionString.IndexOf("Password=", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return connectionString;
            int end = connectionString.IndexOf(';', idx);
            if (end < 0) end = connectionString.Length;
            return connectionString.Substring(0, idx + 9) + "***" + connectionString.Substring(end);
        }
    }
}
