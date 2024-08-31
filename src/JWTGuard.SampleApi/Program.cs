using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

const string authority = "https://demo.duendesoftware.com";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = authority;
        options.Audience = "api";

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

            //// This code would allow specifying a JsonWebKey as part of the token's header
            //IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            //{
            //    // retrieve signing key from token.
            //    var header = JwtHeader.Base64UrlDeserialize(token.Split('.')[0]);
            //    var jwk = header["jwk"];
            //    return [ JsonWebKey.Create(jwk.ToString()) ];
            //},

            //// This code would allow specifying an external JsonWebKey by specifying a URL containing the keys as part of the token's header
            //IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            //{
            //    // retrieve signing key from external url.
            //    var header = JwtHeader.Base64UrlDeserialize(token.Split('.')[0]);
            //    var url = header["jku"] as string;
            //    var client = new HttpClient();
            //    var jwks = client.GetStringAsync(url).GetAwaiter().GetResult();
            //    return JsonWebKeySet.Create(jwks).Keys;
            //},

            //// This code would allow specifying a certificate as part of the token's header
            //IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            //{
            //    // retrieve certificate from token
            //    var header = JwtHeader.Base64UrlDeserialize(token.Split('.')[0]);
            //    var certs = header["x5c"] as List<object>;
            //    var certPem = certs![0] as string;

            //    SecurityKey? securityKey;
            //    if (certPem!.Contains("RSA PUBLIC", StringComparison.Ordinal))
            //    {
            //        var rsaSecurityKey = new RsaSecurityKey(RSA.Create());
            //        rsaSecurityKey.Rsa.ImportFromPem(certPem);

            //        securityKey = rsaSecurityKey;
            //    }
            //    else
            //    {
            //        var ecdsaSecurityKey = new ECDsaSecurityKey(ECDsa.Create());
            //        ecdsaSecurityKey.ECDsa.ImportFromPem(certPem);

            //        securityKey = ecdsaSecurityKey;
            //    }

            //    return [JsonWebKeyConverter.ConvertFromSecurityKey(securityKey)];
            //},

            //// This code would allow specifying an external certificate by specifying a URL containing the keys as part of the token's header
            //IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            //{
            //    // retrieve certificate from external url.
            //    var header = JwtHeader.Base64UrlDeserialize(token.Split('.')[0]);
            //    var url = header["x5u"] as string;
            //    var client = new HttpClient();
            //    var certPem = client.GetStringAsync(url).GetAwaiter().GetResult();

            //    SecurityKey? securityKey;
            //    if (certPem!.Contains("RSA PUBLIC", StringComparison.Ordinal))
            //    {
            //        var rsaSecurityKey = new RsaSecurityKey(RSA.Create());
            //        rsaSecurityKey.Rsa.ImportFromPem(certPem);

            //        securityKey = rsaSecurityKey;
            //    }
            //    else
            //    {
            //        var ecdsaSecurityKey = new ECDsaSecurityKey(ECDsa.Create());
            //        ecdsaSecurityKey.ECDsa.ImportFromPem(certPem);

            //        securityKey = ecdsaSecurityKey;
            //    }

            //    return [JsonWebKeyConverter.ConvertFromSecurityKey(securityKey)];
            //},

            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
.WithOpenApi()
.RequireAuthorization();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public partial class Program {}