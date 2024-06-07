using System.Security.Claims;

using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Test;

using IdentityModel;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace JWTGuard;

public class TargetApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private WebApplication? _duendeHost;
    private CancellationTokenSource? _duendeHostCancellationSource;

    private readonly JsonWebTokenHandler _tokenHandler = new();

    public const string Audience = "api";
    public const string Issuer = "https://localhost:5901";

    private static readonly TestUser Alice = new()
    {
        SubjectId = "1",
        Username = "alice",
        Password = "password",
    };

    private static readonly HttpClient HttpClient = new();

    public TargetApiWebApplicationFactory()
    {
        CreateAndRunIdentityProvider();
    }

    public string TargetUrl => "/weatherforecast";

    public async Task<string> GetJwtAsync(string tokenType)
    {
        var signingCredentials = await GetSigningCredentialsAsync();

        var tokenPayload = new SecurityTokenDescriptor
        {
            TokenType = tokenType,
            Audience = GetExpectedAudience(),
            Issuer = GetExpectedIssuer(),
            SigningCredentials = signingCredentials,
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow.AddSeconds(-10),
            Expires = DateTime.UtcNow.AddMinutes(5),
            Subject = new ClaimsIdentity([
                new Claim(JwtClaimTypes.Subject, Alice.SubjectId),
                new Claim(JwtClaimTypes.Name, Alice.Username),
                new Claim(JwtClaimTypes.PreferredUserName, Alice.Username)
            ])
        };

        return _tokenHandler.CreateToken(tokenPayload);
    }

    private async Task<SigningCredentials> GetSigningCredentialsAsync()
    {
        if (_duendeHost is null)
        {
            throw new InvalidOperationException("The test identity provider is not running!");
        }

        // Generate key material by requesting the discovery document.
        await HttpClient.GetAsync($"{Issuer}/.well-known/openid-configuration");

        using var scope = _duendeHost.Services.CreateScope();
        var keyMaterialService = scope.ServiceProvider.GetRequiredService<IKeyMaterialService>();

        return await keyMaterialService.GetSigningCredentialsAsync([SecurityAlgorithms.RsaSsaPssSha256]);
    }

    private string GetExpectedAudience() => Audience;
    private string GetExpectedIssuer() => Issuer;

    override protected void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Reconfigure (only) the JWT bearer options to use the test identity provider instance.
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = Issuer;
                options.Audience = Audience;
            });
        });
    }

    private void CreateAndRunIdentityProvider()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls(Issuer);

        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();
        builder.Services.AddHttpContextAccessor();

        builder.Services
            .AddIdentityServer(options =>
            {
                options.KeyManagement.SigningAlgorithms = [ 
                    new SigningAlgorithmOptions(SecurityAlgorithms.RsaSsaPssSha256),
                    new SigningAlgorithmOptions(SecurityAlgorithms.RsaSha256),
                    new SigningAlgorithmOptions(SecurityAlgorithms.EcdsaSha256)
                ];
            })
            .AddInMemoryApiScopes([
                new ApiScope(Audience)
            ])
            .AddInMemoryApiResources([
                new ApiResource(Audience) { Scopes = { Audience } }

            ])
            .AddInMemoryIdentityResources([
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResources.Address(),
                new IdentityResources.Phone()
            ])
            .AddInMemoryClients(ConfigureClients())
            .AddInMemoryPersistedGrants()
            .AddTestUsers([Alice])
            .AddKeyManagement();
            //.AddDeveloperSigningCredential(true, "ps256.key", IdentityServerConstants.RsaSigningAlgorithm.PS256)
            //.AddDeveloperSigningCredential(true, "rs256.key", IdentityServerConstants.RsaSigningAlgorithm.RS256);

        _duendeHost = builder.Build();

        _duendeHost.UseStaticFiles();
        _duendeHost.UseRouting();
        _duendeHost.UseIdentityServer();
        _duendeHost.UseAuthorization();

        _duendeHostCancellationSource = new CancellationTokenSource();
        _duendeHost.RunAsync(_duendeHostCancellationSource.Token);
    }

    private static IEnumerable<Client> ConfigureClients()
    {
        yield return new Client
        {
            ClientId = "m2m",
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientSecrets = { new Secret("secret".Sha256()) },
            AllowedScopes = { Audience }
        };

        yield return new Client
        {
            ClientId = "interactive.confidential",
            AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
            RequirePkce = true,
            ClientSecrets = { new Secret("secret".Sha256()) },
            AllowedScopes = { Audience }
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _duendeHostCancellationSource?.Cancel();
        }

        base.Dispose(disposing);
    }
}