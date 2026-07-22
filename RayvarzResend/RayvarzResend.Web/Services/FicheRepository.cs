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
       NULLIF(LTRIM(RTRIM(CAST(f.PaymentBank AS nvarchar(20)))), '') AS BankCode,
       COALESCE(f.BankPaymentDate, f.PaymentDate) AS RowDate,
       f.EumFicheStatus, f.CI_IncomeAccountGroup,
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
        var docTyp = group == 150 ? 11 : 3;

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
            RefReconstructionNo = reader.IsDBNull(reader.GetOrdinal("RefReconstructionNo")) ? null : reader.GetString(reader.GetOrdinal("RefReconstructionNo")),
            BnkAcntNo = reader.IsDBNull(reader.GetOrdinal("BnkAcntNo")) ? "" : reader.GetString(reader.GetOrdinal("BnkAcntNo")),
            BnkAcntNoSource = "کد نوسازی — از Base_NosaziCode (۷ بخش، مثل نوسازی)",
            IncomeRegion = reader.IsDBNull(reader.GetOrdinal("IncomeRegion")) ? null : reader.GetString(reader.GetOrdinal("IncomeRegion")),
            DocTyp = docTyp,
            DocDsc = "اسناد شهرسازی"
        };

        dto.Rows = await LoadIncomeRowsAsync(dto.NidIncome!.Value, ct);
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
        var isSenfi = dutyType == 2;

        var dto = new FicheHeaderDto
        {
            Category = isSenfi ? FicheCategory.DutySenfi : FicheCategory.DutyNosazi,
            FicheNo = reader.GetString(reader.GetOrdinal("FicheNo")),
            BillId = reader.GetString(reader.GetOrdinal("BillID")),
            PaymentId = reader.GetString(reader.GetOrdinal("PaymentID")),
            Payable = ReadDecimal(reader, "Payable"),
            NidFiche = reader.GetGuid(reader.GetOrdinal("NidFiche")),
            PaymentBranch = reader.GetString(reader.GetOrdinal("PaymentBranch")),
            BankCode = reader.IsDBNull(reader.GetOrdinal("BankCode")) ? null : reader.GetString(reader.GetOrdinal("BankCode")),
            RowDate = ReadRowDate(reader, "RowDate"),
            CurrentStatus = ReadInt32(reader, "EumDutyFicheStatus"),
            DutyExportType = exportType,
            BnkAcntNo = reader.IsDBNull(reader.GetOrdinal("BnkAcntNo")) ? "" : reader.GetString(reader.GetOrdinal("BnkAcntNo")),
            BnkAcntNoSource = "کد نوسازی — از Duty_Fiche.OtherFields (XML فیش)",
            DutyRegion = reader.IsDBNull(reader.GetOrdinal("DutyRegion")) ? null : reader.GetString(reader.GetOrdinal("DutyRegion")),
            DocTyp = isSenfi ? 2 : 1,
            DocDsc = isSenfi ? "اسناد صنفی" : "اسناد نوسازی"
        };

        if (isSenfi)
        {
            dto.BnkAcntNo = "7-14-55-1-1-0-1";
            dto.BnkAcntNoSource = "کد ثابت صنفی — Rayvarz (7-14-55-1-1-0-1)";
        }

        dto.Rows = await LoadDutyRowsAsync(dto.NidFiche, dto.Payable, isSenfi, exportType, ct);
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

        // تأیید: نوسازی 101104/9881711 | صنفی 051204/19920388
        // 100003 ← SUM(F3,F0) | 206098003 ← SUM(F3,F16) | 100002 ← SUM(F5,F0)
        // 2003/100062 ← PayablePrice − آتش‌نشانی − پسماند − ارزش‌افزوده
        const int GarbageFormula = 3;
        const int AtashFormula = 5;
        const int AfzodehFiche = 16;

        decimal Afzodeh = subs.Where(s => s.Formula == GarbageFormula && s.Fiche == AfzodehFiche).Sum(s => s.Price);
        decimal Atash = subs.Where(s => s.Formula == AtashFormula && s.Fiche == 0).Sum(s => s.Price);
        decimal Garbage = subs.Where(s => s.Formula == GarbageFormula && s.Fiche == 0).Sum(s => s.Price);
        decimal Nosazi = payable - Atash - Garbage - Afzodeh;

        // ExportType=14 (بانک‌ها): IncmNo=2005 — تأیید 021204/19379176
        var mainIncm = isSenfi switch
        {
            true when exportType == 14 => 2005,
            true => 100062,
            false => 2003
        };
        var mainDsc = mainIncm switch
        {
            2005 => "عوارض ساليانه بانک ها و موسسات اعتباري",
            100062 => "صنفي",
            _ => "نوسازی"
        };

        var rows = new List<IncmRowDto>();
        if (Nosazi != 0)
            rows.Add(new IncmRowDto { IncmNo = mainIncm, Val = Nosazi, IncmRowDsc = mainDsc });
        if (Atash != 0)
            rows.Add(new IncmRowDto { IncmNo = 100002, Val = Atash, IncmRowDsc = "آتش نشانی" });
        if (Garbage != 0)
            rows.Add(new IncmRowDto { IncmNo = 100003, Val = Garbage, IncmRowDsc = "پسماند" });
        if (Afzodeh != 0)
            rows.Add(new IncmRowDto { IncmNo = 206098003, Val = Afzodeh, IncmRowDsc = "مالیات برارزش افزوده" });

        return rows;
    }

    public async Task<bool> ExistsInRayvarzAsync(string ficheNo, CancellationToken ct = default)
    {
        const string sql = @"
SELECT TOP 1 1 FROM ray.incmdocsys
WHERE Ref = @f OR RowDocNo = @f";

        await using var conn = new SqlConnection(_rayCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@f", ficheNo);
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
            DateTime dt => dt.ToString("yyyyMMdd"),
            _ => value.ToString() ?? ""
        };
    }
}
