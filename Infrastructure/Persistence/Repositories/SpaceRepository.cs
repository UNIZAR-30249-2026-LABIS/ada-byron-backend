using AdaByron.Domain.Aggregates.PersonAggregate; using AdaByron.Domain.Aggregates.SpaceAggregate; using AdaByron.Domain.Aggregates.ReservationAggregate;
using AdaByron.Domain.Interfaces;
using AdaByron.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace AdaByron.Infrastructure.Persistence.Repositories;

public class EspacioRepository(AplicacionDbContext context) : IEspacioRepository
{
    public async Task<Espacio?> GetByCodigoAsync(string codigo)
        => await context.Espacios
            .Include(e => e.Reservas)
            .FirstOrDefaultAsync(e => e.CodigoEspacio == codigo);

    public async Task<IEnumerable<Espacio>> GetAllAsync()
        => await context.Espacios.ToListAsync();

    public async Task<IEnumerable<Espacio>> SearchAsync(string? id, string? floor, string? category, int? capacity)
    {
        var query = context.Espacios.AsQueryable();

        if (!string.IsNullOrWhiteSpace(id))
            query = query.Where(e => e.CodigoEspacio.Contains(id));

        if (!string.IsNullOrWhiteSpace(floor))
        {
            int parsedFloor = floor.Equals("S1", StringComparison.OrdinalIgnoreCase) ? -1 : 
                              int.TryParse(floor, out var f) ? f : int.MinValue;
            
            if (parsedFloor != int.MinValue)
            {
                var plantaTarget = Planta.De(parsedFloor);
                query = query.Where(e => e.Planta == plantaTarget);
            }
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var parsed = Enum.TryParse<TipoEspacio>(category, true, out var t) ? t : (TipoEspacio?)null;
            if (parsed.HasValue)
                query = query.Where(e => e.TipoFisico == parsed.Value || e.CategoriaReserva == parsed.Value);
        }

        if (capacity.HasValue)
            query = query.Where(e => e.Aforo.Valor >= capacity.Value);

        return await query.ToListAsync();
    }

    public async Task AddAsync(Espacio espacio)
    {
        await context.Espacios.AddAsync(espacio);
        await context.SaveChangesAsync();
    }

    // Convierte (lat, lon) a Point WGS-84 y consulta la shadow property "Ubicacion" via PostGIS
    public async Task<IEnumerable<Espacio>> GetCercanosAsync(double latitud, double longitud, double radioMetros)
    {
        var punto = new Point(longitud, latitud) { SRID = 4326 };

        return await context.Espacios
            .Where(e => EF.Property<Point>(e, "Ubicacion") != null
                     && EF.Property<Point>(e, "Ubicacion").Distance(punto) <= radioMetros)
            .ToListAsync();
    }
}
