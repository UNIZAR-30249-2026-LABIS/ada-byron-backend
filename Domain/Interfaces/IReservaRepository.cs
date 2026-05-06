using AdaByron.Domain.Aggregates.ReservationAggregate;

namespace AdaByron.Domain.Interfaces;

public interface IReservaRepository
{
    Task<Reserva?> GetByIdAsync(Guid id);
    Task<IEnumerable<Reserva>> GetByEspacioAsync(string codigoEspacio);
    Task<IEnumerable<Reserva>> GetByPersonaAsync(string personaId);
    Task<IEnumerable<Reserva>> GetAllAsync();
    /// <summary>PBI-13: Obtiene reservas Aceptadas de un espacio con franja futura (candidatas a invalidarse).</summary>
    Task<IEnumerable<Reserva>> GetAceptadasFuturasByEspacioAsync(string codigoEspacio);
    /// <summary>PBI-13: Obtiene reservas PotencialmenteInvalida de un espacio con franja futura (candidatas a restaurarse si el aforo aumenta).</summary>
    Task<IEnumerable<Reserva>> GetPotencialmenteInvalidasFuturasByEspacioAsync(string codigoEspacio);
    /// <summary>PBI-14: Obtiene reservas PotencialmenteInvalida cuya fecha de modificación sea anterior a beforeDate.</summary>
    Task<IEnumerable<Reserva>> GetExpiredPotentiallyInvalidAsync(DateTime beforeDate);
    Task AddAsync(Reserva reserva);
    Task UpdateAsync(Reserva reserva);
    Task UpdateRangeAsync(IEnumerable<Reserva> reservas);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<(Reserva, string NombreEspacio, string NombreUsuario)>> GetLiveWithDetailsAsync();
}
