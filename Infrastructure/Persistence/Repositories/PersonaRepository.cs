using AdaByron.Domain.Aggregates.PersonAggregate;
using AdaByron.Domain.Interfaces;
using AdaByron.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace AdaByron.Infrastructure.Persistence.Repositories;

public class PersonaRepository(AplicacionDbContext context) : IPersonaRepository
{
    public async Task<Persona?> GetByEmailAsync(string email)
        => await context.Personas.FindAsync(email);

    public async Task<IEnumerable<Persona>> GetAllAsync()
        => await context.Personas.ToListAsync();

    public async Task AddAsync(Persona persona)
    {
        await context.Personas.AddAsync(persona);
        await context.SaveChangesAsync();
    }
}
