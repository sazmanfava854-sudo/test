using System.Reflection;

namespace RuleTrace
{
    internal static class BuildInfo
    {
        public const string Label = "merge-v7-out-dedupe";

        public static string Banner
        {
            get
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                return "RuleTrace " + Label + " (v" + v + ") — Out/M_Out dedupe + vbc.exe refs fix";
            }
        }
    }
}
