namespace RayvarzResend.Web.Models;

public sealed class AppUserRecord
{
    public Guid Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string NationalId { get; set; } = "";
    public string Position { get; set; } = "";
    public string District { get; set; } = "";
    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class LoginRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public sealed class CreateAppUserRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? NationalId { get; set; }
    public string? Position { get; set; }
    public string? District { get; set; }
    public bool IsAdmin { get; set; }
}

public sealed class AppUserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string NationalId { get; set; } = "";
    public string Position { get; set; } = "";
    public string District { get; set; } = "";
    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; }
    public string CreatedAtUtc { get; set; } = "";
    public List<Guid> GroupIds { get; set; } = [];
}

public sealed class AuthSessionDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string NationalId { get; set; } = "";
    public string Position { get; set; } = "";
    public string District { get; set; } = "";
    public bool IsAdmin { get; set; }
    public bool CanAccessUnsentFiches { get; set; }
    public bool CanAccessInstallment { get; set; }
    public bool CanAccessFicheDateChange { get; set; }
    public bool CanAccessBankInquiryConfirm { get; set; }
    public bool CanManageUsers { get; set; }
    public List<Guid> GroupIds { get; set; } = [];
}

public sealed class UserPermissionsDto
{
    public bool IsAdmin { get; set; }
    public bool CanAccessUnsentFiches { get; set; }
    public bool CanAccessInstallment { get; set; }
    public bool CanAccessFicheDateChange { get; set; }
    public bool CanAccessBankInquiryConfirm { get; set; }
    public bool CanManageUsers { get; set; }
    public List<Guid> GroupIds { get; set; } = [];

    public static UserPermissionsDto FullAdmin() => new()
    {
        IsAdmin = true,
        CanAccessUnsentFiches = true,
        CanAccessInstallment = true,
        CanAccessFicheDateChange = true,
        CanAccessBankInquiryConfirm = true,
        CanManageUsers = true
    };
}

public sealed class AppUserGroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public bool CanAccessUnsentFiches { get; set; }
    public bool CanAccessInstallment { get; set; }
    public bool CanAccessFicheDateChange { get; set; }
    public bool CanAccessBankInquiryConfirm { get; set; }
    public bool CanManageUsers { get; set; }
    public string CreatedAtUtc { get; set; } = "";
}

public sealed class CreateAppUserGroupRequest
{
    public string? Name { get; set; }
    public bool CanAccessUnsentFiches { get; set; }
    public bool CanAccessInstallment { get; set; }
    public bool CanAccessFicheDateChange { get; set; }
    public bool CanAccessBankInquiryConfirm { get; set; }
    public bool CanManageUsers { get; set; }
}

public sealed class UpdateAppUserGroupRequest
{
    public string? Name { get; set; }
    public bool CanAccessUnsentFiches { get; set; }
    public bool CanAccessInstallment { get; set; }
    public bool CanAccessFicheDateChange { get; set; }
    public bool CanAccessBankInquiryConfirm { get; set; }
    public bool CanManageUsers { get; set; }
}

public sealed class UpdateAppUserRequest
{
    public bool? IsAdmin { get; set; }
    public bool? IsActive { get; set; }
    public List<Guid>? GroupIds { get; set; }
}

public sealed class ResetAppUserPasswordRequest
{
    public string? Password { get; set; }
}

public static class AuthPolicies
{
    public const string Authenticated = "Authenticated";
    public const string AdminOnly = "AdminOnly";
}

public static class AuthClaimTypes
{
    public const string UserId = "rayvarz:userId";
    public const string IsAdmin = "rayvarz:isAdmin";
    public const string DisplayName = "rayvarz:displayName";
    public const string District = "rayvarz:district";
}
