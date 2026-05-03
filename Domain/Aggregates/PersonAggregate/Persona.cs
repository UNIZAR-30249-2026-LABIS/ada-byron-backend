using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
namespace AdaByron.Domain.Aggregates.PersonAggregate;

using AdaByron.Domain.Exceptions;

/// <summary>
/// Root del Agregado Persona (HU-13).
/// </summary>
public sealed class Persona
{
    public string Email        { get; private set; } = string.Empty;
    public string Nombre       { get; private set; } = string.Empty;
    public string Apellidos    { get; private set; } = string.Empty;
    public string RolesSerializados  { get; private set; } = SerializeRoles([Rol.Estudiante]);

    private Departamento? _departamento;
    public Departamento Departamento
    {
        get => _departamento ?? Departamento.Null;
        private set => _departamento = value;
    }

    public IReadOnlyCollection<Rol> Roles => DeserializeRoles(RolesSerializados);
    public bool EsGerente => Roles.Contains(Rol.Gerente);
    public Rol Rol => Roles.FirstOrDefault(r => r != Rol.Gerente, Roles.First());

    // Requerido por EF Core — no invocar desde dominio
    private Persona() { }

    [SetsRequiredMembers]
    public Persona(string email, string nombre, string apellidos, Rol rol, Departamento? departamento = null, bool esGerente = false)
        : this(email, nombre, apellidos, BuildRoles(rol, esGerente), departamento)
    {
    }

    [SetsRequiredMembers]
    public Persona(string email, string nombre, string apellidos, IEnumerable<Rol> roles, Departamento? departamento = null)
    {
        ValidarEmail(email);
        (Nombre, Apellidos, RolesSerializados, _departamento) = NormalizarDatos(nombre, apellidos, roles, departamento);
        Email = email.Trim().ToLowerInvariant();
    }

    public string NombreCompleto => $"{Nombre} {Apellidos}";

    public void ActualizarDatosAdministrativos(string nombre, string apellidos, Rol rol, Departamento? departamento = null, bool esGerente = false)
        => ActualizarDatosAdministrativos(nombre, apellidos, BuildRoles(rol, esGerente), departamento);

    public void ActualizarDatosAdministrativos(string nombre, string apellidos, IEnumerable<Rol> roles, Departamento? departamento = null)
    {
        (Nombre, Apellidos, RolesSerializados, _departamento) = NormalizarDatos(nombre, apellidos, roles, departamento);
    }

    private static void ValidarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ExcepcionDominio("El email no puede estar vacío.");

        if (!email.Contains('@') || !email.Contains('.'))
            throw new ExcepcionDominio($"El email '{email}' no tiene un formato válido.");
    }

    private static (string nombre, string apellidos, string rolesSerializados, Departamento departamento) NormalizarDatos(
        string nombre,
        string apellidos,
        IEnumerable<Rol> roles,
        Departamento? departamento)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ExcepcionDominio("El nombre no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(apellidos))
            throw new ExcepcionDominio("Los apellidos no pueden estar vacíos.");

        var normalizedRoles = roles
            .Distinct()
            .OrderBy(r => r)
            .ToArray();

        if (normalizedRoles.Length == 0)
            throw new ExcepcionDominio("Una persona debe tener al menos un rol.");

        if (normalizedRoles.Length > 2)
            throw new ExcepcionDominio("Una persona no puede tener más de dos roles.");

        if (normalizedRoles.Length == 2 &&
            !(normalizedRoles.Contains(Rol.Gerente) && normalizedRoles.Contains(Rol.DocenteInvestigador)))
        {
            throw new ExcepcionDominio("La única combinación de roles permitida es 'Gerente' + 'DocenteInvestigador'.");
        }

        var dpt = departamento ?? Departamento.Null;
        var requiereDepartamento = normalizedRoles.Any(r =>
            r is Rol.InvestigadorContratado or Rol.DocenteInvestigador or Rol.TecnicoLaboratorio);

        if (requiereDepartamento && dpt.IsNull)
            throw new ExcepcionDominio("La persona requiere uno de los dos departamentos válidos.");

        if (!requiereDepartamento && !dpt.IsNull)
            throw new ExcepcionDominio("El rol indicado no admite departamento.");

        return (nombre.Trim(), apellidos.Trim(), SerializeRoles(normalizedRoles), dpt);
    }

    public override bool Equals(object? obj) =>
        obj is Persona otra && Email == otra.Email;

    public override int GetHashCode() => Email.GetHashCode(StringComparison.Ordinal);

    public override string ToString() => $"Persona({Email}, Roles=[{string.Join(", ", Roles)}], Dept={Departamento})";

    private static IEnumerable<Rol> BuildRoles(Rol rol, bool esGerente)
    {
        if (rol == Rol.Gerente || !esGerente)
            return [rol];

        return [rol, Rol.Gerente];
    }

    private static string SerializeRoles(IEnumerable<Rol> roles) =>
        JsonSerializer.Serialize(roles);

    private static IReadOnlyCollection<Rol> DeserializeRoles(string serialized)
    {
        if (string.IsNullOrWhiteSpace(serialized))
            return [Rol.Estudiante];

        try
        {
            var roles = JsonSerializer.Deserialize<List<Rol>>(serialized);
            if (roles is { Count: > 0 })
                return roles;
        }
        catch (JsonException)
        {
            // Soporte defensivo para datos previos a la migración JSON.
        }

        return [serialized.Trim() switch
        {
            "TecnicoLab" => Rol.TecnicoLaboratorio,
            "Docente" => Rol.DocenteInvestigador,
            "Investigador" => Rol.InvestigadorContratado,
            "Gerente" => Rol.Gerente,
            "Conserje" => Rol.Conserje,
            _ => Rol.Estudiante
        }];
    }
}
