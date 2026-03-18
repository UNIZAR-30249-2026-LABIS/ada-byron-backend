using AdaByron.Domain.Entities;
using AdaByron.Domain.ValueObjects;

namespace AdaByron.Domain.Interfaces;

// Contrato de repositorio para Reserva.
public interface IReservaRepository
{
    Task<Reserva?> GetByIdAsync(Guid id);
    Task<IEnumerable<Reserva>> GetByEspacioAsync(string espacioId);
    Task<IEnumerable<Reserva>> GetByPersonaAsync(string personaId);

    // Detecta si ya existe una reserva activa que solape con la franja dada
    Task<bool> ExisteSolapamientoAsync(string espacioId, FranjaHoraria franja);

    Task AddAsync(Reserva reserva);
    Task DeleteAsync(Guid id);
}
