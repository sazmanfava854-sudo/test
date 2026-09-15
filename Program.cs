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

            // optional prefill: RuleTrace.exe --nidproc <GUID> --formula Solh --watch Calc_Chandganeh
            string v;
            if ((v = Arg(args, "--nidproc")) != null) settings.LastNidProc = v;
            if ((v = Arg(args, "--formula")) != null) settings.LastFormula = v;
            if ((v = Arg(args, "--watch")) != null) settings.LastWatch = v;
            if ((v = Arg(args, "--dll-path")) != null) settings.DllPath = v;

            Application.ThreadException += (s, e) =>
                MessageBox.Show(e.Exception.ToString(), "RuleTrace — unhandled", MessageBoxButtons.OK, MessageBoxIcon.Error);

            Application.Run(new MainForm(settings));
        }

        private static string Arg(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) return args[i + 1];
            return null;
        }
    }
}
