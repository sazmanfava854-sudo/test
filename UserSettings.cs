using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace RuleTrace
{
    /// <summary>
    /// Persisted next to RuleTrace.exe as RuleTrace.user.ini (key=value). App.config supplies defaults.
    /// </summary>
    internal sealed class UserSettings
    {
        public string DllPath { get; set; }
        public string RuleEngine { get; set; }
        public string Sara { get; set; }
        public string CityGuid { get; set; }
        public string DefaultCity { get; set; }
        public string CachePath { get; set; }
        public string LegacyDllPath { get; set; }

        public string LastNidProc { get; set; }
        public string LastFormula { get; set; }
        public string LastWatch { get; set; }
        public string LastLookup { get; set; }

        public static string IniPath
        {
            get
            {
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
                return Path.Combine(dir, "RuleTrace.user.ini");
            }
        }

        public static UserSettings Load()
        {
            var s = new UserSettings
            {
                DllPath = ConfigurationManager.AppSettings["DllPath"] ?? string.Empty,
                RuleEngine = ConnStr("RuleEngine"),
                Sara = ConnStr("Sara"),
                CityGuid = ConfigurationManager.AppSettings["CityGuid"] ?? string.Empty,
                DefaultCity = ConfigurationManager.AppSettings["DefaultCity"] ?? "2",
                CachePath = ConfigurationManager.AppSettings["FormulaCachePath"] ?? string.Empty,
                LegacyDllPath = ConfigurationManager.AppSettings["LegacyCompileDllPath"] ?? @"c:\dll10",
                LastFormula = "Solh",
                LastWatch = "Calc_Chandganeh",
            };

            if (File.Exists(IniPath))
            {
                foreach (string raw in File.ReadAllLines(IniPath))
                {
                    string line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith("#") || line.StartsWith(";")) continue;
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    string key = line.Substring(0, eq).Trim();
                    string value = line.Substring(eq + 1).Trim();
                    s.Set(key, value);
                }
            }

            if (string.IsNullOrWhiteSpace(s.DllPath) || !Directory.Exists(s.DllPath))
                s.DllPath = FormulaEngine.DetectDllPath(s.DllPath) ?? s.DllPath;

            if (string.IsNullOrWhiteSpace(s.CachePath))
                s.CachePath = Path.Combine(string.IsNullOrWhiteSpace(s.DllPath) ? @"C:\" : s.DllPath, "SafaFormulaCache");

            return s;
        }

        public void Save()
        {
            var lines = new List<string>
            {
                "# RuleTrace user settings (auto-saved from UI)",
                "DllPath=" + DllPath,
                "RuleEngine=" + RuleEngine,
                "Sara=" + Sara,
                "CityGuid=" + CityGuid,
                "DefaultCity=" + DefaultCity,
                "CachePath=" + CachePath,
                "LegacyDllPath=" + LegacyDllPath,
                "LastNidProc=" + LastNidProc,
                "LastFormula=" + LastFormula,
                "LastWatch=" + LastWatch,
                "LastLookup=" + LastLookup,
            };
            File.WriteAllLines(IniPath, lines);
        }

        private void Set(string key, string value)
        {
            switch (key)
            {
                case "DllPath": DllPath = value; break;
                case "RuleEngine": RuleEngine = value; break;
                case "Sara": Sara = value; break;
                case "CityGuid": CityGuid = value; break;
                case "DefaultCity": DefaultCity = value; break;
                case "CachePath": CachePath = value; break;
                case "LegacyDllPath": LegacyDllPath = value; break;
                case "LastNidProc": LastNidProc = value; break;
                case "LastFormula": LastFormula = value; break;
                case "LastWatch": LastWatch = value; break;
                case "LastLookup": LastLookup = value; break;
            }
        }

        private static string ConnStr(string name)
        {
            var cs = ConfigurationManager.ConnectionStrings[name];
            return cs == null ? string.Empty : cs.ConnectionString;
        }
    }
}
