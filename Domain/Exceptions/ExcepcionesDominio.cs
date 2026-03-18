namespace AdaByron.Domain.Exceptions;

// Excepción base para cualquier violación de regla de negocio del dominio.
public class ExcepcionDominio : Exception
{
    public ExcepcionDominio(string mensaje) : base(mensaje) { }
}

// Rol del usuario no permite reservar ese tipo de espacio → HTTP 403
public sealed class ExcepcionPermisos : ExcepcionDominio
{
    public ExcepcionPermisos(string mensaje) : base(mensaje) { }
}

// Reserva solapa con otra ya activa en el mismo espacio → HTTP 409
public sealed class ExcepcionConflictoReserva : ExcepcionDominio
{
    public ExcepcionConflictoReserva(string mensaje) : base(mensaje) { }
}

// Número de asistentes supera el aforo efectivo del espacio → HTTP 422
public sealed class ExcepcionAforoSuperado : ExcepcionDominio
{
    public ExcepcionAforoSuperado(string mensaje) : base(mensaje) { }
}

// Cambio de categoría no permitido por la Matriz de Mutabilidad (Regla C) → HTTP 422
public sealed class ExcepcionCambioCategoria : ExcepcionDominio
{
    public ExcepcionCambioCategoria(string mensaje) : base(mensaje) { }
}

// Email no existe en la BD — acceso denegado → HTTP 401
public sealed class ExcepcionUsuarioNoRegistrado : ExcepcionDominio
{
    public ExcepcionUsuarioNoRegistrado(string email)
        : base($"El usuario '{email}' no está registrado en el sistema.") { }
}
