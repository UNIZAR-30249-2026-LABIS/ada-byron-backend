namespace AdaByron.Domain.Enums;

// Estado del ciclo de vida de una Reserva.
public enum EstadoReserva
{
    Pendiente,   // Creada, pendiente de aprobación
    Aceptada,    // Aprobada por el sistema o el gerente
    Rechazada,   // Denegada (solapamiento, permisos, aforo)
}
