using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

const string authority = "https://demo.duendesoftware.com";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = authority;
        options.Audience = "api";

        IEnumerable<SecurityKey> AllowSpecialCases(string token, SecurityToken securityToken, string kid, TokenValidationParameters parameters)
        {
            var header = JwtHeader.Base64UrlDeserialize(token.Split('.')[0]);
            
            if (header.TryGetValue("jwk", out object? jwk))
            {
                // This code would allow specifying a JsonWebKey as part of the token's header
                return [JsonWebKey.Create(jwk.ToString())];
            }

            if (header.TryGetValue("jku", out object? jku))
            {
                // This code would allow specifying an external JsonWebKey by specifying a URL containing the keys as part of the token's header
                var client = new HttpClient();
                var jwks = client.GetStringAsync(jku.ToString()).GetAwaiter().GetResult();
                return JsonWebKeySet.Create(jwks).Keys;
            }

            if (header.TryGetValue("x5c", out object? x5c))
            {
                // This code would allow specifying a certificate as part of the token's header
                var certPem = (x5c as List<object>)![0] as string;

                SecurityKey? securityKey;
                if (certPem.Contains("RSA PUBLIC", StringComparison.Ordinal))
                {
                    var rsaSecurityKey = new RsaSecurityKey(RSA.Create());
                    rsaSecurityKey.Rsa.ImportFromPem(certPem);

                    securityKey = rsaSecurityKey;
                }
                else
                {
                    var ecdsaSecurityKey = new ECDsaSecurityKey(ECDsa.Create());
                    ecdsaSecurityKey.ECDsa.ImportFromPem(certPem);

                    securityKey = ecdsaSecurityKey;
                }

                return [JsonWebKeyConverter.ConvertFromSecurityKey(securityKey)];
            }

            if (header.TryGetValue("x5u", out object? x5u))
            {
                // This code would allow specifying an external certificate by specifying a URL containing the keys as part of the token's header
                var client = new HttpClient();
                var certPem = client.GetStringAsync(x5u.ToString()).GetAwaiter().GetResult();

                SecurityKey? securityKey;
                if (certPem.Contains("RSA PUBLIC", StringComparison.Ordinal))
                {
                    var rsaSecurityKey = new RsaSecurityKey(RSA.Create());
                    rsaSecurityKey.Rsa.ImportFromPem(certPem);

                    securityKey = rsaSecurityKey;
                }
                else
                {
                    var ecdsaSecurityKey = new ECDsaSecurityKey(ECDsa.Create());
                    ecdsaSecurityKey.ECDsa.ImportFromPem(certPem);

                    securityKey = ecdsaSecurityKey;
                }

                return [JsonWebKeyConverter.ConvertFromSecurityKey(securityKey)];
            }

            return Array.Empty<SecurityKey>();
        }

        options.TokenValidationParameters = new()
        {
            ValidAlgorithms = [
                SecurityAlgorithms.EcdsaSha256, SecurityAlgorithms.EcdsaSha384, SecurityAlgorithms.EcdsaSha512,
                SecurityAlgorithms.RsaSsaPssSha256, SecurityAlgorithms.RsaSsaPssSha384, SecurityAlgorithms.RsaSsaPssSha512
            ],
            ValidIssuer = authority,
            ValidTypes = ["at+jwt"],

            RequireAudience = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,

            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,

            // Uncomment the following line to enable special signing key resolvers
            // IssuerSigningKeyResolver = AllowSpecialCases,

            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.RequireAuthorization();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public partial class Program {}