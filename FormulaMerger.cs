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

                string fnName = Convert.ToString(GetMember(fn, "Name") ?? GetMember(fn, "M_Name") ?? src.Name ?? "");
                string code = BodyForInject(src.Code, fnName);
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

        /// <summary>Fix structurally broken engine ToString1 (code outside methods, duplicate Out/M_Out) then compile that source.</summary>
        public static string SanitizeInjectedToString1(object injectedCls, IList<MemberSource> sources, Action<string> log)
        {
            string injected = ReadStringMember(injectedCls, "ToString1");
            if (string.IsNullOrEmpty(injected) || injected.Length < 20000)
            {
                if (log != null) log("Sanitize     : ToString1 too short (" + (injected == null ? 0 : injected.Length) + ")");
                return null;
            }
            if (log != null) log("Sanitize     : ToString1 raw len=" + injected.Length);
            return BuildFromInjectedSource(injected, injectedCls, sources, log ?? (m => { }));
        }

        /// <summary>Build merged VB: shell ToString1 (~18KB) + XmlBody member methods when DB sources exist; injected ToString1 is structurally broken for vbc.</summary>
        public static string BuildMergedVb(object shellCls, object injectedCls, IList<MemberSource> sources, Action<string> log)
        {
            if (HasSubstantialXmlBodySources(sources))
            {
                log("Merge path   : shell ToString1 + XmlBody member methods (skip injected ToString1)");
                return ApplyParameterStubsIfNeeded(BuildMergedVbManual(shellCls, sources, log), injectedCls, sources, log);
            }

            string injected = GetInjectedClassSource(injectedCls, log);
            if (IsValidInjectedSource(injected))
                return BuildFromInjectedSource(injected, injectedCls, sources, log);
            log("Merge path   : manual shell+XmlBody (injected source len=" + (injected == null ? 0 : injected.Length) + ")");
            return ApplyParameterStubsIfNeeded(BuildMergedVbManual(shellCls, sources, log), injectedCls, sources, log);
        }

        private static bool HasSubstantialXmlBodySources(IList<MemberSource> sources)
        {
            return sources != null && sources.Count > 0
                && sources.Any(s => !string.IsNullOrWhiteSpace(s.Code) && s.Code.Length >= 50);
        }

        private static string ApplyParameterStubsIfNeeded(string merged, object cls, IList<MemberSource> sources, Action<string> log)
        {
            var missingParams = DiscoverUndeclaredParameters(merged, cls, sources, log);
            if (missingParams.Count == 0) return merged;
            log("Merge params : " + missingParams.Count + " parameter properties added (" + string.Join(", ", missingParams.Take(8)) + (missingParams.Count > 8 ? "..." : "") + ")");
            return InjectPropertyStubs(merged, missingParams);
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

        /// <summary>Engine ToString1 after XmlBody inject — keep structure, fix Out/M_Out duplicates, auto-declare parameters.</summary>
        private static string BuildFromInjectedSource(string injected, object cls, IList<MemberSource> sources, Action<string> log)
        {
            log("Merge path   : injected full class, M_Out=" + CountOccurrences(injected, "M_Out"));
            var stripped = StripAllOutDeclarations(NormalizeNewlines(injected));
            string body = MergePreClassMethodsIntoClass(stripped.CleanedText, log);
            string merged = InsertCanonicalOutBlock(body, stripped.FirstMOut, stripped.FirstPropOut);

            var missingParams = DiscoverUndeclaredParameters(merged, cls, sources, log);
            if (missingParams.Count > 0)
            {
                log("Merge params : " + missingParams.Count + " parameter properties added (" + string.Join(", ", missingParams.Take(8)) + (missingParams.Count > 8 ? "..." : "") + ")");
                merged = InjectPropertyStubs(merged, missingParams);
            }

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

        private static readonly Regex ClassDeclRx = new Regex(
            @"^(?:(?:Public|Private|Friend|Protected|Partial|NotInheritable|MustInherit|Shadows)\s+)*Class\s+\w+",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// ToString1 puts ~20 member Sub/Function bodies before the Class line (file scope). Merge complete method
        /// blocks only — never dump raw lines (causes BC30289 nested-in-method errors).
        /// </summary>
        private static string MergePreClassMethodsIntoClass(string vb, Action<string> log)
        {
            if (string.IsNullOrWhiteSpace(vb)) return vb;
            string[] lines = NormalizeNewlines(vb).Split('\n');

            int classIdx = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (ClassDeclRx.IsMatch(lines[i].TrimStart())) { classIdx = i; break; }
            }
            if (classIdx < 0)
            {
                log("Merge struct : no Class declaration found — leaving source as-is");
                return vb;
            }

            int endClassIdx = -1;
            for (int i = lines.Length - 1; i > classIdx; i--)
            {
                if (lines[i].TrimStart().StartsWith("End Class", StringComparison.OrdinalIgnoreCase)) { endClassIdx = i; break; }
            }
            if (endClassIdx < 0)
            {
                log("Merge struct : no End Class found — leaving source as-is");
                return vb;
            }

            var header = new List<string>();
            var orphanBefore = new List<string>();
            for (int i = 0; i < classIdx; i++)
            {
                if (IsFileScopeHeaderLine(lines[i])) header.Add(lines[i]);
                else orphanBefore.Add(lines[i]);
            }

            var orphanAfter = new List<string>();
            var trailer = new List<string>();
            for (int i = endClassIdx + 1; i < lines.Length; i++)
            {
                if (IsFileScopeTrailerLine(lines[i])) trailer.Add(lines[i]);
                else orphanAfter.Add(lines[i]);
            }

            string innerText = string.Join("\n", lines.Skip(classIdx + 1).Take(endClassIdx - classIdx - 1));
            var orphanMethods = ExtractMethodBlocks(string.Join("\n", orphanBefore));
            var innerMethods = ExtractMethodBlocks(innerText);
            var afterMethods = ExtractMethodBlocks(string.Join("\n", orphanAfter));

            if (orphanBefore.Count == 0 && orphanAfter.Count == 0)
            {
                log("Merge struct : class scope already correct (Class at line " + (classIdx + 1) + ")");
                return vb;
            }
            if (orphanMethods.Count == 0 && afterMethods.Count == 0)
                log("WARN merge   : " + orphanBefore.Count + " pre-Class line(s) but no Sub/Function blocks extracted — check attributes");

            var methodMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string block in innerMethods)
            {
                string key = MethodKey(block);
                if (!string.IsNullOrEmpty(key)) methodMap[key] = block;
            }
            int added = 0, replaced = 0;
            foreach (string block in orphanMethods.Concat(afterMethods))
            {
                string key = MethodKey(block);
                if (string.IsNullOrEmpty(key)) continue;
                if (!methodMap.ContainsKey(key)) { methodMap[key] = block; added++; }
                else if (block.Length > methodMap[key].Length) { methodMap[key] = block; replaced++; }
            }

            string classShell = StripMethodsFromShell(innerText);

            log("Merge struct : merged " + orphanMethods.Count + " pre-Class method block(s) into class ("
                + added + " new, " + replaced + " replaced with longer body)");

            var sb = new StringBuilder(vb.Length + 8192);
            foreach (string l in header) sb.Append(l).Append('\n');
            sb.Append(lines[classIdx]).Append('\n');
            sb.Append(classShell);
            if (!classShell.EndsWith("\n", StringComparison.Ordinal)) sb.Append('\n');
            if (methodMap.Count > 0)
            {
                sb.AppendLine("' --- RuleTrace: member Sub/Function from pre-Class region ---");
                foreach (var kv in methodMap.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
                {
                    sb.AppendLine(kv.Value);
                    sb.AppendLine();
                }
            }
            sb.Append(lines[endClassIdx]).Append('\n');
            foreach (string l in trailer) sb.Append(l).Append('\n');

            return sb.ToString().Replace("\n", "\r\n");
        }

        /// <summary>Imports/Option/Namespace/attribute/comment lines legally precede a Class declaration.</summary>
        private static bool IsFileScopeHeaderLine(string line)
        {
            string t = (line ?? string.Empty).Trim();
            if (t.Length == 0 || t.StartsWith("'")) return true;
            if (t.StartsWith("Imports ", StringComparison.OrdinalIgnoreCase)) return true;
            if (t.StartsWith("Option ", StringComparison.OrdinalIgnoreCase)) return true;
            if (t.StartsWith("Namespace ", StringComparison.OrdinalIgnoreCase)) return true;
            if (t.StartsWith("#", StringComparison.Ordinal)) return true;
            if (t.StartsWith("<", StringComparison.Ordinal)) return true;
            return false;
        }

        private static bool IsFileScopeTrailerLine(string line)
        {
            string t = (line ?? string.Empty).Trim();
            if (t.Length == 0 || t.StartsWith("'")) return true;
            if (t.Equals("End Namespace", StringComparison.OrdinalIgnoreCase)) return true;
            if (t.StartsWith("#", StringComparison.Ordinal)) return true;
            return false;
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

        /// <summary>Ensure Guid, Serializable, Exception, List, DataView, MapArr, SegmentArr resolve under vbc.</summary>
        private static string PrependStandardImports(string merged)
        {
            string norm = NormalizeNewlines(merged);
            var imports = new[]
            {
                "System",
                "System.Collections.Generic",
                "System.ComponentModel",
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
            result = FixSerializableAttribute(result);
            result = FixFormulaAttributes(result);
            return EnsureCommonTypeAliases(result);
        }

        private static string FixFormulaAttributes(string merged)
        {
            if (string.IsNullOrWhiteSpace(merged)) return merged;
            merged = Regex.Replace(merged, @"<\s*DisplayName\s*\(", "<System.ComponentModel.DisplayName(", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            return merged;
        }

        private static string QualifyClsOutReferences(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return line;
            return Regex.Replace(line, @"\bclsOut\b", "BIZ.SC.ClsOut", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private static string EnsureCommonTypeAliases(string merged)
        {
            if (string.IsNullOrWhiteSpace(merged)) return merged;
            // MapArr and SegmentArr are internal structure aliases used in some Sara formulas
            var aliases = new[]
            {
                "Imports MapArr = System.Object",
                "Imports SegmentArr = System.Object",
                "Imports clsOut = BIZ.SC.ClsOut",
                "Imports ClsOut = BIZ.SC.ClsOut",
            };
            string norm = NormalizeNewlines(merged);
            var sb = new StringBuilder();
            foreach (var a in aliases)
            {
                string aliasName = a.Split('=')[0].Replace("Imports", "").Trim();
                if (Regex.IsMatch(norm, @"^\s*Imports\s+" + Regex.Escape(aliasName) + @"\s*=", RegexOptions.Multiline | RegexOptions.IgnoreCase))
                    continue;
                sb.AppendLine(a);
            }
            if (sb.Length > 0)
            {
                sb.AppendLine();
                sb.Append(merged);
                return sb.ToString();
            }
            return merged;
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
                sb.AppendLine(QualifyClsOutReferences(canonicalMOut));
            else
                sb.AppendLine("Private M_Out As BIZ.SC.ClsOut");
            sb.AppendLine();
            if (!string.IsNullOrEmpty(canonicalPropOut))
                sb.AppendLine(QualifyClsOutReferences(canonicalPropOut));
            else
            {
                sb.AppendLine("Public Property Out() As BIZ.SC.ClsOut");
                sb.AppendLine("    Get");
                sb.AppendLine("        If M_Out Is Nothing Then");
                sb.AppendLine("            M_Out = New BIZ.SC.ClsOut()");
                sb.AppendLine("        End If");
                sb.AppendLine("        Return M_Out");
                sb.AppendLine("    End Get");
                sb.AppendLine("    Set(ByVal Value As BIZ.SC.ClsOut)");
                sb.AppendLine("        M_Out = Value");
                sb.AppendLine("    End Set");
                sb.AppendLine("End Property");
            }
            sb.AppendLine();
            sb.Append(norm.Substring(endClass));
            return sb.ToString().Replace("\n", "\r\n");
        }

        public static string InjectPropertyStubs(string vb, IEnumerable<string> names)
        {
            return InjectPropertyStubs(vb, names, false);
        }

        /// <param name="force">When true (vbc auto-fix), ignore false-positive declaration detection.</param>
        public static string InjectPropertyStubs(string vb, IEnumerable<string> names, bool force)
        {
            if (string.IsNullOrWhiteSpace(vb)) return vb;
            var declared = force ? new HashSet<string>(StringComparer.OrdinalIgnoreCase) : CollectStrictDeclarationNames(vb);

            var list = (names ?? Enumerable.Empty<string>())
                .Where(n => !string.IsNullOrWhiteSpace(n) && Regex.IsMatch(n.Trim(), @"^[A-Za-z_]\w*$"))
                .Select(n => n.Trim())
                .Where(n => !declared.Contains(n)
                            && !n.Equals("M_Out", StringComparison.OrdinalIgnoreCase)
                            && !n.Equals("Out", StringComparison.OrdinalIgnoreCase)
                            && (!force || !RuntimeHostFieldNames.Any(h => h.Equals(n, StringComparison.OrdinalIgnoreCase))))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (list.Count == 0) return vb;

            string norm = NormalizeNewlines(vb);
            int endClass = norm.LastIndexOf("End Class", StringComparison.OrdinalIgnoreCase);
            if (endClass < 0) endClass = norm.LastIndexOf("EndClass", StringComparison.OrdinalIgnoreCase);
            if (endClass < 0) return vb;

            var sb = new StringBuilder(norm.Length + list.Count * 45);
            sb.Append(norm.Substring(0, endClass));
            sb.AppendLine();
            sb.AppendLine("' --- RuleTrace: auto-generated parameter properties ---");
            foreach (string name in list.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                sb.AppendLine("Public Property " + name + " As Object");
            }
            sb.AppendLine();
            sb.Append(norm.Substring(endClass));
            return sb.ToString().Replace("\n", "\r\n");
        }

        public static List<string> DiscoverUndeclaredParameters(string code, object cls, IList<MemberSource> sources, Action<string> log)
        {
            var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (cls != null)
            {
                foreach (string n in DiscoverParameterNamesFromClass(cls))
                    candidates.Add(n);
            }

            if (sources != null)
            {
                foreach (var src in sources)
                {
                    if (string.IsNullOrWhiteSpace(src.Code)) continue;
                    foreach (string n in DiscoverParameterNamesFromCode(src.Code))
                        candidates.Add(n);
                }
            }

            foreach (string n in DiscoverParameterNamesFromCode(code))
                candidates.Add(n);

            var declared = CollectStrictDeclarationNames(code);

            var missing = new List<string>();
            foreach (string name in candidates.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(name)) continue;
                if (!Regex.IsMatch(name, @"^[A-Za-z_]\w*$")) continue;
                if (declared.Contains(name)) continue;
                if (name.Equals("M_Out", StringComparison.OrdinalIgnoreCase) || name.Equals("Out", StringComparison.OrdinalIgnoreCase)) continue;
                if (RuntimeHostFieldNames.Any(h => h.Equals(name, StringComparison.OrdinalIgnoreCase))) continue;
                missing.Add(name);
            }
            return missing;
        }

        private static IEnumerable<string> DiscoverParameterNamesFromClass(object cls)
        {
            if (cls == null) yield break;
            foreach (string name in new[] { "M_Parameter", "Parameters", "ParameterList", "UpdatedParameterList", "M_Property", "PropertyList", "UpdatedPropertyList" })
            {
                var en = GetMember(cls, name) as IEnumerable;
                if (en == null) continue;
                foreach (object item in en)
                {
                    if (item == null) continue;
                    string n = Convert.ToString(GetMember(item, "Name") ?? GetMember(item, "M_Name") ?? GetMember(item, "ParameterName") ?? GetMember(item, "PropertyName") ?? item);
                    if (!string.IsNullOrWhiteSpace(n) && Regex.IsMatch(n.Trim(), @"^[A-Za-z_]\w*$"))
                        yield return n.Trim();
                }
            }
        }

        private static IEnumerable<string> DiscoverParameterNamesFromCode(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) yield break;
            // Match variables with parameter prefix: PP_..., PM_..., P_... (e.g. PP_SatheEshghal, P_Vahed, P_M_Tejari)
            var rxPrefixed = new Regex(@"\b(P[PM]_[A-Za-z0-9_]+|P_[A-Za-z0-9_]+)\b", RegexOptions.IgnoreCase);
            foreach (Match m in rxPrefixed.Matches(text))
            {
                string id = m.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(id)) yield return id;
            }

            // Match assignments like: PP_SatheEshghal = ... or P_Vahed = ... or Me.P_Vahed = ...
            var rxAssign = new Regex(@"^\s*(?:Me\.)?([A-Za-z_]\w*)\s*=(?!=)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            foreach (Match m in rxAssign.Matches(text))
            {
                string id = m.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(id) && (id.StartsWith("P", StringComparison.OrdinalIgnoreCase) || id.StartsWith("_", StringComparison.OrdinalIgnoreCase)))
                    yield return id;
            }

            // Match Property declarations in member bodies (e.g. Public Property Foo As ...)
            var rxProp = new Regex(@"(?:Public|Private|Protected|Friend)?\s*Property\s+([A-Za-z_]\w*)", RegexOptions.IgnoreCase);
            foreach (Match m in rxProp.Matches(text))
            {
                string id = m.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(id) && !id.Equals("Out", StringComparison.OrdinalIgnoreCase))
                    yield return id;
            }

            var rxMe = new Regex(@"\bMe\.([A-Za-z_]\w+)\b", RegexOptions.IgnoreCase);
            foreach (Match m in rxMe.Matches(text))
            {
                string id = m.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(id) && !id.Equals("Out", StringComparison.OrdinalIgnoreCase))
                    yield return id;
            }
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
                string t = line.Trim();
                if (!IsFieldDeclarationLine(t) && !IsPropertyDeclarationLine(t)) continue;
                foreach (string n in ExtractFieldNamesFromLine(t))
                    names.Add(n);
                Match prop = PropertyDeclRx.Match(t);
                if (prop.Success)
                {
                    string pName = prop.Groups[1].Value.Trim('[', ']');
                    if (!pName.Equals("Out", StringComparison.OrdinalIgnoreCase))
                        names.Add(pName);
                }
            }
        }

        private static readonly Regex PropertyDeclRx = new Regex(
            @"^(?:(?:Public|Private|Protected|Friend)\s+)+Property\s+(\[?\w+\]?)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static bool IsPropertyDeclarationLine(string t)
        {
            return !string.IsNullOrWhiteSpace(t) && PropertyDeclRx.IsMatch(t.Trim());
        }

        /// <summary>Field/property declarations only — never Dim inside Sub bodies.</summary>
        private static HashSet<string> CollectStrictDeclarationNames(string vb)
        {
            var names = CollectDeclarationNames(vb);
            var strict = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            EnrichDeclarationNamesFromText(vb, strict);
            foreach (string n in strict) names.Add(n);
            return names;
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

        /// <summary>
        /// ToString1 places Body at class scope. Each ClsFunction gets ONE full Sub/Function
        /// (not the whole class dump, not inner statements). Whole-class inject → BC30088;
        /// inner-only inject → BC30689.
        /// </summary>
        public static string BodyForInject(string code, string functionName)
        {
            string stripped = StripDuplicateClassShell(code);
            string one = PickMethodBlock(stripped, functionName);
            return string.IsNullOrWhiteSpace(one) ? stripped : one;
        }

        public static string PickMethodBlock(string code, string functionName)
        {
            if (string.IsNullOrWhiteSpace(code)) return string.Empty;
            var methods = ExtractMethodBlocks(NormalizeNewlines(code));
            if (methods.Count == 0) return code.Trim();
            if (!string.IsNullOrWhiteSpace(functionName))
            {
                foreach (string block in methods)
                    if (string.Equals(MethodKey(block), functionName, StringComparison.OrdinalIgnoreCase))
                        return block;
            }
            return methods[0];
        }

        public static string PeelMethodWrapper(string code, string functionName)
        {
            if (string.IsNullOrWhiteSpace(code)) return string.Empty;
            string norm = NormalizeNewlines(code);
            var methods = ExtractMethodBlocks(norm);
            if (methods.Count == 0)
                return LooksLikeMethodDecl(norm) ? PeelOneMethod(norm) : code.Trim();

            string pick = methods[0];
            if (!string.IsNullOrWhiteSpace(functionName))
            {
                foreach (string block in methods)
                {
                    if (string.Equals(MethodKey(block), functionName, StringComparison.OrdinalIgnoreCase))
                    {
                        pick = block;
                        break;
                    }
                }
            }
            return PeelOneMethod(pick);
        }

        private static string PeelOneMethod(string block)
        {
            if (string.IsNullOrWhiteSpace(block)) return string.Empty;
            string[] lines = NormalizeNewlines(block).Split('\n');
            int start = -1, end = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (start < 0 && LooksLikeMethodDecl(lines[i]))
                {
                    start = i;
                    continue;
                }
                if (start >= 0 && IsMethodEndLine(lines[i]))
                {
                    end = i;
                    break;
                }
            }
            if (start < 0 || end <= start)
                return block.Trim();
            var inner = new List<string>();
            for (int i = start + 1; i < end; i++)
                inner.Add(lines[i]);
            return string.Join("\r\n", inner).Trim();
        }

        private static bool LooksLikeMethodDecl(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            return Regex.IsMatch(
                line,
                @"^\s*(?:<[^>]+>\s*)*(?:(?:Public|Private|Protected|Friend|Partial|Shared|Overrides|Overloads|MustOverride|NotOverridable|Shadows)\s+)*(?:Sub|Function)\s+\w+",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
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
                Match prop = Regex.Match(t, @"^(?:(?:Public|Private|Protected|Friend)\s+)+Property\s+(\[?\w+\]?)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                if (prop.Success)
                {
                    string pName = prop.Groups[1].Value.Trim('[', ']');
                    if (!pName.Equals("Out", StringComparison.OrdinalIgnoreCase))
                        names.Add(pName);
                }
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
                if (!LineDeclaresMethod(lines[i])) continue;
                int start = i;
                while (start > 0 && IsMethodAttributeOrBlankLine(lines[start - 1]))
                    start--;
                int depth = 0;
                var sb = new StringBuilder();
                for (int j = start; j < lines.Length; j++)
                {
                    sb.Append(lines[j]).Append('\n');
                    if (LineDeclaresMethod(lines[j]))
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

        /// <summary>True when line declares Sub/Function (may follow &lt;DisplayName&gt; on same or prior line).</summary>
        private static bool LineDeclaresMethod(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            string t = line.TrimStart();
            if (Regex.IsMatch(t, @"^End\s+(?:Sub|Function)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                return false;
            return Regex.IsMatch(t, @"(?:Sub|Function)\s+\w+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private static bool IsMethodStartLine(string line) => LineDeclaresMethod(line);

        private static bool IsMethodAttributeOrBlankLine(string line)
        {
            string t = (line ?? string.Empty).Trim();
            return t.Length == 0 || t.StartsWith("<", StringComparison.Ordinal);
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

        /// <summary>One VB file per dbo.Member row (partial class) — matches Sara: Run first, then members in order; do not glue all XmlBody into one file.</summary>
        internal sealed class PartialCompileSet
        {
            public string ClassName = "Solh";
            public string ShellPath;
            public readonly List<string> FilePaths = new List<string>();
            public int MemberFileCount;
            public int MethodCount;
        }

        /// <summary>
        /// Shell ToString1 = partial class #1; each Member = separate partial file with ONE primary Sub/Function.
        /// Unlike BuildMergedVbManual, methods from different members are never concatenated into one class body.
        /// </summary>
        public static PartialCompileSet BuildPartialMemberFiles(
            object shellCls, object injectedCls, IList<MemberSource> sources, string cacheFolder, Action<string> log,
            IEnumerable<string> filterKeywords = null)
        {
            var result = new PartialCompileSet();
            if (sources == null || sources.Count == 0) return result;

            string shellSource;
            string shell = GetClassShell(shellCls, out shellSource);
            if (shell.Length < 500)
                throw new InvalidOperationException("class shell too short — ToString1 / GetStrOutClass empty");

            string className = ExtractClassName(shell) ?? "Solh";
            result.ClassName = className;

            var shellOut = StripAllOutDeclarations(shell);
            string shellNorm = NormalizeNewlines(shellOut.CleanedText);
            var shellMethods = ExtractMethodBlocks(shellNorm);
            var shellMethodKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string block in shellMethods)
            {
                string key = MethodKey(block);
                if (!string.IsNullOrEmpty(key)) shellMethodKeys.Add(key);
            }

            var shellNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var fieldByName = CollectSharedFieldsFromSources(sources, shellNames);
            CollectFieldDeclarationsFromText(shellNorm, shellNames, fieldByName);
            var hostStubs = BuildRuntimeHostFieldStubs(shellNames, fieldByName);
            if (fieldByName.Count > 0 || hostStubs.Count > 0)
                log("Partial fields: " + fieldByName.Count + " from XmlBody + " + hostStubs.Count + " host stub(s) in shell");

            var members = sources.OrderBy(s => s.NidMember).ToList();
            if (filterKeywords != null)
            {
                var kw = filterKeywords.Where(k => !string.IsNullOrWhiteSpace(k)).ToList();
                if (kw.Count > 0)
                {
                    int before = members.Count;
                    members = members.Where(s => MemberMatchesKeywords(s, kw)).ToList();
                    log("Member filter: " + members.Count + "/" + before + " member(s) — keywords: " + string.Join(", ", kw.Take(6)) + (kw.Count > 6 ? "..." : ""));
                }
            }

            string partialDir = Path.Combine(cacheFolder, "partial");
            Directory.CreateDirectory(partialDir);

            var assignedMethodKeys = new HashSet<string>(shellMethodKeys, StringComparer.OrdinalIgnoreCase);
            var memberFiles = CollectMemberPartialBodies(members, assignedMethodKeys, log);
            int methodCount = assignedMethodKeys.Count - shellMethodKeys.Count;
            log("Partial methods: " + methodCount + " unique Sub/Function in " + memberFiles.Count + " member file(s) (shell has " + shellMethodKeys.Count + ")");

            string combinedForParams = string.Join("\n", shellMethods)
                + "\n" + string.Join("\n", memberFiles.Select(kv => kv.Value));
            var missingParams = DiscoverUndeclaredParameters(combinedForParams, injectedCls ?? shellCls, sources, log);
            foreach (string n in DiscoverFormulaIdentifiers(combinedForParams))
            {
                if (!missingParams.Any(x => x.Equals(n, StringComparison.OrdinalIgnoreCase)))
                    missingParams.Add(n);
            }
            if (missingParams.Count > 0)
                log("Partial params: " + missingParams.Count + " property stub(s) in shell (" + string.Join(", ", missingParams.Take(8)) + (missingParams.Count > 8 ? "..." : "") + ")");

            shell = BuildCleanPartialShell(className, fieldByName, hostStubs, missingParams, shellMethods);
            log("Partial shell: clean rebuild from " + shellSource + ", methods=" + shellMethodKeys.Count + ", len=" + shell.Length);

            string shellPath = Path.Combine(partialDir, className + "_00_shell.vb");
            File.WriteAllText(shellPath, PrependStandardImports(shell), Encoding.UTF8);
            result.FilePaths.Add(shellPath);
            result.ShellPath = shellPath;
            log("Partial shell: -> " + shellPath);

            foreach (var kv in memberFiles)
            {
                MemberSource src = kv.Key;
                string body = kv.Value;
                string safeName = SanitizeFileName(src.Name);
                if (string.IsNullOrEmpty(safeName)) safeName = "m" + src.NidMember;
                string path = Path.Combine(partialDir, className + "_member_" + src.NidMember + "_" + safeName + ".vb");

                var sb = new StringBuilder();
                sb.AppendLine("' RuleTrace partial — NidMember=" + src.NidMember + " " + src.Name + " (" + src.Meta + ")");
                sb.AppendLine("Partial Public Class " + className);
                sb.AppendLine(body);
                sb.AppendLine("End Class");
                File.WriteAllText(path, PrependStandardImports(sb.ToString()), Encoding.UTF8);
                result.FilePaths.Add(path);
                result.MemberFileCount++;
            }

            result.MethodCount = methodCount;
            log("Partial files: 1 shell + " + result.MemberFileCount + " member file(s), " + methodCount + " method(s) — NOT glued into RuleTrace_merged.vb");
            return result;
        }

        private static Dictionary<string, string> CollectSharedFieldsFromSources(IList<MemberSource> sources, HashSet<string> shellNames)
        {
            var fieldByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (sources == null) return fieldByName;
            foreach (MemberSource src in sources.OrderBy(s => s.NidMember))
            {
                if (string.IsNullOrWhiteSpace(src.Code)) continue;
                string norm = NormalizeNewlines(src.Code);
                norm = UnwrapEmbeddedClass(norm);
                norm = StripAllOutDeclarations(norm).CleanedText;
                CollectFieldDeclarationsFromText(norm, shellNames, fieldByName);
            }
            return fieldByName;
        }

        private static void CollectFieldDeclarationsFromText(string norm, HashSet<string> shellNames, Dictionary<string, string> fieldByName)
        {
            if (string.IsNullOrWhiteSpace(norm)) return;
            foreach (string decl in ExtractFieldDeclarations(norm))
            {
                if (!IsSafeFieldDecl(decl)) continue;
                var names = ExtractFieldNamesFromLine(decl).ToList();
                if (names.Count == 0) continue;
                if (names.Any(n => shellNames.Contains(n) || fieldByName.ContainsKey(n))) continue;
                foreach (string n in names)
                {
                    fieldByName[n] = decl;
                    shellNames.Add(n);
                }
            }
        }

        private static bool IsSafeFieldDecl(string line)
        {
            string t = (line ?? string.Empty).Trim();
            if (t.Length == 0) return false;
            if (!IsFieldDeclarationLine(t)) return false;
            if (t.IndexOf("End Class", StringComparison.OrdinalIgnoreCase) >= 0) return false;
            if (Regex.IsMatch(t, @"\bClass\b", RegexOptions.IgnoreCase)) return false;
            return true;
        }

        /// <summary>One Partial Class block — avoids stray End Class in ToString1 that leaves methods at file scope.</summary>
        private static string BuildCleanPartialShell(
            string className,
            Dictionary<string, string> fieldByName,
            List<string> hostStubs,
            IList<string> propertyStubs,
            List<string> methodBlocks)
        {
            var sb = new StringBuilder(36000);
            sb.AppendLine("Partial Public Class " + className);
            sb.AppendLine();

            if (fieldByName.Count > 0)
            {
                sb.AppendLine("' --- shared fields ---");
                foreach (string decl in fieldByName.Values.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                {
                    string safe = SanitizeFieldLine(decl);
                    if (!string.IsNullOrWhiteSpace(safe)) sb.AppendLine(safe);
                }
                sb.AppendLine();
            }
            if (hostStubs.Count > 0)
            {
                foreach (string stub in hostStubs)
                    sb.AppendLine(stub);
                sb.AppendLine();
            }

            sb.AppendLine("' --- Out (Object stub for vbc) ---");
            sb.AppendLine("Private M_Out As Object");
            sb.AppendLine("Public Property Out() As Object");
            sb.AppendLine("    Get");
            sb.AppendLine("        If M_Out Is Nothing Then M_Out = New Object()");
            sb.AppendLine("        Return M_Out");
            sb.AppendLine("    End Get");
            sb.AppendLine("    Set(ByVal Value As Object)");
            sb.AppendLine("        M_Out = Value");
            sb.AppendLine("    End Set");
            sb.AppendLine("End Property");
            sb.AppendLine();

            if (propertyStubs != null && propertyStubs.Count > 0)
            {
                sb.AppendLine("' --- parameter properties ---");
                foreach (string name in propertyStubs.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    if (name.Equals("Out", StringComparison.OrdinalIgnoreCase) || name.Equals("M_Out", StringComparison.OrdinalIgnoreCase)) continue;
                    sb.AppendLine("Public Property " + name + " As Object");
                }
                sb.AppendLine();
            }

            if (methodBlocks != null && methodBlocks.Count > 0)
            {
                sb.AppendLine("' --- shell methods ---");
                foreach (string block in methodBlocks)
                {
                    sb.AppendLine(block.Trim());
                    sb.AppendLine();
                }
            }

            sb.AppendLine("End Class");
            return SanitizeWholeVb(sb.ToString());
        }

        private static string SanitizeWholeVb(string vb)
        {
            if (string.IsNullOrWhiteSpace(vb)) return vb;
            return Regex.Replace(vb, @"\b(?:BIZ\.SC\.)?(?:clsOut|ClsOut)\b", "Object", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        public static string ApplyShellAutoFix(string shell, IList<string> missing, Action<string> log)
        {
            if (string.IsNullOrWhiteSpace(shell) || missing == null || missing.Count == 0) return shell;
            var props = missing.Where(LooksLikeParameterProperty).ToList();
            var methods = missing.Where(n => !LooksLikeParameterProperty(n)).ToList();
            string updated = shell;
            if (props.Count > 0)
            {
                updated = InjectPropertyStubs(updated, props, true);
                log("VBC auto-fix : +" + props.Count + " property stub(s)");
            }
            if (methods.Count > 0)
            {
                updated = InjectMethodStubs(updated, methods);
                log("VBC auto-fix : +" + methods.Count + " method stub(s): " + string.Join(", ", methods.Take(6)) + (methods.Count > 6 ? "..." : ""));
            }
            return updated;
        }

        private static List<string> DiscoverFormulaIdentifiers(string code)
        {
            var list = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(code)) return list.ToList();
            var rx = new Regex(
                @"\b(PP_[A-Za-z0-9_]+|PM_[A-Za-z0-9_]+|P_[A-Za-z0-9_]+|M_[A-Za-z0-9_]+|IsM[A-Za-z0-9_]+|TmpDto[A-Za-z0-9_]+|MaxFloor_[A-Za-z0-9_]+|AdminSafa|logfilefj|Logfilefj|solhnameh)\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            foreach (Match m in rx.Matches(code))
            {
                string id = m.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(id)) list.Add(id);
            }
            return list.ToList();
        }

        private static string AppendMethodsBeforeEndClass(string shell, List<string> methodBlocks)
        {
            if (methodBlocks == null || methodBlocks.Count == 0) return shell;
            string norm = NormalizeNewlines(shell);
            int endClass = norm.LastIndexOf("End Class", StringComparison.OrdinalIgnoreCase);
            if (endClass < 0) endClass = norm.LastIndexOf("EndClass", StringComparison.OrdinalIgnoreCase);
            if (endClass < 0) return shell;

            var sb = new StringBuilder(norm.Length + methodBlocks.Sum(b => b.Length) + 64);
            sb.Append(norm.Substring(0, endClass));
            sb.AppendLine();
            sb.AppendLine("' --- RuleTrace: shell methods (after fields) ---");
            foreach (string block in methodBlocks)
            {
                sb.AppendLine(block.Trim());
                sb.AppendLine();
            }
            sb.Append(norm.Substring(endClass));
            return sb.ToString().Replace("\n", "\r\n");
        }

        private static string InjectSharedFieldsBeforeEndClass(string shell, Dictionary<string, string> fieldByName, List<string> hostStubs)
        {
            if (fieldByName.Count == 0 && hostStubs.Count == 0) return shell;
            string norm = NormalizeNewlines(shell);
            int endClass = norm.LastIndexOf("End Class", StringComparison.OrdinalIgnoreCase);
            if (endClass < 0) endClass = norm.LastIndexOf("EndClass", StringComparison.OrdinalIgnoreCase);
            if (endClass < 0) return shell;

            var sb = new StringBuilder(norm.Length + 4096);
            sb.Append(norm.Substring(0, endClass));
            sb.AppendLine();
            sb.AppendLine("' --- RuleTrace: shared fields (all Members → shell partial) ---");
            foreach (string decl in fieldByName.Values.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                sb.AppendLine(decl);
            foreach (string stub in hostStubs)
                sb.AppendLine(stub);
            sb.AppendLine();
            sb.Append(norm.Substring(endClass));
            return sb.ToString().Replace("\n", "\r\n");
        }

        /// <summary>Each Member → partial file with all its Sub/Function not already in shell (deduped globally).</summary>
        private static List<KeyValuePair<MemberSource, string>> CollectMemberPartialBodies(
            IList<MemberSource> members, HashSet<string> assignedMethodKeys, Action<string> log)
        {
            var files = new List<KeyValuePair<MemberSource, string>>();
            foreach (MemberSource src in members.OrderBy(s => s.NidMember))
            {
                if (string.IsNullOrWhiteSpace(src.Code)) continue;
                string norm = NormalizeNewlines(src.Code);
                norm = UnwrapEmbeddedClass(norm);
                norm = StripAllOutDeclarations(norm).CleanedText;

                var picked = new List<string>();
                foreach (string block in ExtractMethodBlocks(norm))
                {
                    string key = MethodKey(block);
                    if (string.IsNullOrEmpty(key) || assignedMethodKeys.Contains(key)) continue;
                    assignedMethodKeys.Add(key);
                    picked.Add(block.Trim());
                }
                if (picked.Count == 0) continue;
                files.Add(new KeyValuePair<MemberSource, string>(src, string.Join("\r\n\r\n", picked)));
            }
            return files;
        }

        private static string SanitizeFieldLine(string decl)
        {
            if (string.IsNullOrWhiteSpace(decl)) return null;
            string t = decl.Trim();
            if (t.IndexOf("Property Out", StringComparison.OrdinalIgnoreCase) >= 0) return null;
            if (Regex.IsMatch(t, @"\bM_Out\b", RegexOptions.IgnoreCase)) return null;
            if (Regex.IsMatch(t, @"^\s*Out\s*($|=)", RegexOptions.IgnoreCase)) return null;
            t = Regex.Replace(t, @"\b(?:BIZ\.SC\.)?(?:clsOut|ClsOut)\b", "Object", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (Regex.IsMatch(t, @"^\s*(?:Public|Private|Protected|Friend|Dim)\s+Out\b", RegexOptions.IgnoreCase)) return null;
            return t;
        }

        public static string InjectMethodStubs(string vb, IEnumerable<string> names)
        {
            if (string.IsNullOrWhiteSpace(vb)) return vb;
            var list = (names ?? Enumerable.Empty<string>())
                .Where(n => !string.IsNullOrWhiteSpace(n) && Regex.IsMatch(n, @"^[A-Za-z_]\w*$"))
                .Where(n => !LooksLikeParameterProperty(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (list.Count == 0) return vb;

            string norm = NormalizeNewlines(vb);
            int endClass = norm.LastIndexOf("End Class", StringComparison.OrdinalIgnoreCase);
            if (endClass < 0) return vb;

            var sb = new StringBuilder(norm.Length + list.Count * 80);
            sb.Append(norm.Substring(0, endClass));
            sb.AppendLine();
            sb.AppendLine("' --- RuleTrace: method stubs (compile-only) ---");
            foreach (string name in list)
            {
                sb.AppendLine("Public Sub " + name + "(Optional ByVal ParamArray args() As Object)");
                sb.AppendLine("End Sub");
                sb.AppendLine();
            }
            sb.Append(norm.Substring(endClass));
            return sb.ToString().Replace("\n", "\r\n");
        }

        private static bool LooksLikeParameterProperty(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            return name.StartsWith("P_", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("PM_", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("PP_", StringComparison.OrdinalIgnoreCase);
        }

        private static string ExtractClassName(string shell)
        {
            if (string.IsNullOrWhiteSpace(shell)) return null;
            Match m = Regex.Match(shell, @"(?:Partial\s+)?(?:Public\s+|Private\s+|Friend\s+)?Class\s+(\w+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            return m.Success ? m.Groups[1].Value : null;
        }

        private static string EnsurePartialClassDeclaration(string shell, string className)
        {
            if (string.IsNullOrWhiteSpace(shell)) return shell;
            if (shell.IndexOf("Partial", StringComparison.OrdinalIgnoreCase) >= 0) return shell;
            return Regex.Replace(
                shell,
                @"\b((?:Public|Private|Friend)\s+)?Class\s+" + Regex.Escape(className) + @"\b",
                "Partial Public Class " + className,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private static bool MemberMatchesKeywords(MemberSource s, IList<string> keywords)
        {
            string hay = ((s.Name ?? "") + "\n" + (s.Code ?? "")).ToLowerInvariant();
            foreach (string kw in keywords)
            {
                if (string.IsNullOrWhiteSpace(kw)) continue;
                if (hay.IndexOf(kw.Trim().ToLowerInvariant(), StringComparison.Ordinal) >= 0) return true;
            }
            return false;
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Length > 48 ? name.Substring(0, 48) : name;
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
            var list = GetFunctionList(cls);
            int empty = 0, withBody = 0;
            foreach (object fn in list)
            {
                int len = LenStr(GetMember(fn, "Body") ?? GetMember(fn, "M_Body"));
                if (len == 0) empty++; else withBody++;
            }
                log("Arch         : ClsFunction count=" + list.Count + " emptyBody=" + empty + " withBody=" + withBody);
            int i = 0;
            foreach (object fn in list)
            {
                if (i++ >= max) break;
                string name = Convert.ToString(GetMember(fn, "Name") ?? GetMember(fn, "M_Name") ?? "?");
                int id = ReadInt(fn, "NidFunction");
                int len = LenStr(GetMember(fn, "Body") ?? GetMember(fn, "M_Body"));
                string body = Convert.ToString(GetMember(fn, "Body") ?? GetMember(fn, "M_Body") ?? "");
                string head = FirstLine(body);
                log("Detail      : ClsFunction " + id + " " + name + " BodyLen=" + len
                    + (LooksLikeMethodDecl(head) ? " WRAPPER" : " inner"));
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
