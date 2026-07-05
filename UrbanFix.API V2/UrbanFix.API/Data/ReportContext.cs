using Microsoft.EntityFrameworkCore;
using UrbanFix.API.Entities;

namespace UrbanFix.API.Data;

public class ReportContext : DbContext
{
    public ReportContext(DbContextOptions<ReportContext> options) : base(options)
    {
    }

    public DbSet<Profile> Profiles { get; set; }

    public DbSet<Institution> Institutions { get; set; }

    public DbSet<Report> Reports { get; set; }

    public DbSet<ReportImage> ReportImages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.ToTable("profiles");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("id");
            entity.Property(p => p.Role).HasColumnName("role").IsRequired();
            entity.Property(p => p.FullName).HasColumnName("full_name");
            entity.Property(p => p.NationalId).HasColumnName("national_id").HasMaxLength(15);
            entity.Property(p => p.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(p => p.NationalId).IsUnique();
        });

        modelBuilder.Entity<Institution>(entity =>
        {
            entity.ToTable("institutions");
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Id).HasColumnName("id");
            entity.Property(i => i.ProfileId).HasColumnName("profile_id");
            entity.Property(i => i.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(i => i.OfficialDomain).HasColumnName("official_domain").HasMaxLength(200).IsRequired();
            entity.Property(i => i.Category).HasColumnName("category").HasMaxLength(100);
            entity.Property(i => i.Zone).HasColumnName("zone").HasMaxLength(100);
            entity.Property(i => i.Status).HasColumnName("status").IsRequired();
            entity.Property(i => i.CreatedAt).HasColumnName("created_at");
            entity.Property(i => i.ReviewedAt).HasColumnName("reviewed_at");

            entity.HasOne<Profile>()
                .WithMany()
                .HasForeignKey(i => i.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.ToTable("reports");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).HasColumnName("id");
            entity.Property(r => r.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
            entity.Property(r => r.Description).HasColumnName("description").HasMaxLength(2000).IsRequired();
            entity.Property(r => r.Category).HasColumnName("category");
            entity.Property(r => r.Priority).HasColumnName("priority");
            entity.Property(r => r.Latitude).HasColumnName("latitude").HasPrecision(9, 6);
            entity.Property(r => r.Longitude).HasColumnName("longitude").HasPrecision(9, 6);
            entity.Property(r => r.CivilianUserId).HasColumnName("civilian_user_id");
            entity.Property(r => r.InstitutionId).HasColumnName("institution_id");
            entity.Property(r => r.Status).HasColumnName("status");
            entity.Property(r => r.AiCategory).HasColumnName("ai_category");
            entity.Property(r => r.AiPriority).HasColumnName("ai_priority");
            entity.Property(r => r.AiConfidence).HasColumnName("ai_confidence").HasPrecision(4, 3);
            entity.Property(r => r.CreatedAt).HasColumnName("created_at");
            entity.Property(r => r.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne<Profile>()
                .WithMany()
                .HasForeignKey(r => r.CivilianUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Institution>()
                .WithMany()
                .HasForeignKey(r => r.InstitutionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(r => r.Images)
                .WithOne(i => i.Report)
                .HasForeignKey(i => i.ReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReportImage>(entity =>
        {
            entity.ToTable("report_images");
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Id).HasColumnName("id");
            entity.Property(i => i.ReportId).HasColumnName("report_id");
            entity.Property(i => i.StoragePath).HasColumnName("storage_path").HasMaxLength(500).IsRequired();
            entity.Property(i => i.ThumbnailPath).HasColumnName("thumbnail_path").HasMaxLength(500);
            entity.Property(i => i.ContentType).HasColumnName("content_type").HasMaxLength(50).IsRequired();
            entity.Property(i => i.FileSizeBytes).HasColumnName("file_size_bytes");
            entity.Property(i => i.SortOrder).HasColumnName("sort_order");
            entity.Property(i => i.CreatedAt).HasColumnName("created_at");
        });
    }
}
