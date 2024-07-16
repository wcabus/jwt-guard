using Xunit;

namespace JWTGuard;

[CollectionDefinition(CollectionName)]
public class JwtGuardTestCollection : ICollectionFixture<TargetApiWebApplicationFactory>
{
    public const string CollectionName = "JWTGuard Tests";
}