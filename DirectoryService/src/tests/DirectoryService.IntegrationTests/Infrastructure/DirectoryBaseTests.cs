using DirectoryService.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Infrastructure;

public class DirectoryBaseTests : IClassFixture<DirectoryTestWebFactory>, IAsyncLifetime
{
    protected IServiceProvider Services;
    protected readonly Func<Task> ResetDatabase;
    protected readonly HttpClient HttpClient;
    protected readonly HttpClient AppHttpClient;


    protected DirectoryBaseTests(DirectoryTestWebFactory factory)
    {
        HttpClient = new HttpClient();
        AppHttpClient = factory.CreateClient();
        Services = factory.Services;
        ResetDatabase = factory.ResetDatabaseAsync;
    }
    
    protected async Task<T> ExecuteInDb<T>(Func<DirectoryDbContext, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryDbContext>();

        return await action(dbContext);
    }
    
    protected async Task ExecuteInDb(Func<DirectoryDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryDbContext>();
        await action(dbContext);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() =>  await ResetDatabase();
}