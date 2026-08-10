using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Web.Validation;

/// <summary>
/// اعتبارسنج مرکزی payload مالی — قواعد مستند RayReceiveIncmVchr + قواعد مالی قبل از ارسال.
/// فقط موارد حیاتی Blocking هستند؛ warningهای غیرحیاتی ارسال را متوقف نمی‌کنند.
/// </summary>
public sealed class RayvarzSoapPayloadValidator
{
    private readonly List<RayvarzValidationIssue> _issues = new();

    public RayvarzValidationResult Validate(RayvarzValidationInput input)
    {
        _issues.Clear();
        var fiche = input.Fiche;

        ValidatePreSendBusinessRules(input);
        ValidateFicheModel(fiche, input.Branch, input.Fund);
        ValidateFinancialBalances(fiche);

        if (!string.IsNullOrWhiteSpace(input.SoapXml))
        {
            var parsed = RayvarzSoapXmlInspector.TryParse(input.SoapXml);
            if (parsed is null)
            {
                Critical(RayvarzFieldRequirementCatalog.OpSave, "SoapXml", "SOAP_PARSE_FAILED",
                    "ساختار XML SaveDocument قابل parse نیست");
            }
            else
            {
                ValidateHeader(parsed);
                ValidateDocumentItem(parsed, fiche, input.Fund);
                ValidateIncmRows(parsed, fiche);
                ValidateFinancialFromSoap(parsed, fiche);
            }
        }

        ValidateCompatibilityWarnings(input);
        ValidatePreSoapErrors(input);

        return new RayvarzValidationResult { Issues = _issues.ToList() };
    }

    private void ValidatePreSendBusinessRules(RayvarzValidationInput input)
    {
        var fiche = input.Fiche;

        if (input.ExistsInRayvarz || fiche.ExistsInRayvarz)
            Critical(RayvarzFieldRequirementCatalog.OpPreSend, "ExistsInRayvarz", "BIZ_DUPLICATE_IN_RAYVARZ",
                "فیش در رایورز موجود است — ارسال نشد");

        if (TahatorRowBuilder.IsTahatorFiche(fiche))
            Critical(RayvarzFieldRequirementCatalog.OpPreSend, "Category", "BIZ_TAHATOR_WRONG_PATH",
                "فیش تهاتر — از مسیر تهاتر ارسال کنید");

        if (!fiche.CanSend && !string.IsNullOrWhiteSpace(fiche.BlockReason))
            Critical(RayvarzFieldRequirementCatalog.OpPreSend, "CanSend", "BIZ_INCOMECHECK_BLOCKED",
                fiche.BlockReason);
    }

    private void ValidateFicheModel(FicheHeaderDto fiche, int branch, int fund)
    {
        if (fiche.Payable <= 0)
            Critical(RayvarzFieldRequirementCatalog.OpPreSend, "Payable", "BIZ_PAYABLE_ZERO",
                "مبلغ قابل پرداخت صفر است");

        if (fiche.Rows.Count == 0)
            Critical(RayvarzFieldRequirementCatalog.OpPreSend, "Rows", "BIZ_NO_ROWS",
                "ردیف IncmNo یافت نشد");

        if (!TahatorRowBuilder.IsTahatorFiche(fiche)
            && !FicheBranchResolver.TryResolve(fiche, out _, out _, out var branchError)
            && branch <= 0)
        {
            Critical(RayvarzFieldRequirementCatalog.OpPreSend, "Branch", "BIZ_BRANCH_UNRESOLVED",
                branchError ?? "شعبه/منطقه resolve نشد");
        }

        if (RayvarzFieldRequirementCatalog.RequiresFund(fiche)
            && fund <= 0
            && fiche.SuggestedFund is not > 0)
        {
            Critical(RayvarzFieldRequirementCatalog.OpDocItem, "Fund", "ITEM_FUND_REQUIRED",
                "منبع (Fund) برای این نوع فیش الزامی است");
        }
    }

    private void ValidateFinancialBalances(FicheHeaderDto fiche)
    {
        var rowSum = fiche.Rows.Sum(r => r.Val);
        if (!TahatorRowBuilder.RowSumMatchesPayable(fiche, rowSum))
        {
            Critical(RayvarzFieldRequirementCatalog.OpIncm, "Val", "FIN_ROW_SUM_PAYABLE",
                $"جمع ردیف‌ها ({rowSum}) با Payable ({fiche.Payable}) مطابقت ندارد");
        }

        EnforceDebitCreditBalance(fiche, rowSum);
        EnforceValQtyOnModel(fiche);
    }

