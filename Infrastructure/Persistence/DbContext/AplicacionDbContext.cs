using AdaByron.Domain.Aggregates.PersonAggregate; using AdaByron.Domain.Aggregates.SpaceAggregate; using AdaByron.Domain.Aggregates.ReservationAggregate;
using Microsoft.EntityFrameworkCore;

namespace AdaByron.Infrastructure.Persistence.DbContext;

public class AplicacionDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<Espacio> Espacios => Set<Espacio>();
    public DbSet<Reserva>  Reservas  => Set<Reserva>();

    public AplicacionDbContext(DbContextOptions<AplicacionDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Activa la extensión PostGIS en la base de datos
        modelBuilder.HasPostgresExtension("postgis");

        // Carga automáticamente todas las IEntityTypeConfiguration<T> del ensamblado
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AplicacionDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
