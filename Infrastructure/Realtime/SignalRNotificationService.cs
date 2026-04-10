using AdaByron.Application.Ports.Out;
using Microsoft.AspNetCore.SignalR;

namespace AdaByron.Infrastructure.Realtime;

/// <summary>
/// Implementación del puerto de salida para notificaciones mediante SignalR Hub específico (HU-18).
/// </summary>
public class SignalRNotificationService(IHubContext<NotificationHub> hubContext) : INotificationService
{
    public async Task NotifyReservationRescindedAsync(Guid id, string emailPersona, string codigoEspacio)
    {
        // 1. Notificación PRIVADA al usuario afectado (HU-18)
        await hubContext.Clients.User(emailPersona).SendAsync("ReservaAnulada", new {
            ReservaId = id,
            Espacio = codigoEspacio,
            Mensaje = "Tu reserva ha sido anulada por un administrador del centro."
        });

        // 2. Notificación GENERAL para el Panel de Supervisión (opcional, para feedback del Admin)
        await hubContext.Clients.All.SendAsync("UpdateDashboard", new {
            ReservaId = id,
            Accion = "Anulacion"
        });
    }
}
