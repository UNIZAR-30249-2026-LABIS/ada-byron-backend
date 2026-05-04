namespace AdaByron.Domain.Aggregates.SpaceAggregate;

/// <summary>
/// Representa el Edificio Ada Byron y centraliza el cálculo del aforo permitido (PBI-5).
/// </summary>
public record Edificio
{
    public static readonly Edificio AdaByron = new();

    /// <summary>
    /// Plantas del edificio Ada Byron que contienen espacios reservables (0 = PB, …, 4 = cuarta).
    /// El Sótano (-1) y la Planta 5 no tienen espacios gestionables en el sistema.
    /// </summary>
    public static readonly IReadOnlySet<int> PlantasReservables = new HashSet<int> { 0, 1, 2, 3, 4 };

    private Edificio() { }

    /// <summary>
    /// Calcula la capacidad permitida en base al aforo base y el porcentaje de ocupación (Regla F5).
    /// </summary>
    public int CalcularCapacidadPermitida(int aforoBase, double porcentajeOcupacion)
    {
        return (int)Math.Floor(aforoBase * (porcentajeOcupacion / 100.0));
    }
}
