using AdaByron.Application.DTOs;
using AdaByron.Domain.Interfaces;

namespace AdaByron.Application.UseCases.Admin;

/// <summary>
/// Devuelve la lista completa de personas registradas en el sistema, ordenada alfabéticamente.
/// </summary>
public class GetStaffUseCase(IPersonaRepository personas)
{
    public async Task<IEnumerable<StaffDTO>> ExecuteAsync()
    {
        var all = await personas.GetAllAsync();
        return all
            .OrderBy(p => p.Apellidos)
            .ThenBy(p => p.Nombre)
            .Select(p => new StaffDTO
            {
                Email       = p.Email,
                Nombre      = p.Nombre,
                Apellidos   = p.Apellidos,
                Rol         = p.Rol.ToString(),
                Roles       = p.Roles.Select(r => r.ToString()).ToArray(),
                EsGerente   = p.EsGerente,
                Departamento = p.Departamento.IsNull ? null : p.Departamento.Nombre
            });
    }
}
