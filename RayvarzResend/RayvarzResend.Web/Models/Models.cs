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
    /// <summary>درآمد یا نوسازی/صنفی — جدول جستجو را محدود می‌کند.</summary>
    public UnsentFicheKind? FicheKind { get; set; }
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
    /// <summary>پرونده درآمد — برای BedeHi و فیش قبلی همان NidProc.</summary>
    public Guid? NidProc { get; set; }
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
    /// <summary>Income_Fiche.Deposit — Center1 / Num در Member 1388.</summary>
    public long? Deposit { get; set; }
    /// <summary>Income_Fiche.DepositID — Ref ردیف.</summary>
    public long? DepositId { get; set; }
    /// <summary>Income_Fiche.CreditorPapers — Center وقتی Bank=2.</summary>
    public long? CreditorPapers { get; set; }
    /// <summary>Income_Fiche.CheckNo — Center3 تهاتر.</summary>
    public string? CheckNo { get; set; }
    /// <summary>Center پیشنهادی Member 1388.</summary>
    public long? Center { get; set; }
    /// <summary>کارمزد کارگزار — BedeHi: Payable-Brokers.</summary>
    public decimal Brokers { get; set; }
    /// <summary>بدهی قبلی محاسبه‌شده یا از DB.</summary>
    public decimal? PriorBedeHiAmount { get; set; }
    /// <summary>فیش درآمد قبلی همان پرونده — ورودی BedeHi.</summary>
    public PriorIncomeFicheDto? PriorIncomeFiche { get; set; }
    public int? DutyExportType { get; set; }
    public int CurrentStatus { get; set; }
    public bool ExistsInRayvarz { get; set; }
    /// <summary>آیا فیش از نظر اعتبارسنجی قابل ارسال است.</summary>
    public bool CanSend { get; set; }
    /// <summary>دلیل عدم ارسال — برای نمایش در UI.</summary>
    public string? BlockReason { get; set; }
    public string StatusMessage { get; set; } = "";
    /// <summary>هشدار بارگذاری (مثلاً خطای SQL در GetNosaziNickName) — ارسال ممکن است ادامه یابد.</summary>
    public string? Warning { get; set; }
    /// <summary>رندمان Income_OddmentAccount — اگر خالی باشد Oddment اعمال نمی‌شود.</summary>
    public List<IncomeOddmentDto> Oddments { get; set; } = new();
    /// <summary>رندمان Duty_OddmentAccount — از NidFK فیش نوسازی.</summary>
    public List<DutyOddmentDto> DutyOddments { get; set; } = new();
    public List<IncmRowDto> Rows { get; set; } = new();
}

/// <summary>فیش درآمد قبلی — VB M_AllFiche برای BedeHi.</summary>
public class PriorIncomeFicheDto
{
    public Guid NidIncome { get; set; }
    public string FicheNo { get; set; } = "";
    public decimal Payable { get; set; }
    public decimal Brokers { get; set; }
    public string PaymentDate { get; set; } = "";
    public string BankPaymentDate { get; set; } = "";
    public int IncomeAccountGroup { get; set; }
    public int? FicheStatus { get; set; }
    public List<IncmRowDto> CalculationRows { get; set; } = new();
}

public class IncmRowDto
{
    public int IncmNo { get; set; }
    public string IncmRowDsc { get; set; } = "";
    public decimal Val { get; set; }
    public long? Center1 { get; set; }
    public long? Center2 { get; set; }
    public long? Center3 { get; set; }
    public string? Ref { get; set; }
    public string? Num { get; set; }
}

/// <summary>رندمان Income_OddmentAccount — VB LstOdd / LstOdd_1.</summary>
public class IncomeOddmentDto
{
    public int IncmNo { get; set; }
    public decimal Value { get; set; }
    public int OddmentType { get; set; }
}

/// <summary>رندمان dbo.Duty_OddmentAccount — از NidFK مرتبط با Duty_FicheSub.</summary>
public class DutyOddmentDto
{
    public int DutyFormula { get; set; }
    /// <summary>معادل CI_DutyFormulaFiche — از CI_DutyOddmentFor در DB.</summary>
    public int DutyFormulaFiche { get; set; }
    public decimal Price { get; set; }
    public int OddmentType { get; set; }
    public string? FicheNo { get; set; }
    public int? DutyYear { get; set; }
}

