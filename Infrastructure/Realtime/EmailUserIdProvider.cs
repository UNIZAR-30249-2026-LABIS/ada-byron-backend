using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace AdaByron.Infrastructure.Realtime;

/// <summary>
/// Proveedor de ID de usuario para SignalR.
/// El JWT de este proyecto usa ClaimTypes.Email como identificador primario
/// (no incluye ClaimTypes.NameIdentifier), por lo que el DefaultUserIdProvider
/// devuelve null y Clients.User(email) nunca enruta ningún mensaje.
/// Este provider lo corrige leyendo explícitamente el claim de email.
/// </summary>
public class EmailUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
        => connection.User?.FindFirst(ClaimTypes.Email)?.Value;
}
