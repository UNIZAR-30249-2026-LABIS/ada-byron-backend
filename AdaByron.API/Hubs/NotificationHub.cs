using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace AdaByron.API.Hubs;

/// <summary>
/// Hub de SignalR para gestionar notificaciones push personalizadas (HU-18).
/// Permite el envío de mensajes a usuarios específicos identificados por su JWT.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    // Las conexiones se gestionan automáticamente por SignalR.
    // Usamos el 'Name' del Identity (mapeado desde el Claim Email en nuestro caso)
    // para dirigir las notificaciones a usuarios concretos.
}
