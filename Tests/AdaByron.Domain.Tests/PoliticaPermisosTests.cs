using AdaByron.Domain.Aggregates.PersonAggregate;
using AdaByron.Domain.Aggregates.SpaceAggregate;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Services;
using Xunit;

namespace AdaByron.Domain.Tests;

/// <summary>
/// Pruebas unitarias para PoliticaReserva (Tabla 1 del informe — Reglas F1-F4).
/// Verifica la política de permisos desacoplada del Aggregate Root Espacio.
/// </summary>
public class PoliticaPermisosTests
{
    private static readonly PoliticaReserva Politica = new();

    private static Espacio CrearEspacio(TipoEspacio categoria, string? departamento = null)
        => new(
            codigoEspacio: "TEST-01",
            nombre:        "Sala Test",
            planta:        Planta.De(1),
            aforo:         Aforo.De(10),
            tipoFisico:    categoria,
            departamento:  departamento != null ? new Departamento(departamento) : null);

    private static Persona CrearPersona(Rol rol, string? departamento = null, bool esGerente = false)
        => new(
            email:        "test@unizar.es",
            nombre:       "Test",
            apellidos:    "User",
            rol:          rol,
            departamento: departamento != null ? new Departamento(departamento) : null,
            esGerente:    esGerente);

    // ── Gerente ───────────────────────────────────────────────────────────────
    // Rol.Gerente puro (sin dual role) para no requerir departamento en el test

    [Theory]
    [InlineData(TipoEspacio.SalaComun)]
    [InlineData(TipoEspacio.Aula)]
    [InlineData(TipoEspacio.Seminario)]
    [InlineData(TipoEspacio.Laboratorio)]
    public void Gerente_PuedeReservar_TodosExceptoDespacho(TipoEspacio categoria)
    {
        var gerente = CrearPersona(Rol.Gerente);
        var espacio = CrearEspacio(categoria);
        Politica.VerificarPermisos(gerente, espacio); // No debe lanzar
    }

    [Fact]
    public void Gerente_NoPuede_ReservarDespacho()
    {
        var gerente = CrearPersona(Rol.Gerente);
        var despacho = CrearEspacio(TipoEspacio.Despacho);
        Assert.Throws<ExcepcionPermisos>(() => Politica.VerificarPermisos(gerente, despacho));
    }

    // ── Estudiante ────────────────────────────────────────────────────────────

    [Fact]
    public void Estudiante_PuedeReservar_SalaComun()
    {
        var estudiante = CrearPersona(Rol.Estudiante);
        var sala = CrearEspacio(TipoEspacio.SalaComun);
        Politica.VerificarPermisos(estudiante, sala); // No debe lanzar
    }

    [Theory]
    [InlineData(TipoEspacio.Aula)]
    [InlineData(TipoEspacio.Seminario)]
    [InlineData(TipoEspacio.Laboratorio)]
    [InlineData(TipoEspacio.Despacho)]
    public void Estudiante_NoPuede_ReservarOtrosTipos(TipoEspacio categoria)
    {
        var estudiante = CrearPersona(Rol.Estudiante);
        var espacio = CrearEspacio(categoria);
        Assert.Throws<ExcepcionPermisos>(() => Politica.VerificarPermisos(estudiante, espacio));
    }

    // ── TécnicoLaboratorio ────────────────────────────────────────────────────

    [Theory]
    [InlineData(TipoEspacio.SalaComun)]
    [InlineData(TipoEspacio.Seminario)]
    public void TecnicoLab_PuedeReservar_SalaComunYSeminario(TipoEspacio categoria)
    {
        var tecnico = CrearPersona(Rol.TecnicoLaboratorio, "Informática");
        var espacio = CrearEspacio(categoria);
        Politica.VerificarPermisos(tecnico, espacio);
    }

    [Fact]
    public void TecnicoLab_PuedeReservar_LaboratorioMismoDepartamento()
    {
        var tecnico = CrearPersona(Rol.TecnicoLaboratorio, "Informática");
        var lab = CrearEspacio(TipoEspacio.Laboratorio, "Informática");
        Politica.VerificarPermisos(tecnico, lab);
    }

    [Fact]
    public void TecnicoLab_PuedeReservar_LaboratorioGeneralEina()
    {
        var tecnico = CrearPersona(Rol.TecnicoLaboratorio, "Informática");
        var lab = CrearEspacio(TipoEspacio.Laboratorio);
        Politica.VerificarPermisos(tecnico, lab);
    }

    [Fact]
    public void TecnicoLab_NoPuede_LaboratorioDistintoDepartamento()
    {
        var tecnico = CrearPersona(Rol.TecnicoLaboratorio, "Informática");
        var lab = CrearEspacio(TipoEspacio.Laboratorio, "Ing. de Sistemas e Ing. Electrónica y Comunicaciones");
        Assert.Throws<ExcepcionPermisos>(() => Politica.VerificarPermisos(tecnico, lab));
    }

    [Theory]
    [InlineData(TipoEspacio.Aula)]
    [InlineData(TipoEspacio.Despacho)]
    public void TecnicoLab_NoPuede_AulaDespacho(TipoEspacio categoria)
    {
        var tecnico = CrearPersona(Rol.TecnicoLaboratorio, "Informática");
        var espacio = CrearEspacio(categoria);
        Assert.Throws<ExcepcionPermisos>(() => Politica.VerificarPermisos(tecnico, espacio));
    }

    // ── DocenteInvestigador / InvestigadorContratado ──────────────────────────

