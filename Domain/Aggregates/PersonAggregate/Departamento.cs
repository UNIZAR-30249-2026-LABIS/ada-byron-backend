namespace AdaByron.Domain.Aggregates.PersonAggregate;

/// <summary>
/// Valor que representa uno de los dos departamentos permitidos por el enunciado.
/// </summary>
public sealed class Departamento : IEquatable<Departamento>
{
    public static Departamento Null => new(string.Empty, true);
    public static Departamento Informatica => new("Informática", true);
    public static Departamento SistemasElectronicaComunicaciones =>
        new("Ing. de Sistemas e Ing. Electrónica y Comunicaciones", true);

    public string Nombre { get; private set; } = string.Empty;

    private Departamento() { }

    public Departamento(string nombre)
        : this(nombre, false)
    {
    }

    private Departamento(string nombre, bool skipValidation)
    {
        Nombre = Normalize(nombre, skipValidation);
    }

    public bool IsNull => string.IsNullOrWhiteSpace(Nombre);

    public static Departamento From(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return Null;

        var normalized = nombre.Trim();

        if (normalized.Equals("Informática", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("Informatica", StringComparison.OrdinalIgnoreCase))
        {
            return Informatica;
        }

        if (normalized.Equals("Ing. de Sistemas e Ing. Electrónica y Comunicaciones", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("Ing. de Sistemas e Ing. Electronica y Comunicaciones", StringComparison.OrdinalIgnoreCase))
        {
            return SistemasElectronicaComunicaciones;
        }

        throw new ArgumentException(
            "Departamento no válido. Solo se permiten 'Informática' e 'Ing. de Sistemas e Ing. Electrónica y Comunicaciones'.");
    }

    public bool IsSameAs(Departamento? other) =>
        other != null &&
        !IsNull &&
        Nombre.Equals(other.Nombre, StringComparison.OrdinalIgnoreCase);

    public bool Equals(Departamento? other) =>
        other is not null &&
        Nombre.Equals(other.Nombre, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => Equals(obj as Departamento);

    public override int GetHashCode() => Nombre.ToUpperInvariant().GetHashCode();

    public override string ToString() => Nombre;

    private static string Normalize(string? nombre, bool skipValidation)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return string.Empty;

        if (skipValidation)
            return nombre.Trim();

        return From(nombre).Nombre;
    }
}
