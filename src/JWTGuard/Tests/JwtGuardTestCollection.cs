using JWTGuard.Helpers;

using Xunit;

namespace JWTGuard.Tests;

[CollectionDefinition(CollectionName)]
public class JwtGuardTestCollection : ICollectionFixture<TargetApiWebApplicationFactory>
{
    public const string CollectionName = "JWTGuard Tests";
}