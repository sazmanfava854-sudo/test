using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RuleTrace
{
    /// <summary>Phase 1 — inspect RuleEngine dbo.Member VB next to Sara DLL types. Read-only. No DB write, no compile.</summary>
    internal sealed class CodeEditorPanel : UserControl
    {
        private readonly UserSettings _settings;
        private readonly Action<string> _log;

        private ComboBox _cboFormula;
        private ListView _lvMembers;
        private ListView _lvDll;
        private CheckBox _chkAllVersions;
        private RichTextBox _editor;
        private TextBox _txtDllDetail;
        private Label _lblStatus;
        private Button _btnCombine, _btnLoad, _btnReloadDll;
        private MemberRow _current;
        private bool _dllOk;
        private bool _selecting;
        private FormulaEngine _engine;

        public CodeEditorPanel(UserSettings settings, Action<string> log)
        {
            _settings = settings;
            _log = log;
            Dock = DockStyle.Fill;
            BuildUi();
        }

        private void BuildUi()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var banner = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                Padding = new Padding(8, 6, 8, 6),
                BackColor = Color.FromArgb(220, 235, 252),
                ForeColor = Color.FromArgb(20, 50, 90),
                Text = "عیب‌یابی — فقط خواندن. کد RuleEngine با DLL ترکیب می‌شود؛ چیزی در دیتابیس ذخیره یا حذف نمی‌شود.",
            };

            var tools = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true,
                Padding = new Padding(4),
            };
            _cboFormula = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130, Margin = new Padding(3, 6, 3, 3) };
            foreach (var kv in FormulaEngine.FormulaMap) _cboFormula.Items.Add(kv.Key);
            if (_cboFormula.Items.Count > 0) _cboFormula.SelectedIndex = 0;
            _cboFormula.SelectedIndexChanged += (s, e) => { _lvMembers.Items.Clear(); _current = null; _editor.Clear(); };

            _chkAllVersions = new CheckBox { Text = "همه نسخه‌ها", AutoSize = true, Checked = true, Margin = new Padding(8, 8, 3, 3) };
            _btnCombine = Btn("ترکیب DB + DLL", (s, e) => Combine());
            _btnCombine.Font = new Font(Font, FontStyle.Bold);
            _btnLoad = Btn("بارگذاری از DB", (s, e) => LoadMembers());
            _btnReloadDll = Btn("بارگذاری DLLها", (s, e) => LoadDlls());
            _lblStatus = new Label { AutoSize = true, Margin = new Padding(12, 10, 3, 3), ForeColor = Color.DimGray, Text = "هنوز ترکیب نشده" };

            tools.Controls.Add(_cboFormula);
            tools.Controls.Add(_chkAllVersions);
            tools.Controls.Add(_btnCombine);
            tools.Controls.Add(_btnLoad);
            tools.Controls.Add(_btnReloadDll);
            tools.Controls.Add(_lblStatus);

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 340 };
            var left = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 280 };

            _lvMembers = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                HideSelection = false,
                Font = new Font("Segoe UI", 9F),
            };
            _lvMembers.Columns.Add("Member", 70);
            _lvMembers.Columns.Add("Ver", 40);
            _lvMembers.Columns.Add("Act", 36);
            _lvMembers.Columns.Add("Name", 120);
            _lvMembers.Columns.Add("KB", 44);
            _lvMembers.SelectedIndexChanged += (s, e) => SelectMember();
            var grpMembers = new GroupBox { Text = "کد از RuleEngine (dbo.Member) — فقط خواندن", Dock = DockStyle.Fill, Padding = new Padding(6) };
            grpMembers.Controls.Add(_lvMembers);
            left.Panel1.Controls.Add(grpMembers);

            _lvDll = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                HideSelection = false,
                Font = new Font("Segoe UI", 9F),
            };
            _lvDll.Columns.Add("Type", 160);
            _lvDll.Columns.Add("Kind", 60);
            _lvDll.Columns.Add("Assembly", 90);
            _lvDll.Columns.Add("#", 40);
            _lvDll.SelectedIndexChanged += (s, e) => ShowDllType();
            _txtDllDetail = new TextBox
            {
                Dock = DockStyle.Bottom,
                Height = 110,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 8.5F),
                Text = "یک نوع DLL را انتخاب کنید (مثلاً ClsOut) — فقط خواندنی است.",
            };
            var grpDll = new GroupBox { Text = "نوع‌های DLL (خواندنی)", Dock = DockStyle.Fill, Padding = new Padding(6) };
            grpDll.Controls.Add(_lvDll);
            grpDll.Controls.Add(_txtDllDetail);
            left.Panel2.Controls.Add(grpDll);

            split.Panel1.Controls.Add(left);

            _editor = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10F),
                WordWrap = false,
                ReadOnly = true,
                DetectUrls = false,
                ScrollBars = RichTextBoxScrollBars.Both,
                BackColor = Color.FromArgb(248, 248, 248),
            };
            var grpCode = new GroupBox { Text = "کد VB Member — فقط مشاهده (ذخیره در DB ندارد)", Dock = DockStyle.Fill, Padding = new Padding(6) };
            grpCode.Controls.Add(_editor);
            split.Panel2.Controls.Add(grpCode);

            root.Controls.Add(banner, 0, 0);
            root.Controls.Add(tools, 0, 1);
            root.Controls.Add(split, 0, 2);
            Controls.Add(root);
        }

        private static Button Btn(string text, EventHandler click)
        {
            var b = new Button { Text = text, AutoSize = true, Height = 28, Margin = new Padding(3) };
            b.Click += click;
            return b;
        }

        public void RefreshSettings(UserSettings settings)
        {
            if (settings == null) return;
            _lblStatus.Text = "DLL: " + (_dllOk ? _settings.DllPath : "not loaded");
        }

        public void SyncFormula(string formula)
        {
            if (string.IsNullOrWhiteSpace(formula)) return;
            for (int i = 0; i < _cboFormula.Items.Count; i++)
                if (string.Equals(_cboFormula.Items[i].ToString(), formula, StringComparison.OrdinalIgnoreCase))
                { _cboFormula.SelectedIndex = i; return; }
        }

        public void OnShown()
        {
            SyncFormula(_settings.LastFormula);
            _lblStatus.Text = "«ترکیب DB + DLL» را بزنید — فقط خواندن، بدون ذخیره";
        }

        /// <summary>Load Sara DLLs + dbo.Member rows into one inspect workspace. Does not compile or write.</summary>
        public void Combine()
        {
            LoadDlls();
            LoadDllCatalog();
            LoadMembers();
            int members = _lvMembers.Items.Count;
            int types = _lvDll.Items.Count;
            _log("INSPECT      : " + members + " Member row(s) from RuleEngine + " + types + " public type(s) from DLLs — read-only, no save, no compile");
            _lblStatus.Text = "ترکیب شد (فقط خواندن): " + members + " Member + " + types + " نوع DLL";
        }

        private int FormulaId()
        {
            string f = _cboFormula.Text;
            int nid;
            if (FormulaEngine.FormulaMap.TryGetValue(f, out nid)) return nid;
            int.TryParse(f, out nid);
            return nid;
        }

        public void LoadMembers()
        {
            int nid = FormulaId();
            if (nid == 0) { MessageBox.Show(this, "فرمول را انتخاب کنید.", "RuleTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(_settings.RuleEngine)) { MessageBox.Show(this, "RuleEngine connection خالی است.", "RuleTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            try
            {
                var rows = MemberRepository.List(_settings.RuleEngine, nid, _chkAllVersions.Checked);
                _selecting = true;
                _lvMembers.Items.Clear();
                _current = null;
                foreach (MemberRow row in rows)
                {
                    var item = new ListViewItem(row.NidMember.ToString()) { Tag = row };
                    item.SubItems.Add(row.Version.ToString());
                    item.SubItems.Add(row.IsActive ? "1" : "0");
                    item.SubItems.Add(row.Name ?? "");
                    item.SubItems.Add((row.Code == null ? 0 : row.Code.Length / 1024).ToString());
                    if (row.NidMember == ChidmanAnalyzer.DefaultChidmanMemberId) item.BackColor = Color.LightYellow;
                    if (!row.IsActive) item.ForeColor = Color.Gray;
                    _lvMembers.Items.Add(item);
                }
                _selecting = false;
                _log("INSPECT      : " + rows.Count + " Member row(s) NidClass=" + nid + " (read-only)");
                _lblStatus.Text = rows.Count + " row(s) — DLL: " + (_dllOk ? "OK" : "not loaded");
                if (_lvMembers.Items.Count > 0)
                {
                    ListViewItem pick = null;
                    foreach (ListViewItem it in _lvMembers.Items)
                    {
                        var r = (MemberRow)it.Tag;
                        if (r.NidMember == ChidmanAnalyzer.DefaultChidmanMemberId && r.IsActive) { pick = it; break; }
                        if (r.NidMember == ChidmanAnalyzer.DefaultChidmanMemberId && pick == null) pick = it;
                    }
                    if (pick == null) pick = _lvMembers.Items[0];
                    pick.Selected = true;
                    pick.EnsureVisible();
                }
            }
            catch (Exception ex)
            {
                _selecting = false;
                MessageBox.Show(this, ex.Message, "بارگذاری Member", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _log("INSPECT ERROR: " + ex.Message);
            }
        }

        private void SelectMember()
        {
            if (_selecting) return;
            if (_lvMembers.SelectedItems.Count == 0) return;
            _current = (MemberRow)_lvMembers.SelectedItems[0].Tag;
            string code = _current.Code ?? "";
            _editor.Text = code;
            _lblStatus.Text = "Member " + _current.NidMember + " v" + _current.Version + " " + _current.Name + " — " + (code.Length / 1024) + " KB (read-only)";
        }

        private void LoadDlls()
        {
            try
            {
                _engine = new FormulaEngine(_settings, _log);
                _engine.LoadAssemblies();
                _dllOk = true;
                _lblStatus.Text = "DLL OK: " + _settings.DllPath;
                LoadDllCatalog();
            }
            catch (Exception ex)
            {
                _dllOk = false;
                MessageBox.Show(this, ex.Message, "DLL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _log("DLL ERROR    : " + ex.Message);
            }
        }

        private void LoadDllCatalog()
        {
            if (_engine == null)
            {
                try
                {
                    _engine = new FormulaEngine(_settings, _log);
                    _engine.LoadAssemblies();
                    _dllOk = true;
                }
                catch (Exception ex)
                {
                    _log("DLL ERROR    : " + ex.Message);
                    return;
                }
            }
            try
            {
                var types = _engine.CatalogDllTypes();
                _lvDll.Items.Clear();
                foreach (DllTypeRow t in types)
                {
                    var item = new ListViewItem(t.Name) { Tag = t };
                    item.SubItems.Add(t.Kind);
                    item.SubItems.Add(t.Assembly);
                    item.SubItems.Add(t.PublicMembers.ToString());
                    if (t.Name.IndexOf("Out", StringComparison.OrdinalIgnoreCase) >= 0
                        || t.Name.Equals("clsOut", StringComparison.OrdinalIgnoreCase)
                        || t.Name.Equals("ClsOut", StringComparison.OrdinalIgnoreCase))
                        item.BackColor = Color.Honeydew;
                    _lvDll.Items.Add(item);
                }
                _log("DLL catalog  : " + types.Count + " public type(s)");
                DllTypeRow clsOut = types.FirstOrDefault(t =>
                    t.Name.Equals("clsOut", StringComparison.OrdinalIgnoreCase)
                    || t.Name.Equals("ClsOut", StringComparison.OrdinalIgnoreCase));
                if (clsOut != null)
                    _log("DLL clsOut   : " + clsOut.FullName + " in " + clsOut.Assembly);
                else
                    _log("DLL clsOut   : not found as a public type (formula code still references it)");
            }
            catch (Exception ex)
            {
                _log("DLL catalog  : " + ex.Message);
            }
        }

        private void ShowDllType()
        {
            if (_lvDll.SelectedItems.Count == 0 || _engine == null) return;
            var row = (DllTypeRow)_lvDll.SelectedItems[0].Tag;
            try { _txtDllDetail.Text = _engine.DescribeDllType(row.FullName); }
            catch (Exception ex) { _txtDllDetail.Text = ex.Message; }
        }
    }
}
