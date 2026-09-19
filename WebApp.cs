using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace RuleTrace
{
    /// <summary>JSON API for the browser UI. Same engine as the old WinForms form; no VB rewrite.</summary>
    internal sealed class WebApp
    {
        private readonly UserSettings _settings;
        private readonly object _gate = new object();
        private int _busy;

        public WebApp(UserSettings settings)
        {
            _settings = settings ?? new UserSettings();
        }

        public UserSettings Settings { get { return _settings; } }

        public Dictionary<string, object> Ping()
        {
            return new Dictionary<string, object>
            {
                { "ok", true },
                { "banner", BuildInfo.Banner },
                { "label", BuildInfo.Label },
                { "version", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() },
            };
        }

        public Dictionary<string, object> Bootstrap()
        {
            var formulas = new List<object>();
            foreach (var kv in FormulaEngine.FormulaMap)
                formulas.Add(new Dictionary<string, object> { { "name", kv.Key }, { "nid", kv.Value } });

            return new Dictionary<string, object>
            {
                { "ok", true },
                { "banner", BuildInfo.Banner },
                { "label", BuildInfo.Label },
                { "version", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() },
                { "formulas", formulas },
                { "relatedSolh", FormulaEngine.RelatedNidClasses(344) },
                { "chidmanMember", ChidmanAnalyzer.DefaultChidmanMemberId },
                { "dllOk", FormulaEngine.IsDllFolder(_settings.DllPath) },
                { "scopes", PermitScopes.Catalog() },
                { "mustPick", PermitScopes.MustPick },
                { "settings", SettingsMap() },
            };
        }

        public Dictionary<string, object> SaveSettings(Dictionary<string, object> body)
        {
            ApplySettings(body);
            try { _settings.Save(); }
            catch (Exception ex)
            {
                return Fail("ذخیره تنظیمات: " + ex.Message);
            }
            return Ok("تنظیمات ذخیره شد.", SettingsMap());
        }

        public Dictionary<string, object> DetectDll()
        {
            var log = new List<string>();
            string found = FormulaEngine.DetectDllPath(_settings.DllPath);
            if (found == null)
                return Result(false, log, "پوشه DLL پیدا نشد — مسیر را دستی وارد کنید.", null);
            _settings.DllPath = found;
            if (string.IsNullOrWhiteSpace(_settings.CachePath))
                _settings.CachePath = Path.Combine(found, "SafaFormulaCache");
            try { _settings.Save(); } catch { }
            log.Add("DLL folder   : " + found);
            return Ok("پوشه DLL پیدا شد.", new Dictionary<string, object> { { "settings", SettingsMap() } }, log);
        }

        public Dictionary<string, object> TestDb()
        {
            return Run("تست اتصال...", false, (eng, log) =>
            {
                int fail = eng.TestDatabases();
                log.Add(fail == 0 ? "OK — RuleEngine + Sara + Document با debugger در دسترس‌اند." : "FAILED — " + fail + " اتصال ناموفق.");
                return new Dictionary<string, object> { { "fail", fail }, { "ok", fail == 0 } };
            });
        }

        public Dictionary<string, object> Lookup(Dictionary<string, object> body)
        {
            string text = Json.Str(body, "text", _settings.LastLookup).Trim();
            if (text.Length == 0) return Fail("NidWorkItem یا کد نوسازی را وارد کنید.");
            _settings.LastLookup = text;
            try { _settings.Save(); } catch { }
            return Run("جستجو در Sara8M03...", false, (eng, log) =>
            {
                var rows = eng.LookupCases(text);
                string nidProc = rows.Count > 0 ? rows[0][0] : string.Empty;
                if (!string.IsNullOrEmpty(nidProc))
                {
                    _settings.LastNidProc = nidProc;
                    try { _settings.Save(); } catch { }
                }
                log.Add(rows.Count == 0 ? "هیچ درخواستی پیدا نشد." : rows.Count + " درخواست پیدا شد — NidProc اولین مورد انتخاب شد.");
                return new Dictionary<string, object>
                {
                    { "rows", rows },
                    { "nidProc", nidProc },
                    { "count", rows.Count },
                    { "settings", SettingsMap() },
                };
            });
        }

        public Dictionary<string, object> Combine(Dictionary<string, object> body)
        {
            string formula = FormulaOf(body);
            bool allVersions = Json.Bool(body, "allVersions", true);
            int nid;
            if (!TryFormulaId(formula, out nid)) return Fail("فرمول ناشناخته: " + formula);
            return Run("ترکیب DB + DLL...", true, (eng, log) =>
            {
                var members = new List<object>();
                var rows = new List<MemberRow>();
                foreach (int n in FormulaEngine.RelatedNidClasses(nid))
                    rows.AddRange(MemberRepository.List(_settings.RuleEngine, n, allVersions));
                foreach (MemberRow row in rows)
                {
                    members.Add(new Dictionary<string, object>
                    {
                        { "nidClass", row.NidClass },
                        { "className", FormulaEngine.ClassName(row.NidClass) },
                        { "nidMember", row.NidMember },
                        { "version", row.Version },
                        { "active", row.IsActive },
                        { "name", row.Name ?? string.Empty },
                        { "chidman", row.NidMember == ChidmanAnalyzer.DefaultChidmanMemberId },
                        { "code", Cap(row.Code) },
                        { "kb", (row.Code == null ? 0 : row.Code.Length / 1024) },
                    });
                }
                List<DllTypeRow> types = new List<DllTypeRow>();
                try { types = eng.CatalogDllTypes(); }
                catch (Exception ex) { log.Add("DLL catalog  : " + ex.Message); }
                var dll = types.Select(t => new Dictionary<string, object>
                {
                    { "name", t.Name },
                    { "kind", t.Kind },
                    { "assembly", t.Assembly },
                    { "fullName", t.FullName },
                    { "publicMembers", t.PublicMembers },
                }).ToList();
                log.Add("INSPECT      : " + members.Count + " Member row(s) from RuleEngine + " + dll.Count + " public type(s) from DLLs — read-only, no save, no compile");
                return new Dictionary<string, object>
                {
                    { "members", members },
                    { "dllTypes", dll },
                    { "related", FormulaEngine.RelatedNidClasses(nid) },
                };
            });
        }

        public Dictionary<string, object> AnalyzeMembers(Dictionary<string, object> body)
        {
            string formula = FormulaOf(body);
            int nid;
            if (!TryFormulaId(formula, out nid)) return Fail("فرمول ناشناخته: " + formula);
            return Run("تحلیل Member...", false, (eng, log) =>
            {
                MemberAnalyzer.Print(_settings.RuleEngine, nid, log.Add);
                return new Dictionary<string, object> { { "nid", nid } };
            });
        }

        public Dictionary<string, object> AnalyzeChidman(Dictionary<string, object> body)
        {
            string formula = FormulaOf(body);
            int nid;
            if (!TryFormulaId(formula, out nid)) return Fail("فرمول ناشناخته: " + formula);
            return Run("تحلیل Member 1288 (چیدمان) از DB...", false, (eng, log) =>
            {
                log.Add("");
                log.Add("══════════ " + DateTime.Now.ToString("HH:mm:ss") + " CHIDMAN Member " + ChidmanAnalyzer.DefaultChidmanMemberId + " (DB + history) ══════════");
                eng.AnalyzeChidmanMember(nid, ChidmanAnalyzer.DefaultChidmanMemberId);
                var hist = PackHistory(MemberHistory.List(_settings.RuleEngine, FormulaEngine.RelatedNidClasses(nid), 0, 40, null));
                var summary = eng.Summary.ToList();
                if (summary.Count == 0)
                    summary = log.Where(IsCopyLine).ToList();
                return new Dictionary<string, object>
                {
                    { "members", PackSources(eng.LastMemberSources) },
                    { "history", hist },
                    { "summary", summary },
                    { "chidmanMember", ChidmanAnalyzer.DefaultChidmanMemberId },
                };
            });
        }

        public Dictionary<string, object> FormulaHistory(Dictionary<string, object> body)
        {
            string formula = FormulaOf(body);
            int nid;
            if (!TryFormulaId(formula, out nid)) return Fail("فرمول ناشناخته: " + formula);
            int member = Json.Int(body, "nidMember");
            return Run("تاریخچه فرمول از DB...", false, (eng, log) =>
            {
                log.Add("Arch         : DLL جستجو نمی‌شود. منبع = جدول NidHistory در RuleEngine.");
                var ids = new List<int>(FormulaEngine.RelatedNidClasses(nid));
                MemberHistory.Report(_settings.RuleEngine, ids, log.Add);
                var rows = MemberHistory.List(_settings.RuleEngine, ids, member, 80, log.Add);
                var summary = log.Where(IsCopyLine).ToList();
                return new Dictionary<string, object>
                {
                    { "table", MemberHistory.LastTable ?? "" },
                    { "history", PackHistory(rows) },
                    { "summary", summary },
                    { "related", ids },
                };
            });
        }

        public Dictionary<string, object> GetHistoryRow(Dictionary<string, object> body)
        {
            long id = Json.Long(body, "nidHistory");
            if (id <= 0) return Fail("NidHistory نامعتبر است.");
            return Run("خواندن Body تاریخچه...", false, (eng, log) =>
            {
                var row = MemberHistory.Get(_settings.RuleEngine, id, log.Add);
                if (row == null) return new Dictionary<string, object> { { "ok", false }, { "message", "ردیف پیدا نشد" } };
                return new Dictionary<string, object>
                {
                    { "row", PackHistory(new List<HistoryRow> { row })[0] },
                };
            });
        }

        public Dictionary<string, object> DebugSolhNid(Dictionary<string, object> body)
        {
            string nidProc = Json.Str(body, "nidProc").Trim();
            if (nidProc.Length == 0) nidProc = (_settings.LastNidProc ?? "").Trim();
            if (nidProc.Length == 0)
                return Fail("NidProc خالی است — اول پرونده را جستجو کنید، بعد «دیباگ پروانه این Nid» را بزنید.");
            _settings.LastNidProc = nidProc;
            try { _settings.Save(); } catch { }

            bool dll = FormulaEngine.IsDllFolder(_settings.DllPath);
            return Run("دیباگ پروانه برای NidProc " + nidProc + " ...", dll, (eng, log) =>
            {
                var scopes = PermitScopes.Normalize(Json.StrList(body, "scopes"));
                var extra = eng.DebugSteps(nidProc, scopes);
                var sources = new List<MemberSource>(eng.LastMemberSources ?? new List<MemberSource>());
                var permitSummary = eng.Summary.ToList();
                var caseVars = extra.ContainsKey("vars") ? extra["vars"] as List<Dictionary<string, object>> : null;

                var req = new RunRequest
                {
                    NidProc = nidProc,
                    Watch = Json.Str(body, "watch", "Calc_Chandganeh").Trim(),
                    EntryPoint = Json.Str(body, "entry").Trim(),
                    ReCompile = Json.Bool(body, "recompile"),
                    ClearCache = Json.Bool(body, "clearCache"),
                    ShowAllParams = Json.Bool(body, "allParams"),
                    District = Json.Int(body, "district"),
                    SkipRelatedSources = true,
                };
                ParseParams(Json.Str(body, "parameters"), req);

                var dbMap = HoverDebug.Flatten(null, caseVars);
                int seeded = SeedDbParams(req, sources, dbMap, log);
                log.Add("Hover      : مقدار Sara=" + dbMap.Count + " seed=" + seeded
                    + " tarakom=" + (HoverDebug.Lookup(dbMap, "tarakom") ?? "(خالی)"));

                var trace = new List<TraceEvent>();
                var parms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                HoverDebug.MergeInto(parms, dbMap);
                int live = TryLiveHover(eng, log, req, scopes, trace, parms);
                HoverDebug.MergeInto(parms, dbMap);

                int probes, bound;
                var packed = PackSources(sources, trace, parms, caseVars, out probes, out bound);
                extra["members"] = packed;
                extra["trace"] = PackTrace(trace);
                extra["params"] = parms;
                extra["focusMember"] = FocusMember(packed);
                extra["probeCount"] = probes;
                extra["boundCount"] = bound;
                extra["liveCode"] = live;
                extra["hoverGoal"] = HoverDebug.Goal;
                if (sources.Count == 0)
                    log.Add("Hover      : کد Member خالی — dbo.Member برای فرم تیک‌خورده خوانده نشد");
                else
                    log.Add("Hover      : کد Member=" + sources.Count + " probes=" + probes + " مقداردار=" + bound + " — تب کد");
                extra["diagnosis"] = HoverDiagnosis(sources.Count, probes, bound, live);
                extra["nextAction"] = bound > 0 ? "موس را روی خط سبز نگه دارید" : HoverDebug.Goal;
                var summary = permitSummary;
                foreach (string line in eng.Summary)
                    if (!summary.Contains(line)) summary.Add(line);
                foreach (string line in log)
                    if (IsCopyLine(line) && !summary.Contains(line))
                        summary.Add(line);
                extra["summary"] = summary;
                extra["settings"] = SettingsMap();
                extra["exitCode"] = live == 1 ? 1 : 0;
                return extra;
            });
        }

        public Dictionary<string, object> BrowseDocs(Dictionary<string, object> body)
        {
            return Run("مستند کلی MemberDocument...", false, (eng, log) =>
            {
                var extra = eng.BrowseDocs();
                extra["summary"] = eng.Summary.Count > 0 ? eng.Summary.ToList() : log.Where(IsCopyLine).ToList();
                extra["settings"] = SettingsMap();
                return extra;
            });
        }

        public Dictionary<string, object> Inspect(Dictionary<string, object> body)
        {
            string formula = FormulaOf(body);
            int nid;
            if (!TryFormulaId(formula, out nid)) return Fail("فرمول ناشناخته: " + formula);
            return Run("بررسی موتور...", true, (eng, log) =>
            {
                log.Add("");
                log.Add("══════════ " + DateTime.Now.ToString("HH:mm:ss") + " ENGINE INSPECT ══════════");
                eng.InspectClass(nid, eng.ResolveCityGuid());
                return new Dictionary<string, object> { { "nid", nid } };
            });
        }

        public Dictionary<string, object> RunFormula(Dictionary<string, object> body)
        {
            var req = new RunRequest
            {
                Formula = FormulaOf(body),
                NidProc = Json.Str(body, "nidProc").Trim(),
                Watch = Json.Str(body, "watch", "Calc_Chandganeh").Trim(),
                EntryPoint = Json.Str(body, "entry").Trim(),
                ReCompile = Json.Bool(body, "recompile"),
                ClearCache = Json.Bool(body, "clearCache"),
                ShowAllParams = Json.Bool(body, "allParams"),
                District = Json.Int(body, "district"),
            };
            ParseParams(Json.Str(body, "parameters"), req);
            _settings.LastNidProc = req.NidProc;
            _settings.LastFormula = req.Formula;
            _settings.LastWatch = req.Watch;
            try { _settings.Save(); } catch { }

            if (req.NidProc.Length > 0)
            {
                var scopes = PermitScopes.Normalize(Json.StrList(body, "scopes"));
                if (scopes.Count == 0)
                    return Fail(PermitScopes.MustPick);
                bool dll = FormulaEngine.IsDllFolder(_settings.DllPath);
                string formTitle = PermitScopes.Title(scopes[0]);
                return Run("دیباگ hover فرم " + formTitle + " — بدون UI سارا", dll, (eng, log) =>
                {
                    bool prevStrict = FormulaEngine.StrictSummary;
                    FormulaEngine.StrictSummary = true;
                    try
                    {
                    log.Add("Hover      : " + HoverDebug.Goal);
                    List<MemberSource> sources = new List<MemberSource>();
                    try { sources = eng.LoadFormSources(scopes); }
                    catch (Exception ex) { log.Add("Hover      : Member خوانده نشد — " + ex.Message); }

                    List<Dictionary<string, object>> caseVars = new List<Dictionary<string, object>>();
                    try { caseVars = ZabetehCase.ReadSelected(_settings.Sara, req.NidProc, scopes, log.Add); }
                    catch (Exception ex) { log.Add("Hover      : Sara vars — " + FirstLineSafe(ex.Message)); }

                    var dbMap = HoverDebug.Flatten(null, caseVars);
                    int seeded = SeedDbParams(req, sources, dbMap, log);
                    log.Add("Hover      : مقدار Sara=" + dbMap.Count + " seed=" + seeded
                        + " tarakom=" + (HoverDebug.Lookup(dbMap, "tarakom") ?? "(خالی)"));

                    var trace = new List<TraceEvent>();
                    var parms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    HoverDebug.MergeInto(parms, dbMap);
                    int live = TryLiveHover(eng, log, req, scopes, trace, parms);
                    HoverDebug.MergeInto(parms, dbMap);

                    int probes = 0, bound = 0;
                    var packed = PackSources(sources, trace, parms, caseVars, out probes, out bound);
                    int focus = FocusMember(packed);
                    log.Add("Hover      : probes=" + probes + " مقداردار=" + bound + " — موس را روی خط نگه‌دارید");

                    string diagnosis = HoverDiagnosis(sources.Count, probes, bound, live);
                    string next = bound > 0 ? "موس را روی خط سبز نگه دارید" : HoverDebug.Goal;
                    if (probes > 0 && bound == 0 && live == 2)
                        log.Add("Hover      : " + HoverDebug.NoInstance);

                    var extra = new Dictionary<string, object>
                    {
                        { "members", packed },
                        { "trace", PackTrace(trace) },
                        { "params", parms },
                        { "watch", req.Watch },
                        { "hoverGoal", HoverDebug.Goal },
                        { "probeCount", probes },
                        { "boundCount", bound },
                        { "liveCode", live },
                        { "diagnosis", diagnosis },
                        { "nextAction", next },
                        { "vars", caseVars },
                        { "summary", log.Where(IsCopyLine).ToList() },
                        { "settings", SettingsMap() },
                        { "chidmanMember", ChidmanAnalyzer.DefaultChidmanMemberId },
                        { "focusMember", focus },
                        { "exitCode", live == 1 ? 1 : 0 },
                    };
                    return extra;
                    }
                    finally
                    {
                        FormulaEngine.StrictSummary = prevStrict;
                    }
                });
            }

            return Run("اجرای فرمول " + req.Formula + " ...", true, (eng, log) =>
            {
                log.Add("");
                log.Add("══════════ " + DateTime.Now.ToString("HH:mm:ss") + " ══════════");
                int code = eng.Run(req);
                log.Add("Exit code    : " + code + (code == 0 ? " (OK)" : code == 1 ? " (Stop error in BizErrors)" : code == 2 ? " (no live instance — static debug)" : code == 4 ? " (runtime/engine error)" : ""));
                if (code == 2)
                {
                    log.Add("معماری: منبع فرمول dbo.Member + تاریخچه NidHistory است، نه DLL به‌روز.");
                    log.Add("Instanc ساخته نشد — ادامه با بررسی متن فرمول و لاگ تغییرات در دیتابیس.");
                }
                var summary = eng.Summary.ToList();
                summary.Add("Exit code    : " + code);
                var parms = new Dictionary<string, string>(eng.LastParams, StringComparer.OrdinalIgnoreCase);
                var trace = eng.LastTrace.Select(t => new Dictionary<string, object>
                {
                    { "index", t.Index },
                    { "action", t.Action },
                    { "key", t.Key },
                    { "title", t.Title },
                }).ToList();
                return new Dictionary<string, object>
                {
                    { "exitCode", code },
                    { "summary", summary },
                    { "trace", trace },
                    { "params", parms },
                    { "watch", req.Watch },
                    { "members", PackSources(eng.LastMemberSources) },
                    { "chidmanMember", ChidmanAnalyzer.DefaultChidmanMemberId },
                    { "settings", SettingsMap() },
                };
            });
        }

        private Dictionary<string, object> Run(string title, bool loadDlls, Func<FormulaEngine, List<string>, Dictionary<string, object>> work)
        {
            if (Interlocked.CompareExchange(ref _busy, 1, 0) != 0)
                return Fail("یک عملیات در حال اجراست...");

            var log = new List<string>();
            try
            {
                log.Add(BuildInfo.Banner);
                log.Add(title);
                FormulaEngine eng;
                lock (_gate)
                {
                    eng = new FormulaEngine(_settings, log.Add);
                    if (loadDlls)
                    {
                        eng.LoadAssemblies();
                        eng.ApplyConnections();
                    }
                    Dictionary<string, object> data = work(eng, log) ?? new Dictionary<string, object>();
                    data["ok"] = true;
                    data["log"] = log;
                    return data;
                }
            }
            catch (Exception ex)
            {
                log.Add("FATAL: " + ex.GetType().Name + ": " + ex.Message);
                if (ex.InnerException != null) log.Add("  inner: " + ex.InnerException.Message);
                log.Add(ex.StackTrace ?? string.Empty);
                return Result(false, log, ex.Message, null);
            }
            finally
            {
                Interlocked.Exchange(ref _busy, 0);
            }
        }

        private void ApplySettings(Dictionary<string, object> body)
        {
            if (body == null) return;
            if (body.ContainsKey("dllPath")) _settings.DllPath = Json.Str(body, "dllPath").Trim();
            if (body.ContainsKey("ruleEngine")) _settings.RuleEngine = Json.Str(body, "ruleEngine").Trim();
            if (body.ContainsKey("sara")) _settings.Sara = Json.Str(body, "sara").Trim();
            if (body.ContainsKey("cityGuid")) _settings.CityGuid = Json.Str(body, "cityGuid").Trim();
            if (body.ContainsKey("cachePath")) _settings.CachePath = Json.Str(body, "cachePath").Trim();
            if (body.ContainsKey("nidProc")) _settings.LastNidProc = Json.Str(body, "nidProc").Trim();
            if (body.ContainsKey("formula")) _settings.LastFormula = Json.Str(body, "formula").Trim();
            if (body.ContainsKey("watch")) _settings.LastWatch = Json.Str(body, "watch").Trim();
            if (body.ContainsKey("lookup")) _settings.LastLookup = Json.Str(body, "lookup").Trim();
        }

        private Dictionary<string, object> SettingsMap()
        {
            return new Dictionary<string, object>
            {
                { "dllPath", _settings.DllPath ?? string.Empty },
                { "ruleEngine", _settings.RuleEngine ?? string.Empty },
                { "sara", _settings.Sara ?? string.Empty },
                { "cityGuid", _settings.CityGuid ?? string.Empty },
                { "cachePath", _settings.CachePath ?? string.Empty },
                { "nidProc", _settings.LastNidProc ?? string.Empty },
                { "formula", string.IsNullOrWhiteSpace(_settings.LastFormula) ? "Solh" : _settings.LastFormula },
                { "watch", string.IsNullOrWhiteSpace(_settings.LastWatch) ? "Calc_Chandganeh" : _settings.LastWatch },
                { "lookup", _settings.LastLookup ?? string.Empty },
                { "dllOk", FormulaEngine.IsDllFolder(_settings.DllPath) },
            };
        }

        /// <summary>Run the ticked form so logfilefj AddError fills hover values. Does not rewrite VB.</summary>
        private int TryLiveHover(FormulaEngine eng, List<string> log, RunRequest req, IList<string> scopes, List<TraceEvent> trace, Dictionary<string, string> parms)
        {
            int live = 2;
            if (eng == null || req == null) return live;
            if (scopes == null || scopes.Count == 0)
            {
                log.Add("Hover      : فرمی تیک نخورده — اجرا زنده نشد");
                return live;
            }
            if (!FormulaEngine.IsDllFolder(_settings.DllPath))
            {
                log.Add("Hover      : پوشه DLL نیست — مقدار hover بعد از اجرای زنده پر می‌شود");
                return live;
            }
            int nid = PermitScopes.FormulaNid(scopes[0]);
            if (nid <= 0)
            {
                log.Add("Hover      : NidClass فرم تیک‌خورده پیدا نشد");
                return live;
            }
            req.Formula = FormulaEngine.ClassName(nid);
            req.SkipRelatedSources = true;
            string formTitle = PermitScopes.Title(scopes[0]);
            log.Add("Hover      : اجرای زنده " + formTitle + " (" + req.Formula + "/" + nid + ") — معادل باز کردن فرم");
            try
            {
                live = eng.Run(req);
                if (eng.LastTrace != null && trace != null) trace.AddRange(eng.LastTrace);
                if (parms != null)
                    foreach (var kv in eng.LastParams) parms[kv.Key] = kv.Value;
                log.Add("Hover      : BizErrors/logfilefj=" + (trace == null ? 0 : trace.Count) + " ParametersValue=" + (parms == null ? 0 : parms.Count));
            }
            catch (Exception ex)
            {
                log.Add("Hover      : اجرا زنده نشد — " + FirstLineSafe(ex.Message) + " — hover از سورس logfilefj");
                live = 2;
            }
            return live;
        }

        private static string HoverDiagnosis(int members, int probes, int bound, int live)
        {
            if (members == 0)
                return "کد Member خوانده نشد — اتصال RuleEngine و تیک فرم را چک کنید";
            if (bound > 0)
                return "موس را روی خط سبز نگه دارید — " + bound + " مقدار (دیتابیس/اجرا) مثل tarakom=120";
            if (probes > 0)
                return "پروب=" + probes + " — مقدار معتبر در جداول ضابطه این Nid پیدا نشد";
            return members + " Member از فرم تیک‌خورده — متغیرهای خط از Sara پر می‌شوند";
        }

        private static int SeedDbParams(RunRequest req, IList<MemberSource> sources, IDictionary<string, string> db, List<string> log)
        {
            if (req == null || db == null || db.Count == 0) return 0;
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string n in new[] { "tarakom", "Tarakom", "تراکم", "Masahat", "Ertefa" })
                names.Add(n);
            if (sources != null)
            {
                foreach (MemberSource s in sources)
                {
                    if (s == null || string.IsNullOrEmpty(s.Code)) continue;
                    foreach (string id in HoverDebug.Idents(s.Code))
                        names.Add(id);
                }
            }
            int n = 0;
            foreach (string name in names)
            {
                string v = HoverDebug.Lookup(db, name);
                if (v == null) continue;
                string cur;
                if (req.Parameters.TryGetValue(name, out cur) && !string.IsNullOrWhiteSpace(cur)) continue;
                req.Parameters[name] = v;
                n++;
            }
            return n;
        }

        private static bool IsCopyLine(string m)
        {
            return FormulaEngine.IsSummaryLine(m);
        }

        private static string FormulaOf(Dictionary<string, object> body)
        {
            string f = Json.Str(body, "formula", "Solh").Trim();
            return f.Length == 0 ? "Solh" : f;
        }

        private static bool TryFormulaId(string formula, out int nid)
        {
            if (FormulaEngine.FormulaMap.TryGetValue(formula ?? string.Empty, out nid)) return true;
            return int.TryParse(formula, out nid) && nid > 0;
        }

        private static void ParseParams(string text, RunRequest req)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            foreach (string raw in text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string line = raw.Trim();
                int eq = line.IndexOf('=');
                if (eq <= 0) continue;
                string key = line.Substring(0, eq).Trim();
                string val = line.Substring(eq + 1).Trim();
                if (key.StartsWith("factory:", StringComparison.OrdinalIgnoreCase))
                    req.FactoryParameters[key.Substring(8).Trim()] = val;
                else req.Parameters[key] = val;
            }
        }

        private static List<object> PackHistory(IList<HistoryRow> rows)
        {
            var list = new List<object>();
            if (rows == null) return list;
            foreach (HistoryRow h in rows)
            {
                list.Add(new Dictionary<string, object>
                {
                    { "nidHistory", h.NidHistory },
                    { "nidClass", h.NidClass },
                    { "className", FormulaEngine.ClassName(h.NidClass) },
                    { "nidMember", h.NidMember },
                    { "fromDate", h.FromDate ?? "" },
                    { "toDate", h.ToDate ?? "" },
                    { "enumType", h.EnumType ?? "" },
                    { "active", h.IsActive },
                    { "versionDateTime", h.VersionDateTime ?? "" },
                    { "modifyer", h.Modifyer ?? "" },
                    { "modifyDate", h.ModifyDate ?? "" },
                    { "modifyTime", h.ModifyTime ?? "" },
                    { "modifyDesc", h.ModifyDesc ?? "" },
                    { "bodyChars", h.BodyChars },
                    { "code", h.Code ?? "" },
                    { "table", h.TableName ?? "" },
                });
            }
            return list;
        }

        private static List<object> PackSources(IList<MemberSource> sources)
        {
            int probes, bound;
            return PackSources(sources, null, null, null, out probes, out bound);
        }

        private static List<object> PackSources(IList<MemberSource> sources, IList<TraceEvent> trace, IDictionary<string, string> parms, IList<Dictionary<string, object>> vars, out int probes, out int bound)
        {
            probes = 0;
            bound = 0;
            var list = new List<object>();
            if (sources == null) return list;
            foreach (MemberSource s in sources)
            {
                var items = HoverDebug.Parse(s.Code);
                var map = HoverDebug.Flatten(parms, vars);
                HoverDebug.Bind(items, trace, map, vars);
                HoverDebug.BindIdents(s.Code, items, map);
                probes += HoverDebug.ProbeCount(items);
                bound += HoverDebug.BoundCount(items);
                list.Add(new Dictionary<string, object>
                {
                    { "nidClass", s.NidClass },
                    { "className", FormulaEngine.ClassName(s.NidClass) },
                    { "nidMember", s.NidMember },
                    { "name", s.Name ?? string.Empty },
                    { "meta", s.Meta ?? string.Empty },
                    { "version", s.Version },
                    { "active", s.IsActive },
                    { "chidman", s.NidMember == ChidmanAnalyzer.DefaultChidmanMemberId },
                    { "solhRun", s.NidClass == 344 && (s.NidMember == SolhNidDebug.SolhRunMember || s.NidMember == SolhNidDebug.SolhInitMember) },
                    { "code", Cap(s.Code) },
                    { "label", s.ToString() },
                    { "hover", HoverDebug.PackLines(items) },
                    { "probeCount", HoverDebug.ProbeCount(items) },
                });
            }
            return list;
        }

        private static int FocusMember(List<object> packed)
        {
            int best = 0, nid = 0;
            if (packed == null) return 0;
            foreach (object o in packed)
            {
                var d = o as Dictionary<string, object>;
                if (d == null) continue;
                int probes = Json.Int(d, "probeCount");
                if (probes <= best) continue;
                best = probes;
                nid = Json.Int(d, "nidMember");
            }
            return nid;
        }

        private static List<object> PackTrace(IList<TraceEvent> trace)
        {
            var list = new List<object>();
            if (trace == null) return list;
            foreach (TraceEvent t in trace)
            {
                list.Add(new Dictionary<string, object>
                {
                    { "index", t.Index },
                    { "action", t.Action },
                    { "key", t.Key },
                    { "title", t.Title },
                });
            }
            return list;
        }

        private static string FirstLineSafe(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            int i = s.IndexOf('\n');
            return i < 0 ? s : s.Substring(0, i);
        }

        private static string Cap(string code)
        {
            if (code == null) return string.Empty;
            const int max = 400000;
            if (code.Length <= max) return code;
            return code.Substring(0, max) + "\n\n/* truncated " + (code.Length - max) + " chars */";
        }

        private static Dictionary<string, object> Ok(string message, Dictionary<string, object> extra = null, List<string> log = null)
        {
            return Result(true, log ?? new List<string> { message }, message, extra);
        }

        private static Dictionary<string, object> Fail(string message)
        {
            return Result(false, new List<string> { message }, message, null);
        }

        private static Dictionary<string, object> Result(bool ok, List<string> log, string message, Dictionary<string, object> extra)
        {
            var map = extra ?? new Dictionary<string, object>();
            map["ok"] = ok;
            map["message"] = message ?? string.Empty;
            map["log"] = log ?? new List<string>();
            return map;
        }

        public static string SummaryText(IList<string> summary)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== RuleTrace summary ===");
            if (summary != null)
                foreach (string s in summary) sb.AppendLine(s);
            return sb.ToString();
        }
    }
}
