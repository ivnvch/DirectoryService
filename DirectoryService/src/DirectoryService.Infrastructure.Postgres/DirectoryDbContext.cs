using System.Data;
using System.Reflection;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure;

public class DirectoryDbContext : DbContext, IReadDbContext
{
    public DirectoryDbContext(DbContextOptions<DirectoryDbContext> options): base(options){}

    public DbSet<Location> Locations { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Position> Positions { get; set; } = null!;
    public DbSet<DepartmentLocation> DepartmentLocations { get; set; } = null!;
    public DbSet<DepartmentPosition> DepartmentPositions { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public IQueryable<Location> LocationsRead =>
        Set<Location>().AsQueryable().AsNoTracking();
    public IQueryable<Position> PositionsRead =>
        Set<Position>().AsQueryable().AsNoTracking();
    public IQueryable<Department> DepartmentsRead =>
        Set<Department>().AsQueryable().AsNoTracking();
    public IQueryable<DepartmentPosition> DepartmentPositionsRead =>
        Set<DepartmentPosition>().AsQueryable().AsNoTracking();
}