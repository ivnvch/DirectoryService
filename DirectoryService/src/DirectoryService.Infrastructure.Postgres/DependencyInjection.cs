using DirectoryService.Application.Locations.Repositories;
using DirectoryService.Infrastructure.Locations.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DirectoryService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<DirectoryDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("directory_service");
            
            IHostEnvironment? hostEnvironment = sp.GetService<IHostEnvironment>();
            
            options.UseNpgsql(connectionString);

            if (hostEnvironment != null && hostEnvironment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });
        services.AddScoped<ILocationRepository, LocationRepository>();
        
        return services;
    }
}