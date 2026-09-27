using System.Text.Json;
using System.Text.Json.Serialization;

namespace RayvarzResend.Web.Services;

internal static class MashhadSsoJson
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = null,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}

internal sealed class MashhadLoginKeyRequest
{
    [JsonPropertyName("Time")]
    public string Time { get; set; } = "";

    [JsonPropertyName("Hash")]
    public string Hash { get; set; } = "";

    [JsonPropertyName("ClientId")]
    public string ClientId { get; set; } = "";

    [JsonPropertyName("State")]
    public string State { get; set; } = "";

    [JsonPropertyName("UserType")]
    public int UserType { get; set; }

    [JsonPropertyName("DomainID")]
    public int DomainId { get; set; }
}

internal sealed class MashhadAccessTokenRequest
{
    [JsonPropertyName("RefreshToken")]
    public string RefreshToken { get; set; } = "";

    [JsonPropertyName("UserName")]
    public string UserName { get; set; } = "";

    [JsonPropertyName("ClientID")]
    public string ClientId { get; set; } = "";
}

internal sealed class MashhadUserInfoRequest
{
    [JsonPropertyName("Token")]
    public string Token { get; set; } = "";
}
