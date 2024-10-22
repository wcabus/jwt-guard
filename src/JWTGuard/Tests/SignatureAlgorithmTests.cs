using System.Net;
using System.Net.Http.Headers;

using JWTGuard.Helpers;

using Xunit;

namespace JWTGuard.Tests;

public class SignatureAlgorithmTests(TargetApiWebApplicationFactory factory) : JwtGuardTestBase(factory)
{
    [Theory(DisplayName = "When a token uses a supported signature algorithm, the API should not return a 401 Unauthorized response.")]
    [MemberData(nameof(GetSupportedAlgorithms))]
    public async Task Accessing_AuthorizedUrl_Is_Authorized_For_Supported_Signature_Algorithms(string? signatureAlgorithm)
    {
        if (signatureAlgorithm is null)
        {
            Assert.Null(signatureAlgorithm);
            return;
        }

        // Arrange
        var jwt = await GetJwtAsync(signatureAlgorithm);
        Client!.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await Client.GetAsync(TestSettings.CurrentTestSettings.TargetUrl);

        // Assert
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory(DisplayName = "When a token uses an unsupported signature algorithm, the API should return a 401 Unauthorized response.")]
    [MemberData(nameof(GetUnsupportedAlgorithms))]
    public async Task Accessing_AuthorizedUrl_Is_Unauthorized_For_Unsupported_Signature_Algorithms(string? signatureAlgorithm)
    {
        if (signatureAlgorithm is null)
        {
            Assert.Null(signatureAlgorithm);
            return;
        }

        // Arrange
        var jwt = await GetJwtAsync(signatureAlgorithm);
        Client!.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await Client.GetAsync(TestSettings.CurrentTestSettings.TargetUrl);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private Task<string> GetJwtAsync(string signatureAlgorithm)
    {
        return Factory.CreateJwtBuilder()
            .WithSignatureAlgorithm(signatureAlgorithm)
            .BuildAsync();
    }

    public static TheoryData<string?> GetSupportedAlgorithms()
    {
        return TestSettings.CurrentTestSettings.SupportedAlgorithms.Count == 0
            ? new TheoryData<string?>([null])
            : new TheoryData<string?>(TestSettings.CurrentTestSettings.SupportedAlgorithms);
    }

    public static TheoryData<string?> GetUnsupportedAlgorithms()
    {
        return TestSettings.CurrentTestSettings.UnsupportedAlgorithms.Count == 0
            ? new TheoryData<string?>([null])
            : new TheoryData<string?>(TestSettings.CurrentTestSettings.UnsupportedAlgorithms);
    }
}