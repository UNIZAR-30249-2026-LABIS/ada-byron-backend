using AdaByron.Application.DTOs;
using AdaByron.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;

namespace AdaByron.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Any logged in user
public class UsersController : ControllerBase
{
    [HttpGet("me/reservations")]
    public async Task<IActionResult> GetMyReservations([FromServices] AdaByron.Infrastructure.Persistence.DbContext.AplicacionDbContext dbContext)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (email == null) return Unauthorized("No autorizado.");

        var query = from r in dbContext.Reservas
                    where r.PersonaId == email
                    join e in dbContext.Espacios on r.EspacioId equals e.CodigoEspacio into spaceJoin
                    from e in spaceJoin.DefaultIfEmpty()
                    select new {
                        id = r.Id,
                        espacioId = r.EspacioId,
                        nombreEspacio = e != null ? e.Nombre : null,
                        solicitante = r.PersonaId,
                        inicio = r.Franja.Inicio,
                        fin = r.Franja.Fin,
                        numeroAsistentes = r.NumeroAsistentes,
                        estado = r.Estado.ToString()
                    };

        var dto = await query.ToListAsync();
        return Ok(dto);
    }

    [HttpDelete("me/reservations/{id}/cancel")]
    public async Task<IActionResult> CancelMyReservation(
        Guid id, 
        [FromServices] AdaByron.Infrastructure.Persistence.DbContext.AplicacionDbContext dbContext,
        [FromServices] IHubContext<AdaByron.API.Hubs.ReservasHub> hubContext)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (email == null) return Unauthorized("No autorizado.");

        var reserva = await dbContext.Reservas.FindAsync(id);
        if (reserva == null) return NotFound("Reserva no encontrada.");

        if (reserva.PersonaId != email) return Forbid("No tienes permisos para cancelar esta reserva.");
        
        if (reserva.Estado == AdaByron.Domain.Aggregates.ReservationAggregate.EstadoReserva.Rechazada)
        {
            return BadRequest("La reserva ya está cancelada o rechazada.");
        }

        if (reserva.Franja.Inicio < DateTime.UtcNow)
        {
            return BadRequest("No puedes cancelar una reserva que ya ha comenzado.");
        }

        // Technically, DDD cancellation should alter the entity state to 'Rescindida' (to keep history),
        // or actually delete it depending on requirements. HU-18 specifies removing or marking.
        // I will just remove it to match the Admin approach for simplicity.
        dbContext.Reservas.Remove(reserva);
        await dbContext.SaveChangesAsync();

        await hubContext.Clients.All.SendAsync("ReservaCancelada", new {
            espacio = reserva.EspacioId,
            mensaje = $"La reserva de {email} ha sido cancelada."
        });

        return NoContent();
    }
}
