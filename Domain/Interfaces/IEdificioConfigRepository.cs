namespace AdaByron.Domain.Interfaces;

using AdaByron.Domain.Aggregates.SpaceAggregate;

public interface IEdificioConfigRepository
{
    Task<EdificioConfig?> GetConfigAsync();
    Task UpdateConfigAsync(EdificioConfig config);
}
