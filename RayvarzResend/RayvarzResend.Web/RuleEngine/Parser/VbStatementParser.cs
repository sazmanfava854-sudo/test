using System.Text.RegularExpressions;

namespace RayvarzResend.Web.RuleEngine.Parser;

internal static class VbStatementParser
{
    private static readonly HashSet<string> Phase2SupportedFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "Run", "Nosazi", "نوسازی",
        "iNcOME", "iNcOMEOragh", "iNcOMESeprdeh", "iNcOMEEshghal", "iNcOMESepordeh", "Income"
    };

    private static readonly Regex DimAssignRegex = new(
        @"^Dim\s+(\w+)\s*(?:As\s+[\w.]+)?\s*=\s*(.+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex DimAsNewRegex = new(
        @"^Dim\s+(\w+)\s+As\s+New\s+(.+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex DimAsTypeRegex = new(
        @"^Dim\s+(\w+)\s+As\s+(.+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex SimpleAssignRegex = new(
        @"^([\w.]+)\s*=\s*(.+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex ReturnRegex = new(
        @"^Return\s+(.+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex CallRegex = new(
        @"^(?:Call\s+)?((?:[\w.]+\.)*[\w]+)\s*\((.*)\)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static IReadOnlyList<DslStatement> ParseBlock(
        string body,
        IReadOnlySet<string> localFunctionNames,
        IReadOnlyList<string> warnings,
        bool allowUnsupportedFallback = true)
    {
        var lines = NormalizeLines(body);
        var statements = new List<DslStatement>();
        var index = 0;

        while (index < lines.Count)
        {
            var line = lines[index];
            if (IsSkippable(line))
            {
                index++;
                continue;
            }

            if (TryParseIfBlock(lines, ref index, localFunctionNames, warnings, out var ifStatement))
            {
                statements.Add(ifStatement);
                continue;
            }

            if (TryParseTryBlock(lines, ref index, localFunctionNames, warnings, out var tryStatement))
            {
                statements.Add(tryStatement);
                continue;
            }

            if (TryParseDim(line, out var dimStatement))
            {
                statements.Add(dimStatement);
                index++;
                continue;
            }

            if (SimpleAssignRegex.Match(line).Success && !line.StartsWith("End ", StringComparison.OrdinalIgnoreCase))
            {
                var m = SimpleAssignRegex.Match(line);
                statements.Add(new DslAssignStatement(m.Groups[1].Value, m.Groups[2].Value.Trim()));
                index++;
                continue;
            }

            if (ReturnRegex.Match(line).Success)
            {
                var m = ReturnRegex.Match(line);
                statements.Add(new DslReturnStatement(m.Groups[1].Value.Trim()));
                index++;
                continue;
            }

            if (StartsWithKeyword(line, "Select Case")
                || StartsWithKeyword(line, "Case ")
                || line.Equals("Case Else", StringComparison.OrdinalIgnoreCase)
                || line.Equals("End Select", StringComparison.OrdinalIgnoreCase))
            {
                statements.Add(new DslUnsupportedStatement("Select Case not in Phase 2 subset", line));
                index++;
                continue;
            }

            if (TryParseCall(line, localFunctionNames, out var callStatement))
            {
                statements.Add(callStatement);
                index++;
                continue;
            }

            if (allowUnsupportedFallback)
            {
                statements.Add(new DslUnsupportedStatement("Statement not in Phase 2 subset", line));
            }

            index++;
        }

        return statements;
    }

    private static bool TryParseIfBlock(
        IReadOnlyList<string> lines,
        ref int index,
        IReadOnlySet<string> localFunctionNames,
        IReadOnlyList<string> warnings,
        out DslIfStatement statement)
    {
        statement = null!;
        var line = lines[index];
        if (!StartsWithKeyword(line, "If "))
            return false;

        var thenSplit = SplitOnThen(line);
        if (thenSplit == null)
            return false;

        var condition = thenSplit.Value.condition;
        var thenInline = thenSplit.Value.inlineBody;
        index++;

        var thenBranch = new List<DslStatement>();
        if (!string.IsNullOrWhiteSpace(thenInline))
        {
            thenBranch.AddRange(ParseInlineStatements(thenInline, localFunctionNames, warnings));
        }
        else
        {
            thenBranch.AddRange(ParseUntil(lines, ref index, localFunctionNames, warnings,
                stopWhen: l => StartsWithKeyword(l, "ElseIf ")
                              || StartsWithKeyword(l, "Else")
                              || StartsWithKeyword(l, "End If")));
        }

        var elseIfBranches = new List<DslElseIfBranch>();
        while (index < lines.Count && StartsWithKeyword(lines[index], "ElseIf "))
        {
            var elseIfLine = lines[index];
            var elseIfSplit = SplitOnThen(elseIfLine["ElseIf ".Length..].Insert(0, "If "));
            if (elseIfSplit == null)
                break;

            index++;
            var elseIfBody = new List<DslStatement>();
            if (!string.IsNullOrWhiteSpace(elseIfSplit.Value.inlineBody))
            {
                elseIfBody.AddRange(ParseInlineStatements(elseIfSplit.Value.inlineBody, localFunctionNames, warnings));
            }
            else
            {
                elseIfBody.AddRange(ParseUntil(lines, ref index, localFunctionNames, warnings,
                    stopWhen: l => StartsWithKeyword(l, "ElseIf ")
                                  || StartsWithKeyword(l, "Else")
                                  || StartsWithKeyword(l, "End If")));
            }

            elseIfBranches.Add(new DslElseIfBranch(elseIfSplit.Value.condition, elseIfBody));
        }

        IReadOnlyList<DslStatement>? elseBranch = null;
        if (index < lines.Count && StartsWithKeyword(lines[index], "Else"))
        {
            var elseLine = lines[index].Trim();
            index++;
            if (elseLine.Contains(" Then", StringComparison.OrdinalIgnoreCase))
            {
                var inline = elseLine[(elseLine.IndexOf("Then", StringComparison.OrdinalIgnoreCase) + 4)..].Trim();
                elseBranch = ParseInlineStatements(inline, localFunctionNames, warnings);
            }
            else
            {
                elseBranch = ParseUntil(lines, ref index, localFunctionNames, warnings,
                    stopWhen: l => StartsWithKeyword(l, "End If"));
            }
        }

        if (index < lines.Count && StartsWithKeyword(lines[index], "End If"))
            index++;

        statement = new DslIfStatement(condition, thenBranch, elseIfBranches, elseBranch);
        return true;
    }

    private static bool TryParseTryBlock(
        IReadOnlyList<string> lines,
        ref int index,
        IReadOnlySet<string> localFunctionNames,
        IReadOnlyList<string> warnings,
        out DslTryCatchStatement statement)
    {
        statement = null!;
        if (!StartsWithKeyword(lines[index], "Try"))
            return false;

        index++;
        var tryBranch = ParseUntil(lines, ref index, localFunctionNames, warnings,
            stopWhen: l => StartsWithKeyword(l, "Catch"));

        string? catchVariable = null;
        IReadOnlyList<DslStatement>? catchBranch = null;
        if (index < lines.Count && StartsWithKeyword(lines[index], "Catch"))
        {
            catchVariable = ExtractCatchVariable(lines[index]);
            index++;
            catchBranch = ParseUntil(lines, ref index, localFunctionNames, warnings,
                stopWhen: l => StartsWithKeyword(l, "End Try"));
        }

        if (index < lines.Count && StartsWithKeyword(lines[index], "End Try"))
            index++;

        statement = new DslTryCatchStatement(tryBranch, catchVariable, catchBranch);
        return true;
    }

    private static List<DslStatement> ParseUntil(
        IReadOnlyList<string> lines,
        ref int index,
        IReadOnlySet<string> localFunctionNames,
        IReadOnlyList<string> warnings,
        Func<string, bool> stopWhen)
    {
        var collected = new List<DslStatement>();
        while (index < lines.Count)
        {
            var line = lines[index];
            if (IsSkippable(line))
            {
                index++;
                continue;
            }

            if (stopWhen(line))
                break;

            if (TryParseIfBlock(lines, ref index, localFunctionNames, warnings, out var ifStatement))
            {
                collected.Add(ifStatement);
                continue;
            }

            if (TryParseTryBlock(lines, ref index, localFunctionNames, warnings, out var tryStatement))
            {
                collected.Add(tryStatement);
                continue;
            }

            if (TryParseDim(line, out var dimStatement))
            {
                collected.Add(dimStatement);
                index++;
                continue;
            }

            if (SimpleAssignRegex.Match(line).Success)
            {
                var m = SimpleAssignRegex.Match(line);
                collected.Add(new DslAssignStatement(m.Groups[1].Value, m.Groups[2].Value.Trim()));
                index++;
                continue;
            }

            if (ReturnRegex.Match(line).Success)
            {
                var m = ReturnRegex.Match(line);
                collected.Add(new DslReturnStatement(m.Groups[1].Value.Trim()));
                index++;
                continue;
            }

            if (StartsWithKeyword(line, "Select Case")
                || StartsWithKeyword(line, "Case ")
                || line.Equals("Case Else", StringComparison.OrdinalIgnoreCase)
                || line.Equals("End Select", StringComparison.OrdinalIgnoreCase))
            {
                collected.Add(new DslUnsupportedStatement("Select Case not in Phase 2 subset", line));
                index++;
                continue;
            }

            if (TryParseCall(line, localFunctionNames, out var callStatement))
            {
                collected.Add(callStatement);
                index++;
                continue;
            }

            collected.Add(new DslUnsupportedStatement("Statement not in Phase 2 subset", line));
            index++;
        }

        return collected;
    }

    private static IReadOnlyList<DslStatement> ParseInlineStatements(
        string inline,
        IReadOnlySet<string> localFunctionNames,
        IReadOnlyList<string> warnings)
    {
        var statements = new List<DslStatement>();
        foreach (var part in inline.Split(':'))
        {
            var trimmed = part.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                continue;

            if (ReturnRegex.Match(trimmed).Success)
            {
                var m = ReturnRegex.Match(trimmed);
                statements.Add(new DslReturnStatement(m.Groups[1].Value.Trim()));
                continue;
            }

            if (SimpleAssignRegex.Match(trimmed).Success)
            {
                var m = SimpleAssignRegex.Match(trimmed);
                statements.Add(new DslAssignStatement(m.Groups[1].Value, m.Groups[2].Value.Trim()));
                continue;
            }

            if (TryParseCall(trimmed, localFunctionNames, out var callStatement))
            {
                statements.Add(callStatement);
                continue;
            }

            statements.Add(new DslUnsupportedStatement("Inline statement not in Phase 2 subset", trimmed));
        }

        return statements;
    }

    private static bool TryParseDim(string line, out DslStatement statement)
    {
        statement = null!;
        if (!StartsWithKeyword(line, "Dim "))
            return false;

        var assign = DimAssignRegex.Match(line);
        if (assign.Success)
        {
            statement = new DslAssignStatement(assign.Groups[1].Value, assign.Groups[2].Value.Trim());
            return true;
        }

        var asNew = DimAsNewRegex.Match(line);
        if (asNew.Success)
        {
            statement = new DslAssignStatement(asNew.Groups[1].Value, $"New {asNew.Groups[2].Value.Trim()}");
            return true;
        }

        var asType = DimAsTypeRegex.Match(line);
        if (asType.Success)
        {
            statement = new DslAssignStatement(asType.Groups[1].Value, asType.Groups[2].Value.Trim());
            return true;
        }

        statement = new DslUnsupportedStatement("Dim declaration", line);
        return true;
    }

    private static bool TryParseCall(string line, IReadOnlySet<string> localFunctionNames, out DslStatement statement)
    {
        statement = null!;
        var trimmed = line.Trim();
        if (trimmed.StartsWith("Dim ", StringComparison.OrdinalIgnoreCase))
            return false;
        if (trimmed.Contains('='))
            return false;
        if (StartsWithKeyword(trimmed, "Select Case"))
            return false;
        if (!trimmed.Contains('('))
            return false;

        var open = trimmed.IndexOf('(');
        var close = trimmed.LastIndexOf(')');
        if (open < 0 || close <= open)
            return false;

        var callee = trimmed[..open].Trim();
        if (callee.StartsWith("Call ", StringComparison.OrdinalIgnoreCase))
            callee = callee[5..].Trim();

        var argsText = trimmed[(open + 1)..close];
        var args = SplitArguments(argsText);

        if (TryExtractFunctionName(callee, out var fnName) && localFunctionNames.Contains(fnName))
        {
            statement = new DslCallFunctionStatement(fnName, args);
            return true;
        }

        if (TryExtractBareFunctionName(callee, out var bareName)
            && (localFunctionNames.Contains(bareName)
                || Phase2SupportedFunctions.Contains(bareName)
                || SupportedDslFunctions.IsSupported(bareName)))
        {
            statement = new DslCallFunctionStatement(bareName, args);
            return true;
        }

        var dot = callee.LastIndexOf('.');
        if (dot >= 0)
        {
            statement = new DslCallOperationStatement(
                callee[(dot + 1)..],
                callee[..dot],
                args);
            return true;
        }

        statement = new DslCallOperationStatement(callee, null, args);
        return true;
    }

    private static bool TryExtractFunctionName(string callee, out string name)
    {
        name = callee;
        if (TryExtractBareFunctionName(callee, out name))
            return true;
        return false;
    }

    private static bool TryExtractBareFunctionName(string callee, out string name)
    {
        name = callee.Trim();
        if (name.EndsWith("()", StringComparison.Ordinal))
            name = name[..^2];
        return !string.IsNullOrWhiteSpace(name) && !name.Contains('.');
    }

    private static (string condition, string inlineBody)? SplitOnThen(string line)
    {
        var thenIndex = IndexOfThenKeyword(line);
        if (thenIndex < 0)
            return null;

        var conditionPart = line[..thenIndex].Trim();
        if (conditionPart.StartsWith("If ", StringComparison.OrdinalIgnoreCase))
            conditionPart = conditionPart[3..].Trim();
        else if (conditionPart.StartsWith("ElseIf ", StringComparison.OrdinalIgnoreCase))
            conditionPart = conditionPart[7..].Trim();

        var inline = line[(thenIndex + 4)..].Trim();
        return (conditionPart, inline);
    }

    private static int IndexOfThenKeyword(string line)
    {
        var idx = 0;
        while (idx < line.Length)
        {
            var found = line.IndexOf("Then", idx, StringComparison.OrdinalIgnoreCase);
            if (found < 0)
                return -1;

            var before = found > 0 ? line[found - 1] : ' ';
            var after = found + 4 < line.Length ? line[found + 4] : ' ';
            if (!char.IsLetterOrDigit(before) && !char.IsLetterOrDigit(after))
                return found;

            idx = found + 4;
        }

        return -1;
    }

    private static string? ExtractCatchVariable(string line)
    {
        var trimmed = line.Trim();
        if (!trimmed.StartsWith("Catch", StringComparison.OrdinalIgnoreCase))
            return null;

        var rest = trimmed[5..].Trim();
        if (string.IsNullOrWhiteSpace(rest))
            return null;

        var asIndex = rest.IndexOf(" As ", StringComparison.OrdinalIgnoreCase);
        return asIndex > 0 ? rest[..asIndex].Trim() : rest;
    }

    private static List<string> SplitArguments(string argsText)
    {
        var args = new List<string>();
        if (string.IsNullOrWhiteSpace(argsText))
            return args;

        var current = "";
        var depth = 0;
        foreach (var ch in argsText)
        {
            if (ch is '(' or '[')
                depth++;
            else if (ch is ')' or ']')
                depth--;

            if (ch == ',' && depth == 0)
            {
                args.Add(current.Trim());
                current = "";
                continue;
            }

            current += ch;
        }

        if (!string.IsNullOrWhiteSpace(current))
            args.Add(current.Trim());

        return args;
    }

    private static List<string> NormalizeLines(string body) =>
        body.Replace("\r\n", "\n").Split('\n')
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("'", StringComparison.Ordinal))
            .ToList();

    private static bool IsSkippable(string line) =>
        string.IsNullOrWhiteSpace(line)
        || line.StartsWith("'", StringComparison.Ordinal)
        || line.Equals("End Sub", StringComparison.OrdinalIgnoreCase);

    private static bool StartsWithKeyword(string line, string keyword) =>
        line.StartsWith(keyword, StringComparison.OrdinalIgnoreCase);
}
