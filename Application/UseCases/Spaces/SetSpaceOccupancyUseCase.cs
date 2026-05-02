using AdaByron.Application.DTOs;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Interfaces;

namespace AdaByron.Application.UseCases.Spaces;

/// <summary>
/// Caso de uso PBI-12 (HU-O1): Define o elimina el porcentaje de uso específico de un espacio.
/// Criterios de aceptación:
///   - Si PorcentajeEspecifico es null → se elimina la restricción y el espacio hereda el % global del edificio.
///   - Si PorcentajeEspecifico tiene valor ∈ [0, 100] → se usa ese % para calcular el aforo efectivo del espacio.
/// Solo accesible por el rol Gerente (la autorización se aplica en el controlador).
/// </summary>
public class SetSpaceOccupancyUseCase(IEspacioRepository espacios)
{
    public async Task ExecuteAsync(string codigoEspacio, SetSpaceOccupancyDTO request)
    {
        // 1. Obtener el espacio
        var espacio = await espacios.GetByCodigoAsync(codigoEspacio)
            ?? throw new ExcepcionDominio($"No existe ningún espacio con código '{codigoEspacio}'.");

        // 2. Delegar la validación y mutación al Aggregate Root (encapsula las invariantes del dominio)
        espacio.SetPorcentajeEspecifico(request.PorcentajeEspecifico);

        // 3. Persistir
        await espacios.UpdateAsync(espacio);
    }
}
