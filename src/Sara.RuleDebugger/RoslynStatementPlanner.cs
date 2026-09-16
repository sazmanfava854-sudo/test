using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sara.RuleDebugger;

/// <summary>
/// Lowers a C# script into a cooperative instruction stream while keeping Roslyn line/column spans.
/// Control-flow (if / while / for) becomes EvalJump + Jump so StepNext can pause on each statement.
/// </summary>
internal static class RoslynStatementPlanner
{
    public static StatementPlan Build(string scriptText)
    {
        scriptText ??= "";
        var tree = CSharpSyntaxTree.ParseText(
            scriptText,
            new CSharpParseOptions(LanguageVersion.Latest, kind: SourceCodeKind.Script));
        var root = tree.GetCompilationUnitRoot();

        var errors = tree.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        if (errors.Count > 0)
        {
            var first = errors[0];
            var span = first.Location.GetLineSpan();
            return StatementPlan.FromDiagnostics(
                scriptText,
                span.StartLinePosition.Line + 1,
                span.StartLinePosition.Character + 1,
                first.GetMessage());
        }

        var emit = new Emitter();
        foreach (var member in root.Members)
        {
            if (member is GlobalStatementSyntax global)
                emit.Statement(global.Statement);
            else
                emit.Exec(member);
        }

        return new StatementPlan
        {
            OriginalText = scriptText,
            Instructions = emit.Instructions
        };
    }

    private sealed class Emitter
    {
        public List<Instruction> Instructions { get; } = new();

        public void Statement(StatementSyntax syntax)
        {
            switch (syntax)
            {
                case BlockSyntax block:
                    foreach (var inner in block.Statements)
                        Statement(inner);
                    break;
                case EmptyStatementSyntax:
                    break;
                case IfStatementSyntax iff:
                    If(iff);
                    break;
                case WhileStatementSyntax loop:
                    While(loop);
                    break;
                case ForStatementSyntax loop:
                    For(loop);
                    break;
                default:
                    Exec(syntax);
                    break;
            }
        }

        private void If(IfStatementSyntax iff)
        {
            var cond = JumpFrom(iff.Condition, iff);
            int condIdx = Instructions.Count;
            Instructions.Add(cond);
            cond.ThenIndex = Instructions.Count;
            Statement(iff.Statement);
            var skipElse = new JumpInstruction { Line = iff.GetLocation().GetLineSpan().StartLinePosition.Line + 1 };
            Instructions.Add(skipElse);
            cond.ElseIndex = Instructions.Count;
            if (iff.Else != null)
                Statement(iff.Else.Statement);
            skipElse.TargetIndex = Instructions.Count;
        }

        private void While(WhileStatementSyntax loop)
        {
            int condIdx = Instructions.Count;
            var cond = JumpFrom(loop.Condition, loop);
            Instructions.Add(cond);
            cond.ThenIndex = Instructions.Count;
            Statement(loop.Statement);
            Instructions.Add(new JumpInstruction
            {
                TargetIndex = condIdx,
                Line = loop.GetLocation().GetLineSpan().StartLinePosition.Line + 1
            });
            cond.ElseIndex = Instructions.Count;
        }

        private void For(ForStatementSyntax loop)
        {
            if (loop.Declaration != null)
                Exec(loop.Declaration);
            foreach (var init in loop.Initializers)
                ExecExpression(init, loop);

            int condIdx = Instructions.Count;
            var condition = loop.Condition;
            EvalJumpInstruction cond;
            if (condition == null)
            {
                cond = new EvalJumpInstruction
                {
                    Source = "true",
                    Line = loop.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    Column = loop.GetLocation().GetLineSpan().StartLinePosition.Character + 1,
                    IsCondition = true
                };
            }
            else
                cond = JumpFrom(condition, loop);
            Instructions.Add(cond);
            cond.ThenIndex = Instructions.Count;
            Statement(loop.Statement);
            foreach (var inc in loop.Incrementors)
                ExecExpression(inc, loop);
            Instructions.Add(new JumpInstruction
            {
                TargetIndex = condIdx,
                Line = loop.GetLocation().GetLineSpan().StartLinePosition.Line + 1
            });
            cond.ElseIndex = Instructions.Count;
        }

        public void Exec(SyntaxNode node)
        {
            var loc = node.GetLocation().GetLineSpan().StartLinePosition;
            Instructions.Add(new ExecInstruction
            {
                Source = node.ToString().Trim(),
                Line = loc.Line + 1,
                Column = loc.Character + 1
            });
        }

        private void ExecExpression(ExpressionSyntax expr, SyntaxNode owner)
        {
            var loc = expr.GetLocation().GetLineSpan().StartLinePosition;
            string src = expr.ToString().Trim();
            if (!src.EndsWith(";", StringComparison.Ordinal))
                src += ";";
            Instructions.Add(new ExecInstruction
            {
                Source = src,
                Line = loc.Line + 1,
                Column = loc.Character + 1
            });
        }

        private static EvalJumpInstruction JumpFrom(ExpressionSyntax condition, SyntaxNode owner)
        {
            var loc = condition.GetLocation().GetLineSpan().StartLinePosition;
            return new EvalJumpInstruction
            {
                Source = condition.ToString().Trim(),
                Line = loc.Line + 1,
                Column = loc.Character + 1,
                IsCondition = true
            };
        }
    }
}

internal sealed class StatementPlan
{
    public string OriginalText { get; init; } = "";
    public List<Instruction> Instructions { get; init; } = new();
    public bool HasParseError { get; init; }
    public string ParseError { get; init; } = "";
    public int ParseErrorLine { get; init; }
    public int ParseErrorColumn { get; init; }

    public static StatementPlan FromDiagnostics(string text, int line, int column, string message)
    {
        return new StatementPlan
        {
            OriginalText = text,
            HasParseError = true,
            ParseError = message,
            ParseErrorLine = line < 1 ? 1 : line,
            ParseErrorColumn = column < 1 ? 1 : column
        };
    }
}

internal abstract class Instruction
{
    public string Source { get; init; } = "";
    public int Line { get; init; } = 1;
    public int Column { get; init; } = 1;
    public bool IsUserVisible => this is ExecInstruction or EvalJumpInstruction;
}

internal sealed class ExecInstruction : Instruction { }

internal sealed class EvalJumpInstruction : Instruction
{
    public int ThenIndex { get; set; }
    public int ElseIndex { get; set; }
    public bool IsCondition { get; init; }
}

internal sealed class JumpInstruction : Instruction
{
    public int TargetIndex { get; set; }
}
