using Microsoft.IdentityModel.Tokens;

namespace JWTGuard;

/// <summary>
/// Configure settings for the JWTGuard tests.
/// </summary>
public readonly struct TestSettings
{
    public static readonly IReadOnlyCollection<string> KnownSecurityAlgorithms =
    [
        SecurityAlgorithms.EcdsaSha256,
        SecurityAlgorithms.EcdsaSha384,
        SecurityAlgorithms.EcdsaSha512,
        SecurityAlgorithms.RsaSsaPssSha256,
        SecurityAlgorithms.RsaSsaPssSha384,
        SecurityAlgorithms.RsaSsaPssSha512,
        SecurityAlgorithms.HmacSha256,
        SecurityAlgorithms.HmacSha384,
        SecurityAlgorithms.HmacSha512,
        SecurityAlgorithms.RsaSha256,
        SecurityAlgorithms.RsaSha384,
        SecurityAlgorithms.RsaSha512
    ];

    public static readonly IReadOnlyCollection<string> DuendeSupportedSecurityAlgorithms =
    [
        SecurityAlgorithms.EcdsaSha256,
        SecurityAlgorithms.EcdsaSha384,
        SecurityAlgorithms.EcdsaSha512,
        SecurityAlgorithms.RsaSsaPssSha256,
        SecurityAlgorithms.RsaSsaPssSha384,
        SecurityAlgorithms.RsaSsaPssSha512,
        SecurityAlgorithms.RsaSha256,
        SecurityAlgorithms.RsaSha384,
        SecurityAlgorithms.RsaSha512
    ];

    public static readonly IReadOnlyCollection<string> WeirdSecurityAlgorithms =
    [
        "custom",
        SecurityAlgorithms.None,
        "nOnE"
    ];

    static TestSettings()
    {
        // Override the default test settings here
        //CurrentTestSettings = DefaultTestSettings with
        //{
        //    ValidTokenTypes = ["at+jwt"],
        //    InvalidTokenTypes = ["none", "jwt"],
        //};
    }

    public TestSettings()
    {
    }

    /// <summary>
    /// Default test settings.
    /// </summary>
    public static readonly TestSettings DefaultTestSettings = new();

    /// <summary>
    /// The current test settings. Defaults to <see cref="DefaultTestSettings"/>.
    /// </summary>
    public static TestSettings CurrentTestSettings { get; set; } = DefaultTestSettings;

    /// <summary>
    /// Resets the test settings to the default values.
    /// </summary>
    public static void ResetTestSettings() => CurrentTestSettings = DefaultTestSettings;

    /// <summary>
    /// Collection of valid token types. Defaults to [ "at+jwt" ].
    /// </summary>
    public IReadOnlyCollection<string> ValidTokenTypes { get; init; } = [ "at+jwt" ];

    /// <summary>
    /// Collection of invalid token types. Defaults to an empty collection.
    /// </summary>
    public IReadOnlyCollection<string> InvalidTokenTypes { get; init; } = [ "none", "jwt" ];

    /// <summary>
    /// Collection of supported signature algorithms. Defaults to [ "ES256", "ES384", "ES512", "PS256", "PS384", "PS512" ].
    /// </summary>
    public IReadOnlyCollection<string> SupportedAlgorithms { get; init; } = 
    [
        SecurityAlgorithms.EcdsaSha256,
        SecurityAlgorithms.EcdsaSha384,
        SecurityAlgorithms.EcdsaSha512,
        SecurityAlgorithms.RsaSsaPssSha256,
        SecurityAlgorithms.RsaSsaPssSha384,
        SecurityAlgorithms.RsaSsaPssSha512
    ];

    /// <summary>
    /// Default signature algorithm. Defaults to "PS256".
    /// </summary>
    public string DefaultSignatureAlgorithm { get; init; } = SecurityAlgorithms.RsaSsaPssSha256;

    /// <summary>
    /// Collection of unsupported signature algorithms. Defaults to [ "custom", "none", "nOnE", "HS256", "HS384", "HS512", "RS256", "RS384", "RS512" ].
    /// </summary>
    public IReadOnlyCollection<string> UnsupportedAlgorithms { get; init; } = 
    [
        "custom",
        SecurityAlgorithms.None,
        "nOnE",
        SecurityAlgorithms.HmacSha256,
        SecurityAlgorithms.HmacSha384,
        SecurityAlgorithms.HmacSha512,
        SecurityAlgorithms.RsaSha256,
        SecurityAlgorithms.RsaSha384,
        SecurityAlgorithms.RsaSha512
    ];

    /// <summary>
    /// Public key used for verifying and signing tokens using an HMAC algorithm. Defaults to "hmac-public-key".
    /// </summary>
    public string HmacPublicKey { get; init; } = "hmac-public-key";

    /// <summary>
    /// Expected issuer. Defaults to "https://jwtguard.net".
    /// </summary>
    public string Issuer { get; init; } = "https://jwtguard.net";

    /// <summary>
    /// Invalid issuer. Defaults to "https://invalid.jwtguard.net".
    /// </summary>
    public string InvalidIssuer { get; init; } = "https://invalid.jwtguard.net";

    /// <summary>
    /// Collection of valid audiences. Set this to your expected audiences. Defaults to <c>null</c>, meaning audiences are not validated.
    /// </summary>
    public IReadOnlyCollection<string>? ValidAudiences { get; init; }

    /// <summary>
    /// Collection of invalid audiences. Defaults to [ "https://invalid.jwtguard.net" ].
    /// </summary>
    public IReadOnlyCollection<string> InvalidAudiences { get; init; } = ["https://invalid.jwtguard.net"];
}
