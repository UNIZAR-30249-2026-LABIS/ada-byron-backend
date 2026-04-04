using AdaByron.Domain.Aggregates.PersonAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdaByron.Infrastructure.Persistence.Configurations;

public class ConfiguracionPersona : IEntityTypeConfiguration<Persona>
{
    public void Configure(EntityTypeBuilder<Persona> builder)
    {
        builder.ToTable("personas");

        // Email es la clave primaria e identidad inmutable
        builder.HasKey(p => p.Email);
        builder.Property(p => p.Email)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(p => p.Nombre)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(p => p.Apellidos)
               .HasMaxLength(150)
               .IsRequired();

        // El enum Rol se persiste como string para legibilidad en la BD
        builder.Property(p => p.Rol)
               .HasConversion<string>()
               .HasMaxLength(30)
               .IsRequired();

        // ── ValueObject Departamento ──────────────────────────────────────────
        // Mapeo como Owned Type forzando el nombre exacto de la columna en BD ("Departamento").
        builder.OwnsOne(p => p.Departamento, d =>
        {
            d.Property(x => x.Nombre)
             .HasColumnName("Departamento")
             .HasMaxLength(150)
             .IsRequired(false);
        });
    }
}
