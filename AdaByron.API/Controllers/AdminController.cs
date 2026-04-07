using AdaByron.Application.DTOs;
using AdaByron.Application.UseCases.Admin;
using AdaByron.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdaByron.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Gerente")]
public class AdminController(UpdateBuildingConfigUseCase updateConfigUseCase) : ControllerBase
{
    /// <summary>
    /// Endpoint para modificar el porcentaje de aforo del edificio dinámicamente (PBI 6).
    /// </summary>
    /// <param name="request">Ocupación deseada entre 0 y 100.</param>
    /// <response code="200">El porcentaje se actualizó correctamente.</response>
    /// <response code="400">El valor infringe reglas de dominio.</response>
    [HttpPut("config")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateConfig([FromBody] UpdateConfigDTO request)
    {
        try 
        {
            await updateConfigUseCase.ExecuteAsync(request);
            return Ok(new { PorcentajeOcupacion = request.PorcentajeOcupacion });
        }
        catch (ExcepcionDominio ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Lista de reservas activas para la supervisión del administrador (HU-17).
    /// </summary>
    [HttpGet("reservations/live")]
    [ProducesResponseType(typeof(IEnumerable<LiveReservationDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLiveReservations([FromServices] AdaByron.Application.UseCases.Reservations.GetLiveReservationsUseCase getLiveReservationsUseCase)
    {
        var result = await getLiveReservationsUseCase.ExecuteAsync();
        return Ok(result);
    }

    /// <summary>
    /// Elimina una reserva del sistema y notifica al usuario (HU-18).
    /// </summary>
    [HttpDelete("reservations/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteReservation(Guid id, [FromServices] AdaByron.Application.UseCases.Reservations.DeleteReservationUseCase deleteUseCase)
    {
        try
        {
            await deleteUseCase.ExecuteAsync(id);
            return NoContent();
        }
        catch (ExcepcionDominio ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
