namespace AdaByron.Application.Ports.Out;

public interface IAforoEdificioService
{
    Task<double> GetPorcentajeAsync();
    Task SetPorcentajeAsync(double porcentaje);
}
