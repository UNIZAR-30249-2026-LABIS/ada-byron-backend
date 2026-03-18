namespace AdaByron.Application.Ports.Out;

/// <summary>
/// Puerto de salida para el control dinámico del aforo del edificio Ada Byron.
/// El porcentaje puede cambiarse por API en caliente.
/// </summary>
public interface IAforoEdificioService
{
    /// <summary>Devuelve el porcentaje actual de ocupación permitida (0.10 – 1.00).</summary>
    Task<double> GetPorcentajeAsync();

    /// <summary>Actualiza el porcentaje. Rango: 0.10 – 1.00.</summary>
    Task SetPorcentajeAsync(double porcentaje);
}
