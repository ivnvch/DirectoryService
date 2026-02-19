using DirectoryService.Application.CQRS;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;

namespace DirectoryService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        
        var assembly = typeof(DependencyInjection).Assembly;

        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes
                .AssignableToAny(
                    typeof(ICommandHandler<,>), 
                    typeof(IQueryHandler<,>), 
                    typeof(IListQueryHandler<,>),
                    typeof(IQueryHandler<>)))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        services.AddStackExchangeRedisCache(setup =>
        {
            setup.Configuration = configuration.GetConnectionString("Redis");
        });
        
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions()
            {
                LocalCacheExpiration =  TimeSpan.FromMinutes(5),
                Expiration = TimeSpan.FromMinutes(30),
            };
        });
        
        return services;
    }
}