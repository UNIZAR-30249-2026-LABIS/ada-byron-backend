using AdaByron.Domain.Exceptions;

namespace AdaByron.Domain.Entities;

// Reserva de un espacio, identificada por un Guid.
// La duración debe ser positiva y no superar 24 horas.
public sealed class Reserva
{
    private static readonly TimeSpan DuracionMaxima = TimeSpan.FromHours(24);

    public Guid     Id          { get; }
    public string   PersonaId   { get; }   // Email de la persona
    public string   EspacioId   { get; }   // CodigoEspacio del espacio
    public DateTime FechaInicio { get; }
    public DateTime FechaFin    { get; }
    public TimeSpan Duracion    => FechaFin - FechaInicio;

    public Reserva(string personaId, string espacioId, DateTime fechaInicio, DateTime fechaFin)
    {
        if (string.IsNullOrWhiteSpace(personaId))
            throw new ExcepcionDominio("El identificador de persona (email) no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(espacioId))
            throw new ExcepcionDominio("El identificador de espacio no puede estar vacío.");

        if (fechaFin <= fechaInicio)
            throw new ExcepcionDominio(
                $"La fecha de fin debe ser posterior a la de inicio. " +
                $"Inicio: {fechaInicio:g} — Fin: {fechaFin:g}.");

        var duracion = fechaFin - fechaInicio;
        if (duracion > DuracionMaxima)
            throw new ExcepcionDominio(
                $"Una reserva no puede superar las 24 horas. " +
                $"Duración solicitada: {duracion.TotalHours:F1} h.");

        Id          = Guid.NewGuid();
        PersonaId   = personaId.Trim().ToLowerInvariant();
        EspacioId   = espacioId.Trim().ToUpperInvariant();
        FechaInicio = fechaInicio;
        FechaFin    = fechaFin;
    }

    // Constructor privado para rehidratar desde base de datos sin reejecutar validaciones.
    private Reserva(Guid id, string personaId, string espacioId, DateTime fechaInicio, DateTime fechaFin)
    {
        Id          = id;
        PersonaId   = personaId;
        EspacioId   = espacioId;
        FechaInicio = fechaInicio;
        FechaFin    = fechaFin;
    }

    // Usado por ReservaFactory para reconstituir desde BD.
    public static Reserva Reconstituir(Guid id, string personaId, string espacioId,
                                       DateTime fechaInicio, DateTime fechaFin)
        => new(id, personaId, espacioId, fechaInicio, fechaFin);

    public override bool Equals(object? obj) =>
        obj is Reserva otra && Id == otra.Id;

    public override int GetHashCode() => Id.GetHashCode();

    public override string ToString() =>
        $"Reserva({Id}, Espacio={EspacioId}, {FechaInicio:g}–{FechaFin:g}, {Duracion.TotalHours:F1}h)";
}
