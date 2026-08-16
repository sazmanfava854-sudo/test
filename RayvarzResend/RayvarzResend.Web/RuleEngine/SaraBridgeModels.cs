namespace RayvarzResend.Web.RuleEngine;

/// <summary>قرارداد SaraBridge — POST /rayvarz/build-save-document</summary>
public sealed class SaraBridgeBuildRequest
{
    public int NidMember { get; set; } = 1388;
    public Guid NidFiche { get; set; }
    public string FicheNo { get; set; } = "";
    public string Category { get; set; } = "";
    public int Branch { get; set; }
    public int Fund { get; set; }
    public string? DocDate { get; set; }
    public string? ActDate { get; set; }
    public string? DueDate { get; set; }
}

public sealed class SaraBridgeBuildResponse
{
    public string? SoapXml { get; set; }
    public string? Error { get; set; }
    public string Source { get; set; } = "LocalStub";
    public string? Warning { get; set; }
}
