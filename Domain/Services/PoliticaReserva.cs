using AdaByron.Domain.Aggregates.PersonAggregate;
using AdaByron.Domain.Aggregates.SpaceAggregate;
using AdaByron.Domain.Exceptions;

namespace AdaByron.Domain.Services;

/// <summary>
/// Política de dominio que encapsula las Reglas F de permisos (Tabla 1 del informe),
/// determinando si una Persona puede reservar un Espacio según su rol y departamento.
/// Desacopla esta lógica volátil del Aggregate Root Espacio.
/// </summary>
public sealed class PoliticaReserva
{
    /// <summary>
    /// Verifica que la persona tenga permiso para reservar el espacio indicado.
    /// Lanza <see cref="ExcepcionPermisos"/> si la combinación rol/categoría no está permitida.
    /// </summary>
    public void VerificarPermisos(Persona persona, Espacio espacio)
    {
        bool permitido = persona.Rol switch
        {
            _ when persona.EsGerente => espacio.CategoriaReserva != TipoEspacio.Despacho,
            Rol.Estudiante           => espacio.CategoriaReserva == TipoEspacio.SalaComun,
            Rol.TecnicoLaboratorio   => espacio.CategoriaReserva switch
            {
                TipoEspacio.SalaComun   => true,
                TipoEspacio.Seminario   => true,
                TipoEspacio.Laboratorio => EsLaboratorioGeneral(espacio) || persona.Departamento.IsSameAs(espacio.Departamento),
                _                       => false
            },
            Rol.InvestigadorContratado or Rol.DocenteInvestigador => espacio.CategoriaReserva switch
            {
                TipoEspacio.SalaComun   => true,
                TipoEspacio.Aula        => true,
                TipoEspacio.Seminario   => true,
                TipoEspacio.Laboratorio => EsLaboratorioGeneral(espacio) || persona.Departamento.IsSameAs(espacio.Departamento),
                _                       => false
            },
            Rol.Conserje => espacio.CategoriaReserva != TipoEspacio.Despacho,
            _            => false
        };

        if (!permitido)
            throw new ExcepcionPermisos(
                $"El rol '{persona.Rol}' no tiene permiso para reservar espacios del tipo '{espacio.CategoriaReserva}'.");
    }

    private static bool EsLaboratorioGeneral(Espacio espacio) =>
        espacio.TipoAsignacion == TipoAsignacionEspacio.Eina || espacio.Departamento.IsNull;
}
