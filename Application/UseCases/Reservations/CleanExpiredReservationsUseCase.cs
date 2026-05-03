using AdaByron.Application.Ports.Out;
using AdaByron.Domain.Interfaces;

namespace AdaByron.Application.UseCases.Reservations;

/// <summary>
/// PBI-14: Limpia (cancela) las reservas que llevan en estado PotencialmenteInvalida
/// más de los días especificados.
/// </summary>
public class CleanExpiredReservationsUseCase(
    IReservaRepository reservas,
    INotificationService notifier)
{
    public async Task<int> ExecuteAsync(int daysThreshold = 7)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysThreshold);
        
        // 1. Obtener reservas caducadas en estado PotencialmenteInvalida
        var expiredReservations = (await reservas.GetExpiredPotentiallyInvalidAsync(cutoffDate)).ToList();

        if (expiredReservations.Count == 0)
        {
            return 0;
        }

        // 2. Forzar la cancelación de cada reserva afectada
        foreach (var reserva in expiredReservations)
        {
            reserva.ForzarCancelacion();
        }

        // 3. Persistir cambios en lote
        await reservas.UpdateRangeAsync(expiredReservations);

        // 4. Notificar en tiempo real a los usuarios afectados
        foreach (var reserva in expiredReservations)
        {
            await notifier.NotifyCancellationAsync(
                reserva.PersonaId, 
                $"El sistema ha cancelado tu reserva para el espacio {reserva.EspacioId} al haber transcurrido {daysThreshold} días sin poder acomodarla."
            );
        }

        return expiredReservations.Count;
    }
}
