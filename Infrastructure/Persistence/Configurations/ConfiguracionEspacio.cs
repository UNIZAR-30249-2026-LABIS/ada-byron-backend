using AdaByron.Domain.Entities;
using AdaByron.Domain.ValueObjects;
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

        // ValueObject Planta (int inmutable): se persiste solo su valor entero
        builder.Property(e => e.Planta)
               .HasConversion(
                   p => p.Valor,
                   v => Planta.De(v))
               .HasColumnName("planta")
               .IsRequired();

        // ValueObject Aforo (int inmutable): se persiste solo su valor entero
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

        // El departamento es opcional (solo se usa en laboratorios)
        builder.Property(e => e.Departamento)
               .HasMaxLength(150)
               .IsRequired(false);

        // Ubicación geoespacial PostGIS (shadow property — no contamina el Dominio)
        // Accesible desde repositorio via: entry.Property<Point>("Ubicacion")
        builder.Property<Point>("Ubicacion")
               .HasColumnType("geometry(Point,4326)")
               .HasColumnName("ubicacion")
               .IsRequired(false);
    }
}
