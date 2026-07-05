using Microsoft.EntityFrameworkCore;
using Urbanfix.Models;

namespace Urbanfix.Data;

public class UrbanfixDbContext : DbContext
{
    public UrbanfixDbContext(DbContextOptions<UrbanfixDbContext> options)
        : base(options)
    {
    }

    public DbSet<Punto> Puntos => Set<Punto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Punto>(entity =>
        {
            entity.ToTable("Puntos");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Titulo).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Descripcion).HasMaxLength(1000);
            entity.Property(p => p.Latitud).IsRequired();
            entity.Property(p => p.Longitud).IsRequired();
        });
    }
}
