using System.Diagnostics.CodeAnalysis;
namespace AdaByron.Domain.Aggregates.SpaceAggregate;

/// <summary>
/// Entidad de configuración global del edificio (ej: para el Aforo Dinámico PBI-5 / PBI-6).
/// Se almacena en la base de datos para persistir configuraciones administrativas.
/// </summary>
public class EdificioConfig
{
    public required string Id { get; init; }
    public double PorcentajeOcupacion { get; private set; }

    private EdificioConfig() { }

    [SetsRequiredMembers]
    public EdificioConfig(string id, double porcentajeOcupacion)
    {
        Id = id;
        SetPorcentaje(porcentajeOcupacion);
    }

    public void SetPorcentaje(double porcentaje)
    {
        if (porcentaje < 0 || porcentaje > 100)
            throw new ArgumentException("El porcentaje debe estar entre 0 y 100.");
            
        PorcentajeOcupacion = porcentaje;
    }
}
