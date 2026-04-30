using AdaByron.Application.DTOs;
using AdaByron.Application.UseCases.Admin;
using AdaByron.Application.UseCases.Spaces;
using AdaByron.Domain.Aggregates.PersonAggregate;
using AdaByron.Domain.Aggregates.SpaceAggregate;
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
                Enum.Parse<AdaByron.Domain.Aggregates.SpaceAggregate.TipoEspacio>(dto.Categoria),
                dto.EsReservable,
                dto.HorarioReserva?.Select(h => new HorarioReservaDia
                {
                    DiaSemana = h.DiaSemana,
                    Activo = h.Activo,
                    HoraInicio = h.HoraInicio,
                    HoraFin = h.HoraFin
                }).ToList()
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

    [HttpGet("staff")]
    public async Task<IActionResult> GetStaff([FromServices] AdaByron.Infrastructure.Persistence.DbContext.AplicacionDbContext dbContext)
    {
        var personas = await dbContext.Personas
            .OrderBy(p => p.Apellidos)
            .ThenBy(p => p.Nombre)
            .Select(p => new
            {
                email = p.Email,
                nombre = p.Nombre,
                apellidos = p.Apellidos,
                rol = p.Rol.ToString(),
                departamento = p.Departamento.Nombre
            })
            .ToListAsync();

        return Ok(personas);
    }

    [HttpPost("staff")]
    public async Task<IActionResult> CreateStaff(
        [FromBody] StaffUpsertRequestDTO dto,
        [FromServices] AdaByron.Infrastructure.Persistence.DbContext.AplicacionDbContext dbContext)
    {
        if (await dbContext.Personas.AnyAsync(p => p.Email == dto.Email.Trim().ToLowerInvariant()))
            return Conflict("Ya existe una persona registrada con ese email.");

        try
        {
            var persona = new Persona(
                dto.Email,
                dto.Nombre,
                dto.Apellidos,
                ParseRol(dto.Rol),
                ParseDepartamento(dto.Departamento));

            dbContext.Personas.Add(persona);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStaff), new { email = persona.Email }, new
            {
                email = persona.Email,
                nombre = persona.Nombre,
                apellidos = persona.Apellidos,
                rol = persona.Rol.ToString(),
                departamento = persona.Departamento.Nombre
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("staff/{email}")]
    public async Task<IActionResult> UpdateStaff(
        string email,
        [FromBody] StaffUpsertRequestDTO dto,
        [FromServices] AdaByron.Infrastructure.Persistence.DbContext.AplicacionDbContext dbContext)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var persona = await dbContext.Personas.FirstOrDefaultAsync(p => p.Email == normalizedEmail);
        if (persona == null) return NotFound("Persona no encontrada.");

        try
        {
            persona.ActualizarDatosAdministrativos(
                dto.Nombre,
                dto.Apellidos,
                ParseRol(dto.Rol),
                ParseDepartamento(dto.Departamento));

            await dbContext.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
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

    private static Rol ParseRol(string rol)
    {
        if (!Enum.TryParse<Rol>(rol, true, out var parsed))
            throw new ExcepcionDominio("Rol no válido.");

        return parsed;
    }

    private static Departamento? ParseDepartamento(string? departamento)
    {
        return string.IsNullOrWhiteSpace(departamento)
            ? null
            : new Departamento(departamento.Trim());
    }
}

public class SpaceEditRequestDTO 
{
    public string Nombre { get; set; } = string.Empty;
    public int Aforo { get; set; }
    public int Planta { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public bool EsReservable { get; set; }
    public List<HorarioReservaDiaRequestDTO> HorarioReserva { get; set; } = new();
}

public class HorarioReservaDiaRequestDTO
{
    public int DiaSemana { get; set; }
    public bool Activo { get; set; }
    public string HoraInicio { get; set; } = "00:00";
    public string HoraFin { get; set; } = "23:59";
}

public class StaffUpsertRequestDTO
{
    public string Email { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string? Departamento { get; set; }
}
