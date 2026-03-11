namespace AdaByron.Domain.Aggregates;

// TODO: ReservationAggregate — Unidad de consistencia, raíz: Reservation
// Protege invariantes:
//   - Una reserva no puede solaparse con otra activa en el mismo espacio
//   - El aforo efectivo no puede superarse en el momento de confirmar
//   - Cambios de configuración de Space disparan InvalidSince en las reservas afectadas
