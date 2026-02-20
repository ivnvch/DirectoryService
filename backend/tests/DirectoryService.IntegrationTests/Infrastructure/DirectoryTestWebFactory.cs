using System.Data.Common;
using DirectoryService.API;
using DirectoryService.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace DirectoryService.IntegrationTests.Infrastructure;

public class DirectoryTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithDatabase("directory_service_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
    
    private Respawner _respawner = null!;
    private DbConnection _dbConnection =  null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.Tests.json"), optional: true);
        });
        
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DirectoryDbContext>();

            services.AddDbContextPool<DirectoryDbContext>((sp, options) =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });
        });
    }

    public async Task InitializeAsync()
    {
       await _dbContainer.StartAsync();
       
       await using var scope = Services.CreateAsyncScope();
       
       var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryDbContext>();
       
       await dbContext.Database.EnsureDeletedAsync();
       await dbContext.Database.EnsureCreatedAsync();

       _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
       await _dbConnection.OpenAsync();
       
       await InitialazeRespawner();
    }

    public async Task DisposeAsync()
    {
       await _dbContainer.StopAsync();
       await _dbContainer.DisposeAsync();
       
       await _dbConnection.CloseAsync();
       await _dbConnection.DisposeAsync();
    }

    private async Task InitialazeRespawner()
    {
        _respawner = await Respawner.CreateAsync(
            _dbConnection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = new[]
                {
                    "public"
                }
            });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }
}