    private void ValidateHeader(ParsedSoapDocument doc)
    {
        Require(doc.TransactionId, RayvarzFieldRequirementCatalog.OpHeader, "TransactionId",
            "HDR_TRANSACTION_ID_REQUIRED", "TransactionId الزامی است");
        RequireDate8(doc.DocDate, RayvarzFieldRequirementCatalog.OpHeader, "DocDate",
            "HDR_DOCDATE_REQUIRED", "HDR_DOCDATE_FORMAT");
        Require(doc.DocDsc, RayvarzFieldRequirementCatalog.OpHeader, "DocDsc",
            "HDR_DOCDSC_REQUIRED", "شرح سند (DocDsc) الزامی است");
        Require(doc.DocTyp, RayvarzFieldRequirementCatalog.OpHeader, "DocTyp",
            "HDR_DOCTYP_REQUIRED", "نوع سند (DocTyp) الزامی است");
        Require(doc.DocTypDsc, RayvarzFieldRequirementCatalog.OpHeader, "DocTypDsc",
            "HDR_DOCTYPDSC_REQUIRED", "شرح نوع سند (DocTypDsc) الزامی است");
    }

    private void ValidateDocumentItem(ParsedSoapDocument doc, FicheHeaderDto fiche, int requestFund)
    {
        Require(doc.DocRow, RayvarzFieldRequirementCatalog.OpDocItem, "DocRow",
            "ITEM_DOCROW_REQUIRED", "ردیف وجه (DocRow) الزامی است");
        Require(doc.VchrTyp, RayvarzFieldRequirementCatalog.OpDocItem, "VchrTyp",
            "ITEM_VCHRTYP_REQUIRED", "نوع سند وجه (VchrTyp) الزامی است");
        Require(doc.ActTyp, RayvarzFieldRequirementCatalog.OpDocItem, "ActTyp",
            "ITEM_ACTTYP_REQUIRED", "نوع عملیات (ActTyp) الزامی است");
        RequireDate8(doc.ActDate, RayvarzFieldRequirementCatalog.OpDocItem, "ActDate",
            "ITEM_ACTDATE_REQUIRED", "ITEM_ACTDATE_FORMAT");
        Require(doc.PhasTyp, RayvarzFieldRequirementCatalog.OpDocItem, "PhasTyp",
            "ITEM_PHASTYP_REQUIRED", "نوع وجه (PhasTyp) الزامی است");
        RequireDate8(doc.RowDate, RayvarzFieldRequirementCatalog.OpDocItem, "RowDate",
            "ITEM_ROWDATE_REQUIRED", "ITEM_ROWDATE_FORMAT");
        Require(doc.RowDocNo, RayvarzFieldRequirementCatalog.OpDocItem, "RowDocNo",
            "ITEM_ROWDOCNO_REQUIRED", "شماره چک/فیش (RowDocNo) الزامی است");
        Require(doc.BnkAcntNo, RayvarzFieldRequirementCatalog.OpDocItem, "BnkAcntNo",
            "ITEM_BNKACNTNO_REQUIRED", "حساب بانک (BnkAcntNo) الزامی است");

        var fundText = doc.Fund;
        var fundVal = ParseInt(fundText);
        if (RayvarzFieldRequirementCatalog.RequiresFund(fiche)
            && fundVal <= 0
            && requestFund <= 0
            && fiche.SuggestedFund is not > 0)
        {
            Critical(RayvarzFieldRequirementCatalog.OpDocItem, "Fund", "ITEM_FUND_REQUIRED",
                "منبع (Fund) در SOAP الزامی است");
        }

        if (RayvarzFieldRequirementCatalog.RequiresDocumentCenter(fiche)
            && ParseLong(doc.Center) <= 0
            && fiche.Center is not > 0)
        {
            Critical(RayvarzFieldRequirementCatalog.OpDocItem, "Center", "ITEM_CENTER_REQUIRED",
                "مرکز وجه (Center) برای این نوع فیش الزامی است");
        }
    }

