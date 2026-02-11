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


        builder.ComplexProperty(l => l.Name, ln =>
        {
            ln.Property(l => l.Value)
                .HasColumnName("name")
                .HasMaxLength(LengthConstant.Max150Length)
                .IsRequired();
        });

        builder.OwnsOne(l => l.Address, ad =>
        {
            ad.ToJson("address");
            
            ad.Property(l => l.Country)
                .IsRequired()
                .Metadata.SetJsonPropertyName("country");
            ad.Property(l => l.City)
                .IsRequired()
                .Metadata.SetJsonPropertyName("city");
            ad.Property(l => l.Street)
                .IsRequired()
                .Metadata.SetJsonPropertyName("street");
            ad.Property(l => l.House)
                .IsRequired()
                .Metadata.SetJsonPropertyName("house");
            ad.Property(l => l.Apartment)
                .IsRequired(false)
                .Metadata.SetJsonPropertyName("apartment");
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
        
        builder.Property(d => d.DeletedAt)
            .HasColumnName("deleted_at");
    }
}