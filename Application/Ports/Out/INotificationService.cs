namespace AdaByron.Application.Ports.Out;

/// <summary>
/// Puerto de salida para el envío de notificaciones en tiempo real (SignalR).
/// </summary>
public interface INotificationService
{
    Task NotifyReservationRescindedAsync(Guid id, string emailPersona, string codigoEspacio);
}
