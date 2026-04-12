using AdaByron.Application.DTOs;
using AdaByron.Domain.Aggregates.SpaceAggregate;
using AdaByron.Domain.Interfaces;

namespace AdaByron.Application.UseCases.Spaces;

public class GetFilteredSpacesUseCase(IEspacioRepository _espaciosRepo)
{
    public async Task<IEnumerable<Espacio>> ExecuteAsync(SpaceFilterCriteria criteria)
    {
        return await _espaciosRepo.SearchAsync(criteria.Id, criteria.Floor, criteria.Category, criteria.Capacity);
    }
}
