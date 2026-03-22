using AdaByron.Domain.Entities;
using AdaByron.Domain.Enums;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.ValueObjects;

namespace AdaByron.Domain.Policies;

/// <summary>
/// Domain Service que centraliza las reglas de negocio para validar la creación de una reserva.
/// Coordina las validaciones de Roles y Permisos (HU-13), Disponibilidad (HU-15) y Aforo Dinámico (HU-14).
/// </summary>
public static class PoliticaReserva
{
    // ── 1. Validación de Roles y Permisos (HU-13 / Tabla 1) ───────────────────
    public static void VerificarPermiso(Rol rol, TipoEspacio categoriaReserva, string? deptPersona, string? deptEspacio)
    {
        bool permitido = rol switch
        {
            Rol.Estudiante => categoriaReserva == TipoEspacio.SalaComun,
            Rol.TecnicoLab => categoriaReserva switch
            {
                TipoEspacio.SalaComun   => true,
                TipoEspacio.Seminario   => true, // Interpretación permisiva (pág 12)
                TipoEspacio.Laboratorio => MismoDepartamento(deptPersona, deptEspacio),
                _                       => false
            },
            Rol.Docente => categoriaReserva switch
            {
                TipoEspacio.SalaComun   => true,
                TipoEspacio.Aula        => true,
                TipoEspacio.Seminario   => true,
                TipoEspacio.Laboratorio => MismoDepartamento(deptPersona, deptEspacio),
                _                       => false
            },
            Rol.Conserje => categoriaReserva != TipoEspacio.Despacho,
            Rol.Gerente  => categoriaReserva != TipoEspacio.Despacho,
            _            => false
        };

        if (!permitido)
            LanzarErrorPermiso(rol, categoriaReserva);
    }

    // ── 2. Validación de Disponibilidad (HU-15 / HU-T2) ───────────────────────
    public static void VerificarDisponibilidad(string codigoEspacio, FranjaHoraria franja, bool existeSolapamiento)
    {
        if (existeSolapamiento)
        {
            throw new ExcepcionConflictoReserva(
                $"El espacio '{codigoEspacio}' ya tiene una reserva activa que solapa con la franja " +
                $"({franja.Inicio:HH:mm} – {franja.Fin:HH:mm}).");
        }
    }

    // ── 3. Validación de Aforo Dinámico (HU-14 / HU-10) ───────────────────────
    public static void VerificarAforo(Aforo aforoEspacio, string codigoEspacio, int numeroAsistentes, double porcentajeEdificio)
    {
        var factor = porcentajeEdificio / 100.0;
        var capacidadPermitida = (int)Math.Floor(aforoEspacio.Valor * factor);

        if (numeroAsistentes > capacidadPermitida)
        {
            throw new ExcepcionAforoSuperado(
                $"Aforo excedido. El espacio '{codigoEspacio}' admite un máximo de {capacidadPermitida} personas " +
                $"({porcentajeEdificio}% del aforo de {aforoEspacio.Valor}). " +
                $"Asistentes intentados: {numeroAsistentes}.");
        }
    }

    // ── Punto de Entrada Integrado para el Use Case ──────────────────────────
    public static void ValidarCreacion(
        Persona persona, 
        Espacio espacio,
        string? departamentoEspacio,
        FranjaHoraria franja, 
        int numeroAsistentes, 
        bool existeSolapamiento, 
        double porcentajeEdificio)
    {
        VerificarPermiso(persona.Rol, espacio.CategoriaReserva, persona.Departamento, departamentoEspacio);
        VerificarDisponibilidad(espacio.CodigoEspacio, franja, existeSolapamiento);
        VerificarAforo(espacio.Aforo, espacio.CodigoEspacio, numeroAsistentes, porcentajeEdificio);
    }

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
                : $"Un Técnico de Lab no tiene permiso para reservar espacios de tipo '{tipo}'.",
            Rol.Docente    => tipo == TipoEspacio.Laboratorio
                ? "Un Docente solo puede reservar laboratorios de su propio departamento."
                : $"Un Docente no tiene permiso para reservar espacios de tipo '{tipo}'.",
            _              => $"El rol '{rol}' no tiene permisos para reservar {tipo}s o despachos."
        };
        throw new ExcepcionPermisos(mensaje);
    }
}