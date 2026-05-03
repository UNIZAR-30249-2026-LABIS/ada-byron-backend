using AdaByron.Application.DTOs;
using AdaByron.Application.UseCases.Reservations;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Interfaces;

namespace AdaByron.Application.UseCases.Spaces;

/// <summary>
/// Caso de uso PBI-12 (HU-O1): Define o elimina el porcentaje de uso específico de un espacio.
/// PBI-13 (HU-O4): Tras el cambio, reevalúa las reservas existentes y marca como PotencialmenteInvalida
///                 las que ya no cumplen el nuevo aforo efectivo.
/// </summary>
public class SetSpaceOccupancyUseCase(
    IEspacioRepository espacios,
    ReevaluarReservasEspacioUseCase reevaluar)
{
    public async Task<int> ExecuteAsync(string codigoEspacio, SetSpaceOccupancyDTO request)
    {
        // 1. Obtener el espacio
        var espacio = await espacios.GetByCodigoAsync(codigoEspacio)
            ?? throw new ExcepcionDominio($"No existe ningún espacio con código '{codigoEspacio}'.");

        // 2. Delegar la validación y mutación al Aggregate Root
        espacio.SetPorcentajeEspecifico(request.PorcentajeEspecifico);

        // 3. Persistir el cambio de porcentaje
        await espacios.UpdateAsync(espacio);

        // 4. PBI-13: Reevaluar y marcar reservas que ya no cumplen las reglas
        int marcadas = await reevaluar.ExecuteAsync(espacio);
        return marcadas;
    }
}
