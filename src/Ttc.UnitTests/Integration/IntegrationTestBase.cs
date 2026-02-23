using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Ttc.DataEntities.Core;

namespace Ttc.UnitTests.Integration;

[Collection("Integration")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected TtcWebApplicationFactory Factory { get; }
    protected HttpClient Client => _client ?? throw new InvalidOperationException("Client not initialized. Ensure InitializeAsync has been called.");

    private HttpClient? _client;

    protected IntegrationTestBase(TtcWebApplicationFactory factory)
    {
        Factory = factory;
    }

    public virtual Task InitializeAsync()
    {
        _client = Factory.CreateClient();
        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        _client?.Dispose();
        return Task.CompletedTask;
    }

    protected async Task<T?> GetAsync<T>(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    protected async Task<HttpResponseMessage> PostAsync<T>(string url, T content)
    {
        return await Client.PostAsJsonAsync(url, content);
    }

    protected ITtcDbContext GetDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ITtcDbContext>();
    }
}

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<TtcWebApplicationFactory>
{
}
