using AdaByron.Application.DTOs;
using AdaByron.Application.UseCases.Reservas;
using AdaByron.Domain.Exceptions;
using AdaByron.Infrastructure.Persistence;
using AdaByron.Infrastructure.Persistence.DbContext;
using AdaByron.Infrastructure.Persistence.Repositories;
using AdaByron.Integration.Tests.Fixture;
using FluentAssertions;
using Xunit;

namespace AdaByron.Integration.Tests;

public class ReservaConcurrencyTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public ReservaConcurrencyTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CrearReserva_ConcurrenteMultiple_ElAislamientoAdvisoryLockPrevieneColisionesFantasmas()
    {
        // Arrange
        // Lanzaremos 10 peticiones simultáneas, intentando reservar el mismo espacio "A-100" a la vez.
        int totalRequests = 10;
        const string emailTest = "test@unizar.es";
        var requestDto = new CrearReservaRequestDTO(
            CodigoEspacio: "A-100",
            NumeroAsistentes: 5,
            Inicio: DateTime.UtcNow.Date.AddHours(10), // Hoy de 10:00 a 12:00 UTC
            Fin: DateTime.UtcNow.Date.AddHours(12)
        );

        // Act
        var tasks = new Task<ReservaResponseDTO>[totalRequests];
        
        for (int i = 0; i < totalRequests; i++)
        {
            // OJO: Hay que crear un DbContext + repositorios AISLADOS para cada hilo, 
            // igual que hace net core en requests HTTP distintas gracias al Scoped Lifetime.
            tasks[i] = Task.Run(async () =>
            {
                var options = _fixture.GetDbContextOptions();
                await using var context = new AplicacionDbContext(options);
                var uow = new UnitOfWork(context);
                
                var useCase = new CrearReservaUseCase(
                    new PersonaRepository(context),
                    new EspacioRepository(context),
                    new ReservaRepository(context),
                    uow,
                    new EdificioConfigRepository(context)
                );

                return await useCase.ExecuteAsync(emailTest, requestDto);
            });
        }

        // De las 10 tareas concurrentes, EXACTAMENTE UNA debe completarse creando la reserva.
        // Las demás 9 deben fallar rebotando con ExcepcionConflictoReserva
        int successfulReservations = 0;
        int conflictExceptions = 0;

        foreach (var task in tasks)
        {
            try
            {
                await task;
                successfulReservations++;
            }
            catch (ExcepcionConflictoReserva)
            {
                conflictExceptions++;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado: {ex.Message}", ex);
            }
        }

        // Assert
        successfulReservations.Should().Be(1, "solo uno de los hilos de concurrencia debería lograr la reserva.");
        conflictExceptions.Should().Be(totalRequests - 1, "los otros requests bloquearon su hilo esperando el Lock, y al leer la base de datos detectaron el solapamiento");
    }

    [Fact]
    public async Task ModificacionConcurrente_MismaReserva_LanzaDbUpdateConcurrencyException()
    {
        // Arrange
        var options = _fixture.GetDbContextOptions();

        // 1. Crear una reserva inicial
        var reservaId = Guid.NewGuid();
        var franjaInicial = new AdaByron.Domain.Aggregates.ReservationAggregate.FranjaHoraria(
            DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(2));
            
        var reserva = AdaByron.Domain.Aggregates.ReservationAggregate.Reserva.Reconstituir(
            reservaId, "test@unizar.es", "A-100", franjaInicial, 10, 
            AdaByron.Domain.Aggregates.ReservationAggregate.EstadoReserva.Pendiente, 
            DateTime.UtcNow);

        await using (var seedContext = new AplicacionDbContext(options))
        {
            seedContext.Reservas.Add(reserva);
            await seedContext.SaveChangesAsync();
        }

        // Act & Assert
        // 2. Contexto A y B cargan la MISMA reserva
        await using var contextA = new AplicacionDbContext(options);
        await using var contextB = new AplicacionDbContext(options);

        var reservaA = await contextA.Reservas.FindAsync(reservaId);
        var reservaB = await contextB.Reservas.FindAsync(reservaId);

        // 3. Contexto A la modifica y guarda con éxito
        reservaA!.Aceptar();
        await contextA.SaveChangesAsync(); // Éxito

        // 4. Contexto B la modifica e intenta guardar
        reservaB!.Rechazar();

        // Al hacer SaveChanges, B fallará por el xmin (Optimistic Concurrency)
        var action = async () => await contextB.SaveChangesAsync();

        await action.Should().ThrowAsync<Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException>();
    }
}
