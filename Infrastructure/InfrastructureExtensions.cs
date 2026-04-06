using AdaByron.Application.Ports.Out;
using AdaByron.Domain.Interfaces;
using AdaByron.Infrastructure.Identity;
using AdaByron.Infrastructure.Persistence;
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
                    npgsql.UseNetTopologySuite();
                    npgsql.MigrationsAssembly("AdaByron.Infrastructure");
                }));

        // Repositorios
        services.AddScoped<IEspacioRepository, EspacioRepository>();
        services.AddScoped<IPersonaRepository,  PersonaRepository>();
        services.AddScoped<IReservaRepository,  ReservaRepository>();

        // Transacción ACID (IUnitOfWork → UnitOfWork por petición HTTP)
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Servicios externos (puertos de salida)
        services.AddScoped<ITokenService, TokenService>();

        // Repositorio de Configuración del Edificio (Aforo PBI-5/6)
        services.AddScoped<IEdificioConfigRepository, EdificioConfigRepository>();

        return services;
    }
}
