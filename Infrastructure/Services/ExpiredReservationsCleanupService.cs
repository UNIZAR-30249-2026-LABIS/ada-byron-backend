using AdaByron.Application.UseCases.Reservations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AdaByron.Infrastructure.Services;

/// <summary>
/// PBI-14: Background service que se ejecuta periódicamente para limpiar (cancelar)
/// las reservas atascadas en estado PotencialmenteInvalida por más de N días.
/// </summary>
public class ExpiredReservationsCleanupService(
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration,
    ILogger<ExpiredReservationsCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Leer configuración, por defecto 1 minuto y 7 días
        var intervalMinutes = configuration.GetValue<int>("CleanupService:IntervalMinutes", 1);
        var expirationDays = configuration.GetValue<int>("CleanupService:ExpirationDays", 7);

        logger.LogInformation("ExpiredReservationsCleanupService iniciado. Intervalo: {IntervalMinutes}m, Expiración: {ExpirationDays} días.", intervalMinutes, expirationDays);

        // Retraso inicial para no bloquear el inicio de la app
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Iniciando tarea de limpieza de reservas inválidas expiradas...");

                // ⚠️ TRUCO DE SENIOR: Usar IServiceScopeFactory para resolver Scoped services dentro de un Singleton (BackgroundService)
                using var scope = serviceScopeFactory.CreateScope();
                var useCase = scope.ServiceProvider.GetRequiredService<CleanExpiredReservationsUseCase>();
                
                await useCase.ExecuteAsync(expirationDays);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error crítico durante la limpieza de reservas expiradas.");
            }

            // Esperar hasta la siguiente ejecución
            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }
    }
}
