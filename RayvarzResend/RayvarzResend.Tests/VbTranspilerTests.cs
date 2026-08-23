using Xunit;
using RayvarzResend.Web.RuleEngine.Parser;

namespace RayvarzResend.Tests;

public class VbTranspilerTests
{
    [Fact]
    public void Parse_fixture_extracts_Run_and_Nosazi_dispatch()
    {
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "member-1388-sample.xml");
        Assert.True(File.Exists(fixturePath), $"Fixture not found: {fixturePath}");

        var xml = File.ReadAllText(fixturePath);
        var envelope = XmlEnvelopeReader.Read(xml, "fixture");
        var program = VbTranspiler.Transpile(envelope.Document);

        Assert.True(program.HasEntryPoint);
        Assert.True(program.HasNosazi);
        Assert.Contains(program.UnsupportedFunctions, f => f.Equals("iNcOME", StringComparison.OrdinalIgnoreCase));

        var run = program.Functions.First(f => f.Name.Equals("Run", StringComparison.OrdinalIgnoreCase));
        Assert.True(run.IsSupported);
        Assert.Contains(run.Body, s => s is DslIfStatement);

        var nosazi = program.Functions.First(f => f.Name.Equals("Nosazi", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(nosazi.Body, s => s is DslAssignStatement a
            && a.Expression.Contains("GetAccountingDocCreateParameter", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Parse_fixture_produces_stable_xml_hash()
    {
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "member-1388-sample.xml");
        var xml = File.ReadAllText(fixturePath);
        var first = XmlEnvelopeReader.Read(xml, "fixture");
        var second = XmlEnvelopeReader.Read(xml, "fixture");
        Assert.Equal(first.XmlHash, second.XmlHash);
        Assert.Equal(64, first.XmlHash.Length);
    }
}
