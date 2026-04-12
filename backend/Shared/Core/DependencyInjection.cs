using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions;

namespace Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedCore(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.Scan(scan => scan.FromAssemblies(assemblies)
            .AddClasses(classes => classes
                .AssignableToAny(
                    typeof(ICommandHandler<,>),
                    typeof(ICommandHandler<>),
                    typeof(IQueryHandler<,>),
                    typeof(IQueryHandler<>)))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        return services;
    }
}