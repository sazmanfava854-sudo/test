using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

internal sealed class AppUserGroupRecord
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public bool CanAccessUnsentFiches { get; set; }
    public bool CanAccessInstallment { get; set; }
    public bool CanManageUsers { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
