using DirectoryService.Application.Locations.Repositories;
using DirectoryService.Infrastructure.Locations.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ILocationRepository, LocationRepository>();
        
        return services;
    }
}