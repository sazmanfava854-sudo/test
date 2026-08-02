using System.Text.RegularExpressions;

namespace RayvarzResend.Web.RuleEngine.Parser;

public sealed class VbFunctionInfo
{
    public string Name { get; init; } = "";
    public string? DisplayName { get; init; }
    public string Body { get; init; } = "";
    public bool IsPrivate { get; init; }
}

internal static class VbFunctionExtractor
{
    /// <summary>Public / Private / Function بدون modifier — همه توابع Member.</summary>
    private static readonly Regex FunctionStartRegex = new(
        @"(?:<DisplayName\(""([^""]*)""\)>?\s*)?(?:(Public|Private)\s+)?Function\s+(\w+)\s*\(",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static IReadOnlyList<VbFunctionInfo> Extract(string bodySource)
    {
        if (string.IsNullOrWhiteSpace(bodySource))
            return Array.Empty<VbFunctionInfo>();

        var functions = new List<VbFunctionInfo>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in FunctionStartRegex.Matches(bodySource))
        {
            // رد کردن "End Function" اگر به‌اشتباه match شود
            var prefixStart = Math.Max(0, match.Index - 4);
            var prefix = bodySource[prefixStart..match.Index];
            if (prefix.EndsWith("End ", StringComparison.OrdinalIgnoreCase)
                || prefix.EndsWith("Exit ", StringComparison.OrdinalIgnoreCase))
                continue;

            var displayName = match.Groups[1].Success && match.Groups[1].Length > 0
                ? match.Groups[1].Value
                : null;
            var modifier = match.Groups[2].Success ? match.Groups[2].Value : "";
            var name = match.Groups[3].Value;
            if (!seen.Add(name))
                continue;

            var bodyStart = match.Index + match.Length;
            var endFunctionIndex = FindEndFunction(bodySource, bodyStart);
            if (endFunctionIndex < 0)
                continue;

            var signatureRemainder = bodySource[bodyStart..endFunctionIndex];
            var innerBody = TrimFunctionInnerBody(signatureRemainder);
            functions.Add(new VbFunctionInfo
            {
                Name = name,
                DisplayName = displayName,
                Body = innerBody,
                IsPrivate = modifier.Equals("Private", StringComparison.OrdinalIgnoreCase)
            });
        }

        return functions;
    }

    private static int FindEndFunction(string source, int fromIndex)
    {
        var searchFrom = fromIndex;
        while (searchFrom < source.Length)
        {
            var idx = source.IndexOf("End Function", searchFrom, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
                return -1;
            return idx;
        }

        return -1;
    }

    private static string TrimFunctionInnerBody(string signatureAndBody)
    {
        var lines = signatureAndBody.Replace("\r\n", "\n").Split('\n');
        var bodyLines = new List<string>();
        var started = false;

        foreach (var raw in lines)
        {
            var line = raw.TrimEnd();
            if (!started)
            {
                if (line.Trim().Equals(")", StringComparison.Ordinal) || line.Contains(')'))
                    started = true;
                continue;
            }

            var trimmed = line.Trim();
            if (trimmed.Equals("End Function", StringComparison.OrdinalIgnoreCase))
                break;

            bodyLines.Add(line);
        }

        return string.Join(Environment.NewLine, bodyLines).Trim();
    }
}
