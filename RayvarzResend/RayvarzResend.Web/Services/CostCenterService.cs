using System.Security.Claims;
using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public sealed class CostCenterService
{
    private readonly IConfiguration _config;
    private readonly FicheRepository _repo;
    private readonly string _saraCs;

    public CostCenterService(IConfiguration config, FicheRepository repo)
    {
        _config = config;
        _repo = repo;
        _saraCs = config.GetConnectionString("Sara")
            ?? throw new InvalidOperationException("ConnectionStrings:Sara not set");
    }

    public bool IsDryRun =>
        _config.GetValue<bool?>("CostCenter:DryRun")
        ?? _config.GetValue("Rayvarz:DryRun", true);

    public async Task<CostCenterSearchResult> SearchAsync(
        CostCenterSearchRequest req,
        ClaimsPrincipal user,
        CancellationToken ct = default)
    {
        var result = new CostCenterSearchResult();
        var validationError = CostCenterHelper.ValidateSearchRequest(req);
        if (validationError != null)
        {
            result.Error = validationError;
            return result;
        }

        var (whereSql, parameters) = CostCenterHelper.BuildSearchWhere(req);
        var whereClauses = new List<string> { whereSql };
        DistrictAccessService.AppendIncomeFicheDistrictFilter(user, whereClauses, parameters);
        whereSql = string.Join(" AND ", whereClauses);

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
                   f.Deposit,
                   f.CreditorPapers,
                   f.BillID,
                   f.PaymentID,
                   {BankInquiryConfirmHelper.IncomeNosaziCodeSql} AS NosaziCode
            FROM dbo.Income_Fiche f
            {BankInquiryConfirmHelper.IncomeFicheNosaziJoins}
            WHERE {whereSql}
            ORDER BY f.FicheNo DESC
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
            result.Items.Add(new CostCenterListItem
            {
                FicheNo = reader["FicheNo"]?.ToString() ?? "",
                Deposit = ReadNullableInt64(reader, "Deposit"),
                CreditorPapers = ReadNullableInt64(reader, "CreditorPapers"),
                BillId = reader["BillID"]?.ToString() ?? "",
                PaymentId = reader["PaymentID"]?.ToString() ?? "",
                NosaziCode = reader["NosaziCode"]?.ToString() ?? ""
            });
        }

        result.Count = result.Items.Count;
        result.TotalCount = totalCount;
        result.Page = page;
        result.PageSize = pageSize;
        result.TotalPages = totalPages;
        return result;
    }

    public async Task<CostCenterUpdateResult> UpdateAsync(
        CostCenterUpdateRequest req,
        ClaimsPrincipal user,
        CancellationToken ct = default)
    {
        var result = new CostCenterUpdateResult { DryRun = IsDryRun };
        var validationError = CostCenterHelper.ValidateUpdateRequest(req);
        if (validationError != null)
        {
            result.Error = validationError;
            return result;
        }

        CostCenterHelper.TryParseCenterValue(req.CenterOne, out var deposit);
        CostCenterHelper.TryParseCenterValue(req.CenterTwo, out var creditorPapers);

        var ficheNos = (req.FicheNos ?? [])
            .Select(s => (s ?? "").Trim())
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        result.Deposit = deposit;
        result.CreditorPapers = creditorPapers;

        if (result.DryRun)
        {
            foreach (var ficheNo in ficheNos)
            {
                var item = await BuildUpdatePreviewAsync(ficheNo, user, ct);
                if (item.Success)
                    item.Message = "DryRun — UPDATE انجام نمی‌شود (CostCenter:DryRun=true)";
                AppendUpdateItemResult(result, item);
            }

            FinalizeUpdateResult(result);
            return result;
        }

        const string sql = """
            UPDATE dbo.Income_Fiche
            SET Deposit = @deposit,
                CreditorPapers = @creditorPapers
            WHERE FicheNo = @ficheNo
            """;

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);

        foreach (var ficheNo in ficheNos)
        {
            var item = await BuildUpdatePreviewAsync(ficheNo, user, ct);
            if (!item.Success)
            {
                AppendUpdateItemResult(result, item);
                continue;
            }

            try
            {
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@deposit", deposit);
                cmd.Parameters.AddWithValue("@creditorPapers", creditorPapers);
                cmd.Parameters.AddWithValue("@ficheNo", ficheNo);
                var affected = await cmd.ExecuteNonQueryAsync(ct);
                item.RowsAffected = affected;
                item.Found = affected > 0;
                item.Success = affected > 0;
                item.Message = affected > 0
                    ? "مراکز هزینه به‌روز شد"
                    : "یافت نشد";
            }
            catch (Exception ex)
            {
                item.Success = false;
                item.Message = ex.Message;
            }

            AppendUpdateItemResult(result, item);
        }

        FinalizeUpdateResult(result);
        return result;
    }

    private async Task<CostCenterUpdateItemResult> BuildUpdatePreviewAsync(
        string ficheNo,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var item = new CostCenterUpdateItemResult { FicheNo = ficheNo };
        var fiche = await _repo.LoadAsync(IdentifierType.FicheNo, ficheNo, ct);
        if (fiche == null)
        {
            item.Found = false;
            item.Success = false;
            item.Message = "یافت نشد";
            return item;
        }

        var districtDenied = DistrictAccessService.GetAccessDeniedMessage(user, fiche);
        if (districtDenied != null)
        {
            item.Found = true;
            item.Success = false;
            item.Message = districtDenied;
            return item;
        }

        item.Found = true;
        item.Success = true;
        item.WouldUpdate = 1;
        return item;
    }

    private static void AppendUpdateItemResult(CostCenterUpdateResult result, CostCenterUpdateItemResult item)
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

    private static void FinalizeUpdateResult(CostCenterUpdateResult result)
    {
        if (result.DryRun)
        {
            result.Success = result.WouldUpdate > 0;
            result.Message = result.WouldUpdate > 0
                ? $"شبیه‌سازی — {result.WouldUpdate} فیش UPDATE می‌شد"
                : "شبیه‌سازی — هیچ فیشی به‌روز نمی‌شد";
            return;
        }

        result.Success = result.Updated > 0;
        if (result.Updated > 0)
        {
            result.Message = $"{result.Updated} فیش به‌روز شد";
            return;
        }

        if (result.Failed > 0)
        {
            var firstFailure = result.Results.FirstOrDefault(r => !r.Success && r.Found);
            result.Message = firstFailure?.Message
                ?? $"به‌روزرسانی انجام نشد — خطا: {result.Failed}";
            return;
        }

        result.Message = result.NotFound > 0
            ? "فیش انتخاب‌شده یافت نشد"
            : "هیچ فیشی به‌روز نشد";
    }

    private static long? ReadNullableInt64(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        if (reader.IsDBNull(ordinal)) return null;
        return Convert.ToInt64(reader.GetValue(ordinal));
    }
}
