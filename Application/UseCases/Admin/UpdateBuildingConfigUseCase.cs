using AdaByron.Application.DTOs;
using AdaByron.Application.Ports.Out;
using AdaByron.Domain.Aggregates.SpaceAggregate;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Interfaces;

namespace AdaByron.Application.UseCases.Admin;

public class UpdateBuildingConfigUseCase(
    IEdificioConfigRepository configRepo,
    IUnitOfWork uow)
{
    public async Task ExecuteAsync(UpdateConfigDTO request)
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
            throw new ExcepcionDominio(ex.Message); // Para que se intercepte y transforme en 400
        }
        catch
        {
            await uow.RollbackAsync();
            throw;
        }
    }
}
