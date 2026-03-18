namespace AdaByron.Domain.Enums;

// Estados del ciclo de vida de una reserva.
public enum EstadoReserva
{
    Pendiente,   // Esperando revisión del Gerente
    Aprobada,    // Confirmada y activa
    Rechazada,   // Denegada con motivo
    Invalida,    // Marcada por cambio de configuración del espacio (se elimina en 7 días)
}
