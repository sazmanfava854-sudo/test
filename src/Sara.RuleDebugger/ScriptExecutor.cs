using System.Dynamic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Sara.RuleDebugger;

public sealed class RuleHostGlobals
{
    public dynamic? Host { get; set; }
}

internal sealed class ScriptExecutor
{
    private readonly ScriptOptions _options;
    private readonly RuleHostGlobals _globals;
    private ScriptState<object>? _state;

    public ScriptExecutor(object? hostContext)
    {
        _globals = new RuleHostGlobals { Host = BindHost(hostContext) };
        var opts = ScriptOptions.Default
            .WithImports("System", "System.Collections.Generic", "System.Linq")
            .AddReferences(
                typeof(object).Assembly,
                typeof(Console).Assembly,
                typeof(Enumerable).Assembly,
                typeof(ExpandoObject).Assembly,
                typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly);
        if (hostContext != null)
            opts = opts.AddReferences(hostContext.GetType().Assembly);
        _options = opts;
    }

    public object? Host => _globals.Host;

    public async Task EnsureStartedAsync()
    {
        if (_state != null) return;
        _state = await CSharpScript.RunAsync<object>("null", _options, _globals).ConfigureAwait(false);
    }

    public async Task ExecAsync(string code)
    {
        await EnsureStartedAsync().ConfigureAwait(false);
        string stmt = (code ?? "").Trim();
        if (stmt.Length == 0) return;
        if (!stmt.EndsWith("}", StringComparison.Ordinal) && !stmt.EndsWith(";", StringComparison.Ordinal))
            stmt += ";";
        _state = await _state!.ContinueWithAsync(stmt, _options).ConfigureAwait(false);
    }

    public async Task<bool> EvalConditionAsync(string expression)
    {
        await EnsureStartedAsync().ConfigureAwait(false);
        string expr = (expression ?? "").Trim();
        if (expr.EndsWith(";", StringComparison.Ordinal))
            expr = expr.TrimEnd(';').Trim();
        _state = await _state!.ContinueWithAsync(expr, _options).ConfigureAwait(false);
        object? value = _state.ReturnValue;
        return value is true || (value is not null && value is not bool && Convert.ToBoolean(value));
    }

    public IReadOnlyList<DebugVariable> SnapshotLocals()
    {
        var list = new List<DebugVariable>();
        if (_globals.Host != null)
        {
            foreach (var v in SnapshotObject("Host", _globals.Host, "Global"))
                list.Add(v);
        }
        if (_state != null)
        {
            foreach (var variable in _state.Variables)
            {
                if (variable.Name.StartsWith("__", StringComparison.Ordinal)) continue;
                if (string.Equals(variable.Name, "Host", StringComparison.Ordinal)) continue;
                list.Add(ToVariable(variable.Name, variable.Value, variable.Type, "Local"));
            }
        }
        return list;
    }

    private static object? BindHost(object? host)
    {
        if (host == null) return null;
        if (host is ExpandoObject) return host;
        IDictionary<string, object?> expando = new ExpandoObject();
        if (host is IDictionary<string, object?> dict)
        {
            foreach (var kv in dict)
                expando[kv.Key] = kv.Value;
            return expando;
        }
        if (host is IDictionary<string, object> dict2)
        {
            foreach (var kv in dict2)
                expando[kv.Key] = kv.Value;
            return expando;
        }
        foreach (var p in host.GetType().GetProperties())
        {
            if (p.GetIndexParameters().Length > 0) continue;
            object? value = null;
            try { value = p.GetValue(host); } catch { }
            expando[p.Name] = value;
        }
        return expando;
    }

    private static IEnumerable<DebugVariable> SnapshotObject(string name, object value, string scope)
    {
        yield return ToVariable(name, value, value.GetType(), scope);
        if (value is IDictionary<string, object?> exp)
        {
            foreach (var kv in exp)
                yield return ToVariable(name + "." + kv.Key, kv.Value, kv.Value?.GetType(), scope);
        }
        else
        {
            foreach (var p in value.GetType().GetProperties())
            {
                if (p.GetIndexParameters().Length > 0) continue;
                object? pv = null;
                try { pv = p.GetValue(value); } catch { /* ignore getter faults in snapshot */ }
                yield return ToVariable(name + "." + p.Name, pv, p.PropertyType, scope);
            }
        }
    }

    internal static DebugVariable ToVariable(string name, object? value, Type? type, string scope)
    {
        return new DebugVariable
        {
            Name = name,
            TypeName = type?.FullName ?? value?.GetType().FullName ?? "null",
            ValueString = Format(value),
            RawValue = value,
            Scope = scope
        };
    }

    internal static string Format(object? value)
    {
        if (value == null) return "null";
        if (value is string s) return "\"" + s + "\"";
        if (value is bool b) return b ? "true" : "false";
        if (value is IDictionary<string, object?> d)
        {
            var sb = new StringBuilder("{");
            bool first = true;
            foreach (var kv in d)
            {
                if (!first) sb.Append(", ");
                first = false;
                sb.Append(kv.Key).Append("=").Append(Format(kv.Value));
            }
            sb.Append('}');
            return sb.ToString();
        }
        return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? value.ToString() ?? "";
    }
}
