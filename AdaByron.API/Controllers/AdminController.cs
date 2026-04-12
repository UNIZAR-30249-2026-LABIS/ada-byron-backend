using AdaByron.Application.DTOs;
using AdaByron.Application.UseCases.Admin;
using AdaByron.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AdaByron.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Gerente")]
public class AdminController(UpdateBuildingConfigUseCase updateConfigUseCase) : ControllerBase
{
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
            await updateConfigUseCase.ExecuteAsync(request);
            return Ok(new { PorcentajeOcupacion = request.PorcentajeOcupacion });
        }
        catch (ExcepcionDominio ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Actualiza un espacio de la bd maestro.
    /// </summary>
    [HttpPut("spaces/{id}")]
    public async Task<IActionResult> UpdateSpace(
        string id, 
        [FromBody] SpaceEditRequestDTO dto, 
        [FromServices] AdaByron.Infrastructure.Persistence.DbContext.AplicacionDbContext dbContext)
    {
        var espacio = await dbContext.Espacios.FirstOrDefaultAsync(e => e.CodigoEspacio == id);
        if (espacio == null) return NotFound("Espacio no encontrado.");

        try 
        {
            espacio.UpdateDetails(
                dto.Nombre, 
                AdaByron.Domain.Aggregates.SpaceAggregate.Planta.De(dto.Planta), 
                AdaByron.Domain.Aggregates.SpaceAggregate.Aforo.De(dto.Aforo), 
                Enum.Parse<AdaByron.Domain.Aggregates.SpaceAggregate.TipoEspacio>(dto.Categoria)
            );
            await dbContext.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex) 
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("reservations/live")]
    public async Task<IActionResult> GetLiveReservations([FromServices] AdaByron.Infrastructure.Persistence.DbContext.AplicacionDbContext dbContext)
    {
        // Explicit join since Aggregate Roots only reference by ID (EspacioId)
        var query = from r in dbContext.Reservas
                    join e in dbContext.Espacios on r.EspacioId equals e.CodigoEspacio into spaceJoin
                    from e in spaceJoin.DefaultIfEmpty()
                    select new {
                        id = r.Id,
                        espacioId = r.EspacioId,
                        nombreEspacio = e != null ? e.Nombre : null,
                        solicitante = r.PersonaId, // MVP simplistic approach (email)
                        inicio = r.Franja.Inicio,
                        fin = r.Franja.Fin,
                        estado = r.Estado.ToString(),
                        esPotencialmenteInvalida = r.Estado.ToString() == "PotencialmenteInvalida"
                    };

        var dto = await query.ToListAsync();
        return Ok(dto);
    }

    [HttpDelete("reservations/{id}")]
    public async Task<IActionResult> DeleteReservation(
        Guid id, 
        [FromServices] AdaByron.Infrastructure.Persistence.DbContext.AplicacionDbContext dbContext,
        [FromServices] IHubContext<AdaByron.API.Hubs.ReservasHub> hubContext)
    {
        var res = await dbContext.Reservas.FindAsync(id);
        if (res == null) return NotFound();

        dbContext.Reservas.Remove(res);
        await dbContext.SaveChangesAsync();

        // Notificar en tiempo real (HU-18)
        await hubContext.Clients.All.SendAsync("ReservaAnulada", new {
            espacio = res.EspacioId,
            mensaje = "Anulación administrativa prioritaria."
        });

        return NoContent();
    }
}

public class SpaceEditRequestDTO 
{
    public string Nombre { get; set; } = string.Empty;
    public int Aforo { get; set; }
    public int Planta { get; set; }
    public string Categoria { get; set; } = string.Empty;
}
