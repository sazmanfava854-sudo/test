using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RuleTrace
{
    internal sealed class MainForm : Form
    {
        private readonly UserSettings _settings;

        // settings
        private TextBox _txtDllPath, _txtRuleEngine, _txtSara, _txtCityGuid, _txtCachePath;
        // case
        private TextBox _txtLookup, _txtNidProc, _txtWatch, _txtEntry, _txtDistrict, _txtParams;
        private ComboBox _cboFormula;
        private CheckBox _chkRecompile, _chkClearCache, _chkAllParams;
        private ListView _lvCases;
        // actions
        private Button _btnBrowse, _btnDetect, _btnTestDb, _btnLookup, _btnAnalyze, _btnInspect, _btnRun, _btnClearLog, _btnSaveLog, _btnCopyLog;
        private TextBox _txtLog;
        private TabControl _tabs;
        private TabPage _tabLog, _tabDebug;
        private DebugPanel _debug;
        private GroupBox _grpSettings;
        private Button _btnToggleSettings;
        private ToolStripStatusLabel _status;
        private ToolStripProgressBar _progress;

        private int _running;
        private int _sourcesNid;

        public MainForm(UserSettings settings)
        {
            _settings = settings;
            KeyPreview = true;
            BuildUi();
            LoadFromSettings();
        }

        /// <summary>VS-style keys: F10/F11 next step, Shift+F10 previous, F5 run (or run-to-end when a trace exists), Ctrl+Home first.</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F10:
                case Keys.F11:
                    if (_debug.HasTrace) { _tabs.SelectedTab = _tabDebug; _debug.StepNext(); return true; }
                    break;
                case Keys.F10 | Keys.Shift:
                case Keys.F11 | Keys.Shift:
                    if (_debug.HasTrace) { _tabs.SelectedTab = _tabDebug; _debug.StepPrev(); return true; }
                    break;
                case Keys.Home | Keys.Control:
                    if (_debug.HasTrace && _tabs.SelectedTab == _tabDebug) { _debug.StepFirst(); return true; }
                    break;
                case Keys.F5:
                    if (_debug.HasTrace && _tabs.SelectedTab == _tabDebug) _debug.StepLast();
                    else if (_running == 0) RunFormula();
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ───────────────────────────── UI construction ─────────────────────────────

        private void BuildUi()
        {
            Text = BuildInfo.Banner;
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1180, 880);
            MinimumSize = new Size(980, 680);
            Font = new Font("Segoe UI", 9F);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(8) };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            _grpSettings = BuildSettingsGroup();
            root.Controls.Add(_grpSettings, 0, 0);
            root.Controls.Add(BuildCaseGroup(), 0, 1);
            root.Controls.Add(BuildActionBar(), 0, 2);
            root.Controls.Add(BuildLog(), 0, 3);

            var strip = new StatusStrip();
            _status = new ToolStripStatusLabel("آماده") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            _progress = new ToolStripProgressBar { Style = ProgressBarStyle.Marquee, Visible = false, Width = 160 };
            strip.Items.Add(_status);
            strip.Items.Add(_progress);
            Controls.Add(strip);
        }

        private GroupBox BuildSettingsGroup()
        {
            var g = new GroupBox { Text = "تنظیمات (DLL + دیتابیس) — خودکار ذخیره می‌شود", Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
            var t = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 4 };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            _txtDllPath = Tb();
            _btnBrowse = Btn("انتخاب پوشه...", (s, e) => BrowseDll());
            _btnDetect = Btn("پیدا کردن خودکار", (s, e) => DetectDll());
            AddRow(t, 0, "پوشه DLL (dll10):", _txtDllPath, _btnBrowse, _btnDetect);

            _txtRuleEngine = Tb();
            AddRow(t, 1, "RuleEngine (DbRuleEngein):", _txtRuleEngine, null, null);

            _txtSara = Tb();
            AddRow(t, 2, "Sara (Sara8M03):", _txtSara, null, null);

            _txtCityGuid = Tb();
            var lblCity = new Label { Text = "خالی = از CI_City خوانده می‌شود", AutoSize = true, ForeColor = Color.DimGray, Anchor = AnchorStyles.Left, Margin = new Padding(6, 6, 0, 0) };
            AddRow(t, 3, "CityGuid (مشهد):", _txtCityGuid, lblCity, null);

            _txtCachePath = Tb();
            AddRow(t, 4, "پوشه Cache محلی:", _txtCachePath, null, null);

            g.Controls.Add(t);
            return g;
        }

        private GroupBox BuildCaseGroup()
        {
            var g = new GroupBox { Text = "پرونده و فرمول", Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
            var t = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 6 };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            // row 0: lookup
            _txtLookup = Tb();
            _btnLookup = Btn("جستجو در Sara", (s, e) => Lookup());
            t.Controls.Add(Lbl("NidWorkItem / کد نوسازی:"), 0, 0);
            t.Controls.Add(_txtLookup, 1, 0);
            t.Controls.Add(_btnLookup, 2, 0);
            _lvCases = new ListView { View = View.Details, FullRowSelect = true, Height = 92, Dock = DockStyle.Fill, HideSelection = false };
            _lvCases.Columns.Add("NidProc", 270);
            _lvCases.Columns.Add("NidWorkItem", 90);
            _lvCases.Columns.Add("گردش‌کار", 150);
            _lvCases.Columns.Add("تاریخ", 80);
            _lvCases.Columns.Add("متقاضی / کد نوسازی", 180);
            _lvCases.SelectedIndexChanged += (s, e) =>
            {
                if (_lvCases.SelectedItems.Count > 0) _txtNidProc.Text = _lvCases.SelectedItems[0].Text;
            };
            t.Controls.Add(_lvCases, 3, 0);
            t.SetColumnSpan(_lvCases, 3);
            t.SetRowSpan(_lvCases, 3);

            // row 1: nidproc
            _txtNidProc = Tb();
            t.Controls.Add(Lbl("NidProc:"), 0, 1);
            t.Controls.Add(_txtNidProc, 1, 1);
            t.SetColumnSpan(_txtNidProc, 2);

            // row 2: formula + watch
            _cboFormula = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown, Dock = DockStyle.Fill, Margin = new Padding(3, 3, 3, 3) };
            foreach (var kv in FormulaEngine.FormulaMap) _cboFormula.Items.Add(kv.Key);
            var pFormula = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 3, Margin = new Padding(0) };
            pFormula.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            pFormula.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            pFormula.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            _txtWatch = Tb();
            pFormula.Controls.Add(_cboFormula, 0, 0);
            pFormula.Controls.Add(Lbl("متغیر Watch:"), 1, 0);
            pFormula.Controls.Add(_txtWatch, 2, 0);
            t.Controls.Add(Lbl("فرمول:"), 0, 2);
            t.Controls.Add(pFormula, 1, 2);
            t.SetColumnSpan(pFormula, 2);

            // row 3: options
            var opts = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Margin = new Padding(0) };
            _chkRecompile = new CheckBox { Text = "Recompile (کند — فقط اولین بار / تغییر فرمول)", AutoSize = true };
            _chkClearCache = new CheckBox { Text = "پاک کردن Cache قبل از اجرا", AutoSize = true };
            _chkAllParams = new CheckBox { Text = "نمایش همه ParametersValue", AutoSize = true };
            _txtEntry = new TextBox { Width = 150 };
            _txtDistrict = new TextBox { Width = 50 };
            opts.Controls.Add(_chkRecompile);
            opts.Controls.Add(_chkClearCache);
            opts.Controls.Add(_chkAllParams);
            opts.Controls.Add(new Label { Text = "Entry (خالی=خودکار):", AutoSize = true, Margin = new Padding(12, 6, 3, 0) });
            opts.Controls.Add(_txtEntry);
            opts.Controls.Add(new Label { Text = "ناحیه:", AutoSize = true, Margin = new Padding(12, 6, 3, 0) });
            opts.Controls.Add(_txtDistrict);
            t.Controls.Add(Lbl("گزینه‌ها:"), 0, 3);
            t.Controls.Add(opts, 1, 3);
            t.SetColumnSpan(opts, 5);

            // row 4: params
            _txtParams = new TextBox { Multiline = true, Height = 44, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical, Font = new Font("Consolas", 9F) };
            t.Controls.Add(Lbl("پارامترها (Name=Value هر خط):"), 0, 4);
            t.Controls.Add(_txtParams, 1, 4);
            t.SetColumnSpan(_txtParams, 5);

            g.Controls.Add(t);
            return g;
        }

        private Control BuildActionBar()
        {
            var p = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 4, 0, 4) };
            _btnRun = Btn("▶  اجرا و دیباگ", (s, e) => RunFormula());
            _btnRun.Font = new Font(Font, FontStyle.Bold);
            _btnRun.Height = 34; _btnRun.Width = 160;
            _btnTestDb = Btn("تست اتصال DB", (s, e) => TestDb());
            _btnAnalyze = Btn("تحلیل Member", (s, e) => AnalyzeMembers());
            _btnInspect = Btn("بررسی موتور (ClsClass)", (s, e) => InspectEngine());
            _btnClearLog = Btn("پاک کردن خروجی", (s, e) => _txtLog.Clear());
            _btnCopyLog = Btn("کپی خروجی", (s, e) => { if (_txtLog.TextLength > 0) Clipboard.SetText(_txtLog.Text); });
            _btnSaveLog = Btn("ذخیره خروجی...", (s, e) => SaveLog());
            _btnToggleSettings = Btn("▲ پنهان کردن تنظیمات", (s, e) => ToggleSettings(!_grpSettings.Visible));
            foreach (var b in new[] { _btnRun, _btnTestDb, _btnAnalyze, _btnInspect, _btnClearLog, _btnCopyLog, _btnSaveLog, _btnToggleSettings }) p.Controls.Add(b);
            p.Controls.Add(new Label { Text = "کلیدها: F5 اجرا • F10 رویداد بعدی • Shift+F10 قبلی", AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(12, 10, 0, 0) });
            return p;
        }

        /// <summary>Hides the settings block so the debug tab gets the vertical space; settings stay editable after re-showing.</summary>
        private void ToggleSettings(bool show)
        {
            _grpSettings.Visible = show;
            _btnToggleSettings.Text = show ? "▲ پنهان کردن تنظیمات" : "▼ نمایش تنظیمات";
        }

        private Control BuildLog()
        {
            _txtLog = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Font = new Font("Consolas", 9.5F),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.Gainsboro,
            };

            _debug = new DebugPanel();
            _debug.LoadSourcesRequested += () => LoadSources(force: true);
            _debug.RunRequested += RunFormula;

            _tabs = new TabControl { Dock = DockStyle.Fill };
            _tabLog = new TabPage("خروجی (Log)");
            _tabLog.Controls.Add(_txtLog);
            _tabDebug = new TabPage("دیباگ مرحله‌ای — F10");
            _tabDebug.Controls.Add(_debug);
            _tabs.TabPages.Add(_tabLog);
            _tabs.TabPages.Add(_tabDebug);
            return _tabs;
        }

        private static TextBox Tb() { return new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3) }; }
        private static Label Lbl(string text) { return new Label { Text = text, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 7, 3, 0) }; }
        private static Button Btn(string text, EventHandler onClick)
        {
            var b = new Button { Text = text, AutoSize = true, Padding = new Padding(6, 2, 6, 2), Margin = new Padding(3) };
            b.Click += onClick;
            return b;
        }
        private static void AddRow(TableLayoutPanel t, int row, string label, Control input, Control extra1, Control extra2)
        {
            t.Controls.Add(Lbl(label), 0, row);
            t.Controls.Add(input, 1, row);
            if (extra1 != null) t.Controls.Add(extra1, 2, row);
            if (extra2 != null) t.Controls.Add(extra2, 3, row);
        }

        // ───────────────────────────── settings <-> UI ─────────────────────────────

        private void LoadFromSettings()
        {
            _txtDllPath.Text = _settings.DllPath;
            _txtRuleEngine.Text = _settings.RuleEngine;
            _txtSara.Text = _settings.Sara;
            _txtCityGuid.Text = _settings.CityGuid;
            _txtCachePath.Text = _settings.CachePath;
            _txtNidProc.Text = _settings.LastNidProc;
            _txtLookup.Text = _settings.LastLookup;
            _txtWatch.Text = _settings.LastWatch;
            _cboFormula.Text = string.IsNullOrWhiteSpace(_settings.LastFormula) ? "Solh" : _settings.LastFormula;

            Log(BuildInfo.Banner);
            Log("RuleTrace ready. Settings: " + UserSettings.IniPath);
            if (!FormulaEngine.IsDllFolder(_settings.DllPath))
                Log("WARN: پوشه DLL معتبر نیست — «پیدا کردن خودکار» یا «انتخاب پوشه» را بزنید.");
            else
                Log("DLL folder   : " + _settings.DllPath);
        }

        private void SaveToSettings()
        {
            _settings.DllPath = _txtDllPath.Text.Trim();
            _settings.RuleEngine = _txtRuleEngine.Text.Trim();
            _settings.Sara = _txtSara.Text.Trim();
            _settings.CityGuid = _txtCityGuid.Text.Trim();
            _settings.CachePath = _txtCachePath.Text.Trim();
            _settings.LastNidProc = _txtNidProc.Text.Trim();
            _settings.LastLookup = _txtLookup.Text.Trim();
            _settings.LastWatch = _txtWatch.Text.Trim();
            _settings.LastFormula = _cboFormula.Text.Trim();
            try { _settings.Save(); } catch (Exception ex) { Log("WARN: cannot save settings: " + ex.Message); }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveToSettings();
            base.OnFormClosing(e);
        }

        // ───────────────────────────── actions ─────────────────────────────

        private void BrowseDll()
        {
            using (var dlg = new FolderBrowserDialog { Description = "پوشه‌ای که BIZ.SC.DLL و SafaClassDesingerNew.dll دارد", SelectedPath = _txtDllPath.Text })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                _txtDllPath.Text = dlg.SelectedPath;
                if (string.IsNullOrWhiteSpace(_txtCachePath.Text))
                    _txtCachePath.Text = Path.Combine(dlg.SelectedPath, "SafaFormulaCache");
                Log(FormulaEngine.IsDllFolder(dlg.SelectedPath) ? "DLL folder OK: " + dlg.SelectedPath : "WARN: DLLهای لازم در این پوشه نیست");
            }
        }

        private void DetectDll()
        {
            string found = FormulaEngine.DetectDllPath(_txtDllPath.Text);
            if (found == null) { Log("DLL folder not found automatically — use «انتخاب پوشه»."); return; }
            _txtDllPath.Text = found;
            if (string.IsNullOrWhiteSpace(_txtCachePath.Text)) _txtCachePath.Text = Path.Combine(found, "SafaFormulaCache");
            Log("DLL folder   : " + found);
        }

        private void TestDb()
        {
            RunBackground("تست اتصال...", eng =>
            {
                int fail = eng.TestDatabases();
                Log(fail == 0 ? "OK — هر دو دیتابیس با debugger در دسترس‌اند." : "FAILED — " + fail + " اتصال ناموفق.");
            }, loadDlls: false);
        }

        private void Lookup()
        {
            string text = _txtLookup.Text.Trim();
            if (text.Length == 0) { Log("NidWorkItem یا کد نوسازی را وارد کنید."); return; }
            RunBackground("جستجو در Sara8M03...", eng =>
            {
                var rows = eng.LookupCases(text);
                Ui(() =>
                {
                    _lvCases.Items.Clear();
                    foreach (var r in rows)
                    {
                        var item = new ListViewItem(r[0]);
                        for (int i = 1; i < r.Length; i++) item.SubItems.Add(r[i]);
                        _lvCases.Items.Add(item);
                    }
                    if (rows.Count > 0) { _lvCases.Items[0].Selected = true; _txtNidProc.Text = rows[0][0]; }
                });
                Log(rows.Count == 0 ? "هیچ درخواستی پیدا نشد." : rows.Count + " درخواست پیدا شد — NidProc اولین مورد انتخاب شد.");
            }, loadDlls: false);
        }

        private bool TryFormulaId(out int nid)
        {
            string f = _cboFormula.Text.Trim();
            if (FormulaEngine.FormulaMap.TryGetValue(f, out nid)) return true;
            if (int.TryParse(f, out nid) && nid > 0) return true;
            Log("فرمول ناشناخته: " + f);
            return false;
        }

        private void AnalyzeMembers()
        {
            int nid;
            if (!TryFormulaId(out nid)) return;
            string cs = _txtRuleEngine.Text.Trim();
            RunBackground("تحلیل Member...", eng => MemberAnalyzer.Print(cs, nid, Log), loadDlls: false);
        }

        /// <summary>Loads ClsClass through the Sara engine without running and dumps what it parsed from dbo.Member.</summary>
        private void InspectEngine()
        {
            int nid;
            if (!TryFormulaId(out nid)) return;
            _tabs.SelectedTab = _tabLog;
            RunBackground("بررسی موتور...", eng =>
            {
                Log("");
                Log("══════════ " + DateTime.Now.ToString("HH:mm:ss") + " ENGINE INSPECT ══════════");
                eng.InspectClass(nid, eng.ResolveCityGuid());
            }, loadDlls: true);
        }

        /// <summary>Pulls the VB source of every Member row into the debug panel (once per formula unless forced).</summary>
        private void LoadSources(bool force)
        {
            int nid;
            if (!TryFormulaId(out nid)) return;
            if (!force && _sourcesNid == nid && _debug.HasSources) return;
            RunBackground("بارگذاری کد فرمول از DB...", eng => LoadSourcesInto(eng, nid), loadDlls: false);
        }

        private void LoadSourcesInto(FormulaEngine eng, int nid)
        {
            var sources = eng.GetMemberSources(nid);
            _sourcesNid = nid;
            Ui(() => _debug.SetSources(sources));
            Log("Source       : " + sources.Count + " Member row(s) loaded into debug panel (" + (sources.Sum(s => (long)s.Code.Length) / 1024) + " KB)");
        }

        private void RunFormula()
        {
            var req = new RunRequest
            {
                Formula = _cboFormula.Text.Trim(),
                NidProc = _txtNidProc.Text.Trim(),
                Watch = _txtWatch.Text.Trim(),
                EntryPoint = _txtEntry.Text.Trim(),
                ReCompile = _chkRecompile.Checked,
                ClearCache = _chkClearCache.Checked,
                ShowAllParams = _chkAllParams.Checked,
            };
            int district;
            if (int.TryParse(_txtDistrict.Text.Trim(), out district)) req.District = district;
            foreach (string raw in _txtParams.Lines)
            {
                string line = raw.Trim();
                int eq = line.IndexOf('=');
                if (eq <= 0) continue;
                string key = line.Substring(0, eq).Trim();
                string val = line.Substring(eq + 1).Trim();
                if (key.StartsWith("factory:", StringComparison.OrdinalIgnoreCase)) req.FactoryParameters[key.Substring(8).Trim()] = val;
                else req.Parameters[key] = val;
            }

            if (string.IsNullOrWhiteSpace(req.NidProc))
            {
                if (MessageBox.Show(this, "NidProc خالی است. برای Solh نیاز است. ادامه می‌دهید؟", "RuleTrace", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }

            _tabs.SelectedTab = _tabLog;
            RunBackground("اجرای فرمول " + req.Formula + " ...", eng =>
            {
                Log("");
                Log("══════════ " + DateTime.Now.ToString("HH:mm:ss") + " ══════════");
                int code = eng.Run(req);
                Log("Exit code    : " + code + (code == 0 ? " (OK)" : code == 1 ? " (Stop error in BizErrors)" : code == 4 ? " (compile errors)" : ""));

                if (code == 0 || code == 1)
                {
                    int nid;
                    if (FormulaEngine.FormulaMap.TryGetValue(req.Formula, out nid) || int.TryParse(req.Formula, out nid))
                    {
                        if (_sourcesNid != nid || !_debug.HasSources)
                        {
                            try { LoadSourcesInto(eng, nid); }
                            catch (Exception ex) { Log("WARN         : source load failed: " + ex.Message); }
                        }
                    }
                    var trace = eng.LastTrace.ToList();
                    var parms = new Dictionary<string, string>(eng.LastParams, StringComparer.OrdinalIgnoreCase);
                    Ui(() =>
                    {
                        _debug.SetTrace(trace, parms, req.Watch);
                        if (trace.Count > 0) { _tabs.SelectedTab = _tabDebug; ToggleSettings(false); }
                    });
                    Log(trace.Count > 0
                        ? "Debug        : " + trace.Count + " رویداد در تب «دیباگ مرحله‌ای» — F10 برای رویداد بعدی"
                        : "Debug        : فرمول هیچ AddError ثبت نکرد؛ چیزی برای مرحله‌ای رفتن نیست");
                }
            }, loadDlls: true);
        }

        private void SaveLog()
        {
            using (var dlg = new SaveFileDialog { Filter = "Text|*.txt", FileName = "RuleTrace_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt" })
            {
                if (dlg.ShowDialog(this) == DialogResult.OK) File.WriteAllText(dlg.FileName, _txtLog.Text, Encoding.UTF8);
            }
        }

        // ───────────────────────────── infrastructure ─────────────────────────────

        private void RunBackground(string status, Action<FormulaEngine> work, bool loadDlls)
        {
            if (Interlocked.CompareExchange(ref _running, 1, 0) != 0) { Log("یک عملیات در حال اجراست..."); return; }
            SaveToSettings();
            SetBusy(true, status);

            Task.Run(() =>
            {
                try
                {
                    var eng = new FormulaEngine(_settings, Log);
                    if (loadDlls)
                    {
                        eng.LoadAssemblies();
                        eng.ApplyConnections();
                    }
                    work(eng);
                }
                catch (Exception ex)
                {
                    Log("FATAL: " + ex.GetType().Name + ": " + ex.Message);
                    if (ex.InnerException != null) Log("  inner: " + ex.InnerException.Message);
                    Log(ex.StackTrace);
                }
                finally
                {
                    Interlocked.Exchange(ref _running, 0);
                    Ui(() =>
                    {
                        SetBusy(false, "آماده");
                        if (!string.IsNullOrWhiteSpace(_settings.CityGuid) && _txtCityGuid.Text.Trim().Length == 0) _txtCityGuid.Text = _settings.CityGuid;
                    });
                }
            });
        }

        private void SetBusy(bool busy, string status)
        {
            _status.Text = status;
            _progress.Visible = busy;
            foreach (var b in new[] { _btnRun, _btnTestDb, _btnAnalyze, _btnInspect, _btnLookup }) b.Enabled = !busy;
            UseWaitCursor = busy;
        }

        private void Ui(Action a)
        {
            if (IsDisposed) return;
            if (InvokeRequired) BeginInvoke(a); else a();
        }

        public void Log(string line)
        {
            Ui(() =>
            {
                _txtLog.AppendText((line ?? string.Empty) + Environment.NewLine);
            });
        }
    }
}
