using AdaByron.Domain.Aggregates.PersonAggregate;
using AdaByron.Domain.Aggregates.SpaceAggregate;
using AdaByron.Domain.Aggregates.ReservationAggregate;
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

        builder.Property(r => r.NumeroAsistentes)
               .IsRequired();

        // Estado persiste como string para legibilidad en la BD
        builder.Property(r => r.Estado)
               .HasConversion<string>()
               .HasColumnName("estado")
               .HasMaxLength(20)
               .IsRequired();

        // ── ValueObject FranjaHoraria → OwnsOne ───────────────────────────────
        // FranjaHoraria tiene 2 campos (Inicio, Fin) → OwnsOne es la opción correcta.
        // EF Core mapeará las columnas "inicio" y "fin" dentro de la tabla "reservas".
        builder.OwnsOne(r => r.Franja, fb =>
        {
            fb.Property(f => f.Inicio)
              .HasColumnName("inicio")
              .IsRequired();

            fb.Property(f => f.Fin)
              .HasColumnName("fin")
              .IsRequired();
        });

        // FK hacia Persona (sin navegación en la entidad agregada)
        builder.HasOne<Persona>()
               .WithMany()
               .HasForeignKey(r => r.PersonaId)
               .OnDelete(DeleteBehavior.Restrict);

        // FK hacia Espacio (Aggregate Root tiene una colección Reservas)
        builder.HasOne<Espacio>()
               .WithMany(e => e.Reservas)
               .HasForeignKey(r => r.EspacioId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
