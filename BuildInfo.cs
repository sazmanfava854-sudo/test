using System.Reflection;

namespace RuleTrace
{
    internal static class BuildInfo
    {
        public const string Label = "merge-v4-vbc-refs";

        public static string Banner
        {
            get
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                return "RuleTrace " + Label + " (v" + v + ") — imports + field merge + deduped refs";
            }
        }
    }
}
