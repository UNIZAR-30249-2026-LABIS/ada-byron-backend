using AdaByron.Application.DTOs;
using AdaByron.Domain.Aggregates.PersonAggregate;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Interfaces;

namespace AdaByron.Application.UseCases.Admin;

/// <summary>
/// Registra una nueva persona en el sistema tras validar que el email no existe ya.
/// </summary>
public class CreateStaffUseCase(IPersonaRepository personas)
{
    public async Task<StaffDTO> ExecuteAsync(CreateStaffRequestDTO dto)
    {
        if (await personas.ExistsAsync(dto.Email))
            throw new ExcepcionDominio($"Ya existe una persona registrada con el email '{dto.Email}'.");

        var rol = ParseRol(dto.Rol);
        var departamento = ParseDepartamento(dto.Departamento);

        var persona = new Persona(dto.Email, dto.Nombre, dto.Apellidos, rol, departamento, dto.EsGerente);

        await personas.AddAsync(persona);

        return ToDTO(persona);
    }

    private static Rol ParseRol(string rol)
    {
        if (!Enum.TryParse<Rol>(rol, ignoreCase: true, out var parsed))
            throw new ExcepcionDominio($"Rol '{rol}' no reconocido.");
        return parsed;
    }

    private static Departamento? ParseDepartamento(string? departamento)
        => string.IsNullOrWhiteSpace(departamento) ? Departamento.Null : Departamento.From(departamento.Trim());

    internal static StaffDTO ToDTO(Persona p) => new()
    {
        Email        = p.Email,
        Nombre       = p.Nombre,
        Apellidos    = p.Apellidos,
        Rol          = p.Rol.ToString(),
        Roles        = p.Roles.Select(r => r.ToString()).ToArray(),
        EsGerente    = p.EsGerente,
        Departamento = p.Departamento.IsNull ? null : p.Departamento.Nombre
    };
}
