namespace AdaByron.Infrastructure.Realtime;

// TODO: Implementación técnica de SignalR (adaptador de salida para notificaciones)
//
// SignalRNotificationService — implementa INotificationService (puerto de salida de Application)
//   Inyecta IHubContext<ReservationsHub> para emitir eventos a los clientes React:
//   - NotifyApprovedAsync(reservationId)    → hub.Clients.All.SendAsync("ReservationApproved", ...)
//   - NotifyRejectedAsync(reservationId, reason) → "ReservationRejected"
//   - NotifyInvalidatedAsync(reservationId) → "ReservationInvalidated"
