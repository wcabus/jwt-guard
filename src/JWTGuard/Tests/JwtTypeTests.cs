﻿using System.Net;
using System.Net.Http.Headers;

using JWTGuard.Helpers;

using Xunit;

namespace JWTGuard.Tests;

public class JwtTypeTests(TargetApiWebApplicationFactory factory) : JwtGuardTestBase(factory)
{
    [Theory(DisplayName = "When a token uses an expected token type, the API should not return a 401 Unauthorized response.")]
    [MemberData(nameof(GetValidJwtTypes))]
    public async Task Accessing_AuthorizedUrl_Is_Authorized_For_Valid_JWT_Types(string? tokenType)
    {
        if (tokenType is null)
        {
            Assert.Null(tokenType);
            return;
        }

        // Arrange
        var jwt = await GetJwtAsync(tokenType);
        Client!.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await Client.GetAsync(Factory.TargetUrl);

        // Assert
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory(DisplayName = "When a token uses an unexpected token type, the API should return a 401 Unauthorized response.")]
    [MemberData(nameof(GetInvalidJwtTypes))]
    public async Task Accessing_AuthorizedUrl_Is_Unauthorized_For_Invalid_JWT_Types(string? tokenType)
    {
        if (tokenType is null)
        {
            Assert.Null(tokenType);
            return;
        }

        // Arrange
        var jwt = await GetJwtAsync(tokenType);
        Client!.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await Client.GetAsync(Factory.TargetUrl);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private Task<string> GetJwtAsync(string tokenType)
    {
        return Factory.CreateJwtBuilder()
            .WithTokenType(tokenType)
            .BuildAsync();
    }

    public static TheoryData<string?> GetValidJwtTypes()
    {
        return TestSettings.CurrentTestSettings.ValidTokenTypes.Count == 0
            ? new TheoryData<string?>([null])
            : new TheoryData<string?>(TestSettings.CurrentTestSettings.ValidTokenTypes);
    }

    public static TheoryData<string?> GetInvalidJwtTypes()
    {
        return TestSettings.CurrentTestSettings.InvalidTokenTypes.Count == 0 
            ? new TheoryData<string?>([null]) 
            : new TheoryData<string?>(TestSettings.CurrentTestSettings.InvalidTokenTypes);
    }
}