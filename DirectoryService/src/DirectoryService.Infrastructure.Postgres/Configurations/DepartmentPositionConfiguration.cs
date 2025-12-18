using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_position");
        
        builder.HasKey(dp => dp.Id)
            .HasName("pk_department_position_id");
        
        builder.Property(dp => dp.PositionId)
            .HasColumnName("fk_department_position_position_id");
        
        builder.Property(dp => dp.DepartmentId)
            .HasColumnName("fk_department_position_department_id");
        
        builder.HasOne<Department>()
            .WithMany(d => d.Positions)
            .HasForeignKey(dp => dp.DepartmentId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne<Position>()
            .WithMany(d => d.Departments)
            .HasForeignKey(dp => dp.PositionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}