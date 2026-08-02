using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public class FicheRepository
{
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

    private async Task<FicheHeaderDto?> TryLoadIncomeAsync(IdentifierType type, string value, CancellationToken ct)
    {
        var where = type == IdentifierType.FicheNo
            ? "f.FicheNo = @val"
            : "f.BillID + f.PaymentID = @val";

        var sql = $@"
SELECT f.FicheNo, f.BillID, f.PaymentID, f.Payable, f.NidFiche, f.NidIncome,
       ISNULL(CAST(f.PaymentBranch AS nvarchar(20)), '18') AS PaymentBranch,
       COALESCE(
         NULLIF(LTRIM(RTRIM(CAST(f.CI_Bank AS nvarchar(20)))), ''),
         NULLIF(LTRIM(RTRIM(CAST(f.PaymentBank AS nvarchar(20)))), '')
       ) AS BankCode,
       COALESCE(f.BankPaymentDate, f.PaymentDate) AS RowDate,
       f.PaymentDate,
       f.BankPaymentDate,
       f.EumFicheStatus, f.CI_IncomeAccountGroup,
       CAST(f.CheckNo AS nvarchar(20)) AS CheckNo,
       CAST(f.Deposit AS bigint) AS Deposit,
       CAST(f.DepositID AS bigint) AS DepositID,
       CAST(f.CreditorPapers AS bigint) AS CreditorPapers,
       CAST(r.NidWorkItem AS nvarchar(50)) AS RefReconstructionNo,
       ISNULL(
         NULLIF(LTRIM(RTRIM(
           CAST(b.CI_City AS varchar) + '-' + CAST(b.District AS varchar) + '-' +
           CAST(b.Region AS varchar) + '-' + CAST(b.Block AS varchar) + '-' +
           CAST(b.House AS varchar) + '-' + CAST(b.Building AS varchar) + '-' +
           CAST(b.Apartment AS varchar)
         )), '-'),
         ''
       ) AS BnkAcntNo,
       NULLIF(LTRIM(RTRIM(CAST(b.CI_City AS nvarchar(20)))), '') AS IncomeRegion
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
        var docTyp = group == 150 ? 11 : group == TahatorRowBuilder.IncomeAccountGroupTahator ? 14 : 3;

        var dto = new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            FicheNo = reader.GetString(reader.GetOrdinal("FicheNo")),
            BillId = reader.GetString(reader.GetOrdinal("BillID")),
            PaymentId = reader.GetString(reader.GetOrdinal("PaymentID")),
            Payable = ReadDecimal(reader, "Payable"),
            NidFiche = reader.GetGuid(reader.GetOrdinal("NidFiche")),
            NidIncome = reader.GetGuid(reader.GetOrdinal("NidIncome")),
            PaymentBranch = reader.GetString(reader.GetOrdinal("PaymentBranch")),
            BankCode = reader.IsDBNull(reader.GetOrdinal("BankCode")) ? null : reader.GetString(reader.GetOrdinal("BankCode")),
            RowDate = ReadRowDate(reader, "RowDate"),
            CurrentStatus = ReadInt32(reader, "EumFicheStatus"),
            IncomeAccountGroup = group,
            CheckNo = reader.IsDBNull(reader.GetOrdinal("CheckNo")) ? null : reader.GetString(reader.GetOrdinal("CheckNo")),
            Deposit = reader.IsDBNull(reader.GetOrdinal("Deposit")) ? null : Convert.ToInt64(reader.GetValue(reader.GetOrdinal("Deposit"))),
            DepositId = reader.IsDBNull(reader.GetOrdinal("DepositID")) ? null : Convert.ToInt64(reader.GetValue(reader.GetOrdinal("DepositID"))),
            CreditorPapers = reader.IsDBNull(reader.GetOrdinal("CreditorPapers")) ? null : Convert.ToInt64(reader.GetValue(reader.GetOrdinal("CreditorPapers"))),
            RefReconstructionNo = reader.IsDBNull(reader.GetOrdinal("RefReconstructionNo")) ? null : reader.GetString(reader.GetOrdinal("RefReconstructionNo")),
            BnkAcntNo = reader.IsDBNull(reader.GetOrdinal("BnkAcntNo")) ? "" : reader.GetString(reader.GetOrdinal("BnkAcntNo")),
            BnkAcntNoSource = "کد نوسازی — از Base_NosaziCode (۷ بخش، مثل نوسازی)",
            IncomeRegion = reader.IsDBNull(reader.GetOrdinal("IncomeRegion")) ? null : reader.GetString(reader.GetOrdinal("IncomeRegion")),
            DocTyp = docTyp,
            DocDsc = group == TahatorRowBuilder.IncomeAccountGroupTahator ? "اسناد تهاتر مبلغ" : "اسناد شهرسازی"
        };

        if (group == TahatorRowBuilder.IncomeAccountGroupTahator)
        {
            // ردیف SOAP تهاتر (نه Income_Calculation) — مطابق Tahator1 + نمونه‌های golden
            TahatorRowBuilder.ApplyTahatorRows(dto);
        }
        else
        {
            dto.Rows = await LoadIncomeRowsAsync(dto.NidIncome!.Value, ct);
            // Income_Calculation = مبلغ ناخالص؛ PayablePrice پس از تخفیف است — مثل SOAP اسکیل کن
            IncomeRowScaler.ScaleToPayable(dto.Rows, dto.Payable);
        }

        FicheDateResolver.ApplyFromIncomeColumns(
            dto,
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
        const string sql = @"
SELECT TOP 1
  CAST(b.CI_City AS varchar) + '-' + CAST(b.District AS varchar) + '-' +
  CAST(b.Region AS varchar) + '-' + CAST(b.Block AS varchar) + '-' +
  CAST(b.House AS varchar) + '-' + CAST(b.Building AS varchar) + '-' +
  CAST(b.Apartment AS varchar) + '-' +
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
            return string.IsNullOrWhiteSpace(s) || s == "-------0" ? null : s;
        }
        catch (SqlException)
        {
            return null;
        }
    }

    public async Task<bool> ExistsInRayvarzAsync(string ficheNo, int? shamsiYear = null, CancellationToken ct = default)
    {
        var sql = shamsiYear is > 0
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
