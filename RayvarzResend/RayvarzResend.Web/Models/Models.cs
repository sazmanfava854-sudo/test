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
}

public class AppConfig
{
    public string ServiceUrl { get; set; } = "";
    public string SoapAction { get; set; } = "";
    public string SourceSystemId { get; set; } = "11111";
    public bool DryRun { get; set; }
    public int SendDelayMs { get; set; } = 2000;
}
