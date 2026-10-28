using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public class FicheRepository
{
    private const string NosaziNickSql = """
        CAST(b.District AS varchar) + '-' + CAST(b.Region AS varchar) + '-' +
        CAST(b.Block AS varchar) + '-' + CAST(b.House AS varchar) + '-' +
        CAST(b.Building AS varchar) + '-' + CAST(b.Apartment AS varchar) + '-' +
        ISNULL(NULLIF(CAST(b.Shop AS varchar), ''), '0')
        """;

    private static string IncomeBnkAcntNoSelect => $"""
        ISNULL(
          NULLIF(LTRIM(RTRIM({NosaziNickSql})), '-'),
          ''
        ) AS BnkAcntNo
        """;

    private const string IncomeNosaziJoins = """
        LEFT JOIN dbo.Income i WITH (NOLOCK) ON i.NidIncome = f.NidIncome
        LEFT JOIN dbo.Sh_RequestInfo r WITH (NOLOCK) ON r.NidProc = i.NidProc
        LEFT JOIN dbo.Base_NosaziCode b WITH (NOLOCK) ON b.NidNosaziCode = r.NidNosaziCode
        """;

    private const string DutyBnkAcntNoSelect = """
        COALESCE(
            NULLIF(LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject="کد نوسازی"]/Value)[1]', 'nvarchar(100)'))), ''),
            NULLIF(LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject="کد نوسازي"]/Value)[1]', 'nvarchar(100)'))), ''),
            LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject="منطقه"]/Value)[1]', 'nvarchar(20)'))) + '-' +
            LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject="حوزه"]/Value)[1]', 'nvarchar(20)'))) + '-' +
            LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject="بلوک"]/Value)[1]', 'nvarchar(20)'))) + '-' +
            LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject="ملک"]/Value)[1]', 'nvarchar(20)'))) + '-' +
            ISNULL(NULLIF(LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject="ساختمان"]/Value)[1]', 'nvarchar(20)'))), ''), '0') + '-' +
            ISNULL(NULLIF(LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject="آپارتمان"]/Value)[1]', 'nvarchar(20)'))), ''), '0') + '-' +
            ISNULL(NULLIF(LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject="واحد صنفی"]/Value)[1]', 'nvarchar(20)'))), ''), '0')
        ) AS BnkAcntNo
        """;

    private readonly string _saraCs;
    private readonly string _rayCs;

    public FicheRepository(IConfiguration config)
    {
        _saraCs = config.GetConnectionString("Sara") ?? throw new InvalidOperationException("ConnectionStrings:Sara not set");
        _rayCs = config.GetConnectionString("Rayvarz") ?? throw new InvalidOperationException("ConnectionStrings:Rayvarz not set");
    }

    public async Task<FicheHeaderDto?> LoadAsync(IdentifierType type, string value, CancellationToken ct = default)
    {
        value = IdentifierText.Normalize(value);
        var income = await TryLoadIncomeAsync(type, value, ct);
        if (income != null) return income;
        return await TryLoadDutyAsync(type, value, ct);
    }

    public async Task<FicheHeaderDto?> LoadByKindAsync(
        UnsentFicheKind kind, IdentifierType type, string value, CancellationToken ct = default) =>
        kind == UnsentFicheKind.Duty
            ? await TryLoadDutyAsync(type, value, ct)
            : await TryLoadIncomeAsync(type, value, ct);

    /// <summary>نوع شناسه را حدس می‌زند؛ در صورت عدم یافتن نوع دیگر را امتحان می‌کند.</summary>
    public async Task<(FicheHeaderDto? Fiche, IdentifierType UsedType)> LoadByKindWithAutoDetectAsync(
        UnsentFicheKind kind, string rawValue, CancellationToken ct = default)
    {
        var value = IdentifierText.Normalize(rawValue);
        var primary = IdentifierDetector.Detect(value);
        var fiche = await LoadByKindAsync(kind, primary, value, ct);
        if (fiche != null)
            return (fiche, primary);

        var alternate = primary == IdentifierType.FicheNo
            ? IdentifierType.BillPaymentKey
            : IdentifierType.FicheNo;
        fiche = await LoadByKindAsync(kind, alternate, value, ct);
        return (fiche, fiche != null ? alternate : primary);
    }

    /// <summary>
    /// جفت تهاتر: دو ردیف Income_Fiche با همان NidIncome — گروه ۱۵۷ (Tahator1) و ۱۵۸ (Tahator).
    /// </summary>
    public async Task<TahatorPairInfo?> ResolveTahatorPairAsync(string ficheNo, CancellationToken ct = default)
    {
        ficheNo = TahatorRowBuilder.NormalizeFicheNo(ficheNo);
        var seed = await LoadAsync(IdentifierType.FicheNo, ficheNo, ct);
        if (seed?.NidIncome is not { } nid || nid == Guid.Empty)
            return null;
        if (!TahatorRowBuilder.IsTahatorFiche(seed))
            return null;

        const string sql = @"
SELECT FicheNo, CI_IncomeAccountGroup, EumFicheStatus, NidExportation, Payable
FROM dbo.Income_Fiche
WHERE NidIncome = @nid
  AND CI_IncomeAccountGroup IN (@g157, @g158)";

        var candidates = new List<TahatorPairResolver.Candidate>();

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nid);
        cmd.Parameters.AddWithValue("@g157", TahatorRowBuilder.IncomeAccountGroupTahatorAmount);
        cmd.Parameters.AddWithValue("@g158", TahatorRowBuilder.IncomeAccountGroupTahatorIncome);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var exportOrd = reader.GetOrdinal("NidExportation");
            candidates.Add(new TahatorPairResolver.Candidate(
                reader.GetString(reader.GetOrdinal("FicheNo")).Trim(),
                ReadInt32(reader, "CI_IncomeAccountGroup"),
                ReadInt32(reader, "EumFicheStatus"),
                reader.IsDBNull(exportOrd) ? Guid.Empty : reader.GetGuid(exportOrd),
                ReadDecimal(reader, "Payable")));
        }

        var seedRow = candidates.FirstOrDefault(c =>
            string.Equals(c.FicheNo, ficheNo, StringComparison.Ordinal));
        if (seedRow == null)
            return null;

        var resolved = TahatorPairResolver.Resolve(
            candidates,
            ficheNo,
            seed.IncomeAccountGroup ?? seedRow.IncomeAccountGroup,
            seedRow.NidExportation,
            seedRow.Payable);
        if (resolved == null)
            return null;

        var amountNo = resolved.Value.AmountFicheNo;
        var incomeNo = resolved.Value.IncomeFicheNo;

        var amountFiche = string.Equals(amountNo, ficheNo, StringComparison.Ordinal)
            && TahatorRowBuilder.IsTahatorAmountFiche(seed)
            ? seed
            : await LoadAsync(IdentifierType.FicheNo, amountNo, ct);
        var incomeFiche = string.Equals(incomeNo, ficheNo, StringComparison.Ordinal)
            && TahatorRowBuilder.IsTahatorIncomeFiche(seed)
            ? seed
            : await LoadAsync(IdentifierType.FicheNo, incomeNo, ct);

        if (amountFiche == null || incomeFiche == null)
            return null;

        return new TahatorPairInfo
        {
            NidIncome = nid,
            AmountFicheNo = amountFiche.FicheNo.Trim(),
            IncomeFicheNo = incomeFiche.FicheNo.Trim(),
            AmountFiche = amountFiche,
            IncomeFiche = incomeFiche
        };
    }

    private async Task<FicheHeaderDto?> TryLoadIncomeAsync(IdentifierType type, string value, CancellationToken ct)
    {
        var ficheKeys = type == IdentifierType.FicheNo
            ? UnsentBillPayLookupHelper.ExpandFicheNoKeys(value)
            : Array.Empty<string>();
        var where = type == IdentifierType.FicheNo
            ? InClause("f.FicheNo", "fn", ficheKeys)
            : "f.BillID + f.PaymentID = @val";

        var sql = $@"
SELECT f.FicheNo, f.BillID, f.PaymentID, f.Payable, f.NidFiche, f.NidIncome,
       i.NidProc,
       ISNULL(f.Brokers, 0) AS Brokers,
       NULLIF(LTRIM(RTRIM(CAST(f.PaymentBranch AS nvarchar(20)))), '') AS PaymentBranch,
       NULLIF(LTRIM(RTRIM(CAST(f.CI_Bank AS nvarchar(20)))), '') AS CiBank,
       NULLIF(LTRIM(RTRIM(CAST(f.PaymentBank AS nvarchar(20)))), '') AS PaymentBank,
       NULLIF(LTRIM(RTRIM(CAST(f.Deposit AS nvarchar(50)))), '') AS Deposit,
       NULLIF(LTRIM(RTRIM(CAST(f.DepositID AS nvarchar(50)))), '') AS DepositID,
       NULLIF(LTRIM(RTRIM(CAST(f.CreditorPapers AS nvarchar(50)))), '') AS CreditorPapers,
       CAST(f.CheckNo AS nvarchar(20)) AS CheckNo,
       COALESCE(f.BankPaymentDate, f.PaymentDate) AS RowDate,
       f.PaymentDate,
       f.BankPaymentDate,
       f.EumFicheStatus, f.CI_IncomeAccountGroup,
       CAST(r.NidWorkItem AS nvarchar(50)) AS RefReconstructionNo,
       ISNULL(
         NULLIF(LTRIM(RTRIM({NosaziNickSql})), '-'),
         ''
       ) AS BnkAcntNo,
       NULLIF(LTRIM(RTRIM(CAST(b.District AS nvarchar(20)))), '') AS IncomeRegion
FROM dbo.Income_Fiche f
JOIN dbo.Income i ON i.NidIncome = f.NidIncome
LEFT JOIN dbo.Sh_RequestInfo r ON r.NidProc = i.NidProc
LEFT JOIN dbo.Base_NosaziCode b ON b.NidNosaziCode = r.NidNosaziCode
WHERE {where}
{(type == IdentifierType.FicheNo ? InClauseOrderBy("f.FicheNo", "fn", ficheKeys) : "")}";

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        if (type == IdentifierType.FicheNo)
            BindInClause(cmd, "fn", ficheKeys);
        else
            cmd.Parameters.AddWithValue("@val", value.Trim());

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;

        var group = ReadInt32(reader, "CI_IncomeAccountGroup");
        var isTahatorAmount = group == TahatorRowBuilder.IncomeAccountGroupTahatorAmount;
        var isTahatorIncome = group == TahatorRowBuilder.IncomeAccountGroupTahatorIncome;
        var isTahator = isTahatorAmount || isTahatorIncome;
        var docTyp = group == 150
            ? 11
            : isTahatorAmount
                ? 14
                : isTahatorIncome
                    ? 18
                    : 3;
        var ciBank = reader.IsDBNull(reader.GetOrdinal("CiBank")) ? null : reader.GetString(reader.GetOrdinal("CiBank"));
        var paymentBank = reader.IsDBNull(reader.GetOrdinal("PaymentBank")) ? null : reader.GetString(reader.GetOrdinal("PaymentBank"));
        var paymentBranch = reader.IsDBNull(reader.GetOrdinal("PaymentBranch"))
            ? null
            : reader.GetString(reader.GetOrdinal("PaymentBranch"));

        var dto = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            FicheNo = reader.GetString(reader.GetOrdinal("FicheNo")),
            BillIdRaw = reader.GetString(reader.GetOrdinal("BillID")).Trim(),
            PaymentIdRaw = reader.GetString(reader.GetOrdinal("PaymentID")).Trim(),
            BillId = reader.GetString(reader.GetOrdinal("BillID")),
            PaymentId = reader.GetString(reader.GetOrdinal("PaymentID")),
            Payable = ReadDecimal(reader, "Payable"),
            Brokers = ReadDecimal(reader, "Brokers"),
            NidFiche = reader.GetGuid(reader.GetOrdinal("NidFiche")),
            NidIncome = reader.GetGuid(reader.GetOrdinal("NidIncome")),
            NidProc = reader.IsDBNull(reader.GetOrdinal("NidProc"))
                ? null
                : reader.GetGuid(reader.GetOrdinal("NidProc")),
            PaymentBranch = paymentBranch ?? (isTahator ? "" : "18"),
            BankCode = isTahator
                ? (ciBank ?? paymentBank ?? paymentBranch)
                : (paymentBank ?? ciBank ?? paymentBranch ?? "18"),
            Deposit = ReadNullableInt64(reader, "Deposit"),
            DepositId = ReadNullableInt64(reader, "DepositID"),
            CreditorPapers = ReadNullableInt64(reader, "CreditorPapers"),
            CheckNo = reader.IsDBNull(reader.GetOrdinal("CheckNo")) ? null : reader.GetString(reader.GetOrdinal("CheckNo")),
            RowDate = ReadRowDate(reader, "RowDate"),
            CurrentStatus = ReadInt32(reader, "EumFicheStatus"),
            IncomeAccountGroup = group,
            RefReconstructionNo = reader.IsDBNull(reader.GetOrdinal("RefReconstructionNo")) ? null : reader.GetString(reader.GetOrdinal("RefReconstructionNo")),
            BnkAcntNo = reader.IsDBNull(reader.GetOrdinal("BnkAcntNo")) ? "" : reader.GetString(reader.GetOrdinal("BnkAcntNo")),
            BnkAcntNoSource = "کد نوسازی — GetNosaziNickName (منطقه-حوزه-بلوک-ملک-ساختمان-آپارتمان-واحد)",
            IncomeRegion = reader.IsDBNull(reader.GetOrdinal("IncomeRegion")) ? null : reader.GetString(reader.GetOrdinal("IncomeRegion")),
            DocTyp = docTyp,
            DocDsc = isTahatorAmount
                ? "اسناد تهاتر مبلغ"
                : isTahatorIncome
                    ? "اسناد تهاتر درامد"
                    : "اسناد شهرسازی"
        };

        var districtBranch = DutyDistrictBranchResolver.ResolveBranch(dto.BillIdRaw, dto.PaymentIdRaw);
        if (districtBranch > 0)
        {
            dto.ResolvedDistrictBranch = districtBranch;
            if (!isTahatorAmount && !isTahatorIncome)
                dto.SuggestedFund = DutyDistrictBranchResolver.ResolveFund(districtBranch, dto.BankCode ?? "18");
        }

        if (isTahatorAmount)
            TahatorRowBuilder.ApplyTahatorAmountRows(dto);
        else if (isTahatorIncome)
        {
            // VB Tahator(): TmpDT از Income_Calculation — WrapperAccountNo = CI_IncomeCalculation (نه IncomeCode).
            dto.Rows = await LoadIncomeRowsAsync(dto.NidIncome!.Value, ct, mapRayvarzIncmNo: false);

            await IncomeFicheSupplementLoader.EnrichAsync(dto, _saraCs, LoadIncomeRowsAsync, ct);
            IncomeMember1388RowBuilder.Apply(dto);
        }
        else
        {
            dto.Rows = await LoadIncomeRowsAsync(dto.NidIncome!.Value, ct);
            await IncomeFicheSupplementLoader.EnrichAsync(dto, _saraCs, LoadIncomeRowsAsync, ct);
            IncomeMember1388RowBuilder.Apply(dto);
        }
        FicheDateResolver.ApplyFromIncomeColumns(
            dto,
            dto.CurrentStatus,
            ReadRowDate(reader, "PaymentDate"),
            ReadRowDate(reader, "BankPaymentDate"),
            tahatorFiche: isTahator);
        return dto;
    }

    private Task<List<IncmRowDto>> LoadIncomeRowsAsync(Guid nidIncome, CancellationToken ct) =>
        LoadIncomeRowsAsync(nidIncome, ct, mapRayvarzIncmNo: false);

    private async Task<List<IncmRowDto>> LoadIncomeRowsAsync(
        Guid nidIncome,
        CancellationToken ct,
        bool mapRayvarzIncmNo,
        Dictionary<int, string>? incomeCodeByCalculation = null,
        HashSet<int>? allowedCalculationIds = null)
    {
        const string sql = """
            SELECT ic.CI_IncomeCalculation AS CalcId,
                   COALESCE(ic.SysValue, ic.Value) AS Val,
                   ISNULL(c.Title, '') AS IncmRowDsc,
                   ISNULL(LTRIM(RTRIM(CAST(c.IncomeCode AS nvarchar(30)))), '') AS IncomeCode
            FROM dbo.Income_Calculation ic
            LEFT JOIN dbo.CI_IncomeCalculation c ON c.ID = ic.CI_IncomeCalculation
            WHERE ic.NidIncome = @nid
            """;

        var rows = new List<IncmRowDto>();
        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidIncome);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var calcId = ReadInt32(reader, "CalcId");
            if (allowedCalculationIds is { Count: > 0 } && !allowedCalculationIds.Contains(calcId))
                continue;

            var incomeCode = reader.IsDBNull(reader.GetOrdinal("IncomeCode"))
                ? ""
                : reader.GetString(reader.GetOrdinal("IncomeCode"));
            if (incomeCodeByCalculation != null)
                incomeCodeByCalculation[calcId] = incomeCode;

            var incmNo = mapRayvarzIncmNo
                ? IncomeRayvarzIncmNoResolver.Resolve(calcId, incomeCode)
                : calcId;
            if (incmNo <= 0 || IncomeExcludedCodes.Codes.Contains(calcId))
                continue;

            var val = ReadDecimal(reader, "Val");
            if (val == 0) continue;
            rows.Add(new IncmRowDto
            {
                IncmNo = incmNo,
                Val = val,
                IncmRowDsc = reader.GetString(reader.GetOrdinal("IncmRowDsc"))
            });
        }

        return rows;
    }

    private async Task<FicheHeaderDto?> TryLoadDutyAsync(IdentifierType type, string value, CancellationToken ct)
    {
        var ficheKeys = type == IdentifierType.FicheNo
            ? UnsentBillPayLookupHelper.ExpandFicheNoKeys(value)
            : Array.Empty<string>();
        var where = type == IdentifierType.FicheNo
            ? InClause("d.FicheNo", "fn", ficheKeys)
            : "d.BillID + d.PaymentID = @val";

        var sql = $@"
SELECT d.FicheNo, d.BillID, d.PaymentID, d.PayablePrice AS Payable, d.NidFiche,
       d.EumDutyType,
       '18' AS PaymentBranch,
       NULLIF(LTRIM(RTRIM(CAST(d.ConfirmBankCode AS nvarchar(20)))), '') AS BankCode,
       COALESCE(d.BankPaymentDate, d.PaymentDate, d.PrintDate, d.ExportDate) AS RowDate,
       d.PaymentDate,
       d.BankPaymentDate,
       d.PrintDate,
       d.ExportDate,
       d.EumDutyFicheStatus, d.CI_DutyFicheExportType,
       COALESCE(
           NULLIF(LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject=""کد نوسازی""]/Value)[1]', 'nvarchar(100)'))), ''),
           NULLIF(LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject=""کد نوسازي""]/Value)[1]', 'nvarchar(100)'))), ''),
           LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject=""منطقه""]/Value)[1]', 'nvarchar(20)'))) + '-' +
           LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject=""حوزه""]/Value)[1]', 'nvarchar(20)'))) + '-' +
           LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject=""بلوک""]/Value)[1]', 'nvarchar(20)'))) + '-' +
           LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject=""ملک""]/Value)[1]', 'nvarchar(20)'))) + '-' +
           ISNULL(NULLIF(LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject=""ساختمان""]/Value)[1]', 'nvarchar(20)'))), ''), '0') + '-' +
           ISNULL(NULLIF(LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject=""آپارتمان""]/Value)[1]', 'nvarchar(20)'))), ''), '0') + '-' +
           ISNULL(NULLIF(LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject=""واحد صنفی""]/Value)[1]', 'nvarchar(20)'))), ''), '0')
       ) AS BnkAcntNo,
       NULLIF(LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject=""منطقه""]/Value)[1]', 'nvarchar(20)'))), '') AS DutyRegion
