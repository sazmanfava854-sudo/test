using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RuleTrace
{
    /// <summary>Phase 1 — CRUD on dbo.Member code (RuleEngine) + DLL folder status. No compile.</summary>
    internal sealed class CodeEditorPanel : UserControl
    {
        private readonly UserSettings _settings;
        private readonly Action<string> _log;

        private ComboBox _cboFormula;
        private ListView _lvMembers;
        private CheckBox _chkAllVersions;
        private RichTextBox _editor;
        private Label _lblStatus;
        private Button _btnLoad, _btnSave, _btnRevert, _btnReloadDll;
        private MemberRow _current;
        private string _loadedText;
        private bool _dllOk;

        public CodeEditorPanel(UserSettings settings, Action<string> log)
        {
            _settings = settings;
            _log = log;
            Dock = DockStyle.Fill;
            BuildUi();
        }

        private void BuildUi()
        {
            var top = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(4) };
            _cboFormula = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120, Left = 4, Top = 8 };
            foreach (var kv in FormulaEngine.FormulaMap) _cboFormula.Items.Add(kv.Key);
            if (_cboFormula.Items.Count > 0) _cboFormula.SelectedIndex = 0;

            _chkAllVersions = new CheckBox { Text = "همه نسخه‌ها", AutoSize = true, Left = 140, Top = 10, Checked = true };
            _btnLoad = Btn("بارگذاری از DB", (s, e) => LoadMembers());
            _btnSave = Btn("ذخیره در DB", (s, e) => SaveCurrent());
            _btnSave.Enabled = false;
            _btnRevert = Btn("بازگردانی", (s, e) => Revert());
            _btnRevert.Enabled = false;
            _btnReloadDll = Btn("بارگذاری DLLها", (s, e) => LoadDlls());
            _btnLoad.Left = 260; _btnSave.Left = 380; _btnRevert.Left = 490; _btnReloadDll.Left = 590;
            foreach (Control c in new Control[] { _btnLoad, _btnSave, _btnRevert, _btnReloadDll }) c.Top = 6;

            _lblStatus = new Label { AutoSize = false, Left = 720, Top = 10, Width = 400, Height = 24, ForeColor = Color.DimGray, Text = "مرحله ۱: ویرایش کد Member — بدون کامپایل" };
            top.Controls.AddRange(new Control[] { _cboFormula, _chkAllVersions, _btnLoad, _btnSave, _btnRevert, _btnReloadDll, _lblStatus });

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 320 };
            _lvMembers = new ListView
            {
                Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false,
                Font = new Font("Segoe UI", 9F),
            };
            _lvMembers.Columns.Add("Member", 70);
            _lvMembers.Columns.Add("Ver", 40);
            _lvMembers.Columns.Add("Act", 36);
            _lvMembers.Columns.Add("Name", 120);
            _lvMembers.Columns.Add("KB", 44);
            _lvMembers.SelectedIndexChanged += (s, e) => SelectMember();
            split.Panel1.Controls.Add(_lvMembers);

            _editor = new RichTextBox
            {
                Dock = DockStyle.Fill, Font = new Font("Consolas", 10F), WordWrap = false,
                AcceptsTab = true, ScrollBars = RichTextBoxScrollBars.Both,
            };
            _editor.TextChanged += (s, e) => UpdateDirty();
            split.Panel2.Controls.Add(_editor);

            Controls.Add(split);
            Controls.Add(top);
        }

        private static Button Btn(string text, EventHandler click)
        {
            var b = new Button { Text = text, AutoSize = true, Height = 28 };
            b.Click += click;
            return b;
        }

        public void RefreshSettings(UserSettings settings)
        {
            if (settings == null) return;
            _settings.DllPath = settings.DllPath;
            _settings.RuleEngine = settings.RuleEngine;
            _settings.Sara = settings.Sara;
            _settings.CityGuid = settings.CityGuid;
            _settings.CachePath = settings.CachePath;
            _settings.LastFormula = settings.LastFormula;
            _lblStatus.Text = "مرحله ۱: ویرایش کد Member — DLL: " + (_dllOk ? _settings.DllPath : "not loaded");
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
            LoadDllsQuiet();
            _lblStatus.Text = "مرحله ۱: ویرایش کد Member — DLL: " + (_dllOk ? "OK" : "not loaded") + " — «بارگذاری از DB» را بزنید";
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
                _lvMembers.Items.Clear();
                foreach (MemberRow row in rows)
                {
                    var item = new ListViewItem(row.NidMember.ToString()) { Tag = row };
                    item.SubItems.Add(row.Version.ToString());
                    item.SubItems.Add(row.IsActive ? "1" : "0");
                    item.SubItems.Add(row.Name ?? "");
                    item.SubItems.Add((row.Code == null ? 0 : row.Code.Length / 1024).ToString());
                    if (row.NidMember == ChidmanAnalyzer.DefaultChidmanMemberId) item.BackColor = Color.LightYellow;
                    _lvMembers.Items.Add(item);
                }
                _log("CRUD         : " + rows.Count + " Member row(s) NidClass=" + nid);
                _lblStatus.Text = rows.Count + " row(s) — DLL: " + (_dllOk ? _settings.DllPath : "not loaded");
                if (_lvMembers.Items.Count > 0)
                {
                    foreach (ListViewItem it in _lvMembers.Items)
                    {
                        var r = (MemberRow)it.Tag;
                        if (r.NidMember == ChidmanAnalyzer.DefaultChidmanMemberId) { it.Selected = true; break; }
                    }
                    if (_lvMembers.SelectedItems.Count == 0) _lvMembers.Items[0].Selected = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "بارگذاری Member", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _log("CRUD ERROR   : " + ex.Message);
            }
        }

        private void SelectMember()
        {
            if (_lvMembers.SelectedItems.Count == 0) return;
            _current = (MemberRow)_lvMembers.SelectedItems[0].Tag;
            _loadedText = _current.Code ?? "";
            _editor.Text = _loadedText;
            _lblStatus.Text = "Member " + _current.NidMember + " v" + _current.Version + " " + _current.Name + " — " + (_loadedText.Length / 1024) + " KB";
        }

        private void UpdateDirty()
        {
            if (_current == null) return;
            bool dirty = !string.Equals(_editor.Text, _loadedText, StringComparison.Ordinal);
            _btnSave.Enabled = dirty;
            _btnRevert.Enabled = dirty;
        }

        private void Revert()
        {
            if (_current == null) return;
            _editor.Text = _loadedText;
        }

        private void SaveCurrent()
        {
            if (_current == null) return;
            if (!ConfirmSave()) return;

            try
            {
                MemberRepository.UpdateCode(_settings.RuleEngine, _current.NidClass, _current.NidMember, _current.Version, _editor.Text);
                _loadedText = _editor.Text;
                _current.Code = _editor.Text;
                _log("CRUD SAVE    : Member " + _current.NidMember + " v" + _current.Version + " (" + (_editor.Text.Length / 1024) + " KB) → RuleEngine.dbo.Member");
                MessageBox.Show(this, "ذخیره شد.\nMember " + _current.NidMember + " Version " + _current.Version, "RuleTrace", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateDirty();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "ذخیره", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _log("CRUD ERROR   : " + ex.Message);
            }
        }

        private bool ConfirmSave()
        {
            return MessageBox.Show(this,
                "کد Member " + _current.NidMember + " (v" + _current.Version + ") در RuleEngine به‌روز می‌شود.\nادامه؟",
                "ذخیره در DB", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private void LoadDlls()
        {
            try
            {
                var eng = new FormulaEngine(_settings, _log);
                eng.LoadAssemblies();
                _dllOk = true;
                _lblStatus.Text = "DLL OK: " + _settings.DllPath;
                _log("DLLs         : loaded from " + _settings.DllPath);
            }
            catch (Exception ex)
            {
                _dllOk = false;
                MessageBox.Show(this, ex.Message, "DLL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadDllsQuiet()
        {
            try
            {
                if (!FormulaEngine.IsDllFolder(_settings.DllPath)) return;
                var eng = new FormulaEngine(_settings, _log);
                eng.LoadAssemblies();
                _dllOk = true;
            }
            catch { _dllOk = false; }
        }
    }
}
