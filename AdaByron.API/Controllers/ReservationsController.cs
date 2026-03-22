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
    /// <returns>La reserva generada y aceptada, o error HTTP 400 si sobrepasa el aforo o entra en colisión.</returns>
    /// <response code="200">Reserva aceptada e insertada en la Base de Datos con su Token identificativo.</response>
    /// <response code="400">Error de Dominio: Inconsistencia con el aforo, la matriz de mutabilidad o conflicto horario concurrente detectado por C#.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ReservaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReservaResponseDTO>> Create([FromBody] CrearReservaRequestDTO request)
    {
        var result = await _crearReservaUseCase.ExecuteAsync(request);
        return Ok(result);
    }
}