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
        PoliticaReserva.VerificarPermiso(estudiante.Rol, salaComun.CategoriaReserva, estudiante.Departamento, salaComun.Departamento);
    }

    [Fact]
    public void VerificarPermiso_EstudianteReservaAula_LanzaExcepcion()
    {
        var estudiante = CrearPersona(Rol.Estudiante);
        var aula = CrearEspacio(TipoEspacio.Aula, 50);

        Assert.Throws<ExcepcionPermisos>(() => 
            PoliticaReserva.VerificarPermiso(estudiante.Rol, aula.CategoriaReserva, estudiante.Departamento, aula.Departamento));
    }

    [Fact]
    public void VerificarPermiso_TecnicoLabReservaLaboratorioMismoDepartamento_Exito()
    {
        var tecnico = CrearPersona(Rol.TecnicoLab, "Informatica");
        var lab = CrearEspacio(TipoEspacio.Laboratorio, 20, "Informatica");

        PoliticaReserva.VerificarPermiso(tecnico.Rol, lab.CategoriaReserva, tecnico.Departamento, lab.Departamento);
    }

    [Fact]
    public void VerificarPermiso_TecnicoLabReservaLaboratorioDistintoDepartamento_Lanza()
    {
        var tecnico = CrearPersona(Rol.TecnicoLab, "Informatica");
        var lab = CrearEspacio(TipoEspacio.Laboratorio, 20, "Matematicas");

        Assert.Throws<ExcepcionPermisos>(() => 
            PoliticaReserva.VerificarPermiso(tecnico.Rol, lab.CategoriaReserva, tecnico.Departamento, lab.Departamento));
    }

    [Fact]
    public void VerificarPermiso_NadieReservaDespacho_LanzaExcepcion()
    {
        var docente = CrearPersona(Rol.Docente, "Informatica");
        var gerente = CrearPersona(Rol.Gerente);
        var conserje = CrearPersona(Rol.Conserje);
        var despacho = CrearEspacio(TipoEspacio.Despacho, 2);

        Assert.Throws<ExcepcionPermisos>(() => PoliticaReserva.VerificarPermiso(docente.Rol, despacho.CategoriaReserva, docente.Departamento, despacho.Departamento));
        Assert.Throws<ExcepcionPermisos>(() => PoliticaReserva.VerificarPermiso(gerente.Rol, despacho.CategoriaReserva, gerente.Departamento, despacho.Departamento));
        Assert.Throws<ExcepcionPermisos>(() => PoliticaReserva.VerificarPermiso(conserje.Rol, despacho.CategoriaReserva, conserje.Departamento, despacho.Departamento));
    }

    // ── Pruebas de Aforo (F5) ────────────────────────────────────────────────

    [Fact]
    public void VerificarAforo_PorDebajoDelLimite_Exito()
    {
        var espacio = CrearEspacio(TipoEspacio.Aula, 100);
        // 80% de 100 = 80
        PoliticaReserva.VerificarAforo(espacio.Aforo, espacio.CodigoEspacio, numeroAsistentes: 80, porcentajeEdificio: 80.0);
    }

    [Fact]
    public void VerificarAforo_PorEncimaDelLimite_LanzaExcepcionAforo()
    {
        var espacio = CrearEspacio(TipoEspacio.Aula, 100);
        // 50% de 100 = 50 permitidos.
        Assert.Throws<ExcepcionAforoSuperado>(() => 
            PoliticaReserva.VerificarAforo(espacio.Aforo, espacio.CodigoEspacio, numeroAsistentes: 51, porcentajeEdificio: 50.0));
    }

    // ── Casos límite PBI-5: Aforo Dinámico ───────────────────────────────────

    /// <summary>
    /// Al 100 % cualquier número de asistentes ≤ capacidad máxima debe pasar.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void VerificarAforo_Edificio100PorCiento_PermiteHastaCapacidadMaxima(int asistentes)
    {
        // Arrange
        var espacio = CrearEspacio(TipoEspacio.Aula, aforo: 100);

        // Act & Assert – No debe lanzar excepción
        PoliticaReserva.VerificarAforo(espacio.Aforo, espacio.CodigoEspacio,
            numeroAsistentes: asistentes, porcentajeEdificio: 100.0);
    }

    /// <summary>
    /// Al 100 % un asistente por encima de la capacidad máxima debe fallar.
    /// </summary>
    [Fact]
    public void VerificarAforo_Edificio100PorCiento_SuperaCapacidad_LanzaExcepcion()
    {
        var espacio = CrearEspacio(TipoEspacio.Aula, aforo: 100);

        Assert.Throws<ExcepcionAforoSuperado>(() =>
            PoliticaReserva.VerificarAforo(espacio.Aforo, espacio.CodigoEspacio,
                numeroAsistentes: 101, porcentajeEdificio: 100.0));
    }

    /// <summary>
    /// Al 0 % (edificio cerrado) incluso 1 asistente debe ser rechazado.
    /// Floor(100 * 0 / 100) = 0 → cualquier asistente > 0 falla.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void VerificarAforo_Edificio0PorCiento_RecualquierAsistenteLanzaExcepcion(int asistentes)
    {
        // Arrange
        var espacio = CrearEspacio(TipoEspacio.Aula, aforo: 100);

        // Act & Assert
        Assert.Throws<ExcepcionAforoSuperado>(() =>
            PoliticaReserva.VerificarAforo(espacio.Aforo, espacio.CodigoEspacio,
                numeroAsistentes: asistentes, porcentajeEdificio: 0.0));
    }

    /// <summary>
    /// Frontera exacta: asistentes == capacidadPermitida no debe lanzar.
    /// </summary>
    [Theory]
    [InlineData(100, 50.0, 50)]   // 50% de 100 → límite = 50
    [InlineData(200, 75.0, 150)]  // 75% de 200 → límite = 150
    [InlineData(60,  10.0, 6)]    // 10% de 60  → límite = 6
    public void VerificarAforo_FronteraExacta_NoLanzaExcepcion(int capacidad, double porcentaje, int asistentes)
    {
        var espacio = CrearEspacio(TipoEspacio.Aula, aforo: capacidad);

        // Act & Assert – exactamente en el límite: debe pasar
        PoliticaReserva.VerificarAforo(espacio.Aforo, espacio.CodigoEspacio,
            numeroAsistentes: asistentes, porcentajeEdificio: porcentaje);
    }

    /// <summary>
    /// Un asistente por encima de la frontera exacta debe lanzar excepción.
    /// </summary>
    [Theory]
    [InlineData(100, 50.0, 51)]   // 50% de 100 → límite = 50; 51 falla
    [InlineData(200, 75.0, 151)]  // 75% de 200 → límite = 150; 151 falla
    [InlineData(60,  10.0, 7)]    // 10% de 60  → límite = 6; 7 falla
    public void VerificarAforo_UnAsistenteSobreFrontera_LanzaExcepcion(int capacidad, double porcentaje, int asistentes)
    {
        var espacio = CrearEspacio(TipoEspacio.Aula, aforo: capacidad);

        Assert.Throws<ExcepcionAforoSuperado>(() =>
            PoliticaReserva.VerificarAforo(espacio.Aforo, espacio.CodigoEspacio,
                numeroAsistentes: asistentes, porcentajeEdificio: porcentaje));
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
            PoliticaReserva.VerificarDisponibilidad(espacio.CodigoEspacio, franjaNueva, existeSolapamiento: true));
    }
}
