using AdaByron.Domain.Aggregates.SpaceAggregate;

namespace AdaByron.Domain.Interfaces;

public interface IEspacioRepository
{
    Task<Espacio?> GetByCodigoAsync(string codigo);
    Task<IEnumerable<Espacio>> GetAllAsync();
    Task<IEnumerable<Espacio>> SearchAsync(string? id, int? floor, string? category, int? capacity);
    Task AddAsync(Espacio espacio);
}
