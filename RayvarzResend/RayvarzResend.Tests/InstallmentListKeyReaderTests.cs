using System.Data;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Tests;

public class InstallmentListKeyReaderTests
{
    [Fact]
    public void ReadNidInstallment_parses_guid_column()
    {
        var expected = Guid.Parse("E37F8EA3-4C97-4935-9B48-8482A4CB3391");
        using var reader = CreateReader(("NidInstallment", typeof(Guid), expected));
        Assert.True(reader.Read());
        Assert.Equal(expected, InstallmentListKeyReader.ReadNidInstallment(reader));
    }

    [Fact]
    public void ReadNidInstallment_parses_guid_string()
    {
        var expected = Guid.Parse("E37F8EA3-4C97-4935-9B48-8482A4CB3391");
        using var reader = CreateReader(("NidInstallment", typeof(string), expected.ToString()));
        Assert.True(reader.Read());
        Assert.Equal(expected, InstallmentListKeyReader.ReadNidInstallment(reader));
    }

    private static DataTableReader CreateReader(params (string Name, Type Type, object Value)[] columns)
    {
        var table = new DataTable();
        foreach (var (name, type, _) in columns)
            table.Columns.Add(name, type);
        var row = table.NewRow();
        foreach (var (name, _, value) in columns)
            row[name] = value;
        table.Rows.Add(row);
        return table.CreateDataReader();
    }
}
