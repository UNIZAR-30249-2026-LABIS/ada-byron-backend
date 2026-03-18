using AdaByron.Domain.Entities;

namespace AdaByron.Domain.Interfaces;

// Contrato de repositorio para Espacio.
// Las coordenadas se pasan como primitivos (lat/lon) para no contaminar el Dominio con NetTopologySuite.
public interface IEspacioRepository
{
    Task<Espacio?> GetByCodigoAsync(string codigo);
    Task<IEnumerable<Espacio>> GetAllAsync();
    Task AddAsync(Espacio espacio);

    // Búsqueda por cercanía geoespacial mediante PostGIS (implementada en Infrastructure)
    Task<IEnumerable<Espacio>> GetCercanosAsync(double latitud, double longitud, double radioMetros);
}
