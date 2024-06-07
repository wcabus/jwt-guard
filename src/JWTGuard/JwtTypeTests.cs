using System.Net;
using System.Net.Http.Headers;

using Xunit;

namespace JWTGuard;

public class JwtTypeTests(TargetApiWebApplicationFactory factory) : IClassFixture<TargetApiWebApplicationFactory>
{
    [Theory]
    [MemberData(nameof(GetValidJwtTypes))]
    public async Task Accessing_AuthorizedUrl_Is_Authorized_For_Valid_JWT_Types(string tokenType)
    {
        // Arrange
        var client = factory.CreateClient();
        var jwt = await factory.GetJwtAsync(tokenType);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await client.GetAsync(factory.TargetUrl);

        // Assert
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetInvalidJwtTypes))]
    public async Task Accessing_AuthorizedUrl_Is_Unauthorized_For_Invalid_JWT_Types(string tokenType)
    {
        // Arrange
        var client = factory.CreateClient();
        var jwt = await factory.GetJwtAsync(tokenType);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await client.GetAsync(factory.TargetUrl);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public static IEnumerable<object[]> GetValidJwtTypes()
    {
        if (TestSettings.CurrentTestSettings.ValidTokenTypes.Count == 0)
        {
            yield break;
        }

        foreach (string tokenType in TestSettings.CurrentTestSettings.ValidTokenTypes)
        {
            yield return [tokenType];
        }
    }

    public static IEnumerable<object[]> GetInvalidJwtTypes()
    {
        if (TestSettings.CurrentTestSettings.InvalidTokenTypes.Count == 0)
        {
            yield break;
        }

        foreach (string tokenType in TestSettings.CurrentTestSettings.InvalidTokenTypes)
        {
            yield return [tokenType];
        }
    }
}