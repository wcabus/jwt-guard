using System.Net;
using System.Net.Http.Headers;

using JWTGuard.Helpers;

using Xunit;

namespace JWTGuard.Tests;

public class IssuerTests(TargetApiWebApplicationFactory factory) : JwtGuardTestBase(factory)
{
    [Theory(DisplayName = "When a token uses allowed values for the issuer claim, the API should not return a 401 Unauthorized response.")]
    [MemberData(nameof(GetAllowedIssuers))]
    public async Task Accessing_AuthorizedUrl_Is_Authorized_For_Allowed_Issuer(string? issuer)
    {
        if (issuer is null)
        {
            Assert.Null(issuer);
            return;
        }

        // Arrange
        var jwt = await GetJwtAsync(issuer);
        Client!.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await Client.GetAsync(TestSettings.CurrentTestSettings.TargetUrl);

        // Assert
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory(DisplayName = "When a token uses disallowed values for the issuer claim, the API should return a 401 Unauthorized response.")]
    [MemberData(nameof(GetDisallowedIssuers))]
    public async Task Accessing_AuthorizedUrl_Is_Unauthorized_For_Disallowed_Issuers(string? issuer)
    {
        if (issuer is null)
        {
            Assert.Null(issuer);
            return;
        }

        // Arrange
        var jwt = await GetJwtAsync(issuer);
        Client!.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await Client.GetAsync(TestSettings.CurrentTestSettings.TargetUrl);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private Task<string> GetJwtAsync(string issuer)
    {
        return Factory.CreateJwtBuilder()
            .WithIssuer(issuer)
            .BuildAsync();
    }

    public static TheoryData<string?> GetAllowedIssuers()
    {
        return TestSettings.CurrentTestSettings.AllowedIssuers.Count == 0
            ? new TheoryData<string?>([null])
            : new TheoryData<string?>(TestSettings.CurrentTestSettings.AllowedIssuers);
    }

    public static TheoryData<string?> GetDisallowedIssuers()
    {
        return TestSettings.CurrentTestSettings.DisallowedIssuers.Count == 0
            ? new TheoryData<string?>([null])
            : new TheoryData<string?>(TestSettings.CurrentTestSettings.DisallowedIssuers);
    }
}