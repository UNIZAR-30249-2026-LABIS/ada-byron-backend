using AdaByron.Application.DTOs;
using AdaByron.Application.UseCases.Reservas;
using AdaByron.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AdaByron.API.Controllers;

[ApiController]
[Route("api/reservations")]
public class ReservationsController : ControllerBase
{
    private readonly CrearReservaUseCase _crearReservaUseCase;

    public ReservationsController(CrearReservaUseCase crearReservaUseCase)
    {
        _crearReservaUseCase = crearReservaUseCase;
    }

    /// <summary>
    /// Crea una nueva reserva para un espacio, validando las Reglas de Negocio F1-F6 y la Incompatibilidad Horaria de forma Atómica.
    /// </summary>
    /// <param name="request">Los datos de la reserva y el email del solicitante (DUMMY AUTH para el prototipo).</param>
    /// <returns>La reserva generada y aceptada, o error HTTP 409 si sobrepasa el aforo o entra en colisión.</returns>
    /// <response code="200">Reserva aceptada e insertada en la Base de Datos con su Token identificativo.</response>
    /// <response code="409">Conflicto de Dominio: Inconsistencia con el aforo, la matriz de mutabilidad o conflicto horario concurrente.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ReservaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservaResponseDTO>> Create([FromBody] CrearReservaRequestDTO request)
    {
        // El middleware capturará las excepciones de dominio y retornará 409 automáticamente
        var result = await _crearReservaUseCase.ExecuteAsync(request);
        return Ok(result);
    }
}
