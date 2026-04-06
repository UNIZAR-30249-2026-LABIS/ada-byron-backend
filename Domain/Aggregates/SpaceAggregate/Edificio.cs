namespace AdaByron.Domain.Aggregates.SpaceAggregate;

/// <summary>
/// Representa el Edificio Ada Byron y centraliza el cálculo del aforo permitido (PBI-5).
/// </summary>
public record Edificio
{
    public static readonly Edificio AdaByron = new();

    private Edificio() { }

    /// <summary>
    /// Calcula la capacidad permitida en base al aforo base y el porcentaje de ocupación (Regla F5).
    /// </summary>
    public int CalcularCapacidadPermitida(int aforoBase, double porcentajeOcupacion)
    {
        return (int)Math.Floor(aforoBase * (porcentajeOcupacion / 100.0));
    }
}
