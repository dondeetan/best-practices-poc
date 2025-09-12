
using System.Text.Json.Serialization;

namespace Functions.Entities;
public partial class Token
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }

    [JsonPropertyName("expires_in_minutes")]
    public long ExpiresInMinutes { get; set; }
}