public class SendFicheRequest
{
    public FicheHeaderDto Fiche { get; set; } = new();
    public int Branch { get; set; }
    public int Fund { get; set; }
    public string DocDate { get; set; } = "";
    public string ActDate { get; set; } = "";
    public string DueDate { get; set; } = "";
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

public enum UnsentFicheKind
{
    /// <summary>شهرسازی — Income_Fiche</summary>
    Income,
    /// <summary>نوسازی و صنفی — Duty_Fiche</summary>
    Duty
}

/// <summary>جستجوی فیش‌های تاییدشده که در Accounting_DocHeader نیستند.</summary>
public class UnsentFicheSearchRequest
{
    public UnsentFicheKind FicheKind { get; set; } = UnsentFicheKind.Income;
    public string? FicheNo { get; set; }
    public string? FromDate { get; set; }
    public string? ToDate { get; set; }
    public string? BillId { get; set; }
    public string? PaymentId { get; set; }
    /// <summary>منطقه (۱–۱۲ یا ۲۱۸)</summary>
    public string? District { get; set; }
    public int MaxResults { get; set; } = 500;

    public bool HasDateRange =>
        !string.IsNullOrWhiteSpace(FromDate) && !string.IsNullOrWhiteSpace(ToDate);

    public bool HasPartialDateRange =>
        !string.IsNullOrWhiteSpace(FromDate) != !string.IsNullOrWhiteSpace(ToDate);

