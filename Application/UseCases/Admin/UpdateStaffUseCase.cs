using AdaByron.Application.DTOs;
using AdaByron.Domain.Aggregates.PersonAggregate;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Interfaces;

namespace AdaByron.Application.UseCases.Admin;

/// <summary>
/// Actualiza los datos administrativos (rol, departamento, flag gerente) de una persona existente.
/// </summary>
public class UpdateStaffUseCase(IPersonaRepository personas)
{
    public async Task<StaffDTO> ExecuteAsync(string email, UpdateStaffRequestDTO dto)
    {
        var persona = await personas.GetByEmailAsync(email)
            ?? throw new ExcepcionDominio($"No existe ninguna persona con email '{email}'.");

        var rol = ParseRol(dto.Rol);
        var departamento = ParseDepartamento(dto.Departamento);

        persona.ActualizarDatosAdministrativos(dto.Nombre, dto.Apellidos, rol, departamento, dto.EsGerente);

        await personas.UpdateAsync(persona);

        return CreateStaffUseCase.ToDTO(persona);
    }

    private static Rol ParseRol(string rol)
    {
        if (!Enum.TryParse<Rol>(rol, ignoreCase: true, out var parsed))
            throw new ExcepcionDominio($"Rol '{rol}' no reconocido.");
        return parsed;
    }

    private static Departamento? ParseDepartamento(string? departamento)
        => string.IsNullOrWhiteSpace(departamento) ? Departamento.Null : Departamento.From(departamento.Trim());
}
