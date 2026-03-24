using System.Reflection;
using FileService.Domain;
using Microsoft.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres;

public class FileServiceDbContext : DbContext
{
    public FileServiceDbContext(DbContextOptions<FileServiceDbContext> options)
    : base(options)
    {
        
    }

    public DbSet<MediaAsset> MediaAssets { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}