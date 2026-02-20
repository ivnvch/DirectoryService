using DirectoryService.Shared.Errors;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Exceptions;

namespace DirectoryService.API;

public static class DependencyInjection
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddOpenApiSpec()
            .AddSerilogLogging(configuration);
    }

    private static IServiceCollection AddOpenApiSpec(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddSchemaTransformer((schema, context, _) =>
            {
                if (context.JsonTypeInfo.Type == typeof(Envelope<Errors>))
                {
                    if (schema.Properties.TryGetValue("errors", out var errorsProp))
                    {
                        errorsProp.Items.Reference = new OpenApiReference
                        {
                            Type = ReferenceType.Schema,
                            Id = "Error"
                        };
                    }
                }
                return Task.CompletedTask;
            });
        });
        
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "DirectoryService",
                Version = "v1",
                Contact = new OpenApiContact
                {
                    Name = "Education"
                }
            });
        });
        
        return services;
    }
    
    private static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((sp, lc) => lc
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(sp)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("Application", "DirectoryService"));

        return services;
    }
}