using AdaByron.Application.Ports.Out;

namespace AdaByron.Infrastructure.Services;

public sealed class AforoEdificioService : IAforoEdificioService
{
    private long _porcentajeBits = BitConverter.DoubleToInt64Bits(100.0); // 100% por defecto

    public Task<double> GetPorcentajeAsync()
    {
        var bits = Interlocked.Read(ref _porcentajeBits);
        return Task.FromResult(BitConverter.Int64BitsToDouble(bits));
    }

    public Task SetPorcentajeAsync(double porcentaje)
    {
        if (porcentaje is < 10.0 or > 100.0)
            throw new ArgumentOutOfRangeException(nameof(porcentaje), "Debe estar entre 10% y 100%.");

        Interlocked.Exchange(ref _porcentajeBits, BitConverter.DoubleToInt64Bits(porcentaje));
        return Task.CompletedTask;
    }
}
