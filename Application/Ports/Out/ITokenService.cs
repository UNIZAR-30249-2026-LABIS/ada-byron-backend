using AdaByron.Domain.Aggregates.PersonAggregate; using AdaByron.Domain.Aggregates.SpaceAggregate; using AdaByron.Domain.Aggregates.ReservationAggregate;

namespace AdaByron.Application.Ports.Out;

// Puerto de salida para la generación de tokens JWT.
// La implementación vive en Infrastructure/Identity/TokenService.
public interface ITokenService
{
    string GenerarToken(Persona persona);
}
