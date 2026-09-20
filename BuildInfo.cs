using System.Reflection;

namespace RuleTrace
{
    internal static class BuildInfo
    {
        public const string Label = "v23n-cs0136";

        public static string Banner
        {
            get
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                return "RuleTrace " + Label + " (v" + v + ") — وب محلی، بدون CMD، بدون بازنویسی VB";
            }
        }
    }
}
