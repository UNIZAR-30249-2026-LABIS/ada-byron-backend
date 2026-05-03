using AdaByron.Application.DTOs;
using AdaByron.Application.UseCases.Reservas;
using AdaByron.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdaByron.API.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationsController(CrearReservaUseCase crearReservaUseCase) : ControllerBase
{
    /// <summary>
    /// Crea una reserva para el usuario autenticado (HU-10).
    /// El email se extrae del JWT; el cliente no puede suplantar a otro usuario.
    /// Valida las Reglas F1-F6 y la incompatibilidad horaria de forma atómica (HU-T2).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ReservaResponseDTO>> Create([FromBody] CrearReservaRequestDTO request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)
                 ?? User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
                 ?? User.Identity?.Name;

        if (string.IsNullOrWhiteSpace(email))
            return Unauthorized("No se pudo identificar al usuario desde el token.");

        var result = await crearReservaUseCase.ExecuteAsync(email, request);
        return Ok(result);
    }
}
