using AdaByron.Domain.Aggregates.ReservationAggregate;
using AdaByron.Domain.Interfaces;
using AdaByron.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace AdaByron.Infrastructure.Persistence.Repositories;

public class ReservaRepository(AplicacionDbContext context) : IReservaRepository
{
    public async Task<Reserva?> GetByIdAsync(Guid id)
        => await context.Reservas.FindAsync(id);

    public async Task<IEnumerable<Reserva>> GetByEspacioAsync(string codigoEspacio)
        => await context.Reservas
            .Where(r => r.EspacioId == codigoEspacio)
            .ToListAsync();

    public async Task<IEnumerable<Reserva>> GetByPersonaAsync(string personaId)
        => await context.Reservas
            .Where(r => r.PersonaId == personaId)
            .OrderByDescending(r => r.Franja.Inicio)
            .ToListAsync();

    public async Task<IEnumerable<Reserva>> GetAllAsync()
        => await context.Reservas.ToListAsync();

    public async Task AddAsync(Reserva reserva)
    {
        await context.Reservas.AddAsync(reserva);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Reserva reserva)
    {
        try
        {
            context.Reservas.Update(reserva);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new AdaByron.Domain.Exceptions.ConcurrencyException("La reserva ha sido modificada por otro usuario.", ex);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var reserva = await context.Reservas.FindAsync(id);
        if (reserva != null)
        {
            context.Reservas.Remove(reserva);
            await context.SaveChangesAsync();
        }
    }
    
    public async Task<IEnumerable<Reserva>> GetAceptadasFuturasByEspacioAsync(string codigoEspacio)
    {
        var now = DateTime.UtcNow;
        return await context.Reservas
            .Where(r => r.EspacioId == codigoEspacio
                     && r.Estado == EstadoReserva.Aceptada
                     && r.Franja.Fin > now)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reserva>> GetPotencialmenteInvalidasFuturasByEspacioAsync(string codigoEspacio)
    {
        var now = DateTime.UtcNow;
        return await context.Reservas
            .Where(r => r.EspacioId == codigoEspacio
                     && r.Estado == EstadoReserva.PotencialmenteInvalida
                     && r.Franja.Fin > now)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reserva>> GetExpiredPotentiallyInvalidAsync(DateTime beforeDate)
    {
        return await context.Reservas
            .Where(r => r.Estado == EstadoReserva.PotencialmenteInvalida 
                     && r.FechaEstadoModificado < beforeDate)
            .ToListAsync();
    }

    public async Task UpdateRangeAsync(IEnumerable<Reserva> reservas)
    {
        try
        {
            context.Reservas.UpdateRange(reservas);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new AdaByron.Domain.Exceptions.ConcurrencyException("Alguna de las reservas ha sido modificada por otro usuario.", ex);
        }
    }

    public async Task<IEnumerable<(Reserva, string NombreEspacio, string NombreUsuario)>> GetLiveWithDetailsAsync()
    {
        var now = DateTime.UtcNow;
        var query = from r in context.Reservas
                    join e in context.Espacios on r.EspacioId equals e.CodigoEspacio
                    join p in context.Personas on r.PersonaId equals p.Email
                    // Filtramos las que terminan en el futuro
                    where r.Franja.Fin > now
                    select new { Reserva = r, SpaceName = e.Nombre, UserName = p.Nombre + " " + p.Apellidos };

        var result = await query.ToListAsync();
        return result.Select(x => (x.Reserva, x.SpaceName, x.UserName));
    }
}
