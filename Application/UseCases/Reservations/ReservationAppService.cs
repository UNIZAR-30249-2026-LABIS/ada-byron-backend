namespace AdaByron.Application.Services;

// TODO: ReservationAppService — coordina objetos de dominio para los Casos de Uso
// Casos de uso orquestados:
//   - MakeReservation(requesterEmail, spaceId, start, end, attendees)
//   - GetActiveReservations() → IEnumerable<Reservation>
//   - ApproveReservation(reservationId)
//   - RejectReservation(reservationId, reason)
//   - PurgeInvalidReservationsAsync() → invocado por background job