FROM dbo.Duty_Fiche d
WHERE {where}
{(type == IdentifierType.FicheNo ? InClauseOrderBy("d.FicheNo", "fn", ficheKeys) : "")}";

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        if (type == IdentifierType.FicheNo)
            BindInClause(cmd, "fn", ficheKeys);
        else
            cmd.Parameters.AddWithValue("@val", value.Trim());

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;

        var exportType = reader.IsDBNull(reader.GetOrdinal("CI_DutyFicheExportType"))
            ? 0 : ReadInt32(reader, "CI_DutyFicheExportType");
        var dutyType = ReadInt32(reader, "EumDutyType");
        var isSenfi = DutyNosaziLogic.IsSenfiObjOnPrice(dutyType);
        var rawBill = reader.GetString(reader.GetOrdinal("BillID"));
        var rawPayment = reader.GetString(reader.GetOrdinal("PaymentID"));
        var bankCode = reader.IsDBNull(reader.GetOrdinal("BankCode"))
            ? "18"
            : DutyNosaziLogic.DefaultBankCode(reader.GetString(reader.GetOrdinal("BankCode")));
        var dutyStatus = ReadInt32(reader, "EumDutyFicheStatus");
        var paymentDateRay = ReadRowDate(reader, "PaymentDate");
        var bankPaymentDateRay = ReadRowDate(reader, "BankPaymentDate");
        var nidFiche = reader.GetGuid(reader.GetOrdinal("NidFiche"));

        var dto = new FicheHeaderDto
        {
            Category = isSenfi ? FicheCategory.DutySenfi : FicheCategory.DutyNosazi,
            FicheNo = reader.GetString(reader.GetOrdinal("FicheNo")),
            BillIdRaw = rawBill.Trim(),
            PaymentIdRaw = rawPayment.Trim(),
            BillId = DutyNosaziLogic.NormalizeMergedId(rawBill),
            PaymentId = DutyNosaziLogic.NormalizeMergedId(rawPayment),
            Payable = ReadDecimal(reader, "Payable"),
            NidFiche = nidFiche,
            PaymentBranch = bankCode,
            BankCode = bankCode,
            RowDate = ReadRowDate(reader, "RowDate"),
            CurrentStatus = dutyStatus,
            DutyExportType = exportType,
            BnkAcntNo = reader.IsDBNull(reader.GetOrdinal("BnkAcntNo")) ? "" : reader.GetString(reader.GetOrdinal("BnkAcntNo")),
            BnkAcntNoSource = "کد نوسازی — از Duty_Fiche.OtherFields (XML فیش)",
            DutyRegion = reader.IsDBNull(reader.GetOrdinal("DutyRegion")) ? null : reader.GetString(reader.GetOrdinal("DutyRegion")),
            DocTyp = isSenfi ? 2 : 1,
            DocDsc = isSenfi ? "اسناد صنفی" : "اسناد نوسازی"
        };

        var districtBranch = DutyDistrictBranchResolver.ResolveBranch(rawBill, rawPayment);
        if (districtBranch > 0)
        {
            dto.ResolvedDistrictBranch = districtBranch;
            dto.SuggestedFund = DutyDistrictBranchResolver.ResolveFund(districtBranch, bankCode);
        }

        if (!isSenfi)
        {
            var nickResult = await TryLoadNosaziNickNameAsync(nidFiche, ct);
            NosaziNickNameLogic.ApplyLoadResult(dto, nickResult.Nick, nickResult.LoadError);
        }

        if (isSenfi)
        {
            dto.BnkAcntNo = "7-14-55-1-1-0-1";
            dto.BnkAcntNoSource = "کد ثابت صنفی — Rayvarz (7-14-55-1-1-0-1)";
        }

        await DutyFicheSupplementLoader.EnrichAsync(dto, _saraCs, dutyType, ct);

        dto.Rows = await LoadDutyRowsAsync(
            dto.NidFiche, dto.Payable, isSenfi, exportType, dto.DutyOddments, dto.FicheNo, ct);
        DutyNosaziLogic.ApplyRayvarzDates(dto, dutyStatus, paymentDateRay, bankPaymentDateRay);
        return dto;
    }

    private async Task<List<IncmRowDto>> LoadDutyRowsAsync(
        Guid nidFiche,
        decimal payable,
        bool isSenfi,
        int exportType,
        IReadOnlyList<DutyOddmentDto> dutyOddments,
        string ficheNo,
        CancellationToken ct)
    {
        const string sql = @"
SELECT CI_DutyFormula, CI_DutyFormulaFiche, Price
FROM dbo.Duty_FicheSub
WHERE NidFiche = @nid";

        var subs = new List<(int Formula, int Fiche, decimal Price)>();
        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidFiche);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            subs.Add((
                ReadInt32(reader, "CI_DutyFormula"),
                reader.IsDBNull(reader.GetOrdinal("CI_DutyFormulaFiche")) ? 0 : ReadInt32(reader, "CI_DutyFormulaFiche"),
                ReadDecimal(reader, "Price")
            ));
        }

        DutyOddmentLogic.ApplyToSubs(subs, dutyOddments, ficheNo);

        var amounts = DutyNosaziLogic.CalculateSubAmounts(subs, payable);
        return DutyNosaziLogic.BuildIncmRows(amounts, isSenfi, exportType);
    }

    private async Task<(string? Nick, string? LoadError)> TryLoadNosaziNickNameAsync(Guid nidFiche, CancellationToken ct)
    {
        const string sql = @"
SELECT TOP 1
  CAST(b.District AS varchar) + '-' + CAST(b.Region AS varchar) + '-' +
  CAST(b.Block AS varchar) + '-' + CAST(b.House AS varchar) + '-' +
  CAST(b.Building AS varchar) + '-' + CAST(b.Apartment AS varchar) + '-' +
  ISNULL(NULLIF(CAST(b.Shop AS varchar), ''), '0') AS Nick
FROM dbo.Duty_FicheSub fs
INNER JOIN dbo.Base_NosaziCode b ON b.NidNosaziCode = fs.NidFK
WHERE fs.NidFiche = @nid
ORDER BY fs.CI_DutyFormula, fs.CI_DutyFormulaFiche";

        try
        {
            await using var conn = new SqlConnection(_saraCs);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nid", nidFiche);
            var result = await cmd.ExecuteScalarAsync(ct);
            var s = result?.ToString()?.Trim();
            var nick = string.IsNullOrWhiteSpace(s) || s == "-------0" ? null : s;
            return (nick, null);
        }
        catch (SqlException ex)
        {
            return (null, NosaziNickNameLogic.FormatSqlFailureWarning(ex.Message));
        }
    }

    /// <summary>مقایسه متنی RowDocNo تا شمارهٔ نزدیک (مثل …۱۹۹ و …۱۹۶) با تبدیل عددی یکی دیده نشود.</summary>
    private const string RowDocNoEqualsSql =
        "(LTRIM(RTRIM(CONVERT(nvarchar(40), RowDocNo))) = @f OR LTRIM(RTRIM(CONVERT(nvarchar(40), RowDocNo))) = @fAlt)";

    private const string RefEqualsSql =
        "(LTRIM(RTRIM(CONVERT(nvarchar(40), Ref))) = @f OR LTRIM(RTRIM(CONVERT(nvarchar(40), Ref))) = @fAlt)";

    private static void AddRowDocNoParameters(SqlCommand cmd, string ficheNo)
    {
        var alt = ficheNo.TrimStart('0');
        if (alt.Length == 0)
            alt = ficheNo;
        cmd.Parameters.Add(new SqlParameter("@f", System.Data.SqlDbType.NVarChar, 40) { Value = ficheNo });
        cmd.Parameters.Add(new SqlParameter("@fAlt", System.Data.SqlDbType.NVarChar, 40) { Value = alt });
    }

    private static string InClause(string column, string prefix, IReadOnlyList<string> keys)
    {
        if (keys.Count == 0)
            return "1 = 0";
        return $"{column} IN ({string.Join(", ", keys.Select((_, i) => $"@{prefix}{i}"))})";
    }

    /// <summary>کلید اول همان شمارهٔ واردشده است — تطبیق دقیق قبل از نسخه‌های بدون صفر ابتدایی.</summary>
    private static string InClauseOrderBy(string column, string prefix, IReadOnlyList<string> keys)
    {
        if (keys.Count <= 1)
            return "";
        var whens = string.Join(" ", keys.Select((_, i) => $"WHEN @{prefix}{i} THEN {i}"));
        return $"ORDER BY CASE {column} {whens} ELSE {keys.Count} END";
    }

    private static void BindInClause(SqlCommand cmd, string prefix, IReadOnlyList<string> keys)
    {
        for (var i = 0; i < keys.Count; i++)
            cmd.Parameters.Add(new SqlParameter($"@{prefix}{i}", System.Data.SqlDbType.NVarChar, 40) { Value = keys[i] });
    }

    /// <summary>کمترین Val هر شماره فیش در incmdocsys. نبودن کلید یعنی ردیفی در رایورز نیست.</summary>
    public async Task<Dictionary<string, decimal>> GetRayvarzMinAmountByFicheNoAsync(
        IReadOnlyCollection<string> ficheNos,
        CancellationToken ct = default)
    {
        var byNumber = new Dictionary<decimal, List<string>>();
        foreach (var raw in ficheNos)
        {
            var key = IdentifierText.Normalize(raw);
            if (key.Length == 0 || !TryFicheDecimal(key, out var number))
                continue;
            if (!byNumber.TryGetValue(number, out var owners))
            {
                owners = new List<string>();
                byNumber[number] = owners;
            }
            if (!owners.Contains(key))
                owners.Add(key);
        }

        var found = new Dictionary<string, decimal>(StringComparer.Ordinal);
        if (byNumber.Count == 0)
            return found;

        await using var conn = new SqlConnection(_rayCs);
        await conn.OpenAsync(ct);

        foreach (var chunk in byNumber.Keys.Chunk(40))
        {
            var names = new string[chunk.Length];
            await using var cmd = new SqlCommand { Connection = conn, CommandTimeout = BillPaySqlCommandTimeoutSeconds };
            for (var i = 0; i < chunk.Length; i++)
            {
                names[i] = $"@n{i}";
                cmd.Parameters.Add(new SqlParameter(names[i], System.Data.SqlDbType.Decimal)
                {
                    Precision = 24,
                    Scale = 0,
                    Value = chunk[i]
                });
            }

            cmd.CommandText = $"""
                SELECT TRY_CONVERT(decimal(24, 0), LTRIM(RTRIM(CONVERT(nvarchar(40), RowDocNo)))) AS DocNum,
                       Val
                FROM ray.incmdocsys WITH (NOLOCK)
                WHERE TRY_CONVERT(decimal(24, 0), LTRIM(RTRIM(CONVERT(nvarchar(40), RowDocNo)))) IN ({string.Join(", ", names)})
                """;

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                if (reader.IsDBNull(0) || reader.IsDBNull(1))
                    continue;
                var number = Convert.ToDecimal(reader.GetValue(0));
                var amount = Convert.ToDecimal(reader.GetValue(1));
                if (!byNumber.TryGetValue(number, out var owners))
                    continue;
                foreach (var owner in owners)
                {
                    if (!found.TryGetValue(owner, out var existing) || amount < existing)
                        found[owner] = amount;
                }
            }
        }

        return found;
    }

    private static bool TryFicheDecimal(string key, out decimal number)
    {
        var digits = new string(key.Where(char.IsDigit).ToArray()).TrimStart('0');
        if (digits.Length == 0)
        {
            number = 0;
            return false;
        }

        return decimal.TryParse(digits, System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture, out number);
    }

    /// <summary>شماره فیش‌هایی که در incmdocsys دست‌کم یک ردیف با Val منفی دارند.</summary>
    public async Task<HashSet<string>> FindFicheNosWithNegativeRayvarzAmountAsync(
        IReadOnlyCollection<string> ficheNos,
        CancellationToken ct = default)
    {
        var keys = ficheNos
            .Select(IdentifierText.Normalize)
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var found = new HashSet<string>(StringComparer.Ordinal);
        if (keys.Length == 0)
            return found;

        await using var conn = new SqlConnection(_rayCs);
        await conn.OpenAsync(ct);

        foreach (var chunk in keys.Chunk(40))
        {
            var predicates = new List<string>(chunk.Length);
            await using var cmd = new SqlCommand { Connection = conn, CommandTimeout = BillPaySqlCommandTimeoutSeconds };
            for (var i = 0; i < chunk.Length; i++)
            {
                var alt = chunk[i].TrimStart('0');
                if (alt.Length == 0)
                    alt = chunk[i];
                predicates.Add(
                    $"(LTRIM(RTRIM(CONVERT(nvarchar(40), RowDocNo))) = @f{i} OR LTRIM(RTRIM(CONVERT(nvarchar(40), RowDocNo))) = @a{i})");
                cmd.Parameters.Add(new SqlParameter($"@f{i}", System.Data.SqlDbType.NVarChar, 40) { Value = chunk[i] });
                cmd.Parameters.Add(new SqlParameter($"@a{i}", System.Data.SqlDbType.NVarChar, 40) { Value = alt });
            }

            cmd.CommandText = $"""
                SELECT LTRIM(RTRIM(CONVERT(nvarchar(40), RowDocNo))) AS DocNo, Val
                FROM ray.incmdocsys WITH (NOLOCK)
                WHERE {string.Join(" OR ", predicates)}
                """;

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                if (reader.IsDBNull(1))
                    continue;
                var amount = Convert.ToDecimal(reader.GetValue(1));
                if (amount >= 0)
                    continue;

                var docNo = reader.GetString(0).Trim();
                var docAlt = docNo.TrimStart('0');
                foreach (var key in chunk)
                {
                    var keyAlt = key.TrimStart('0');
                    if (docNo == key || docNo == keyAlt || docAlt == key || (docAlt.Length > 0 && docAlt == keyAlt))
                        found.Add(key);
                }
            }
        }

        return found;
    }

    public Task<UnsentFicheListItem?> LoadUnsentIncomeItemByFicheNoAsync(
        string ficheNo,
        CancellationToken ct = default) =>
        LoadIncomeListItemByFicheNoAsync(ficheNo, unsentOnly: true, ct);

    public async Task<UnsentFicheListItem?> LoadIncomeListItemByFicheNoAsync(
        string ficheNo,
        bool unsentOnly,
        CancellationToken ct = default)
    {
        ficheNo = IdentifierText.Normalize(ficheNo);
        if (ficheNo.Length == 0)
            return null;

        var pair = new NormalizedBillPayPair(
            ficheNo,
            ficheNo,
            UnsentBillPayLookupHelper.TrimLeadingZeros(ficheNo),
            UnsentBillPayLookupHelper.TrimLeadingZeros(ficheNo),
            ficheNo);
        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        var items = await ExecuteSeekLookupAsync(
            conn,
            UnsentFicheKind.Income,
            [pair],
            unsentOnly,
            ct);
        return items.FirstOrDefault();
    }

    public async Task<bool> ExistsInRayvarzAsync(string ficheNo, int? shamsiYear = null, CancellationToken ct = default)
    {
        ficheNo = TahatorRowBuilder.NormalizeFicheNo(ficheNo.Trim());
        if (await QueryExistsInRayvarzAsync(ficheNo, shamsiYear, docTypFilter: null, ct))
            return true;

        if (shamsiYear is > 0)
            return await QueryExistsInRayvarzAsync(ficheNo, shamsiYear: null, docTypFilter: null, ct);

        return false;
    }

    /// <summary>تهاتر: RowDocNo + DocTyp ۱۴|۱۵ (مبلغ) یا ۱۷|۱۸ (درآمد) — نه Ref عمومی.</summary>
    public async Task<bool> ExistsTahatorDocumentInRayvarzAsync(
        string ficheNo,
        bool isAmountPath,
        int? shamsiYear = null,
        CancellationToken ct = default)
    {
        ficheNo = TahatorRowBuilder.NormalizeFicheNo(ficheNo.Trim());
        var docTypes = isAmountPath ? new[] { 14, 15 } : new[] { 17, 18 };
        if (await QueryExistsInRayvarzAsync(ficheNo, shamsiYear, docTypes, ct))
            return true;

        if (shamsiYear is > 0)
            return await QueryExistsInRayvarzAsync(ficheNo, shamsiYear: null, docTypes, ct);

        return false;
    }

    public async Task<bool> ExistsTahatorDocumentInRayvarzRobustAsync(
        string ficheNo,
        bool isAmountPath,
        IEnumerable<int> yearCandidates,
        CancellationToken ct = default)
    {
        foreach (var year in yearCandidates.Distinct().Where(y => y > 0))
        {
            if (await ExistsTahatorDocumentInRayvarzAsync(ficheNo, isAmountPath, year, ct))
                return true;
        }

        return await ExistsTahatorDocumentInRayvarzAsync(ficheNo, isAmountPath, shamsiYear: null, ct);
    }

    private async Task<bool> QueryExistsInRayvarzAsync(
        string ficheNo,
        int? shamsiYear,
        int[]? docTypFilter,
        CancellationToken ct)
    {
        var sql = docTypFilter is { Length: > 0 }
            ? shamsiYear is > 0
                ? $"""
                  SELECT TOP 1 1 FROM ray.incmdocsys
                  WHERE yr = @yr AND {RowDocNoEqualsSql} AND DocTyp IN (@d0, @d1)
                  """
                : $"""
                  SELECT TOP 1 1 FROM ray.incmdocsys
                  WHERE {RowDocNoEqualsSql} AND DocTyp IN (@d0, @d1)
                  """
            : shamsiYear is > 0
                ? $"""
                  SELECT TOP 1 1 FROM ray.incmdocsys
                  WHERE yr = @yr AND ({RefEqualsSql} OR {RowDocNoEqualsSql})
                  """
                : $"""
                  SELECT TOP 1 1 FROM ray.incmdocsys
                  WHERE {RefEqualsSql} OR {RowDocNoEqualsSql}
                  """;

        await using var conn = new SqlConnection(_rayCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        AddRowDocNoParameters(cmd, ficheNo);
        if (shamsiYear is > 0)
            cmd.Parameters.AddWithValue("@yr", shamsiYear.Value);
        if (docTypFilter is { Length: > 0 })
        {
            cmd.Parameters.AddWithValue("@d0", docTypFilter[0]);
            cmd.Parameters.AddWithValue("@d1", docTypFilter[1]);
        }

        var result = await cmd.ExecuteScalarAsync(ct);
        return result != null;
    }

    public async Task<string?> GetDocNotSentErrorAsync(string ficheNo, CancellationToken ct = default)
    {
        const string sql = @"
SELECT TOP 1 Comment FROM dbo.Accounting_DocNotSent
WHERE FicheNo = @f ORDER BY Uptime DESC";

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@f", ficheNo);
        var result = await cmd.ExecuteScalarAsync(ct);
        return result as string;
    }

    public async Task<bool> ExistsInAccountingDocHeaderAsync(string ficheNo, CancellationToken ct = default)
    {
        const string sql = @"SELECT TOP 1 1 FROM dbo.Accounting_DocHeader WHERE FicheNo = @f";
        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@f", ficheNo.Trim());
        var result = await cmd.ExecuteScalarAsync(ct);
        return result != null;
    }

    /// <summary>متادیتای سند رایورز برای AccountingNo — از ray.incmdocsys پس از ارسال موفق.</summary>
    public async Task<RayvarzDocMeta?> GetRayvarzDocMetaAsync(string ficheNo, CancellationToken ct = default) =>
        await QueryRayvarzDocMetaAsync(ficheNo.Trim(), shamsiYear: null, docTypFilter: null, ct);

    /// <summary>پس از SOAP: صبر تا incmdocsys (با فیلتر DocTyp تهاتر در صورت نیاز) برای ثبت واسط.</summary>
    public async Task<RayvarzDocMeta?> WaitForRayvarzDocMetaAsync(
        FicheHeaderDto fiche,
        IReadOnlyList<int>? yearCandidates,
        int timeoutSeconds,
        int pollIntervalMs,
        CancellationToken ct = default)
    {
        var ficheNo = TahatorRowBuilder.NormalizeFicheNo(fiche.FicheNo);
        var isTahator = TahatorRowBuilder.IsTahatorFiche(fiche);
        var isAmount = TahatorRowBuilder.IsTahatorAmountFiche(fiche);
        int[]? docTypes = isTahator
            ? isAmount ? new[] { 14, 15 } : new[] { 17, 18 }
            : null;

        var years = (yearCandidates ?? Array.Empty<int>()).Where(y => y > 0).Distinct().ToList();
        var deadline = DateTime.UtcNow.AddSeconds(Math.Max(1, timeoutSeconds));
        var delay = Math.Max(200, pollIntervalMs);

        while (true)
        {
            if (docTypes != null)
            {
                foreach (var yr in years)
                {
                    var meta = await QueryRayvarzDocMetaAsync(ficheNo, yr, docTypes, ct);
                    if (meta != null) return meta;
                }

                var anyYear = await QueryRayvarzDocMetaAsync(ficheNo, shamsiYear: null, docTypes, ct);
                if (anyYear != null) return anyYear;
            }
            else
            {
                foreach (var yr in years)
                {
                    var meta = await QueryRayvarzDocMetaAsync(ficheNo, yr, docTypFilter: null, ct);
                    if (meta != null) return meta;
                }

                var fallback = await GetRayvarzDocMetaAsync(ficheNo, ct);
                if (fallback != null) return fallback;
            }

            if (DateTime.UtcNow >= deadline)
                return null;

            await Task.Delay(delay, ct);
        }
    }

    private async Task<RayvarzDocMeta?> QueryRayvarzDocMetaAsync(
        string ficheNo,
        int? shamsiYear,
        int[]? docTypFilter,
        CancellationToken ct)
    {
        var sql = docTypFilter is { Length: > 0 }
            ? shamsiYear is > 0
                ? $"""
                  SELECT TOP 1 Branch, Yr, DocTyp, Doc, Fund
                  FROM ray.incmdocsys
                  WHERE yr = @yr AND {RowDocNoEqualsSql} AND DocTyp IN (@d0, @d1)
                  ORDER BY ActDate DESC
                  """
                : $"""
                  SELECT TOP 1 Branch, Yr, DocTyp, Doc, Fund
                  FROM ray.incmdocsys
                  WHERE {RowDocNoEqualsSql} AND DocTyp IN (@d0, @d1)
                  ORDER BY ActDate DESC
                  """
            : shamsiYear is > 0
                ? $"""
                  SELECT TOP 1 Branch, Yr, DocTyp, Doc, Fund
                  FROM ray.incmdocsys
                  WHERE yr = @yr AND ({RowDocNoEqualsSql} OR {RefEqualsSql})
                  ORDER BY ActDate DESC
                  """
                : $"""
                  SELECT TOP 1 Branch, Yr, DocTyp, Doc, Fund
                  FROM ray.incmdocsys
                  WHERE {RowDocNoEqualsSql} OR {RefEqualsSql}
                  ORDER BY ActDate DESC
                  """;

        await using var conn = new SqlConnection(_rayCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        AddRowDocNoParameters(cmd, ficheNo);
        if (shamsiYear is > 0)
            cmd.Parameters.AddWithValue("@yr", shamsiYear.Value);
        if (docTypFilter is { Length: > 0 })
        {
            cmd.Parameters.AddWithValue("@d0", docTypFilter[0]);
            cmd.Parameters.AddWithValue("@d1", docTypFilter[1]);
        }

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;

        return new RayvarzDocMeta
        {
            Branch = reader["Branch"] is DBNull ? 0 : Convert.ToInt32(reader["Branch"]),
            Yr = reader["Yr"] is DBNull ? 0 : Convert.ToInt32(reader["Yr"]),
            DocTyp = reader["DocTyp"] is DBNull ? 0 : Convert.ToInt32(reader["DocTyp"]),
            Doc = reader["Doc"] is DBNull ? 0 : Convert.ToInt32(reader["Doc"]),
            Fund = reader["Fund"] is DBNull ? null : Convert.ToInt32(reader["Fund"])
        };
    }

    private const string IncomeUnsentDateClause = """
        AND (
              (NULLIF(LTRIM(RTRIM(CAST(f.PaymentDate AS nvarchar(20)))), '') >= @from
               AND NULLIF(LTRIM(RTRIM(CAST(f.PaymentDate AS nvarchar(20)))), '') <= @to)
              OR (NULLIF(LTRIM(RTRIM(CAST(f.BankPaymentDate AS nvarchar(20)))), '') >= @from
                  AND NULLIF(LTRIM(RTRIM(CAST(f.BankPaymentDate AS nvarchar(20)))), '') <= @to)
            )
        """;

    private const string DutyUnsentDateClause = """
        AND (
              (NULLIF(LTRIM(RTRIM(CAST(d.PaymentDate AS nvarchar(20)))), '') >= @from
               AND NULLIF(LTRIM(RTRIM(CAST(d.PaymentDate AS nvarchar(20)))), '') <= @to)
              OR (NULLIF(LTRIM(RTRIM(CAST(d.BankPaymentDate AS nvarchar(20)))), '') >= @from
                  AND NULLIF(LTRIM(RTRIM(CAST(d.BankPaymentDate AS nvarchar(20)))), '') <= @to)
            )
        """;

    public async Task<UnsentFicheSearchResult> SearchUnsentIncomeAsync(UnsentFicheSearchRequest req, CancellationToken ct = default)
    {
        var max = req.MaxResults is > 0 and <= 2000 ? req.MaxResults : 500;
        var hasDateRange = req.HasDateRange;
        var from = hasDateRange ? DateHelper.ToShamsiSlashDate(req.FromDate) : "";
        var to = hasDateRange ? DateHelper.ToShamsiSlashDate(req.ToDate) : "";
        var dateClause = hasDateRange ? IncomeUnsentDateClause : "";
        var hasDistrict = !string.IsNullOrWhiteSpace(req.District);

        var sql = hasDistrict
            ? $"""
              SELECT TOP (@max)
                     f.FicheNo, f.NidFiche, f.BillID, f.PaymentID, f.Payable,
                     f.PaymentDate, f.BankPaymentDate, f.EumFicheStatus,
                     f.CI_IncomeAccountGroup AS IncomeAccountGroup,
                     CAST(r.NidWorkItem AS nvarchar(50)) AS NidWorkItem,
                     NULLIF(LTRIM(RTRIM(CAST(b.District AS nvarchar(20)))), '') AS District,
                     {IncomeBnkAcntNoSelect}
              FROM dbo.Income_Fiche f WITH (NOLOCK)
              INNER JOIN dbo.Income i WITH (NOLOCK) ON i.NidIncome = f.NidIncome
              INNER JOIN dbo.Sh_RequestInfo r WITH (NOLOCK) ON r.NidProc = i.NidProc
              INNER JOIN dbo.Base_NosaziCode b WITH (NOLOCK) ON b.NidNosaziCode = r.NidNosaziCode
              WHERE NOT EXISTS (
                    SELECT 1 FROM dbo.Accounting_DocHeader h WITH (NOLOCK)
                    WHERE h.NidFiche = f.NidFiche)
                AND f.EumFicheStatus <> 4
                {dateClause}
                AND (@ficheNo = '' OR f.FicheNo LIKE @ficheNoPat)
                AND (@billId = '' OR f.BillID LIKE @billIdPat)
                AND (@paymentId = '' OR f.PaymentID LIKE @paymentIdPat)
                AND (@restrictAg IS NULL OR f.CI_IncomeAccountGroup = @restrictAg)
                AND CAST(b.District AS nvarchar(20)) = @district
              ORDER BY COALESCE(f.BankPaymentDate, f.PaymentDate) DESC, f.FicheNo
              """
            : $"""
              SELECT TOP (@max)
                     f.FicheNo, f.NidFiche, f.BillID, f.PaymentID, f.Payable,
                     f.PaymentDate, f.BankPaymentDate, f.EumFicheStatus,
                     f.CI_IncomeAccountGroup AS IncomeAccountGroup,
                     CAST(r.NidWorkItem AS nvarchar(50)) AS NidWorkItem,
                     '' AS District,
                     {IncomeBnkAcntNoSelect}
              FROM dbo.Income_Fiche f WITH (NOLOCK)
              {IncomeNosaziJoins}
              WHERE NOT EXISTS (
                    SELECT 1 FROM dbo.Accounting_DocHeader h WITH (NOLOCK)
                    WHERE h.NidFiche = f.NidFiche)
                AND f.EumFicheStatus <> 4
                {dateClause}
                AND (@ficheNo = '' OR f.FicheNo LIKE @ficheNoPat)
                AND (@billId = '' OR f.BillID LIKE @billIdPat)
                AND (@paymentId = '' OR f.PaymentID LIKE @paymentIdPat)
                AND (@restrictAg IS NULL OR f.CI_IncomeAccountGroup = @restrictAg)
              ORDER BY COALESCE(f.BankPaymentDate, f.PaymentDate) DESC, f.FicheNo
              """;

        var items = await ExecuteUnsentSearchAsync(sql, max, from, to, req, ct, hasDateRange: hasDateRange);
        return new UnsentFicheSearchResult
        {
            FicheKind = UnsentFicheKind.Income,
            Count = items.Count,
            Truncated = items.Count >= max,
            Items = items
        };
    }

    public async Task<UnsentFicheSearchResult> SearchUnsentDutyAsync(UnsentFicheSearchRequest req, CancellationToken ct = default)
    {
        var max = req.MaxResults is > 0 and <= 2000 ? req.MaxResults : 500;
        var hasDateRange = req.HasDateRange;
        var from = hasDateRange ? DateHelper.ToShamsiSlashDate(req.FromDate) : "";
        var to = hasDateRange ? DateHelper.ToShamsiSlashDate(req.ToDate) : "";
        var dateClause = hasDateRange ? DutyUnsentDateClause : "";
        var hasDistrict = !string.IsNullOrWhiteSpace(req.District);

        var sql = hasDistrict
            ? $"""
              SELECT TOP (@max)
                     d.FicheNo, d.NidFiche, d.BillID, d.PaymentID, d.PayablePrice AS Payable,
                     d.PaymentDate, d.BankPaymentDate, d.EumDutyFicheStatus AS EumFicheStatus,
                     '' AS NidWorkItem,
                     NULLIF(LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject="منطقه"]/Value)[1]', 'nvarchar(20)'))), '') AS District,
                     {DutyBnkAcntNoSelect}
              FROM dbo.Duty_Fiche d WITH (NOLOCK)
              WHERE NOT EXISTS (
                    SELECT 1 FROM dbo.Accounting_DocHeader h WITH (NOLOCK)
                    WHERE h.NidFiche = d.NidFiche)
                AND d.EumDutyFicheStatus <> 2
                {dateClause}
                AND (@ficheNo = '' OR d.FicheNo LIKE @ficheNoPat)
                AND (@billId = '' OR d.BillID LIKE @billIdPat)
                AND (@paymentId = '' OR d.PaymentID LIKE @paymentIdPat)
                AND LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject="منطقه"]/Value)[1]', 'nvarchar(20)'))) = @district
              ORDER BY COALESCE(d.BankPaymentDate, d.PaymentDate) DESC, d.FicheNo
              """
            : $"""
              SELECT TOP (@max)
                     d.FicheNo, d.NidFiche, d.BillID, d.PaymentID, d.PayablePrice AS Payable,
                     d.PaymentDate, d.BankPaymentDate, d.EumDutyFicheStatus AS EumFicheStatus,
                     '' AS NidWorkItem,
                     '' AS District,
                     {DutyBnkAcntNoSelect}
              FROM dbo.Duty_Fiche d WITH (NOLOCK)
              WHERE NOT EXISTS (
                    SELECT 1 FROM dbo.Accounting_DocHeader h WITH (NOLOCK)
                    WHERE h.NidFiche = d.NidFiche)
                AND d.EumDutyFicheStatus <> 2
                {dateClause}
                AND (@ficheNo = '' OR d.FicheNo LIKE @ficheNoPat)
                AND (@billId = '' OR d.BillID LIKE @billIdPat)
                AND (@paymentId = '' OR d.PaymentID LIKE @paymentIdPat)
              ORDER BY COALESCE(d.BankPaymentDate, d.PaymentDate) DESC, d.FicheNo
              """;

        var items = await ExecuteUnsentSearchAsync(sql, max, from, to, req, ct, isDuty: true, hasDateRange: hasDateRange);
        return new UnsentFicheSearchResult
        {
            FicheKind = UnsentFicheKind.Duty,
            Count = items.Count,
            Truncated = items.Count >= max,
            Items = items
        };
    }

    public sealed record BillPayBatchLookupResult(
        List<UnsentFicheListItem> Items,
        Dictionary<string, BillPayMissDiagnostic> Diagnostics);

    private const int BillPayLookupChunkSize = 80;
    private const int BillPaySqlCommandTimeoutSeconds = 20;

    /// <summary>جستجوی سریع اکسل: یک اتصال SQL، بدون join نوسازی؛ diagnose اختیاری.</summary>
    public async Task<BillPayBatchLookupResult> LookupBillPayBatchAsync(
        UnsentFicheKind kind,
        IReadOnlyList<NormalizedBillPayPair> pairs,
        bool diagnoseMisses = true,
        CancellationToken ct = default)
    {
        var items = new List<UnsentFicheListItem>();
        var diagnostics = new Dictionary<string, BillPayMissDiagnostic>(StringComparer.Ordinal);
        if (pairs == null || pairs.Count == 0)
            return new BillPayBatchLookupResult(items, diagnostics);

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);

        foreach (var chunk in pairs.Chunk(BillPayLookupChunkSize))
        {
            var chunkArray = chunk as NormalizedBillPayPair[] ?? chunk.ToArray();
            items.AddRange(await ExecuteSeekLookupAsync(
                conn, kind, chunkArray, unsentOnly: true, ct));
        }

        items = items
            .GroupBy(i => i.FicheNo, StringComparer.Ordinal)
            .Select(g => g.First())
            .ToList();

        var foundKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            var key = UnsentBillPayLookupHelper.MatchKey(item.BillId, item.PaymentId);
            if (key.Length > 0)
                foundKeys.Add(key);
        }

        var missPairs = pairs
            .Where(p => !foundKeys.Contains(UnsentBillPayLookupHelper.MatchKey(p.BillId, p.PaymentId)))
            .ToList();

        if (diagnoseMisses && missPairs.Count > 0)
            await AppendBillPayDiagnosticsOnConnectionAsync(conn, kind, missPairs, diagnostics, ct);

        return new BillPayBatchLookupResult(items, diagnostics);
    }

    /// <summary>تشخیص دلیل یافت‌نشدن فقط برای ردیف‌های miss نهایی اکسل (یک بار Income و در صورت نیاز Duty).</summary>
    public async Task<Dictionary<string, BillPayMissDiagnostic>> DiagnoseBillPayMissesAsync(
        IReadOnlyList<NormalizedBillPayPair> missPairs,
        CancellationToken ct = default)
    {
        var diagnostics = new Dictionary<string, BillPayMissDiagnostic>(StringComparer.Ordinal);
        if (missPairs == null || missPairs.Count == 0)
            return diagnostics;

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);

        await AppendBillPayDiagnosticsOnConnectionAsync(
            conn, UnsentFicheKind.Income, missPairs, diagnostics, ct);

        var stillUnknown = missPairs
            .Where(p =>
            {
                var key = UnsentBillPayLookupHelper.PairKey(p);
                return key.Length == 0 || !diagnostics.TryGetValue(key, out var d) || !d.Found;
            })
            .ToList();

        if (stillUnknown.Count > 0)
            await AppendBillPayDiagnosticsOnConnectionAsync(
                conn, UnsentFicheKind.Duty, stillUnknown, diagnostics, ct);

        return diagnostics;
    }

    private async Task AppendBillPayDiagnosticsOnConnectionAsync(
        SqlConnection conn,
        UnsentFicheKind kind,
        IReadOnlyList<NormalizedBillPayPair> pairs,
        Dictionary<string, BillPayMissDiagnostic> diagnostics,
        CancellationToken ct)
    {
        foreach (var chunk in pairs.Chunk(BillPayLookupChunkSize))
        {
            var chunkArray = chunk as NormalizedBillPayPair[] ?? chunk.ToArray();
            var rows = await ExecuteSeekDiagnoseAsync(conn, kind, chunkArray, ct);
            foreach (var row in rows)
            {
                if (!diagnostics.ContainsKey(row.PairKey))
                    diagnostics[row.PairKey] = row.Diagnostic;
            }
        }
    }

    private sealed record BillPayDiagnoseRow(string PairKey, BillPayMissDiagnostic Diagnostic);

    private async Task<List<UnsentFicheListItem>> ExecuteSeekLookupAsync(
        SqlConnection conn,
        UnsentFicheKind kind,
        NormalizedBillPayPair[] pairs,
        bool unsentOnly,
        CancellationToken ct)
    {
        var items = new List<UnsentFicheListItem>();
        var isDuty = kind == UnsentFicheKind.Duty;
        var ficheKeys = BuildFicheSeekKeys(pairs);
        if (ficheKeys.Count > 0)
        {
            var sql = BuildFicheNoSeekSql(ficheKeys.Count, isDuty, unsentOnly);
            items.AddRange(await ExecuteUnsentPairLookupOnConnectionAsync(
                conn, sql, BindFicheSeek(ficheKeys), isDuty, ct));
        }

        var direct = BuildDirectSeekKeys(pairs.Where(p => p.BillId != p.PaymentId));
        if (direct.Count > 0)
        {
            var sql = BuildBillPaySeekSql(direct.Count, isDuty, unsentOnly, swapped: false);
            items.AddRange(await ExecuteUnsentPairLookupOnConnectionAsync(
                conn, sql, BindBillPaySeek(direct), isDuty, ct));
        }

        return items;
    }

    private async Task<List<BillPayDiagnoseRow>> ExecuteSeekDiagnoseAsync(
        SqlConnection conn,
        UnsentFicheKind kind,
        NormalizedBillPayPair[] pairs,
        CancellationToken ct)
    {
        var rows = new List<BillPayDiagnoseRow>();
        var isDuty = kind == UnsentFicheKind.Duty;
        var ficheKeys = BuildFicheSeekKeys(pairs);
        if (ficheKeys.Count > 0)
            rows.AddRange(await ReadDiagnoseAsync(conn, BuildFicheNoDiagnoseSql(ficheKeys.Count, isDuty), BindFicheSeek(ficheKeys), ct));

        var distinct = pairs.Where(p => p.BillId != p.PaymentId).ToArray();
        var direct = BuildDirectSeekKeys(distinct);
        if (direct.Count > 0)
            rows.AddRange(await ReadDiagnoseAsync(conn, BuildBillPayDiagnoseSeekSql(direct.Count, isDuty, swapped: false), BindBillPaySeek(direct), ct));

        var found = new HashSet<string>(rows.Select(r => r.PairKey), StringComparer.Ordinal);
        var missingDistinct = distinct.Where(p => !found.Contains(UnsentBillPayLookupHelper.PairKey(p))).ToArray();
        var swapped = BuildDirectSeekKeys(missingDistinct);
        if (swapped.Count > 0)
            rows.AddRange(await ReadDiagnoseAsync(conn, BuildBillPayDiagnoseSeekSql(swapped.Count, isDuty, swapped: true), BindBillPaySeek(swapped), ct));

        return rows;
    }

    private sealed record FicheSeekKey(string PairBill, string PairPay, string KeyNo);
    private sealed record BillPaySeekKey(string PairBill, string PairPay, string KeyBill, string KeyPay);

    private static List<FicheSeekKey> BuildFicheSeekKeys(IEnumerable<NormalizedBillPayPair> pairs)
    {
        var keys = new List<FicheSeekKey>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var pair in pairs.Where(p => p.BillId == p.PaymentId))
        {
            var source = string.IsNullOrWhiteSpace(pair.FicheNo) ? pair.BillId : pair.FicheNo;
            foreach (var key in UnsentBillPayLookupHelper.ExpandLookupKeys(source)
                         .Concat(UnsentBillPayLookupHelper.ExpandLookupKeys(pair.BillId)))
            {
                if (!seen.Add(pair.BillId + "|" + key))
                    continue;
                keys.Add(new FicheSeekKey(pair.BillId, pair.PaymentId, key));
            }
        }

        return keys;
    }

    private static List<BillPaySeekKey> BuildDirectSeekKeys(IEnumerable<NormalizedBillPayPair> pairs)
    {
        var keys = new List<BillPaySeekKey>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var pair in pairs)
        {
            foreach (var bill in UnsentBillPayLookupHelper.ExpandLookupKeys(pair.BillId))
            foreach (var pay in UnsentBillPayLookupHelper.ExpandLookupKeys(pair.PaymentId))
            {
                if (!seen.Add(pair.BillId + "|" + pair.PaymentId + "|" + bill + "|" + pay))
                    continue;
                keys.Add(new BillPaySeekKey(pair.BillId, pair.PaymentId, bill, pay));
            }
        }

        return keys;
    }

    private static NormalizedBillPayPair[] BindFicheSeek(IReadOnlyList<FicheSeekKey> keys) =>
        keys.Select(k => new NormalizedBillPayPair(k.PairBill, k.PairPay, "", "", k.KeyNo)).ToArray();

    private static NormalizedBillPayPair[] BindBillPaySeek(IReadOnlyList<BillPaySeekKey> keys) =>
        keys.Select(k => new NormalizedBillPayPair(k.PairBill, k.PairPay, k.KeyBill, k.KeyPay, "")).ToArray();

    private static string Values3(int count) =>
        string.Join(", ", Enumerable.Range(0, count).Select(i => $"(@b{i}, @p{i}, @fn{i})"));

    private static string Values4(int count) =>
        string.Join(", ", Enumerable.Range(0, count).Select(i => $"(@b{i}, @p{i}, @bt{i}, @pt{i})"));

    private static string UnsentIncomeFilter(bool unsentOnly) => unsentOnly
        ? """
          AND f.EumFicheStatus <> 4
          AND NOT EXISTS (
                SELECT 1 FROM dbo.Accounting_DocHeader h WITH (NOLOCK)
                WHERE h.NidFiche = f.NidFiche)
          """
        : "";

    private static string UnsentDutyFilter(bool unsentOnly) => unsentOnly
        ? """
          AND d.EumDutyFicheStatus <> 2
          AND NOT EXISTS (
                SELECT 1 FROM dbo.Accounting_DocHeader h WITH (NOLOCK)
                WHERE h.NidFiche = d.NidFiche)
          """
        : "";

    private static string BuildFicheNoSeekSql(int count, bool isDuty, bool unsentOnly)
    {
        var values = Values3(count);
        if (isDuty)
        {
            return $"""
                SELECT d.FicheNo, d.NidFiche, d.BillID, d.PaymentID, d.PayablePrice AS Payable,
                       d.PaymentDate, d.BankPaymentDate, d.EumDutyFicheStatus AS EumFicheStatus,
                       '' AS NidWorkItem, '' AS District, '' AS BnkAcntNo
                FROM (VALUES {values}) AS k(PairBill, PairPay, KeyNo)
                INNER JOIN dbo.Duty_Fiche d WITH (NOLOCK) ON d.FicheNo = k.KeyNo
                WHERE 1 = 1
                {UnsentDutyFilter(unsentOnly)}
                """;
        }

        return $"""
            SELECT f.FicheNo, f.NidFiche, f.BillID, f.PaymentID, f.Payable,
                   f.PaymentDate, f.BankPaymentDate, f.EumFicheStatus,
                   f.CI_IncomeAccountGroup AS IncomeAccountGroup,
                   '' AS NidWorkItem, '' AS District, '' AS BnkAcntNo
            FROM (VALUES {values}) AS k(PairBill, PairPay, KeyNo)
            INNER JOIN dbo.Income_Fiche f WITH (NOLOCK) ON f.FicheNo = k.KeyNo
            WHERE 1 = 1
            {UnsentIncomeFilter(unsentOnly)}
            """;
    }

    private static string BuildBillPaySeekSql(int count, bool isDuty, bool unsentOnly, bool swapped)
    {
        var values = Values4(count);
        var on = swapped
            ? "f.BillID = k.KeyPay AND f.PaymentID = k.KeyBill"
            : "f.BillID = k.KeyBill AND f.PaymentID = k.KeyPay";
        var onDuty = swapped
            ? "d.BillID = k.KeyPay AND d.PaymentID = k.KeyBill"
            : "d.BillID = k.KeyBill AND d.PaymentID = k.KeyPay";
        if (isDuty)
        {
            return $"""
                SELECT d.FicheNo, d.NidFiche, d.BillID, d.PaymentID, d.PayablePrice AS Payable,
                       d.PaymentDate, d.BankPaymentDate, d.EumDutyFicheStatus AS EumFicheStatus,
                       '' AS NidWorkItem, '' AS District, '' AS BnkAcntNo
                FROM (VALUES {values}) AS k(PairBill, PairPay, KeyBill, KeyPay)
                INNER JOIN dbo.Duty_Fiche d WITH (NOLOCK) ON {onDuty}
                WHERE 1 = 1
                {UnsentDutyFilter(unsentOnly)}
                """;
        }

        return $"""
            SELECT f.FicheNo, f.NidFiche, f.BillID, f.PaymentID, f.Payable,
                   f.PaymentDate, f.BankPaymentDate, f.EumFicheStatus,
                   f.CI_IncomeAccountGroup AS IncomeAccountGroup,
                   '' AS NidWorkItem, '' AS District, '' AS BnkAcntNo
            FROM (VALUES {values}) AS k(PairBill, PairPay, KeyBill, KeyPay)
            INNER JOIN dbo.Income_Fiche f WITH (NOLOCK) ON {on}
            WHERE 1 = 1
            {UnsentIncomeFilter(unsentOnly)}
            """;
    }

    private static string BuildFicheNoDiagnoseSql(int count, bool isDuty)
    {
        var values = Values3(count);
        var status = isDuty ? "d.EumDutyFicheStatus = 2" : "f.EumFicheStatus = 4";
        var table = isDuty ? "dbo.Duty_Fiche d" : "dbo.Income_Fiche f";
        var alias = isDuty ? "d" : "f";
        return $"""
            SELECT k.PairBill, k.PairPay, {alias}.FicheNo,
                   CASE WHEN EXISTS (
                         SELECT 1 FROM dbo.Accounting_DocHeader h WITH (NOLOCK)
                         WHERE h.NidFiche = {alias}.NidFiche) THEN 1 ELSE 0 END AS Sent,
                   CASE WHEN {status} THEN 1 ELSE 0 END AS Cancelled,
                   0 AS Swapped
            FROM (VALUES {values}) AS k(PairBill, PairPay, KeyNo)
            INNER JOIN {table} WITH (NOLOCK) ON {alias}.FicheNo = k.KeyNo
            """;
    }

    private static string BuildBillPayDiagnoseSeekSql(int count, bool isDuty, bool swapped)
    {
        var values = Values4(count);
        var alias = isDuty ? "d" : "f";
        var table = isDuty ? "dbo.Duty_Fiche d" : "dbo.Income_Fiche f";
        var status = isDuty ? "d.EumDutyFicheStatus = 2" : "f.EumFicheStatus = 4";
        var on = swapped
            ? $"{alias}.BillID = k.KeyPay AND {alias}.PaymentID = k.KeyBill"
            : $"{alias}.BillID = k.KeyBill AND {alias}.PaymentID = k.KeyPay";
        var swappedFlag = swapped ? "1" : "0";
        return $"""
            SELECT k.PairBill, k.PairPay, {alias}.FicheNo,
                   CASE WHEN EXISTS (
                         SELECT 1 FROM dbo.Accounting_DocHeader h WITH (NOLOCK)
                         WHERE h.NidFiche = {alias}.NidFiche) THEN 1 ELSE 0 END AS Sent,
                   CASE WHEN {status} THEN 1 ELSE 0 END AS Cancelled,
                   {swappedFlag} AS Swapped
            FROM (VALUES {values}) AS k(PairBill, PairPay, KeyBill, KeyPay)
            INNER JOIN {table} WITH (NOLOCK) ON {on}
            """;
    }

    private async Task<List<BillPayDiagnoseRow>> ReadDiagnoseAsync(
        SqlConnection conn,
        string sql,
        NormalizedBillPayPair[] bound,
        CancellationToken ct)
    {
        var rows = new List<BillPayDiagnoseRow>();
        await using var cmd = new SqlCommand(sql, conn) { CommandTimeout = BillPaySqlCommandTimeoutSeconds };
        for (var i = 0; i < bound.Length; i++)
        {
            cmd.Parameters.Add(new SqlParameter($"@b{i}", System.Data.SqlDbType.NVarChar, 40) { Value = bound[i].BillId });
            cmd.Parameters.Add(new SqlParameter($"@p{i}", System.Data.SqlDbType.NVarChar, 40) { Value = bound[i].PaymentId });
            cmd.Parameters.Add(new SqlParameter($"@bt{i}", System.Data.SqlDbType.NVarChar, 40) { Value = bound[i].BillIdTrim ?? "" });
            cmd.Parameters.Add(new SqlParameter($"@pt{i}", System.Data.SqlDbType.NVarChar, 40) { Value = bound[i].PaymentIdTrim ?? "" });
            cmd.Parameters.Add(new SqlParameter($"@fn{i}", System.Data.SqlDbType.NVarChar, 40) { Value = bound[i].FicheNo ?? "" });
        }

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var pairKey = UnsentBillPayLookupHelper.MatchKey(
                reader.GetString(reader.GetOrdinal("PairBill")),
                reader.GetString(reader.GetOrdinal("PairPay")));
            rows.Add(new BillPayDiagnoseRow(pairKey, new BillPayMissDiagnostic
            {
                Found = true,
                FicheNo = reader.GetString(reader.GetOrdinal("FicheNo")).Trim(),
                AlreadySent = reader.GetInt32(reader.GetOrdinal("Sent")) != 0,
                Cancelled = reader.GetInt32(reader.GetOrdinal("Cancelled")) != 0,
                SwappedColumns = reader.GetInt32(reader.GetOrdinal("Swapped")) != 0
            }));
        }

        return rows;
    }

    private async Task<List<BillPayDiagnoseRow>> ExecuteBillPayDiagnoseOnConnectionAsync(
        SqlConnection conn,
        string sql,
        NormalizedBillPayPair[] pairs,
        CancellationToken ct)
    {
        var rows = new List<BillPayDiagnoseRow>();
        await using var cmd = new SqlCommand(sql, conn) { CommandTimeout = BillPaySqlCommandTimeoutSeconds };
        for (var i = 0; i < pairs.Length; i++)
        {
            cmd.Parameters.Add(new SqlParameter($"@b{i}", System.Data.SqlDbType.NVarChar, 40) { Value = pairs[i].BillId });
            cmd.Parameters.Add(new SqlParameter($"@p{i}", System.Data.SqlDbType.NVarChar, 40) { Value = pairs[i].PaymentId });
            cmd.Parameters.Add(new SqlParameter($"@bt{i}", System.Data.SqlDbType.NVarChar, 40) { Value = pairs[i].BillIdTrim ?? "" });
            cmd.Parameters.Add(new SqlParameter($"@pt{i}", System.Data.SqlDbType.NVarChar, 40) { Value = pairs[i].PaymentIdTrim ?? "" });
            cmd.Parameters.Add(new SqlParameter($"@fn{i}", System.Data.SqlDbType.NVarChar, 40) { Value = pairs[i].FicheNo ?? "" });
        }

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var pairBill = reader.GetString(reader.GetOrdinal("PairBill")).Trim();
            var pairPay = reader.GetString(reader.GetOrdinal("PairPay")).Trim();
            var pairKey = UnsentBillPayLookupHelper.MatchKey(pairBill, pairPay);
            if (pairKey.Length == 0)
                pairKey = pairBill + "|" + pairPay;
            rows.Add(new BillPayDiagnoseRow(
                pairKey,
                new BillPayMissDiagnostic
                {
                    Found = true,
                    FicheNo = reader.GetString(reader.GetOrdinal("FicheNo")).Trim(),
                    AlreadySent = reader.GetInt32(reader.GetOrdinal("Sent")) != 0,
                    Cancelled = reader.GetInt32(reader.GetOrdinal("Cancelled")) != 0,
                    SwappedColumns = reader.GetInt32(reader.GetOrdinal("Swapped")) != 0
                }));
        }

        return rows;
    }

    private static string BuildBillPayPairValuesClause(int pairCount) =>
        string.Join(", ", Enumerable.Range(0, pairCount).Select(i => $"(@b{i}, @p{i}, @bt{i}, @pt{i}, @fn{i})"));

    private static string BuildBillPayDiagnoseIncomeSql(int pairCount, bool tahatorSameBillPay)
    {
        var values = BuildBillPayPairValuesClause(pairCount);
        var match = tahatorSameBillPay
            ? BillPayIdSqlHelper.TahatorSameBillPayFicheApplyWhere("x.BillID", "x.PaymentID", "x.FicheNo")
            : BillPayIdSqlHelper.PairMatchExcelApplyWhere("x.BillID", "x.PaymentID", "x.FicheNo");
        return $"""
                SELECT p.BillId AS PairBill,
                       p.PaymentId AS PairPay,
                       hit.FicheNo,
                       hit.Sent,
                       hit.Cancelled,
                       hit.Swapped
                FROM (VALUES {values}) AS p(BillId, PaymentId, BillTrim, PayTrim, FicheNo)
                CROSS APPLY (
                    SELECT TOP 1
                           x.FicheNo,
                           CASE WHEN EXISTS (
                                 SELECT 1 FROM dbo.Accounting_DocHeader h WITH (NOLOCK)
                                 WHERE h.NidFiche = x.NidFiche) THEN 1 ELSE 0 END AS Sent,
                           CASE WHEN x.EumFicheStatus = 4 THEN 1 ELSE 0 END AS Cancelled,
                           {BillPayIdSqlHelper.SwappedColumnsCaseSql("x.BillID", "x.PaymentID", "p.BillId", "p.PaymentId")} AS Swapped
                    FROM dbo.Income_Fiche x WITH (NOLOCK)
                    WHERE {match}
                    ORDER BY COALESCE(x.BankPaymentDate, x.PaymentDate) DESC, x.FicheNo
                ) hit
                """;
    }

    private static string BuildBillPayDiagnoseDutySql(int pairCount, bool tahatorSameBillPay)
    {
        var values = BuildBillPayPairValuesClause(pairCount);
        var match = tahatorSameBillPay
            ? BillPayIdSqlHelper.TahatorSameBillPayFicheApplyWhere("x.BillID", "x.PaymentID", "x.FicheNo")
            : BillPayIdSqlHelper.PairMatchExcelApplyWhere("x.BillID", "x.PaymentID", "x.FicheNo");
        return $"""
                SELECT p.BillId AS PairBill,
                       p.PaymentId AS PairPay,
                       hit.FicheNo,
                       hit.Sent,
                       hit.Cancelled,
                       hit.Swapped
                FROM (VALUES {values}) AS p(BillId, PaymentId, BillTrim, PayTrim, FicheNo)
                CROSS APPLY (
                    SELECT TOP 1
                           x.FicheNo,
                           CASE WHEN EXISTS (
                                 SELECT 1 FROM dbo.Accounting_DocHeader h WITH (NOLOCK)
                                 WHERE h.NidFiche = x.NidFiche) THEN 1 ELSE 0 END AS Sent,
                           CASE WHEN x.EumDutyFicheStatus = 2 THEN 1 ELSE 0 END AS Cancelled,
                           {BillPayIdSqlHelper.SwappedColumnsCaseSql("x.BillID", "x.PaymentID", "p.BillId", "p.PaymentId")} AS Swapped
                    FROM dbo.Duty_Fiche x WITH (NOLOCK)
                    WHERE {match}
                    ORDER BY COALESCE(x.BankPaymentDate, x.PaymentDate) DESC, x.FicheNo
                ) hit
                """;
    }

    private static string BuildIncomePairApplySql(int pairCount, bool tahatorSameBillPay)
    {
        var values = BuildBillPayPairValuesClause(pairCount);
        var match = tahatorSameBillPay
            ? BillPayIdSqlHelper.TahatorSameBillPayFicheApplyWhere("f.BillID", "f.PaymentID", "f.FicheNo")
            : BillPayIdSqlHelper.PairMatchDistinctBillPayApplyWhere("f.BillID", "f.PaymentID", "f.FicheNo");
        return $"""
              SELECT x.FicheNo, x.NidFiche, x.BillID, x.PaymentID, x.Payable,
                     x.PaymentDate, x.BankPaymentDate, x.EumFicheStatus,
                     x.IncomeAccountGroup,
                     '' AS NidWorkItem,
                     '' AS District,
                     '' AS BnkAcntNo
              FROM (VALUES {values}) AS p(BillId, PaymentId, BillTrim, PayTrim, FicheNo)
              CROSS APPLY (
                  SELECT TOP 1
                         f.FicheNo, f.NidFiche, f.BillID, f.PaymentID, f.Payable,
                         f.PaymentDate, f.BankPaymentDate, f.EumFicheStatus,
                         f.CI_IncomeAccountGroup AS IncomeAccountGroup
                  FROM dbo.Income_Fiche f WITH (NOLOCK)
                  WHERE {match}
                    AND NOT EXISTS (
                          SELECT 1 FROM dbo.Accounting_DocHeader h WITH (NOLOCK)
                          WHERE h.NidFiche = f.NidFiche)
                    AND f.EumFicheStatus <> 4
                  ORDER BY COALESCE(f.BankPaymentDate, f.PaymentDate) DESC, f.FicheNo
              ) x
              """;
    }

    private static string BuildDutyPairApplySql(int pairCount, bool tahatorSameBillPay)
    {
        var values = BuildBillPayPairValuesClause(pairCount);
        var match = tahatorSameBillPay
            ? BillPayIdSqlHelper.TahatorSameBillPayFicheApplyWhere("d.BillID", "d.PaymentID", "d.FicheNo")
            : BillPayIdSqlHelper.PairMatchDistinctBillPayApplyWhere("d.BillID", "d.PaymentID", "d.FicheNo");
        return $"""
              SELECT x.FicheNo, x.NidFiche, x.BillID, x.PaymentID, x.Payable,
                     x.PaymentDate, x.BankPaymentDate, x.EumFicheStatus,
                     '' AS NidWorkItem,
                     '' AS District,
                     '' AS BnkAcntNo
              FROM (VALUES {values}) AS p(BillId, PaymentId, BillTrim, PayTrim, FicheNo)
              CROSS APPLY (
                  SELECT TOP 1
                         d.FicheNo, d.NidFiche, d.BillID, d.PaymentID, d.PayablePrice AS Payable,
                         d.PaymentDate, d.BankPaymentDate, d.EumDutyFicheStatus AS EumFicheStatus
                  FROM dbo.Duty_Fiche d WITH (NOLOCK)
                  WHERE {match}
                    AND NOT EXISTS (
                          SELECT 1 FROM dbo.Accounting_DocHeader h WITH (NOLOCK)
                          WHERE h.NidFiche = d.NidFiche)
                    AND d.EumDutyFicheStatus <> 2
                  ORDER BY COALESCE(d.BankPaymentDate, d.PaymentDate) DESC, d.FicheNo
              ) x
              """;
    }

    private async Task<List<UnsentFicheListItem>> ExecuteUnsentPairLookupOnConnectionAsync(
        SqlConnection conn,
        string sql,
        NormalizedBillPayPair[] pairs,
        bool isDuty,
        CancellationToken ct)
    {
        var items = new List<UnsentFicheListItem>();
        await using var cmd = new SqlCommand(sql, conn) { CommandTimeout = BillPaySqlCommandTimeoutSeconds };
        for (var i = 0; i < pairs.Length; i++)
        {
            cmd.Parameters.Add(new SqlParameter($"@b{i}", System.Data.SqlDbType.NVarChar, 40) { Value = pairs[i].BillId });
            cmd.Parameters.Add(new SqlParameter($"@p{i}", System.Data.SqlDbType.NVarChar, 40) { Value = pairs[i].PaymentId });
            cmd.Parameters.Add(new SqlParameter($"@bt{i}", System.Data.SqlDbType.NVarChar, 40) { Value = pairs[i].BillIdTrim ?? "" });
            cmd.Parameters.Add(new SqlParameter($"@pt{i}", System.Data.SqlDbType.NVarChar, 40) { Value = pairs[i].PaymentIdTrim ?? "" });
            cmd.Parameters.Add(new SqlParameter($"@fn{i}", System.Data.SqlDbType.NVarChar, 40) { Value = pairs[i].FicheNo ?? "" });
        }

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            items.Add(ReadUnsentFicheListItem(reader, isDuty));

        return items;
    }

    private async Task<List<UnsentFicheListItem>> ExecuteUnsentPairLookupAsync(
        string sql,
        NormalizedBillPayPair[] pairs,
        bool isDuty,
        CancellationToken ct)
    {
        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        return await ExecuteUnsentPairLookupOnConnectionAsync(conn, sql, pairs, isDuty, ct);
    }

    private async Task<List<UnsentFicheListItem>> ExecuteUnsentSearchAsync(
        string sql, int max, string from, string to, UnsentFicheSearchRequest req, CancellationToken ct,
        bool isDuty = false, bool hasDateRange = false)
    {
        var items = new List<UnsentFicheListItem>();
        var ficheNo = IdentifierText.Normalize(req.FicheNo);
        var billId = IdentifierText.Normalize(req.BillId);
        var paymentId = IdentifierText.Normalize(req.PaymentId);

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 180 };
        cmd.Parameters.AddWithValue("@max", max);
        if (hasDateRange)
        {
            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);
        }
        cmd.Parameters.AddWithValue("@ficheNo", ficheNo);
        cmd.Parameters.AddWithValue("@billId", billId);
        cmd.Parameters.AddWithValue("@paymentId", paymentId);
        cmd.Parameters.AddWithValue("@ficheNoPat", string.IsNullOrEmpty(ficheNo) ? "%" : $"%{ficheNo}%");
        cmd.Parameters.AddWithValue("@billIdPat", string.IsNullOrEmpty(billId) ? "%" : $"%{billId}%");
        cmd.Parameters.AddWithValue("@paymentIdPat", string.IsNullOrEmpty(paymentId) ? "%" : $"%{paymentId}%");
        cmd.Parameters.AddWithValue("@district", (req.District ?? "").Trim());
        cmd.Parameters.AddWithValue("@restrictAg", (object?)req.RestrictIncomeAccountGroup ?? DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            items.Add(ReadUnsentFicheListItem(reader, isDuty));

        return items;
    }

    private static UnsentFicheListItem ReadUnsentFicheListItem(SqlDataReader reader, bool isDuty)
    {
        var group = isDuty ? 0 : ReadInt32(reader, "IncomeAccountGroup");
        var isTahator = !isDuty && group is TahatorRowBuilder.IncomeAccountGroupTahatorAmount
            or TahatorRowBuilder.IncomeAccountGroupTahatorIncome;
        return new UnsentFicheListItem
        {
            FicheNo = reader.GetString(reader.GetOrdinal("FicheNo")).Trim(),
            NidFiche = reader.GetGuid(reader.GetOrdinal("NidFiche")),
            NidWorkItem = reader.IsDBNull(reader.GetOrdinal("NidWorkItem"))
                ? ""
                : reader.GetString(reader.GetOrdinal("NidWorkItem")).Trim(),
            BillId = BankInquiryConfirmHelper.NormalizeBillOrPayId(reader.GetString(reader.GetOrdinal("BillID"))),
            PaymentId = BankInquiryConfirmHelper.NormalizeBillOrPayId(reader.GetString(reader.GetOrdinal("PaymentID"))),
            Payable = ReadDecimal(reader, "Payable"),
            PaymentDate = ReadRowDate(reader, "PaymentDate"),
            BankPaymentDate = ReadRowDate(reader, "BankPaymentDate"),
            Status = ReadInt32(reader, "EumFicheStatus"),
            District = reader.IsDBNull(reader.GetOrdinal("District"))
                ? null
                : reader.GetString(reader.GetOrdinal("District")).Trim(),
            BnkAcntNo = reader.IsDBNull(reader.GetOrdinal("BnkAcntNo"))
                ? ""
                : reader.GetString(reader.GetOrdinal("BnkAcntNo")).Trim(),
            IncomeAccountGroup = isDuty ? null : group,
            IsTahator = isTahator,
            SubKindLabel = isDuty ? "نوسازی/صنفی" : isTahator ? "تهاتر" : "درآمدی"
        };
    }

    private static long? ReadNullableInt64(SqlDataReader reader, string column)
    {
        var ord = reader.GetOrdinal(column);
        if (reader.IsDBNull(ord)) return null;
        var text = reader.GetString(ord).Trim();
        return long.TryParse(text, out var value) ? value : null;
    }

    private static int ReadInt32(SqlDataReader reader, string column)
    {
        var ord = reader.GetOrdinal(column);
        if (reader.IsDBNull(ord)) return 0;
        return Convert.ToInt32(reader.GetValue(ord));
    }

    private static decimal ReadDecimal(SqlDataReader reader, string column)
    {
        var ord = reader.GetOrdinal(column);
        if (reader.IsDBNull(ord)) return 0;
        return Convert.ToDecimal(reader.GetValue(ord));
    }

    private static string ReadRowDate(SqlDataReader reader, string column)
    {
        var ord = reader.GetOrdinal(column);
        if (reader.IsDBNull(ord)) return "";
        var value = reader.GetValue(ord);
        return value switch
        {
            DateTime => DateHelper.FromDatabaseDateValue(value),
            _ => DateHelper.ToRayvarzDate(value.ToString() ?? "")
        };
    }
}
