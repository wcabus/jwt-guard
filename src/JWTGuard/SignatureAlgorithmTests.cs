using System.Net;
using System.Net.Http.Headers;

using Xunit;

namespace JWTGuard;

public class SignatureAlgorithmTests(TargetApiWebApplicationFactory factory) : JwtGuardTestBase(factory)
{
    [Theory]
    [MemberData(nameof(GetSupportedAlgorithms))]
    public async Task Accessing_AuthorizedUrl_Is_Authorized_For_Supported_Signature_Algorithms(string signatureAlgorithm)
    {
        // Arrange
        var jwt = await GetJwtAsync(signatureAlgorithm);
        Client!.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await Client.GetAsync(Factory.TargetUrl);

        // Assert
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetUnsupportedAlgorithms))]
    public async Task Accessing_AuthorizedUrl_Is_Unauthorized_For_Unsupported_Signature_Algorithms(string signatureAlgorithm)
    {
        // Arrange
        var jwt = await GetJwtAsync(signatureAlgorithm);
        Client!.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await Client.GetAsync(Factory.TargetUrl);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private Task<string> GetJwtAsync(string signatureAlgorithm)
    {
        return Factory.CreateJwtBuilder()
            .WithSignatureAlgorithm(signatureAlgorithm)
            .BuildAsync();
    }

    public static IEnumerable<object[]> GetSupportedAlgorithms()
    {
        if (TestSettings.CurrentTestSettings.SupportedAlgorithms.Count == 0)
        {
            yield break;
        }

        foreach (string signatureAlgorithm in TestSettings.CurrentTestSettings.SupportedAlgorithms)
        {
            yield return [signatureAlgorithm];
        }
    }

    public static IEnumerable<object[]> GetUnsupportedAlgorithms()
    {
        if (TestSettings.CurrentTestSettings.UnsupportedAlgorithms.Count == 0)
        {
            yield break;
        }

        foreach (string signatureAlgorithm in TestSettings.CurrentTestSettings.UnsupportedAlgorithms)
        {
            yield return [signatureAlgorithm];
        }
    }
}