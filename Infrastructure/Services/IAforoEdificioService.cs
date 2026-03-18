// Esta interfaz se movió a AdaByron.Application.Ports.Out.IAforoEdificioService
// Este archivo se mantiene vacío para no romper referencias existentes.
// La implementación: Infrastructure/Services/AforoEdificioService.cs

/// <summary>
/// Puerto de salida para obtener y modificar el porcentaje de ocupación del edificio.
/// Definido en Infrastructure (no en Domain) porque es un servicio técnico/operacional.
/// </summary>
public interface IAforoEdificioService
{
    /// <summary>Devuelve el porcentaje actual (0.10 – 1.00).</summary>
    Task<double> GetPorcentajeAsync();

    /// <summary>Actualiza el porcentaje. Rango: 0.10 – 1.00.</summary>
    Task SetPorcentajeAsync(double porcentaje);
}
