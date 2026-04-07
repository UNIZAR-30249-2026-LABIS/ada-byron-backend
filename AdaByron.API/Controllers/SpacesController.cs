using AdaByron.Application.DTOs;
using AdaByron.Application.Ports.Out;
using AdaByron.Domain.Interfaces;
using AdaByron.Domain.Aggregates.SpaceAggregate;
using AdaByron.Application.UseCases.Spaces;
using Microsoft.AspNetCore.Mvc;

namespace AdaByron.API.Controllers;

[ApiController]
[Route("api/spaces")]
public class SpacesController(IEspacioRepository espacios, GetFilteredSpacesUseCase getFilteredSpacesUseCase) : ControllerBase
{
    /// <summary>
    /// Lista todos los espacios del edificio Ada Byron registrados en el sistema.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Espacio>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await espacios.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// Busca espacios basándose en criterios de filtrado opcionales (PBI 7).
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<Espacio>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] SpaceFilterCriteria criteria)
    {
        var result = await getFilteredSpacesUseCase.ExecuteAsync(criteria);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el detalle de un espacio por su código alfanumérico.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var espacio = await espacios.GetByCodigoAsync(id);
        if (espacio == null) return NotFound();
        return Ok(espacio);
    }
}
