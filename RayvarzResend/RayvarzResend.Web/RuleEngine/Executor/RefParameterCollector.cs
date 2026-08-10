using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>جمع‌آوری RefParameetrs از ListRefP.add — جایگزین no-op برای Centerها.</summary>
public static class RefParameterCollector
{
    public const string ListKey = "ListRefP";
    public const string PendingRefKey = "__pendingRefParam";

    public static List<RefParameter> GetOrCreateList(DslExecutionContext context)
    {
        if (!context.Variables.TryGetValue(ListKey, out var existing) || existing is not List<RefParameter> list)
        {
            list = new List<RefParameter>();
            context.Variables[ListKey] = list;
        }

        return list;
    }

    public static void TrackAssignment(DslExecutionContext context, string target, string expression)
    {
        if (!target.Contains("Ref", StringComparison.OrdinalIgnoreCase))
            return;

        if (!context.Variables.TryGetValue(PendingRefKey, out var pending)
            || pending is not RefParameter param
            || !target.StartsWith(param.VariableName, StringComparison.OrdinalIgnoreCase))
        {
            var varName = target.Split('.')[0];
            param = new RefParameter { VariableName = varName };
            context.Variables[PendingRefKey] = param;
        }

        if (target.EndsWith(".Name", StringComparison.OrdinalIgnoreCase))
            param.Name = Unquote(expression);
        else if (target.EndsWith(".Value", StringComparison.OrdinalIgnoreCase))
            param.Value = Unquote(expression);

        if (!string.IsNullOrWhiteSpace(param.Name))
            GetOrCreateList(context).RemoveAll(r =>
                r.Name.Equals(param.Name, StringComparison.OrdinalIgnoreCase)
                && r.VariableName == param.VariableName);
    }

    public static void AddPending(DslExecutionContext context, IReadOnlyList<string> arguments)
    {
        var list = GetOrCreateList(context);
        if (context.Variables.TryGetValue(PendingRefKey, out var pending) && pending is RefParameter param
            && !string.IsNullOrWhiteSpace(param.Name))
        {
            list.Add(CloneRef(param));
            context.Variables.Remove(PendingRefKey);
            return;
        }

        if (arguments.Count > 0)
        {
            var arg = arguments[0];
            if (context.Variables.TryGetValue(arg, out var obj) && obj is RefParameter fromVar)
                list.Add(CloneRef(fromVar));
        }
    }

    public static void ApplyToFiche(
        FicheHeaderDto fiche,
        IEnumerable<RefParameter> refs,
        IList<string>? warnings = null) =>
        RefParameterRegistry.ApplyAll(fiche, refs, warnings);

    private static string Unquote(string raw)
    {
        var s = raw.Trim();
        if (s.Length >= 2 && s.StartsWith('"') && s.EndsWith('"'))
            return s[1..^1];
        return s;
    }

    private static RefParameter CloneRef(RefParameter source) => new()
    {
        VariableName = source.VariableName,
        Name = source.Name,
        Value = source.Value
    };
}

public sealed class RefParameter
{
    public string VariableName { get; set; } = "";
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
}
