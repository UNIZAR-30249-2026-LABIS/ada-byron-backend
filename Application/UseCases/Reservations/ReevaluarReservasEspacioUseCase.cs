using AdaByron.Domain.Aggregates.SpaceAggregate;
using AdaByron.Domain.Interfaces;

namespace AdaByron.Application.UseCases.Reservations;

/// <summary>
/// PBI-13 (HU-O4): Reevalúa las reservas futuras de un espacio tras un cambio de aforo o porcentaje.
/// - Si el nuevo aforo es menor: reservas Aceptadas que ya no caben → PotencialmenteInvalida.
/// - Si el nuevo aforo es mayor: reservas PotencialmenteInvalida que ahora caben → Aceptada (restauración automática).
/// Invocado desde SetSpaceOccupancyUseCase, UpdateSpaceUseCase y UpdateBuildingConfigUseCase.
/// </summary>
public class ReevaluarReservasEspacioUseCase(
    IReservaRepository reservas,
    IEdificioConfigRepository configRepo)
{
    /// <summary>
    /// Reevalúa en ambas direcciones. Devuelve el neto de cambios:
    ///   positivo = nº de reservas marcadas como PotencialmenteInvalida
    ///   negativo = nº de reservas restauradas a Aceptada (aforo subió)
    /// </summary>
    public async Task<int> ExecuteAsync(Espacio espacio)
    {
        // 1. Config del edificio (para calcular el % heredado si el espacio no tiene uno específico)
        var config = await configRepo.GetConfigAsync()
                     ?? new EdificioConfig("AdaByron", 100);

        // 2. Aforo efectivo con el nuevo valor ya persistido
        int aforoEfectivo = espacio.CalcularAforoEfectivo(config);

        // ── Dirección ↓: Aceptada → PotencialmenteInvalida ───────────────────
        var aceptadas = (await reservas.GetAceptadasFuturasByEspacioAsync(espacio.CodigoEspacio)).ToList();
        var aInvalidar = aceptadas.Where(r => r.NumeroAsistentes > aforoEfectivo).ToList();
        foreach (var r in aInvalidar)
            r.MarcarComoPotencialmenteInvalida();

        if (aInvalidar.Count > 0)
            await reservas.UpdateRangeAsync(aInvalidar);

        // ── Dirección ↑: PotencialmenteInvalida → Aceptada (si el aforo subió) ─
        var potInvalidas = (await reservas.GetPotencialmenteInvalidasFuturasByEspacioAsync(espacio.CodigoEspacio)).ToList();
        var aRestaurar = potInvalidas.Where(r => r.NumeroAsistentes <= aforoEfectivo).ToList();
        foreach (var r in aRestaurar)
            r.RestablecerAceptada();

        if (aRestaurar.Count > 0)
            await reservas.UpdateRangeAsync(aRestaurar);

        // Devolvemos marcadas − restauradas (positivo = situación empeoró, negativo = mejoró)
        return aInvalidar.Count - aRestaurar.Count;
    }
}
