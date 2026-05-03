using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Interfaces;

namespace AdaByron.Application.UseCases.Reservations;

/// <summary>
/// PBI-13 (HU-O4): El Gerente admite una excepción, restaurando la reserva de PotencialmenteInvalida → Aceptada.
/// Esto indica explícitamente que el gerente decide no aplicar la restricción de aforo para esa reserva concreta.
/// </summary>
public class ApproveReservationExceptionUseCase(IReservaRepository reservas)
{
    public async Task ExecuteAsync(Guid reservaId)
    {
        // 1. Obtener la reserva
        var reserva = await reservas.GetByIdAsync(reservaId)
            ?? throw new ExcepcionDominio($"No existe ninguna reserva con ID '{reservaId}'.");

        // 2. Delegar la mutación al dominio (valida que esté en PotencialmenteInvalida)
        reserva.AdmitirExcepcion();

        // 3. Persistir
        await reservas.UpdateAsync(reserva);
    }
}
