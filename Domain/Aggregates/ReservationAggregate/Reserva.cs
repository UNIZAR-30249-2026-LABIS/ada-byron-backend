namespace AdaByron.Domain.Aggregates.ReservationAggregate;

using AdaByron.Domain.Exceptions;

/// <summary>
/// Entidad Reserva (HU-15). Identifica una reserva de un Espacio por una Persona en una Franja.
/// Es gestionada como entidad dentro del Agregado Space (Aggregate Root).
/// </summary>
public sealed class Reserva
{
    public Guid           Id                { get; }
    public string         PersonaId         { get; private set; }
    public string         EspacioId         { get; private set; }
    public FranjaHoraria  Franja            { get; private set; }
    public int            NumeroAsistentes  { get; private set; }
    public EstadoReserva  Estado            { get; private set; }

    // Requerido por EF Core (HU-15)
    private Reserva() { }

    public Reserva(string personaId, string espacioId, FranjaHoraria franja, int numeroAsistentes)
    {
        if (string.IsNullOrWhiteSpace(personaId))
            throw new ExcepcionDominio("El identificador de persona no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(espacioId))
            throw new ExcepcionDominio("El identificador de espacio no puede estar vacío.");

        if (numeroAsistentes <= 0)
            throw new ExcepcionDominio("El número de asistentes debe ser positivo.");

        Id               = Guid.NewGuid();
        PersonaId        = personaId.Trim().ToLowerInvariant();
        EspacioId        = espacioId.Trim().ToUpperInvariant();
        Franja           = franja ?? throw new ExcepcionDominio("La franja horaria es obligatoria.");
        NumeroAsistentes = numeroAsistentes;
        Estado           = EstadoReserva.Pendiente;
    }

    // Constructor para reconstituir desde persistencia
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

    public void Aceptar()
    {
        if (Estado != EstadoReserva.Pendiente)
            throw new ExcepcionDominio($"Solo se puede aceptar una reserva en estado Pendiente (Actual: {Estado}).");
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
}
