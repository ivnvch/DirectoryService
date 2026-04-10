using System.Reflection;
using FileService.Core;
using FileService.Domain;
using Microsoft.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres;

public class FileServiceDbContext : DbContext, IReadDbContext
{
    public FileServiceDbContext(DbContextOptions<FileServiceDbContext> options)
    : base(options)
    {
        
    }

    public DbSet<MediaAsset> MediaAssets { get; set; } = null!;
    
    public IQueryable<MediaAsset> MediaAssetsRead => 
        Set<MediaAsset>().AsQueryable().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}