using AdaByron.Application.Ports.Out;

namespace AdaByron.Infrastructure.Services;

/// <summary>
/// Implementación en memoria del IAforoEdificioService.
/// Thread-safe mediante Interlocked (volatile no admite double en C#).
/// Singleton -> el porcentaje persiste durante toda la vida de la aplicación.
/// Por defecto: 100%. Rango permitido: 10%–100%.
/// </summary>
public sealed class AforoEdificioService : IAforoEdificioService
{
    // Almacenado como long (bit pattern) para acceso atómico (100.0)
    private long _porcentajeBits = BitConverter.DoubleToInt64Bits(100.0);

    public Task<double> GetPorcentajeAsync()
    {
        var bits = Interlocked.Read(ref _porcentajeBits);
        return Task.FromResult(BitConverter.Int64BitsToDouble(bits));
    }

    public Task SetPorcentajeAsync(double porcentaje)
    {
        if (porcentaje is < 10.0 or > 100.0)
            throw new ArgumentOutOfRangeException(nameof(porcentaje),
                "El porcentaje debe estar entre 10.0 (10%) y 100.0 (100%).");

        Interlocked.Exchange(ref _porcentajeBits, BitConverter.DoubleToInt64Bits(porcentaje));
        return Task.CompletedTask;
    }
}
