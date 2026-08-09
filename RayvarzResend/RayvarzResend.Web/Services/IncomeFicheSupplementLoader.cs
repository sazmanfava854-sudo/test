using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Executor;

namespace RayvarzResend.Web.Services;

/// <summary>
/// بارگذاری Oddment و فیش قبلی از Sara8M03 —
/// dbo.Income_OddmentAccount و Income_Fiche (BedeHi).
/// </summary>
public static class IncomeFicheSupplementLoader
{
  public static async Task EnrichAsync(
    FicheHeaderDto fiche,
    string saraConnectionString,
    Func<Guid, CancellationToken, Task<List<IncmRowDto>>> loadCalculationRowsAsync,
    CancellationToken ct = default)
  {
    if (fiche.Category != FicheCategory.Income || fiche.NidIncome is not { } nidIncome)
      return;

    fiche.Oddments = await LoadOddmentsAsync(saraConnectionString, nidIncome, ct);

    if (fiche.NidProc is not { } nidProc || nidProc == Guid.Empty)
      return;

    var district = ResolveDistrictBranch(fiche);
    var prior = await LoadPriorIncomeFicheAsync(
      saraConnectionString,
      nidProc,
      fiche.FicheNo,
      district,
      ct);

    if (prior is null)
      return;

    if (prior.NidIncome != Guid.Empty)
      prior.CalculationRows = await loadCalculationRowsAsync(prior.NidIncome, ct);

    fiche.PriorIncomeFiche = prior;
    fiche.PriorBedeHiAmount ??= BedeHiLogic.Resolve(district, fiche.FicheNo, prior);
  }

  public static async Task<List<IncomeOddmentDto>> LoadOddmentsAsync(
    string connectionString,
    Guid nidIncome,
    CancellationToken ct = default)
  {
    const string sql = """
      SELECT o.CI_IncomeCalculation AS IncmNo,
             o.Value,
             o.CI_OddmentType AS OddmentType
      FROM dbo.Income_OddmentAccount o
      WHERE o.NidIncome = @nid
      """;

    var list = new List<IncomeOddmentDto>();
    try
    {
      await using var conn = new SqlConnection(connectionString);
      await conn.OpenAsync(ct);
      await using var cmd = new SqlCommand(sql, conn);
      cmd.Parameters.AddWithValue("@nid", nidIncome);
      await using var reader = await cmd.ExecuteReaderAsync(ct);
      while (await reader.ReadAsync(ct))
      {
        list.Add(new IncomeOddmentDto
        {
          IncmNo = ReadInt32(reader, "IncmNo"),
          Value = ReadDecimal(reader, "Value"),
          OddmentType = ReadInt32(reader, "OddmentType")
        });
      }
    }
    catch (SqlException)
    {
      return [];
    }

    return list;
  }

  private static async Task<PriorIncomeFicheDto?> LoadPriorIncomeFicheAsync(
    string connectionString,
    Guid nidProc,
    string currentFicheNo,
    int districtBranch,
    CancellationToken ct)
  {
    var groups = BedeHiLogic.AllowedAccountGroups(districtBranch);
    if (groups.Count == 0)
      return null;

    var inList = string.Join(",", groups);
    var sql = $"""
      SELECT TOP (1)
             f.NidIncome,
             f.FicheNo,
             f.Payable,
             ISNULL(f.Brokers, 0) AS Brokers,
             f.PaymentDate,
             f.BankPaymentDate,
             f.CI_IncomeAccountGroup AS IncomeAccountGroup
      FROM dbo.Income_Fiche f
      INNER JOIN dbo.Income i ON i.NidIncome = f.NidIncome
      WHERE i.NidProc = @nidProc
        AND f.FicheNo <> @ficheNo
        AND f.EumFicheStatus IN (5, 7)
        AND f.CI_IncomeAccountGroup IN ({inList})
      ORDER BY f.BankPaymentDate DESC
      """;

    try
    {
      await using var conn = new SqlConnection(connectionString);
      await conn.OpenAsync(ct);
      await using var cmd = new SqlCommand(sql, conn);
      cmd.Parameters.AddWithValue("@nidProc", nidProc);
      cmd.Parameters.AddWithValue("@ficheNo", currentFicheNo.Trim());
      await using var reader = await cmd.ExecuteReaderAsync(ct);
      if (!await reader.ReadAsync(ct))
        return null;

      return new PriorIncomeFicheDto
      {
        NidIncome = reader.GetGuid(reader.GetOrdinal("NidIncome")),
        FicheNo = reader.GetString(reader.GetOrdinal("FicheNo")).Trim(),
        Payable = ReadDecimal(reader, "Payable"),
        Brokers = ReadDecimal(reader, "Brokers"),
        PaymentDate = ToSlashDate(reader, "PaymentDate"),
        BankPaymentDate = ToSlashDate(reader, "BankPaymentDate"),
        IncomeAccountGroup = ReadInt32(reader, "IncomeAccountGroup")
      };
    }
    catch (SqlException)
    {
      return null;
    }
  }

  private static int ResolveDistrictBranch(FicheHeaderDto fiche)
  {
    if (fiche.ResolvedDistrictBranch is > 0)
      return fiche.ResolvedDistrictBranch.Value;

    var fromNosazi = TahatorRowBuilder.ResolveDistrictBranchFromNosaziCode(fiche.BnkAcntNo);
    if (fromNosazi > 0)
      return fromNosazi;

    var bill = string.IsNullOrWhiteSpace(fiche.BillIdRaw) ? fiche.BillId : fiche.BillIdRaw;
    var payment = string.IsNullOrWhiteSpace(fiche.PaymentIdRaw) ? fiche.PaymentId : fiche.PaymentIdRaw;
    return DutyDistrictBranchResolver.ResolveBranch(bill, payment);
  }

  private static string ToSlashDate(SqlDataReader reader, string column)
  {
    var ord = reader.GetOrdinal(column);
    if (reader.IsDBNull(ord))
      return "";

    var value = reader.GetValue(ord);
    return value switch
    {
      DateTime dt when dt.Year is >= 1300 and <= 1500 =>
        $"{dt.Year:0000}/{dt.Month:00}/{dt.Day:00}",
      DateTime dt =>
        DateHelper.ToShamsiSlashDate(DateHelper.FromDatabaseDateValue(dt)),
      _ => DateHelper.ToShamsiSlashDate(value.ToString() ?? "")
    };
  }

  private static int ReadInt32(SqlDataReader reader, string column)
  {
    var ord = reader.GetOrdinal(column);
    return reader.IsDBNull(ord) ? 0 : Convert.ToInt32(reader.GetValue(ord));
  }

  private static decimal ReadDecimal(SqlDataReader reader, string column)
  {
    var ord = reader.GetOrdinal(column);
    return reader.IsDBNull(ord) ? 0 : Convert.ToDecimal(reader.GetValue(ord));
  }
}
