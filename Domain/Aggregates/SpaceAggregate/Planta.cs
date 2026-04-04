namespace AdaByron.Domain.Aggregates.SpaceAggregate;

using AdaByron.Domain.Exceptions;

/// <summary>
/// Planta del edificio (HU-13).
/// </summary>
public record Planta
{
    public int Valor { get; }

    private Planta(int valor)
    {
        if (valor < 0 || valor > 9)
            throw new ExcepcionDominio("Planta no válida para el edificio Ada Byron (0-9).");
        Valor = valor;
    }

    public static Planta De(int valor) => new(valor);

    public override string ToString() => Valor.ToString();
}
