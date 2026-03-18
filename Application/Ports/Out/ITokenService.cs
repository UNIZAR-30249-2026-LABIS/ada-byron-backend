using AdaByron.Domain.Entities;

namespace AdaByron.Application.Ports.Out;

// Puerto de salida para la generación de tokens JWT.
// La implementación vive en Infrastructure/Identity/TokenService.
public interface ITokenService
{
    string GenerarToken(Persona persona);
}
