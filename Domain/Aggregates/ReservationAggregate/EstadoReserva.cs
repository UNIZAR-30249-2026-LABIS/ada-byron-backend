namespace AdaByron.Domain.Aggregates.ReservationAggregate;

public enum EstadoReserva
{
    Pendiente,
    Aceptada,
    Rechazada,
    PotencialmenteInvalida,
    /// <summary>Reserva rescindida por el propio propietario (HU-18).</summary>
    Rescindida
}
