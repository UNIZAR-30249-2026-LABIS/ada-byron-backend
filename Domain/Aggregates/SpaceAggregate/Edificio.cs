namespace AdaByron.Domain.Aggregates.SpaceAggregate;

/// <summary>
/// El edificio "Ada Byron" como concepto de dominio (HU-14).
/// Centraliza la capacidad total y el aforo dinámico global.
/// </summary>
public record Edificio(string Nombre, int CapacidadMaxima)
{
    public static Edificio AdaByron => new("Ada Byron", 1000); // Ejemplo de capacidad total

    /// <summary>
    /// Calcula la capacidad permitida para un aforo dado según el porcentaje dinámico.
    /// Formula: Asistentes <= Capacidad * (Porcentaje / 100)
    /// </summary>
    public int CalcularCapacidadPermitida(int capacidadEspacio, double porcentajeEdificio)
    {
        var factor = porcentajeEdificio / 100.0;
        return (int)System.Math.Floor(capacidadEspacio * factor);
    }
}
