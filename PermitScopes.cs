using System;
using System.Collections.Generic;

namespace RuleTrace
{
    /// <summary>
    /// User picks which forms to debug — same as opening a form in Sara.
    /// Solh / agreement / commission are optional. Named tables only.
    /// </summary>
    internal static class PermitScopes
    {
        public const string Zabeteh = "zabeteh";
        public const string Solh = "solh";
        public const string Chidman = "chidman";
        public const string Tahlil = "tahlil";
        public const string Tavafogh = "tavafogh";
        public const string Commission = "commission";
        public const string Income = "income";

        public const string MustPick =
            "کاربر باید انتخاب کند کدام بخش‌ها را دیباگ کند. مثل باز کردن فرم در سارا فقط همان کدها اجرا می‌شوند و متغیرها از لاگ RuleEngine چک می‌شوند.";

        public static readonly string[] All =
        {
            Zabeteh, Solh, Chidman, Tahlil, Tavafogh, Commission, Income
        };

        public static readonly string[] LandTables = { "Base_Info", "Base_NosaziCode" };

        public static string Title(string id)
        {
            if (id == Zabeteh) return "ضابطه";
            if (id == Solh) return "صلح";
            if (id == Chidman) return "چیدمان";
            if (id == Tahlil) return "تحلیل";
            if (id == Tavafogh) return "توافق";
            if (id == Commission) return "کمیسیون ماده ۱۰۰";
            if (id == Income) return "درآمد";
            return id ?? "";
        }

        public static string[] Tables(string id)
        {
            if (id == Zabeteh) return new[] { "Zabeteh", "Zabeteh_Details" };
            if (id == Solh) return new[] { "Sh_Peace", "Sh_PeaceLetter", "CI_PeaceType", "Base_Using" };
            if (id == Chidman) return new[] { "Base_Using", "Base_Front", "CI_UsingType", "CI_UsingGroup", "CI_FrontPlace", "CI_FrontType" };
            if (id == Tahlil) return new[] { "AnalysisBuilding", "AnalysisBuilding_Details", "GetAnalysisBuilding", "CI_Penalty" };
            if (id == Tavafogh) return new[] { "Sh_Agreement", "Sh_AgreementLetter", "CI_AgreementType" };
            return new string[0];
        }

        public static bool BuildingZero(string id)
        {
            return id == Solh || id == Tavafogh;
        }

        public static bool Has(IList<string> selected, string id)
        {
            if (selected == null) return false;
            foreach (string s in selected)
                if (string.Equals(s, id, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        public static List<string> Normalize(IList<string> raw)
        {
            var list = new List<string>();
            if (raw == null) return list;
            foreach (string item in raw)
            {
                string t = (item ?? "").Trim().ToLowerInvariant();
                if (t == "peace" || t == "صلح") t = Solh;
                if (t == "analysis" || t == "تحلیل" || t == "takhalofat" || t == "foul") t = Tahlil;
                if (t == "agreement" || t == "توافق") t = Tavafogh;
                if (t == "ضابطه") t = Zabeteh;
                if (t == "چیدمان" || t == "zabetehconvert") t = Chidman;
                if (t == "کمیسیون" || t == "commissionfine" || t == "ماده 100" || t == "ماده ۱۰۰") t = Commission;
                if (t == "درآمد" || t == "daramad") t = Income;
                bool known = false;
                foreach (string id in All)
                    if (id == t) { known = true; break; }
                if (!known) continue;
                if (!list.Contains(t)) list.Add(t);
            }
            return list;
        }

        public static List<object> Catalog()
        {
            var list = new List<object>();
            foreach (string id in All)
            {
                list.Add(new Dictionary<string, object>
                {
                    { "id", id },
                    { "title", Title(id) },
                    { "tables", Tables(id) },
                    { "buildingZero", BuildingZero(id) },
                    { "optional", id != Zabeteh },
                });
            }
            return list;
        }
    }
}
