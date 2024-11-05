using System.Net;

using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace JWTGuard.Helpers;

/// <summary>
/// Factory for bootstrapping your target Web API so that JWT Guard can run integration tests against it.
/// </summary>
/// <remarks>
/// If this class fails to compile, check that your Web API project has a Program class defined and has a reference to Microsoft.AspNetCore.Authentication.JwtBearer. <br/>
/// <br/>
/// If you're using top-level statements in your project, add this code at the bottom of your Program.cs file: <br/>
/// <code>public partial class Program {}</code>
/// </remarks>
public class TargetApiWebApplicationFactory : WebApplicationFactory<Program>, ISigningCredentialsProvider
{
    private WebApplication? _duendeHost;
    private CancellationTokenSource? _duendeHostCancellationSource;

    private readonly JsonWebTokenHandler _tokenHandler = new();

    private static readonly Dictionary<string, (string KeyId, SecurityKey SecurityKey)> GeneratedSecurityKeys = new();

    private static readonly HttpClient HttpClient = new();

    /// <summary>
    /// Constructor of the factory class.
    /// </summary>
    public TargetApiWebApplicationFactory()
    {
        CreateAndRunIdentityProvider();
    }

    /// <summary>
    /// Creates a new instance of <see cref="JwtBuilder"/> and returns it.
    /// </summary>
    public JwtBuilder CreateJwtBuilder() => new(this, _tokenHandler);

    /// <summary>
    /// Returns the <see cref="SigningCredentials"/> from the local issuer instance.
    /// </summary>
    async Task<SigningCredentials> ISigningCredentialsProvider.GetSigningCredentialsAsync(string algorithm)
    {
        if (_duendeHost is null)
        {
            throw new InvalidOperationException("The test identity provider is not running!");
        }

        // Generate key material by requesting the discovery document.
        await HttpClient.GetAsync($"{TestSettings.CurrentTestSettings.DefaultIssuer}/.well-known/openid-configuration");

        using var scope = _duendeHost.Services.CreateScope();
        var keyMaterialService = scope.ServiceProvider.GetRequiredService<IKeyMaterialService>();

        return await keyMaterialService.GetSigningCredentialsAsync([algorithm]);
    }

    /// <summary>
    /// Configures the target Web API's host.
    /// </summary>
    /// <remarks>
    /// Some of the authentication settings need to be overwritten for JWT Guard to function correctly.<br/>
    /// Other settings, like the supported signature algorithms or expected token types, remain unchanged.
    /// </remarks>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Reconfigure (only) the JWT bearer options to use the test identity provider instance.
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = TestSettings.CurrentTestSettings.DefaultIssuer;
                options.TokenValidationParameters.ValidIssuer = TestSettings.CurrentTestSettings.DefaultIssuer;
            });
        });
    }

    /// <summary>
    /// Creates and runs a local identity provider instance.
    /// </summary>
    /// <remarks>
    /// During the test runs, JWT Guard spins up its own issuer using Duende IdentityServer. This allows JWT Guard to have full control over the issued tokens in order to influence them during the tests.
    /// </remarks>
    private void CreateAndRunIdentityProvider()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls(TestSettings.CurrentTestSettings.DefaultIssuer);

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
                new ApiScope(TestSettings.CurrentTestSettings.DefaultAudience)
            ])
            .AddInMemoryApiResources([
                new ApiResource(TestSettings.CurrentTestSettings.DefaultAudience) { Scopes = { TestSettings.CurrentTestSettings.DefaultAudience } }

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
            .AddTestUsers([TestSettings.CurrentTestSettings.DefaultTestUser])
            .AddKeyManagement();

        _duendeHost = builder.Build();

        _duendeHost.UseStaticFiles();
        _duendeHost.UseRouting();
        _duendeHost.UseIdentityServer();

        _duendeHost.MapGet("/external-jwks", async context =>
        {
            var signatureAlgorithm = context.Request.Query["alg"];
            if (string.IsNullOrEmpty(signatureAlgorithm))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync("400 - Bad Request.");
                
                return;
            }

            var securityKeyData = GetExternalSecurityKeyData(signatureAlgorithm!);
            var jsonWebKey = JsonWebKeyConverter.ConvertFromSecurityKey(securityKeyData.SecurityKey);
            jsonWebKey.Alg = signatureAlgorithm;
            jsonWebKey.Use = "sig";

            var keys = new JsonWebKeys();
            keys.Keys.Add(jsonWebKey);

            await context.Response.WriteAsJsonAsync(keys);
        });

        _duendeHost.MapGet("/external-cert", async context =>
        {
            var signatureAlgorithm = context.Request.Query["alg"];
            if (string.IsNullOrEmpty(signatureAlgorithm))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync("400 - Bad Request.");

                return;
            }

            var securityKeyData = GetExternalSecurityKeyData(signatureAlgorithm!);
            var pem = SecurityKeyBuilder.GetCertificatePublicKeyPem(securityKeyData.SecurityKey);

            await context.Response.WriteAsync(pem);
        });

        _duendeHost.UseAuthorization();

        _duendeHostCancellationSource = new CancellationTokenSource();
        _duendeHost.RunAsync(_duendeHostCancellationSource.Token);
    }

    /// <summary>
    /// Retrieves "external" security key information to test tokens containing a "x5u" or "jku" claim.
    /// </summary>
    public static (string KeyId, SecurityKey SecurityKey) GetExternalSecurityKeyData(string signatureAlgorithm)
    {
        if (GeneratedSecurityKeys.TryGetValue(signatureAlgorithm!, out var securityKeyData))
        {
            return securityKeyData;
        }

        var securityKey = SecurityKeyBuilder.CreateSecurityKey(signatureAlgorithm!);
        securityKeyData = (securityKey.KeyId, securityKey);
        GeneratedSecurityKeys.Add(signatureAlgorithm!, securityKeyData);

        return securityKeyData;
    }

    private static IEnumerable<Client> ConfigureClients()
    {
        yield return new Client
        {
            ClientId = "m2m",
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientSecrets = { new Secret("secret".Sha256()) },
            AllowedScopes = { TestSettings.CurrentTestSettings.DefaultAudience }
        };

        yield return new Client
        {
            ClientId = "interactive.confidential",
            AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
            RequirePkce = true,
            ClientSecrets = { new Secret("secret".Sha256()) },
            AllowedScopes = { TestSettings.CurrentTestSettings.DefaultAudience }
        };
    }

    /// <summary>
    /// Disposes the local Duende IdentityServer instance.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _duendeHostCancellationSource?.Cancel();
            
            var task = _duendeHost?.DisposeAsync();
            if (task is not null && !task.Value.IsCompleted)
            {
                task.Value.GetAwaiter().GetResult();
            }
        }

        base.Dispose(disposing);
    }
}