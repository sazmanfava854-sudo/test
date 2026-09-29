namespace MashhadSso;

public sealed class SsoResult<T>
{
    public int ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public T? Data { get; set; }
}

public sealed class LoginKeyResult
{
    public string? loginKey { get; set; }
    public string? ExpireTime { get; set; }
}

public sealed class AccessTokenResult
{
    public string? Status { get; set; }
    public string? AccessToken { get; set; }
    public string? RxpireAt { get; set; }
}

public sealed class LoginKeyRequestBody
{
    public string Time { get; set; } = "";
    public string Hash { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string State { get; set; } = "test";
    public int UserType { get; set; }
    public int DomainID { get; set; }
}

public sealed class AccessTokenRequestBody
{
    public string RefreshToken { get; set; } = "";
    public string UserName { get; set; } = "";
    public string ClientID { get; set; } = "";
}

public sealed class UserInfoRequestBody
{
    public string Token { get; set; } = "";
}
