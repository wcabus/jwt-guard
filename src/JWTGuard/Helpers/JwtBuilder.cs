using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Duende.IdentityServer.Test;

using IdentityModel;

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using JwtHeaderParameterNames = Microsoft.IdentityModel.JsonWebTokens.JwtHeaderParameterNames;

namespace JWTGuard.Helpers;

/// <summary>
/// Utility class to help build JSON Web Tokens.
/// </summary>
public class JwtBuilder
{
    private readonly ISigningCredentialsProvider _signingCredentialsProvider;
    private readonly JsonWebTokenHandler _tokenHandler;

    internal JwtBuilder(ISigningCredentialsProvider signingCredentialsProvider, JsonWebTokenHandler tokenHandler)
    {
        _signingCredentialsProvider = signingCredentialsProvider;
        _tokenHandler = tokenHandler;
    }

    /// <summary>
    /// The token type or "typ" property in the token's header.
    /// </summary>
    public string TokenType { get; private set; } = TestSettings.CurrentTestSettings.ValidTokenTypes.FirstOrDefault() ?? "";

    /// <summary>
    /// The intended audience of the token ("aud" claim in the token payload). 
    /// </summary>
    public string Audience { get; private set; } = TestSettings.CurrentTestSettings.DefaultAudience;

    /// <summary>
    /// The issuer of the token ("iss" claim in the token payload).
    /// </summary>
    public string Issuer { get; private set; } = TestSettings.CurrentTestSettings.DefaultIssuer;

    /// <summary>
    /// The signing credentials used to sign the JSON Web Token. <see cref="SigningCredentials"/>.  
    /// </summary>
    public SigningCredentials? SigningCredentials { get; private set; }
    
    /// <summary>
    /// The signature algorithm to use when signing the JSON Web Token.
    /// </summary>
    public string? SignatureAlgorithm { get; private set; }
    
    /// <summary>
    ///  A secret to use when signing the JSON Web Token using an HMACSHA algorithm.
    /// </summary>
    /// <remarks>This value is prepopulated with enough data to satisfy HMACSHA512.</remarks>
    public string? HmacShaSecret { get; private set; } = $"{Guid.NewGuid()}{Guid.NewGuid()}"; // Ensure this is long enough for 512-bit HMAC by default.

    /// <summary>
    /// The timestamp when the token is issued ("iat" claim in the token payload). Defaults to now.
    /// </summary>
    public DateTime IssuedAt { get; private set; } = DateTime.UtcNow;
    
    /// <summary>
    /// The timestamp when the token becomes valid ("nbf" claim in the token payload). Defaults to now minus ten seconds.
    /// </summary>
    public DateTime NotBefore { get; private set; } = DateTime.UtcNow.AddSeconds(-10);
    
    /// <summary>
    /// The timestamp when the token expires ("exp" claim in the token payload). Defaults to now plus five minutes.
    /// </summary>
    public DateTime Expires { get; private set; } = DateTime.UtcNow.AddMinutes(5);

    /// <summary>
    /// The subject to include in the JSON Web Token payload as claims. Defaults to using <see cref="TestSettings.DefaultTestUser"/>.
    /// </summary>
    public ClaimsIdentity? Subject { get; private set; } = new([
        new Claim(JwtClaimTypes.Subject, TestSettings.CurrentTestSettings.DefaultTestUser.SubjectId),
        new Claim(JwtClaimTypes.Name, TestSettings.CurrentTestSettings.DefaultTestUser.Username),
        new Claim(JwtClaimTypes.PreferredUserName, TestSettings.CurrentTestSettings.DefaultTestUser.Username)
    ]);

    /// <summary>
    /// Allows you to override the token type ("typ").
    /// </summary>
    public JwtBuilder WithTokenType(string tokenType)
    {
        TokenType = tokenType;
        return this;
    }

    /// <summary>
    /// Allows you to override the audience ("aud").
    /// </summary>
    public JwtBuilder WithAudience(string audience)
    {
        Audience = audience;
        return this;
    }

    /// <summary>
    /// Allows you to override the issuer ("iss").
    /// </summary>
    public JwtBuilder WithIssuer(string issuer)
    {
        Issuer = issuer;
        return this;
    }

    /// <summary>
    /// Allows you to use different signing credentials to sign the JWT with.
    /// </summary>
    public JwtBuilder WithSigningCredentials(SigningCredentials signingCredentials)
    {
        SigningCredentials = signingCredentials;
        SignatureAlgorithm = null;

        return this;
    }

    /// <summary>
    /// Allows you to specify another signature algorithm to use when signing the JWT.
    /// </summary>
    public JwtBuilder WithSignatureAlgorithm(string signatureAlgorithm)
    {
        SignatureAlgorithm = signatureAlgorithm;
        SigningCredentials = null;

        return this;
    }

