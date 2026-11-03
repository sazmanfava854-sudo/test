using Xunit;

namespace RayvarzResend.Tests;

public class UnsentBatchChunkTests
{
    private static string WebFile(params string[] parts) =>
        Path.GetFullPath(Path.Combine(
            new[] { AppContext.BaseDirectory, "..", "..", "..", "..", "RayvarzResend.Web" }.Concat(parts).ToArray()));

    [Fact]
    public void Config_exposes_unsent_batch_chunk_size()
    {
        var program = File.ReadAllText(WebFile("Program.cs"));
        Assert.Contains("UnsentBatchChunkSize", program);
        Assert.Contains("unsentBatch = new", program);

        var json = File.ReadAllText(WebFile("appsettings.json"));
        Assert.Contains("\"UnsentBatchChunkSize\"", json);
    }

    [Fact]
    public void Frontend_sends_unsent_batch_in_chunks()
    {
        var js = File.ReadAllText(WebFile("wwwroot", "js", "app.js"));
        Assert.Contains("function sendUnsentBatchInChunks(", js);
        Assert.Contains("function mergeUnsentBatchSendResults(", js);
        Assert.Contains("resolveUnsentBatchChunkSize", js);
        Assert.DoesNotContain("body: JSON.stringify({ targets })", js.Split("bindClick('btnUnsentSend'")[1].Split("bindClick('btnCheckDateSearch'")[0]);
    }
}
