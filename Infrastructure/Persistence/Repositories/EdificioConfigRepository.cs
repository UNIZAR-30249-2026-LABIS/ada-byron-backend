using AdaByron.Domain.Aggregates.SpaceAggregate;
using AdaByron.Domain.Interfaces;
using AdaByron.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace AdaByron.Infrastructure.Persistence.Repositories;

public class EdificioConfigRepository(AplicacionDbContext context) : IEdificioConfigRepository
{
    public async Task<EdificioConfig?> GetConfigAsync()
    {
        return await context.EdificioConfigs.FirstOrDefaultAsync(c => c.Id == "AdaByron");
    }

    public async Task UpdateConfigAsync(EdificioConfig config)
    {
        var existing = await context.EdificioConfigs.FirstOrDefaultAsync(c => c.Id == "AdaByron");
        if (existing == null)
        {
            // Forzar ID si llegamos aquí
            var newConfig = new EdificioConfig("AdaByron", config.PorcentajeOcupacion);
            context.EdificioConfigs.Add(newConfig);
        }
        else
        {
            existing.SetPorcentaje(config.PorcentajeOcupacion);
            context.EdificioConfigs.Update(existing);
        }
        await context.SaveChangesAsync();
    }
}
