using System.Diagnostics.CodeAnalysis;
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
        ValidarEmail(email);
        (Nombre, Apellidos, Rol, _departamento) = NormalizarDatos(nombre, apellidos, rol, departamento);
        Email = email.Trim().ToLowerInvariant();
    }

    public string NombreCompleto => $"{Nombre} {Apellidos}";

    public void ActualizarDatosAdministrativos(string nombre, string apellidos, Rol rol, Departamento? departamento = null)
    {
        (Nombre, Apellidos, Rol, _departamento) = NormalizarDatos(nombre, apellidos, rol, departamento);
    }

    private static void ValidarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ExcepcionDominio("El email no puede estar vacío.");

        if (!email.Contains('@') || !email.Contains('.'))
            throw new ExcepcionDominio($"El email '{email}' no tiene un formato válido.");
    }

    private static (string nombre, string apellidos, Rol rol, Departamento departamento) NormalizarDatos(
        string nombre,
        string apellidos,
        Rol rol,
        Departamento? departamento)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ExcepcionDominio("El nombre no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(apellidos))
            throw new ExcepcionDominio("Los apellidos no pueden estar vacíos.");

        var dpt = departamento ?? Departamento.Null;
        if ((rol is Rol.TecnicoLab or Rol.Docente) && dpt == Departamento.Null)
            throw new ExcepcionDominio($"El rol '{rol}' requiere especificar un departamento.");

        var normalizedDept = dpt == Departamento.Null ? new Departamento("Sin Departamento") : dpt;
        return (nombre.Trim(), apellidos.Trim(), rol, normalizedDept);
    }

    public override bool Equals(object? obj) =>
        obj is Persona otra && Email == otra.Email;

    public override int GetHashCode() => Email.GetHashCode(StringComparison.Ordinal);

    public override string ToString() => $"Persona({Email}, {Rol}, Dept={Departamento})";
}
