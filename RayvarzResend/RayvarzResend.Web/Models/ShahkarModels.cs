namespace RayvarzResend.Web.Models;

public sealed class ShahkarSearchRequest
{
    public string? UserName { get; set; }
    public string? NationalCode { get; set; }
}

public sealed class ShahkarUserRowDto
{
    public string UserName { get; set; } = "";
    public string NationalCode { get; set; } = "";
    public string ShahkarOk { get; set; } = "";
    public bool IsShahkarOk => ShahkarOk is "1" or "true" or "True" or "Y" or "y";
}

public sealed class ShahkarSearchResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public List<ShahkarUserRowDto> Items { get; set; } = [];
    public int Total { get; set; }
}

public sealed class ShahkarConfirmRequest
{
    public List<string>? UserNames { get; set; }
}

public sealed class ShahkarConfirmResult
{
    public bool Success { get; set; }
    public bool DryRun { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
    public int Updated { get; set; }
    public List<ShahkarConfirmRowResult> Results { get; set; } = [];
}

public sealed class ShahkarConfirmRowResult
{
    public string UserName { get; set; } = "";
    public bool Success { get; set; }
    public string? Message { get; set; }
}
