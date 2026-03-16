using AdaByron.Domain.Enums;
using AdaByron.Domain.Exceptions;

namespace AdaByron.Domain.Entities;

// Persona identificada por Email (identidad inmutable).
// TecnicoLab y Docente requieren Departamento obligatorio.
public sealed class Persona
{
    public string  Email        { get; }
    public string  Nombre       { get; private set; }
    public string  Apellidos    { get; private set; }
    public Rol     Rol          { get; private set; }
    public string? Departamento { get; private set; }

    public Persona(string email, string nombre, string apellidos, Rol rol, string? departamento = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ExcepcionDominio("El email no puede estar vacío.");

        if (!email.Contains('@') || !email.Contains('.'))
            throw new ExcepcionDominio($"El email '{email}' no tiene un formato válido.");

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ExcepcionDominio("El nombre no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(apellidos))
            throw new ExcepcionDominio("Los apellidos no pueden estar vacíos.");

        if ((rol is Rol.TecnicoLab or Rol.Docente) && string.IsNullOrWhiteSpace(departamento))
            throw new ExcepcionDominio($"El rol '{rol}' requiere especificar un departamento.");

        Email        = email.Trim().ToLowerInvariant();
        Nombre       = nombre.Trim();
        Apellidos    = apellidos.Trim();
        Rol          = rol;
        Departamento = departamento?.Trim();
    }

    public string NombreCompleto => $"{Nombre} {Apellidos}";

    public override bool Equals(object? obj) =>
        obj is Persona otra && Email == otra.Email;

    public override int GetHashCode() => Email.GetHashCode(StringComparison.Ordinal);

    public override string ToString() => $"Persona({Email}, {Rol})";
}
