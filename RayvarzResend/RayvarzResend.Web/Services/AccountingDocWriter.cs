using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>
/// پس از ارسال موفق SOAP و تأیید incmdocsys، ردیف واسط Sara (Accounting_DocHeader/Details) را می‌نویسد.
/// </summary>
public sealed class AccountingDocWriter
{
    private readonly string _saraCs;
    private readonly FicheRepository _repo;
    private readonly IConfiguration _config;
    private readonly ILogger<AccountingDocWriter> _logger;

    public AccountingDocWriter(
        IConfiguration config,
        FicheRepository repo,
        ILogger<AccountingDocWriter> logger)
    {
        _config = config;
        _repo = repo;
        _logger = logger;
        _saraCs = config.GetConnectionString("Sara")
            ?? throw new InvalidOperationException("ConnectionStrings:Sara not set");
    }

    public bool IsDryRun =>
        _config.GetValue<bool?>("AccountingDoc:DryRun")
        ?? _config.GetValue("Rayvarz:DryRun", true);

    public async Task<AccountingDocWriteResult> TryWriteAfterSendAsync(
        FicheHeaderDto fiche,
        string? pursuitDocNo,
        CancellationToken ct = default)
    {
        var ficheNo = fiche.FicheNo.Trim();
        if (string.IsNullOrWhiteSpace(ficheNo))
            return AccountingDocWriteResult.Skipped("شماره فیش خالی است");

        if (await _repo.ExistsInAccountingDocHeaderAsync(ficheNo, ct))
            return AccountingDocWriteResult.Skipped("Accounting_DocHeader از قبل موجود است");

        if (fiche.Payable <= 0)
            return AccountingDocWriteResult.Skipped("مبلغ قابل پرداخت صفر است");

        RayvarzDocMeta? rayMeta = null;
        try
        {
            rayMeta = await _repo.GetRayvarzDocMetaAsync(ficheNo, ct);
        }
        catch (SqlException ex)
        {
            _logger.LogWarning(ex, "incmdocsys meta read failed for {FicheNo}", ficheNo);
            return AccountingDocWriteResult.Failed($"خواندن incmdocsys: {ex.Message}");
        }

        if (rayMeta == null)
            return AccountingDocWriteResult.Failed("فیش در incmdocsys یافت نشد — واسط ثبت نشد");

        var (header, details) = AccountingDocRowBuilder.Build(fiche, rayMeta, pursuitDocNo);
        if (details.Count == 0)
            return AccountingDocWriteResult.Failed("ردیف Accounting_DocDetails خالی است");

        if (IsDryRun)
        {
            return new AccountingDocWriteResult
            {
                Written = false,
                DryRun = true,
                WasSkipped = false,
                Message = $"DryRun — {details.Count} ردیف Details و Header INSERT نمی‌شود",
                HeaderGid = header.GidDocHeader,
                DetailCount = details.Count,
                AccountingNo = header.AccountingNo
            };
        }

        try
        {
            await InsertAsync(header, details, ct);
            _logger.LogInformation(
                "Accounting_Doc written for {FicheNo}: header={HeaderGid}, details={DetailCount}, accountingNo={AccountingNo}",
                ficheNo, header.GidDocHeader, details.Count, header.AccountingNo);

            return new AccountingDocWriteResult
            {
                Written = true,
                Message = $"واسط Sara ثبت شد — {details.Count} ردیف",
                HeaderGid = header.GidDocHeader,
                DetailCount = details.Count,
                AccountingNo = header.AccountingNo
            };
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Accounting_Doc INSERT failed for {FicheNo}", ficheNo);
            return AccountingDocWriteResult.Failed($"INSERT واسط: {ex.Message}");
        }
    }

