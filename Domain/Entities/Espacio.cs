using AdaByron.Domain.Enums;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.ValueObjects;

namespace AdaByron.Domain.Entities;

// Espacio reservable del edificio Ada Byron, identificado por CodigoEspacio (inmutable).
// TipoFisico es permanente; CategoriaReserva puede cambiar según la Matriz de Mutabilidad (Regla C).
public sealed class Espacio
{
    public string      CodigoEspacio    { get; }
    public string      Nombre           { get; private set; }
    public Planta      Planta           { get; private set; }
    public Aforo       Aforo            { get; private set; }
    public TipoEspacio TipoFisico       { get; }                // inmutable
    public TipoEspacio CategoriaReserva { get; private set; }   // mutable (Regla C)
    public string?     Departamento     { get; private set; }   // para laboratorios (Regla F3)

    // Matriz de Mutabilidad (Regla C):
    //   Aula        → Aula, Seminario, SalaComun
    //   Laboratorio → solo Laboratorio (bloqueado)
    //   Seminario   → Seminario, SalaComun
    //   SalaComun   → SalaComun, Seminario
    //   Despacho    → solo Despacho (bloqueado)
    private static readonly IReadOnlyDictionary<TipoEspacio, IReadOnlySet<TipoEspacio>>
        _transicionesPermitidas = new Dictionary<TipoEspacio, IReadOnlySet<TipoEspacio>>
        {
            [TipoEspacio.Aula]        = new HashSet<TipoEspacio> { TipoEspacio.Aula, TipoEspacio.Seminario, TipoEspacio.SalaComun },
            [TipoEspacio.Laboratorio] = new HashSet<TipoEspacio> { TipoEspacio.Laboratorio },
            [TipoEspacio.Seminario]   = new HashSet<TipoEspacio> { TipoEspacio.Seminario, TipoEspacio.SalaComun },
            [TipoEspacio.SalaComun]   = new HashSet<TipoEspacio> { TipoEspacio.SalaComun, TipoEspacio.Seminario },
            [TipoEspacio.Despacho]    = new HashSet<TipoEspacio> { TipoEspacio.Despacho },
        };

    public Espacio(string codigoEspacio, string nombre, Planta planta, Aforo aforo, TipoEspacio tipoFisico)
    {
        if (string.IsNullOrWhiteSpace(codigoEspacio))
            throw new ExcepcionDominio("El código de espacio no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ExcepcionDominio("El nombre del espacio no puede estar vacío.");

        CodigoEspacio    = codigoEspacio.Trim().ToUpperInvariant();
        Nombre           = nombre.Trim();
        Planta           = planta;
        Aforo            = aforo ?? throw new ExcepcionDominio("El aforo no puede ser nulo.");
        TipoFisico       = tipoFisico;
        CategoriaReserva = tipoFisico;
    }

    // Constructor con departamento (laboratorios)
    public Espacio(string codigoEspacio, string nombre, Planta planta, Aforo aforo,
                   TipoEspacio tipoFisico, string? departamento)
        : this(codigoEspacio, nombre, planta, aforo, tipoFisico)
    {
        Departamento = departamento?.Trim();
    }

    // Cambia la categoría de reserva respetando la Matriz de Mutabilidad.
    public void CambiarCategoria(TipoEspacio nuevaCategoria)
    {
        if (CategoriaReserva == nuevaCategoria) return;

        if (!_transicionesPermitidas[CategoriaReserva].Contains(nuevaCategoria))
            throw new ExcepcionCambioCategoria(
                $"No se puede cambiar la categoría de '{CategoriaReserva}' a '{nuevaCategoria}' " +
                $"en el espacio '{CodigoEspacio}'.");

        CategoriaReserva = nuevaCategoria;
    }

    // ── Regla F6: Disponibilidad ──────────────────────────────────────────────
    // Devuelve true si no hay reservas en estado Aceptada que solapen con la franja dada.
    public bool IsDisponible(FranjaHoraria franja, IEnumerable<Reserva> reservasActivas)
    {
        return !reservasActivas.Any(r =>
            r.EspacioId == CodigoEspacio
            && r.Estado  == EstadoReserva.Aceptada
            && r.Franja.Inicio < franja.Fin
            && r.Franja.Fin   > franja.Inicio);
    }

    public override bool Equals(object? obj) =>
        obj is Espacio otro && CodigoEspacio == otro.CodigoEspacio;

    public override int GetHashCode() =>
        CodigoEspacio.GetHashCode(StringComparison.Ordinal);

    public override string ToString() =>
        $"Espacio({CodigoEspacio}, {CategoriaReserva}, Aforo={Aforo})";
}
