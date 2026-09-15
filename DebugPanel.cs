using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RuleTrace
{
    /// <summary>
    /// Step debugger over the formula trace: every AddError / BizErrors entry the formula produced is one "step".
    /// F10 = next step, Shift+F10 = previous, F5 = run to end, Ctrl+Home = first.
    /// The matching line of the VB source (dbo.Member.XmlBody) is highlighted for each step, and the
    /// ParametersValue of the stepped variable is shown, so the run can be replayed line by line after execution.
    /// </summary>
    internal sealed class DebugPanel : UserControl
    {
        private sealed class Loc
        {
            public MemberSource Source;
            public int Line;
            public bool IsAddError;
        }

        private ToolStripLabel _lblPos;
        private ToolStripComboBox _cboMember;
        private ToolStripButton _btnFirst, _btnPrev, _btnNext, _btnLast, _btnLoad, _btnRun;
        private ListView _lvTrace;
        private RichTextBox _rtb;
        private TextBox _txtDetail;

        private List<TraceEvent> _trace = new List<TraceEvent>();
        private Dictionary<string, string> _params = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<MemberSource> _sources = new List<MemberSource>();
        private readonly Dictionary<string, List<Loc>> _locCache = new Dictionary<string, List<Loc>>(StringComparer.OrdinalIgnoreCase);
        private string _watch = string.Empty;

        private int _pos = -1;
        private MemberSource _shown;
        private int _hlStart = -1, _hlLen;
        private bool _suppressCombo;

        public event Action LoadSourcesRequested;
        public event Action RunRequested;

        public bool HasTrace { get { return _trace.Count > 0; } }
        public bool HasSources { get { return _sources.Count > 0; } }

        public DebugPanel()
        {
            Dock = DockStyle.Fill;
            BuildUi();
            UpdateBar();
        }

        private void BuildUi()
        {
            var bar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, RenderMode = ToolStripRenderMode.System, Dock = DockStyle.Top };
            _btnFirst = new ToolStripButton("⏮ اول") { ToolTipText = "Ctrl+Home" };
            _btnPrev = new ToolStripButton("◀ قبلی") { ToolTipText = "Shift+F10" };
            _btnNext = new ToolStripButton("▶ بعدی  F10") { ToolTipText = "F10 / F11 — رفتن به رویداد بعدی" };
            _btnLast = new ToolStripButton("⏭ آخر  F5") { ToolTipText = "F5 — تا انتها" };
            _btnFirst.Click += (s, e) => StepFirst();
            _btnPrev.Click += (s, e) => StepPrev();
            _btnNext.Click += (s, e) => StepNext();
            _btnLast.Click += (s, e) => StepLast();
            _lblPos = new ToolStripLabel("—");

            _cboMember = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = false, Width = 340, ToolTipText = "ردیف Member (بخش کد فرمول)" };
            _cboMember.SelectedIndexChanged += (s, e) =>
            {
                if (_suppressCombo) return;
                var m = _cboMember.SelectedItem as MemberSource;
                if (m != null) ShowSource(m);
            };
            _btnLoad = new ToolStripButton("بارگذاری کد از DB");
            _btnLoad.Click += (s, e) => { if (LoadSourcesRequested != null) LoadSourcesRequested(); };
            _btnRun = new ToolStripButton("▶ اجرا") { ToolTipText = "اجرای دوباره فرمول" };
            _btnRun.Click += (s, e) => { if (RunRequested != null) RunRequested(); };

            bar.Items.AddRange(new ToolStripItem[]
            {
                _btnFirst, _btnPrev, _btnNext, _btnLast, new ToolStripSeparator(), _lblPos, new ToolStripSeparator(),
                new ToolStripLabel("کد فرمول:"), _cboMember, _btnLoad, new ToolStripSeparator(), _btnRun,
            });

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, FixedPanel = FixedPanel.Panel1 };
            bool splitInit = false;
            split.Layout += (s, e) =>
            {
                if (splitInit || split.Width < 700) return;
                splitInit = true;
                split.SplitterDistance = 430;
            };

            _lvTrace = new ListView
            {
                Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false, MultiSelect = false,
                Font = new Font("Segoe UI", 9F),
            };
            _lvTrace.Columns.Add("#", 44);
            _lvTrace.Columns.Add("Action", 62);
            _lvTrace.Columns.Add("Key", 150);
            _lvTrace.Columns.Add("Title", 320);
            _lvTrace.ItemActivate += (s, e) => { if (_lvTrace.SelectedIndices.Count > 0) GoTo(_lvTrace.SelectedIndices[0]); };
            _lvTrace.SelectedIndexChanged += (s, e) =>
            {
                if (_lvTrace.SelectedIndices.Count > 0 && _lvTrace.SelectedIndices[0] != _pos) GoTo(_lvTrace.SelectedIndices[0]);
            };
            split.Panel1.Controls.Add(_lvTrace);

            _rtb = new RichTextBox
            {
                Dock = DockStyle.Fill, ReadOnly = true, WordWrap = false, DetectUrls = false, HideSelection = false,
                Font = new Font("Consolas", 9.5F), BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.Gainsboro,
                ScrollBars = RichTextBoxScrollBars.Both, RightToLeft = RightToLeft.No,
            };
            _rtb.Text = "کد فرمول هنوز بارگذاری نشده — «بارگذاری کد از DB» یا اجرای فرمول.";
            _txtDetail = new TextBox
            {
                Dock = DockStyle.Bottom, Multiline = true, ReadOnly = true, Height = 88, ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9.5F), BackColor = Color.FromArgb(45, 45, 48), ForeColor = Color.White,
            };
            split.Panel2.Controls.Add(_rtb);
            split.Panel2.Controls.Add(_txtDetail);

            Controls.Add(split);
            Controls.Add(bar);
        }

        // ───────────────────────────── data in ─────────────────────────────

        public void SetTrace(IList<TraceEvent> trace, IDictionary<string, string> parameters, string watch)
        {
            _trace = trace == null ? new List<TraceEvent>() : trace.ToList();
            _params = parameters == null
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(parameters, StringComparer.OrdinalIgnoreCase);
            _watch = watch ?? string.Empty;

            _lvTrace.BeginUpdate();
            _lvTrace.Items.Clear();
            foreach (TraceEvent ev in _trace)
            {
                var item = new ListViewItem((ev.Index + 1).ToString());
                item.SubItems.Add(ev.Action ?? "");
                item.SubItems.Add(ev.Key ?? "");
                item.SubItems.Add(ev.Title ?? "");
                if (string.Equals(ev.Action, "Stop", StringComparison.OrdinalIgnoreCase)) item.ForeColor = Color.Firebrick;
                else if (!string.IsNullOrEmpty(_watch) && string.Equals(ev.Key, _watch, StringComparison.OrdinalIgnoreCase)) item.BackColor = Color.LightGoldenrodYellow;
                _lvTrace.Items.Add(item);
            }
            _lvTrace.EndUpdate();

            _pos = -1;
            ClearHighlight();
            _txtDetail.Text = _trace.Count == 0
                ? "هیچ رویدادی (AddError) ثبت نشد."
                : _trace.Count + " رویداد. F10 = رویداد بعدی، Shift+F10 = قبلی، F5 = تا انتها.";
            UpdateBar();
            if (_trace.Count > 0) GoTo(0);
        }

        public void SetSources(IList<MemberSource> sources)
        {
            _sources.Clear();
            _locCache.Clear();
            if (sources != null) _sources.AddRange(sources);

            _suppressCombo = true;
            _cboMember.Items.Clear();
            foreach (MemberSource m in _sources) _cboMember.Items.Add(m);
            _suppressCombo = false;

            _shown = null;
            if (_sources.Count > 0)
            {
                MemberSource prefer = _sources.FirstOrDefault(m => m.NidMember == ChidmanAnalyzer.DefaultChidmanMemberId)
                    ?? _sources.OrderByDescending(m => m.Code.Length).First();
                ShowSource(prefer);
            }
            UpdateBar();
            if (_pos >= 0) GoTo(_pos);
        }

        // ───────────────────────────── stepping ─────────────────────────────

        public void StepFirst() { if (HasTrace) GoTo(0); }
        public void StepLast() { if (HasTrace) GoTo(_trace.Count - 1); }
        public void StepNext() { if (HasTrace) GoTo(Math.Min(_pos + 1, _trace.Count - 1)); }
        public void StepPrev() { if (HasTrace) GoTo(Math.Max(_pos - 1, 0)); }

        public void FocusMember(int nidMember)
        {
            MemberSource m = _sources.FirstOrDefault(s => s.NidMember == nidMember);
            if (m != null) ShowSource(m);
        }

        public void JumpToFirstChidmanTrace()
        {
            if (!HasTrace) return;
            for (int i = 0; i < _trace.Count; i++)
            {
                TraceEvent e = _trace[i];
                string k = (e.Key ?? "") + " " + (e.Title ?? "");
                if (k.IndexOf("chidman", StringComparison.OrdinalIgnoreCase) >= 0
                    || k.IndexOf("chandganeh", StringComparison.OrdinalIgnoreCase) >= 0
                    || k.IndexOf("چیدمان", StringComparison.OrdinalIgnoreCase) >= 0
                    || k.IndexOf("InsertChidman", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    GoTo(i);
                    return;
                }
            }
        }

        private void GoTo(int i)
        {
            if (i < 0 || i >= _trace.Count) return;
            _pos = i;
            TraceEvent ev = _trace[i];

            if (_lvTrace.Items.Count > i)
            {
                // _pos is already i, so the SelectedIndexChanged handler does not re-enter GoTo
                _lvTrace.SelectedIndices.Clear();
                _lvTrace.Items[i].Selected = true;
                _lvTrace.Items[i].Focused = true;
                _lvTrace.EnsureVisible(i);
            }

            var sb = new StringBuilder();
            sb.Append("#").Append(i + 1).Append('/').Append(_trace.Count).Append("  [").Append(ev.Action).Append("]  ").Append(ev.Key).AppendLine();
            sb.Append("Title : ").Append(ev.Title).AppendLine();
            string v;
            if (!string.IsNullOrEmpty(ev.Key) && _params.TryGetValue(ev.Key, out v)) sb.Append("Value : ").Append(ev.Key).Append(" = ").Append(v).AppendLine();
            if (!string.IsNullOrEmpty(_watch) && _params.TryGetValue(_watch, out v)) sb.Append("Watch : ").Append(_watch).Append(" = ").Append(v).AppendLine();

            Loc loc = Locate(i);
            if (loc == null)
            {
                sb.Append(_sources.Count == 0 ? "Source: (کد فرمول بارگذاری نشده)" : "Source: (خطی با این Key در کد پیدا نشد)");
                ClearHighlight();
            }
            else
            {
                sb.Append("Source: Member ").Append(loc.Source.NidMember).Append(' ').Append(loc.Source.Name).Append("  line ").Append(loc.Line + 1);
                if (_shown != loc.Source) ShowSource(loc.Source);
                Highlight(loc.Line);
            }
            _txtDetail.Text = sb.ToString();
            UpdateBar();
        }

        /// <summary>
        /// Finds the source line of step i. Lines containing AddError win over plain mentions; when the same key is
        /// raised several times (loops) the n-th event maps to the n-th matching line.
        /// </summary>
        private Loc Locate(int i)
        {
            string key = _trace[i].Key;
            if (string.IsNullOrWhiteSpace(key) || _sources.Count == 0) return null;

            List<Loc> locs;
            if (!_locCache.TryGetValue(key, out locs))
            {
                locs = new List<Loc>();
                var rx = new Regex(@"(?<![\w])" + Regex.Escape(key) + @"(?![\w])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                foreach (MemberSource m in _sources)
                {
                    string[] lines = Lines(m);
                    for (int l = 0; l < lines.Length; l++)
                    {
                        if (lines[l].IndexOf(key, StringComparison.OrdinalIgnoreCase) < 0 || !rx.IsMatch(lines[l])) continue;
                        locs.Add(new Loc { Source = m, Line = l, IsAddError = lines[l].IndexOf("AddError", StringComparison.OrdinalIgnoreCase) >= 0 });
                    }
                }
                if (locs.Any(x => x.IsAddError)) locs = locs.Where(x => x.IsAddError).ToList();
                _locCache[key] = locs;
            }
            if (locs.Count == 0) return null;

            int occurrence = 0;
            for (int k = 0; k < i; k++) if (string.Equals(_trace[k].Key, key, StringComparison.OrdinalIgnoreCase)) occurrence++;
            return locs[occurrence % locs.Count];
        }

        // ───────────────────────────── source view ─────────────────────────────

        private readonly Dictionary<MemberSource, string[]> _lineCache = new Dictionary<MemberSource, string[]>();

        private string[] Lines(MemberSource m)
        {
            string[] lines;
            if (!_lineCache.TryGetValue(m, out lines))
            {
                lines = (m.Code ?? string.Empty).Replace("\r\n", "\n").Split('\n');
                _lineCache[m] = lines;
            }
            return lines;
        }

        private void ShowSource(MemberSource m)
        {
            if (m == null) return;
            _shown = m;
            _hlStart = -1;
            _rtb.SuspendLayout();
            _rtb.Text = string.Join("\n", Lines(m));
            _rtb.ResumeLayout();

            _suppressCombo = true;
            try { _cboMember.SelectedItem = m; } finally { _suppressCombo = false; }
        }

        private void Highlight(int line)
        {
            ClearHighlight();
            int start = _rtb.GetFirstCharIndexFromLine(line);
            if (start < 0) return;
            string[] lines = Lines(_shown);
            int len = line < lines.Length ? lines[line].Length : 0;

            _rtb.Select(start, len);
            _rtb.SelectionBackColor = Color.FromArgb(120, 100, 0);
            _rtb.SelectionColor = Color.White;
            _hlStart = start; _hlLen = len;

            // keep a few lines of context above the current line
            int ctx = _rtb.GetFirstCharIndexFromLine(Math.Max(0, line - 6));
            _rtb.Select(ctx < 0 ? start : ctx, 0);
            _rtb.ScrollToCaret();
            _rtb.Select(start, 0);
            _rtb.ScrollToCaret();
        }

        private void ClearHighlight()
        {
            if (_hlStart < 0 || _shown == null) return;
            try
            {
                _rtb.Select(_hlStart, _hlLen);
                _rtb.SelectionBackColor = _rtb.BackColor;
                _rtb.SelectionColor = _rtb.ForeColor;
                _rtb.Select(_hlStart, 0);
            }
            catch { }
            _hlStart = -1;
        }

        private void UpdateBar()
        {
            bool has = HasTrace;
            _btnFirst.Enabled = _btnPrev.Enabled = has && _pos > 0;
            _btnNext.Enabled = _btnLast.Enabled = has && _pos < _trace.Count - 1;
            _lblPos.Text = has ? "رویداد " + (_pos + 1) + " / " + _trace.Count : "بدون رویداد — فرمول را اجرا کنید";
        }
    }
}
