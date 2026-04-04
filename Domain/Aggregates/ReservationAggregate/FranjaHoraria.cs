namespace AdaByron.Domain.Aggregates.ReservationAggregate;

using AdaByron.Domain.Exceptions;

/// <summary>
/// Franja horaria de una reserva: de Inicio a Fin (HU-15).
/// </summary>
public record FranjaHoraria
{
    public DateTime Inicio { get; }
    public DateTime Fin    { get; }

    // Requerido por EF Core (HU-15)
    private FranjaHoraria() { }

    public FranjaHoraria(DateTime inicio, DateTime fin)
    {
        if (inicio >= fin)
            throw new ExcepcionDominio("La hora de inicio debe ser anterior a la de fin.");
        
        Inicio = inicio;
        Fin    = fin;
    }

    public bool Overlaps(FranjaHoraria other) =>
        Inicio < other.Fin && Fin > other.Inicio;

    public override string ToString() => $"({Inicio:HH:mm} - {Fin:HH:mm})";
}
