namespace AdaByron.Domain.Aggregates.PersonAggregate;

/// <summary>
/// Valor que representa un departamento universitario (HU-13).
/// </summary>
public record Departamento(string Nombre)
{
    public static Departamento Null => new(string.Empty);

    public bool IsSameAs(Departamento? other) =>
        other != null && 
        !string.IsNullOrWhiteSpace(Nombre) && 
        Nombre.Trim().Equals(other.Nombre.Trim(), StringComparison.OrdinalIgnoreCase);

    public override string ToString() => Nombre;
}
