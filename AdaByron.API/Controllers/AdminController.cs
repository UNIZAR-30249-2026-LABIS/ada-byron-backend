using AdaByron.Application.DTOs;
using AdaByron.Application.UseCases.Admin;
using AdaByron.Application.UseCases.Reservations;
using AdaByron.Application.UseCases.Spaces;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdaByron.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Gerente")]
public class AdminController(
    UpdateBuildingConfigUseCase updateConfigUseCase,
    UpdateSpaceUseCase updateSpaceUseCase,
    GetLiveReservationsUseCase getLiveReservationsUseCase,
    IEdificioConfigRepository configRepo) : ControllerBase
{
    /// <summary>
    /// Devuelve la configuración global actual del edificio (porcentaje de aforo).
    /// </summary>
    [HttpGet("config")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConfig()
    {
        var config = await configRepo.GetConfigAsync();
        return Ok(new { PorcentajeOcupacion = config?.PorcentajeOcupacion ?? 100.0 });
    }

    /// <summary>
    /// Endpoint para modificar el porcentaje de aforo del edificio dinámicamente (PBI 6).
    /// </summary>
    [HttpPut("config")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateConfig([FromBody] UpdateConfigDTO request)
    {
        try
        {
            int marcadas = await updateConfigUseCase.ExecuteAsync(request);
            return Ok(new { PorcentajeOcupacion = request.PorcentajeOcupacion, ReservasMarcadasComoInvalidas = marcadas });
        }
        catch (ExcepcionDominio ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Actualiza un espacio (nombre, aforo, planta, categoría, horario, asignación HU-09).
    /// Delega en UpdateSpaceUseCase para garantizar la reevaluación de reservas (PBI-13).
    /// </summary>
    [HttpPut("spaces/{id}")]
    public async Task<IActionResult> UpdateSpace(string id, [FromBody] UpdateSpaceRequestDTO dto)
    {
        try
        {
            await updateSpaceUseCase.ExecuteAsync(id, dto);
            return Ok();
        }
        catch (ExcepcionDominio ex)    { return BadRequest(ex.Message); }
        catch (ArgumentException ex)   { return BadRequest(ex.Message); }
    }

    /// <summary>
    /// PBI-12 (HU-O1) + PBI-13 (HU-O4): Define o elimina el porcentaje de uso específico de un espacio.
    /// Tras el cambio, marca como PotencialmenteInvalida las reservas que ya no cumplen el aforo efectivo.
    /// PATCH /api/admin/spaces/{id}/aforo-especifico
    /// </summary>
    [HttpPatch("spaces/{id}/aforo-especifico")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetSpaceOccupancy(
        string id,
        [FromBody] SetSpaceOccupancyDTO dto,
        [FromServices] SetSpaceOccupancyUseCase setOccupancyUseCase,
        [FromServices] AdaByron.Domain.Interfaces.IEspacioRepository espacios)
    {
        try
        {
            int reservasMarcadas = await setOccupancyUseCase.ExecuteAsync(id, dto);

            var espacio = await espacios.GetByCodigoAsync(id);
            return Ok(new
            {
                codigoEspacio = id,
                porcentajeEspecifico = espacio!.PorcentajeOcupacionEspecifico,
                esHeredado = !espacio.PorcentajeOcupacionEspecifico.HasValue,
                reservasMarcadasComoInvalidas = reservasMarcadas,
                mensaje = espacio.PorcentajeOcupacionEspecifico.HasValue
                    ? $"Porcentaje específico del {espacio.PorcentajeOcupacionEspecifico}% aplicado. {reservasMarcadas} reservas marcadas como PotencialmenteInvalida."
                    : $"Porcentaje eliminado. El espacio hereda el % global. {reservasMarcadas} reservas marcadas."
            });
        }
        catch (ExcepcionDominio ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// PBI-13 (HU-O4): El Gerente fuerza la cancelación de una reserva (incluso en curso).
    /// POST /api/admin/reservations/{id}/force-cancel
    /// </summary>
    [HttpPost("reservations/{id}/force-cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ForceCancel(
        Guid id,
        [FromServices] AdaByron.Application.UseCases.Reservations.ForceCancelReservationUseCase useCase)
    {
        try
        {
            await useCase.ExecuteAsync(id);
            return NoContent();
        }
        catch (AdaByron.Domain.Exceptions.ExcepcionDominio ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// PBI-13 (HU-O4): El Gerente admite una excepción, restaurando la reserva a Aceptada.
    /// POST /api/admin/reservations/{id}/approve-exception
    /// </summary>
    [HttpPost("reservations/{id}/approve-exception")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveException(
        Guid id,
        [FromServices] AdaByron.Application.UseCases.Reservations.ApproveReservationExceptionUseCase useCase)
    {
        try
        {
            await useCase.ExecuteAsync(id);
            return NoContent();
        }
        catch (AdaByron.Domain.Exceptions.ExcepcionDominio ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// PBI-14: Endpoint de testing para forzar la ejecución manual del Background Service de limpieza.
    /// POST /api/admin/reservations/trigger-cleanup
    /// </summary>
    [HttpPost("reservations/trigger-cleanup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TriggerCleanup(
        [FromServices] AdaByron.Application.UseCases.Reservations.CleanExpiredReservationsUseCase useCase,
        [FromQuery] int days = 7)
    {
        var count = await useCase.ExecuteAsync(days);
        return Ok(new { message = $"Limpieza manual ejecutada.", canceladas = count });
    }

    [HttpGet("reservations/live")]
    public async Task<IActionResult> GetLiveReservations()
    {
        var data = await getLiveReservationsUseCase.ExecuteAsync();
        return Ok(data);
    }

    [HttpGet("staff")]
    public async Task<IActionResult> GetStaff([FromServices] GetStaffUseCase useCase)
        => Ok(await useCase.ExecuteAsync());

    [HttpPost("staff")]
    public async Task<IActionResult> CreateStaff(
        [FromBody] CreateStaffRequestDTO dto,
        [FromServices] CreateStaffUseCase useCase)
    {
        try
        {
            var result = await useCase.ExecuteAsync(dto);
            return CreatedAtAction(nameof(GetStaff), new { email = result.Email }, result);
        }
        catch (ExcepcionDominio ex) when (ex.Message.Contains("Ya existe"))
        {
            return Conflict(ex.Message);
        }
        catch (ExcepcionDominio ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("staff/{email}")]
    public async Task<IActionResult> UpdateStaff(
        string email,
        [FromBody] UpdateStaffRequestDTO dto,
        [FromServices] UpdateStaffUseCase useCase)
    {
        try
        {
            var result = await useCase.ExecuteAsync(email, dto);
            return Ok(result);
        }
        catch (ExcepcionDominio ex) when (ex.Message.Contains("No existe"))
        {
            return NotFound(ex.Message);
        }
        catch (ExcepcionDominio ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// HU-18: Eliminación administrativa de una reserva; notifica al propietario vía SignalR.
    /// DELETE /api/admin/reservations/{id}
    /// </summary>
    [HttpDelete("reservations/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReservation(
        Guid id,
        [FromServices] DeleteReservationUseCase useCase)
    {
        try
        {
            await useCase.ExecuteAsync(id);
            return NoContent();
        }
        catch (ExcepcionDominio ex) { return NotFound(ex.Message); }
    }

}

