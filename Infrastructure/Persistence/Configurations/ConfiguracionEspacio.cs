using AdaByron.Domain.Aggregates.PersonAggregate;
using AdaByron.Domain.Aggregates.SpaceAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;

namespace AdaByron.Infrastructure.Persistence.Configurations;

public class ConfiguracionEspacio : IEntityTypeConfiguration<Espacio>
{
    public void Configure(EntityTypeBuilder<Espacio> builder)
    {
        builder.ToTable("espacios");

        // CodigoEspacio es la clave primaria e identidad inmutable
        builder.HasKey(e => e.CodigoEspacio);
        builder.Property(e => e.CodigoEspacio)
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(e => e.Nombre)
               .HasMaxLength(200)
               .IsRequired();

        // ── ValueObject Planta → int ──────────────────────────────────────────
        builder.Property(e => e.Planta)
               .HasConversion(
                   p => p.Valor,
                   v => Planta.De(v))
               .HasColumnName("altura")
               .IsRequired();

        // ── ValueObject Aforo → int ───────────────────────────────────────────
        builder.Property(e => e.Aforo)
               .HasConversion(
                   a => a.Valor,
                   v => Aforo.De(v))
               .HasColumnName("aforo")
               .IsRequired();

        // Los enums se persisten como string para legibilidad en la BD
        builder.Property(e => e.TipoFisico)
               .HasConversion<string>()
               .HasMaxLength(30)
               .IsRequired();

        builder.Property(e => e.CategoriaReserva)
               .HasConversion<string>()
               .HasMaxLength(30)
               .IsRequired();

        // ── ValueObject Departamento → string ─────────────────────────────────
        // Mapeo como Owned Type forzando el nombre de columna en BD.
        builder.OwnsOne(e => e.Departamento, d =>
        {
            d.Property(x => x.Nombre)
             .HasColumnName("Departamento")
             .HasMaxLength(150)
             .IsRequired(false);
        });

        // Ubicación geoespacial PostGIS (shadow property — no contamina el Dominio)
        builder.Property<Point>("Ubicacion")
               .HasColumnType("geometry(Point,4326)")
               .HasColumnName("ubicacion")
               .IsRequired(false);

        // ── Relaciones ───────────────────────────────────────────────────────
        // EF Core mapeará automáticamente a la colección privada _reservas
        builder.HasMany(e => e.Reservas)
               .WithOne()
               .HasForeignKey(r => r.EspacioId);
        
        builder.Metadata.FindNavigation(nameof(Espacio.Reservas))?
               .SetPropertyAccessMode(PropertyAccessMode.Field);

        // ── Índices para PostGIS y Búsquedas ─────────────────────────────────
        builder.HasIndex("Ubicacion").HasMethod("GIST");
        builder.HasIndex(e => e.Planta);
    }
}
