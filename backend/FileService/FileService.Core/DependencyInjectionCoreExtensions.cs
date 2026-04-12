using FluentValidation;
using FileService.Core.Features.MediaAssets;
using FileService.Core.Features.MediaAssets.Delete;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace FileService.Core;

public static class DependencyInjectionCoreExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(DependencyInjectionCoreExtensions).Assembly;
        
        services
            .AddValidatorsFromAssembly(assembly)
            .AddSharedCore(assembly);
        
        return services;
    }
}