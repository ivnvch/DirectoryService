using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.ToTable("department_locations");
        
        builder.HasKey(p => p.Id)
            .HasName("pk_department_location_id");

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(dl => dl.DepartmentId)
            .HasColumnName("department_id");
        
        builder.Property(dl => dl.LocationId)
            .HasColumnName("location_id");

        builder.HasOne<Department>()
            .WithMany(d => d.Locations)
            .HasForeignKey(dp => dp.DepartmentId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne<Location>()
            .WithMany(l => l.Departments)
            .HasForeignKey(p => p.LocationId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}