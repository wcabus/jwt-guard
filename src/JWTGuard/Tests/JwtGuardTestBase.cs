using JWTGuard.Helpers;

using Microsoft.AspNetCore.Mvc.Testing;

using Xunit;

namespace JWTGuard.Tests;

/// <summary>
/// Base class for JWT Guard test cases.
/// </summary>
/// <param name="factory"></param>
[Collection(JwtGuardTestCollection.CollectionName)]
public abstract class JwtGuardTestBase(TargetApiWebApplicationFactory factory) : IAsyncLifetime
{
    private AsyncServiceScope _serviceScope;

    /// <summary>
    /// The <see cref="TargetApiWebApplicationFactory"/>
    /// </summary>
    protected TargetApiWebApplicationFactory Factory { get; } = factory;
    
    /// <summary>
    /// A <see cref="HttpClient"/> that can access the target Web API.
    /// </summary>
    protected HttpClient? Client { get; private set; }

    /// <summary>
    /// A <see cref="IServiceProvider"/> to request services from.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    protected IServiceProvider? ServiceProvider { get; private set; }
    
    /// <summary>
    /// Initializes the base class for a test run.
    /// </summary>
    public Task InitializeAsync()
    {
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost/")
        });

        _serviceScope = Factory.Services.CreateAsyncScope();
        ServiceProvider = _serviceScope.ServiceProvider;
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the service scope and every service requested during the test run.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _serviceScope.DisposeAsync();
    }
}