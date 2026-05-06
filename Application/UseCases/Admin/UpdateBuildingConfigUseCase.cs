using AdaByron.Application.DTOs;
using AdaByron.Application.Ports.Out;
using AdaByron.Application.UseCases.Reservations;
using AdaByron.Domain.Aggregates.SpaceAggregate;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Interfaces;

namespace AdaByron.Application.UseCases.Admin;

public class UpdateBuildingConfigUseCase(
    IEdificioConfigRepository configRepo,
    IEspacioRepository espacios,
    ReevaluarReservasEspacioUseCase reevaluar,
    IUnitOfWork uow)
{
    /// <summary>
    /// Actualiza el porcentaje global de ocupación del edificio y reevalúa
    /// las reservas de los espacios que heredan ese porcentaje (spec sección I).
    /// </summary>
    /// <returns>Número de reservas marcadas como PotencialmenteInvalida.</returns>
    public async Task<int> ExecuteAsync(UpdateConfigDTO request)
    {
        await uow.BeginTransactionAsync();
        try
        {
            var conf = new EdificioConfig("AdaByron", request.PorcentajeOcupacion);
            await configRepo.UpdateConfigAsync(conf);
            await uow.CommitAsync();
        }
        catch(ArgumentException ex)
        {
            await uow.RollbackAsync();
            throw new ExcepcionDominio(ex.Message);
        }
        catch
        {
            await uow.RollbackAsync();
            throw;
        }

        // Reevaluar reservas de los espacios que heredan el % global (sin override específico).
        // Se hace FUERA de la transacción anterior (igual que SetSpaceOccupancyUseCase).
        var todosEspacios = await espacios.GetAllAsync();
        var espaciosHeredados = todosEspacios
            .Where(e => !e.PorcentajeOcupacionEspecifico.HasValue)
            .ToList();

        int totalMarcadas = 0;
        foreach (var espacio in espaciosHeredados)
            totalMarcadas += await reevaluar.ExecuteAsync(espacio);

        return totalMarcadas;
    }
}
