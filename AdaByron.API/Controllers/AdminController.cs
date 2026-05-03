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
        [FromServices] AdaByron.Infrastructure.Persistence.DbContext.AplicacionDbContext dbContext)
    {
        try
        {
            int reservasMarcadas = await setOccupancyUseCase.ExecuteAsync(id, dto);

            // Leer el estado actualizado para devolverlo en la respuesta
            var espacio = await dbContext.Espacios.FindAsync(id);
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
        catch (AdaByron.Domain.Exceptions.ExcepcionDominio ex)
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
