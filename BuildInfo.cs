using System.Reflection;

namespace RuleTrace
{
    internal static class BuildInfo
    {
        public const string Label = "v20c-phase2-inject";

        public static string Banner
        {
            get
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                return "RuleTrace " + Label + " (v" + v + ") — مرحله ۲: تزریق XmlBody + کامپایل موتور";
            }
        }
    }
}
