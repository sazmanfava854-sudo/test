using System.Reflection;

namespace RuleTrace
{
    internal static class BuildInfo
    {
        public const string Label = "v19c-phase1-inspect";

        public static string Banner
        {
            get
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                return "RuleTrace " + Label + " (v" + v + ") — عیب‌یابی: ترکیب DB+DLL فقط خواندنی";
            }
        }
    }
}
