using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Duende.IdentityServer.Test;

using IdentityModel;

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using JwtHeaderParameterNames = Microsoft.IdentityModel.JsonWebTokens.JwtHeaderParameterNames;

namespace JWTGuard.Helpers;

public class JwtBuilder
{
    private readonly ISigningCredentialsProvider _signingCredentialsProvider;
    private readonly JsonWebTokenHandler _tokenHandler;

    internal JwtBuilder(ISigningCredentialsProvider signingCredentialsProvider, JsonWebTokenHandler tokenHandler)
    {
        _signingCredentialsProvider = signingCredentialsProvider;
        _tokenHandler = tokenHandler;
    }

    public string TokenType { get; private set; } = TestSettings.CurrentTestSettings.ValidTokenTypes.FirstOrDefault() ?? "";

    public string Audience { get; private set; } = TargetApiWebApplicationFactory.Audience;

    public string Issuer { get; private set; } = TargetApiWebApplicationFactory.Issuer;

    public SigningCredentials? SigningCredentials { get; private set; }
    public string? SignatureAlgorithm { get; private set; }
    public string? HmacShaSecret { get; private set; } = Guid.NewGuid().ToString() + Guid.NewGuid(); // Ensure this is long enough for 512-bit HMAC by default.

    public DateTime IssuedAt { get; private set; } = DateTime.UtcNow;
    public DateTime NotBefore { get; private set; } = DateTime.UtcNow.AddSeconds(-10);
    public DateTime Expires { get; private set; } = DateTime.UtcNow.AddMinutes(5);

    public ClaimsIdentity? Subject { get; private set; } = new([
        new Claim(JwtClaimTypes.Subject, TargetApiWebApplicationFactory.DefaultTestUser.SubjectId),
        new Claim(JwtClaimTypes.Name, TargetApiWebApplicationFactory.DefaultTestUser.Username),
        new Claim(JwtClaimTypes.PreferredUserName, TargetApiWebApplicationFactory.DefaultTestUser.Username)
    ]);

    public JwtBuilder WithTokenType(string tokenType)
    {
        TokenType = tokenType;
        return this;
    }

    public JwtBuilder WithAudience(string audience)
    {
        Audience = audience;
        return this;
    }

    public JwtBuilder WithIssuer(string issuer)
    {
        Issuer = issuer;
        return this;
    }

    public JwtBuilder WithSigningCredentials(SigningCredentials signingCredentials)
    {
        SigningCredentials = signingCredentials;
        SignatureAlgorithm = null;

        return this;
    }

    public JwtBuilder WithSignatureAlgorithm(string signatureAlgorithm)
    {
        SignatureAlgorithm = signatureAlgorithm;
        SigningCredentials = null;

        return this;
    }

    public JwtBuilder WithSignatureAlgorithm(string signatureAlgorithm, string hmacShaSecret)
    {
        SignatureAlgorithm = signatureAlgorithm;
        SigningCredentials = null;
        HmacShaSecret = hmacShaSecret;

        return this;
    }

    public JwtBuilder WithIssuedAt(DateTime issuedAt)
    {
        IssuedAt = issuedAt;
        return this;
    }

    public JwtBuilder WithNotBefore(DateTime notBefore)
    {
        NotBefore = notBefore;
        return this;
    }

    public JwtBuilder WithExpires(DateTime expires)
    {
        Expires = expires;
        return this;
    }

    public JwtBuilder WithSubject(ClaimsIdentity subject)
    {
        Subject = subject;
        return this;
    }

    public JwtBuilder WithSubject(TestUser subject)
    {
        Subject = new ClaimsIdentity([
            new Claim(JwtClaimTypes.Subject, subject.SubjectId),
            new Claim(JwtClaimTypes.Name, subject.Username),
            new Claim(JwtClaimTypes.PreferredUserName, subject.Username)
        ]);

        return this;
    }

    public JwtHeader BuildJwtHeader()
    {
        return new JwtHeader
        {
            [JwtHeaderParameterNames.Alg] = SignatureAlgorithm,
            [JwtHeaderParameterNames.Typ] = TokenType
        };
    }

    public JwtPayload BuildJwtPayload()
    {
        return new JwtPayload(Issuer, Audience, Subject?.Claims ?? [], NotBefore, Expires, IssuedAt);
    }

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