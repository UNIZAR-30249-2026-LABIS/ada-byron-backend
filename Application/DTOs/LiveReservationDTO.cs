namespace AdaByron.Application.DTOs;

public record LiveReservationDTO(
    Guid Id,
    string EspacioId,
    string NombreEspacio,
    string Solicitante,
    DateTime Inicio,
    DateTime Fin,
    string Estado,
    bool EsPotencialmenteInvalida
);
