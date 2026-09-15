using System;
using System.Linq;
using System.Windows.Forms;

namespace RuleTrace
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            if (args != null && args.Any(a => string.Equals(a, "--self-test", StringComparison.OrdinalIgnoreCase)))
            {
                Environment.ExitCode = SelfTest.Run();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            UserSettings settings;
            try
            {
                settings = UserSettings.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در خواندن تنظیمات:\n" + ex.Message, "RuleTrace", MessageBoxButtons.OK, MessageBoxIcon.Error);
                settings = new UserSettings();
            }

            string v;
            if ((v = Arg(args, "--nidproc")) != null) settings.LastNidProc = v;
            if ((v = Arg(args, "--formula")) != null) settings.LastFormula = v;
            if ((v = Arg(args, "--watch")) != null) settings.LastWatch = v;
            if ((v = Arg(args, "--dll-path")) != null) settings.DllPath = v;

            Application.ThreadException += (s, e) =>
                MessageBox.Show(e.Exception.ToString(), "RuleTrace — unhandled", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (HasFlag(args, "--desktop"))
            {
                Application.Run(new MainForm(settings));
                return;
            }

            int port = 17880;
            string portArg = Arg(args, "--port");
            int parsed;
            if (portArg != null && int.TryParse(portArg, out parsed) && parsed > 0) port = parsed;

            var app = new WebApp(settings);
            var host = new WebHost(app);
            try
            {
                host.Start(port);
            }
            catch (Exception ex)
            {
                MessageBox.Show("میزبان وب شروع نشد:\n" + ex.Message, "RuleTrace", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (HasFlag(args, "--headless"))
            {
                Console.WriteLine(BuildInfo.Banner);
                Console.WriteLine(host.Url);
                var done = new System.Threading.ManualResetEvent(false);
                host.StopRequested = () => { host.Stop(); done.Set(); };
                done.WaitOne();
                host.Stop();
                return;
            }

            var form = new HostForm(host.Url);
            host.StopRequested = () =>
            {
                host.Stop();
                try
                {
                    if (!form.IsDisposed) form.BeginInvoke(new Action(() => form.Close()));
                }
                catch { }
            };
            form.FormClosed += (s, e) => host.Stop();
            if (!HasFlag(args, "--no-browser"))
                HostForm.OpenBrowser(host.Url);
            Application.Run(form);
            host.Stop();
        }

        private static bool HasFlag(string[] args, string name)
        {
            if (args == null) return false;
            for (int i = 0; i < args.Length; i++)
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static string Arg(string[] args, string name)
        {
            if (args == null) return null;
            for (int i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) return args[i + 1];
            return null;
        }
    }
}
