using DirectoryService.Application.CQRS;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace DirectoryService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
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
        
        return services;
    }
}