using AdaByron.Domain.Entities;
using AdaByron.Domain.Interfaces;
using AdaByron.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace AdaByron.Infrastructure.Persistence.Repositories;

public class EspacioRepository(AplicacionDbContext context) : IEspacioRepository
{
    public async Task<Espacio?> GetByCodigoAsync(string codigo)
        => await context.Espacios.FindAsync(codigo);

    public async Task<IEnumerable<Espacio>> GetAllAsync()
        => await context.Espacios.ToListAsync();

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
