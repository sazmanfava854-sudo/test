namespace RayvarzResend.Web.Models;

public sealed class ShimasUserProfile
{
    public string Username { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string NationalId { get; set; } = "";
    public string Position { get; set; } = "";
    public string District { get; set; } = "";
}

public sealed class ShimasValidationResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public bool UsedRemoteApi { get; set; }
    public ShimasUserProfile Profile { get; set; } = new();
}

public sealed class ShimasAuthStatusDto
{
    public bool Enabled { get; set; }
    public bool SsoReady { get; set; }
    public bool PreferSsoLogin { get; set; }
    public bool LocalLoginAvailable { get; set; }
    public string LoginPath { get; set; } = "/auth/login";
    public string CallbackPath { get; set; } = "/auth/callback";
}
