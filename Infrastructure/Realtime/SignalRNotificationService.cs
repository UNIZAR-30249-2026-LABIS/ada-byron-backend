using AdaByron.Application.Ports.Out;
using AdaByron.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AdaByron.Infrastructure.Realtime;

/// <summary>
/// Implementación del puerto de salida para notificaciones mediante SignalR Hub específico (HU-18).
/// </summary>
public class SignalRNotificationService(IHubContext<NotificationHub> hubContext) : INotificationService
{
    public async Task NotifyReservationRescindedAsync(Guid id, string emailPersona, string codigoEspacio)
    {
        // Enviamos la notificación SOLO al usuario cuya reserva ha sido cancelada
        // SignalR usará el NameClaimType (Email) configurado en Program.cs para encontrar la conexión.
        await hubContext.Clients.User(emailPersona).SendAsync("ReservaAnulada", new {
            ReservaId = id,
            Espacio = codigoEspacio,
            Mensaje = "Tu reserva ha sido anulada por un administrador del centro."
        });
    }
}
