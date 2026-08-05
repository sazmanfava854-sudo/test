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
    /// <summary>BillID همان DB (Ref2 در nosazo — بدون ادغام اجباری).</summary>
    public string BillIdRaw { get; set; } = "";
    public string PaymentIdRaw { get; set; } = "";
    public string BillId { get; set; } = "";
    public string PaymentId { get; set; } = "";
    public decimal Payable { get; set; }
    public Guid NidFiche { get; set; }
    public Guid? NidIncome { get; set; }
    public string BnkAcntNo { get; set; } = "";
    public string BnkAcntNoSource { get; set; } = "";
    public string? DutyRegion { get; set; }
    public int? ResolvedDistrictBranch { get; set; }
    public int? SuggestedFund { get; set; }
    public string? IncomeRegion { get; set; }
    public string? RefReconstructionNo { get; set; }
    public string PaymentBranch { get; set; } = "18";
    public string? BankCode { get; set; }
    public string RowDate { get; set; } = "";
    /// <summary>تاریخ سند — از فیش (PaymentDate / PrintDate / …).</summary>
    public string RayvarzDocDate { get; set; } = "";
    /// <summary>تاریخ عملیات — معمولاً BankPaymentDate.</summary>
    public string RayvarzActDate { get; set; } = "";
    /// <summary>سررسید ردیف درآمد (Due / RefRowDate).</summary>
    public string RayvarzDueDate { get; set; } = "";
    public int DocTyp { get; set; }
    public string DocDsc { get; set; } = "";
    public string? DocTypDsc { get; set; }
    public int? IncomeAccountGroup { get; set; }
    public int? DutyExportType { get; set; }
    public int CurrentStatus { get; set; }
    public bool ExistsInRayvarz { get; set; }
    public string StatusMessage { get; set; } = "";
    /// <summary>DocumentItem.Center — از Tahator1: Bank=2→CreditorPapers وگرنه 0.</summary>
    public long? Center { get; set; }
    /// <summary>Income_Fiche.Deposit → Center1 ردیف تهاتر.</summary>
    public long? Deposit { get; set; }
    /// <summary>Income_Fiche.DepositID → Ref ردیف تهاتر.</summary>
    public long? DepositId { get; set; }
    /// <summary>Income_Fiche.CreditorPapers — برای Center وقتی Bank=2.</summary>
    public long? CreditorPapers { get; set; }
    /// <summary>Income_Fiche.CheckNo — Center3: 5→700100002 وگرنه 700100001.</summary>
    public string? CheckNo { get; set; }
    public List<IncmRowDto> Rows { get; set; } = new();
}

public class RuleDslParsePreviewRequest
{
    public string? XmlBody { get; set; }
}

public class RulePromotionRollbackRequest
{
    public string? Reason { get; set; }
}

public class IncmRowDto
{
    public int IncmNo { get; set; }
    public string IncmRowDsc { get; set; } = "";
    public decimal Val { get; set; }
    /// <summary>DocumentItemIncm.Center1 (تهاتر: Deposit).</summary>
    public long? Center1 { get; set; }
    /// <summary>DocumentItemIncm.Center2.</summary>
    public long? Center2 { get; set; }
    /// <summary>DocumentItemIncm.Center3 (تهاتر: 700100001 / 700100002).</summary>
    public long? Center3 { get; set; }
    public string? Ref { get; set; }
    public string? Num { get; set; }
}

