using DirectoryService.Application.CQRS;
using DirectoryService.Application.Extensions.Cache.Options;
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

        services
            .AddCache(configuration);
        
        return services;
    }

    private static IServiceCollection AddCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(setup =>
        {
            setup.Configuration = configuration.GetConnectionString("Redis");
        });
        
        var cacheOptions = configuration.GetSection("CacheOptions").Get<CacheOptions>() ??  new CacheOptions();
        
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions()
            {
                LocalCacheExpiration =  cacheOptions.LocalCacheExpiration,
                Expiration =  cacheOptions.Expiration
            };
        });

        return services;
    }
}