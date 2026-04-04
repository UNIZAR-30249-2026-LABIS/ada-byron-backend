namespace AdaByron.Domain.Aggregates.SpaceAggregate;

using AdaByron.Domain.Exceptions;

/// <summary>
/// Aforo de un espacio: número máximo de personas que pueden estar en él (HU-14).
/// </summary>
public record Aforo
{
    public int Valor { get; }

    private Aforo(int valor)
    {
        if (valor <= 0)
            throw new ExcepcionDominio("El aforo debe ser mayor que cero.");
        Valor = valor;
    }

    public static Aforo De(int valor) => new(valor);

    public override string ToString() => Valor.ToString();
}
