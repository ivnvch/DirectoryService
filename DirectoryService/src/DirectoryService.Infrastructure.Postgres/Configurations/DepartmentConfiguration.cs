using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObject;
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

        builder.ComplexProperty(d => d.Name, dn =>
        {
            dn.Property(x => x.Value)
                .IsRequired()
                .HasMaxLength(LengthConstant.Max150Length)
                .HasColumnName("name");
        });

        builder.ComplexProperty(d => d.DepartmentIdentifier, di =>
        {
            di.Property(d => d.Value)
                .IsRequired()
                .HasColumnName("identifier")
                .HasMaxLength(LengthConstant.Max150Length);
        });
        
        builder.Property(d => d.DepartmentPath)
            .HasColumnType("ltree")
            .IsRequired()
            .HasColumnName("path")
            .HasConversion(value => value.Value,
                value => DepartmentPath.Create(value));
        
        builder.HasIndex(x => x.DepartmentPath).HasMethod("gist").HasDatabaseName("idx_department_path");
        
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

        builder.HasMany(x => x.ChildrenDepartments)
            .WithOne()
            .IsRequired(false)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}