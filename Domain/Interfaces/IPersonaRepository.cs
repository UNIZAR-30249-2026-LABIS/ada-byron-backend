using AdaByron.Domain.Aggregates.PersonAggregate;

namespace AdaByron.Domain.Interfaces;

public interface IPersonaRepository
{
    Task<Persona?> GetByEmailAsync(string email);
    Task<IEnumerable<Persona>> GetAllAsync();
    Task<bool> ExistsAsync(string email);
    Task AddAsync(Persona persona);
    Task UpdateAsync(Persona persona);
}
