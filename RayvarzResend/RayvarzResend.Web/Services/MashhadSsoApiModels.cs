namespace RayvarzResend.Web.Services;

public sealed class MashhadSsoResult<T>
{
    public int ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public T? Data { get; set; }

    public bool IsSuccess => ErrorCode == 0;
}

public sealed class MashhadLoginKeyData
{
    public string? loginKey { get; set; }
    public string? LoginKey { get; set; }
    public string? ExpireTime { get; set; }

    public string? EffectiveLoginKey =>
        string.IsNullOrWhiteSpace(loginKey) ? LoginKey?.Trim() : loginKey.Trim();
}

public sealed class MashhadAccessTokenData
{
    public string? Status { get; set; }
    public string? AccessToken { get; set; }
    public string? RxpireAt { get; set; }
}

public sealed class MashhadUserInfoData
{
    public string? FName { get; set; }
    public string? LName { get; set; }
    public string? NationalCode { get; set; }
    public string? UserName { get; set; }
    public MashhadOldSsoUserInfo? OldSSO_UserInfo { get; set; }
}

public sealed class MashhadOldSsoUserInfo
{
    public MashhadSsoBasicInfo? basicInfo { get; set; }
}

public sealed class MashhadSsoBasicInfo
{
    public string? username { get; set; }
    public string? nationalCode { get; set; }
    public string? firstname { get; set; }
    public string? surname { get; set; }
}
