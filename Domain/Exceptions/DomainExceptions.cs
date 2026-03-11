namespace AdaByron.Domain.Exceptions;

// TODO: Excepciones de dominio — extendidas por la capa API para convertirlas en HTTP responses

// ReservationConflictException → 409 Conflict
//   Lanzada cuando se intenta reservar un espacio ya ocupado en ese tramo horario

// PermissionDeniedException → 403 Forbidden
//   Lanzada por PermissionPolicy cuando el rol del usuario no permite reservar ese tipo de espacio

// OccupancyLimitException → 422 Unprocessable Entity
//   Lanzada por OccupancyPolicy cuando el número de asistentes supera el aforo efectivo

// ReservationNotFoundException → 404 Not Found
//   Lanzada cuando se intenta operar sobre una reserva inexistente
