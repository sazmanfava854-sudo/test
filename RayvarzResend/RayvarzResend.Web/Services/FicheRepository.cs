using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public class FicheRepository
{
    /// <summary>
    /// کد نوسازی Rayvarz (GetNosaziNickName): منطقه-حوزه-بلوک-ملک-ساختمان-آپارتمان-واحد — بدون CI_City.
    /// </summary>
    private const string NosaziNickSql = """
        CAST(b.District AS varchar) + '-' + CAST(b.Region AS varchar) + '-' +
        CAST(b.Block AS varchar) + '-' + CAST(b.House AS varchar) + '-' +
        CAST(b.Building AS varchar) + '-' + CAST(b.Apartment AS varchar) + '-' +
        ISNULL(NULLIF(CAST(b.Shop AS varchar), ''), '0')
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
        var income = await TryLoadIncomeAsync(type, value, ct);
        if (income != null) return income;
        return await TryLoadDutyAsync(type, value, ct);
    }

    public async Task<FicheHeaderDto?> LoadByKindAsync(
        UnsentFicheKind kind, IdentifierType type, string value, CancellationToken ct = default)
    {
        return kind == UnsentFicheKind.Duty
            ? await TryLoadDutyAsync(type, value, ct)
            : await TryLoadIncomeAsync(type, value, ct);
    }

    /// <summary>نوع شناسه را حدس می‌زند؛ در صورت عدم یافتن نوع دیگر را امتحان می‌کند.</summary>
    public async Task<(FicheHeaderDto? Fiche, IdentifierType UsedType)> LoadByKindWithAutoDetectAsync(
        UnsentFicheKind kind, string rawValue, CancellationToken ct = default)
    {
        var value = rawValue.Trim();
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
        var where = type == IdentifierType.FicheNo
            ? "f.FicheNo = @val"
            : "f.BillID + f.PaymentID = @val";

        var sql = $@"
SELECT f.FicheNo, f.BillID, f.PaymentID, f.Payable, f.NidFiche, f.NidIncome,
       NULLIF(LTRIM(RTRIM(CAST(f.PaymentBranch AS nvarchar(20)))), '') AS PaymentBranch,
       NULLIF(LTRIM(RTRIM(CAST(f.CI_Bank AS nvarchar(20)))), '') AS CiBank,
       NULLIF(LTRIM(RTRIM(CAST(f.PaymentBank AS nvarchar(20)))), '') AS PaymentBank,
       COALESCE(f.BankPaymentDate, f.PaymentDate) AS RowDate,
       f.PaymentDate,
       f.BankPaymentDate,
       f.EumFicheStatus, f.CI_IncomeAccountGroup,
       CAST(f.CheckNo AS nvarchar(20)) AS CheckNo,
       NULLIF(LTRIM(RTRIM(CAST(f.Deposit AS nvarchar(50)))), '') AS Deposit,
       NULLIF(LTRIM(RTRIM(CAST(f.DepositID AS nvarchar(50)))), '') AS DepositID,
       NULLIF(LTRIM(RTRIM(CAST(f.CreditorPapers AS nvarchar(50)))), '') AS CreditorPapers,
       CAST(r.NidWorkItem AS nvarchar(50)) AS RefReconstructionNo,
       ISNULL(
         NULLIF(LTRIM(RTRIM({NosaziNickSql})), '-'),
         ''
       ) AS BnkAcntNoNosaziNick,
       NULLIF(LTRIM(RTRIM(CAST(b.District AS nvarchar(20)))), '') AS NosaziDistrict
FROM dbo.Income_Fiche f
JOIN dbo.Income i ON i.NidIncome = f.NidIncome
LEFT JOIN dbo.Sh_RequestInfo r ON r.NidProc = i.NidProc
LEFT JOIN dbo.Base_NosaziCode b ON b.NidNosaziCode = r.NidNosaziCode
WHERE {where}";

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@val", value.Trim());

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;

        var group = ReadInt32(reader, "CI_IncomeAccountGroup");
        var isTahatorAmount = group == TahatorRowBuilder.IncomeAccountGroupTahatorAmount;
        var isTahatorIncome = group == TahatorRowBuilder.IncomeAccountGroupTahatorIncome;
        var isTahator = isTahatorAmount || isTahatorIncome;
        // DocTyp نهایی بعد از ApplyTahator* با CI_Bank تصحیح می‌شود (۴→۱۴|۱۷ ، غیر۴→۱۵|۱۸)
        var docTyp = group == 150
            ? 11
            : isTahatorAmount
                ? 14
                : isTahatorIncome
                    ? 18
                    : 3;
        var ciBank = reader.IsDBNull(reader.GetOrdinal("CiBank")) ? null : reader.GetString(reader.GetOrdinal("CiBank"));
        var paymentBank = reader.IsDBNull(reader.GetOrdinal("PaymentBank")) ? null : reader.GetString(reader.GetOrdinal("PaymentBank"));
        var nick = reader.IsDBNull(reader.GetOrdinal("BnkAcntNoNosaziNick"))
            ? ""
            : reader.GetString(reader.GetOrdinal("BnkAcntNoNosaziNick"));
        var district = reader.IsDBNull(reader.GetOrdinal("NosaziDistrict"))
            ? null
            : reader.GetString(reader.GetOrdinal("NosaziDistrict"));

        var dto = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            FicheNo = reader.GetString(reader.GetOrdinal("FicheNo")),
            BillId = reader.GetString(reader.GetOrdinal("BillID")),
            PaymentId = reader.GetString(reader.GetOrdinal("PaymentID")),
            Payable = ReadDecimal(reader, "Payable"),
            NidFiche = reader.GetGuid(reader.GetOrdinal("NidFiche")),
            NidIncome = reader.GetGuid(reader.GetOrdinal("NidIncome")),
            PaymentBranch = reader.IsDBNull(reader.GetOrdinal("PaymentBranch"))
                ? ""
                : reader.GetString(reader.GetOrdinal("PaymentBranch")),
            // تهاتر: DocTyp/IncmNo از CI_Bank؛ فیلد Bank در SOAP از PaymentBranch است
            BankCode = isTahator
                ? (ciBank ?? paymentBank)
                : (paymentBank ?? ciBank),
            RowDate = ReadRowDate(reader, "RowDate"),
            CurrentStatus = ReadInt32(reader, "EumFicheStatus"),
            IncomeAccountGroup = group,
            CheckNo = reader.IsDBNull(reader.GetOrdinal("CheckNo")) ? null : reader.GetString(reader.GetOrdinal("CheckNo")),
            Deposit = ReadNullableInt64(reader, "Deposit"),
            DepositId = ReadNullableInt64(reader, "DepositID"),
            CreditorPapers = ReadNullableInt64(reader, "CreditorPapers"),
            RefReconstructionNo = reader.IsDBNull(reader.GetOrdinal("RefReconstructionNo")) ? null : reader.GetString(reader.GetOrdinal("RefReconstructionNo")),
            BnkAcntNo = nick,
            BnkAcntNoSource = "کد نوسازی — GetNosaziNickName (منطقه-حوزه-بلوک-ملک-ساختمان-آپارتمان-واحد)",
            IncomeRegion = district,
            DocTyp = docTyp,
            DocDsc = isTahatorAmount
                ? "اسناد تهاتر مبلغ"
                : isTahatorIncome
                    ? "اسناد تهاتر درامد"
                    : "اسناد شهرسازی"
        };

        if (isTahatorAmount)
        {
            // ردیف SOAP تهاتر مبلغ (نه Income_Calculation) — مطابق Tahator1 / مرکز
            TahatorRowBuilder.ApplyTahatorAmountRows(dto);
        }
        else if (isTahatorIncome)
        {
            // درآمدی تهاتر: ردیف از Calculation + DocTyp ۱۷/۱۸ + Branch منطقه
            dto.Rows = await LoadIncomeRowsAsync(dto.NidIncome!.Value, ct);
            IncomeRowScaler.ScaleToPayable(dto.Rows, dto.Payable);
            TahatorRowBuilder.ApplyTahatorIncomeRows(dto);
        }
        else
        {
            dto.Rows = await LoadIncomeRowsAsync(dto.NidIncome!.Value, ct);
            // Income_Calculation = مبلغ ناخالص؛ PayablePrice پس از تخفیف است — مثل SOAP اسکیل کن
            IncomeRowScaler.ScaleToPayable(dto.Rows, dto.Payable);
        }

        FicheDateResolver.ApplyFromIncomeColumns(
            dto,
            dto.CurrentStatus,
            ReadRowDate(reader, "PaymentDate"),
            ReadRowDate(reader, "BankPaymentDate"));
        return dto;
    }

    private async Task<List<IncmRowDto>> LoadIncomeRowsAsync(Guid nidIncome, CancellationToken ct)
    {
        const string sql = @"
SELECT ic.CI_IncomeCalculation AS IncmNo,
       COALESCE(ic.SysValue, ic.Value) AS Val,
       ISNULL(c.Title, '') AS IncmRowDsc
FROM dbo.Income_Calculation ic
LEFT JOIN dbo.CI_IncomeCalculation c ON c.ID = ic.CI_IncomeCalculation
WHERE ic.NidIncome = @nid";

        var rows = new List<IncmRowDto>();
        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nid", nidIncome);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var incmNo = ReadInt32(reader, "IncmNo");
            if (IncomeExcludedCodes.Codes.Contains(incmNo)) continue;
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
        var where = type == IdentifierType.FicheNo
            ? "d.FicheNo = @val"
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
WHERE {where}";

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
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
            var nick = await TryLoadNosaziNickNameAsync(nidFiche, ct);
            if (!string.IsNullOrWhiteSpace(nick))
            {
                dto.BnkAcntNo = nick;
                dto.BnkAcntNoSource = "کد نوسازی — GetNosaziNickName (Duty_FicheSub.NidFK → Base_NosaziCode)";
            }
        }

        if (isSenfi)
        {
            dto.BnkAcntNo = "7-14-55-1-1-0-1";
            dto.BnkAcntNoSource = "کد ثابت صنفی — Rayvarz (7-14-55-1-1-0-1)";
        }

        dto.Rows = await LoadDutyRowsAsync(dto.NidFiche, dto.Payable, isSenfi, exportType, ct);
        DutyNosaziLogic.ApplyRayvarzDates(dto, dutyStatus, paymentDateRay, bankPaymentDateRay);
        return dto;
    }

    private async Task<List<IncmRowDto>> LoadDutyRowsAsync(Guid nidFiche, decimal payable, bool isSenfi, int exportType, CancellationToken ct)
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

        var amounts = DutyNosaziLogic.CalculateSubAmounts(subs, payable);
        return DutyNosaziLogic.BuildIncmRows(amounts, isSenfi, exportType);
    }

    private async Task<string?> TryLoadNosaziNickNameAsync(Guid nidFiche, CancellationToken ct)
    {
        var sql = $@"
SELECT TOP 1
  {NosaziNickSql} AS Nick
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
            return string.IsNullOrWhiteSpace(s) || s == "-------0" ? null : s;
        }
        catch (SqlException)
        {
            return null;
        }
    }

    public async Task<bool> ExistsInRayvarzAsync(string ficheNo, int? shamsiYear = null, CancellationToken ct = default)
    {
        ficheNo = TahatorRowBuilder.NormalizeFicheNo(ficheNo.Trim());
        if (await QueryExistsInRayvarzAsync(ficheNo, shamsiYear, docTypFilter: null, ct))
            return true;

        // SOAP ممکن است با DocDate متفاوت از PaymentDate در yr دیگری ثبت شود
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
                ? """
                  SELECT TOP 1 1 FROM ray.incmdocsys
                  WHERE yr = @yr AND RowDocNo = @f AND DocTyp IN (@d0, @d1)
                  """
                : """
                  SELECT TOP 1 1 FROM ray.incmdocsys
                  WHERE RowDocNo = @f AND DocTyp IN (@d0, @d1)
                  """
            : shamsiYear is > 0
                ? """
                  SELECT TOP 1 1 FROM ray.incmdocsys
                  WHERE yr = @yr AND (Ref = @f OR RowDocNo = @f)
                  """
                : """
                  SELECT TOP 1 1 FROM ray.incmdocsys
                  WHERE Ref = @f OR RowDocNo = @f
                  """;

        await using var conn = new SqlConnection(_rayCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@f", ficheNo);
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

    public async Task ResetStatusAsync(FicheHeaderDto fiche, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);

        if (fiche.Category == FicheCategory.Income)
        {
            const string sql = @"UPDATE dbo.Income_Fiche SET EumFicheStatus = 2
WHERE FicheNo = @f AND EumFicheStatus IN (5, 7)";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@f", fiche.FicheNo);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        else
        {
            const string sql = @"UPDATE dbo.Duty_Fiche SET EumDutyFicheStatus = 1
WHERE FicheNo = @f AND EumDutyFicheStatus = 4";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@f", fiche.FicheNo);
            await cmd.ExecuteNonQueryAsync(ct);
        }
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
                     NULLIF(LTRIM(RTRIM(CAST(b.District AS nvarchar(20)))), '') AS District,
                     '' AS BnkAcntNo
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
                AND CAST(b.District AS nvarchar(20)) = @district
              ORDER BY COALESCE(f.BankPaymentDate, f.PaymentDate) DESC, f.FicheNo
              """
            : $"""
              SELECT TOP (@max)
                     f.FicheNo, f.NidFiche, f.BillID, f.PaymentID, f.Payable,
                     f.PaymentDate, f.BankPaymentDate, f.EumFicheStatus,
                     f.CI_IncomeAccountGroup AS IncomeAccountGroup,
                     '' AS District, '' AS BnkAcntNo
              FROM dbo.Income_Fiche f WITH (NOLOCK)
              WHERE NOT EXISTS (
                    SELECT 1 FROM dbo.Accounting_DocHeader h WITH (NOLOCK)
                    WHERE h.NidFiche = f.NidFiche)
                AND f.EumFicheStatus <> 4
                {dateClause}
                AND (@ficheNo = '' OR f.FicheNo LIKE @ficheNoPat)
                AND (@billId = '' OR f.BillID LIKE @billIdPat)
                AND (@paymentId = '' OR f.PaymentID LIKE @paymentIdPat)
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
                     NULLIF(LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject=""منطقه""]/Value)[1]', 'nvarchar(20)'))), '') AS District,
                     '' AS BnkAcntNo
              FROM dbo.Duty_Fiche d WITH (NOLOCK)
              WHERE NOT EXISTS (
                    SELECT 1 FROM dbo.Accounting_DocHeader h WITH (NOLOCK)
                    WHERE h.NidFiche = d.NidFiche)
                AND d.EumDutyFicheStatus <> 2
                {dateClause}
                AND (@ficheNo = '' OR d.FicheNo LIKE @ficheNoPat)
                AND (@billId = '' OR d.BillID LIKE @billIdPat)
                AND (@paymentId = '' OR d.PaymentID LIKE @paymentIdPat)
                AND LTRIM(RTRIM(d.OtherFields.value('(//ClsLog[Subject=""منطقه""]/Value)[1]', 'nvarchar(20)'))) = @district
              ORDER BY COALESCE(d.BankPaymentDate, d.PaymentDate) DESC, d.FicheNo
              """
            : $"""
              SELECT TOP (@max)
                     d.FicheNo, d.NidFiche, d.BillID, d.PaymentID, d.PayablePrice AS Payable,
                     d.PaymentDate, d.BankPaymentDate, d.EumDutyFicheStatus AS EumFicheStatus,
                     '' AS District, '' AS BnkAcntNo
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

    private async Task<List<UnsentFicheListItem>> ExecuteUnsentSearchAsync(
        string sql, int max, string from, string to, UnsentFicheSearchRequest req, CancellationToken ct,
        bool isDuty = false, bool hasDateRange = false)
    {
        var items = new List<UnsentFicheListItem>();
        var ficheNo = (req.FicheNo ?? "").Trim();
        var billId = (req.BillId ?? "").Trim();
        var paymentId = (req.PaymentId ?? "").Trim();

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

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var group = isDuty ? 0 : ReadInt32(reader, "IncomeAccountGroup");
            var isTahator = !isDuty && group is TahatorRowBuilder.IncomeAccountGroupTahatorAmount
                or TahatorRowBuilder.IncomeAccountGroupTahatorIncome;
            items.Add(new UnsentFicheListItem
            {
                FicheNo = reader.GetString(reader.GetOrdinal("FicheNo")).Trim(),
                NidFiche = reader.GetGuid(reader.GetOrdinal("NidFiche")),
                BillId = reader.GetString(reader.GetOrdinal("BillID")).Trim(),
                PaymentId = reader.GetString(reader.GetOrdinal("PaymentID")).Trim(),
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
            });
        }

        return items;
    }

    private static long? ReadNullableInt64(SqlDataReader reader, string column)
    {
        var ord = reader.GetOrdinal(column);
        if (reader.IsDBNull(ord)) return null;
        return NumericHelper.TryParseLegacyLong(reader.GetValue(ord));
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
