using AdaByron.Domain.Enums;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.ValueObjects;

namespace AdaByron.Domain.Entities;

// Reserva de un espacio, identificada por un Guid.
// Incorpora EstadoReserva (Pendiente → Aceptada | Rechazada) y NumeroAsistentes para la Regla F5.
public sealed class Reserva
{
    public Guid           Id                { get; }
    public string         PersonaId         { get; }       // Email de la persona
    public string         EspacioId         { get; }       // CodigoEspacio del espacio
    public FranjaHoraria  Franja            { get; }
    public int            NumeroAsistentes  { get; }
    public EstadoReserva  Estado            { get; private set; }

    public Reserva(string personaId, string espacioId, FranjaHoraria franja, int numeroAsistentes)
    {
        if (string.IsNullOrWhiteSpace(personaId))
            throw new ExcepcionDominio("El identificador de persona (email) no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(espacioId))
            throw new ExcepcionDominio("El identificador de espacio no puede estar vacío.");

        if (numeroAsistentes <= 0)
            throw new ExcepcionDominio("El número de asistentes debe ser mayor que cero.");

        Id               = Guid.NewGuid();
        PersonaId        = personaId.Trim().ToLowerInvariant();
        EspacioId        = espacioId.Trim().ToUpperInvariant();
        Franja           = franja ?? throw new ExcepcionDominio("La franja horaria no puede ser nula.");
        NumeroAsistentes = numeroAsistentes;
        Estado           = EstadoReserva.Pendiente;
    }

    private Reserva() 
    { 
        PersonaId = null!;
        EspacioId = null!;
        Franja    = null!;
    }

    // Constructor para reconstituir desde BD (sin reejecutar invariantes)
    private Reserva(Guid id, string personaId, string espacioId, FranjaHoraria franja,
                    int numeroAsistentes, EstadoReserva estado)
    {
        Id               = id;
        PersonaId        = personaId;
        EspacioId        = espacioId;
        Franja           = franja;
        NumeroAsistentes = numeroAsistentes;
        Estado           = estado;
    }

    public static Reserva Reconstituir(Guid id, string personaId, string espacioId,
                                       FranjaHoraria franja, int numeroAsistentes, EstadoReserva estado)
        => new(id, personaId, espacioId, franja, numeroAsistentes, estado);

    // Retro-compatibilidad: reconstituir sin asistentes ni estado (para migraciones)
    public static Reserva Reconstituir(Guid id, string personaId, string espacioId, FranjaHoraria franja)
        => new(id, personaId, espacioId, franja, 1, EstadoReserva.Aceptada);

    // ── Ciclo de vida ──────────────────────────────────────────────────────────
    public void Aceptar()
    {
        if (Estado != EstadoReserva.Pendiente)
            throw new ExcepcionDominio($"Solo se puede aceptar una reserva en estado Pendiente. Estado actual: {Estado}.");
        Estado = EstadoReserva.Aceptada;
    }

    public void Rechazar()
    {
        if (Estado == EstadoReserva.Aceptada)
            throw new ExcepcionDominio("No se puede rechazar una reserva ya aceptada.");
        Estado = EstadoReserva.Rechazada;
    }

    public override bool Equals(object? obj) => obj is Reserva otra && Id == otra.Id;
    public override int GetHashCode()        => Id.GetHashCode();
    public override string ToString()        => $"Reserva({Id}, Espacio={EspacioId}, {Franja}, {Estado})";
}
