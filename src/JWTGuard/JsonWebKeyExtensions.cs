using Microsoft.IdentityModel.Tokens;

namespace JWTGuard;

internal static class JsonWebKeyExtensions
{
    public static IDictionary<string, object?> ToDictionary(this JsonWebKey jwk)
    {
        var dictionary = new Dictionary<string, object?>();

        if (!string.IsNullOrEmpty(jwk.Alg))
        {
            dictionary.Add("alg", jwk.Alg);
        }

        if (!string.IsNullOrEmpty(jwk.Crv))
        {
            dictionary.Add("crv", jwk.Crv);
        }

        if (!string.IsNullOrEmpty(jwk.D))
        {
            dictionary.Add("d", jwk.D);
        }

        if (!string.IsNullOrEmpty(jwk.DP))
        {
            dictionary.Add("dp", jwk.DP);
        }

        if (!string.IsNullOrEmpty(jwk.DQ))
        {
            dictionary.Add("dq", jwk.DQ);
        }

        if (!string.IsNullOrEmpty(jwk.E))
        {
            dictionary.Add("e", jwk.E);
        }

        if (!string.IsNullOrEmpty(jwk.K))
        {
            dictionary.Add("k", jwk.K);
        }

        if (jwk.KeyOps.Count > 0)
        {
            dictionary.Add("key_ops", jwk.KeyOps);
        }

        if (!string.IsNullOrEmpty(jwk.Kid))
        {
            dictionary.Add("kid", jwk.Kid);
        }

        if (!string.IsNullOrEmpty(jwk.Kty))
        {
            dictionary.Add("kty", jwk.Kty);
        }

        if (!string.IsNullOrEmpty(jwk.N))
        {
            dictionary.Add("n", jwk.N);
        }

        if (jwk.Oth.Count > 0)
        {
            dictionary.Add("oth", jwk.Oth);
        }

        if (!string.IsNullOrEmpty(jwk.P))
        {
            dictionary.Add("p", jwk.P);
        }

        if (!string.IsNullOrEmpty(jwk.Q))
        {
            dictionary.Add("q", jwk.Q);
        }

        if (!string.IsNullOrEmpty(jwk.QI))
        {
            dictionary.Add("qi", jwk.QI);
        }

        if (!string.IsNullOrEmpty(jwk.Use))
        {
            dictionary.Add("use", jwk.Use);
        }

        if (!string.IsNullOrEmpty(jwk.X))
        {
            dictionary.Add("x", jwk.X);
        }

        if (jwk.X5c.Count > 0)
        {
            dictionary.Add("x5c", jwk.X5c);
        }

        if (!string.IsNullOrEmpty(jwk.X5t))
        {
            dictionary.Add("x5t", jwk.X5t);
        }

        if (!string.IsNullOrEmpty(jwk.X5tS256))
        {
            dictionary.Add("x5t#S256", jwk.X5tS256);
        }

        if (!string.IsNullOrEmpty(jwk.X5u))
        {
            dictionary.Add("x5u", jwk.X5u);
        }

        if (!string.IsNullOrEmpty(jwk.Y))
        {
            dictionary.Add("y", jwk.Y);
        }

        if (jwk.AdditionalData.Count > 0)
        {
            foreach (KeyValuePair<string, object> keyValuePair in jwk.AdditionalData)
            {
                dictionary.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        return dictionary;
    }
}