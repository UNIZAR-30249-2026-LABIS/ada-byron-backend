using AdaByron.Application.Ports.Out;

namespace AdaByron.Infrastructure.Services;

/// <summary>
/// Implementación en memoria del IAforoEdificioService.
/// Thread-safe mediante Interlocked (volatile no admite double en C#).
/// Singleton → el porcentaje persiste durante toda la vida de la aplicación.
/// Por defecto: 100%. Rango permitido: 10%–100%.
/// </summary>
public sealed class AforoEdificioService : IAforoEdificioService
{
    // Almacenado como long (bit pattern) para acceso atómico
    private long _porcentajeBits = BitConverter.DoubleToInt64Bits(1.0);

    public Task<double> GetPorcentajeAsync()
    {
        var bits = Interlocked.Read(ref _porcentajeBits);
        return Task.FromResult(BitConverter.Int64BitsToDouble(bits));
    }

    public Task SetPorcentajeAsync(double porcentaje)
    {
        if (porcentaje is < 0.10 or > 1.0)
            throw new ArgumentOutOfRangeException(nameof(porcentaje),
                "El porcentaje debe estar entre 0.10 (10%) y 1.00 (100%).");

        Interlocked.Exchange(ref _porcentajeBits, BitConverter.DoubleToInt64Bits(porcentaje));
        return Task.CompletedTask;
    }
}
