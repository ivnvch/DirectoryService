using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FileService.Infrastructure.Postgres.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();

        var retries = 5;

        while (retries > 0)
        {
            try
            {
                await db.Database.MigrateAsync();
                return;
            }
            catch
            {
                retries--;
                if (retries == 0) throw;
                await Task.Delay(3000);
            }
        }
    }
}