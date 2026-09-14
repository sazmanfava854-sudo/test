using System.Reflection;

namespace RuleTrace
{
    internal static class BuildInfo
    {
        public const string Label = "v19b-phase1-crud-workspace";

        public static string Banner
        {
            get
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                return "RuleTrace " + Label + " (v" + v + ") — مرحله ۱: ترکیب DB+DLL و CRUD (بدون کامپایل)";
            }
        }
    }
}
