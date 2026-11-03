using System.Data;

namespace RayvarzResend.Web.Services;

/// <summary>خواندن NidInstallment از Installment_List — در Sara معمولاً uniqueidentifier است.</summary>
public static class InstallmentListKeyReader
{
    public static Guid ReadNidInstallment(IDataRecord reader, string column = "NidInstallment")
    {
        var ordinal = reader.GetOrdinal(column);
        if (reader.IsDBNull(ordinal))
            return Guid.Empty;

        var value = reader.GetValue(ordinal);
        return value switch
        {
            Guid g => g,
            byte[] bytes when bytes.Length == 16 => new Guid(bytes),
            string s when Guid.TryParse(s, out var parsed) => parsed,
            _ => Guid.TryParse(value.ToString(), out var fromText) ? fromText : Guid.Empty
        };
    }
}
