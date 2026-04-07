using AdaByron.Domain.Aggregates.ReservationAggregate;

namespace AdaByron.Domain.Interfaces;

public interface IReservaRepository
{
    Task<Reserva?> GetByIdAsync(Guid id);
    Task<IEnumerable<Reserva>> GetByEspacioAsync(string codigoEspacio);
    Task<IEnumerable<Reserva>> GetAllAsync();
    Task AddAsync(Reserva reserva);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<(Reserva, string NombreEspacio, string NombreUsuario)>> GetLiveWithDetailsAsync();
}
