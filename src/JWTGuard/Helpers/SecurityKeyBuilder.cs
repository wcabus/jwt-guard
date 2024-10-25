using Duende.IdentityServer.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JWTGuard.Helpers;

/// <summary>
/// Utility class to build <see cref="SecurityKey"/>s.
/// </summary>
public static class SecurityKeyBuilder
{
    /// <summary>
    /// Create a new <see cref="SecurityKey"/> for the specified <paramref name="signatureAlgorithm"/>.
    /// </summary>
    /// <param name="signatureAlgorithm">A valid signature algorithm. See <see cref="SecurityAlgorithms"/>.</param>
    public static SecurityKey CreateSecurityKey(string signatureAlgorithm)
    {
        if (!signatureAlgorithm.StartsWith("ES"))
        {
            return CryptoHelper.CreateRsaSecurityKey();
        }

        var curve = signatureAlgorithm switch
        {
            SecurityAlgorithms.EcdsaSha256 => JsonWebKeyECTypes.P256,
            SecurityAlgorithms.EcdsaSha384 => JsonWebKeyECTypes.P384,
            SecurityAlgorithms.EcdsaSha512 => JsonWebKeyECTypes.P521,
            _ => JsonWebKeyECTypes.P256
        };

        return CryptoHelper.CreateECDsaSecurityKey(curve);
    }

    /// <summary>
    /// Returns the public key data for the given <paramref name="securityKey"/>.
    /// </summary>
    /// <param name="securityKey">A <see cref="SecurityKey"/></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException">Thrown if this method is being called for a security key which is not an instance of either <see cref="RsaSecurityKey"/> or <see cref="ECDsaSecurityKey"/>.</exception>
    public static string GetCertificatePublicKeyPem(SecurityKey securityKey)
    {
        switch (securityKey)
        {
            case RsaSecurityKey rsaSecurityKey:
                {
                    return rsaSecurityKey.Rsa.ExportRSAPublicKeyPem();
                }
            case ECDsaSecurityKey ecdsaSecurityKey:
                {
                    return ecdsaSecurityKey.ECDsa.ExportSubjectPublicKeyInfoPem();
                }
            default:
                throw new NotImplementedException();
        }
    }
}