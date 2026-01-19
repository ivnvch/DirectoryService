using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");
        
        builder.HasKey(p => p.Id)
            .HasName("pk_position_id");

        builder.Property(p => p.Name)
            .IsRequired()
            .HasColumnName("name");
        
        builder.HasIndex(p => p.Name)
            .IsUnique()
            .HasFilter("is_active = true")
            .HasName("idx_position_name_active");
        
        builder.Property(p => p.Description)
            .HasColumnName("description");

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active");
            
        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone");
        
        

    }
}