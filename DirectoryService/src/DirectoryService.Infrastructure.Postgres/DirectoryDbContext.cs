using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure;

public class DirectoryDbContext : DbContext
{
    private readonly string _connectionString;

    public DirectoryDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DirectoryDbContext).Assembly);
    }
}