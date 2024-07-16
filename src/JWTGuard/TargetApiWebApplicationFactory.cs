using System.Diagnostics;

using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Test;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace JWTGuard;

public class TargetApiWebApplicationFactory : WebApplicationFactory<Program>, ISigningCredentialsProvider
{
    private WebApplication? _duendeHost;
    private CancellationTokenSource? _duendeHostCancellationSource;

    private readonly JsonWebTokenHandler _tokenHandler = new();

    public const string Audience = "api";
    public const string Issuer = "https://localhost:5901";

    public static readonly TestUser DefaultTestUser = new()
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

    public JwtBuilder CreateJwtBuilder() => new(this, _tokenHandler);

    async Task<SigningCredentials> ISigningCredentialsProvider.GetSigningCredentialsAsync(string algorithm)
    {
        if (_duendeHost is null)
        {
            throw new InvalidOperationException("The test identity provider is not running!");
        }

        // Generate key material by requesting the discovery document.
        await HttpClient.GetAsync($"{Issuer}/.well-known/openid-configuration");

        using var scope = _duendeHost.Services.CreateScope();
        var keyMaterialService = scope.ServiceProvider.GetRequiredService<IKeyMaterialService>();

        return await keyMaterialService.GetSigningCredentialsAsync([algorithm]);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
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
                options.KeyManagement.SigningAlgorithms = TestSettings.DuendeSupportedSecurityAlgorithms
                    .Select(alg => new SigningAlgorithmOptions(alg))
                    .ToArray();
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
            .AddTestUsers([DefaultTestUser])
            .AddKeyManagement();

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
            _duendeHost?.DisposeAsync().GetAwaiter().GetResult();
        }

        base.Dispose(disposing);
    }
}