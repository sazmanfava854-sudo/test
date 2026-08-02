using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>
/// ذخیره پایدار snapshot تهاتر روی RayvarzRuleEngine قبل از UPDATE وضعیت ۲.
/// </summary>
public sealed class TahatorSnapshotStore
{
    public const string StatusPending = "Pending";
    public const string StatusRestored = "Restored";
    public const string StatusAbandoned = "Abandoned";

    private readonly string? _cs;
    private readonly ILogger<TahatorSnapshotStore> _logger;
    private bool _schemaEnsured;

    public TahatorSnapshotStore(IConfiguration config, ILogger<TahatorSnapshotStore> logger)
    {
        _cs = config.GetConnectionString("RayvarzRuleEngine");
        _logger = logger;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_cs);

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        if (!IsConfigured || _schemaEnsured) return;

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        const string sql = @"
IF OBJECT_ID(N'dbo.TahatorRestoreSnapshot', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TahatorRestoreSnapshot (
        SnapshotId              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TahatorRestoreSnapshot PRIMARY KEY,
        FicheNo                 NVARCHAR(50)  NOT NULL,
        EumFicheStatus          INT           NOT NULL,
        ExportPermanentDate     NVARCHAR(30)  NULL,
        PaymentBreakDate        NVARCHAR(30)  NULL,
        PaymentDate             NVARCHAR(30)  NULL,
        UserConfirmDate         NVARCHAR(30)  NULL,
        UsernameUserConfirm     NVARCHAR(200) NULL,
        NidUserUserConfirm      UNIQUEIDENTIFIER NULL,
        TriggerDate             NVARCHAR(30)  NULL,
        Status                  VARCHAR(30)   NOT NULL CONSTRAINT DF_TahatorRestore_Status DEFAULT ('Pending'),
        CreatedAtUtc            DATETIME2(3)  NOT NULL CONSTRAINT DF_TahatorRestore_Created DEFAULT (SYSUTCDATETIME()),
        RestoredAtUtc           DATETIME2(3)  NULL,
        Notes                   NVARCHAR(500) NULL
    );
    CREATE INDEX IX_TahatorRestore_Fiche_Status
        ON dbo.TahatorRestoreSnapshot (FicheNo, Status)
        INCLUDE (CreatedAtUtc);
END";
        await using var cmd = new SqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync(ct);
        _schemaEnsured = true;
        _logger.LogInformation("TahatorRestoreSnapshot schema ensured on RayvarzRuleEngine");
    }

    public async Task<long> InsertPendingAsync(IncomeFicheTahatorSnapshot snap, string? triggerDate, string? notes, CancellationToken ct = default)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("ConnectionStrings:RayvarzRuleEngine برای ذخیره snapshot تهاتر تنظیم نشده.");

        await EnsureSchemaAsync(ct);

        // Pending قبلی همان فیش را Abandoned کن تا فقط یک Pending فعال بماند
        await AbandonPendingForFicheAsync(snap.FicheNo, "جایگزین با snapshot جدید", ct);

        const string sql = @"
INSERT INTO dbo.TahatorRestoreSnapshot
    (FicheNo, EumFicheStatus, ExportPermanentDate, PaymentBreakDate, PaymentDate,
     UserConfirmDate, UsernameUserConfirm, NidUserUserConfirm, TriggerDate, Status, Notes)
OUTPUT INSERTED.SnapshotId
VALUES
    (@f, @st, @export, @brk, @pay, @ucDate, @ucName, @ucNid, @trigger, @status, @notes);";

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@f", snap.FicheNo);
        cmd.Parameters.AddWithValue("@st", snap.EumFicheStatus);
        cmd.Parameters.AddWithValue("@export", (object?)NullIfEmpty(snap.ExportPermanentDate) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@brk", (object?)NullIfEmpty(snap.PaymentBreakDate) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@pay", (object?)NullIfEmpty(snap.PaymentDate) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ucDate", (object?)NullIfEmpty(snap.UserConfirmDate) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ucName", (object?)NullIfEmpty(snap.UsernameUserConfirm) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ucNid", (object?)snap.NidUserUserConfirm ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@trigger", (object?)NullIfEmpty(triggerDate) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@status", StatusPending);
        cmd.Parameters.AddWithValue("@notes", (object?)NullIfEmpty(notes) ?? DBNull.Value);

        var id = Convert.ToInt64(await cmd.ExecuteScalarAsync(ct));
        _logger.LogInformation("Tahator snapshot saved SnapshotId={Id} FicheNo={FicheNo}", id, snap.FicheNo);
        return id;
    }

    public async Task<IncomeFicheTahatorSnapshot?> GetPendingAsync(string ficheNo, CancellationToken ct = default)
    {
        if (!IsConfigured) return null;
        await EnsureSchemaAsync(ct);

        const string sql = @"
SELECT TOP 1 SnapshotId, FicheNo, EumFicheStatus, ExportPermanentDate, PaymentBreakDate, PaymentDate,
       UserConfirmDate, UsernameUserConfirm, NidUserUserConfirm, TriggerDate, Status, CreatedAtUtc
FROM dbo.TahatorRestoreSnapshot
WHERE FicheNo = @f AND Status = @st
ORDER BY SnapshotId DESC";

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@f", ficheNo);
        cmd.Parameters.AddWithValue("@st", StatusPending);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;
        return Map(reader);
    }

