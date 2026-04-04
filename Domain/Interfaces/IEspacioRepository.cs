using AdaByron.Domain.Aggregates.SpaceAggregate;

namespace AdaByron.Domain.Interfaces;

public interface IEspacioRepository
{
    Task<Espacio?> GetByCodigoAsync(string codigo);
    Task<IEnumerable<Espacio>> GetAllAsync();
    Task AddAsync(Espacio espacio);
}
