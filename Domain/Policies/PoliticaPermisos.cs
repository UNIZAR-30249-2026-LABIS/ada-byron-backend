using AdaByron.Domain.Entities;
using AdaByron.Domain.Enums;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.ValueObjects;

namespace AdaByron.Domain.Policies;

/// <summary>
/// Domain Service que centraliza las reglas de negocio para crear una reserva.
/// Implementa las Reglas F1-F6 del informe de proyecto.
/// No tiene identidad propia ni estado persistido → es un servicio de dominio puro.
/// </summary>
public static class PoliticaReserva
{
    // Porcentaje máximo de ocupación permitido en el edificio (Regla F5).
    // Valor configurable; por defecto el 80 % de la capacidad máxima del espacio.
    private const double PorcentajeOcupacionEdificio = 0.80;

    // ── Regla F1-F4: Matriz de permisos por rol ───────────────────────────────
    /// <summary>
    /// Verifica que la persona puede reservar el espacio según su rol y departamento.
    /// Lanza <see cref="ExcepcionPermisos"/> si la regla no se cumple.
    /// </summary>
    public static void VerificarPermiso(Persona persona, Espacio espacio)
    {
        bool permitido = persona.Rol switch
        {
            Rol.Estudiante => VerificarEstudiante(espacio),
            Rol.TecnicoLab => VerificarTecnicoLab(persona, espacio),
            Rol.Docente    => VerificarDocente(persona, espacio),
            Rol.Conserje   => VerificarConserje(espacio),
            Rol.Gerente    => VerificarGerente(espacio),
            _              => false,
        };

        if (!permitido)
            LanzarErrorPermiso(persona.Rol, espacio.CategoriaReserva);
    }

    // ── Regla F5: Aforo ───────────────────────────────────────────────────────
    /// <summary>
    /// Verifica que el número de asistentes no supera la capacidad permitida.
    /// <paramref name="porcentajeOcupacion"/>: fracción 0.10–1.00 (ej: 0.80 = 80%).
    /// </summary>
    public static void VerificarAforo(Espacio espacio, int numeroAsistentes,
                                      double porcentajeOcupacion = 1.0)
    {
        var capacidadPermitida = (int)(espacio.Aforo.Valor * porcentajeOcupacion);

        if (numeroAsistentes > capacidadPermitida)
            throw new ExcepcionAforoSuperado(
                $"El espacio '{espacio.Nombre}' admite un máximo de {capacidadPermitida} personas " +
                $"({(int)(porcentajeOcupacion * 100)}% del aforo de {espacio.Aforo.Valor}). " +
                $"Se solicitaron {numeroAsistentes}.");
    }

    // ── Regla F6: Solapamiento ────────────────────────────────────────────────
    /// <summary>
    /// Verifica que el espacio está disponible en la franja horaria solicitada.
    /// Lanza <see cref="ExcepcionConflictoReserva"/> si existe solapamiento con una reserva Aceptada.
    /// </summary>
    public static void VerificarDisponibilidad(Espacio espacio, FranjaHoraria franja,
                                               IEnumerable<Reserva> reservasExistentes)
    {
        if (!espacio.IsDisponible(franja, reservasExistentes))
            throw new ExcepcionConflictoReserva(
                $"El espacio '{espacio.Nombre}' ya tiene una reserva activa que solapa con la franja " +
                $"{franja.Inicio:HH:mm} – {franja.Fin:HH:mm}.");
    }

    // ── Punto de entrada único ────────────────────────────────────────────────
    /// <summary>
    /// Valida en orden todas las reglas F1-F6.
    /// <paramref name="porcentajeOcupacion"/>: fracción dinámica del edificio (default 1.0 = 100%).
    /// </summary>
    public static void ValidarCreacion(Persona persona, Espacio espacio, FranjaHoraria franja,
                                       int numeroAsistentes, IEnumerable<Reserva> reservasExistentes,
                                       double porcentajeOcupacion = 1.0)
    {
        VerificarPermiso(persona, espacio);                              // F1-F4
        VerificarAforo(espacio, numeroAsistentes, porcentajeOcupacion); // F5
        VerificarDisponibilidad(espacio, franja, reservasExistentes);   // F6
    }

    // ── Helpers privados (Reglas F1-F4) ──────────────────────────────────────

    private static bool VerificarEstudiante(Espacio espacio)
        => espacio.CategoriaReserva == TipoEspacio.SalaComun;

    private static bool VerificarTecnicoLab(Persona persona, Espacio espacio)
        => espacio.CategoriaReserva switch
        {
            TipoEspacio.SalaComun   => true,
            TipoEspacio.Seminario   => true,
            // Aula NO permitida para TecnicoLab (Tabla 1, Regla F2)
            TipoEspacio.Laboratorio => MismoDepartamento(persona.Departamento, espacio.Departamento),
            _                       => false,
        };

    private static bool VerificarDocente(Persona persona, Espacio espacio)
        => espacio.CategoriaReserva switch
        {
            TipoEspacio.SalaComun   => true,
            TipoEspacio.Aula        => true,
            TipoEspacio.Seminario   => true,
            TipoEspacio.Laboratorio => MismoDepartamento(persona.Departamento, espacio.Departamento),
            _                       => false,
        };

    private static bool VerificarConserje(Espacio espacio)
        => espacio.CategoriaReserva != TipoEspacio.Despacho;

    private static bool VerificarGerente(Espacio espacio)
        => espacio.CategoriaReserva != TipoEspacio.Despacho;

    private static bool MismoDepartamento(string? deptPersona, string? deptEspacio)
        => !string.IsNullOrWhiteSpace(deptPersona)
        && !string.IsNullOrWhiteSpace(deptEspacio)
        && deptPersona.Trim().Equals(deptEspacio.Trim(), StringComparison.OrdinalIgnoreCase);

    private static void LanzarErrorPermiso(Rol rol, TipoEspacio tipo)
    {
        var mensaje = rol switch
        {
            Rol.Estudiante => "Un estudiante solo puede reservar Salas Comunes.",
            Rol.TecnicoLab => tipo == TipoEspacio.Laboratorio
                ? "Un Técnico de Lab solo puede reservar laboratorios de su propio departamento."
                : $"Un Técnico de Lab no puede reservar espacios de tipo '{tipo}'.",
            Rol.Docente    => tipo == TipoEspacio.Laboratorio
                ? "Un Docente solo puede reservar laboratorios de su propio departamento."
                : $"Un Docente no puede reservar espacios de tipo '{tipo}'.",
            Rol.Conserje   => "El Conserje no puede reservar despachos.",
            Rol.Gerente    => "El Gerente no puede reservar despachos.",
            _              => $"El rol '{rol}' no tiene permisos para reservar espacios de tipo '{tipo}'.",
        };

        throw new ExcepcionPermisos(mensaje);
    }
}