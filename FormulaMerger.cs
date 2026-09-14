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

        /// <summary>Creates ClsClass without injecting member bodies (use for class shell / ToString1).</summary>
        public static object CreateClass(Assembly safa, int nid, Guid cityGuid, bool recompile)
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
            return ctor.Invoke(args);
        }

        /// <summary>Creates ClsClass and copies XmlBody &lt;Body&gt; text into each ClsFunction (by NidFunction).</summary>
        public static object CreateClassWithBodies(Assembly safa, int nid, Guid cityGuid, bool recompile, IList<MemberSource> sources, Action<string> log)
        {
            object cls = CreateClass(safa, nid, cityGuid, recompile);
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

        /// <summary>Build merged VB: prefer injected ToString1 (full engine output); fallback manual shell+XmlBody merge.</summary>
        public static string BuildMergedVb(object shellCls, object injectedCls, IList<MemberSource> sources, Action<string> log)
        {
            string injected = GetInjectedClassSource(injectedCls, log);
            if (IsValidInjectedSource(injected))
                return BuildFromInjectedSource(injected, log);
            log("Merge path   : manual shell+XmlBody (injected source len=" + (injected == null ? 0 : injected.Length) + ")");
            return BuildMergedVbManual(shellCls, sources, log);
        }

        private static string GetInjectedClassSource(object cls, Action<string> log)
        {
            string s = ReadStringMember(cls, "ToString1");
            if (IsValidInjectedSource(s)) { log("Merge source : ToString1 len=" + s.Length); return s; }
            s = ReadStringMember(cls, "ToString");
            if (IsValidInjectedSource(s)) { log("Merge source : ToString() len=" + s.Length); return s; }
            s = InvokeString(cls, "GetStrOutClass");
            if (IsValidInjectedSource(s)) { log("Merge source : GetStrOutClass len=" + s.Length); return s; }
            return ReadStringMember(cls, "ToString1");
        }

        /// <summary>Engine ToString1 after XmlBody inject — keep structure, only fix Out/M_Out duplicates.</summary>
        private static string BuildFromInjectedSource(string injected, Action<string> log)
        {
            log("Merge path   : injected full class, M_Out=" + CountOccurrences(injected, "M_Out"));
            var stripped = StripAllOutDeclarations(NormalizeNewlines(injected));
            string merged = InsertCanonicalOutBlock(stripped.CleanedText, stripped.FirstMOut, stripped.FirstPropOut);
            int mOutDecls = CountMOutDeclarations(merged);
            int outProps = CountPropertyOutDeclarations(merged);
            log("Merged VB    : " + merged.Length + " chars, M_Out decls=" + mOutDecls + ", Property Out=" + outProps + " (injected path)");
            return PrependStandardImports(merged);
        }

        private static bool IsValidInjectedSource(string s)
        {
            return !string.IsNullOrEmpty(s) && s.Length >= 50000 && s.Length <= 6000000
                && s.IndexOf("End Class", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>Fallback: ToString1 shell once + stripped member Sub/Function bodies.</summary>
        private static string BuildMergedVbManual(object cls, IList<MemberSource> sources, Action<string> log)
        {
            string shellSource;
            string shell = GetClassShell(cls, out shellSource);
            if (shell.Length < 500)
                throw new InvalidOperationException("class shell too short — ToString1 / GetStrOutClass empty");

            int rawShellMOut = CountOccurrences(shell, "M_Out");
            log("Merge shell  : " + shellSource + " len=" + shell.Length + ", M_Out=" + rawShellMOut);

            var shellOut = StripAllOutDeclarations(shell);
            shell = shellOut.CleanedText;
            string canonicalMOut = shellOut.FirstMOut;
            string canonicalPropOut = shellOut.FirstPropOut;

            var methodMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string shellNorm = NormalizeNewlines(shell);
            foreach (string block in ExtractMethodBlocks(shellNorm))
            {
                string key = MethodKey(block);
                if (string.IsNullOrEmpty(key)) continue;
                methodMap[key] = block.Replace("\n", "\r\n");
            }
            int shellMethodCount = methodMap.Count;

            shell = StripMethodsFromShell(shell);
            shell = DedupeFieldLinesByName(shell);
            log("Merge shell  : after stripping stubs, M_Out decls=" + CountMOutDeclarations(shell) + ", shell methods=" + shellMethodCount);

            var shellNames = CollectDeclarationNames(shell);
            EnrichDeclarationNamesFromText(shell, shellNames);
            var fieldByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            int methodCount = 0;
            int fieldSkipped = 0;
            foreach (MemberSource src in sources.OrderBy(s => s.NidMember))
            {
                if (string.IsNullOrWhiteSpace(src.Code)) continue;
                string norm = NormalizeNewlines(src.Code);
                norm = UnwrapEmbeddedClass(norm);
                var memOut = StripAllOutDeclarations(norm);
                norm = memOut.CleanedText;
                if (string.IsNullOrEmpty(canonicalMOut) && !string.IsNullOrEmpty(memOut.FirstMOut))
                    canonicalMOut = memOut.FirstMOut;
                if (string.IsNullOrEmpty(canonicalPropOut) && !string.IsNullOrEmpty(memOut.FirstPropOut))
                    canonicalPropOut = memOut.FirstPropOut;

                foreach (string decl in ExtractFieldDeclarations(norm))
                {
                    var declNames = ExtractFieldNamesFromLine(decl).ToList();
                    if (declNames.Count == 0) continue;
                    if (declNames.Any(n => shellNames.Contains(n) || fieldByName.ContainsKey(n)))
                    {
                        fieldSkipped++;
                        continue;
                    }
                    foreach (string n in declNames)
                        fieldByName[n] = decl;
                }
                foreach (string block in ExtractMethodBlocks(norm))
                {
                    string key = MethodKey(block);
                    if (string.IsNullOrEmpty(key)) continue;
                    methodMap[key] = block.Replace("\n", "\r\n");
                    methodCount++;
                }
            }
            log("Merge methods: " + methodMap.Count + " unique Sub/Function (" + shellMethodCount + " shell + " + methodCount + " member block(s))");
            log("Merge fields  : " + fieldByName.Count + " unique (" + fieldSkipped + " skipped — already in shell or duplicate)");

            int cap = shell.Length + (int)Math.Min(methodMap.Values.Sum(b => (long)b.Length), int.MaxValue - shell.Length - 8192) + 8192;
            var sb = new StringBuilder(cap);
            int endClass = shell.LastIndexOf("End Class", StringComparison.OrdinalIgnoreCase);
            if (endClass < 0) endClass = shell.LastIndexOf("EndClass", StringComparison.OrdinalIgnoreCase);
            if (endClass < 0) throw new InvalidOperationException("End Class not found in shell");

            sb.Append(shell.Substring(0, endClass));
            sb.AppendLine();
            var hostStubs = BuildRuntimeHostFieldStubs(shellNames, fieldByName);
            if (hostStubs.Count > 0)
                log("Merge host   : " + hostStubs.Count + " runtime stub field(s) (Info8, …)");

            if (fieldByName.Count > 0 || hostStubs.Count > 0)
            {
                sb.AppendLine("' --- RuleTrace: shared fields from Member XmlBody ---");
                foreach (string decl in fieldByName.Values.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                    sb.AppendLine(decl);
                foreach (string decl in hostStubs)
                    sb.AppendLine(decl);
                sb.AppendLine();
            }
            sb.AppendLine("' --- RuleTrace: Member XmlBody methods (class shells stripped) ---");
            foreach (var kv in methodMap.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            {
                sb.AppendLine(kv.Value);
                sb.AppendLine();
            }

            string merged = InsertCanonicalOutBlock(
                sb.ToString() + shell.Substring(endClass),
                canonicalMOut, canonicalPropOut);
            merged = DedupeFieldLinesByName(merged);
            int mOutDecls = CountMOutDeclarations(merged);
            int outProps = CountPropertyOutDeclarations(merged);
            log("Merged VB    : " + merged.Length + " chars, M_Out decls=" + mOutDecls + ", Property Out=" + outProps + " (expect 1 each)");
            if (mOutDecls > 1 || outProps > 1)
                log("WARN merge   : duplicate Out/M_Out — send RuleTrace_merged.vb");
            return PrependStandardImports(merged);
        }

        /// <summary>Ensure Guid, Serializable, Exception, List, DataView resolve under vbc.</summary>
        private static string PrependStandardImports(string merged)
        {
            string norm = NormalizeNewlines(merged);
            var imports = new[]
            {
                "System",
                "System.Collections.Generic",
                "System.Data",
                "System.Runtime.Serialization",
                "System.Xml",
                "System.Linq",
                "Microsoft.VisualBasic",
                "BIZ.SC",
                "BIZ.SA",
            };
            var sb = new StringBuilder();
            foreach (string ns in imports)
            {
                if (HasImportLine(norm, ns)) continue;
                sb.AppendLine("Imports " + ns);
            }
            string result = merged;
            if (sb.Length > 0)
            {
                sb.AppendLine();
                sb.Append(merged);
                result = sb.ToString();
            }
            return FixSerializableAttribute(result);
        }

        /// <summary>Match whole import line — avoid skipping System when System.Data exists.</summary>
        private static bool HasImportLine(string norm, string ns)
        {
            foreach (string line in norm.Split('\n'))
            {
                string t = line.Trim();
                if (!t.StartsWith("Imports ", StringComparison.OrdinalIgnoreCase)) continue;
                string rest = t.Substring(8).Trim();
                if (rest.Equals(ns, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private static string FixSerializableAttribute(string merged)
        {
            merged = Regex.Replace(merged, @"<\s*Serializable\s*>", "<System.SerializableAttribute()>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            merged = Regex.Replace(merged, @"<\s*Serializable\s*\(\s*\)\s*>", "<System.SerializableAttribute()>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            merged = Regex.Replace(merged, @"<\s*SerializableAttribute\s*>", "<System.SerializableAttribute()>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            return merged;
        }

        private static readonly string[] RuntimeHostFieldNames = { "Info8", "Info", "MyInfo", "M_Info", "_OutList" };

        private static List<string> BuildRuntimeHostFieldStubs(HashSet<string> shellNames, Dictionary<string, string> fieldByName)
        {
            var stubs = new List<string>();
            foreach (string name in RuntimeHostFieldNames)
            {
                if (shellNames.Contains(name) || fieldByName.ContainsKey(name)) continue;
                if (name.StartsWith("_", StringComparison.Ordinal))
                    stubs.Add("Private " + name + " As Object");
                else
                    stubs.Add("Public " + name + " As Object");
            }
            return stubs;
        }

        /// <summary>Keep first Dim/Private/Public field per identifier (VB case-insensitive).</summary>
        private static string DedupeFieldLinesByName(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var kept = new List<string>();
            foreach (string line in NormalizeNewlines(text).Split('\n'))
            {
                string t = line.Trim();
                if (t.Length == 0 || !IsFieldDeclarationLine(t))
                {
                    kept.Add(line);
                    continue;
                }
                var names = ExtractFieldNamesFromLine(t).ToList();
                if (names.Count == 0)
                {
                    kept.Add(line);
                    continue;
                }
                if (names.Any(n => seen.Contains(n))) continue;
                foreach (string n in names) seen.Add(n);
                kept.Add(line);
            }
            return string.Join("\n", kept).Replace("\n", "\r\n");
        }

        private static string InsertCanonicalOutBlock(string text, string canonicalMOut, string canonicalPropOut)
        {
            string norm = NormalizeNewlines(text);
            int endClass = norm.LastIndexOf("End Class", StringComparison.OrdinalIgnoreCase);
            if (endClass < 0) endClass = norm.LastIndexOf("EndClass", StringComparison.OrdinalIgnoreCase);
            if (endClass < 0) throw new InvalidOperationException("End Class not found in merged source");

            var sb = new StringBuilder(norm.Length + 512);
            sb.Append(norm.Substring(0, endClass));
            sb.AppendLine();
            sb.AppendLine("' --- RuleTrace: canonical Out / M_Out (single copy) ---");
            if (!string.IsNullOrEmpty(canonicalMOut))
                sb.AppendLine(canonicalMOut);
            else
                sb.AppendLine("Private M_Out As ClsOut");
            sb.AppendLine();
            if (!string.IsNullOrEmpty(canonicalPropOut))
                sb.AppendLine(canonicalPropOut);
            else
            {
                sb.AppendLine("Public Property Out() As ClsOut");
                sb.AppendLine("    Get");
                sb.AppendLine("        If M_Out Is Nothing Then");
                sb.AppendLine("            M_Out = New ClsOut()");
                sb.AppendLine("        End If");
                sb.AppendLine("        Return M_Out");
                sb.AppendLine("    End Get");
                sb.AppendLine("    Set(ByVal Value As ClsOut)");
                sb.AppendLine("        M_Out = Value");
                sb.AppendLine("    End Set");
                sb.AppendLine("End Property");
            }
            sb.AppendLine();
            sb.Append(norm.Substring(endClass));
            return sb.ToString().Replace("\n", "\r\n");
        }

        private static readonly Regex FieldDeclFirstNameRx = new Regex(
            @"^(?:Public|Private|Protected|Friend|Dim|Const)(?:\s+(?:ReadOnly|Shared))*\s+([\w_]+)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static IEnumerable<string> ExtractFieldNamesFromLine(string line)
        {
            var names = ParseFieldNames(line).ToList();
            if (names.Count > 0)
            {
                foreach (string n in names) yield return n;
                yield break;
            }
            if (string.IsNullOrWhiteSpace(line)) yield break;
            Match m = FieldDeclFirstNameRx.Match(line.Trim());
            if (m.Success) yield return m.Groups[1].Value;
        }

        private static void EnrichDeclarationNamesFromText(string text, HashSet<string> names)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            foreach (string line in NormalizeNewlines(text).Split('\n'))
            {
                foreach (string n in ExtractFieldNamesFromLine(line.Trim()))
                    names.Add(n);
            }
        }

        /// <summary>Class shell from ToString1 (~18KB). Never use ToString() — after inject it embeds all bodies (~1MB, 100× M_Out).</summary>
        private static string GetClassShell(object cls, out string source)
        {
            source = "none";
            string s = ReadStringMember(cls, "ToString1");
            if (IsReasonableShell(s)) { source = "ToString1"; return s; }

            s = InvokeString(cls, "GetStrOutClass");
            if (!string.IsNullOrEmpty(s) && s.Length >= 200 && s.Length <= 250000) { source = "GetStrOutClass"; return s; }

            foreach (string name in new[] { "ClassSource", "FullText" })
            {
                s = ReadStringMember(cls, name);
                if (IsReasonableShell(s)) { source = name; return s; }
            }
            return string.Empty;
        }

        private static bool IsReasonableShell(string s)
        {
            return !string.IsNullOrEmpty(s) && s.Length >= 500 && s.Length <= 250000
                && s.IndexOf("End Class", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string ReadStringMember(object o, string name)
        {
            object v = GetMember(o, name);
            if (v is string) return (string)v;
            return InvokeString(o, name);
        }

        /// <summary>Keep only Sub/Function blocks; drop duplicate class shells (M_Out/Out/Namespace/Class) from member XML bodies.</summary>
        public static string StripDuplicateClassShell(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return string.Empty;
            string norm = NormalizeNewlines(code);
            norm = UnwrapEmbeddedClass(norm);
            norm = StripAllOutDeclarations(norm).CleanedText;

            var methods = ExtractMethodBlocks(norm);
            if (methods.Count > 0)
                return string.Join("\r\n\r\n", methods);

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

        /// <summary>Remove empty/stub Sub/Function from ToString1 shell so member bodies do not duplicate signatures.</summary>
        private static string StripMethodsFromShell(string shell)
        {
            if (string.IsNullOrWhiteSpace(shell)) return shell;
            string norm = NormalizeNewlines(shell);
            foreach (string block in ExtractMethodBlocks(norm))
                norm = norm.Replace(NormalizeNewlines(block).Replace("\r\n", "\n"), string.Empty);
            return norm.Replace("\n", "\r\n").Trim();
        }

        /// <summary>Class-level Dim/Private/Public fields from member bodies (TempMasahat, TmpDt, …).</summary>
        private static IEnumerable<string> ExtractFieldDeclarations(string norm)
        {
            if (string.IsNullOrWhiteSpace(norm)) yield break;
            foreach (string block in ExtractMethodBlocks(norm))
                norm = norm.Replace(NormalizeNewlines(block).Replace("\r\n", "\n"), string.Empty);

            foreach (string line in norm.Split('\n'))
            {
                string t = line.Trim();
                if (t.Length == 0 || t.StartsWith("'")) continue;
                if (IsShellLine(t)) continue;
                if (!IsFieldDeclarationLine(t)) continue;
                yield return t;
            }
        }

        private static bool IsFieldDeclarationLine(string t)
        {
            if (!Regex.IsMatch(t, @"^(?:Public|Private|Protected|Friend|Dim|Const)\s+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                return false;
            if (Regex.IsMatch(t, @"\b(?:Sub|Function|Property|Class|Structure|Enum|Interface|Event|Delegate)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                return false;
            if (Regex.IsMatch(t, @"\bPrivate\s+M_Out\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) return false;
            if (t.IndexOf("Property Out", StringComparison.OrdinalIgnoreCase) >= 0) return false;
            return true;
        }

        private static readonly Regex FieldDeclHeadRx = new Regex(
            @"^(?:Public|Private|Protected|Friend|Dim|Const)(?:\s+(?:ReadOnly|Shared))*\s+",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>First identifier on a field line (VB names are case-insensitive).</summary>
        private static bool TryParseFieldName(string line, out string name)
        {
            name = null;
            foreach (string n in ParseFieldNames(line))
            {
                name = n;
                return true;
            }
            return false;
        }

        /// <summary>All identifiers declared on one line (Dim a, b As Integer → a, b).</summary>
        private static IEnumerable<string> ParseFieldNames(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) yield break;
            string t = line.Trim();
            Match head = FieldDeclHeadRx.Match(t);
            if (!head.Success) yield break;

            string rest = t.Substring(head.Length);
            int asIdx = IndexOfWord(rest, "As");
            int eqIdx = IndexOfWord(rest, "="); // Const x = 1
            int end = rest.Length;
            if (asIdx >= 0) end = asIdx;
            else if (eqIdx >= 0) end = eqIdx;

            string namesPart = rest.Substring(0, end).Trim();
            if (namesPart.Length == 0) yield break;

            foreach (string part in namesPart.Split(','))
            {
                string id = part.Trim();
                if (id.Length == 0) continue;
                int paren = id.IndexOf('(');
                if (paren > 0) id = id.Substring(0, paren).Trim();
                if (id.Length > 0 && (char.IsLetter(id[0]) || id[0] == '_')) yield return id;
            }
        }

        private static int IndexOfWord(string text, string word)
        {
            var rx = new Regex(@"\b" + word + @"\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            Match m = rx.Match(text);
            return m.Success ? m.Index : -1;
        }

        private static HashSet<string> CollectDeclarationNames(string vb)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(vb)) return names;
            foreach (string line in NormalizeNewlines(vb).Split('\n'))
            {
                string t = line.Trim();
                foreach (string name in ExtractFieldNamesFromLine(t))
                    names.Add(name);
                Match prop = Regex.Match(t, @"^(?:(?:Public|Private|Protected|Friend)\s+)+Property\s+(\w+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                if (prop.Success && !prop.Groups[1].Value.Equals("Out", StringComparison.OrdinalIgnoreCase))
                    names.Add(prop.Groups[1].Value);
            }
            return names;
        }

        private static string NormalizeNewlines(string code)
        {
            return (code ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n");
        }

        private static string UnwrapEmbeddedClass(string norm)
        {
            int classIdx = IndexOfLineMatch(norm, @"^\s*(?:Public\s+|Partial\s+)*Class\s+\w+");
            int endIdx = norm.LastIndexOf("\nEnd Class", StringComparison.OrdinalIgnoreCase);
            if (classIdx < 0 || endIdx < classIdx) return norm;
            int bodyStart = norm.IndexOf('\n', classIdx);
            if (bodyStart < 0) return norm;
            bodyStart++;
            string inner = norm.Substring(bodyStart, endIdx - bodyStart);
            return inner.Trim();
        }

        private struct OutExtractResult
        {
            public string CleanedText;
            public string FirstMOut;
            public string FirstPropOut;
        }

        /// <summary>
        /// Strip ALL Private M_Out lines and ALL Property Out blocks (single-line or multiline Get/Set/End Property).
        /// Captures the first occurrence of each so exactly ONE canonical copy can be inserted at class end.
        /// </summary>
        private static OutExtractResult StripAllOutDeclarations(string text)
        {
            var res = new OutExtractResult();
            if (string.IsNullOrWhiteSpace(text))
            {
                res.CleanedText = text ?? string.Empty;
                return res;
            }

            string[] lines = NormalizeNewlines(text).Split('\n');
            var kept = new List<string>(lines.Length);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string t = line.TrimStart();

                // 1. Private M_Out declaration
                if (Regex.IsMatch(t, @"^Private\s+M_Out\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    if (string.IsNullOrEmpty(res.FirstMOut))
                        res.FirstMOut = line.Trim();
                    continue;
                }

                // 2. Property Out block (Public/Private Property Out ...)
                if (Regex.IsMatch(t, @"^(?:(?:Public|Private|Protected|Friend)\s+)*Property\s+Out\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    int endPropIdx = -1;
                    int maxLookahead = Math.Min(lines.Length, i + 80);
                    for (int j = i + 1; j < maxLookahead; j++)
                    {
                        string tj = lines[j].TrimStart();
                        if (Regex.IsMatch(tj, @"^End\s+Property\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                        {
                            endPropIdx = j;
                            break;
                        }
                        if (Regex.IsMatch(tj, @"^(?:(?:Public|Private|Protected|Friend)\s+)*(?:Sub|Function|Class|Structure|Enum)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
                            || tj.StartsWith("End Class", StringComparison.OrdinalIgnoreCase)
                            || tj.StartsWith("End Sub", StringComparison.OrdinalIgnoreCase)
                            || tj.StartsWith("End Function", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                    }

                    if (endPropIdx >= i)
                    {
                        // Multiline property block (includes Get/Set and End Property)
                        if (string.IsNullOrEmpty(res.FirstPropOut))
                        {
                            var sbProp = new StringBuilder();
                            for (int k = i; k <= endPropIdx; k++)
                                sbProp.AppendLine(lines[k]);
                            res.FirstPropOut = sbProp.ToString().TrimEnd();
                        }
                        i = endPropIdx; // skip all lines through End Property
                        continue;
                    }
                    else
                    {
                        // Single-line auto property
                        if (string.IsNullOrEmpty(res.FirstPropOut))
                            res.FirstPropOut = line.Trim();
                        continue;
                    }
                }

                kept.Add(line);
            }

            res.CleanedText = string.Join("\n", kept);
            return res;
        }

        private static int CountMOutDeclarations(string text)
        {
            int n = 0;
            foreach (string line in NormalizeNewlines(text).Split('\n'))
                if (Regex.IsMatch(line.TrimStart(), @"^Private\s+M_Out\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) n++;
            return n;
        }

        private static int CountPropertyOutDeclarations(string text)
        {
            int n = 0;
            foreach (string line in NormalizeNewlines(text).Split('\n'))
                if (Regex.IsMatch(line.TrimStart(), @"^(?:(?:Public|Private|Protected|Friend)\s+)*Property\s+Out\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) n++;
            return n;
        }

        private static List<string> ExtractMethodBlocks(string norm)
        {
            var blocks = new List<string>();
            if (string.IsNullOrWhiteSpace(norm)) return blocks;
            string[] lines = norm.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (!IsMethodStartLine(lines[i])) continue;
                int depth = 0;
                var sb = new StringBuilder();
                for (int j = i; j < lines.Length; j++)
                {
                    sb.Append(lines[j]).Append('\n');
                    if (IsMethodStartLine(lines[j]))
                        depth++;
                    else if (IsMethodEndLine(lines[j]))
                    {
                        depth--;
                        if (depth <= 0)
                        {
                            string block = sb.ToString().Trim();
                            if (block.Length > 10 && !IsMOutPropertyShell(block))
                                blocks.Add(block.Replace("\n", "\r\n"));
                            i = j;
                            break;
                        }
                    }
                }
            }
            return blocks;
        }

        private static bool IsMethodStartLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            string t = line.TrimStart();
            return Regex.IsMatch(t, @"^(?:(?:Public|Private|Protected|Friend|Partial)\s+)*(?:Overrides\s+)?(?:Sub|Function)\s+\w+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private static bool IsMethodEndLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            return Regex.IsMatch(line.Trim(), @"^End\s+(?:Sub|Function)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private static bool IsMOutPropertyShell(string block)
        {
            string t = block.TrimStart();
            return t.StartsWith("Private M_Out", StringComparison.OrdinalIgnoreCase)
                || t.StartsWith("Public Property Out", StringComparison.OrdinalIgnoreCase)
                || t.StartsWith("Private Property Out", StringComparison.OrdinalIgnoreCase);
        }

        private static string MethodKey(string block)
        {
            if (string.IsNullOrWhiteSpace(block)) return string.Empty;
            var m = Regex.Match(block, @"(?:Sub|Function)\s+(\w+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            return m.Success ? m.Groups[1].Value : string.Empty;
        }

        private static int IndexOfLineMatch(string text, string pattern)
        {
            var rx = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            foreach (Match m in rx.Matches(text))
            {
                if (m.Index == 0 || text[m.Index - 1] == '\n') return m.Index;
            }
            return -1;
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
