using System.Data.Common;
using Amazon.S3;
using FileService.Infrastructure.Postgres;
using FileService.Infrastructure.S3;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using FileService.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Respawn;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;

namespace FileService.IntegrationTests;

public class FileTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithDatabase("file_service_test_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
    
    private readonly MinioContainer _minioContainer = new MinioBuilder()
        .WithImage("minio/minio")
        .WithUsername("minioadmin")
        .WithPassword("minioadmin")
        .Build();
    

    private Respawner _respawner = null!;
    private DbConnection _dbConnection = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.IntegrationTests.json"),
                optional: true);
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:file_service"] = _dbContainer.GetConnectionString(),
            });
        });
        
        
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<FileServiceDbContext>();
            
            services.AddScoped<FileServiceDbContext>(provider =>
            {
                DbContextOptions<FileServiceDbContext> options = new DbContextOptionsBuilder<FileServiceDbContext>()
                    .UseNpgsql(_dbContainer.GetConnectionString())
                    .Options;
                return new FileServiceDbContext(options);
            });
            
            services.RemoveAll<IAmazonS3>();

            services.AddSingleton<IAmazonS3>(sp =>
            {
                S3Options s3Options = sp.GetRequiredService<IOptions<S3Options>>().Value;
                ushort minioPort = _minioContainer.GetMappedPublicPort(9000);
                var config = new AmazonS3Config
                {
                    ServiceURL = $"http://{_minioContainer.Hostname}:{minioPort}",
                    UseHttp = true,
                    ForcePathStyle = true
                };

                return new AmazonS3Client(s3Options.AccessKey, s3Options.SecretKey, config);
            });
        });
        
        
    }

    public async Task InitializeAsync()
    {
      await _dbContainer.StartAsync();
      await _minioContainer.StartAsync();

      await using var scope = Services.CreateAsyncScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();

      await dbContext.Database.EnsureDeletedAsync();
      await dbContext.Database.EnsureCreatedAsync();
      
      _dbConnection = dbContext.Database.GetDbConnection();
      //_dbConnection = new NpgSqlConnection(_dbContainer.GetConnectionString("file_service"));
      await _dbConnection.OpenAsync();
      
      await InitializeRespawner();
    }

    public new async Task DisposeAsync()
    {
       await _dbContainer.StopAsync();
       await _dbContainer.DisposeAsync();
       
       await _minioContainer.StopAsync();
       await _minioContainer.DisposeAsync();

       await _dbConnection.CloseAsync();
       await _dbConnection.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }
    
    private async Task InitializeRespawner()
    {
        _respawner = await Respawner.CreateAsync(_dbConnection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"]
            });
    }
}