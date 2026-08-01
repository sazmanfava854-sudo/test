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

    public bool IsKnown(string operationKey) => _handlers.ContainsKey(operationKey);

    public object? Invoke(string operationKey, DslExecutionContext context, IReadOnlyList<string> arguments)
    {
        if (!_handlers.TryGetValue(operationKey, out var handler))
            throw new InvalidOperationException($"Operation ناشناخته: {operationKey}");

        return handler(context, arguments);
    }
}
