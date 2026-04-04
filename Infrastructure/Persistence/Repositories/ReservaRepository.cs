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

    public async Task<IEnumerable<Reserva>> GetAllAsync()
        => await context.Reservas.ToListAsync();

    public async Task AddAsync(Reserva reserva)
    {
        await context.Reservas.AddAsync(reserva);
        await context.SaveChangesAsync();
    }
}
