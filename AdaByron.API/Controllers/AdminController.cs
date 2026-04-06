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
}