    private void ValidateIncmRows(ParsedSoapDocument doc, FicheHeaderDto fiche)
    {
        if (doc.IncmRows.Count == 0)
        {
            Critical(RayvarzFieldRequirementCatalog.OpIncm, "IncmRow", "INCM_ROW_REQUIRED",
                "حداقل یک ردیف درآمد (AddIncm) الزامی است");
            return;
        }

        var incmNo = 0;
        foreach (var row in doc.IncmRows)
        {
            incmNo = ParseInt(row.IncmNo);
            Require(row.IncmRow, RayvarzFieldRequirementCatalog.OpIncm, "IncmRow",
                "INCM_ROW_INDEX_REQUIRED", "شماره ردیف درآمد (IncmRow) الزامی است");
            Require(row.IncmNo, RayvarzFieldRequirementCatalog.OpIncm, "IncmNo",
                "INCM_INCMNO_REQUIRED", "کد درآمد (IncmNo) الزامی است");

            var val = row.ParseVal();
            if (val is null or 0)
            {
                Critical(RayvarzFieldRequirementCatalog.OpIncm, "Val", "INCM_VAL_REQUIRED",
                    $"مبلغ ردیف IncmNo={row.IncmNo} الزامی است");
            }

            var rowDsc = row.IncmRowDsc ?? row.ReasonDsc;
            if (string.IsNullOrWhiteSpace(rowDsc) && fiche.Category == FicheCategory.Income)
            {
                Critical(RayvarzFieldRequirementCatalog.OpIncm, "IncmRowDsc", "INCM_ROWDSC_REQUIRED",
                    $"شرح ردیف IncmNo={row.IncmNo} الزامی است");
            }

            Require(row.RefRowDocNo, RayvarzFieldRequirementCatalog.OpIncm, "RefRowDocNo",
                "INCM_REFROWDOCNO_REQUIRED", "RefRowDocNo الزامی است");
            RequireDate8(row.RefRowDate, RayvarzFieldRequirementCatalog.OpIncm, "RefRowDate",
                "INCM_REFROWDATE_REQUIRED", "INCM_REFROWDATE_FORMAT");
            Require(row.Reason, RayvarzFieldRequirementCatalog.OpIncm, "Reason",
                "INCM_REASON_REQUIRED", "کد علت (Reason) الزامی است");

            var reqs = RayvarzFieldRequirementCatalog.ResolveIncmRowRequirements(incmNo, fiche);
            if (reqs.RequiresCenter1 && ParseLong(row.Center1) <= 0)
                Critical(RayvarzFieldRequirementCatalog.OpIncm, "Center1", "INCM_CENTER1_REQUIRED",
                    $"Center1 برای IncmNo={incmNo} الزامی است");
            if (reqs.RequiresCenter2 && ParseLong(row.Center2) <= 0)
                Critical(RayvarzFieldRequirementCatalog.OpIncm, "Center2", "INCM_CENTER2_REQUIRED",
                    $"Center2 برای IncmNo={incmNo} الزامی است");
            if (reqs.RequiresCenter3 && ParseLong(row.Center3) <= 0)
                Critical(RayvarzFieldRequirementCatalog.OpIncm, "Center3", "INCM_CENTER3_REQUIRED",
                    $"Center3 برای IncmNo={incmNo} الزامی است");
            if (reqs.RequiresRef && string.IsNullOrWhiteSpace(row.Ref))
                Warning(RayvarzFieldRequirementCatalog.OpIncm, "Ref", "INCM_REF_RECOMMENDED",
                    $"Ref برای IncmNo={incmNo} توصیه می‌شود");

            if (reqs.RequiresQtyEqualsVal && val.HasValue && row.ParseQty() is { } qty
                && Math.Abs(qty) != Math.Abs(val.Value))
            {
                Critical(RayvarzFieldRequirementCatalog.OpIncm, "Qty", "FIN_VAL_QTY_MISMATCH",
                    $"Qty ({qty}) ≠ Val ({val}) برای IncmNo={incmNo}");
            }
        }
    }

    private void ValidateFinancialFromSoap(ParsedSoapDocument doc, FicheHeaderDto fiche)
    {
        var rowSum = doc.IncmRows.Sum(r => r.ParseVal() ?? 0m);
        if (!TahatorRowBuilder.RowSumMatchesPayable(fiche, rowSum))
        {
            Critical(RayvarzFieldRequirementCatalog.OpIncm, "Val", "FIN_ROW_SUM_PAYABLE",
                $"جمع Val در SOAP ({rowSum}) با Payable ({fiche.Payable}) مطابقت ندارد");
        }

        EnforceDebitCreditBalance(fiche, rowSum);

        foreach (var row in doc.IncmRows)
        {
            if (row.ParseQty() is not { } qty || row.ParseVal() is not { } val)
                continue;
            if (Math.Abs(qty) != Math.Abs(val))
            {
                Critical(RayvarzFieldRequirementCatalog.OpIncm, "Qty", "FIN_VAL_QTY_MISMATCH",
                    $"Qty ({qty}) ≠ Val ({val}) در SOAP برای IncmNo={row.IncmNo}");
            }
        }
    }

