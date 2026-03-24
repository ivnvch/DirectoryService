using FileService.Core;
using FileService.Infrastructure.Postgres;
using FileService.Infrastructure.S3;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Exceptions;
using Shared.Logging;

namespace FileService.Web.Configurations;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSerilogLogging(configuration, "FileService")
            .AddS3(configuration)
            .AddInfrastructure(configuration);/*
            .AddEndpoints(typeof(DependencyInjectionCoreExtensions).Assembly);
*/
        services
            .AddCore(configuration);
        
        return services;
    }
}