    public async Task<IncomeFicheTahatorSnapshot?> GetByIdAsync(long snapshotId, CancellationToken ct = default)
    {
        if (!IsConfigured) return null;
        await EnsureSchemaAsync(ct);

        const string sql = @"
SELECT SnapshotId, FicheNo, EumFicheStatus, ExportPermanentDate, PaymentBreakDate, PaymentDate,
       UserConfirmDate, UsernameUserConfirm, NidUserUserConfirm, TriggerDate, Status, CreatedAtUtc
FROM dbo.TahatorRestoreSnapshot
WHERE SnapshotId = @id";

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", snapshotId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;
        return Map(reader);
    }

    public async Task<IReadOnlyList<IncomeFicheTahatorSnapshot>> ListPendingAsync(int take = 50, CancellationToken ct = default)
    {
        if (!IsConfigured) return Array.Empty<IncomeFicheTahatorSnapshot>();
        await EnsureSchemaAsync(ct);

        const string sql = @"
SELECT TOP (@n) SnapshotId, FicheNo, EumFicheStatus, ExportPermanentDate, PaymentBreakDate, PaymentDate,
       UserConfirmDate, UsernameUserConfirm, NidUserUserConfirm, TriggerDate, Status, CreatedAtUtc
FROM dbo.TahatorRestoreSnapshot
WHERE Status = @st
ORDER BY SnapshotId DESC";

        var list = new List<IncomeFicheTahatorSnapshot>();
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@n", take);
        cmd.Parameters.AddWithValue("@st", StatusPending);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            list.Add(Map(reader));
        return list;
    }

    public async Task MarkRestoredAsync(long snapshotId, string? notes = null, CancellationToken ct = default)
    {
        if (!IsConfigured) return;
        await EnsureSchemaAsync(ct);

        const string sql = @"
UPDATE dbo.TahatorRestoreSnapshot
SET Status = @st, RestoredAtUtc = SYSUTCDATETIME(),
    Notes = CASE WHEN @notes IS NULL OR @notes = '' THEN Notes ELSE @notes END
WHERE SnapshotId = @id";

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", snapshotId);
        cmd.Parameters.AddWithValue("@st", StatusRestored);
        cmd.Parameters.AddWithValue("@notes", (object?)NullIfEmpty(notes) ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task AbandonPendingForFicheAsync(string ficheNo, string? notes, CancellationToken ct = default)
    {
        if (!IsConfigured) return;
        await EnsureSchemaAsync(ct);

        const string sql = @"
UPDATE dbo.TahatorRestoreSnapshot
SET Status = @st, RestoredAtUtc = SYSUTCDATETIME(), Notes = ISNULL(@notes, Notes)
WHERE FicheNo = @f AND Status = @pending";

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@f", ficheNo);
        cmd.Parameters.AddWithValue("@st", StatusAbandoned);
        cmd.Parameters.AddWithValue("@pending", StatusPending);
        cmd.Parameters.AddWithValue("@notes", (object?)NullIfEmpty(notes) ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static IncomeFicheTahatorSnapshot Map(SqlDataReader reader) =>
        new()
        {
            SnapshotId = reader.GetInt64(reader.GetOrdinal("SnapshotId")),
            FicheNo = reader.GetString(reader.GetOrdinal("FicheNo")),
            EumFicheStatus = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("EumFicheStatus"))),
            ExportPermanentDate = ReadStr(reader, "ExportPermanentDate"),
            PaymentBreakDate = ReadStr(reader, "PaymentBreakDate"),
            PaymentDate = ReadStr(reader, "PaymentDate"),
            UserConfirmDate = ReadStr(reader, "UserConfirmDate"),
            UsernameUserConfirm = ReadStr(reader, "UsernameUserConfirm"),
            NidUserUserConfirm = ReadGuid(reader, "NidUserUserConfirm"),
            TriggerDate = ReadStr(reader, "TriggerDate"),
            PersistStatus = ReadStr(reader, "Status"),
            CreatedAtUtc = reader.IsDBNull(reader.GetOrdinal("CreatedAtUtc"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
        };

    private static string? ReadStr(SqlDataReader r, string col)
    {
        var o = r.GetOrdinal(col);
        return r.IsDBNull(o) ? null : r.GetValue(o)?.ToString();
    }

    private static Guid? ReadGuid(SqlDataReader r, string col)
    {
        var o = r.GetOrdinal(col);
        if (r.IsDBNull(o)) return null;
        var v = r.GetValue(o);
        if (v is Guid g) return g;
        return Guid.TryParse(v?.ToString(), out var p) ? p : null;
    }

    private static string? NullIfEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;
}
