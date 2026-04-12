using AdaByron.Application.DTOs;
using AdaByron.Application.Ports.Out;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Interfaces;

namespace AdaByron.Application.UseCases.Reservas;

/// <summary>
/// Caso de uso HU-18: Cancela una reserva por iniciativa de su propietario.
/// Invariantes de dominio:
///   - Solo el propietario puede cancelarla (verificado por el controlador con el JWT).
///   - No se puede cancelar si ya está Rescindida o Rechazada.
///   - No se puede cancelar si la franja horaria ya ha comenzado (pasado o en curso).
/// </summary>
public class CancelarReservaUseCase(
    IReservaRepository reservas,
    IEspacioRepository espacios,
    INotificationService notifications)
{
    public async Task<MiReservaDTO> ExecuteAsync(Guid reservaId, string emailSolicitante)
    {
        // 1. Obtener la reserva
        var reserva = await reservas.GetByIdAsync(reservaId)
            ?? throw new ExcepcionDominio($"No existe ninguna reserva con ID '{reservaId}'.");

        // 2. Verificar propiedad: solo el dueño puede cancelar (HU-18)
        if (!reserva.PersonaId.Equals(emailSolicitante, StringComparison.OrdinalIgnoreCase))
            throw new ExcepcionPermisos("No tienes permiso para cancelar una reserva que no te pertenece.");

        // 3. Llamar al método de dominio que valida el resto de invariantes
        reserva.Cancelar(); // Lanza ExcepcionDominio si ya está rescindida o si es pasada/en-curso

        // 4. Persistir el cambio de estado
        await reservas.UpdateAsync(reserva);

        // 5. Obtener nombre del espacio para el DTO de respuesta
        var espacio = await espacios.GetByCodigoAsync(reserva.EspacioId);
        var nombreEspacio = espacio?.Nombre ?? reserva.EspacioId;

        // 6. Notificar (reuse existing port — en este caso el propio usuario ya sabe, pero lo registramos)
        await notifications.NotifyReservationRescindedAsync(reserva.Id, reserva.PersonaId, reserva.EspacioId);

        return new MiReservaDTO
        {
            Id               = reserva.Id,
            EspacioId        = reserva.EspacioId,
            NombreEspacio    = nombreEspacio,
            Inicio           = reserva.Franja.Inicio,
            Fin              = reserva.Franja.Fin,
            Estado           = reserva.Estado.ToString(),
            NumeroAsistentes = reserva.NumeroAsistentes
        };
    }
}
