namespace RayvarzResend.Web.Models;

/// <summary>نام ستون تاریخ ابطال در Sara — اگر در دیتابیس وجود ندارد خالی بگذارید (فقط وضعیت ۴/۲).</summary>
public sealed class FicheSchemaOptions
{
    /// <summary>Income_Fiche — مثلاً CanceledDate یا CancelDate؛ خالی = بدون ستون.</summary>
    public string? IncomeFicheCanceledDateColumn { get; set; }

    /// <summary>Duty_Fiche — مثلاً CanceledDate؛ خالی = بدون ستون.</summary>
    public string? DutyFicheCanceledDateColumn { get; set; } = "CanceledDate";
}
