using AdaByron.Application.Ports.Out;
using Microsoft.Extensions.Configuration;

namespace AdaByron.Infrastructure.Services;

/// <summary>
/// Implementación del puerto de salida <see cref="IAforoEdificioService"/> (HU-T1 / PBI-5).
///
/// El porcentaje de aforo del edificio se inicializa desde la configuración de la aplicación
/// (<c>appsettings.json → AforoEdificio:PorcentajeInicial</c>), con fallback a la constante
/// <see cref="DefaultPorcentaje"/> (100 %) si la clave no existe.
///
/// Thread-safe: los accesos concurrentes utilizan <see cref="Interlocked"/> sobre el patrón de
/// "bit pattern as long" porque <c>volatile</c> no admite <c>double</c> en C#.
/// Singleton → el porcentaje persiste durante toda la vida de la aplicación.
/// </summary>
public sealed class AforoEdificioService : IAforoEdificioService
{
    // ── Constante fallback (HU-T1: valor por defecto si no hay configuración) ────
    private const double DefaultPorcentaje = 100.0;

    // Almacenado como long (bit pattern) para acceso atómico con Interlocked
    private long _porcentajeBits;

    public AforoEdificioService(IConfiguration configuration)
    {
        var configRaw = configuration["AforoEdificio:PorcentajeInicial"];
        var inicial   = double.TryParse(configRaw, out var val) ? val : DefaultPorcentaje;

        // Validación defensiva: si el valor configurado está fuera de rango usamos el default
        if (inicial < 10.0 || inicial > 100.0)
            inicial = DefaultPorcentaje;

        _porcentajeBits = BitConverter.DoubleToInt64Bits(inicial);
    }

    /// <inheritdoc/>
    public Task<double> GetPorcentajeActualAsync()
    {
        var bits = Interlocked.Read(ref _porcentajeBits);
        return Task.FromResult(BitConverter.Int64BitsToDouble(bits));
    }

    /// <inheritdoc/>
    public Task SetPorcentajeAsync(double porcentaje)
    {
        if (porcentaje is < 10.0 or > 100.0)
            throw new ArgumentOutOfRangeException(nameof(porcentaje),
                "El porcentaje debe estar entre 10.0 (10 %) y 100.0 (100 %).");

        Interlocked.Exchange(ref _porcentajeBits, BitConverter.DoubleToInt64Bits(porcentaje));
        return Task.CompletedTask;
    }
}
