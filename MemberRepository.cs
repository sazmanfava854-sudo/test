using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Xml.Linq;

namespace RuleTrace
{
    /// <summary>Phase 1 — read/write dbo.Member in RuleEngine (XmlBody / Body). No compile, no merge.</summary>
    internal sealed class MemberRow
    {
        public int NidClass;
        public int NidMember;
        public int Version;
        public bool IsActive;
        public string EnumType;
        public string FromDate;
        public string ToDate;
        public string Name;
        public string XmlBody;
        public string Code;
        public int XmlBytes;

        public override string ToString()
        {
            return NidMember + " v" + Version + (IsActive ? " *" : "") + "  " + (Name ?? "") + "  (" + (Code == null ? 0 : Code.Length / 1024) + " KB)";
        }
    }

    internal static class MemberRepository
    {
        public static List<MemberRow> List(string ruleEngineConn, int nidClass, bool allVersions = true)
        {
            var list = new List<MemberRow>();
            string sql = allVersions
                ? @"SELECT NidMember, EnumType, isActive, Version, RTRIM(FromDate) AS FromDate, RTRIM(ToDate) AS ToDate,
                           CAST(XmlBody AS NVARCHAR(MAX)) AS XmlBody
                    FROM dbo.Member WHERE NidClass=@nid ORDER BY NidMember, Version DESC"
                : @"SELECT NidMember, EnumType, isActive, Version, RTRIM(FromDate) AS FromDate, RTRIM(ToDate) AS ToDate,
                           CAST(XmlBody AS NVARCHAR(MAX)) AS XmlBody
                    FROM dbo.Member WHERE NidClass=@nid ORDER BY NidMember";

            using (var c = new SqlConnection(ruleEngineConn))
            using (var cmd = new SqlCommand(sql, c) { CommandTimeout = 120 })
            {
                cmd.Parameters.AddWithValue("@nid", nidClass);
                c.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(ReadRow(r, nidClass));
                }
            }

            if (!allVersions)
                list = DedupeLatest(list);
            return list;
        }

        public static MemberRow Get(string ruleEngineConn, int nidClass, int nidMember, int version)
        {
            using (var c = new SqlConnection(ruleEngineConn))
            using (var cmd = new SqlCommand(
                @"SELECT NidMember, EnumType, isActive, Version, RTRIM(FromDate), RTRIM(ToDate), CAST(XmlBody AS NVARCHAR(MAX))
                  FROM dbo.Member WHERE NidClass=@nid AND NidMember=@mid AND Version=@ver", c)
            { CommandTimeout = 120 })
            {
                cmd.Parameters.AddWithValue("@nid", nidClass);
                cmd.Parameters.AddWithValue("@mid", nidMember);
                cmd.Parameters.AddWithValue("@ver", version);
                c.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return null;
                    return ReadRow(r, nidClass);
                }
            }
        }

        public static void UpdateCode(string ruleEngineConn, int nidClass, int nidMember, int version, string newCode)
        {
            MemberRow row = Get(ruleEngineConn, nidClass, nidMember, version);
            if (row == null) throw new InvalidOperationException("Member not found: " + nidMember + " v" + version);

            string newXml = MemberXml.SetBodyText(row.XmlBody, newCode, row.Name);
            using (var c = new SqlConnection(ruleEngineConn))
            using (var cmd = new SqlCommand(
                @"UPDATE dbo.Member SET XmlBody=@xml WHERE NidClass=@nid AND NidMember=@mid AND Version=@ver", c)
            { CommandTimeout = 120 })
            {
                cmd.Parameters.AddWithValue("@xml", newXml);
                cmd.Parameters.AddWithValue("@nid", nidClass);
                cmd.Parameters.AddWithValue("@mid", nidMember);
                cmd.Parameters.AddWithValue("@ver", version);
                c.Open();
                int n = cmd.ExecuteNonQuery();
                if (n != 1) throw new InvalidOperationException("UPDATE affected " + n + " row(s)");
            }
        }

