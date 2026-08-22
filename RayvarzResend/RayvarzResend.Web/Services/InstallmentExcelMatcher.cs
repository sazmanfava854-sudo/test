using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>اعتبارسنجی ردیف‌های اکسل با dbo.Installment_List.</summary>
public static class InstallmentExcelMatcher
{
  public static readonly string[] ExpectedColumnNames =
  {
    "Identifier", "PaymentCost", "PaymentDate"
  };

  public static string NormalizeCell(string? raw) => (raw ?? "").Trim();

  public static string NormalizeDigits(string? raw) =>
    new string((raw ?? "").Where(char.IsDigit).ToArray());

  public static string NormalizeDate(string? raw)
  {
    var digits = NormalizeDigits(raw);
    if (digits.Length >= 8)
      return $"{digits[..4]}/{digits.Substring(4, 2)}/{digits.Substring(6, 2)}";
    return NormalizeCell(raw);
  }

  public static bool TryParseCost(string? raw, out decimal cost)
  {
    cost = 0;
    var text = NormalizeCell(raw);
    if (string.IsNullOrEmpty(text))
      return false;

    text = text.Replace(",", "");
    return decimal.TryParse(text, System.Globalization.NumberStyles.Any,
      System.Globalization.CultureInfo.InvariantCulture, out cost);
  }

  public static bool CostsMatch(string? excelCost, decimal? dbCost)
  {
    if (!TryParseCost(excelCost, out var expected))
      return false;
    if (dbCost == null)
      return false;
    return expected == dbCost.Value;
  }

  public static bool DatesMatch(string? excelDate, string? dbDate) =>
    NormalizeDate(excelDate) == NormalizeDate(dbDate);

  public static (InstallmentLookupKind Kind, string Value, string? Error) ResolveLookup(InstallmentExcelRowInput row)
  {
    var digits = NormalizeDigits(row.Identifier);
    if (string.IsNullOrEmpty(digits))
      return (InstallmentLookupKind.NoDocument, "", "شناسه (شماره سند / کد پیگیری) الزامی است");

    var kind = InstallmentIdentifierDetector.Detect(row.Identifier);
    return (kind, digits, null);
  }

  public static string? ValidateAgainstDb(
    InstallmentExcelRowInput excel,
    InstallmentRowSnapshot db,
    InstallmentLookupKind lookupKind,
    string lookupValue)
  {
    if (!CostsMatch(excel.PaymentCost, db.PaymentCost))
      return "مبلغ (PaymentCost) با دیتابیس مطابقت ندارد";

    if (!DatesMatch(excel.PaymentDate, db.PaymentDate))
      return "تاریخ (PaymentDate) با دیتابیس مطابقت ندارد";

    if (lookupKind == InstallmentLookupKind.NoDocument)
    {
      var dbNoDoc = NormalizeDigits(db.NoDocument);
      if (!string.Equals(lookupValue, dbNoDoc, StringComparison.OrdinalIgnoreCase))
        return "شماره سند (NoDocument) با دیتابیس مطابقت ندارد";
    }
    else
    {
      var dbTracking = NormalizeDigits(db.TrackingNo);
      if (!string.Equals(lookupValue, dbTracking, StringComparison.OrdinalIgnoreCase))
        return "کد پیگیری (TrackingNo) با دیتابیس مطابقت ندارد";
    }

    return null;
  }
}

public sealed class InstallmentRowSnapshot
{
  public string NoDocument { get; set; } = "";
  public string TrackingNo { get; set; } = "";
  public decimal? PaymentCost { get; set; }
  public string PaymentDate { get; set; } = "";
  public string CI_InstallmentStatus { get; set; } = "";
  public string EndStateDesc { get; set; } = "";
  public string EndStateCode { get; set; } = "";
  public string Comments { get; set; } = "";
}
