using AdaByron.Application.DTOs;
using AdaByron.Application.UseCases.Reservations;
using AdaByron.Domain.Aggregates.PersonAggregate;
using AdaByron.Domain.Aggregates.SpaceAggregate;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Interfaces;

namespace AdaByron.Application.UseCases.Spaces;

/// <summary>
/// Caso de uso HU-XX Admin: Actualiza las especificaciones de un espacio.
/// PBI-13 (HU-O4): Tras el cambio, reevalúa las reservas existentes.
/// Solo accesible por el rol Gerente (la autorización se aplica en el controlador).
/// </summary>
public class UpdateSpaceUseCase(
    IEspacioRepository espacios,
    ReevaluarReservasEspacioUseCase reevaluar)
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
        espacio.Actualizar(
            request.Nombre,
            nuevoAforo,
            nuevaPlanta,
            nuevaCategoria,
            request.EsReservable,
            request.HorarioReserva.Select(h => new HorarioReservaDia
            {
                DiaSemana = h.DiaSemana,
                Activo = h.Activo,
                HoraInicio = h.HoraInicio,
                HoraFin = h.HoraFin
            }).ToList());

        if (!string.IsNullOrWhiteSpace(request.TipoAsignacion) &&
            Enum.TryParse<TipoAsignacionEspacio>(request.TipoAsignacion, true, out var tipoAsignacion))
        {
            var departamento = string.IsNullOrWhiteSpace(request.DepartamentoAsignado)
                ? Departamento.Null
                : Departamento.From(request.DepartamentoAsignado);

            espacio.ActualizarAsignacion(tipoAsignacion, departamento, request.PersonasAsignadas);
        }

        // 5. Persistir
        await espacios.UpdateAsync(espacio);

        // 6. PBI-13: Reevaluar reservas afectadas por el nuevo aforo base
        await reevaluar.ExecuteAsync(espacio);

        return espacio;
    }
}
