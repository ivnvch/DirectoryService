using FileService.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests.Infrastructure;

public class FileBaseTests : IClassFixture<FileTestWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase;
    protected IServiceProvider Services { get; set; }
    protected HttpClient AppHttpClient { get; set; }
    protected HttpClient HttpClient { get; set; }

    public const string TEST_FILE_NAME = "file-example.mp4";
    
    protected FileBaseTests(FileTestWebFactory factory)
    {
        Services = factory.Services;

        _resetDatabase = factory.ResetDatabaseAsync;
        
        AppHttpClient = factory.CreateClient();
        HttpClient = new HttpClient();
    }
    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _resetDatabase();
    }
    
    protected async Task<T> ExecuteHandler<T>(Func<T, Task<T>> action) where T : notnull
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<T>();

        return await action(handler);
    }

    protected async Task ExecuteInDb(Func<FileServiceDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();
        
        await action(dbContext);
    }
}