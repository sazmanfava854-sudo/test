using System.Reflection;

namespace RuleTrace
{
    internal static class BuildInfo
    {
        public const string Label = "v20b-phase2-setmyinfo";

        public static string Banner
        {
            get
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                return "RuleTrace " + Label + " (v" + v + ") — مرحله ۲: اجرا با موتور Sara / Cache DLL";
            }
        }
    }
}
