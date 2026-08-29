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

    public async Task<BankInquiryConfirmResult> ConfirmAsync(
        BankInquiryConfirmRequest req,
        CancellationToken ct = default)
    {
        var result = new BankInquiryConfirmResult { DryRun = IsDryRun };

        var validationError = BankInquiryConfirmHelper.ValidateRequest(req);
        if (validationError != null)
        {
            result.Error = validationError;
            return result;
        }

        var where = BankInquiryConfirmHelper.BuildWhereClause(
            req.FicheNo, req.BillId, req.PaymentId, req.IdentifierValue);
        if (where == null)
        {
            result.Error = "شناسه فیش نامعتبر است";
            return result;
        }

        var paymentDate = BankInquiryConfirmHelper.NormalizeSlashDate(req.PaymentDate);
        var userConfirmDate = DateHelper.CurrentShamsiSlashDate();
        var usernameUserConfirm = (req.PerformedByUser ?? "").Trim();
        if (string.IsNullOrEmpty(usernameUserConfirm))
            usernameUserConfirm = "کاربر";

        var lookupSql = $"""
            SELECT TOP (1) FicheNo, BillID, PaymentID, EumFicheStatus, PaymentDate
            FROM dbo.Income_Fiche
            WHERE {where.Value.WhereClause}
            """;

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);

        string? ficheNo = null;
        string? billId = null;
        string? paymentId = null;
        int? previousStatus = null;
        string? previousPaymentDate = null;

        await using (var lookupCmd = new SqlCommand(lookupSql, conn))
        {
            foreach (var (name, value) in where.Value.Parameters)
                lookupCmd.Parameters.AddWithValue(name, value);

            await using var reader = await lookupCmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
            {
                result.Error = "فیش یافت نشد";
                result.NotFound = 1;
                return result;
            }

            ficheNo = reader["FicheNo"]?.ToString() ?? "";
            billId = reader["BillID"]?.ToString() ?? "";
            paymentId = reader["PaymentID"]?.ToString() ?? "";
            previousStatus = ReadNullableInt(reader, "EumFicheStatus");
            previousPaymentDate = reader["PaymentDate"]?.ToString();
        }

        result.FicheNo = ficheNo ?? "";
        result.BillId = billId ?? "";
        result.PaymentId = paymentId ?? "";
        result.PreviousEumFicheStatus = previousStatus;
        result.PreviousPaymentDate = previousPaymentDate;
        result.PaymentDate = paymentDate;
        result.UserConfirmDate = userConfirmDate;
        result.UsernameUserConfirm = usernameUserConfirm;
        result.NewEumFicheStatus = BankInquiryConfirmHelper.ConfirmedFicheStatus;
        result.NewEumIncomePaymentType = BankInquiryConfirmHelper.ConfirmedIncomePaymentType;

        if (result.DryRun)
        {
            result.Success = true;
            result.WouldUpdate = 1;
            result.Message = "DryRun — UPDATE نمی‌شود (BankInquiryConfirm:DryRun=true)";
            return result;
        }

        var updateSql = $"""
            UPDATE dbo.Income_Fiche
            SET EumFicheStatus = @status,
                EumIncomePaymentType = @paymentType,
                PaymentDate = @paymentDate,
                UserConfirmDate = @userConfirmDate,
                UsernameUserConfirm = @usernameUserConfirm
            WHERE {where.Value.WhereClause}
            """;

        try
        {
            await using var updateCmd = new SqlCommand(updateSql, conn);
            updateCmd.Parameters.AddWithValue("@status", BankInquiryConfirmHelper.ConfirmedFicheStatus);
            updateCmd.Parameters.AddWithValue("@paymentType", BankInquiryConfirmHelper.ConfirmedIncomePaymentType);
            updateCmd.Parameters.AddWithValue("@paymentDate", paymentDate);
            updateCmd.Parameters.AddWithValue("@userConfirmDate", userConfirmDate);
            updateCmd.Parameters.AddWithValue("@usernameUserConfirm", usernameUserConfirm);
            foreach (var (name, value) in where.Value.Parameters)
                updateCmd.Parameters.AddWithValue(name, value);

            var affected = await updateCmd.ExecuteNonQueryAsync(ct);
            result.RowsAffected = affected;
            result.Success = affected > 0;
            result.Updated = affected > 0 ? 1 : 0;
            result.Message = affected > 0 ? "تایید استعلام بانک با موفقیت ثبت شد" : "فیش یافت نشد";
            if (affected == 0)
                result.NotFound = 1;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Failed = 1;
            result.Message = ex.Message;
        }

        return result;
    }

    private static int? ReadNullableInt(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        if (reader.IsDBNull(ordinal)) return null;
        return Convert.ToInt32(reader.GetValue(ordinal));
    }
}
