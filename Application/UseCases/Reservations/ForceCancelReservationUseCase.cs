using AdaByron.Application.Ports.Out;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Interfaces;

namespace AdaByron.Application.UseCases.Reservations;

/// <summary>
/// PBI-13 (HU-O4): Cancelación forzada administrativa de una reserva por el Gerente.
/// A diferencia de CancelarReservaUseCase (HU-18), no hay restricciones de tiempo
/// y el gerente puede actuar sobre cualquier estado de reserva.
/// </summary>
public class ForceCancelReservationUseCase(
    IReservaRepository reservas,
    INotificationService notifications)
{
    public async Task ExecuteAsync(Guid reservaId)
    {
        // 1. Obtener la reserva
        var reserva = await reservas.GetByIdAsync(reservaId)
            ?? throw new ExcepcionDominio($"No existe ninguna reserva con ID '{reservaId}'.");

        // 2. Delegar la mutación al dominio (valida invariantes)
        reserva.ForzarCancelacion();

        // 3. Persistir
        await reservas.UpdateAsync(reserva);

        // 4. Notificar al solicitante (reutiliza el puerto existente de rescisión)
        await notifications.NotifyReservationRescindedAsync(
            reserva.Id,
            reserva.PersonaId,
            reserva.EspacioId);
    }
}
