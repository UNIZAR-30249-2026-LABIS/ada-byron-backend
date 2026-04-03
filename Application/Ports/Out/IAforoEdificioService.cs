namespace AdaByron.Application.Ports.Out;

/// <summary>
/// Puerto de salida (HU-T1 / PBI-5) para el control dinámico del aforo del edificio Ada Byron.
/// El porcentaje se inicializa desde appsettings.json y puede cambiarse por API en caliente.
/// Rango válido: 10 %–100 %.
/// </summary>
public interface IAforoEdificioService
{
    /// <summary>
    /// Devuelve el porcentaje *actual* de ocupación permitida del edificio (10.0–100.0).
    /// El valor se inicializa desde la configuración (appsettings.json → AforoEdificio:PorcentajeInicial)
    /// y puede actualizarse en caliente vía <see cref="SetPorcentajeAsync"/>.
    /// </summary>
    Task<double> GetPorcentajeActualAsync();

    /// <summary>Actualiza el porcentaje de aforo permitido. Rango válido: 10.0–100.0.</summary>
    Task SetPorcentajeAsync(double porcentaje);
}
