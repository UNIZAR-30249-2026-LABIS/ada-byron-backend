using AdaByron.Application.DTOs;
using AdaByron.Domain.Aggregates.SpaceAggregate;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Interfaces;

namespace AdaByron.Application.UseCases.Spaces;

/// <summary>
/// Caso de uso HU-XX Admin: Actualiza las especificaciones de un espacio.
/// Solo accesible por el rol Gerente (la autorización se aplica en el controlador).
/// </summary>
public class UpdateSpaceUseCase(IEspacioRepository espacios)
{
    public async Task<Espacio> ExecuteAsync(string codigoEspacio, UpdateSpaceRequestDTO request)
    {
        // 1. Obtener el espacio
        var espacio = await espacios.GetByCodigoAsync(codigoEspacio)
            ?? throw new ExcepcionDominio($"No existe ningún espacio con código '{codigoEspacio}'.");

        // 2. Parsear y validar categoría
        if (!Enum.TryParse<TipoEspacio>(request.Categoria, ignoreCase: true, out var nuevaCategoria))
            throw new ExcepcionDominio($"Categoría inválida: '{request.Categoria}'. Valores permitidos: {string.Join(", ", Enum.GetNames<TipoEspacio>())}");

        // 3. Construir Value Objects (sus constructores validan las reglas)
        var nuevoAforo = Aforo.De(request.Aforo);   // Lanza si aforo <= 0
        var nuevaPlanta = Planta.De(request.Planta); // Lanza si fuera del rango

        // 4. Delegar la mutación al Aggregate Root (encapsula las invariantes del dominio)
        espacio.Actualizar(request.Nombre, nuevoAforo, nuevaPlanta, nuevaCategoria);

        // 5. Persistir
        await espacios.UpdateAsync(espacio);

        return espacio;
    }
}