        public static void SetActive(string ruleEngineConn, int nidClass, int nidMember, int version, bool active)
        {
            using (var c = new SqlConnection(ruleEngineConn))
            using (var cmd = new SqlCommand(
                @"UPDATE dbo.Member SET isActive=@a WHERE NidClass=@nid AND NidMember=@mid AND Version=@ver", c))
            {
                cmd.Parameters.AddWithValue("@a", active ? 1 : 0);
                cmd.Parameters.AddWithValue("@nid", nidClass);
                cmd.Parameters.AddWithValue("@mid", nidMember);
                cmd.Parameters.AddWithValue("@ver", version);
                c.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static MemberRow ReadRow(IDataRecord r, int nidClass)
        {
            string xml = r.IsDBNull(6) ? "" : r.GetString(6);
            string name;
            string code = MemberXml.ExtractCode(xml, out name);
            return new MemberRow
            {
                NidClass = nidClass,
                NidMember = Convert.ToInt32(r.GetValue(0)),
                EnumType = Str(r, 1),
                IsActive = ParseActive(Str(r, 2)),
                Version = ParseInt(Str(r, 3)),
                FromDate = Str(r, 4),
                ToDate = Str(r, 5),
                XmlBody = xml,
                Name = name,
                Code = code,
                XmlBytes = string.IsNullOrEmpty(xml) ? 0 : xml.Length,
            };
        }

        private static List<MemberRow> DedupeLatest(List<MemberRow> rows)
        {
            return rows
                .GroupBy(m => m.NidMember)
                .Select(g => g.OrderByDescending(m => m.IsActive).ThenByDescending(m => m.Version).ThenByDescending(m => m.Code == null ? 0 : m.Code.Length).First())
                .OrderBy(m => m.NidMember)
                .ToList();
        }

        private static string Str(IDataRecord r, int i)
        {
            try { return r.IsDBNull(i) ? "" : Convert.ToString(r.GetValue(i)); } catch { return ""; }
        }

        private static int ParseInt(string s)
        {
            int n;
            return int.TryParse(s, out n) ? n : 0;
        }

        private static bool ParseActive(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Trim();
            return s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
    }

    internal static class MemberXml
    {
        public static string ExtractCode(string xml, out string name)
        {
            name = null;
            if (string.IsNullOrWhiteSpace(xml)) return string.Empty;
            try
            {
                var doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
                var bodyEl = doc.Descendants().FirstOrDefault(e => e.Name.LocalName.Equals("Body", StringComparison.OrdinalIgnoreCase) && !e.HasElements);
                if (bodyEl != null)
                {
                    var nameEl = doc.Descendants().FirstOrDefault(e => e.Name.LocalName.Equals("Name", StringComparison.OrdinalIgnoreCase) && !e.HasElements && e.Value.Length > 0 && e.Value.Length < 120);
                    if (nameEl != null) name = nameEl.Value.Trim();
                    return NormalizeBody(bodyEl.Value);
                }
                string best = null;
                foreach (var el in doc.Descendants())
                {
                    if (el.HasElements) continue;
                    string v = el.Value;
                    string ln = el.Name.LocalName;
                    if (name == null && v.Length > 0 && v.Length < 120 && !v.Contains("\n") &&
                        (ln.IndexOf("Name", StringComparison.OrdinalIgnoreCase) >= 0 || ln.IndexOf("Title", StringComparison.OrdinalIgnoreCase) >= 0))
                        name = v.Trim();
                    if (best == null || v.Length > best.Length) best = v;
                }
                return NormalizeBody(best ?? xml);
            }
            catch
            {
                return xml;
            }
        }

        public static string SetBodyText(string xml, string newCode, string memberName)
        {
            if (string.IsNullOrWhiteSpace(xml))
                throw new InvalidOperationException("XmlBody is empty — cannot update");

            var doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
            XElement bodyEl = doc.Descendants().FirstOrDefault(e => e.Name.LocalName.Equals("Body", StringComparison.OrdinalIgnoreCase));
            if (bodyEl == null)
            {
                bodyEl = new XElement("Body");
                if (doc.Root != null) doc.Root.Add(bodyEl);
                else throw new InvalidOperationException("XmlBody has no root element");
            }
            bodyEl.Value = NormalizeBody(newCode);
            if (!string.IsNullOrWhiteSpace(memberName))
            {
                XElement nameEl = doc.Descendants().FirstOrDefault(e => e.Name.LocalName.Equals("Name", StringComparison.OrdinalIgnoreCase) && !e.HasElements);
                if (nameEl != null && string.IsNullOrWhiteSpace(nameEl.Value))
                    nameEl.Value = memberName;
            }
            return doc.ToString(SaveOptions.DisableFormatting);
        }

        private static string NormalizeBody(string text)
        {
            if (text == null) return string.Empty;
            return text.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
        }
    }
}
