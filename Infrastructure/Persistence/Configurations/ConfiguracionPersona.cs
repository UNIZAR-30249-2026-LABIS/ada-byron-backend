using AdaByron.Domain.Entities;
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

        // El enum se persiste como string para legibilidad en la BD
        builder.Property(p => p.Rol)
               .HasConversion<string>()
               .HasMaxLength(30)
               .IsRequired();

        builder.Property(p => p.Departamento)
               .HasMaxLength(150);
    }
}
