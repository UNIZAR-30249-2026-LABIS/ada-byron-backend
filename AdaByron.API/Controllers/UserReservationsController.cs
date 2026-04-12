using AdaByron.Application.DTOs;
using AdaByron.Application.UseCases.Reservas;
using AdaByron.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdaByron.API.Controllers;

/// <summary>
/// Endpoints de reservas del usuario autenticado (HU-18).
/// Ruta canónica: DELETE /api/users/me/reservations/{id}/cancel
/// </summary>
[ApiController]
[Route("api/users/me/reservations")]
[Authorize]
public class UserReservationsController(
    GetMisReservasUseCase getMisReservasUseCase,
    CancelarReservaUseCase cancelarReservaUseCase) : ControllerBase
{
    private string GetEmailFromToken()
    {
        // El email está en el claim estándar de email del JWT
        var email = User.FindFirstValue(ClaimTypes.Email)
                 ?? User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
                 ?? User.Identity?.Name;

        if (string.IsNullOrWhiteSpace(email))
            throw new UnauthorizedAccessException("No se pudo identificar al usuario desde el token.");

        return email;
    }

    /// <summary>
    /// Devuelve todas las reservas del usuario autenticado, ordenadas de más recientes a más antiguas.
    /// </summary>
    /// <response code="200">Lista de reservas del usuario.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MiReservaDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyReservations()
    {
        var email = GetEmailFromToken();
        var result = await getMisReservasUseCase.ExecuteAsync(email);
        return Ok(result);
    }

    /// <summary>
    /// Cancela (rescinde) una reserva por iniciativa del propio propietario (HU-18).
    /// No se puede cancelar si: la reserva no pertenece al usuario, ya está rescindida,
    /// está rechazada o la franja horaria ya ha comenzado.
    /// </summary>
    /// <param name="id">Identificador único de la reserva a cancelar.</param>
    /// <response code="200">La reserva fue cancelada con éxito; devuelve el DTO actualizado.</response>
    /// <response code="400">Violación de invariante de dominio (ya cancelada, en curso, etc.).</response>
    /// <response code="403">El usuario no es el propietario de la reserva.</response>
    [HttpDelete("{id:guid}/cancel")]
    [ProducesResponseType(typeof(MiReservaDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelReservation(Guid id)
    {
        try
        {
            var email = GetEmailFromToken();
            var result = await cancelarReservaUseCase.ExecuteAsync(id, email);
            return Ok(result);
        }
        catch (ExcepcionPermisos ex)
        {
            return Forbid(ex.Message);
        }
        catch (ExcepcionDominio ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
