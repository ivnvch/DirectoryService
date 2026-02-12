using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
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

    protected Task<Guid> CreateLocationAsync()
        => ExecuteInDb(context => TestDataFactory.CreateLocationAsync(context));
    
    protected Task<Position> CreatePositionAsync(IEnumerable<Guid> departmentIds)
        => ExecuteInDb(context => TestDataFactory.CreatePositionAsync(context, departmentIds: departmentIds));

    protected Task<Department> CreateDepartmentAsync(int locationsCount = 1)
        => ExecuteInDb(context => TestDataFactory.CreateDepartmentAsync(context, locationsCount));

    protected Task<Guid> CreateDepartmentIdAsync(int locationsCount = 1)
        => ExecuteInDb(context => TestDataFactory.CreateDepartmentIdAsync(context, locationsCount));

    protected Task<Department> CreateChildDepartmentAsync(Department parent, int locationsCount = 1)
        => ExecuteInDb(context => TestDataFactory.CreateChildDepartmentAsync(context, parent, locationsCount));

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() =>  await ResetDatabase();
}