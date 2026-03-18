using AdaByron.Domain.Enums;

namespace AdaByron.Application.DTOs;

public record AuthResponseDTO(
    string Token,
    string Email,
    string NombreCompleto,
    Rol    Rol);
