using AdaByron.Domain.Aggregates.PersonAggregate;
using AdaByron.Domain.Aggregates.ReservationAggregate;
using AdaByron.Domain.Aggregates.SpaceAggregate;
using AdaByron.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace AdaByron.Integration.Tests.Fixture;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;

    public DatabaseFixture()
    {
        // Require PostGIS image because the DbContext relies on HasPostgresExtension("postgis")
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgis/postgis:15-3.3")
            .WithDatabase("adabyron_test_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithCleanUp(true)
            .Build();
    }

    public string ConnectionString => _dbContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var options = new DbContextOptionsBuilder<AplicacionDbContext>()
            .UseNpgsql(ConnectionString, o => o.UseNetTopologySuite())
            .Options;

        // Apply migrations
        await using var context = new AplicacionDbContext(options);
        await context.Database.MigrateAsync();

        // Seed basic data required for the concurrecy test
        await SeedDataAsync(context);
    }

    private async Task SeedDataAsync(AplicacionDbContext context)
    {
        var configure = new EdificioConfig("AdaByron", 100);
        
        var persona = new Persona("test@unizar.es", "Test", "User", Rol.Docente, new Departamento("Informatica"));
        
        var espacio = new Espacio("A-100", "Aula Magna", Planta.De(1), Aforo.De(50), TipoEspacio.Aula, new Departamento("Informatica"));

        context.EdificioConfigs.Add(configure);
        context.Personas.Add(persona);
        context.Espacios.Add(espacio);
        
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }

    public DbContextOptions<AplicacionDbContext> GetDbContextOptions()
    {
        return new DbContextOptionsBuilder<AplicacionDbContext>()
            .UseNpgsql(ConnectionString, o => o.UseNetTopologySuite())
            .Options;
    }
}
