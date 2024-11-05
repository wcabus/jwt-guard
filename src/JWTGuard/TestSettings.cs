using Duende.IdentityServer.Test;
using Microsoft.IdentityModel.Tokens;

namespace JWTGuard;

/// <summary>
/// Configure settings for the JWTGuard tests.
/// </summary>
public readonly struct TestSettings
{
    /// <summary>
    /// Static constructor for the <see cref="TestSettings"/> struct.
    /// </summary>
    static TestSettings()
    {
        // Override the default test settings here
        // CurrentTestSettings = DefaultTestSettings with
        // {
        //     TargetUrl = "/your-api-target-endpoint",
        //     DefaultAudience = "my-api",
        //     AllowedAudiences = ["my-api"]
        // };
    }

    /// <summary>
    /// List of known security algorithms. These are the ones JWT Guard can use to sign JWT tokens.
    /// </summary>
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

    /// <summary>
    /// List of security algorithms that are supported by Duende IdentityServer.
    /// </summary>
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

    /// <summary>
    /// Default constructor.
    /// </summary>
    public TestSettings()
    {
    }

    /// <summary>
    /// Default test settings.
    /// </summary>
    private static readonly TestSettings DefaultTestSettings = new();

    /// <summary>
    /// The current test settings. Defaults to <see cref="DefaultTestSettings"/>.
    /// </summary>
    public static TestSettings CurrentTestSettings { get; private set; } = DefaultTestSettings;

    /// <summary>
    /// Resets the test settings to the default values.
    /// </summary>
    public static void ResetTestSettings() => CurrentTestSettings = DefaultTestSettings;

    /// <summary>
    /// The default target API endpoint to test. Defaults to "/weatherforecast".
    /// </summary>
    public string TargetUrl { get; init; } = "/weatherforecast";
    
    /// <summary>
    /// The default test user. Defaults to a user with subject ID "1", username "alice", and password "password".
    /// </summary>
    public TestUser DefaultTestUser { get; init; } = new()
    {
        SubjectId = "1",
        Username = "alice",
        Password = "password"
    };
    
    /// <summary>
    /// The default audience. Defaults to "api".
    /// </summary>
    public string DefaultAudience { get; init; } = "api";

    /// <summary>
    /// Collection of allowed audiences. Defaults to [ "api" ].
    /// </summary>
    public IReadOnlyCollection<string> AllowedAudiences { get; init; } = ["api"];

    /// <summary>
    /// Collection of disallowed audiences. Defaults to [ "another-api" ].
    /// </summary>
    public IReadOnlyCollection<string> DisallowedAudiences { get; init; } = ["another-api"];
    
    /// <summary>
    /// The default issuer. Defaults to "https://localhost:5901".
    /// </summary>
    public string DefaultIssuer { get; init; } = "https://localhost:5901";

    /// <summary>
    /// Collection of allowed issuers. Defaults to [ "https://localhost:5901" ].
    /// </summary>
    public IReadOnlyCollection<string> AllowedIssuers { get; init; } = ["https://localhost:5901"];

    /// <summary>
    /// Collection of disallowed issuers. Defaults to [ "" ].
    /// </summary>
    public IReadOnlyCollection<string> DisallowedIssuers { get; init; } = [ "https://my-idp.org" ];
    
    /// <summary>
    /// Default signature algorithm. Defaults to "PS256".
    /// </summary>
    public string DefaultSignatureAlgorithm { get; init; } = SecurityAlgorithms.RsaSsaPssSha256;
    
    /// <summary>
    /// Collection of allowed signature algorithms. Defaults to [ "ES256", "ES384", "ES512", "PS256", "PS384", "PS512" ].
    /// </summary>
    public IReadOnlyCollection<string> AllowedAlgorithms { get; init; } = 
    [
        SecurityAlgorithms.EcdsaSha256,
        SecurityAlgorithms.EcdsaSha384,
        SecurityAlgorithms.EcdsaSha512,
        SecurityAlgorithms.RsaSsaPssSha256,
        SecurityAlgorithms.RsaSsaPssSha384,
        SecurityAlgorithms.RsaSsaPssSha512
    ];

    /// <summary>
    /// Collection of disallowed signature algorithms. Defaults to [ "custom", "none", "nOnE", "RS256", "RS384", "RS512", "HS256", "HS384", "HS512" ].
    /// </summary>
    public IReadOnlyCollection<string> DisallowedAlgorithms { get; init; } = 
    [
        "custom",
        SecurityAlgorithms.None,
        "nOnE",
        SecurityAlgorithms.RsaSha256,
        SecurityAlgorithms.RsaSha384,
        SecurityAlgorithms.RsaSha512,
        SecurityAlgorithms.HmacSha256,
        SecurityAlgorithms.HmacSha384,
        SecurityAlgorithms.HmacSha512
    ];
    
    /// <summary>
    /// Collection of valid token types. Defaults to [ "at+jwt" ].
    /// </summary>
    public IReadOnlyCollection<string> ValidTokenTypes { get; init; } = [ "at+jwt" ];

    /// <summary>
    /// Collection of invalid token types. Defaults to an empty collection.
    /// </summary>
    public IReadOnlyCollection<string> InvalidTokenTypes { get; init; } = [ "none", "jwt" ];
}