using AdaByron.Domain.Entities;
using AdaByron.Domain.Interfaces;
using AdaByron.Domain.ValueObjects;
using AdaByron.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace AdaByron.Infrastructure.Persistence.Repositories;

public class ReservaRepository(AplicacionDbContext context) : IReservaRepository
{
    public async Task<Reserva?> GetByIdAsync(Guid id)
        => await context.Reservas.FindAsync(id);

    public async Task<IEnumerable<Reserva>> GetByEspacioAsync(string espacioId)
        => await context.Reservas
            .Where(r => r.EspacioId == espacioId)
            .ToListAsync();

    public async Task<IEnumerable<Reserva>> GetByPersonaAsync(string personaId)
        => await context.Reservas
            .Where(r => r.PersonaId == personaId)
            .ToListAsync();

    public async Task<bool> ExisteSolapamientoAsync(string espacioId, FranjaHoraria franja)
        => await context.Reservas
            .Where(r => r.EspacioId == espacioId)
            .AnyAsync(r => r.Franja.Inicio < franja.Fin
                        && r.Franja.Fin   > franja.Inicio);

    public async Task AddAsync(Reserva reserva)
    {
        await context.Reservas.AddAsync(reserva);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var reserva = await context.Reservas.FindAsync(id);
        if (reserva is not null)
        {
            context.Reservas.Remove(reserva);
            await context.SaveChangesAsync();
        }
    }
}
