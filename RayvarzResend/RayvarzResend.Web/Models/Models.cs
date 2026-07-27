namespace RayvarzResend.Web.Models;

public enum FicheCategory
{
    Unknown,
    Income,
    DutyNosazi,
    DutySenfi
}

public enum IdentifierType
{
    FicheNo,
    BillPaymentKey
}

public class LoadFicheRequest
{
    public IdentifierType IdentifierType { get; set; }
    public string IdentifierValue { get; set; } = "";
    public int Branch { get; set; }
    public string DocDate { get; set; } = ""; // 1405/03/23 or 14050323
}

public class FicheHeaderDto
{
    public FicheCategory Category { get; set; }
    public string FicheNo { get; set; } = "";
    public string BillId { get; set; } = "";
    public string PaymentId { get; set; } = "";
    public decimal Payable { get; set; }
    public Guid NidFiche { get; set; }
    public Guid? NidIncome { get; set; }
    public string BnkAcntNo { get; set; } = "";
    public string BnkAcntNoSource { get; set; } = "";
    public string? DutyRegion { get; set; }
    public string? IncomeRegion { get; set; }
    public string? RefReconstructionNo { get; set; }
    public string PaymentBranch { get; set; } = "18";
    public string? BankCode { get; set; }
    public string RowDate { get; set; } = "";
    public int DocTyp { get; set; }
    public string DocDsc { get; set; } = "";
    public int? IncomeAccountGroup { get; set; }
    public int? DutyExportType { get; set; }
    public int CurrentStatus { get; set; }
    public bool ExistsInRayvarz { get; set; }
    public string StatusMessage { get; set; } = "";
    public List<IncmRowDto> Rows { get; set; } = new();
}

public class IncmRowDto
{
    public int IncmNo { get; set; }
    public string IncmRowDsc { get; set; } = "";
    public decimal Val { get; set; }
}

public class SendFicheRequest
{
    public FicheHeaderDto Fiche { get; set; } = new();
    public int Branch { get; set; }
    public int Fund { get; set; }
    public string DocDate { get; set; } = "";
    public bool ResetStatus { get; set; } = true;
}

public class SendResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string? PursuitDocNo { get; set; }
    public string? SoapResponse { get; set; }
    public string? PreviewXml { get; set; }
    public bool DryRun { get; set; }
    public bool VerifiedInRayvarz { get; set; }
    public string? DocNotSentError { get; set; }
    public string? Warning { get; set; }
    public RayvarzTransportDiagnostics? Diagnostics { get; set; }
}

/// <summary>جزئیات فنی ارسال/اتصال برای UI و لاگ (بدون ذخیره کل XML در لاگ فایل).</summary>
public class RayvarzTransportDiagnostics
{
    public string Category { get; set; } = "";
    public string Stage { get; set; } = "";
    public long ElapsedMs { get; set; }
    public string? PostUrl { get; set; }
    public string? WsAddressingTo { get; set; }
    public string? SoapAction { get; set; }
    public string? ContentType { get; set; }
    public int RequestBodyBytes { get; set; }
    public bool HasWsAddressingHeader { get; set; }
    public string? EnvelopeStyle { get; set; }
    public int? HttpStatusCode { get; set; }
    public int? ResponseBodyBytes { get; set; }
    public List<string> ExceptionChain { get; set; } = new();
    public string? LikelyCause { get; set; }
    public string? Hint { get; set; }
}

public class RayvarzPingResultDto
{
    public bool Ok { get; set; }
    public string Url { get; set; } = "";
    public int? StatusCode { get; set; }
    public long ElapsedMs { get; set; }
    public string? BodyPreview { get; set; }
    public bool AllowInvalidSsl { get; set; }
    public string? Error { get; set; }
    public string? Inner { get; set; }
    public string? Hint { get; set; }
    public RayvarzTransportDiagnostics? Diagnostics { get; set; }
}

public class AppConfig
{
    public string ServiceUrl { get; set; } = "";
    public string SoapAction { get; set; } = "";
    public string? SourceSystemId { get; set; }
    public bool DryRun { get; set; }
    public int SendDelayMs { get; set; } = 2000;
}
