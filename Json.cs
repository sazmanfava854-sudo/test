using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace RuleTrace
{
    /// <summary>Tiny JSON encoder/decoder so the web host needs no NuGet packages.</summary>
    internal static class Json
    {
        public static string Encode(object value)
        {
            var sb = new StringBuilder();
            Write(sb, value, 0);
            return sb.ToString();
        }

        public static Dictionary<string, object> ParseObject(string json)
        {
            object v = new Parser(json ?? string.Empty).ParseValue();
            return v as Dictionary<string, object> ?? new Dictionary<string, object>();
        }

        public static string Str(Dictionary<string, object> map, string key, string fallback = "")
        {
            object v;
            if (map == null || !map.TryGetValue(key, out v) || v == null) return fallback;
            return Convert.ToString(v, CultureInfo.InvariantCulture) ?? fallback;
        }

        public static bool Bool(Dictionary<string, object> map, string key, bool fallback = false)
        {
            object v;
            if (map == null || !map.TryGetValue(key, out v) || v == null) return fallback;
            if (v is bool) return (bool)v;
            string s = Convert.ToString(v, CultureInfo.InvariantCulture);
            if (string.Equals(s, "true", StringComparison.OrdinalIgnoreCase) || s == "1") return true;
            if (string.Equals(s, "false", StringComparison.OrdinalIgnoreCase) || s == "0") return false;
            return fallback;
        }

        public static long Long(Dictionary<string, object> map, string key, long fallback = 0)
        {
            object v;
            if (map == null || !map.TryGetValue(key, out v) || v == null) return fallback;
            if (v is long) return (long)v;
            if (v is int) return (int)v;
            long n;
            return long.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, out n) ? n : fallback;
        }

        public static int Int(Dictionary<string, object> map, string key, int fallback = 0)
        {
            object v;
            if (map == null || !map.TryGetValue(key, out v) || v == null) return fallback;
            if (v is int) return (int)v;
            if (v is long) return (int)(long)v;
            int n;
            return int.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, out n) ? n : fallback;
        }

        private static void Write(StringBuilder sb, object value, int depth)
        {
            if (depth > 40) { sb.Append("null"); return; }
            if (value == null || value is DBNull) { sb.Append("null"); return; }
            if (value is string) { Quote(sb, (string)value); return; }
            if (value is bool) { sb.Append((bool)value ? "true" : "false"); return; }
            if (value is Enum) { Quote(sb, value.ToString()); return; }
            if (value is byte || value is sbyte || value is short || value is ushort || value is int || value is uint || value is long || value is ulong || value is float || value is double || value is decimal)
            {
                sb.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
                return;
            }
            if (value is DateTime)
            {
                Quote(sb, ((DateTime)value).ToString("o", CultureInfo.InvariantCulture));
                return;
            }
            var dict = value as IDictionary;
            if (dict != null)
            {
                sb.Append('{');
                bool first = true;
                foreach (DictionaryEntry e in dict)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    Quote(sb, Convert.ToString(e.Key, CultureInfo.InvariantCulture) ?? string.Empty);
                    sb.Append(':');
                    Write(sb, e.Value, depth + 1);
                }
                sb.Append('}');
                return;
            }
            var list = value as IEnumerable;
            if (list != null && !(value is string))
            {
                sb.Append('[');
                bool first = true;
                foreach (object item in list)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    Write(sb, item, depth + 1);
                }
                sb.Append(']');
                return;
            }

            Type t = value.GetType();
            sb.Append('{');
            bool firstProp = true;
            foreach (FieldInfo f in t.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!firstProp) sb.Append(',');
                firstProp = false;
                Quote(sb, f.Name);
                sb.Append(':');
                Write(sb, f.GetValue(value), depth + 1);
            }
            foreach (PropertyInfo p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!p.CanRead || p.GetIndexParameters().Length > 0) continue;
                if (!firstProp) sb.Append(',');
                firstProp = false;
                Quote(sb, p.Name);
                sb.Append(':');
                object pv = null;
                try { pv = p.GetValue(value, null); } catch { }
                Write(sb, pv, depth + 1);
            }
            sb.Append('}');
        }

        private static void Quote(StringBuilder sb, string s)
        {
            sb.Append('"');
            if (s != null)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    switch (c)
                    {
                        case '"': sb.Append("\\\""); break;
                        case '\\': sb.Append("\\\\"); break;
                        case '\n': sb.Append("\\n"); break;
                        case '\r': sb.Append("\\r"); break;
                        case '\t': sb.Append("\\t"); break;
                        case '\b': sb.Append("\\b"); break;
                        case '\f': sb.Append("\\f"); break;
                        default:
                            if (c < 32) sb.Append("\\u").Append(((int)c).ToString("x4"));
                            else sb.Append(c);
                            break;
                    }
                }
            }
            sb.Append('"');
        }

        private sealed class Parser
        {
            private readonly string _s;
            private int _i;

            public Parser(string s) { _s = s; }

            public object ParseValue()
            {
                Skip();
                if (_i >= _s.Length) return null;
                char c = _s[_i];
                if (c == '{') return ParseObj();
                if (c == '[') return ParseArr();
                if (c == '"') return ParseStr();
                if (c == 't' && Match("true")) return true;
                if (c == 'f' && Match("false")) return false;
                if (c == 'n' && Match("null")) return null;
                return ParseNum();
            }

            private Dictionary<string, object> ParseObj()
            {
                var map = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                _i++;
                Skip();
                if (Peek() == '}') { _i++; return map; }
                while (_i < _s.Length)
                {
                    Skip();
                    string key = ParseStr();
                    Skip();
                    if (Peek() != ':') throw new FormatException("JSON ':' expected");
                    _i++;
                    object val = ParseValue();
                    map[key] = val;
                    Skip();
                    char n = Peek();
                    if (n == ',') { _i++; continue; }
                    if (n == '}') { _i++; break; }
                    throw new FormatException("JSON object");
                }
                return map;
            }

            private List<object> ParseArr()
            {
                var list = new List<object>();
                _i++;
                Skip();
                if (Peek() == ']') { _i++; return list; }
                while (_i < _s.Length)
                {
                    list.Add(ParseValue());
                    Skip();
                    char n = Peek();
                    if (n == ',') { _i++; continue; }
                    if (n == ']') { _i++; break; }
                    throw new FormatException("JSON array");
                }
                return list;
            }

            private string ParseStr()
            {
                if (Peek() != '"') throw new FormatException("JSON string");
                _i++;
                var sb = new StringBuilder();
                while (_i < _s.Length)
                {
                    char c = _s[_i++];
                    if (c == '"') return sb.ToString();
                    if (c != '\\') { sb.Append(c); continue; }
                    if (_i >= _s.Length) break;
                    char e = _s[_i++];
                    switch (e)
                    {
                        case '"':
                        case '\\':
                        case '/': sb.Append(e); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (_i + 4 <= _s.Length)
                            {
                                int code;
                                int.TryParse(_s.Substring(_i, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out code);
                                sb.Append((char)code);
                                _i += 4;
                            }
                            break;
                        default: sb.Append(e); break;
                    }
                }
                return sb.ToString();
            }

            private object ParseNum()
            {
                int start = _i;
                if (Peek() == '-') _i++;
                while (_i < _s.Length && char.IsDigit(_s[_i])) _i++;
                bool frac = false;
                if (Peek() == '.')
                {
                    frac = true;
                    _i++;
                    while (_i < _s.Length && char.IsDigit(_s[_i])) _i++;
                }
                if (Peek() == 'e' || Peek() == 'E')
                {
                    frac = true;
                    _i++;
                    if (Peek() == '+' || Peek() == '-') _i++;
                    while (_i < _s.Length && char.IsDigit(_s[_i])) _i++;
                }
                string t = _s.Substring(start, _i - start);
                if (!frac)
                {
                    long l;
                    if (long.TryParse(t, NumberStyles.Integer, CultureInfo.InvariantCulture, out l))
                    {
                        if (l >= int.MinValue && l <= int.MaxValue) return (int)l;
                        return l;
                    }
                }
                double d;
                double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out d);
                return d;
            }

            private void Skip()
            {
                while (_i < _s.Length && char.IsWhiteSpace(_s[_i])) _i++;
            }

            private char Peek()
            {
                return _i < _s.Length ? _s[_i] : '\0';
            }

            private bool Match(string w)
            {
                if (_i + w.Length > _s.Length) return false;
                if (_s.Substring(_i, w.Length) != w) return false;
                _i += w.Length;
                return true;
            }
        }
    }
}
