using System.Globalization;
using System.Xml.Linq;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Web.Validation;

/// <summary>استخراج فیلدهای SOAP SaveDocument برای اعتبارسنجی پس از ساخت.</summary>
public static class RayvarzSoapXmlInspector
{
    private static readonly XNamespace B = "http://schemas.datacontract.org/2004/07/WCFServer";

    public static ParsedSoapDocument? TryParse(string? soapXml)
    {
        if (string.IsNullOrWhiteSpace(soapXml))
            return null;

        try
        {
            var doc = XDocument.Parse(soapXml);
            var save = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "SaveDocument");
            if (save is null)
                return null;

            var headerDoc = save.Descendants().FirstOrDefault(e => e.Name.LocalName == "doc");
            var item = headerDoc?.Descendants().FirstOrDefault(e => e.Name.LocalName == "DocumentItem");
            if (item is null)
                return null;

            var incms = item.Elements().Where(e => e.Name.LocalName == "Incms")
                .SelectMany(x => x.Elements().Where(e => e.Name.LocalName == "DocumentItemIncm"))
                .Select(ParseIncm)
                .ToList();

            return new ParsedSoapDocument
            {
                TransactionId = ElementText(headerDoc, "TransactionId"),
                DocDate = ElementText(headerDoc, "DocDate"),
                DocDsc = ElementText(headerDoc, "DocDsc"),
                DocTyp = ElementText(headerDoc, "DocTyp"),
                DocTypDsc = ElementText(headerDoc, "DocTypDsc"),
                DocRow = ElementText(item, "DocRow"),
                VchrTyp = ElementText(item, "VchrTyp"),
                ActTyp = ElementText(item, "ActTyp"),
                ActDate = ElementText(item, "ActDate"),
                PhasTyp = ElementText(item, "PhasTyp"),
                RowDate = ElementText(item, "RowDate"),
                RowDocNo = ElementText(item, "RowDocNo"),
                Fund = ElementText(item, "Fund"),
                BnkAcntNo = ElementText(item, "BnkAcntNo"),
                Center = ElementText(item, "Center"),
                IncmRows = incms
            };
        }
        catch
        {
            return null;
        }
    }

    private static ParsedIncmRow ParseIncm(XElement el) => new()
    {
        IncmRow = ElementText(el, "IncmRow"),
        IncmNo = ElementText(el, "IncmNo"),
        Val = ElementText(el, "Val"),
        Qty = ElementText(el, "Qty"),
        Due = ElementText(el, "Due"),
        RefRowDocNo = ElementText(el, "RefRowDocNo"),
        RefRowDate = ElementText(el, "RefRowDate"),
        Reason = ElementText(el, "Reason"),
        ReasonDsc = ElementText(el, "ReasonDsc"),
        IncmRowDsc = ElementText(el, "IncmRowDsc"),
        Center1 = ElementText(el, "Center1"),
        Center2 = ElementText(el, "Center2"),
        Center3 = ElementText(el, "Center3"),
        Ref = ElementText(el, "Ref")
    };

    private static string? ElementText(XElement? parent, string localName)
    {
        if (parent is null)
            return null;
        var el = parent.Elements().FirstOrDefault(e => e.Name.LocalName == localName);
        if (el is null)
            return null;
        var nil = el.Attribute(XName.Get("nil", "http://www.w3.org/2001/XMLSchema-instance"));
        if (nil?.Value == "true")
            return null;
        return el.Value?.Trim();
    }
}

public sealed class ParsedSoapDocument
{
    public string? TransactionId { get; init; }
    public string? DocDate { get; init; }
    public string? DocDsc { get; init; }
    public string? DocTyp { get; init; }
    public string? DocTypDsc { get; init; }
    public string? DocRow { get; init; }
    public string? VchrTyp { get; init; }
    public string? ActTyp { get; init; }
    public string? ActDate { get; init; }
    public string? PhasTyp { get; init; }
    public string? RowDate { get; init; }
    public string? RowDocNo { get; init; }
    public string? Fund { get; init; }
    public string? BnkAcntNo { get; init; }
    public string? Center { get; init; }
    public IReadOnlyList<ParsedIncmRow> IncmRows { get; init; } = Array.Empty<ParsedIncmRow>();
}

public sealed class ParsedIncmRow
{
    public string? IncmRow { get; init; }
    public string? IncmNo { get; init; }
    public string? Val { get; init; }
    public string? Qty { get; init; }
    public string? Due { get; init; }
    public string? RefRowDocNo { get; init; }
    public string? RefRowDate { get; init; }
    public string? Reason { get; init; }
    public string? ReasonDsc { get; init; }
    public string? IncmRowDsc { get; init; }
    public string? Center1 { get; init; }
    public string? Center2 { get; init; }
    public string? Center3 { get; init; }
    public string? Ref { get; init; }

    public decimal? ParseVal() => ParseMoney(Val);
    public decimal? ParseQty() => ParseMoney(Qty);

    private static decimal? ParseMoney(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        var digits = new string(raw.Where(c => char.IsDigit(c) || c == '-' || c == '.').ToArray());
        return decimal.TryParse(digits, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : null;
    }
}
