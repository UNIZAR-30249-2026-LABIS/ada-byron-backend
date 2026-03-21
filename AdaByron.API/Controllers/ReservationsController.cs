using AdaByron.Application.DTOs;
using AdaByron.Application.UseCases.Reservations;
using AdaByron.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AdaByron.API.Controllers;

[ApiController]
[Route("api/reservations")]
public class ReservationsController : ControllerBase
{
    private readonly ReservationAppService _reservationAppService;

    public ReservationsController(ReservationAppService reservationAppService)
    {
        _reservationAppService = reservationAppService;
    }

    [HttpPost]
    public async Task<ActionResult<ReservationDto>> Create([FromBody] CreateReservationRequest request, CancellationToken cancellationToken)
    {
        var result = await _reservationAppService.MakeReservation(request, cancellationToken);
        return Ok(result);
    }
}