    /// <summary>
    /// Allows you to specify another signature algorithm to use when signing the JWT, including overriding the HMACSHA secret.
    /// </summary>
    public JwtBuilder WithSignatureAlgorithm(string signatureAlgorithm, string hmacShaSecret)
    {
        SignatureAlgorithm = signatureAlgorithm;
        SigningCredentials = null;
        HmacShaSecret = hmacShaSecret;

        return this;
    }

    /// <summary>
    /// Allows you to override the "issued at" timestamp ("iat").
    /// </summary>
    public JwtBuilder WithIssuedAt(DateTime issuedAt)
    {
        IssuedAt = issuedAt;
        return this;
    }

    /// <summary>
    /// Allows you to override the "not before" timestamp ("nbf").
    /// </summary>
    public JwtBuilder WithNotBefore(DateTime notBefore)
    {
        NotBefore = notBefore;
        return this;
    }

    /// <summary>
    /// Allows you to override the "expiration" timestamp ("exp").
    /// </summary>
    public JwtBuilder WithExpires(DateTime expires)
    {
        Expires = expires;
        return this;
    }

    /// <summary>
    /// Allows you to override the subject used to identify the user in the JWT.
    /// </summary>
    /// <param name="subject">A <see cref="ClaimsIdentity"/> which represents the current user.</param>
    public JwtBuilder WithSubject(ClaimsIdentity subject)
    {
        Subject = subject;
        return this;
    }

    /// <summary>
    /// Allows you to override the subject used to identify the user in the JWT.
    /// </summary>
    /// <param name="subject">A <see cref="TestUser"/> which represents the current user.</param>
    public JwtBuilder WithSubject(TestUser subject)
    {
        Subject = new ClaimsIdentity([
            new Claim(JwtClaimTypes.Subject, subject.SubjectId),
            new Claim(JwtClaimTypes.Name, subject.Username),
            new Claim(JwtClaimTypes.PreferredUserName, subject.Username)
        ]);

        return this;
    }

    /// <summary>
    /// Builds the JWT header.
    /// </summary>
    public JwtHeader BuildJwtHeader()
    {
        return new JwtHeader
        {
            [JwtHeaderParameterNames.Alg] = SignatureAlgorithm,
            [JwtHeaderParameterNames.Typ] = TokenType
        };
    }

    /// <summary>
    /// Builds the JWT payload.
    /// </summary>
    public JwtPayload BuildJwtPayload()
    {
        return new JwtPayload(Issuer, Audience, Subject?.Claims ?? [], NotBefore, Expires, IssuedAt);
    }

    /// <summary>
    /// Builds and signs the JWT token.
    /// </summary>
    /// <returns>A string containing the signed JWT token.</returns>
    /// <remarks>If an unknown signature algorithm or "none" is specified, the signature will be empty.</remarks>
    public async Task<string> BuildAsync()
    {
        SigningCredentials ??= await GetSigningCredentialsAsync();

        if (!string.IsNullOrEmpty(SignatureAlgorithm) &&
            (string.Equals(SecurityAlgorithms.None, SignatureAlgorithm, StringComparison.OrdinalIgnoreCase) ||
            !TestSettings.KnownSecurityAlgorithms.Contains(SignatureAlgorithm)))
        {
            // Either using "none" (case-insensitive) or an unknown algorithm.
            return BuildJwtHeader().Base64UrlEncode() + "." + BuildJwtPayload().Base64UrlEncode() + ".";
        }

        var tokenPayload = new SecurityTokenDescriptor
        {
            TokenType = TokenType,
            Audience = Audience,
            Issuer = Issuer,
            SigningCredentials = SigningCredentials,
            IssuedAt = IssuedAt,
            NotBefore = NotBefore,
            Expires = Expires,
            Subject = Subject
        };

        return _tokenHandler.CreateToken(tokenPayload);
    }

    private async Task<SigningCredentials?> GetSigningCredentialsAsync()
    {
        if (SignatureAlgorithm is null)
        {
            return await _signingCredentialsProvider.GetSigningCredentialsAsync(TestSettings.CurrentTestSettings.DefaultSignatureAlgorithm);
        }

        if (string.Equals(SecurityAlgorithms.None, SignatureAlgorithm, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (TestSettings.DuendeSupportedSecurityAlgorithms.Contains(SignatureAlgorithm))
        {
            // Attempt to get the signing credentials for the specified algorithm.
            return await _signingCredentialsProvider.GetSigningCredentialsAsync(SignatureAlgorithm);
        }

        if (TestSettings.KnownSecurityAlgorithms.Contains(SignatureAlgorithm))
        {
            if (string.IsNullOrEmpty(HmacShaSecret))
            {
                throw new InvalidOperationException("The HMAC secret is not set.");
            }

            // Can't generate signing credentials for the specified algorithm using Duende Identity Server. Currently only applies to HMAC algorithms.
            var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(HmacShaSecret));
            return new SigningCredentials(signinKey, SignatureAlgorithm);
        }

        // Unknown algorithm
        return null;
    }
}