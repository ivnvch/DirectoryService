using DirectoryService.Domain.Locations;
using DirectoryService.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");
        
        builder.HasKey(l => l.Id)
            .HasName("pk_location_id");

        builder.Property(l => l.Name)
            .HasColumnName("name")
            .HasMaxLength(LengthConstant.Max150Length)
            .IsRequired();

        builder.ComplexProperty(l => l.Address, ad =>
        {
            ad.Property(l => l.Value)
                .HasColumnName("address")
                .IsRequired();
        });

        builder.ComplexProperty(l => l.Timezone, tz =>
        {
            tz.Property(l => l.Value)
                .HasColumnName("timezone")
                .IsRequired();
        });

        builder.Property(l => l.IsActive)
            .HasColumnName("is_active");
        
        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasColumnType("timestamp with time zone");
        
        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone");
        

    }
}