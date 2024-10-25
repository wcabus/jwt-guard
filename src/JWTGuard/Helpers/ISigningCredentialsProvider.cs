using Microsoft.IdentityModel.Tokens;

namespace JWTGuard.Helpers;

/// <summary>
/// Interface to provide access to <see cref="SigningCredentials"/>.
/// </summary>
public interface ISigningCredentialsProvider
{
    /// <summary>
    /// Retrieves <see cref="SigningCredentials"/> asynchronously for the specified <paramref name="algorithm"/>.
    /// </summary>
    /// <param name="algorithm">A signature algorithm.</param>
    Task<SigningCredentials> GetSigningCredentialsAsync(string algorithm);
}