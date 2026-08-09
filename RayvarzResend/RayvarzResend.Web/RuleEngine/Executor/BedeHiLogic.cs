using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>بدهی قبلی — VB BedeHi (member-1388-full-body.vb خطوط ۴۸۱۲–۴۸۷۰).</summary>
public static class BedeHiLogic
{
  private static readonly int[] RegionalAccountGroups = [1, 7, 10];
  private static readonly int[] StandardAccountGroups =
  [
      64, 78, 1, 8, 15, 22, 29, 36, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100,
      43, 50, 71, 101, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116
  ];

  private const string MinPaymentDate = "1399/01/01";

  public static IReadOnlyList<int> AllowedAccountGroups(int districtBranch) =>
    districtBranch is 80 or 218 ? RegionalAccountGroups : StandardAccountGroups;

  public static decimal Resolve(int districtBranch, string currentFicheNo, PriorIncomeFicheDto? prior)
  {
    if (prior is null || string.IsNullOrWhiteSpace(prior.FicheNo))
      return 0;

    if (prior.FicheNo.Equals(currentFicheNo.Trim(), StringComparison.OrdinalIgnoreCase))
      return 0;

    var allowedGroups = AllowedAccountGroups(districtBranch);

    if (!allowedGroups.Contains(prior.IncomeAccountGroup))
      return 0;

    var paymentDate = NormalizeDate(prior.PaymentDate);
    var bankPaymentDate = NormalizeDate(prior.BankPaymentDate);

    if (string.CompareOrdinal(paymentDate, MinPaymentDate) < 0
        || string.CompareOrdinal(bankPaymentDate, MinPaymentDate) < 0)
      return 0;

    var amount = prior.Payable - prior.Brokers;
    return amount > 0 ? amount : 0;
  }

  private static string NormalizeDate(string? value)
  {
    if (string.IsNullOrWhiteSpace(value))
      return "1394/12/29";
    return value.Trim();
  }
}
