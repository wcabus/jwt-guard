using System.Text.Json.Serialization;

using Microsoft.IdentityModel.Tokens;

namespace JWTGuard.Helpers;

/// <summary>
/// Represents a list of JSON Web Keys to serialize into JSON.
/// </summary>
public class JsonWebKeys
{
    /// <summary>
    /// A list of <see cref="JsonWebKey"/>. 
    /// </summary>
    [JsonPropertyName("keys")]
    public IList<JsonWebKey> Keys { get; } = new List<JsonWebKey>();
}