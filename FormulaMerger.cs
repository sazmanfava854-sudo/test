using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace RuleTrace
{
    /// <summary>
    /// Workaround for SafaClassDesingerNew 2012.5 merging 20 empty class shells (BC30269):
    /// Member VB code lives in XmlBody/&lt;Body&gt; but the engine compile path often reads EncryptXmlBody
    /// (decrypt fails locally) so ClsFunction.Body is empty at merge time.
    /// </summary>
    internal static class FormulaMerger
    {
        private const BindingFlags AnyStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        private const BindingFlags AnyInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>Prefer XmlBody plaintext; disable EncryptXmlBody path; enable partial-class merge if supported.</summary>
        public static void PatchEngineFlags(Assembly safa, Action<string> log)
        {
            if (safa == null) return;
            int n = 0;
            foreach (string tn in new[] { "SafaClassDesingerNew.ClsCommon", "SafaClassDesingerNew.ClsClass", "SafaClassDesingerNew.ClsFunction", "SafaClassDesingerNew.ClsRunRuleResult" })
            {
                Type t = safa.GetType(tn, false);
                if (t == null) continue;
                foreach (FieldInfo f in t.GetFields(AnyStatic))
                {
                    if (f.FieldType != typeof(bool)) continue;
                    bool? v = PickBool(f.Name);
                    if (!v.HasValue) continue;
                    try { f.SetValue(null, v.Value); n++; log("Engine flag  : " + t.Name + "." + f.Name + " = " + v.Value); } catch { }
                }
            }
            if (n == 0) log("Engine flags : (no known bool flags patched — DLL may differ)");
        }

        private static bool? PickBool(string name)
        {
            string n = name ?? string.Empty;
            if (n.IndexOf("Partial", StringComparison.OrdinalIgnoreCase) >= 0 && n.IndexOf("Not", StringComparison.OrdinalIgnoreCase) < 0) return true;
            if (n.IndexOf("Merge", StringComparison.OrdinalIgnoreCase) >= 0 && n.IndexOf("Member", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (n.Equals("UseXmlBody", StringComparison.OrdinalIgnoreCase) || n.Equals("ReadXmlBody", StringComparison.OrdinalIgnoreCase)) return true;
            if (n.IndexOf("Encrypt", StringComparison.OrdinalIgnoreCase) >= 0 && n.IndexOf("Xml", StringComparison.OrdinalIgnoreCase) >= 0) return false;
            if (n.IndexOf("EncXml", StringComparison.OrdinalIgnoreCase) >= 0) return false;
            return null;
        }

        /// <summary>Creates ClsClass and copies XmlBody &lt;Body&gt; text into each ClsFunction (by NidFunction).</summary>
        public static object CreateClassWithBodies(Assembly safa, int nid, Guid cityGuid, bool recompile, IList<MemberSource> sources, Action<string> log)
        {
            Type tCls = safa.GetType("SafaClassDesingerNew.ClsClass", true);
            ConstructorInfo ctor = tCls.GetConstructors(AnyInstance).FirstOrDefault(c => c.GetParameters().Length == 3)
                                   ?? tCls.GetConstructors(AnyInstance).OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
            if (ctor == null) throw new MissingMethodException("ClsClass constructor not found");

            var ps = ctor.GetParameters();
            var args = new object[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                Type pt = ps[i].ParameterType;
                if (pt == typeof(Guid)) args[i] = cityGuid;
                else if (pt == typeof(bool)) args[i] = recompile;
                else if (pt == typeof(string)) args[i] = cityGuid.ToString("D");
                else args[i] = Convert.ChangeType(nid, pt);
            }

            object cls = ctor.Invoke(args);
            int injected = InjectBodies(cls, sources, log);
            log("Inject       : " + injected + "/" + sources.Count + " ClsFunction.Body set from XmlBody <Body>");
            return cls;
        }

        public static int InjectBodies(object cls, IList<MemberSource> sources, Action<string> log)
        {
            var funcs = GetFunctionList(cls);
            if (funcs.Count == 0) throw new InvalidOperationException("ClsClass has no M_Function / Function list");

            var map = sources.ToDictionary(s => s.NidMember);
            int injected = 0;
            foreach (object fn in funcs)
            {
                int id = ReadInt(fn, "NidFunction");
                if (id == 0) id = ReadInt(fn, "NidMember");
                MemberSource src;
                if (!map.TryGetValue(id, out src) || string.IsNullOrWhiteSpace(src.Code)) continue;

                string code = StripDuplicateClassShell(src.Code);
                if (TrySet(fn, "Body", code) | TrySet(fn, "M_Body", code) | TrySet(fn, "Source", code) | TrySet(fn, "Text", code))
                    injected++;
                TrySet(fn, "EncryptXmlBody", null);
                TrySet(fn, "M_EncryptXmlBody", null);
                TrySet(fn, "_ReCompile", true);
                TrySet(fn, "ReCompile", true);
            }
            return injected;
        }

        private static List<object> GetFunctionList(object cls)
        {
            foreach (string name in new[] { "M_Function", "Function", "Functions", "m_Function", "UpdatedFunctionList" })
            {
                var en = GetMember(cls, name) as IEnumerable;
                if (en == null) continue;
                var list = en.Cast<object>().Where(x => x != null).ToList();
                if (list.Count > 0) return list;
            }
            return new List<object>();
        }

        /// <summary>Build one VB source: full class shell (ToString1) once + stripped member Sub/Function bodies inside.</summary>
        public static string BuildMergedVb(object cls, IList<MemberSource> sources, Action<string> log)
        {
            string shell = GetClassShell(cls);
            if (shell.Length < 500)
                throw new InvalidOperationException("class shell too short — ToString1 / GetStrOutClass empty");

            int shellMOut = CountOccurrences(shell, "M_Out");
            log("Merge shell  : len=" + shell.Length + ", M_Out=" + shellMOut + " (from ToString1 if len>5KB)");

            int cap = shell.Length + (int)Math.Min(sources.Sum(s => (long)s.Code.Length), int.MaxValue - shell.Length - 4096) + 4096;
            var sb = new StringBuilder(cap);
            int endClass = shell.LastIndexOf("End Class", StringComparison.OrdinalIgnoreCase);
            if (endClass < 0) endClass = shell.LastIndexOf("EndClass", StringComparison.OrdinalIgnoreCase);
            if (endClass < 0) throw new InvalidOperationException("End Class not found in shell");

            sb.Append(shell.Substring(0, endClass));
            sb.AppendLine();
            sb.AppendLine("' --- RuleTrace: Member XmlBody methods (class shells stripped) ---");
            foreach (MemberSource src in sources.OrderBy(s => s.NidMember))
            {
                if (string.IsNullOrWhiteSpace(src.Code)) continue;
                string body = StripDuplicateClassShell(src.Code);
                if (body.Length < 20) continue;
                sb.AppendLine("#Region \"NidFunction " + src.NidMember + " " + src.Name + "\"");
                sb.AppendLine(body);
                sb.AppendLine("#End Region");
                sb.AppendLine();
            }
            sb.AppendLine(shell.Substring(endClass));
            string merged = sb.ToString();
            int totalMOut = CountOccurrences(merged, "M_Out");
            log("Merged VB    : " + merged.Length + " chars, M_Out=" + totalMOut + " (expect 1–2)");
            if (totalMOut > 3)
                log("WARN merge   : still " + totalMOut + " M_Out — member bodies may contain nested shells");
            return merged;
        }

        /// <summary>Full VB class header from ClsClass.ToString1 (~18KB); GetStrOutClass is only ~1KB and unusable.</summary>
        private static string GetClassShell(object cls)
        {
            string best = string.Empty;
            foreach (string name in new[] { "ToString1", "ToString", "ClassSource", "FullText" })
            {
                string s = ReadStringMember(cls, name);
                if (s != null && s.Length > best.Length) best = s;
            }
            string getStr = InvokeString(cls, "GetStrOutClass");
            if (!string.IsNullOrEmpty(getStr) && getStr.Length > best.Length) best = getStr;
            return best ?? string.Empty;
        }

        private static string ReadStringMember(object o, string name)
        {
            object v = GetMember(o, name);
            if (v is string) return (string)v;
            return InvokeString(o, name);
        }

        /// <summary>Keep only Sub/Function/#Region blocks; drop duplicate M_Out/Out/Namespace/Class from member XML bodies.</summary>
        public static string StripDuplicateClassShell(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return string.Empty;
            string norm = code.Replace("\r\n", "\n");

            var parts = new List<string>();
            var methodRx = new Regex(
                @"(?ms)^\s*((?:Public|Private|Protected|Friend|Partial)?\s*(?:Overrides\s+)?(?:Sub|Function)\s+\w+.+?^End\s+(?:Sub|Function)\s*)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            foreach (Match m in methodRx.Matches(norm))
            {
                string block = m.Groups[1].Value.Trim();
                if (block.IndexOf("M_Out", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                parts.Add(block.Replace("\n", "\r\n"));
            }

            if (parts.Count > 0)
                return string.Join("\r\n\r\n", parts);

            var regionRx = new Regex(@"(?ms)^\s*(#Region\b.+?^#End Region)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            foreach (Match m in regionRx.Matches(norm))
            {
                string block = m.Groups[1].Value;
                if (block.IndexOf("M_Out", StringComparison.OrdinalIgnoreCase) >= 0
                    || block.IndexOf("Property Out As", StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;
                parts.Add(block.Trim().Replace("\n", "\r\n"));
            }
            if (parts.Count > 0)
                return string.Join("\r\n\r\n", parts);

            norm = Regex.Replace(norm, @"(?ms)^\s*Private\s+M_Out\b.*?^End\s+Property\s*", "", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            norm = Regex.Replace(norm, @"(?ms)^\s*Public\s+Property\s+Out\b.*?^End\s+Property\s*", "", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            var kept = new List<string>();
            foreach (string line in norm.Split('\n'))
            {
                string t = line.Trim();
                if (t.Length == 0) continue;
                if (IsShellLine(t)) continue;
                kept.Add(line);
            }
            return string.Join("\r\n", kept).Trim();
        }

        private static bool IsShellLine(string t)
        {
            if (t.StartsWith("Imports ", StringComparison.OrdinalIgnoreCase)) return true;
            if (t.StartsWith("Namespace ", StringComparison.OrdinalIgnoreCase)) return true;
            if (t.Equals("End Namespace", StringComparison.OrdinalIgnoreCase)) return true;
            if (t.IndexOf(" Class ", StringComparison.OrdinalIgnoreCase) >= 0
                || t.StartsWith("Public Class", StringComparison.OrdinalIgnoreCase)
                || t.StartsWith("Partial Class", StringComparison.OrdinalIgnoreCase)) return true;
            if (t.Equals("End Class", StringComparison.OrdinalIgnoreCase)) return true;
            if (t.IndexOf("M_Out", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (t.IndexOf("Property Out As", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (t.StartsWith("<Serializable", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        public static void SaveMergedFile(string mergedVb, string cacheFolder, Action<string> log)
        {
            if (string.IsNullOrWhiteSpace(mergedVb) || string.IsNullOrWhiteSpace(cacheFolder)) return;
            try
            {
                Directory.CreateDirectory(cacheFolder);
                string path = Path.Combine(cacheFolder, "RuleTrace_merged.vb");
                File.WriteAllText(path, mergedVb, Encoding.UTF8);
                log("Merged file  : " + path);
            }
            catch (Exception ex) { log("WARN         : cannot write merged.vb: " + ex.Message); }
        }

        public static bool IsMOutDuplicateError(object compilerErrors)
        {
            if (compilerErrors == null) return false;
            int n = 0;
            foreach (object e in (IEnumerable)compilerErrors)
            {
                string s = e == null ? "" : e.ToString();
                if (s.IndexOf("BC30269", StringComparison.OrdinalIgnoreCase) >= 0
                    || s.IndexOf("M_Out", StringComparison.OrdinalIgnoreCase) >= 0)
                    n++;
            }
            return n >= 2;
        }

        public static void LogFunctionBodies(object cls, Action<string> log, int max = 5)
        {
            int i = 0;
            foreach (object fn in GetFunctionList(cls))
            {
                if (i++ >= max) break;
                string name = Convert.ToString(GetMember(fn, "Name") ?? GetMember(fn, "M_Name") ?? "?");
                int id = ReadInt(fn, "NidFunction");
                int len = LenStr(GetMember(fn, "Body") ?? GetMember(fn, "M_Body"));
                log("  ClsFunction " + id + " " + name + " BodyLen=" + len);
            }
        }

        private static int LenStr(object o)
        {
            string s = o as string;
            return s == null ? 0 : s.Length;
        }

        private static int ReadInt(object o, string name)
        {
            object v = GetMember(o, name);
            if (v == null) return 0;
            try { return Convert.ToInt32(v); } catch { return 0; }
        }

        private static object GetMember(object o, string name)
        {
            if (o == null) return null;
            Type t = o.GetType();
            PropertyInfo p = t.GetProperty(name, AnyInstance);
            if (p != null) try { return p.GetValue(o, null); } catch { }
            FieldInfo f = t.GetField(name, AnyInstance);
            return f == null ? null : f.GetValue(o);
        }

        private static bool TrySet(object o, string name, object value)
        {
            Type t = o.GetType();
            PropertyInfo p = t.GetProperty(name, AnyInstance);
            if (p != null && p.CanWrite) { try { p.SetValue(o, value, null); return true; } catch { } }
            FieldInfo f = t.GetField(name, AnyInstance);
            if (f != null) { try { f.SetValue(o, value); return true; } catch { } }
            return false;
        }

        private static string InvokeString(object o, string method)
        {
            if (o == null) return null;
            MethodInfo m = o.GetType().GetMethod(method, AnyInstance);
            if (m == null || m.GetParameters().Length != 0 || m.ReturnType != typeof(string)) return null;
            try { return m.Invoke(o, null) as string; } catch { return null; }
        }

        private static int CountOccurrences(string text, string token)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(token)) return 0;
            int count = 0, i = 0;
            while ((i = text.IndexOf(token, i, StringComparison.OrdinalIgnoreCase)) >= 0) { count++; i += token.Length; }
            return count;
        }

        private static string FirstLine(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            int i = s.IndexOf('\n');
            return i < 0 ? s : s.Substring(0, i).Trim();
        }
    }
}
