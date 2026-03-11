using AdaByron.Domain.Exceptions;

namespace AdaByron.Domain.ValueObjects;

// Valor inmutable que representa la planta de un espacio. Rango válido: 0 (baja) – 4.
public sealed record Planta
{
    public int Valor { get; }

    public Planta(int valor)
    {
        if (valor < 0 || valor > 4)
            throw new ExcepcionDominio(
                $"La planta debe estar entre 0 y 4. Valor recibido: {valor}.");

        Valor = valor;
    }

    public static implicit operator int(Planta planta) => planta.Valor;

    public static Planta De(int valor) => new(valor);

    public override string ToString() => $"Planta {Valor}";
}
