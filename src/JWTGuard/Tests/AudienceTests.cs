using System.Net;
using System.Net.Http.Headers;

using JWTGuard.Helpers;

using Xunit;

namespace JWTGuard.Tests;

/// <summary>
/// Test class to test the audience ("aud") claim in JWTs.
/// </summary>
public class AudienceTests(TargetApiWebApplicationFactory factory) : JwtGuardTestBase(factory)
{
    [Theory(DisplayName = "When a token uses allowed values for the audience claim, the API should not return a 401 Unauthorized response.")]
    [MemberData(nameof(GetAllowedAudiences))]
    internal async Task Accessing_AuthorizedUrl_Is_Authorized_For_Allowed_Audiences(string? audience)
    {
        if (audience is null)
        {
            Assert.Null(audience);
            return;
        }

        // Arrange
        var jwt = await GetJwtAsync(audience);
        Client!.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await Client.GetAsync(TestSettings.CurrentTestSettings.TargetUrl);

        // Assert
        TestSettings.CurrentTestSettings.AssertAuthorizedResponse(response);
    }

    [Theory(DisplayName = "When a token uses disallowed values for the audience claim, the API should return a 401 Unauthorized response.")]
    [MemberData(nameof(GetDisallowedAudiences))]
    internal async Task Accessing_AuthorizedUrl_Is_Unauthorized_For_Disallowed_Audiences(string? audience)
    {
        if (audience is null)
        {
            Assert.Null(audience);
            return;
        }

        // Arrange
        var jwt = await GetJwtAsync(audience);
        Client!.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await Client.GetAsync(TestSettings.CurrentTestSettings.TargetUrl);

        // Assert
        TestSettings.CurrentTestSettings.AssertUnauthorizedResponse(response);
    }

    private Task<string> GetJwtAsync(string audience)
    {
        return Factory.CreateJwtBuilder()
            .WithAudience(audience)
            .BuildAsync();
    }

    /// <summary>
    /// Retrieves the allowed audiences for our test theories.
    /// </summary>
    public static TheoryData<string?> GetAllowedAudiences()
    {
        return TestSettings.CurrentTestSettings.AllowedAudiences.Count == 0
            ? new TheoryData<string?>([null])
            : new TheoryData<string?>(TestSettings.CurrentTestSettings.AllowedAudiences);
    }

    /// <summary>
    /// Retrieves the disallowed audiences for our test theories.
    /// </summary>
    public static TheoryData<string?> GetDisallowedAudiences()
    {
        return TestSettings.CurrentTestSettings.DisallowedAudiences.Count == 0
            ? new TheoryData<string?>([null])
            : new TheoryData<string?>(TestSettings.CurrentTestSettings.DisallowedAudiences);
    }
}