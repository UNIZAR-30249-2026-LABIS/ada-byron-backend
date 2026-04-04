using AdaByron.Domain.Aggregates.PersonAggregate;

namespace AdaByron.Domain.Interfaces;

public interface IPersonaRepository
{
    Task<Persona?> GetByEmailAsync(string email);
    Task<IEnumerable<Persona>> GetAllAsync();
    Task AddAsync(Persona persona);
}
