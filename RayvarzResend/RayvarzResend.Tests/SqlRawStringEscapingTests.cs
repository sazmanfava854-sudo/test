using System.Reflection;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

/// <summary>
/// در رشتهٔ خام C# (""") دو گیومه escape نیست و همان "" به SQL می‌رود؛
/// XQuery با Subject=""…"" خطای Syntax error near 'کد' می‌دهد.
/// </summary>
public class SqlRawStringEscapingTests
{
    [Fact]
    public void FicheRepository_sql_constants_have_no_doubled_quotes_in_xquery()
    {
        var constants = typeof(FicheRepository)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => (f.Name, Value: (string)f.GetRawConstantValue()!))
            .ToList();

        Assert.Contains(constants, c => c.Name == "DutyBnkAcntNoSelect");
        foreach (var (name, value) in constants)
            Assert.False(value.Contains("Subject=\"\"", StringComparison.Ordinal), name);
    }

    [Fact]
    public void Duty_bnk_acnt_no_select_uses_single_quoted_xquery_subjects()
    {
        var value = (string)typeof(FicheRepository)
            .GetField("DutyBnkAcntNoSelect", BindingFlags.NonPublic | BindingFlags.Static)!
            .GetRawConstantValue()!;

        Assert.Contains("ClsLog[Subject=\"کد نوسازی\"]", value);
        Assert.Contains("ClsLog[Subject=\"منطقه\"]", value);
    }

    [Fact]
    public void Source_has_no_doubled_xquery_quotes_inside_raw_strings()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..",
            "RayvarzResend.Web", "Services", "FicheRepository.cs"));
        var lines = File.ReadAllLines(path);

        var inRaw = false;
        var offenders = new List<int>();
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var rawDelimiters = CountOccurrences(line, "\"\"\"");
            if (inRaw && line.Contains("Subject=\"\"", StringComparison.Ordinal))
                offenders.Add(i + 1);
            if (rawDelimiters % 2 == 1)
                inRaw = !inRaw;
        }

        Assert.True(offenders.Count == 0, "خطوط مشکل‌دار: " + string.Join(", ", offenders));
    }

    private static int CountOccurrences(string text, string token)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += token.Length;
        }
        return count;
    }
}