    private async Task InsertAsync(
        AccountingDocRowBuilder.AccountingDocHeaderDraft header,
        IReadOnlyList<AccountingDocRowBuilder.AccountingDocDetailDraft> details,
        CancellationToken ct)
    {
        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        await using var tx = (SqlTransaction)await conn.BeginTransactionAsync(ct);

        try
        {
            const string headerSql = """
                INSERT INTO dbo.Accounting_DocHeader
                    (GidDocHeader, AccountingNo, FromDate, ToDate, SaraPrice, Comments,
                     DocDate, DocTime, NidUser, EumObjOnPrice, EumAccountingObjInDocument,
                     EumAccountingDocumentingCause, DocRow, PhasType, SubSystem, FicheNo, NidFiche)
                VALUES
                    (@gid, @accountingNo, NULL, NULL, @saraPrice, NULL,
                     @docDate, @docTime, NULL, @objOnPrice, @objInDoc,
                     @docCause, @docRow, @phasType, @subSystem, @ficheNo, @nidFiche)
                """;

            await using (var cmd = new SqlCommand(headerSql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@gid", header.GidDocHeader);
                cmd.Parameters.AddWithValue("@accountingNo", header.AccountingNo);
                cmd.Parameters.AddWithValue("@saraPrice", header.SaraPrice);
                cmd.Parameters.AddWithValue("@docDate", header.DocDate);
                cmd.Parameters.AddWithValue("@docTime", header.DocTime);
                cmd.Parameters.AddWithValue("@objOnPrice", header.EumObjOnPrice);
                cmd.Parameters.AddWithValue("@objInDoc", header.EumAccountingObjInDocument);
                cmd.Parameters.AddWithValue("@docCause", header.EumAccountingDocumentingCause);
                cmd.Parameters.AddWithValue("@docRow", header.DocRow);
                cmd.Parameters.AddWithValue("@phasType", header.PhasType);
                cmd.Parameters.AddWithValue("@subSystem", header.SubSystem);
                cmd.Parameters.AddWithValue("@ficheNo", header.FicheNo);
                cmd.Parameters.AddWithValue("@nidFiche", header.NidFiche);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            const string detailSql = """
                INSERT INTO dbo.Accounting_DocDetails
                    (GidDocDetails, GidDocHeader, Nid, Price, FicheNo, BillID, PaymentID,
                     PaymentDate, BankCode, AccountNo, AccountNoComments, WrapperAccountNo, IncmRow)
                VALUES
                    (@gid, @headerGid, NULL, @price, @ficheNo, @billId, @paymentId,
                     @paymentDate, @bankCode, @accountNo, @comments, @wrapper, @incmRow)
                """;

            foreach (var d in details)
            {
                await using var cmd = new SqlCommand(detailSql, conn, tx);
                cmd.Parameters.AddWithValue("@gid", d.GidDocDetails);
                cmd.Parameters.AddWithValue("@headerGid", header.GidDocHeader);
                cmd.Parameters.AddWithValue("@price", d.Price);
                cmd.Parameters.AddWithValue("@ficheNo", d.FicheNo);
                cmd.Parameters.AddWithValue("@billId", (object?)d.BillId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@paymentId", (object?)d.PaymentId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@paymentDate", d.PaymentDate);
                cmd.Parameters.AddWithValue("@bankCode", d.BankCode);
                cmd.Parameters.AddWithValue("@accountNo", (object?)d.AccountNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@comments", (object?)d.AccountNoComments ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@wrapper", d.WrapperAccountNo);
                cmd.Parameters.AddWithValue("@incmRow", d.IncmRow);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}

public sealed class AccountingDocWriteResult
{
    public bool Written { get; init; }
    public bool WasSkipped { get; init; }
    public bool DryRun { get; init; }
    public bool IsFailure => !Written && !WasSkipped && !DryRun;
    public string Message { get; init; } = "";
    public Guid? HeaderGid { get; init; }
    public int DetailCount { get; init; }
    public string? AccountingNo { get; init; }

    public static AccountingDocWriteResult Skipped(string message) =>
        new() { WasSkipped = true, Message = message };

    public static AccountingDocWriteResult Failed(string message) =>
        new() { Message = message };
}
