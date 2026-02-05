using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Departments.Repositories;
using DirectoryService.Application.Locations.Repositories;
using DirectoryService.Application.Positions.Repositories;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Infrastructure.Departments.Repositories;
using DirectoryService.Infrastructure.Locations.Repositories;
using DirectoryService.Infrastructure.Positions.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<DirectoryDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("directory_service");
            
            IHostEnvironment? hostEnvironment = sp.GetService<IHostEnvironment>();
            ILoggerFactory? loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            
            options.UseNpgsql(connectionString);

            if (hostEnvironment != null && hostEnvironment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
            
            options.UseLoggerFactory(loggerFactory);
        });
        
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        
        services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<DirectoryDbContext>());
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<ITransactionManager, TransactionManager>();
        
        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
        
        return services;
    }
}