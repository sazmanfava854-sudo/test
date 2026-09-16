using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace RuleTrace
{
    /// <summary>Formula change log in RuleEngine (NidHistory). Source of truth is DB, not compiled DLLs.</summary>
    internal sealed class HistoryRow
    {
        public long NidHistory;
        public int NidClass;
        public int NidMember;
        public string FromDate;
        public string ToDate;
        public string EnumType;
        public bool IsActive;
        public string VersionDateTime;
        public string Modifyer;
        public string ModifyDate;
        public string ModifyTime;
        public string ModifyDesc;
        public int BodyChars;
        public string Code;
        public string TableName;
    }

    internal static class MemberHistory
    {
        public static string LastTable { get; private set; }

        public static string DiscoverTable(string ruleEngineConn)
        {
            const string sql = @"
SELECT TOP 1 c.TABLE_SCHEMA, c.TABLE_NAME
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.COLUMN_NAME = 'NidHistory'
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS x
              WHERE x.TABLE_SCHEMA = c.TABLE_SCHEMA AND x.TABLE_NAME = c.TABLE_NAME AND x.COLUMN_NAME = 'NidClass')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS x
              WHERE x.TABLE_SCHEMA = c.TABLE_SCHEMA AND x.TABLE_NAME = c.TABLE_NAME AND x.COLUMN_NAME = 'NidMember')
ORDER BY CASE WHEN c.TABLE_NAME LIKE '%Member%' THEN 0 ELSE 1 END, c.TABLE_NAME";
            using (var c = new SqlConnection(ruleEngineConn))
            using (var cmd = new SqlCommand(sql, c) { CommandTimeout = 30 })
            {
                c.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return null;
                    string schema = Convert.ToString(r.GetValue(0));
                    string name = Convert.ToString(r.GetValue(1));
                    if (string.IsNullOrWhiteSpace(name)) return null;
                    LastTable = "[" + schema + "].[" + name + "]";
                    return LastTable;
                }
            }
        }

        public static List<HistoryRow> List(string ruleEngineConn, IList<int> nidClasses, int nidMember, int take, Action<string> log)
        {
            var list = new List<HistoryRow>();
            if (string.IsNullOrWhiteSpace(ruleEngineConn)) return list;
            string table;
            try { table = DiscoverTable(ruleEngineConn); }
            catch (Exception ex)
            {
                if (log != null) log("History      : discover failed — " + FirstLine(ex.Message));
                return list;
            }
            if (table == null)
            {
                if (log != null) log("History      : جدولی با ستون NidHistory پیدا نشد");
                return list;
            }
            if (log != null) log("History      : table " + table);

            HashSet<string> cols;
            try { cols = Columns(ruleEngineConn, table); }
            catch (Exception ex)
            {
                if (log != null) log("History      : columns — " + FirstLine(ex.Message));
                return list;
            }

            Dictionary<string, string> types;
            try { types = ColumnTypes(ruleEngineConn, table); }
            catch { types = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); }
            string bodyCol = FirstCol(cols, "Body", "XmlBody", "EncryptXmlBody");
            string bodyType = bodyCol != null && types.ContainsKey(bodyCol) ? types[bodyCol] : "";
            if (bodyCol != null && log != null)
                log("History      : Body column " + bodyCol + " type=" + (string.IsNullOrEmpty(bodyType) ? "?" : bodyType)
                    + (IsBinaryBody(bodyType) ? " — list uses DATALENGTH (no CAST image→nvarchar)" : ""));

            var select = new List<string>();
            AddCol(select, cols, "NidHistory");
            AddCol(select, cols, "NidClass");
            AddCol(select, cols, "NidMember");
            AddCol(select, cols, "FromDate");
            AddCol(select, cols, "ToDate");
            AddCol(select, cols, "EnumType");
            AddCol(select, cols, "isActive", "IsActive");
            AddCol(select, cols, "VersionDateTime");
            AddCol(select, cols, "Modifyer", "Modifier", "ModifyUser", "UserName");
            AddCol(select, cols, "ModifyDate");
            AddCol(select, cols, "ModifyTime");
            AddCol(select, cols, "ModifyDesc", "Description", "Desc");
            // Never CAST(image AS NVARCHAR(MAX)) — SQL Server rejects it.
            if (bodyCol != null) select.Add(ListBodyExpr(bodyCol));

            var where = new StringBuilder();
            where.Append(" WHERE 1=1 ");
            if (nidClasses != null && nidClasses.Count > 0)
                where.Append(" AND NidClass IN (" + string.Join(",", nidClasses.Distinct().Select(n => n.ToString())) + ") ");
            if (nidMember > 0)
                where.Append(" AND NidMember = " + nidMember + " ");

            string order = cols.Contains("NidHistory") ? "NidHistory DESC"
                : cols.Contains("ModifyDate") ? "ModifyDate DESC"
                : "NidMember";
            if (take <= 0 || take > 200) take = 80;
            string sql = "SELECT TOP " + take + " " + string.Join(", ", select) + " FROM " + table + where + " ORDER BY " + order;

            try
            {
                using (var c = new SqlConnection(ruleEngineConn))
                using (var cmd = new SqlCommand(sql, c) { CommandTimeout = 60 })
                {
                    c.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                            list.Add(Read(r, table, false));
                    }
                }
            }
            catch (Exception ex)
            {
                if (log != null) log("History      : query failed — " + FirstLine(ex.Message));
            }
            if (log != null) log("History      : " + list.Count + " change row(s)");
            return list;
        }

        public static HistoryRow Get(string ruleEngineConn, long nidHistory, Action<string> log)
        {
            if (string.IsNullOrWhiteSpace(ruleEngineConn) || nidHistory <= 0) return null;
            string table;
            try { table = DiscoverTable(ruleEngineConn); }
            catch (Exception ex)
            {
                if (log != null) log("History      : discover failed — " + FirstLine(ex.Message));
                return null;
            }
            if (table == null) return null;
            HashSet<string> cols;
            Dictionary<string, string> types;
            try
            {
                cols = Columns(ruleEngineConn, table);
                types = ColumnTypes(ruleEngineConn, table);
            }
            catch (Exception ex)
            {
                if (log != null) log("History      : columns — " + FirstLine(ex.Message));
                return null;
            }
            if (!cols.Contains("NidHistory")) return null;
            string bodyCol = FirstCol(cols, "Body", "XmlBody", "EncryptXmlBody");
            string bodyType = bodyCol != null && types.ContainsKey(bodyCol) ? types[bodyCol] : "";
            var select = new List<string>();
            AddCol(select, cols, "NidHistory");
            AddCol(select, cols, "NidClass");
            AddCol(select, cols, "NidMember");
            AddCol(select, cols, "FromDate");
            AddCol(select, cols, "ToDate");
            AddCol(select, cols, "EnumType");
            AddCol(select, cols, "isActive", "IsActive");
            AddCol(select, cols, "VersionDateTime");
            AddCol(select, cols, "Modifyer", "Modifier", "ModifyUser", "UserName");
            AddCol(select, cols, "ModifyDate");
            AddCol(select, cols, "ModifyTime");
            AddCol(select, cols, "ModifyDesc", "Description", "Desc");
            if (bodyCol != null)
            {
                select.Add(ListBodyExpr(bodyCol));
                select.Add(BodyPayloadExpr(bodyCol, bodyType));
            }
            string sql = "SELECT TOP 1 " + string.Join(", ", select) + " FROM " + table + " WHERE NidHistory=@id";
            try
            {
                using (var c = new SqlConnection(ruleEngineConn))
                using (var cmd = new SqlCommand(sql, c) { CommandTimeout = 60 })
                {
                    cmd.Parameters.AddWithValue("@id", nidHistory);
                    c.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return null;
                        return Read(r, table, bodyCol != null);
                    }
                }
            }
            catch (Exception ex)
            {
                if (log != null) log("History      : get failed — " + FirstLine(ex.Message));
                return null;
            }
        }

        /// <summary>List query: size only. CAST(image AS NVARCHAR(MAX)) is illegal on SQL Server.</summary>
        internal static string ListBodyExpr(string col)
        {
            return "DATALENGTH(" + Quote(col) + ") AS BodyBytes";
        }

        /// <summary>One-row payload. Prefer VARBINARY so image never CAST to nvarchar.</summary>
        internal static string BodyPayloadExpr(string col, string dataType)
        {
            string q = Quote(col);
            string t = (dataType ?? "").ToLowerInvariant();
            if (t == "text" || t == "ntext")
                return "CONVERT(NVARCHAR(MAX), " + q + ") AS BodyText";
            return "CONVERT(VARBINARY(MAX), " + q + ") AS BodyBin";
        }

        internal static bool IsBinaryBody(string dataType)
        {
            string t = (dataType ?? "").ToLowerInvariant();
            return t == "image" || t == "varbinary" || t == "binary";
        }

        internal static string DecodeBodyBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return "";
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
                return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
                return Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
            int nuls = 0;
            int lim = Math.Min(bytes.Length, 200);
            for (int i = 1; i < lim; i += 2)
                if (bytes[i] == 0) nuls++;
            if (lim >= 4 && nuls > lim / 6)
                return Encoding.Unicode.GetString(bytes);
            return Encoding.UTF8.GetString(bytes);
        }

        public static void Report(string ruleEngineConn, IList<int> nidClasses, Action<string> log)
        {
            if (log == null) return;
            var rows = List(ruleEngineConn, nidClasses, 0, 40, log);
            if (rows.Count == 0) return;
            log("History      : آخرین تغییرات فرمول (Modifyer / تاریخ / توضیح) — منبع DB است نه DLL:");
            foreach (HistoryRow h in rows.Take(25))
            {
                log("History      : " + FormulaEngine.ClassName(h.NidClass) + "/" + h.NidClass
                    + " Member " + h.NidMember
                    + " hist=" + h.NidHistory
                    + " " + h.Modifyer
                    + " " + h.ModifyDate + " " + h.ModifyTime
                    + " " + Trunc(h.ModifyDesc, 60)
                    + (h.BodyChars > 0 ? " body=" + h.BodyChars : " body=NULL"));
            }
            var chid = rows.Where(h => h.NidMember == ChidmanAnalyzer.DefaultChidmanMemberId || h.NidClass == 342 || h.NidClass == 344).Take(8).ToList();
            if (chid.Count > 0)
            {
                log("History      : ردیف‌های مرتبط چیدمان/صلح:");
                foreach (HistoryRow h in chid)
                    log("History      :   " + h.NidClass + "/" + h.NidMember + " " + h.ModifyDate + " " + Trunc(h.ModifyDesc, 80));
            }
        }

        private static HistoryRow Read(IDataRecord r, string table, bool hasBody)
        {
            var h = new HistoryRow { TableName = table };
            h.NidHistory = ToLong(Val(r, "NidHistory"));
            h.NidClass = ToInt(Val(r, "NidClass"));
            h.NidMember = ToInt(Val(r, "NidMember"));
            h.FromDate = Trim(Val(r, "FromDate"));
            h.ToDate = Trim(Val(r, "ToDate"));
            h.EnumType = Trim(Val(r, "EnumType"));
            h.IsActive = ParseActive(Val(r, "isActive")) || ParseActive(Val(r, "IsActive"));
            h.VersionDateTime = Trim(Val(r, "VersionDateTime"));
            h.Modifyer = FirstNonEmpty(Val(r, "Modifyer"), Val(r, "Modifier"), Val(r, "ModifyUser"), Val(r, "UserName"));
            h.ModifyDate = Trim(Val(r, "ModifyDate"));
            h.ModifyTime = Trim(Val(r, "ModifyTime"));
            h.ModifyDesc = FirstNonEmpty(Val(r, "ModifyDesc"), Val(r, "Description"), Val(r, "Desc"));
            long bytes = ToLong(Val(r, "BodyBytes"));
            if (hasBody)
            {
                string raw = Val(r, "BodyText");
                if (string.IsNullOrEmpty(raw))
                {
                    byte[] bin = Bytes(r, "BodyBin");
                    if (bin != null && bin.Length > 0)
                    {
                        raw = DecodeBodyBytes(bin);
                        if (bytes <= 0) bytes = bin.Length;
                    }
                }
                h.BodyChars = bytes > 0 ? (bytes > int.MaxValue ? int.MaxValue : (int)bytes)
                    : (raw == null ? 0 : raw.Length);
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    string name;
                    h.Code = MemberXml.ExtractCode(raw, out name);
                    if (string.IsNullOrEmpty(h.Code)) h.Code = raw;
                    if (h.Code.Length > 200000) h.Code = h.Code.Substring(0, 200000) + "\n/* truncated */";
                }
            }
            else
            {
                h.BodyChars = bytes > int.MaxValue ? int.MaxValue : (int)bytes;
            }
            return h;
        }

        private static HashSet<string> Columns(string cs, string table)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string name = table.Replace("[", "").Replace("]", "");
            string schema = "dbo";
            string tbl = name;
            int dot = name.IndexOf('.');
            if (dot > 0) { schema = name.Substring(0, dot); tbl = name.Substring(dot + 1); }
            using (var c = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=@s AND TABLE_NAME=@t", c))
            {
                cmd.Parameters.AddWithValue("@s", schema);
                cmd.Parameters.AddWithValue("@t", tbl);
                c.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) set.Add(r.GetString(0));
            }
            return set;
        }

        private static Dictionary<string, string> ColumnTypes(string cs, string table)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string name = table.Replace("[", "").Replace("]", "");
            string schema = "dbo";
            string tbl = name;
            int dot = name.IndexOf('.');
            if (dot > 0) { schema = name.Substring(0, dot); tbl = name.Substring(dot + 1); }
            using (var c = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=@s AND TABLE_NAME=@t", c))
            {
                cmd.Parameters.AddWithValue("@s", schema);
                cmd.Parameters.AddWithValue("@t", tbl);
                c.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) map[r.GetString(0)] = r.GetString(1);
            }
            return map;
        }

        private static void AddCol(List<string> select, HashSet<string> cols, params string[] names)
        {
            foreach (string n in names)
            {
                if (!cols.Contains(n)) continue;
                select.Add(Quote(n));
                return;
            }
        }

        private static string FirstCol(HashSet<string> cols, params string[] names)
        {
            foreach (string n in names)
                if (cols.Contains(n)) return n;
            return null;
        }

        private static string Quote(string ident)
        {
            return "[" + ident.Replace("]", "]]") + "]";
        }

        private static byte[] Bytes(IDataRecord r, string name)
        {
            try
            {
                int i = r.GetOrdinal(name);
                if (r.IsDBNull(i)) return null;
                object v = r.GetValue(i);
                return v as byte[];
            }
            catch { return null; }
        }

        private static string Val(IDataRecord r, string name)
        {
            try
            {
                int i = r.GetOrdinal(name);
                if (r.IsDBNull(i)) return "";
                return Convert.ToString(r.GetValue(i)) ?? "";
            }
            catch { return ""; }
        }

        private static string Trim(string s) { return (s ?? "").Trim(); }

        private static string FirstNonEmpty(params string[] parts)
        {
            foreach (string p in parts)
                if (!string.IsNullOrWhiteSpace(p)) return p.Trim();
            return "";
        }

        private static int ToInt(string s)
        {
            int n;
            return int.TryParse(s, out n) ? n : 0;
        }

        private static long ToLong(string s)
        {
            long n;
            return long.TryParse(s, out n) ? n : 0;
        }

        private static bool ParseActive(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Trim();
            return s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        private static string Trunc(string s, int n)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Replace("\r", " ").Replace("\n", " ");
            return s.Length <= n ? s : s.Substring(0, n) + "…";
        }

        private static string FirstLine(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            int i = s.IndexOf('\n');
            return i < 0 ? s : s.Substring(0, i);
        }
    }
}
