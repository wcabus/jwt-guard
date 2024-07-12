using Microsoft.IdentityModel.Tokens;

namespace JWTGuard;

public interface ISigningCredentialsProvider
{
    Task<SigningCredentials> GetSigningCredentialsAsync(string algorithm);
}