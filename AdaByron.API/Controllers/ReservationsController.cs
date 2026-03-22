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

    [HttpPost]
    public async Task<ActionResult<ReservaResponseDTO>> Create([FromBody] CrearReservaRequestDTO request)
    {
        var result = await _crearReservaUseCase.ExecuteAsync(request);
        return Ok(result);
    }
}