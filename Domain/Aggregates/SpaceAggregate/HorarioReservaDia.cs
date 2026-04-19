using System.Text.Json;
using AdaByron.Domain.Aggregates.ReservationAggregate;
using AdaByron.Domain.Exceptions;

namespace AdaByron.Domain.Aggregates.SpaceAggregate;

/// <summary>
/// Representa la disponibilidad semanal de un espacio para reservas.
/// Se serializa como JSON dentro del agregado para mantener el modelo simple.
/// </summary>
public sealed class HorarioReservaDia
{
    public int DiaSemana { get; init; }
    public bool Activo { get; init; }
    public string HoraInicio { get; init; } = "00:00";
    public string HoraFin { get; init; } = "23:59";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static IReadOnlyList<HorarioReservaDia> CrearHorarioPorDefecto()
    {
        return Enumerable.Range(0, 7)
            .Select(dia => new HorarioReservaDia
            {
                DiaSemana = dia,
                Activo = true,
                HoraInicio = "00:00",
                HoraFin = "23:59"
            })
            .ToList();
    }

    public static IReadOnlyList<HorarioReservaDia> Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return CrearHorarioPorDefecto();

        try
        {
            var items = JsonSerializer.Deserialize<List<HorarioReservaDia>>(json, JsonOptions);
            return Normalizar(items);
        }
        catch
        {
            return CrearHorarioPorDefecto();
        }
    }

    public static string Serialize(IEnumerable<HorarioReservaDia>? horarios)
    {
        var normalizados = Normalizar(horarios?.ToList());
        return JsonSerializer.Serialize(normalizados, JsonOptions);
    }

    public static bool PermiteReserva(IEnumerable<HorarioReservaDia> horarios, FranjaHoraria franja)
    {
        if (franja.Inicio.Date != franja.Fin.Date)
            return false;

        var horarioDia = Normalizar(horarios.ToList())
            .SingleOrDefault(h => h.DiaSemana == (int)franja.Inicio.DayOfWeek);

        if (horarioDia is null || !horarioDia.Activo)
            return false;

        var inicioReserva = TimeOnly.FromDateTime(franja.Inicio);
        var finReserva = TimeOnly.FromDateTime(franja.Fin);
        var inicioHorario = ParseTime(horarioDia.HoraInicio);
        var finHorario = ParseTime(horarioDia.HoraFin);

        return inicioReserva >= inicioHorario && finReserva <= finHorario;
    }

    public static IReadOnlyList<HorarioReservaDia> Normalizar(IReadOnlyCollection<HorarioReservaDia>? horarios)
    {
        if (horarios is null || horarios.Count == 0)
            return CrearHorarioPorDefecto();

        if (horarios.Count != 7)
            throw new ExcepcionDominio("El horario de reserva debe incluir exactamente los 7 días de la semana.");

        var duplicados = horarios.GroupBy(h => h.DiaSemana).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicados.Count > 0)
            throw new ExcepcionDominio("No puede haber días repetidos en el horario de reserva.");

        var normalizados = horarios
            .OrderBy(h => h.DiaSemana)
            .Select(h =>
            {
                if (h.DiaSemana < 0 || h.DiaSemana > 6)
                    throw new ExcepcionDominio("El día de la semana debe estar entre 0 y 6.");

                if (!h.Activo)
                {
                    return new HorarioReservaDia
                    {
                        DiaSemana = h.DiaSemana,
                        Activo = false,
                        HoraInicio = "00:00",
                        HoraFin = "23:59"
                    };
                }

                var inicio = ParseTime(h.HoraInicio);
                var fin = ParseTime(h.HoraFin);

                if (inicio >= fin)
                    throw new ExcepcionDominio("La hora de inicio debe ser anterior a la de fin en cada día activo.");

                return new HorarioReservaDia
                {
                    DiaSemana = h.DiaSemana,
                    Activo = true,
                    HoraInicio = inicio.ToString("HH:mm"),
                    HoraFin = fin.ToString("HH:mm")
                };
            })
            .ToList();

        var diasEsperados = Enumerable.Range(0, 7).ToArray();
        if (!normalizados.Select(h => h.DiaSemana).SequenceEqual(diasEsperados))
            throw new ExcepcionDominio("El horario de reserva debe cubrir los 7 días de la semana.");

        return normalizados;
    }

    private static TimeOnly ParseTime(string value)
    {
        if (!TimeOnly.TryParse(value, out var parsed))
            throw new ExcepcionDominio($"La hora '{value}' no tiene un formato válido. Usa HH:mm.");

        return parsed;
    }
}
