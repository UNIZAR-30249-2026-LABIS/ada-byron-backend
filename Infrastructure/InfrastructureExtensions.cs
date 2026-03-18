using AdaByron.Domain.Interfaces;
using AdaByron.Infrastructure.Persistence.DbContext;
using AdaByron.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AdaByron.Infrastructure;

// Registra todos los servicios de Infrastructure en el contenedor DI.
// Es el único punto de contacto entre AdaByron.API y los detalles de EF Core / PostGIS.
public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AplicacionDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql =>
                {
                    npgsql.UseNetTopologySuite();                              // soporte PostGIS
                    npgsql.MigrationsAssembly("AdaByron.Infrastructure");     // migraciones en Infrastructure
                }));

        // Repositorios — conecta interfaces de Application/Ports/Out con implementaciones EF Core
        services.AddScoped<IEspacioRepository, EspacioRepository>();
        services.AddScoped<IPersonaRepository, PersonaRepository>();
        services.AddScoped<IReservaRepository,  ReservaRepository>();

        return services;
    }
}