public class SendFicheRequest
{
    public FicheHeaderDto Fiche { get; set; } = new();
    public int Branch { get; set; }
    public int Fund { get; set; }
    public string DocDate { get; set; } = "";
    public string ActDate { get; set; } = "";
    public string DueDate { get; set; } = "";
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
    public string? ProxyMode { get; set; }
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
    /// <summary>مثلاً WSDL با 502 ولی مسیر HTTP تا MSB باز است.</summary>
    public string? Warning { get; set; }
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

/// <summary>نگه‌داشت فیلدهای Income_Fiche قبل از تریگر تهاتر (وضعیت ۲) — پایدار در RayvarzRuleEngine.</summary>
public class IncomeFicheTahatorSnapshot
{
    public long SnapshotId { get; set; }
    public string FicheNo { get; set; } = "";
    public int EumFicheStatus { get; set; }
    public string? ExportPermanentDate { get; set; }
    public string? PaymentBreakDate { get; set; }
    public string? PaymentDate { get; set; }
    public string? UserConfirmDate { get; set; }
    public string? UsernameUserConfirm { get; set; }
    public Guid? NidUserUserConfirm { get; set; }
    public string? TriggerDate { get; set; }
    public string? PersistStatus { get; set; }
    public DateTime? CreatedAtUtc { get; set; }
}

public class TahatorFicheRequest
{
    public string FicheNo { get; set; } = "";
    public int Branch { get; set; }
    public int Fund { get; set; }
    public string DocDate { get; set; } = "";
    public string ActDate { get; set; } = "";
    public string DueDate { get; set; } = "";
    /// <summary>اگر فیش در رایورز باشد هم ادامه بده (برای تست مرحله وضعیت ۲).</summary>
    public bool Force { get; set; }
    /// <summary>
    /// فقط مرحله نگه‌داشت + UPDATE وضعیت ۲ (تاریخ روز) — بدون SOAP و بدون بازگردانی.
    /// بعد از SELECT در Sara، با POST /api/tahator/restore بازگردانید.
    /// </summary>
    public bool HoldAfterStatus2 { get; set; }
}

/// <summary>جفت تهاتر — گروه ۱۵۷ (مبلغ/مرکز) + ۱۵۸ (درآمد/منطقه) با همان NidIncome.</summary>
public class TahatorPairInfo
{
    public Guid NidIncome { get; set; }
    public string AmountFicheNo { get; set; } = "";
    public string IncomeFicheNo { get; set; } = "";
    public FicheHeaderDto? AmountFiche { get; set; }
    public FicheHeaderDto? IncomeFiche { get; set; }
}

/// <summary>وضعیت هر فیش جفت در check/send.</summary>
public class TahatorPairMemberStatus
{
    public string FicheNo { get; set; } = "";
    public int IncomeAccountGroup { get; set; }
    public int DocTyp { get; set; }
    public int Branch { get; set; }
    public int Fund { get; set; }
    public bool ExistsInAccountingDocHeader { get; set; }
    public bool ExistsInRayvarz { get; set; }
    public bool NeedsSend { get; set; }
    public string? DocNotSentError { get; set; }
}

public class TahatorCheckResult
{
    public string FicheNo { get; set; } = "";
    public bool ExistsInAccountingDocHeader { get; set; }
    public bool ExistsInIncomeFiche { get; set; }
    public bool ExistsInRayvarz { get; set; }
    public bool NeedsSend { get; set; }
    public string Message { get; set; } = "";
    public string? DocNotSentError { get; set; }
    public IncomeFicheTahatorSnapshot? Snapshot { get; set; }
    /// <summary>اگر فرایند قبلی قطع شده باشد، snapshot Pending از RayvarzRuleEngine.</summary>
    public IncomeFicheTahatorSnapshot? PendingStoredSnapshot { get; set; }
    public FicheHeaderDto? Fiche { get; set; }
    /// <summary>جفت ۱۵۷+۱۵۸ — هر دو باید ارسال شوند.</summary>
    public TahatorPairInfo? Pair { get; set; }
    public List<TahatorPairMemberStatus> PairMembers { get; set; } = new();
}

public class TahatorFicheSendDetail
{
    public string FicheNo { get; set; } = "";
    public int IncomeAccountGroup { get; set; }
    public int DocTyp { get; set; }
    public int Branch { get; set; }
    public int Fund { get; set; }
    public bool Success { get; set; }
    public bool Skipped { get; set; }
    public string? SkipReason { get; set; }
    public bool ExistsInAccountingDocHeaderAfter { get; set; }
    public bool ExistsInRayvarz { get; set; }
    public string? SoapMessage { get; set; }
    public string? PursuitDocNo { get; set; }
    public string? PreviewXml { get; set; }
    public string? DocNotSentError { get; set; }
}

public class TahatorSendResult
{
    public bool Success { get; set; }
    public bool Skipped { get; set; }
    public bool DryRun { get; set; }
    public string FicheNo { get; set; } = "";
    public string Message { get; set; } = "";
    /// <summary>InDocHeader | InRayvarz | null</summary>
    public string? SkipReason { get; set; }
    public bool ExistsInAccountingDocHeaderBefore { get; set; }
    public bool ExistsInAccountingDocHeaderAfter { get; set; }
    public bool ExistsInRayvarz { get; set; }
    public string? TriggerDate { get; set; }
    public long? SnapshotId { get; set; }
    public string? DocNotSentError { get; set; }
    public IncomeFicheTahatorSnapshot? Snapshot { get; set; }
    public string? EngineName { get; set; }
    public int DocTyp { get; set; }
    public int Branch { get; set; }
    public int Fund { get; set; }
    public string? PreviewXml { get; set; }
    public string? SoapResponse { get; set; }
    public string? PursuitDocNo { get; set; }
    public string? SoapMessage { get; set; }
    public List<string> Steps { get; set; } = new();
    public TahatorPairInfo? Pair { get; set; }
    public List<TahatorFicheSendDetail> FicheResults { get; set; } = new();
}
