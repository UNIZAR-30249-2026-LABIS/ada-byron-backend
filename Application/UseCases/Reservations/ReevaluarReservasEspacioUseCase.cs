using AdaByron.Domain.Aggregates.SpaceAggregate;
using AdaByron.Domain.Interfaces;

namespace AdaByron.Application.UseCases.Reservations;

/// <summary>
/// PBI-13 (HU-O4): Reevalúa las reservas Aceptadas futuras de un espacio tras un cambio de aforo o porcentaje.
/// Las reservas que ya no cumplen las reglas de aforo efectivo se marcan como PotencialmenteInvalida.
/// Este Use Case es invocado internamente desde SetSpaceOccupancyUseCase y UpdateSpaceUseCase.
/// </summary>
public class ReevaluarReservasEspacioUseCase(
    IReservaRepository reservas,
    IEdificioConfigRepository configRepo)
{
    /// <summary>
    /// Evalúa todas las reservas Aceptadas futuras del espacio y marca las que incumplen el nuevo aforo.
    /// Devuelve el número de reservas marcadas.
    /// </summary>
    public async Task<int> ExecuteAsync(Espacio espacio)
    {
        // 1. Obtener config del edificio (para el % global si el espacio no tiene específico)
        var config = await configRepo.GetConfigAsync()
                     ?? new EdificioConfig("AdaByron", 100);

        // 2. Calcular el aforo efectivo ACTUAL del espacio (ya con el nuevo valor guardado)
        int aforoEfectivo = espacio.CalcularAforoEfectivo(config);

        // 3. Obtener reservas Aceptadas futuras del espacio
        var reservasAfectadas = (await reservas.GetAceptadasFuturasByEspacioAsync(espacio.CodigoEspacio)).ToList();

        if (reservasAfectadas.Count == 0) return 0;

        // 4. Marcar las que superan el nuevo aforo efectivo
        var marcadas = reservasAfectadas
            .Where(r => r.NumeroAsistentes > aforoEfectivo)
            .ToList();

        foreach (var r in marcadas)
            r.MarcarComoPotencialmenteInvalida();

        if (marcadas.Count > 0)
            await reservas.UpdateRangeAsync(marcadas);

        return marcadas.Count;
    }
}
