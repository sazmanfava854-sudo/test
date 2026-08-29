using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public sealed class BankInquiryConfirmService
{
    private readonly IConfiguration _config;
    private readonly string _saraCs;

    public BankInquiryConfirmService(IConfiguration config)
    {
        _config = config;
        _saraCs = config.GetConnectionString("Sara")
            ?? throw new InvalidOperationException("ConnectionStrings:Sara not set");
    }

    public bool IsDryRun =>
        _config.GetValue<bool?>("BankInquiryConfirm:DryRun")
        ?? _config.GetValue("Rayvarz:DryRun", true);

    public async Task<BankInquirySearchResult> SearchAsync(
        BankInquirySearchRequest req,
        CancellationToken ct = default)
    {
        var result = new BankInquirySearchResult();
        var validationError = BankInquiryConfirmHelper.ValidateSearchRequest(req);
        if (validationError != null)
        {
            result.Error = validationError;
            return result;
        }

        var (whereSql, parameters) = BankInquiryConfirmHelper.BuildSearchWhere(req);
        if (string.IsNullOrWhiteSpace(whereSql))
        {
            result.Error = "فیلتر جستجو نامعتبر است";
            return result;
        }

        var page = req.Page > 0 ? req.Page : 1;
        var pageSize = req.PageSize is > 0 and <= 200 ? req.PageSize : 25;
        var offset = (page - 1) * pageSize;

        var countSql = $"""
            SELECT COUNT(*)
            FROM dbo.Income_Fiche f
            {BankInquiryConfirmHelper.IncomeFicheNosaziJoins}
            WHERE {whereSql}
            """;

        var sql = $"""
            SELECT f.FicheNo,
                   CAST(r.NidWorkItem AS nvarchar(50)) AS NidWorkItem,
                   f.BillID,
                   f.PaymentID,
                   f.PaymentDate,
                   f.BankPaymentDate,
                   f.EumFicheStatus,
                   f.UserConfirmDate,
                   f.UsernameUserConfirm,
                   {BankInquiryConfirmHelper.IncomeNosaziCodeSql} AS NosaziCode
            FROM dbo.Income_Fiche f
            {BankInquiryConfirmHelper.IncomeFicheNosaziJoins}
            WHERE {whereSql}
            ORDER BY f.PaymentDate DESC, f.FicheNo DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY
            """;

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);

        var totalCount = 0;
        await using (var countCmd = new SqlCommand(countSql, conn))
        {
            foreach (var (name, value) in parameters)
                countCmd.Parameters.AddWithValue(name, value);
            var scalar = await countCmd.ExecuteScalarAsync(ct);
            totalCount = scalar == null || scalar == DBNull.Value ? 0 : Convert.ToInt32(scalar);
        }

        var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
            offset = (page - 1) * pageSize;
        }

        await using var cmd = new SqlCommand(sql, conn);
        foreach (var (name, value) in parameters)
            cmd.Parameters.AddWithValue(name, value);
        cmd.Parameters.AddWithValue("@offset", offset);
        cmd.Parameters.AddWithValue("@pageSize", pageSize);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var status = ReadInt(reader, "EumFicheStatus");
            result.Items.Add(new BankInquiryListItem
            {
                FicheNo = reader["FicheNo"]?.ToString() ?? "",
                NidWorkItem = reader["NidWorkItem"]?.ToString() ?? "",
                BillId = reader["BillID"]?.ToString() ?? "",
                PaymentId = reader["PaymentID"]?.ToString() ?? "",
                PaymentDate = reader["PaymentDate"]?.ToString() ?? "",
                BankPaymentDate = reader["BankPaymentDate"]?.ToString() ?? "",
                NosaziCode = reader["NosaziCode"]?.ToString() ?? "",
                EumFicheStatus = status,
                EumFicheStatusLabel = FicheDateChangeHelper.StatusLabel(status),
                UserConfirmDate = reader["UserConfirmDate"]?.ToString() ?? "",
                UsernameUserConfirm = reader["UsernameUserConfirm"]?.ToString() ?? ""
            });
        }

        result.Count = result.Items.Count;
        result.TotalCount = totalCount;
        result.Page = page;
        result.PageSize = pageSize;
        result.TotalPages = totalPages;
        return result;
    }

    public async Task<BankInquiryConfirmResult> ConfirmAsync(
        BankInquiryConfirmRequest req,
        CancellationToken ct = default)
    {
        var result = new BankInquiryConfirmResult { DryRun = IsDryRun };

        var validationError = BankInquiryConfirmHelper.ValidateConfirmRequest(req);
        if (validationError != null)
        {
            result.Error = validationError;
            return result;
        }

        var ficheNos = (req.FicheNos ?? [])
            .Select(s => (s ?? "").Trim())
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var paymentDate = BankInquiryConfirmHelper.NormalizeSlashDate(req.NewPaymentDate);
        var userConfirmDate = DateHelper.CurrentShamsiSlashDate();
        var usernameUserConfirm = (req.PerformedByUser ?? "").Trim();
        if (string.IsNullOrEmpty(usernameUserConfirm))
            usernameUserConfirm = "کاربر";

        result.PaymentDate = paymentDate;
        result.UserConfirmDate = userConfirmDate;
        result.UsernameUserConfirm = usernameUserConfirm;
        result.NewEumFicheStatus = BankInquiryConfirmHelper.ConfirmedFicheStatus;
        result.NewEumIncomePaymentType = BankInquiryConfirmHelper.ConfirmedIncomePaymentType;

        if (result.DryRun)
        {
            result.Total = ficheNos.Count;
            result.WouldUpdate = ficheNos.Count;
            foreach (var ficheNo in ficheNos)
            {
                result.Results.Add(new BankInquiryConfirmItemResult
                {
                    FicheNo = ficheNo,
                    Success = true,
                    Found = true,
                    WouldUpdate = 1,
                    Message = "DryRun — UPDATE نمی‌شود (BankInquiryConfirm:DryRun=true)"
                });
            }

            return result;
        }

        var sql = """
            UPDATE dbo.Income_Fiche
            SET EumFicheStatus = @status,
                EumIncomePaymentType = @paymentType,
                PaymentDate = @paymentDate,
                UserConfirmDate = @userConfirmDate,
                UsernameUserConfirm = @usernameUserConfirm
            WHERE FicheNo = @ficheNo
            """;

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);

        foreach (var ficheNo in ficheNos)
        {
            var item = new BankInquiryConfirmItemResult { FicheNo = ficheNo };
            try
            {
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@status", BankInquiryConfirmHelper.ConfirmedFicheStatus);
                cmd.Parameters.AddWithValue("@paymentType", BankInquiryConfirmHelper.ConfirmedIncomePaymentType);
                cmd.Parameters.AddWithValue("@paymentDate", paymentDate);
                cmd.Parameters.AddWithValue("@userConfirmDate", userConfirmDate);
                cmd.Parameters.AddWithValue("@usernameUserConfirm", usernameUserConfirm);
                cmd.Parameters.AddWithValue("@ficheNo", ficheNo);

                var affected = await cmd.ExecuteNonQueryAsync(ct);
                item.RowsAffected = affected;
                item.Found = affected > 0;
                item.Success = affected > 0;
                item.Message = affected > 0 ? "تایید استعلام بانک ثبت شد" : "یافت نشد";
            }
            catch (Exception ex)
            {
                item.Success = false;
                item.Message = ex.Message;
            }

            AppendConfirmItemResult(result, item);
        }

        return result;
    }

    private static void AppendConfirmItemResult(BankInquiryConfirmResult result, BankInquiryConfirmItemResult item)
    {
        result.Results.Add(item);
        result.Total++;
        if (result.DryRun)
        {
            if (item.WouldUpdate > 0) result.WouldUpdate += item.WouldUpdate;
            if (!item.Found) result.NotFound++;
            else if (!item.Success) result.Failed++;
            return;
        }

        if (item.Success) result.Updated += item.RowsAffected;
        else if (!item.Found) result.NotFound++;
        else result.Failed++;
    }

    private static int ReadInt(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        if (reader.IsDBNull(ordinal)) return 0;
        return Convert.ToInt32(reader.GetValue(ordinal));
    }
}
