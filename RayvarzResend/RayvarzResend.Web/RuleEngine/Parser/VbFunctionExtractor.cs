using System.Text.RegularExpressions;

namespace RayvarzResend.Web.RuleEngine.Parser;

public sealed class VbFunctionInfo
{
    public string Name { get; init; } = "";
    public string? DisplayName { get; init; }
    public string Body { get; init; } = "";
}

internal static class VbFunctionExtractor
{
    private static readonly Regex FunctionStartRegex = new(
        @"(?:<DisplayName\(""([^""]*)""\)>?\s*)?Public\s+Function\s+(\w+)\s*\(",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static IReadOnlyList<VbFunctionInfo> Extract(string bodySource)
    {
        if (string.IsNullOrWhiteSpace(bodySource))
            return Array.Empty<VbFunctionInfo>();

        var functions = new List<VbFunctionInfo>();
        foreach (Match match in FunctionStartRegex.Matches(bodySource))
        {
            var displayName = match.Groups[1].Success ? match.Groups[1].Value : null;
            var name = match.Groups[2].Value;
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
                Body = innerBody
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
