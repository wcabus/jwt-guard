using Microsoft.IdentityModel.Tokens;

namespace JWTGuard.Helpers;

public interface ISigningCredentialsProvider
{
    Task<SigningCredentials> GetSigningCredentialsAsync(string algorithm);
}