using FileService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileService.Infrastructure.Postgres.Configurations;

public class MediAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("media_assets");
        
        builder.HasKey(x => x.Id)
            .HasName("pk_media_asset_id");
        
        builder.Property(e => e.Id)
            .HasColumnName("id");


        builder.HasDiscriminator(e => e.AssetType)
            .HasValue<VideoAsset>(AssetType.Video)
            .HasValue<PreviewAsset>(AssetType.Preview);
        
        builder.Property(x => x.AssetType)
            .HasConversion<string>()
            .HasColumnName("asset_type");
        
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();
        
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.OwnsOne(e => e.MediaData, md =>
        {
            md.ToJson("media_data");
            
            md.OwnsOne(mde => mde.FileName, fn =>
            {
                fn.Property(x => x.Name).HasJsonPropertyName("name");

                fn.Property(x => x.Extension).HasJsonPropertyName("extension");
            });

            md.OwnsOne(mde => mde.ContentType, ct =>
            {
                ct.Property(x => x.Category)
                    .HasConversion<string>()
                    .HasJsonPropertyName("category");

                ct.Property(x => x.Value).HasJsonPropertyName("content_type");
            });
            
            md.Property(x => x.ExpectedChunksCount).HasJsonPropertyName("expected_chunks_count");
            
            md.Property(x => x.Size).HasJsonPropertyName("size");
        });

        builder.OwnsOne(e => e.Key, rkb =>
        {
            rkb.ToJson("key");

            rkb.Property(rke => rke.Location).HasJsonPropertyName("location");

            rkb.Property(rke => rke.Prefix).HasJsonPropertyName("prefix");

            rkb.Property(rke => rke.Key).HasJsonPropertyName("key");
        });

        builder.OwnsOne(e => e.FinalKey, fkb =>
        {
            fkb.ToJson("final_key");

            fkb.Property(fke => fke.Location).HasJsonPropertyName("location");

            fkb.Property(fke => fke.Prefix).HasJsonPropertyName("prefix");

            fkb.Property(fke => fke.Key).HasJsonPropertyName("key");
        });

        builder.ComplexProperty(e => e.Owner, ob =>
        {
            ob.Property(oe => oe.Context)
                .HasColumnName("context")
                .HasMaxLength(50)
                .IsRequired();

            ob.Property(oe => oe.EntityId)
                .HasColumnName("entity_id")
                .IsRequired();
        });
        
        builder.HasIndex(e => new { e.Status, e.CreatedAt })
            .HasDatabaseName("ix_media_assets_status_created_at");

    }
}