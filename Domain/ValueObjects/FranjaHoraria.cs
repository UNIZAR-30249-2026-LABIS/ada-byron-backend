using AdaByron.Domain.Exceptions;

namespace AdaByron.Domain.ValueObjects;

// Intervalo temporal inmutable para una reserva. Valida que el fin sea posterior al inicio
// y que la duración no supere 24 horas.
public sealed record FranjaHoraria
{
    private static readonly TimeSpan DuracionMaxima = TimeSpan.FromHours(24);

    public DateTime Inicio   { get; }
    public DateTime Fin      { get; }
    public TimeSpan Duracion => Fin - Inicio;

    public FranjaHoraria(DateTime inicio, DateTime fin)
    {
        if (fin <= inicio)
            throw new ExcepcionDominio(
                $"La hora de fin debe ser posterior a la de inicio. " +
                $"Inicio: {inicio:g} — Fin: {fin:g}.");

        if (fin - inicio > DuracionMaxima)
            throw new ExcepcionDominio(
                $"Un intervalo no puede superar las 24 horas. " +
                $"Duración solicitada: {(fin - inicio).TotalHours:F1} h.");

        Inicio = inicio;
        Fin    = fin;
    }

    public static FranjaHoraria De(DateTime inicio, DateTime fin) => new(inicio, fin);

    // Comprueba si dos franjas horarias se solapan (para detectar conflictos de reserva).
    public bool SeSolapaCon(FranjaHoraria otra) =>
        Inicio < otra.Fin && Fin > otra.Inicio;

    public override string ToString() =>
        $"{Inicio:dd/MM/yyyy HH:mm} – {Fin:HH:mm} ({Duracion.TotalHours:F1} h)";
}
