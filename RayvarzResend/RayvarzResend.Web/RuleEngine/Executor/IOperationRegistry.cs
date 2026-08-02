namespace RayvarzResend.Web.RuleEngine.Executor;

public delegate object? OperationHandler(DslExecutionContext context, IReadOnlyList<string> arguments);

public interface IOperationRegistry
{
    bool IsKnown(string operationKey);
    IReadOnlyCollection<string> KnownOperationKeys { get; }
    object? Invoke(string operationKey, DslExecutionContext context, IReadOnlyList<string> arguments);
    static string BuildKey(string? receiver, string operation) =>
        string.IsNullOrWhiteSpace(receiver) ? operation.Trim() : $"{receiver.Trim()}.{operation.Trim()}";
}

public sealed class OperationRegistry : IOperationRegistry
{
    private readonly Dictionary<string, OperationHandler> _handlers =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<string> KnownOperationKeys => _handlers.Keys;

    public void Register(string key, OperationHandler handler) => _handlers[key] = handler;

    public bool IsKnown(string operationKey) =>
        _handlers.ContainsKey(operationKey) || IsCollectionMutationNoOp(operationKey);

    public object? Invoke(string operationKey, DslExecutionContext context, IReadOnlyList<string> arguments)
    {
        if (_handlers.TryGetValue(operationKey, out var handler))
            return handler(context, arguments);

        if (IsCollectionMutationNoOp(operationKey))
        {
            context.Variables["lastCollectionOp"] = operationKey;
            return null;
        }

        throw new InvalidOperationException($"Operation ناشناخته: {operationKey}");
    }

    /// <summary>List*.Add / TmpAccounting_*.Add — در DryRun/فاز ۳ فقط side-effect VB؛ ردیف‌ها از Fiche live می‌آیند.</summary>
    internal static bool IsCollectionMutationNoOp(string operationKey)
    {
        if (string.IsNullOrWhiteSpace(operationKey))
            return false;

        var key = operationKey.Trim();
        var dot = key.LastIndexOf('.');
        if (dot <= 0 || dot >= key.Length - 1)
            return false;

        var method = key[(dot + 1)..];
        if (!method.Equals("Add", StringComparison.OrdinalIgnoreCase)
            && !method.Equals("Clear", StringComparison.OrdinalIgnoreCase)
            && !method.Equals("Remove", StringComparison.OrdinalIgnoreCase))
            return false;

        var receiver = key[..dot];
        return receiver.StartsWith("List", StringComparison.OrdinalIgnoreCase)
               || receiver.StartsWith("TmpAccounting", StringComparison.OrdinalIgnoreCase)
               || receiver.StartsWith("TmpDocument", StringComparison.OrdinalIgnoreCase)
               || receiver.Contains("List", StringComparison.OrdinalIgnoreCase);
    }
}
