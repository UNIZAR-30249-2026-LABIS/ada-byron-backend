using AdaByron.Application.DTOs;
using AdaByron.Application.Ports.Out;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Interfaces;

namespace AdaByron.Application.UseCases.Auth;

public class LoginUseCase(IPersonaRepository personas, ITokenService tokenService)
{
    public async Task<AuthResponseDTO> ExecuteAsync(LoginRequestDTO request)
    {
        var persona = await personas.GetByEmailAsync(request.Email);

        if (persona is null)
            throw new ExcepcionUsuarioNoRegistrado(request.Email);

        var token = tokenService.GenerarToken(persona);

        return new AuthResponseDTO(
            Token:          token,
            Email:          persona.Email,
            NombreCompleto: persona.NombreCompleto,
            Rol:            persona.Rol);
    }
}
