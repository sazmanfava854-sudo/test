using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>اعتبارسنجی ردیف‌های اکسل با dbo.Installment_List.</summary>
public static class InstallmentExcelMatcher
{
  public static readonly string[] RequiredColumnNames =
  {
    "Identifier", "PaymentCost", "PaymentDate"
  };

  public static readonly string[] OptionalColumnNames =
  {
    "Odooat", "عودت"
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

  public static bool? TryParseOdooatFlag(string? raw)
  {
    var text = NormalizeCell(raw);
    if (string.IsNullOrEmpty(text))
      return null;

    var lower = text.ToLowerInvariant().Replace('ي', 'ی').Replace('ك', 'ک');
    if (lower is "1" or "yes" or "y" or "true" or "بله" or "بلی" or "عودت" or "ع")
      return true;
    if (lower is "0" or "no" or "n" or "false" or "خیر" or "نه")
      return false;

    return null;
  }

  /// <summary>NoDocument همیشه عودت — TrackingNo از ستون اکسل یا تیک فرم.</summary>
  public static bool ResolveWillApplyEndState(
    InstallmentLookupKind kind,
    bool globalApplyEndStateRequested,
    InstallmentExcelRowInput row)
  {
    if (kind == InstallmentLookupKind.NoDocument)
      return true;

    var rowFlag = TryParseOdooatFlag(row.Odooat);
    if (rowFlag.HasValue)
      return rowFlag.Value;

    return globalApplyEndStateRequested;
  }

  public static string DescribeOdooatPlan(InstallmentLookupKind kind, bool willApplyEndState) =>
    kind == InstallmentLookupKind.NoDocument
      ? "اجباری"
      : willApplyEndState ? "بله" : "خیر";

  public static string? ValidateAgainstDb(
    InstallmentExcelRowInput excel,
    InstallmentRowSnapshot db,
    InstallmentLookupKind lookupKind,
    string lookupValue)
  {
    if (lookupKind == InstallmentLookupKind.TrackingNo)
    {
      var dbTracking = NormalizeDigits(db.TrackingNo);
      if (!string.Equals(lookupValue, dbTracking, StringComparison.OrdinalIgnoreCase))
        return "کد پیگیری (TrackingNo) با دیتابیس مطابقت ندارد";
      return null;
    }

    if (!CostsMatch(excel.PaymentCost, db.PaymentCost))
      return "مبلغ (PaymentCost) با دیتابیس مطابقت ندارد";

    if (!DatesMatch(excel.PaymentDate, db.PaymentDate))
      return "تاریخ (PaymentDate) با دیتابیس مطابقت ندارد";

    var dbNoDoc = NormalizeDigits(db.NoDocument);
    if (!string.Equals(lookupValue, dbNoDoc, StringComparison.OrdinalIgnoreCase))
      return "شماره سند (NoDocument) با دیتابیس مطابقت ندارد";

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
  public string NidWorkItem { get; set; } = "";
  public string NosaziCode { get; set; } = "";
}