    private void EnforceDebitCreditBalance(FicheHeaderDto fiche, decimal rowSum)
    {
        // بدهکار/بستانکار: برای تهاتر مبلغی یک ردیف منفی؛ برای درآمدی جمع مثبت
        if (TahatorRowBuilder.IsTahatorAmountFiche(fiche) || fiche.DocTyp is 14 or 15)
        {
            if (rowSum >= 0)
            {
                Critical(RayvarzFieldRequirementCatalog.OpIncm, "Val", "FIN_DEBIT_CREDIT_IMBALANCE",
                    "تهاتر مبلغی: جمع ردیف‌ها باید منفی باشد (بدهکار/بستانکار)");
            }
            return;
        }

        if (fiche.IncomeAccountGroup == 151 && rowSum > 0)
        {
            Critical(RayvarzFieldRequirementCatalog.OpIncm, "Val", "FIN_DEBIT_CREDIT_IMBALANCE",
                "برگشت سپرده: جمع ردیف‌ها باید منفی باشد");
        }
    }

    private void EnforceValQtyOnModel(FicheHeaderDto fiche)
    {
        foreach (var row in fiche.Rows.Where(r => r.Val != 0))
        {
            var incmNo = row.IncmNo;
            var reqs = RayvarzFieldRequirementCatalog.ResolveIncmRowRequirements(incmNo, fiche);
            if (!reqs.RequiresQtyEqualsVal)
                continue;

            // مدل: Qty در SOAP از |Val| ساخته می‌شود — اگر Num ست شده باشد همان نقش Qty را دارد
            if (!string.IsNullOrWhiteSpace(row.Num)
                && decimal.TryParse(row.Num, out var num)
                && Math.Abs(num) != Math.Abs(row.Val))
            {
                Warning(RayvarzFieldRequirementCatalog.OpIncm, "Num", "FIN_VAL_QTY_MISMATCH",
                    $"Num ({num}) با Val ({row.Val}) برای IncmNo={incmNo} متفاوت است");
            }
        }
    }

    private void ValidateCompatibilityWarnings(RayvarzValidationInput input)
    {
        foreach (var w in input.CompatibilityWarnings)
        {
            Warning(RayvarzFieldRequirementCatalog.OpCompatibility, "VB", "DSL_COMPATIBILITY_DEFER", w);
        }
    }

    private void ValidatePreSoapErrors(RayvarzValidationInput input)
    {
        foreach (var err in input.PreSoapRuleErrors)
        {
            Critical(RayvarzFieldRequirementCatalog.OpPreSend, "RuleEngine", "BIZ_PRESOAP_RULE_FAILED", err);
        }
    }

    private void Require(string? value, string operation, string field, string code, string message)
    {
        if (!string.IsNullOrWhiteSpace(value))
            return;
        Critical(operation, field, code, message);
    }

    private void RequireDate8(string? value, string operation, string field, string requiredCode, string formatCode)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Critical(operation, field, requiredCode, $"{field} الزامی است");
            return;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.Length != 8)
            Critical(operation, field, formatCode, $"{field} باید ۸ رقم شمسی باشد (yyyyMMdd)");
    }

    private void Critical(string operation, string field, string code, string message) =>
        _issues.Add(new RayvarzValidationIssue
        {
            Operation = operation,
            Field = field,
            Code = code,
            Message = message,
            Severity = RayvarzValidationSeverity.Critical,
            Blocking = true
        });

    private void Warning(string operation, string field, string code, string message) =>
        _issues.Add(new RayvarzValidationIssue
        {
            Operation = operation,
            Field = field,
            Code = code,
            Message = message,
            Severity = RayvarzValidationSeverity.Warning,
            Blocking = false
        });

    private static int ParseInt(string? s) =>
        int.TryParse(s?.Trim(), out var v) ? v : 0;

    private static long ParseLong(string? s) =>
        long.TryParse(s?.Trim(), out var v) ? v : 0;
}
