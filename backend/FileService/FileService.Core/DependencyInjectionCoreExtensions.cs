using FluentValidation;
using FileService.Core.Features.MediaAssets;
using FileService.Core.Features.MediaAssets.Delete;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.Core;

public static class DependencyInjectionCoreExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjectionCoreExtensions).Assembly);
        services.AddScoped<DeleteFileHandler>();
        
        return services;
    }
}