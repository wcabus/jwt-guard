using Duende.IdentityServer.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JWTGuard.Helpers;

public static class SecurityKeyBuilder
{
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