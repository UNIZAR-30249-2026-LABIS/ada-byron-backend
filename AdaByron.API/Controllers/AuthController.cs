using AdaByron.Application.DTOs;
using AdaByron.Application.UseCases.Auth;
using AdaByron.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AdaByron.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(LoginUseCase loginUseCase) : ControllerBase
{
    /// <summary>
    /// Autenticación passwordless: si el email existe en la BD, devuelve un JWT firmado con los Roles y Departamentos (HU-02).
    /// </summary>
    /// <param name="request">Email del usuario registrado.</param>
    /// <response code="200">Devuelve un JWT válido de Sesión.</response>
    /// <response code="401">Usuario no encontrado o dado de baja.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
    {
        try
        {
            var respuesta = await loginUseCase.ExecuteAsync(request);
            return Ok(respuesta);
        }
        catch (ExcepcionUsuarioNoRegistrado)
        {
            return Unauthorized("El usuario no está registrado en el sistema.");
        }
    }
}
