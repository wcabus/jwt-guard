using System.Text.Json.Serialization;

using Microsoft.IdentityModel.Tokens;

namespace JWTGuard.Helpers;

public class JsonWebKeys
{
    [JsonPropertyName("keys")]
    public IList<JsonWebKey> Keys { get; } = new List<JsonWebKey>();
}