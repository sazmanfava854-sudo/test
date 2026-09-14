using System.Reflection;

namespace RuleTrace
{
    internal static class BuildInfo
    {
        public const string Label = "v18e-all-member-methods";

        public static string Banner
        {
            get
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                return "RuleTrace " + Label + " (v" + v + ") — partial compile all unique methods";
            }
        }
    }
}
