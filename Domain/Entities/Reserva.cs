using AdaByron.Domain.Exceptions;
using AdaByron.Domain.ValueObjects;

namespace AdaByron.Domain.Entities;

// Reserva de un espacio, identificada por un Guid.
public sealed class Reserva
{
    public Guid          Id        { get; }
    public string        PersonaId { get; }       // Email de la persona
    public string        EspacioId { get; }       // CodigoEspacio del espacio
    public FranjaHoraria Franja    { get; }       // Inicio, Fin y Duracion encapsulados

    public Reserva(string personaId, string espacioId, FranjaHoraria franja)
    {
        if (string.IsNullOrWhiteSpace(personaId))
            throw new ExcepcionDominio("El identificador de persona (email) no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(espacioId))
            throw new ExcepcionDominio("El identificador de espacio no puede estar vacío.");

        Id        = Guid.NewGuid();
        PersonaId = personaId.Trim().ToLowerInvariant();
        EspacioId = espacioId.Trim().ToUpperInvariant();
        Franja    = franja ?? throw new ExcepcionDominio("La franja horaria no puede ser nula.");
    }

    private Reserva() { }
    
    // Constructor privado para rehidratar desde base de datos sin reejecutar validaciones.
    private Reserva(Guid id, string personaId, string espacioId, FranjaHoraria franja)
    {
        Id        = id;
        PersonaId = personaId;
        EspacioId = espacioId;
        Franja    = franja;
    }

    // Usado por FabricaReserva para reconstituir desde BD.
    public static Reserva Reconstituir(Guid id, string personaId, string espacioId, FranjaHoraria franja)
        => new(id, personaId, espacioId, franja);

    public override bool Equals(object? obj) =>
        obj is Reserva otra && Id == otra.Id;

    public override int GetHashCode() => Id.GetHashCode();

    public override string ToString() =>
        $"Reserva({Id}, Espacio={EspacioId}, {Franja})";
}
