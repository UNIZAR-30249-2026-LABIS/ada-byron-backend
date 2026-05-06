using System.Diagnostics.CodeAnalysis;
namespace AdaByron.Domain.Aggregates.ReservationAggregate;

using AdaByron.Domain.Exceptions;

/// <summary>
/// Entidad Reserva (HU-15). Identifica una reserva de un Espacio por una Persona en una Franja.
/// Es gestionada como entidad dentro del Agregado Space (Aggregate Root).
/// </summary>
public sealed class Reserva
{
    public Guid           Id                { get; }
    public required string PersonaId         { get; init; }
    public required string EspacioId         { get; init; }
    public required FranjaHoraria  Franja            { get; init; }
    public int            NumeroAsistentes  { get; private set; }
    public EstadoReserva  Estado            { get; private set; }
    public DateTime       FechaEstadoModificado { get; private set; }
    public uint           Version           { get; private set; }

    /// <summary>Tipo de uso declarado por el solicitante (Sección E del enunciado).</summary>
    public TipoUso?       TipoUso           { get; private set; }

    /// <summary>Descripción o motivo libre de la reserva (Sección E del enunciado).</summary>
    public string?        Descripcion       { get; private set; }

    // Requerido por EF Core (HU-15)
    private Reserva() { }

    [SetsRequiredMembers]
    public Reserva(string personaId, string espacioId, FranjaHoraria franja, int numeroAsistentes,
                   TipoUso? tipoUso = null, string? descripcion = null)
    {
        if (string.IsNullOrWhiteSpace(personaId))
            throw new ExcepcionDominio("El identificador de persona no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(espacioId))
            throw new ExcepcionDominio("El identificador de espacio no puede estar vacío.");

        if (numeroAsistentes <= 0)
            throw new ExcepcionDominio("El número de asistentes debe ser positivo.");

        if (descripcion is { Length: > 500 })
            throw new ExcepcionDominio("La descripción no puede superar los 500 caracteres.");

        Id               = Guid.NewGuid();
        PersonaId        = personaId.Trim().ToLowerInvariant();
        EspacioId        = espacioId.Trim().ToUpperInvariant();
        Franja           = franja ?? throw new ExcepcionDominio("La franja horaria es obligatoria.");
        NumeroAsistentes = numeroAsistentes;
        TipoUso          = tipoUso;
        Descripcion      = descripcion?.Trim();
        Estado           = EstadoReserva.Pendiente;
        FechaEstadoModificado = DateTime.UtcNow;
    }

    // Constructor para reconstituir desde persistencia
    [SetsRequiredMembers]
    private Reserva(Guid id, string personaId, string espacioId, FranjaHoraria franja,
                    int numeroAsistentes, EstadoReserva estado, DateTime fechaEstadoModificado, uint version,
                    TipoUso? tipoUso, string? descripcion)
    {
        Id               = id;
        PersonaId        = personaId;
        EspacioId        = espacioId;
        Franja           = franja;
        NumeroAsistentes = numeroAsistentes;
        Estado           = estado;
        FechaEstadoModificado = fechaEstadoModificado;
        Version          = version;
        TipoUso          = tipoUso;
        Descripcion      = descripcion;
    }

    public static Reserva Reconstituir(Guid id, string personaId, string espacioId,
                                       FranjaHoraria franja, int numeroAsistentes, EstadoReserva estado,
                                       DateTime fechaEstadoModificado, uint version = 0,
                                       TipoUso? tipoUso = null, string? descripcion = null)
        => new(id, personaId, espacioId, franja, numeroAsistentes, estado, fechaEstadoModificado, version, tipoUso, descripcion);

    public void Aceptar()
    {
        if (Estado != EstadoReserva.Pendiente)
            throw new ExcepcionDominio($"Solo se puede aceptar una reserva en estado Pendiente (Actual: {Estado}).");
        Estado = EstadoReserva.Aceptada;
        FechaEstadoModificado = DateTime.UtcNow;
    }

    public void Rechazar()
    {
        if (Estado == EstadoReserva.Aceptada)
            throw new ExcepcionDominio("No se puede rechazar una reserva ya aceptada.");
        Estado = EstadoReserva.Rechazada;
        FechaEstadoModificado = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancela el propietario de la reserva (HU-18).
    /// Invariantes: no se puede cancelar si está Rescindida/Rechazada, ni si la franja ya ha comenzado.
    /// </summary>
    public void Cancelar()
    {
        if (Estado == EstadoReserva.Rescindida)
            throw new ExcepcionDominio("La reserva ya ha sido cancelada previamente.");
        if (Estado == EstadoReserva.Rechazada)
            throw new ExcepcionDominio("No se puede cancelar una reserva rechazada.");
        if (Franja.Inicio <= DateTime.UtcNow)
            throw new ExcepcionDominio("No se puede cancelar una reserva que ya ha comenzado o que pertenece al pasado.");
        Estado = EstadoReserva.Rescindida;
        FechaEstadoModificado = DateTime.UtcNow;
    }

    /// <summary>
    /// PBI-13 (HU-O4): Marca la reserva como PotencialmenteInvalida tras un cambio de aforo/porcentaje.
    /// Solo aplica a reservas Aceptadas (las Pendientes no han superado las reglas aún).
    /// </summary>
    public void MarcarComoPotencialmenteInvalida()
    {
        if (Estado != EstadoReserva.Aceptada)
            return; // Idempotente: si ya está en otro estado no se hace nada

        Estado = EstadoReserva.PotencialmenteInvalida;
        FechaEstadoModificado = DateTime.UtcNow;
    }

    /// <summary>
    /// PBI-13 (HU-O4): Cancelación administrativa forzada por el Gerente.
    /// Sin restricciones de tiempo; el gerente puede cancelar incluso reservas en curso.
    /// </summary>
    public void ForzarCancelacion()
    {
        if (Estado == EstadoReserva.Rescindida)
            throw new ExcepcionDominio("La reserva ya está cancelada.");

        Estado = EstadoReserva.Rescindida;
        FechaEstadoModificado = DateTime.UtcNow;
    }

    /// <summary>
    /// Restauración automática: cuando el aforo efectivo vuelve a ser suficiente para esta reserva
    /// (p.ej. el Gerente sube el % global), la reserva vuelve a Aceptada sin intervención manual.
    /// Solo aplica a reservas PotencialmenteInvalida con franja futura.
    /// </summary>
    public void RestablecerAceptada()
    {
        if (Estado != EstadoReserva.PotencialmenteInvalida)
            return; // Idempotente

        Estado = EstadoReserva.Aceptada;
        FechaEstadoModificado = DateTime.UtcNow;
    }

    /// <summary>
    /// PBI-13 (HU-O4): El Gerente admite una excepción y restaura la reserva a Aceptada.
    /// Solo es válido desde el estado PotencialmenteInvalida.
    /// </summary>
    public void AdmitirExcepcion()
    {
        if (Estado != EstadoReserva.PotencialmenteInvalida)
            throw new ExcepcionDominio(
                $"Solo se puede admitir una excepción en reservas PotencialmenteInvalida (Actual: {Estado}).");

        Estado = EstadoReserva.Aceptada;
        FechaEstadoModificado = DateTime.UtcNow;
    }

    public override bool Equals(object? obj) => obj is Reserva otra && Id == otra.Id;
    public override int GetHashCode()        => Id.GetHashCode();
}
