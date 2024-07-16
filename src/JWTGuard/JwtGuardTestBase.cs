using Microsoft.AspNetCore.Mvc.Testing;

using Xunit;

namespace JWTGuard;

[Collection(JwtGuardTestCollection.CollectionName)]
public abstract class JwtGuardTestBase(TargetApiWebApplicationFactory factory) : IAsyncLifetime
{
    private AsyncServiceScope _serviceScope;

    protected TargetApiWebApplicationFactory Factory { get; } = factory;
    protected HttpClient? Client { get; private set; }
    protected IServiceProvider? ServiceProvider { get; private set; }

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

    public async Task DisposeAsync()
    {
        await _serviceScope.DisposeAsync();
    }
}