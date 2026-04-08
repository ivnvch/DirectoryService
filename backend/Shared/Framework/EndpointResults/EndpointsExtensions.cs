using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.EndpointResults;

public static class EndpointsExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        services.Scan(scan =>
            scan.FromAssemblies(assembly)
                .AddClasses(classes =>
                    classes.AssignableTo<IEndpoint>()).AsSelfWithInterfaces()
                .WithTransientLifetime());
        
        return services;
    }

    public static IApplicationBuilder MapEndpoints(this WebApplication app, RouteGroupBuilder? routeGroupBuilder = null)
    {
        IEnumerable<IEndpoint> endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

        foreach (IEndpoint endpoint in endpoints)
            endpoint.MapEndpoint(builder);
        
        return app;
    }
}