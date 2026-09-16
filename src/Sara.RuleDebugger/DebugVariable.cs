namespace Sara.RuleDebugger;

public sealed class DebugVariable
{
    public string Name { get; init; } = "";
    public string TypeName { get; init; } = "";
    public string ValueString { get; init; } = "";
    public object? RawValue { get; init; }
    public string Scope { get; init; } = "Local";
}
