using System.Text.Json.Serialization;

namespace RayvarzResend.Web.RuleEngine.Parser;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(DslAssignStatement), "assign")]
[JsonDerivedType(typeof(DslIfStatement), "if")]
[JsonDerivedType(typeof(DslCallOperationStatement), "callOp")]
[JsonDerivedType(typeof(DslCallFunctionStatement), "callFn")]
[JsonDerivedType(typeof(DslTryCatchStatement), "try")]
[JsonDerivedType(typeof(DslReturnStatement), "return")]
[JsonDerivedType(typeof(DslUnsupportedStatement), "unsupported")]
public abstract record DslStatement;

public sealed record DslAssignStatement(string Target, string Expression) : DslStatement;

public sealed record DslIfStatement(
    string Condition,
    IReadOnlyList<DslStatement> ThenBranch,
    IReadOnlyList<DslElseIfBranch> ElseIfBranches,
    IReadOnlyList<DslStatement>? ElseBranch) : DslStatement;

public sealed record DslElseIfBranch(string Condition, IReadOnlyList<DslStatement> Body);

public sealed record DslCallOperationStatement(string Operation, string? Receiver, IReadOnlyList<string> Arguments)
    : DslStatement;

public sealed record DslCallFunctionStatement(string FunctionName, IReadOnlyList<string> Arguments) : DslStatement;

public sealed record DslTryCatchStatement(
    IReadOnlyList<DslStatement> TryBranch,
    string? CatchVariable,
    IReadOnlyList<DslStatement>? CatchBranch) : DslStatement;

public sealed record DslReturnStatement(string? Expression) : DslStatement;

public sealed record DslUnsupportedStatement(string Reason, string SourceSnippet) : DslStatement;

public sealed class DslFunction
{
    public string Name { get; init; } = "";
    public string? DisplayName { get; init; }
    public bool IsSupported { get; init; }
    public IReadOnlyList<DslStatement> Body { get; init; } = Array.Empty<DslStatement>();
}

public sealed class DslProgram
{
    public string ParserVersion { get; init; } = "2.0.0";
    public string EntryPoint { get; init; } = "Run";
    public int NidFunction { get; init; }
    public string FunctionName { get; init; } = "";
    public IReadOnlyList<DslFunction> Functions { get; init; } = Array.Empty<DslFunction>();
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> UnsupportedFunctions { get; init; } = Array.Empty<string>();
    public bool HasEntryPoint => Functions.Any(f => f.Name.Equals(EntryPoint, StringComparison.OrdinalIgnoreCase));
    public bool HasNosazi => Functions.Any(f =>
        f.Name.Equals("Nosazi", StringComparison.OrdinalIgnoreCase)
        || string.Equals(f.DisplayName, "نوسازی", StringComparison.Ordinal));
}

public sealed class DslParseResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public XmlEnvelope? Envelope { get; init; }
    public DslProgram? Program { get; init; }
}

public sealed class DslPersistResult
{
    public bool Stored { get; init; }
    public bool SkippedExisting { get; init; }
    public long? SnapshotId { get; init; }
    public int DslVersion { get; init; }
    public string? XmlHash { get; init; }
    public DslParseResult? Parse { get; init; }
    public string? Message { get; init; }
}
