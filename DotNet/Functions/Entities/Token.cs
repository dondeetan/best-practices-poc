
using System.Text.Json.Serialization;

namespace Functions.Entities;
public partial class Token
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("expires_in_minutes")]
    public long ExpiresInMinutes { get; set; }
}
