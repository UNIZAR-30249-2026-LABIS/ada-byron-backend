namespace AdaByron.Domain.Entities;

// TODO: Reservation — entidad con identidad en el Aggregate ReservationAggregate
// Propiedades: Id, RequesterEmail, SpaceId, StartTime, EndTime,
//              AttendeeCount, Status (ReservationStatus), CreatedAt, InvalidSince?
// Invariant: AttendeeCount <= AforoEfectivo en el momento de crear/modificar
