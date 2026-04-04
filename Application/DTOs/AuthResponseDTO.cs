using AdaByron.Domain.Aggregates.PersonAggregate; using AdaByron.Domain.Aggregates.SpaceAggregate; using AdaByron.Domain.Aggregates.ReservationAggregate;

namespace AdaByron.Application.DTOs;

public record AuthResponseDTO(
    string Token,
    string Email,
    string NombreCompleto,
    Rol    Rol);
