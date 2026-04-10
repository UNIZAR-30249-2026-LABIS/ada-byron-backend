using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace AdaByron.Infrastructure.Realtime;

/// <summary>
/// Hub de SignalR para gestionar notificaciones push personalizadas (HU-18).
/// Reubicado en Infrastructure para evitar dependencias circulares.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
}
