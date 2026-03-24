using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileService.Infrastructure.Postgres;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<FileServiceDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("file_service");

            IHostEnvironment? hostEnvironment =  sp.GetService<IHostEnvironment>();
            ILoggerFactory? loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            options.UseNpgsql(connectionString);

            if (hostEnvironment != null && hostEnvironment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
            options.UseLoggerFactory(loggerFactory);
        });
        
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        return services;
    }
}