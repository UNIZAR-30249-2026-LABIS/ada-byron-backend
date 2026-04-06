namespace AdaByron.Domain.Aggregates.SpaceAggregate;

using AdaByron.Domain.Aggregates.ReservationAggregate;
using AdaByron.Domain.Aggregates.PersonAggregate;
using AdaByron.Domain.Exceptions;

/// <summary>
/// Aggregate Root de Espacios (HU-13). 
/// Responsable de gestionar su propia disponibilidad y aforo (HU-14 y HU-15).
/// </summary>
public sealed class Espacio
{
    private readonly List<Reserva> _reservas = new();

    public string       CodigoEspacio    { get; }
    public string       Nombre           { get; private set; }
    public Planta       Planta           { get; private set; }
    public Aforo        Aforo            { get; private set; }
    public TipoEspacio  TipoFisico       { get; }
    public TipoEspacio  CategoriaReserva { get; private set; }
    public Departamento Departamento     { get; private set; }

    public IReadOnlyCollection<Reserva> Reservas => _reservas.AsReadOnly();

    private Espacio() { }

    public Espacio(string codigoEspacio, string nombre, Planta planta, Aforo aforo, TipoEspacio tipoFisico, Departamento? departamento = null)
    {
        if (string.IsNullOrWhiteSpace(codigoEspacio))
            throw new ExcepcionDominio("Còdigo de espacio obligatorio.");

        CodigoEspacio     = codigoEspacio.Trim().ToUpperInvariant();
        Nombre            = nombre.Trim();
        Planta            = planta;
        Aforo             = aforo;
        TipoFisico        = tipoFisico;
        CategoriaReserva  = tipoFisico;
        Departamento      = departamento ?? Departamento.Null;
    }

    // ── Gestión de Reservas (Espacio como Aggregate Root) ───────────────────

    public void AddReserva(Reserva reserva, EdificioConfig configEdificio, Persona persona)
    {
        // 1. Verificar Permisos (HU-13)
        VerificarPermisos(persona);

        // 2. Verificar disponibilidad horaria (Regla F6 / HU-15)
        if (!IsDisponible(reserva.Franja))
            throw new ExcepcionConflictoReserva($"El espacio '{CodigoEspacio}' ya tiene una reserva en ese horario.");

        // 3. Verificar aforo dinámico (Regla F5 / HU-14)
        var edificio = Edificio.AdaByron;
        int maximoPermitido = edificio.CalcularCapacidadPermitida(Aforo.Valor, configEdificio.PorcentajeOcupacion);
        
        if (reserva.NumeroAsistentes > maximoPermitido)
            throw new ExcepcionAforoSuperado($"Aforo superado. Máximo permitido: {maximoPermitido} ({configEdificio.PorcentajeOcupacion}% de {Aforo.Valor}).");

        _reservas.Add(reserva);
    }

    public bool IsDisponible(FranjaHoraria franja)
    {
        return !_reservas.Any(r => r.Estado == EstadoReserva.Aceptada && r.Franja.Overlaps(franja));
    }

    private void VerificarPermisos(Persona persona)
    {
        bool permitido = persona.Rol switch
        {
            Rol.Estudiante => CategoriaReserva == TipoEspacio.SalaComun,
            Rol.TecnicoLab => CategoriaReserva switch
            {
                TipoEspacio.SalaComun => true,
                TipoEspacio.Seminario => true,
                TipoEspacio.Laboratorio => persona.Departamento.IsSameAs(Departamento),
                _ => false
            },
            Rol.Docente => CategoriaReserva switch
            {
                TipoEspacio.SalaComun => true,
                TipoEspacio.Aula => true,
                TipoEspacio.Seminario => true,
                TipoEspacio.Laboratorio => persona.Departamento.IsSameAs(Departamento),
                _ => false
            },
            Rol.Conserje => CategoriaReserva != TipoEspacio.Despacho,
            Rol.Gerente  => CategoriaReserva != TipoEspacio.Despacho,
            _ => false
        };

        if (!permitido)
            throw new ExcepcionPermisos($"El rol '{persona.Rol}' no tiene permiso para reservar espacios del tipo '{CategoriaReserva}'.");
    }

    public override bool Equals(object? obj) =>
        obj is Espacio otro && CodigoEspacio == otro.CodigoEspacio;

    public override int GetHashCode() =>
        CodigoEspacio.GetHashCode(StringComparison.Ordinal);
}
