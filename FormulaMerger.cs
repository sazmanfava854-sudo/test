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

        /// <summary>Build one VB source: class shell once + all member bodies inside (fixes 20× M_Out).</summary>
        public static string BuildMergedVb(object cls, IList<MemberSource> sources, Action<string> log)
        {
            string shell = InvokeString(cls, "GetStrOutClass") ?? InvokeString(cls, "ToString1") ?? string.Empty;
            if (shell.Length < 100)
                throw new InvalidOperationException("GetStrOutClass returned empty shell");

            int mOut = CountOccurrences(shell, "M_Out");
            log("Merge shell  : GetStrOutClass len=" + shell.Length + ", M_Out=" + mOut);

            var sb = new StringBuilder(shell.Length + sources.Sum(s => s.Code.Length) + 4096);
            int endClass = shell.LastIndexOf("End Class", StringComparison.OrdinalIgnoreCase);
            if (endClass < 0) endClass = shell.LastIndexOf("EndClass", StringComparison.OrdinalIgnoreCase);
            if (endClass < 0) throw new InvalidOperationException("End Class not found in shell");

            sb.Append(shell.Substring(0, endClass));
            sb.AppendLine();
            sb.AppendLine("' --- RuleTrace merged Member bodies from XmlBody <Body> ---");
            foreach (MemberSource src in sources.OrderBy(s => s.NidMember))
            {
                if (string.IsNullOrWhiteSpace(src.Code)) continue;
                sb.AppendLine("#Region \"NidFunction " + src.NidMember + " " + src.Name + "\"");
                sb.AppendLine(StripDuplicateClassShell(src.Code));
                sb.AppendLine("#End Region");
                sb.AppendLine();
            }
            sb.AppendLine(shell.Substring(endClass));
            string merged = sb.ToString();
            log("Merged VB    : " + merged.Length + " chars, M_Out=" + CountOccurrences(merged, "M_Out") + " (expect 1)");
            return merged;
        }

        /// <summary>Remove class shell (M_Out/Out/Namespace/Class) from a member body that incorrectly carries it.</summary>
        public static string StripDuplicateClassShell(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return string.Empty;
            if (code.IndexOf("M_Out", StringComparison.OrdinalIgnoreCase) < 0
                && code.IndexOf("Property Out As", StringComparison.OrdinalIgnoreCase) < 0)
                return code.Trim();

            var lines = code.Replace("\r\n", "\n").Split('\n');
            int start = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string t = lines[i].Trim();
                if (t.Length == 0 || t.StartsWith("'")) continue;
                if (t.StartsWith("#Region", StringComparison.OrdinalIgnoreCase)
                    || t.StartsWith("Sub ", StringComparison.OrdinalIgnoreCase)
                    || t.StartsWith("Function ", StringComparison.OrdinalIgnoreCase)
                    || t.StartsWith("Public Sub", StringComparison.OrdinalIgnoreCase)
                    || t.StartsWith("Private Sub", StringComparison.OrdinalIgnoreCase)
                    || t.StartsWith("Protected Sub", StringComparison.OrdinalIgnoreCase)
                    || t.StartsWith("Public Function", StringComparison.OrdinalIgnoreCase)
                    || t.StartsWith("Private Function", StringComparison.OrdinalIgnoreCase)
                    || t.StartsWith("Dim ", StringComparison.OrdinalIgnoreCase)
                    || t.StartsWith("Const ", StringComparison.OrdinalIgnoreCase))
                {
                    start = i;
                    break;
                }
            }
            return string.Join("\r\n", lines.Skip(start)).Trim();
        }

        /// <summary>Try engine compile methods after body injection; returns ClsRunRuleResult or null.</summary>
        public static object TryCompile(Assembly safa, object cls, Guid cityGuid, string cacheFolder, string mergedVb, Action<string> log)
        {
            Type tCommon = safa.GetType("SafaClassDesingerNew.ClsCommon", false);
            Type tResult = safa.GetType("SafaClassDesingerNew.ClsRunRuleResult", false);
            Type tCls = cls.GetType();

            if (!string.IsNullOrWhiteSpace(mergedVb) && !string.IsNullOrWhiteSpace(cacheFolder))
            {
                try
                {
                    Directory.CreateDirectory(cacheFolder);
                    string path = Path.Combine(cacheFolder, "RuleTrace_merged.vb");
                    File.WriteAllText(path, mergedVb, Encoding.UTF8);
                    log("Merged file  : " + path);
                }
                catch (Exception ex) { log("WARN         : cannot write merged.vb: " + ex.Message); }
            }

            // instance: ClsClass.Compile / RunRule / CreateResult
            foreach (MethodInfo m in tCls.GetMethods(AnyInstance))
            {
                if (!m.Name.Contains("Compile") && !m.Name.Contains("RunRule") && !m.Name.Contains("Assembly")) continue;
                object r = TryInvoke(log, "ClsClass." + m.Name, cls, m, cls, cityGuid, mergedVb, cacheFolder);
                if (IsRunRuleResult(r, tResult)) return r;
            }

            if (tCommon != null)
            {
                foreach (MethodInfo m in tCommon.GetMethods(AnyStatic))
                {
                    if (tResult != null && m.ReturnType != tResult && !m.ReturnType.Name.Contains("RunRule")) continue;
                    if (!m.Name.Contains("Compile") && !m.Name.Contains("RunRule") && !m.Name.Contains("Assembly") && !m.Name.Contains("Create")) continue;
                    object r = TryInvoke(log, "ClsCommon." + m.Name, null, m, cls, cityGuid, mergedVb, cacheFolder);
                    if (IsRunRuleResult(r, tResult)) return r;
                }
            }

            return null;
        }

        private static object TryInvoke(Action<string> log, string label, object target, MethodInfo m, object cls, Guid cityGuid, string mergedVb, string cacheFolder)
        {
            var ps = m.GetParameters();
            if (ps.Length > 4) return null;
            try
            {
                var args = new object[ps.Length];
                for (int i = 0; i < ps.Length; i++)
                {
                    Type pt = ps[i].ParameterType;
                    string pn = ps[i].Name ?? "";
                    if (pt.IsInstanceOfType(cls)) args[i] = cls;
                    else if (pt == typeof(Guid)) args[i] = cityGuid;
                    else if (pt == typeof(bool)) args[i] = true;
                    else if (pt == typeof(string) && pn.IndexOf("path", StringComparison.OrdinalIgnoreCase) >= 0) args[i] = cacheFolder;
                    else if (pt == typeof(string) && (pn.IndexOf("source", StringComparison.OrdinalIgnoreCase) >= 0 || pn.IndexOf("code", StringComparison.OrdinalIgnoreCase) >= 0 || pn.IndexOf("vb", StringComparison.OrdinalIgnoreCase) >= 0))
                        args[i] = mergedVb;
                    else if (pt == typeof(int) || pt == typeof(double)) args[i] = Convert.ToInt32(GetMember(cls, "NidClass") ?? 0);
                    else return null;
                }
                object r = m.Invoke(target, args);
                log("Compile try  : " + label + "(" + ps.Length + " args) => " + (r == null ? "null" : r.GetType().Name));
                return r;
            }
            catch (Exception ex)
            {
                log("Compile skip : " + label + " — " + FirstLine((ex.InnerException ?? ex).Message));
                return null;
            }
        }

        private static bool IsRunRuleResult(object r, Type tResult)
        {
            if (r == null) return false;
            if (tResult != null && tResult.IsInstanceOfType(r)) return true;
            return r.GetType().Name.IndexOf("RunRule", StringComparison.OrdinalIgnoreCase) >= 0;
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
