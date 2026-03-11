namespace AdaByron.API.Middleware;

// TODO: ExceptionHandlingMiddleware — captura excepciones de dominio y las convierte en
//       respuestas HTTP estándar (ProblemDetails / RFC 7807)
// Excepciones mapeadas:
//   - Domain.Exceptions.ReservationConflictException → 409 Conflict
//   - Domain.Exceptions.PermissionDeniedException    → 403 Forbidden
//   - Domain.Exceptions.OccupancyLimitException      → 422 Unprocessable Entity
