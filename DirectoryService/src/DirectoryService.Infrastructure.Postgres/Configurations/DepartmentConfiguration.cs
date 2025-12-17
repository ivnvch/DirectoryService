using DirectoryService.Domain.Departments;
using DirectoryService.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");
        
        builder.HasKey(x => x.Id)
            .HasName("pk_department_id");

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(LengthConstant.Max150Length)
            .HasColumnName("name");

        builder.ComplexProperty(d => d.DepartmentIdentifier, di =>
        {
            di.Property(d => d.Value)
                .IsRequired()
                .HasColumnName("identifier")
                .HasMaxLength(LengthConstant.Max150Length);
        });

        builder.ComplexProperty(d => d.DepartmentPath, dp =>
        {
            dp.Property(d => d.Value)
                .IsRequired()
                .HasColumnName("path");
        });
        
        builder.Property(d => d.ParentId)
            .HasColumnName("parent_id");
        
        builder.Property(d => d.Depth)
            .HasColumnName("depth");
        
        builder.Property(d => d.IsActive)
            .HasColumnName("is_active");
        
        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone");
        
        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone");
    }
}