using AdaByron.Application.Ports.Out;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Interfaces;

namespace AdaByron.Application.UseCases.Reservations;

/// <summary>
/// Caso de uso para la eliminación administrativa de reservas (HU-18).
/// Valida la existencia y notifica al usuario vía SignalR tras la eliminación.
/// </summary>
public class DeleteReservationUseCase(
    IReservaRepository reservas,
    INotificationService notifications)
{
    public async Task ExecuteAsync(Guid id)
    {
        // 1. Validar existencia
        var reserva = await reservas.GetByIdAsync(id)
            ?? throw new ExcepcionDominio($"No se encontró la reserva con ID: {id}");

        // 2. Ejecutar eliminación física
        // Se podría añadir lógica para comprobar si es 'live' (fin > ahora) según reglas de negocio
        // pero el panel de administración ya filtra las activas. 
        await reservas.DeleteAsync(id);

        // 3. Notificación a través del puerto de salida SignalR (HU-18)
        await notifications.NotifyReservationRescindedAsync(
            reserva.Id, 
            reserva.PersonaId, 
            reserva.EspacioId);
    }
}
