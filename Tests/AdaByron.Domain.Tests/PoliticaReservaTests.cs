using AdaByron.Domain.Entities;
using AdaByron.Domain.Enums;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Policies;
using AdaByron.Domain.ValueObjects;
using Xunit;

namespace AdaByron.Domain.Tests;

public class PoliticaReservaTests
{
    private static Espacio CrearEspacio(TipoEspacio categoria, int aforo, string? departamento = null)
    {
        var espacio = new Espacio(
            codigoEspacio: "TEST-01",
            nombre: "Sala Test",
            planta: Planta.De(1),
            aforo: Aforo.De(aforo),
            tipoFisico: categoria,
            departamento: departamento
        );
        return espacio;
    }

    private static Persona CrearPersona(Rol rol, string? departamento = null)
    {
        return new Persona(
            email: "test@unizar.es",
            nombre: "Test",
            apellidos: "User",
            rol: rol,
            departamento: departamento
        );
    }

    // ── Pruebas de Permisos (F1-F4) ──────────────────────────────────────────

    [Fact]
    public void VerificarPermiso_EstudianteReservaSalaComun_Exito()
    {
        var estudiante = CrearPersona(Rol.Estudiante);
        var salaComun = CrearEspacio(TipoEspacio.SalaComun, 10);
        
        // No lanza excepción
        PoliticaReserva.VerificarPermiso(estudiante, salaComun);
    }

    [Fact]
    public void VerificarPermiso_EstudianteReservaAula_LanzaExcepcion()
    {
        var estudiante = CrearPersona(Rol.Estudiante);
        var aula = CrearEspacio(TipoEspacio.Aula, 50);

        Assert.Throws<ExcepcionPermisos>(() => 
            PoliticaReserva.VerificarPermiso(estudiante, aula));
    }

    [Fact]
    public void VerificarPermiso_TecnicoLabReservaLaboratorioMismoDepartamento_Exito()
    {
        var tecnico = CrearPersona(Rol.TecnicoLab, "Informatica");
        var lab = CrearEspacio(TipoEspacio.Laboratorio, 20, "Informatica");

        PoliticaReserva.VerificarPermiso(tecnico, lab);
    }

    [Fact]
    public void VerificarPermiso_TecnicoLabReservaLaboratorioDistintoDepartamento_Lanza()
    {
        var tecnico = CrearPersona(Rol.TecnicoLab, "Informatica");
        var lab = CrearEspacio(TipoEspacio.Laboratorio, 20, "Matematicas");

        Assert.Throws<ExcepcionPermisos>(() => 
            PoliticaReserva.VerificarPermiso(tecnico, lab));
    }

    [Fact]
    public void VerificarPermiso_NadieReservaDespacho_LanzaExcepcion()
    {
        var docente = CrearPersona(Rol.Docente, "Informatica");
        var gerente = CrearPersona(Rol.Gerente);
        var conserje = CrearPersona(Rol.Conserje);
        var despacho = CrearEspacio(TipoEspacio.Despacho, 2);

        Assert.Throws<ExcepcionPermisos>(() => PoliticaReserva.VerificarPermiso(docente, despacho));
        Assert.Throws<ExcepcionPermisos>(() => PoliticaReserva.VerificarPermiso(gerente, despacho));
        Assert.Throws<ExcepcionPermisos>(() => PoliticaReserva.VerificarPermiso(conserje, despacho));
    }

    // ── Pruebas de Aforo (F5) ────────────────────────────────────────────────

    [Fact]
    public void VerificarAforo_PorDebajoDelLimite_Exito()
    {
        var espacio = CrearEspacio(TipoEspacio.Aula, 100);
        // 80% de 100 = 80
        PoliticaReserva.VerificarAforo(espacio, numeroAsistentes: 80, porcentajeOcupacion: 0.80);
    }

    [Fact]
    public void VerificarAforo_PorEncimaDelLimite_LanzaExcepcionAforo()
    {
        var espacio = CrearEspacio(TipoEspacio.Aula, 100);
        // 50% de 100 = 50 permitidos.
        Assert.Throws<ExcepcionAforoSuperado>(() => 
            PoliticaReserva.VerificarAforo(espacio, numeroAsistentes: 51, porcentajeOcupacion: 0.50));
    }

    // ── Pruebas de Solapamiento (F6) ─────────────────────────────────────────

    [Fact]
    public void VerificarDisponibilidad_ConSolapamiento_LanzaExcepcionConflicto()
    {
        var espacio = CrearEspacio(TipoEspacio.SalaComun, 10);
        var franjaExistente = new FranjaHoraria(
            new DateTime(2026, 1, 1, 10, 0, 0),
            new DateTime(2026, 1, 1, 12, 0, 0)
        );
        var reservaAnterior = Reserva.Reconstituir(Guid.NewGuid(), "p1", espacio.CodigoEspacio, franjaExistente, 5, EstadoReserva.Aceptada);

        var franjaNueva = new FranjaHoraria(
            new DateTime(2026, 1, 1, 11, 0, 0),
            new DateTime(2026, 1, 1, 13, 0, 0)
        );

        Assert.Throws<ExcepcionConflictoReserva>(() => 
            PoliticaReserva.VerificarDisponibilidad(espacio, franjaNueva, new[] { reservaAnterior }));
    }
}
