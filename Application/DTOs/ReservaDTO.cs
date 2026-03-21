namespace AdaByron.Application.DTOs;

public record CrearReservaRequestDTO(
    string Email,
    string CodigoEspacio,
    DateTime Inicio,
    DateTime Fin,
    int NumeroAsistentes);

public record ReservaResponseDTO(
    Guid   Id,
    string Email,
    string CodigoEspacio,
    DateTime Inicio,
    DateTime Fin,
    int    NumeroAsistentes,
    string Estado);
