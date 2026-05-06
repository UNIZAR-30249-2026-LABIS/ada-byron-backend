namespace AdaByron.Application.DTOs;

public record CrearReservaRequestDTO(
    string CodigoEspacio,
    DateTime Inicio,
    DateTime Fin,
    int NumeroAsistentes,
    /// <summary>Tipo de uso: Docencia | Investigacion | Gestion | Otros (opcional).</summary>
    string? TipoUso = null,
    /// <summary>Descripción libre del propósito de la reserva (máx. 500 caracteres, opcional).</summary>
    string? Descripcion = null);

public record ReservaResponseDTO(
    Guid   Id,
    string Email,
    string CodigoEspacio,
    DateTime Inicio,
    DateTime Fin,
    int    NumeroAsistentes,
    string Estado,
    string? TipoUso = null,
    string? Descripcion = null);
