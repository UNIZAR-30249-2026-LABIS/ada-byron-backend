using AdaByron.Application.DTOs;
using AdaByron.Domain.Interfaces;

namespace AdaByron.Application.UseCases.Reservas;

/// <summary>
/// Caso de uso: devuelve todas las reservas de un usuario identificado por su email (HU-18).
/// </summary>
public class GetMisReservasUseCase(
    IReservaRepository reservas,
    IEspacioRepository espacios)
{
    public async Task<IEnumerable<MiReservaDTO>> ExecuteAsync(string email)
    {
        var misReservas = await reservas.GetByPersonaAsync(email.ToLowerInvariant());

        // Cargar nombres de espacios (batch simple, pocas reservas por usuario)
        var allSpaces = await espacios.GetAllAsync();
        var spaceMap  = allSpaces.ToDictionary(e => e.CodigoEspacio, e => e.Nombre);

        return misReservas.Select(r => new MiReservaDTO
        {
            Id               = r.Id,
            EspacioId        = r.EspacioId,
            NombreEspacio    = spaceMap.TryGetValue(r.EspacioId, out var nombre) ? nombre : r.EspacioId,
            Inicio           = r.Franja.Inicio,
            Fin              = r.Franja.Fin,
            Estado           = r.Estado.ToString(),
            NumeroAsistentes = r.NumeroAsistentes
        });
    }
}
