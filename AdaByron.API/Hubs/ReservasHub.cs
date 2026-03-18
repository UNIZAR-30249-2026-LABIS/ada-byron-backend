using Microsoft.AspNetCore.SignalR;

namespace AdaByron.API.Hubs;

// Hub SignalR para notificaciones en tiempo real a los clientes React.
// Los eventos se emiten desde Infrastructure/Realtime/SignalRNotificationService.
public class ReservasHub : Hub
{
    // El servidor emite, los clientes escuchan — no hay métodos invocables por el cliente.
    // Eventos push:
    //   "ReservaAprobada"    → { reservaId }
    //   "ReservaRechazada"   → { reservaId, motivo }
    //   "ReservaInvalidada"  → { reservaId }
    //   "AforoActualizado"   → { espacioId, porcentaje }
}
