using System.Diagnostics.CodeAnalysis;
namespace AdaByron.Domain.Aggregates.PersonAggregate;

using AdaByron.Domain.Exceptions;

/// <summary>
/// Root del Agregado Persona (HU-13).
/// </summary>
public sealed class Persona
{
    public required string Email        { get; init; }
    public required string Nombre       { get; init; }
    public required string Apellidos    { get; init; }
    public Rol          Rol          { get; private set; }

    private Departamento? _departamento;
    public Departamento Departamento
    {
        get => _departamento ?? new Departamento("Sin Departamento");
        private set => _departamento = value;
    }

    // Requerido por EF Core — no invocar desde dominio
    private Persona() { }

    [SetsRequiredMembers]
    public Persona(string email, string nombre, string apellidos, Rol rol, Departamento? departamento = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ExcepcionDominio("El email no puede estar vacío.");

        if (!email.Contains('@') || !email.Contains('.'))
            throw new ExcepcionDominio($"El email '{email}' no tiene un formato válido.");

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ExcepcionDominio("El nombre no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(apellidos))
            throw new ExcepcionDominio("Los apellidos no pueden estar vacíos.");

        var dpt = departamento ?? Departamento.Null;
        if ((rol is Rol.TecnicoLab or Rol.Docente) && dpt == Departamento.Null)
            throw new ExcepcionDominio($"El rol '{rol}' requiere especificar un departamento.");

        Email        = email.Trim().ToLowerInvariant();
        Nombre       = nombre.Trim();
        Apellidos    = apellidos.Trim();
        Rol          = rol;
        _departamento = dpt == Departamento.Null ? new Departamento("Sin Departamento") : dpt;
    }

    public string NombreCompleto => $"{Nombre} {Apellidos}";

    public override bool Equals(object? obj) =>
        obj is Persona otra && Email == otra.Email;

    public override int GetHashCode() => Email.GetHashCode(StringComparison.Ordinal);

    public override string ToString() => $"Persona({Email}, {Rol}, Dept={Departamento})";
}