    /// <summary>حداقل یک فیلتر برای جلوگیری از اسکن کل جدول.</summary>
    public bool HasAnyFilter =>
        !string.IsNullOrWhiteSpace(FicheNo) ||
        !string.IsNullOrWhiteSpace(BillId) ||
        !string.IsNullOrWhiteSpace(PaymentId) ||
        !string.IsNullOrWhiteSpace(District) ||
        HasDateRange;
}

public class UnsentFicheListItem
{
    public string FicheNo { get; set; } = "";
    public Guid NidFiche { get; set; }
    public string BnkAcntNo { get; set; } = "";
    public string BillId { get; set; } = "";
    public string PaymentId { get; set; } = "";
    public string PaymentDate { get; set; } = "";
    public string BankPaymentDate { get; set; } = "";
    public decimal Payable { get; set; }
    public int Status { get; set; }
    public string? District { get; set; }
    public string? DocNotSentError { get; set; }
    public int? IncomeAccountGroup { get; set; }
    public bool IsTahator { get; set; }
    public string SubKindLabel { get; set; } = "";
}

public class UnsentBatchPlanItem
{
    public string FicheNo { get; set; } = "";
    public string SendPath { get; set; } = "";
    public string Detail { get; set; } = "";
    public bool CanSend { get; set; }
    public string? BlockReason { get; set; }
    public string? TahatorPairFicheNo { get; set; }
}

public class UnsentBatchPlanResult
{
    public int Total { get; set; }
    public List<UnsentBatchPlanItem> Items { get; set; } = new();
}

public class UnsentFicheSearchResult
{
    public UnsentFicheKind FicheKind { get; set; }
    public int Count { get; set; }
    public bool Truncated { get; set; }
    public List<UnsentFicheListItem> Items { get; set; } = new();
}

public class UnsentBatchSendRequest
{
    public UnsentFicheKind FicheKind { get; set; } = UnsentFicheKind.Income;
    public List<string> FicheNos { get; set; } = new();
}

public class UnsentBatchSendItemResult
{
    public string FicheNo { get; set; } = "";
    public string SendPath { get; set; } = "";
    public bool Success { get; set; }
    public bool Skipped { get; set; }
    public string Message { get; set; } = "";
    public string? SkipReason { get; set; }
    public bool VerifiedInRayvarz { get; set; }
    public string? DocNotSentError { get; set; }
}

public class UnsentBatchSendResult
{
    public int Total { get; set; }
    public int Succeeded { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
    public bool DryRun { get; set; }
    public List<UnsentBatchSendItemResult> Results { get; set; } = new();
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
    /// <summary>اگر فیش در رایورز باشد هم ادامه بده.</summary>
    public bool Force { get; set; }
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
    public string? Warning { get; set; }
    public string? DocNotSentError { get; set; }
    public IncomeFicheTahatorSnapshot? Snapshot { get; set; }
    public IncomeFicheTahatorSnapshot? PendingStoredSnapshot { get; set; }
    public FicheHeaderDto? Fiche { get; set; }
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

public enum InstallmentLookupKind
{
    /// <summary>جستجو/آپدیت با NoDocument</summary>
    NoDocument,
    /// <summary>جستجو/آپدیت با TrackingNo — EndState اختیاری</summary>
    TrackingNo
}

/// <summary>ردیف اکسل — سه ستون: Identifier (شماره سند یا کد پیگیری), PaymentCost, PaymentDate.</summary>
public class InstallmentExcelRowInput
{
    /// <summary>شماره سند یا کد پیگیری — نوع از طول رقم تشخیص داده می‌شود.</summary>
    public string Identifier { get; set; } = "";
    public string PaymentCost { get; set; } = "";
    public string PaymentDate { get; set; } = "";
}

/// <summary>تب تغییر وضعیت چک به خزانه — dbo.Installment_List (بدون رایورز).</summary>
public class InstallmentCheckRequest
{
    /// <summary>لیست شماره‌ها — در UI به‌صورت متن چندخطی/با کاما. نوع (NoDocument/TrackingNo) خودکار تشخیص داده می‌شود.</summary>
    public string ValuesText { get; set; } = "";
    /// <summary>ردیف‌های اکسل — وقتی پر باشد حالت دسته‌ای فعال می‌شود.</summary>
    public List<InstallmentExcelRowInput>? ExcelRows { get; set; }
    /// <summary>از لاگین — در API پر می‌شود.</summary>
    public string PerformedByUser { get; set; } = "";
    /// <summary>فقط وقتی TrackingNo تشخیص داده شود — اعمال EndStateDesc/EndStateCode عودت.</summary>
    public bool ApplyEndState { get; set; }
}

public class InstallmentCheckPreviewItem
{
    public int RowIndex { get; set; }
    public string LookupValue { get; set; } = "";
    public InstallmentLookupKind DetectedLookupKind { get; set; }
    public bool Found { get; set; }
    public bool DataMatches { get; set; }
    public string? ValidationMessage { get; set; }
    public long? NidInstallmentList { get; set; }
    public string NoDocument { get; set; } = "";
    public string TrackingNo { get; set; } = "";
    public string? PaymentCost { get; set; }
    public string? PaymentDate { get; set; }
    public string? ExcelIdentifier { get; set; }
    public string? ExcelPaymentCost { get; set; }
    public string? ExcelPaymentDate { get; set; }
    public string CI_InstallmentStatus { get; set; } = "";
    public string EndStateDesc { get; set; } = "";
    public string EndStateCode { get; set; } = "";
    public string Comments { get; set; } = "";
    public string ProposedComments { get; set; } = "";
    public string ProposedCI_InstallmentStatus { get; set; } = "";
    public string ProposedEndStateDesc { get; set; } = "";
    public string ProposedEndStateCode { get; set; } = "";
}

public class InstallmentCheckPreviewResult
{
    public bool ExcelMode { get; set; }
    public bool ApplyEndState { get; set; }
    public int FoundCount { get; set; }
    public int NotFoundCount { get; set; }
    public int MatchedCount { get; set; }
    public int MismatchCount { get; set; }
    public string? Error { get; set; }
    public List<InstallmentCheckPreviewItem> Items { get; set; } = new();
}

public class InstallmentCheckUpdateItemResult
{
    public string LookupValue { get; set; } = "";
    public InstallmentLookupKind DetectedLookupKind { get; set; }
    public bool Success { get; set; }
    public bool Found { get; set; }
    public int RowsAffected { get; set; }
    /// <summary>در DryRun — تعداد ردیف‌هایی که UPDATE می‌شدند.</summary>
    public int WouldUpdate { get; set; }
    public string? Message { get; set; }
}

public class InstallmentCheckUpdateResult
{
    public bool ExcelMode { get; set; }
    public bool ApplyEndState { get; set; }
    public bool DryRun { get; set; }
    public int Total { get; set; }
    public int Updated { get; set; }
    /// <summary>در DryRun — جمع ردیف‌هایی که UPDATE می‌شدند.</summary>
    public int WouldUpdate { get; set; }
    public int NotFound { get; set; }
    public int Failed { get; set; }
    public int SkippedMismatch { get; set; }
    public string? Error { get; set; }
    public List<InstallmentCheckUpdateItemResult> Results { get; set; } = new();
}

public class TahatorSendResult
{
    public bool Success { get; set; }
    public bool Skipped { get; set; }
    public bool DryRun { get; set; }
    public string FicheNo { get; set; } = "";
    public string Message { get; set; } = "";
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
