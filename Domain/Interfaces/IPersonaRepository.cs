using AdaByron.Domain.Entities;

namespace AdaByron.Domain.Interfaces;

// Contrato de repositorio para Persona.
public interface IPersonaRepository
{
    Task<Persona?> GetByEmailAsync(string email);
    Task AddAsync(Persona persona);
}
