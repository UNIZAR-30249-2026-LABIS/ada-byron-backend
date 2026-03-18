using AdaByron.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdaByron.Infrastructure.Persistence.Configurations;

public class ConfiguracionReserva : IEntityTypeConfiguration<Reserva>
{
    public void Configure(EntityTypeBuilder<Reserva> builder)
    {
        builder.ToTable("reservas");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.PersonaId)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(r => r.EspacioId)
               .HasMaxLength(20)
               .IsRequired();

        // ValueObject FranjaHoraria: se persiste en dos columnas dentro de la misma tabla
        builder.OwnsOne(r => r.Franja, fb =>
        {
            fb.Property(f => f.Inicio)
              .HasColumnName("inicio")
              .IsRequired();

            fb.Property(f => f.Fin)
              .HasColumnName("fin")
              .IsRequired();
        });

        // Relaciones con Persona y Espacio por FK (sin navegación en la entidad)
        builder.HasOne<Persona>()
               .WithMany()
               .HasForeignKey(r => r.PersonaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Espacio>()
               .WithMany()
               .HasForeignKey(r => r.EspacioId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
