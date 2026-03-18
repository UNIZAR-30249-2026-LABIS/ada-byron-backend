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
    /// Autenticación passwordless: si el email existe en la BD, devuelve un JWT.
    /// </summary>
    [HttpPost("login")]
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
