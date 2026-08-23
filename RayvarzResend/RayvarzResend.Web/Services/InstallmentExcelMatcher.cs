using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>اعتبارسنجی ردیف‌های اکسل با dbo.Installment_List.</summary>
public static class InstallmentExcelMatcher
{
  public static readonly string[] RequiredColumnNames =
  {
    "شناسه", "مبلغ", "تاریخ پرداخت"
  };

  public static readonly string[] OptionalColumnNames =
  {
    "عودت"
  };

  public static string NormalizeCell(string? raw) => (raw ?? "").Trim();

  public static bool LooksLikeScientificNotation(string? raw)
  {
    var t = NormalizeCell(raw);
    return System.Text.RegularExpressions.Regex.IsMatch(t, @"e[+-]?\d", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
  }

  public static string NormalizeDigits(string? raw) =>
    new string((raw ?? "").Where(char.IsDigit).ToArray());

  public static string NormalizeDate(string? raw)
  {
    var digits = NormalizeDateDigits(raw);
    if (digits.Length >= 8)
      return $"{digits[..4]}/{digits.Substring(4, 2)}/{digits.Substring(6, 2)}";
    return NormalizeCell(raw);
  }

  public static string NormalizeDateDigits(string? raw)
  {
    var text = NormalizeCell(raw);
    if (string.IsNullOrEmpty(text))
      return "";

    var parts = text.Split(['/', '-'], StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length >= 3)
    {
      var year = NormalizeDigits(parts[0]);
      var month = NormalizeDigits(parts[1]);
      var day = NormalizeDigits(parts[2]);
      if (year.Length == 4 && month.Length >= 1 && day.Length >= 1)
        return $"{year}{month.PadLeft(2, '0')}{day.PadLeft(2, '0')}";
    }

    var digits = NormalizeDigits(raw);
    return digits.Length >= 8 ? digits[..8] : digits;
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
    NormalizeDateDigits(excelDate) == NormalizeDateDigits(dbDate);

  public const string ScientificIdentifierError =
    "شناسه در اکسل خراب است (نمایش علمی مثل 5.02E+14). تغییر نوع ستون به Text عدد را برنمی‌گرداند — کد پیگیری را دوباره تایپ کنید یا از قالب اکسل استفاده کنید.";

  public static (InstallmentLookupKind Kind, string Value, string? Error) ResolveLookup(InstallmentExcelRowInput row)
  {
    if (LooksLikeScientificNotation(row.Identifier))
      return (InstallmentLookupKind.NoDocument, "", ScientificIdentifierError);

    var digits = NormalizeDigits(row.Identifier);
    if (string.IsNullOrEmpty(digits))
      return (InstallmentLookupKind.NoDocument, "", "شناسه (شماره سند / کد پیگیری) الزامی است");

    var kind = InstallmentIdentifierDetector.Detect(digits);
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

  /// <summary>NoDocument همیشه عودت — ستون Odooat نادیده. TrackingNo فقط از ستون اکسل.</summary>
  public static bool ResolveWillApplyEndState(
    InstallmentLookupKind kind,
    bool globalApplyEndStateRequested,
    InstallmentExcelRowInput row,
    bool excelMode = false)
  {
    if (kind == InstallmentLookupKind.NoDocument)
      return true;

    var rowFlag = TryParseOdooatFlag(row.Odooat);
    if (rowFlag.HasValue)
      return rowFlag.Value;

    return excelMode ? false : globalApplyEndStateRequested;
  }

  public static string DescribeOdooatPlan(InstallmentLookupKind kind, bool willApplyEndState) =>
    kind == InstallmentLookupKind.NoDocument
      ? "همیشه"
      : willApplyEndState ? "بله" : "خیر";

  public static string? ValidateAgainstDb(
    InstallmentExcelRowInput excel,
    InstallmentRowSnapshot db,
    InstallmentLookupKind lookupKind,
    string lookupValue)
  {
    if (lookupKind == InstallmentLookupKind.TrackingNo)
    {
      if (!InstallmentIdentifierDetector.TrackingNoDigitsMatch(excel.Identifier, db.TrackingNo))
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
