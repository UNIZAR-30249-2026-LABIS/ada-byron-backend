using AdaByron.Domain.Exceptions;

namespace AdaByron.Domain.ValueObjects;

// Valor inmutable que representa el aforo máximo de un espacio. Debe ser > 0.
public sealed record Aforo
{
    public int Valor { get; }

    public Aforo(int valor)
    {
        if (valor <= 0 || valor > 200)
            throw new ExcepcionDominio(
                $"El aforo debe ser mayor que 0 y menor que 200. Valor recibido: {valor}.");

        Valor = valor;
    }

    public static implicit operator int(Aforo aforo) => aforo.Valor;

    public static Aforo De(int valor) => new(valor);

    public override string ToString() => $"{Valor} personas";
}
