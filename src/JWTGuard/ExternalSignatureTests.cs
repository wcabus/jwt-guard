using System.Net.Http.Headers;
using System.Net;
using System.Text;

using Duende.IdentityServer.Configuration;

using Xunit;
using Microsoft.IdentityModel.Tokens;

namespace JWTGuard;

public class ExternalSignatureTests(TargetApiWebApplicationFactory factory) : JwtGuardTestBase(factory)
{
    [Fact(DisplayName = "When using an external JSON Web Key by specifying the 'jku' and 'kid' claims in the token, the API should return a 401 Unauthorized response.")]
    public async Task Accessing_AuthorizedUrl_Is_Unauthorized_For_External_WebKey_Using_jku_Claim()
    {
        // Arrange
        var jwt = GetJwt(ExternalSignatureTestCase.UseJkuAndKidClaims);
        Client!.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await Client.GetAsync(Factory.TargetUrl);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "When using an external JSON Web Key by specifying the 'jwk' claim in the token, the API should return a 401 Unauthorized response.")]
    public async Task Accessing_AuthorizedUrl_Is_Unauthorized_For_External_WebKey_Using_jwk_Claim()
    {
        // Arrange
        var jwt = GetJwt(ExternalSignatureTestCase.UseJwkClaim);
        Client!.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await Client.GetAsync(Factory.TargetUrl);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "When using an external certificate by specifying the 'x5u' claim in the token, the API should return a 401 Unauthorized response.")]
    public async Task Accessing_AuthorizedUrl_Is_Unauthorized_For_External_Certificate_Using_x5u_Claim()
    {
        // Arrange
        var jwt = GetJwt(ExternalSignatureTestCase.UseX5uClaim);
        Client!.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await Client.GetAsync(Factory.TargetUrl);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "When using an external certificate by specifying the 'x5c' claim in the token, the API should return a 401 Unauthorized response.")]
    public async Task Accessing_AuthorizedUrl_Is_Unauthorized_For_External_Certificate_Using_x5c_Claim()
    {
        // Arrange
        var jwt = GetJwt(ExternalSignatureTestCase.UseX5cClaim);
        Client!.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await Client.GetAsync(Factory.TargetUrl);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private string GetJwt(ExternalSignatureTestCase testCase)
    {
        // Use one of the supported signature algorithms that also supports using a certificate.
        var signatureAlgorithm = TestSettings.CurrentTestSettings.SupportedAlgorithms
            .Where(x => x.StartsWith("ES") || x.StartsWith("PS") || x.StartsWith("RS"))
            .MinBy(_ => Random.Shared.Next()); // Takes the first result at random
        
        if (signatureAlgorithm is null)
        {
            throw new InvalidOperationException("No supported signature algorithm found that supports using a certificate.");
        }

        var jwtBuilder = Factory.CreateJwtBuilder()
            .WithSignatureAlgorithm(signatureAlgorithm);

        var header = jwtBuilder.BuildJwtHeader();
        var payload = jwtBuilder.BuildJwtPayload();
        
        var encodedPayload = payload.Base64UrlEncode();
        
        var headerAndPayload = "";
        var signature = "";

        switch (testCase)
        {
            case ExternalSignatureTestCase.UseJwkClaim:
                {
                    SecurityKey? securityKey;

                    if (signatureAlgorithm.StartsWith("ES"))
                    {
                        var curve = signatureAlgorithm switch
                        {
                            SecurityAlgorithms.EcdsaSha256 => JsonWebKeyECTypes.P256,
                            SecurityAlgorithms.EcdsaSha384 => JsonWebKeyECTypes.P384,
                            SecurityAlgorithms.EcdsaSha512 => JsonWebKeyECTypes.P521,
                            _ => JsonWebKeyECTypes.P256
                        };

                        securityKey = CryptoHelper.CreateECDsaSecurityKey(curve);
                    }
                    else 
                    {
                        securityKey = CryptoHelper.CreateRsaSecurityKey();
                    }
                    
                    var jsonWebKey = JsonWebKeyConverter.ConvertFromSecurityKey(securityKey);
                    jsonWebKey.Alg = signatureAlgorithm;
                    jsonWebKey.Use = "sig";

                    header["jwk"] = jsonWebKey.ToDictionary();
                    header["kid"] = jsonWebKey.KeyId;

                    headerAndPayload = header.Base64UrlEncode() + "." + encodedPayload;
        
                    var asciiBytes = Encoding.ASCII.GetBytes(headerAndPayload);
                    var signatureProvider = CryptoProviderFactory.Default.CreateForSigning(securityKey, signatureAlgorithm);
                    try
                    {
                        var signatureBytes = signatureProvider.Sign(asciiBytes);
                        signature = Base64UrlEncoder.Encode(signatureBytes);
                    }
                    finally
                    {
                        CryptoProviderFactory.Default.ReleaseSignatureProvider(signatureProvider);
                    }
                }
                break;
        }

        return headerAndPayload + "." + signature;
    }

    private enum ExternalSignatureTestCase
    {
        UseJkuAndKidClaims,
        UseJwkClaim,
        UseX5uClaim,
        UseX5cClaim
    }
}