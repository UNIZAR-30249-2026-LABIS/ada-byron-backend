using AdaByron.Application.Ports.Out;
using AdaByron.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AdaByron.Infrastructure.Realtime;

/// <summary>
/// Implementación del puerto de salida para notificaciones mediante SignalR Hub.
/// Envía eventos push a los clientes conectados (React) (HU-18).
/// </summary>
public class SignalRNotificationService(IHubContext<ReservasHub> hubContext) : INotificationService
{
    public async Task NotifyReservationRescindedAsync(Guid id, string emailPersona, string codigoEspacio)
    {
        // En un sistema real, podríamos filtrar por ClienteId si el usuario se registra en un grupo
        // Por ahora, emitimos a todos (broadcast) para el panel de supervisión
        await hubContext.Clients.All.SendAsync("ReservaAnulada", new {
            ReservaId = id,
            Email = emailPersona,
            Espacio = codigoEspacio,
            Mensaje = "Tu reserva ha sido anulada por un administrador del centro."
        });
    }
}