    [Theory]
    [InlineData(TipoEspacio.SalaComun)]
    [InlineData(TipoEspacio.Aula)]
    [InlineData(TipoEspacio.Seminario)]
    public void DocenteInvestigador_PuedeReservar_SalaComunAulaSeminario(TipoEspacio categoria)
    {
        var docente = CrearPersona(Rol.DocenteInvestigador, "Informática");
        var espacio = CrearEspacio(categoria);
        Politica.VerificarPermisos(docente, espacio);
    }

    [Fact]
    public void DocenteInvestigador_PuedeReservar_LaboratorioMismoDepartamento()
    {
        var docente = CrearPersona(Rol.DocenteInvestigador, "Informática");
        var lab = CrearEspacio(TipoEspacio.Laboratorio, "Informática");
        Politica.VerificarPermisos(docente, lab);
    }

    [Fact]
    public void DocenteInvestigador_PuedeReservar_LaboratorioGeneralEina()
    {
        var docente = CrearPersona(Rol.DocenteInvestigador, "Informática");
        var lab = CrearEspacio(TipoEspacio.Laboratorio);
        Politica.VerificarPermisos(docente, lab);
    }

    [Fact]
    public void DocenteInvestigador_NoPuede_Despacho()
    {
        var docente = CrearPersona(Rol.DocenteInvestigador, "Informática");
        var despacho = CrearEspacio(TipoEspacio.Despacho);
        Assert.Throws<ExcepcionPermisos>(() => Politica.VerificarPermisos(docente, despacho));
    }

    // ── Conserje ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(TipoEspacio.SalaComun)]
    [InlineData(TipoEspacio.Aula)]
    [InlineData(TipoEspacio.Seminario)]
    [InlineData(TipoEspacio.Laboratorio)]
    public void Conserje_PuedeReservar_TodosExceptoDespacho(TipoEspacio categoria)
    {
        var conserje = CrearPersona(Rol.Conserje);
        var espacio = CrearEspacio(categoria);
        Politica.VerificarPermisos(conserje, espacio);
    }

    [Fact]
    public void Conserje_NoPuede_Despacho()
    {
        var conserje = CrearPersona(Rol.Conserje);
        var despacho = CrearEspacio(TipoEspacio.Despacho);
        Assert.Throws<ExcepcionPermisos>(() => Politica.VerificarPermisos(conserje, despacho));
    }

    // ── Dual-role: Gerente + DocenteInvestigador ──────────────────────────────
    // Se aplica el permiso más permisivo (Gerente): puede reservar todo excepto Despacho.

    /// <summary>
    /// Un Gerente+DocenteInvestigador hereda los permisos de Gerente (más permisivos):
    /// puede reservar SalaComun, Aula, Seminario y Laboratorio —incluso de otro departamento—.
    /// </summary>
    [Theory]
    [InlineData(TipoEspacio.SalaComun)]
    [InlineData(TipoEspacio.Aula)]
    [InlineData(TipoEspacio.Seminario)]
    [InlineData(TipoEspacio.Laboratorio)]
    public void GerenteYDocente_PuedeReservar_TodosExceptoDespacho(TipoEspacio categoria)
    {
        // Sin departamento asignado al espacio → un DocenteInvestigador puro no podría el Laboratorio,
        // pero el rol Gerente lo permite.
        var dual    = CrearPersona(Rol.DocenteInvestigador, "Informática", esGerente: true);
        var espacio = CrearEspacio(categoria, "Ing. de Sistemas e Ing. Electrónica y Comunicaciones");

        Politica.VerificarPermisos(dual, espacio); // No debe lanzar
    }

    /// <summary>
    /// Aunque tenga rol DocenteInvestigador, el Gerente+DocenteInvestigador
    /// tampoco puede reservar un Despacho (límite del Gerente).
    /// </summary>
    [Fact]
    public void GerenteYDocente_NoPuede_Despacho()
    {
        var dual     = CrearPersona(Rol.DocenteInvestigador, "Informática", esGerente: true);
        var despacho = CrearEspacio(TipoEspacio.Despacho);

        Assert.Throws<ExcepcionPermisos>(() => Politica.VerificarPermisos(dual, despacho));
    }

    /// <summary>
    /// Caso clave: un DocenteInvestigador puro NO puede reservar un laboratorio
    /// de otro departamento, pero un Gerente+DocenteInvestigador SÍ puede,
    /// porque se aplica el permiso más permisivo (Gerente).
    /// </summary>
    [Fact]
    public void GerenteYDocente_PuedeReservar_LaboratorioDeOtroDepartamento()
    {
        var dualRole  = CrearPersona(Rol.DocenteInvestigador, "Informática", esGerente: true);
        var labAjeno  = CrearEspacio(TipoEspacio.Laboratorio, "Ing. de Sistemas e Ing. Electrónica y Comunicaciones");

        Politica.VerificarPermisos(dualRole, labAjeno); // Gerente puede: no debe lanzar
    }

    [Fact]
    public void DocenteInvestigadorPuro_NoPuede_LaboratorioDeOtroDepartamento()
    {
        var docentePuro = CrearPersona(Rol.DocenteInvestigador, "Informática");
        var labAjeno    = CrearEspacio(TipoEspacio.Laboratorio, "Ing. de Sistemas e Ing. Electrónica y Comunicaciones");

        Assert.Throws<ExcepcionPermisos>(() => Politica.VerificarPermisos(docentePuro, labAjeno));
    }
}
