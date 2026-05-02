using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
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

    public required string       CodigoEspacio    { get; init; }
    public string                Nombre           { get; private set; } = string.Empty;
    public Planta                Planta           { get; private set; } = Planta.De(0);
    public Aforo                 Aforo            { get; private set; } = Aforo.De(1);
    public TipoEspacio           TipoFisico       { get; }
    public TipoEspacio           CategoriaReserva { get; private set; }
    public bool                  EsReservable     { get; private set; }
    [JsonIgnore]
    public string                HorarioReservaJson { get; private set; } = HorarioReservaDia.Serialize(HorarioReservaDia.CrearHorarioPorDefecto());
    public required Departamento Departamento     { get; init; }
    [NotMapped]
    public IReadOnlyCollection<HorarioReservaDia> HorarioReserva => HorarioReservaDia.Deserialize(HorarioReservaJson);

    /// <summary>
    /// Porcentaje de ocupación máximo específico para este espacio (PBI-12 / HU-O1).
    /// Si es null se hereda el porcentaje global del edificio configurado en EdificioConfig.
    /// Rango válido cuando tiene valor: [0, 100].
    /// </summary>
    public double? PorcentajeOcupacionEspecifico { get; private set; }

    public IReadOnlyCollection<Reserva> Reservas => _reservas.AsReadOnly();

    private Espacio() { }

    [SetsRequiredMembers]
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
        EsReservable      = tipoFisico != TipoEspacio.Despacho;
        HorarioReservaJson = HorarioReservaDia.Serialize(HorarioReservaDia.CrearHorarioPorDefecto());
        Departamento      = departamento ?? Departamento.Null;
    }

    public void UpdateDetails(string nombre, Planta planta, Aforo aforo, TipoEspacio categoria, bool esReservable, IEnumerable<HorarioReservaDia>? horarioReserva = null)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ExcepcionDominio("El nombre no puede estar vacío.");
        Nombre = nombre.Trim();
        Planta = planta;
        Aforo = aforo;
        CategoriaReserva = categoria;
        ActualizarConfiguracionReserva(esReservable, horarioReserva);
    }

    // ── Gestión de Reservas (Espacio como Aggregate Root) ───────────────────

    public void AddReserva(Reserva reserva, EdificioConfig configEdificio, Persona persona)
    {
        VerificarConfiguracionReserva(reserva.Franja);

        // 1. Verificar Permisos (HU-13)
        VerificarPermisos(persona);

        // 2. Verificar disponibilidad horaria (Regla F6 / HU-15)
        if (!IsDisponible(reserva.Franja))
            throw new ExcepcionConflictoReserva($"El espacio '{CodigoEspacio}' ya tiene una reserva en ese horario.");

        // 3. Verificar aforo dinámico (Regla F5 / HU-14 / PBI-12)
        int maximoPermitido = CalcularAforoEfectivo(configEdificio);

        if (reserva.NumeroAsistentes > maximoPermitido)
        {
            double porcentajeAplicado = PorcentajeOcupacionEspecifico ?? configEdificio.PorcentajeOcupacion;
            string origen = PorcentajeOcupacionEspecifico.HasValue ? "específico del espacio" : "global del edificio";
            throw new ExcepcionAforoSuperado(
                $"Aforo superado. Máximo permitido: {maximoPermitido} ({porcentajeAplicado}% {origen} de {Aforo.Valor}).");
        }

        _reservas.Add(reserva);
    }

    public bool IsDisponible(FranjaHoraria franja)
    {
        return !_reservas.Any(r => r.Estado == EstadoReserva.Aceptada && r.Franja.Overlaps(franja));
    }

    public void ActualizarConfiguracionReserva(bool esReservable, IEnumerable<HorarioReservaDia>? horarioReserva)
    {
        if (TipoFisico == TipoEspacio.Despacho && esReservable)
            throw new ExcepcionDominio("Los despachos no pueden hacerse reservables.");

        var horarioNormalizado = HorarioReservaDia.Normalizar(horarioReserva?.ToList());

        if (esReservable && horarioNormalizado.All(h => !h.Activo))
            throw new ExcepcionDominio("Un espacio reservable debe tener al menos un día activo en su horario.");

        EsReservable = esReservable;
        HorarioReservaJson = HorarioReservaDia.Serialize(horarioNormalizado);
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

    private void VerificarConfiguracionReserva(FranjaHoraria franja)
    {
        if (!EsReservable)
            throw new ExcepcionDominio($"El espacio '{CodigoEspacio}' no está habilitado para reservas.");

        if (!HorarioReservaDia.PermiteReserva(HorarioReserva, franja))
            throw new ExcepcionDominio($"El espacio '{CodigoEspacio}' no permite reservas en el horario solicitado.");
    }

    /// <summary>
    /// Actualiza las especificaciones mutables del espacio (HU-XX Admin).
    /// Solo puede ser invocado por el Gerente a través del caso de uso correspondiente.
    /// </summary>
    public void Actualizar(string nuevoNombre, Aforo nuevoAforo, Planta nuevaPlanta, TipoEspacio nuevaCategoria, bool esReservable, IEnumerable<HorarioReservaDia>? horarioReserva)
    {
        if (string.IsNullOrWhiteSpace(nuevoNombre))
            throw new ExcepcionDominio("La designación del espacio no puede estar vacía.");
        if (nuevoNombre.Trim().Length > 200)
            throw new ExcepcionDominio("La designación no puede superar los 200 caracteres.");

        Nombre           = nuevoNombre.Trim();
        Aforo            = nuevoAforo  ?? throw new ExcepcionDominio("El aforo es obligatorio.");
        Planta           = nuevaPlanta ?? throw new ExcepcionDominio("La planta es obligatoria.");
        CategoriaReserva = nuevaCategoria;
        ActualizarConfiguracionReserva(esReservable, horarioReserva);
    }

    /// <summary>
    /// Establece o elimina el porcentaje de ocupación específico de este espacio (PBI-12 / HU-O1).
    /// Pasar null elimina la restricción específica y se aplicará el porcentaje global del edificio.
    /// </summary>
    public void SetPorcentajeEspecifico(double? porcentaje)
    {
        if (porcentaje.HasValue && (porcentaje.Value < 0 || porcentaje.Value > 100))
            throw new ExcepcionDominio("El porcentaje específico debe estar entre 0 y 100.");

        PorcentajeOcupacionEspecifico = porcentaje;
    }

    /// <summary>
    /// Calcula el aforo efectivo del espacio aplicando el porcentaje correcto (PBI-12).
    /// Usa el porcentaje específico del espacio si está definido; si no, usa el global del edificio.
    /// </summary>
    public int CalcularAforoEfectivo(EdificioConfig configEdificio)
    {
        double porcentaje = PorcentajeOcupacionEspecifico ?? configEdificio.PorcentajeOcupacion;
        return Edificio.AdaByron.CalcularCapacidadPermitida(Aforo.Valor, porcentaje);
    }

    public override bool Equals(object? obj) =>
        obj is Espacio otro && CodigoEspacio == otro.CodigoEspacio;

    public override int GetHashCode() =>
        CodigoEspacio.GetHashCode(StringComparison.Ordinal);
}
