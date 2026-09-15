using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace RuleTrace
{
    /// <summary>Tiny host window so the process stays alive without CMD. The real UI is the browser.</summary>
    internal sealed class HostForm : Form
    {
        public HostForm(string url)
        {
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Text = BuildInfo.Banner;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = true;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(460, 220);
            BackColor = Color.FromArgb(15, 23, 42);
            ForeColor = Color.FromArgb(241, 245, 249);
            Font = new Font("Segoe UI", 10F);

            var title = new Label
            {
                Text = "RuleTrace روی مرورگر اجرا می‌شود",
                Dock = DockStyle.Top,
                Height = 42,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 212, 191),
            };
            var hint = new Label
            {
                Text = "پنجره CMD لازم نیست. اگر مرورگر باز نشد، این آدرس را کپی کنید:\n" + url + "\n\nبرای خروج این پنجره را ببندید.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(16),
            };
            var bar = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(12, 8, 12, 8),
                BackColor = Color.FromArgb(30, 41, 59),
            };
            var open = new Button
            {
                Text = "باز کردن مرورگر",
                AutoSize = true,
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(16, 6, 16, 6),
            };
            open.FlatAppearance.BorderSize = 0;
            open.Click += (s, e) => OpenBrowser(url);
            var copy = new Button
            {
                Text = "کپی آدرس",
                AutoSize = true,
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(16, 6, 16, 6),
            };
            copy.FlatAppearance.BorderSize = 0;
            copy.Click += (s, e) =>
            {
                try { Clipboard.SetText(url); } catch { }
            };
            bar.Controls.Add(open);
            bar.Controls.Add(copy);
            Controls.Add(hint);
            Controls.Add(bar);
            Controls.Add(title);
        }

        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true,
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("مرورگر باز نشد. این آدرس را در مرورگر پیست کنید:\n" + url + "\n\n" + ex.Message,
                    "RuleTrace", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
