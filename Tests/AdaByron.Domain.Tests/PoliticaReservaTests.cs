using AdaByron.Domain.Aggregates.PersonAggregate;
using AdaByron.Domain.Aggregates.SpaceAggregate;
using AdaByron.Domain.Aggregates.ReservationAggregate;
using AdaByron.Domain.Exceptions;
using Xunit;

namespace AdaByron.Domain.Tests;

/// <summary>
/// Pruebas unitarias para el Aggregate Root Espacio (HU-13, HU-14, HU-15).
/// Verifica que el AR gestione correctamente sus reglas de negocio.
/// </summary>
public class EspacioTests
{
    private static Espacio CrearEspacio(TipoEspacio categoria, int aforo, string? departamento = null)
    {
        return new Espacio(
            codigoEspacio: "TEST-01",
            nombre:        "Sala Test",
            planta:        Planta.De(1),
            aforo:         Aforo.De(aforo),
            tipoFisico:    categoria,
            departamento:  departamento != null ? new Departamento(departamento) : null
        );
    }

    private static Persona CrearPersona(Rol rol, string? departamento = null)
    {
        return new Persona(
            email:        "test@unizar.es",
            nombre:       "Test",
            apellidos:    "User",
            rol:          rol,
            departamento: departamento != null ? new Departamento(departamento) : null
        );
    }

    private static Reserva CrearReservaDeIntento(Espacio espacio, int asistentes)
    {
        var franja = new FranjaHoraria(DateTime.Now.AddHours(1), DateTime.Now.AddHours(2));
        return new Reserva("test@unizar.es", espacio.CodigoEspacio, franja, asistentes);
    }

    // ── Pruebas de Permisos (F1-F4 / HU-13) ──────────────────────────────────

    [Fact]
    public void AddReserva_EstudianteReservaSalaComun_Exito()
    {
        var estudiante = CrearPersona(Rol.Estudiante);
        var salaComun  = CrearEspacio(TipoEspacio.SalaComun, 10);
        var reserva    = CrearReservaDeIntento(salaComun, 5);
        
        // Act & Assert: No debe lanzar excepción
        salaComun.AddReserva(reserva, new EdificioConfig("AdaByron", 100.0), estudiante);
        Assert.Single(salaComun.Reservas);
    }

    [Fact]
    public void AddReserva_EstudianteReservaAula_LanzaExcepcion()
    {
        var estudiante = CrearPersona(Rol.Estudiante);
        var aula       = CrearEspacio(TipoEspacio.Aula, 50);
        var reserva    = CrearReservaDeIntento(aula, 5);

        Assert.Throws<ExcepcionPermisos>(() => 
            aula.AddReserva(reserva, new EdificioConfig("AdaByron", 100.0), estudiante));
    }

    [Fact]
    public void AddReserva_TecnicoLabReservaLaboratorioMismoDepartamento_Exito()
    {
        var tecnico = CrearPersona(Rol.TecnicoLab, "Informatica");
        var lab     = CrearEspacio(TipoEspacio.Laboratorio, 20, "Informatica");
        var reserva = CrearReservaDeIntento(lab, 5);

        lab.AddReserva(reserva, new EdificioConfig("AdaByron", 100.0), tecnico);
        Assert.Single(lab.Reservas);
    }

    [Fact]
    public void AddReserva_TecnicoLabReservaLaboratorioDistintoDepartamento_Lanza()
    {
        var tecnico = CrearPersona(Rol.TecnicoLab, "Informatica");
        var lab     = CrearEspacio(TipoEspacio.Laboratorio, 20, "Matematicas");
        var reserva = CrearReservaDeIntento(lab, 5);

        Assert.Throws<ExcepcionPermisos>(() => 
            lab.AddReserva(reserva, new EdificioConfig("AdaByron", 100.0), tecnico));
    }

    // ── Pruebas de Aforo (F5 / HU-14 / PBI-5) ────────────────────────────────

    [Theory]
    [InlineData(100, 100.0, 100)] // Límite exacto al 100%
    [InlineData(100, 50.0, 50)]   // Límite exacto al 50%
    [InlineData(60, 10.0, 6)]     // Límite exacto al 10%
    public void AddReserva_LímiteExactoAforo_Exito(int capacidad, double porcentaje, int asistentes)
    {
        var docente = CrearPersona(Rol.Docente, "Informatica");
        var aula    = CrearEspacio(TipoEspacio.Aula, capacidad, "Informatica");
        var reserva = CrearReservaDeIntento(aula, asistentes);

        aula.AddReserva(reserva, new EdificioConfig("AdaByron", porcentaje), docente);
        Assert.Single(aula.Reservas);
    }

    [Theory]
    [InlineData(100, 100.0, 101)] // Supera por 1 al 100%
    [InlineData(100, 50.0, 51)]   // Supera por 1 al 50%
    [InlineData(100, 0.0, 1)]     // Edificio cerrado (0%) cualquier reserva falla
    public void AddReserva_AforoExcedido_LanzaExcepcion(int capacidad, double porcentaje, int asistentes)
    {
        var docente = CrearPersona(Rol.Docente, "Informatica");
        var aula    = CrearEspacio(TipoEspacio.Aula, capacidad, "Informatica");
        var reserva = CrearReservaDeIntento(aula, asistentes);

        Assert.Throws<ExcepcionAforoSuperado>(() => 
            aula.AddReserva(reserva, new EdificioConfig("AdaByron", porcentaje), docente));
    }

    // ── Pruebas de Disponibilidad (F6 / HU-15) ───────────────────────────────

    [Fact]
    public void AddReserva_HorarioSolapado_LanzaExcepcionConflicto()
    {
        // Arrange
        var docente = CrearPersona(Rol.Docente, "Informatica");
        var aula    = CrearEspacio(TipoEspacio.Aula, 100, "Informatica");
        
        var inicio = DateTime.Today.AddHours(14);
        var fin    = DateTime.Today.AddHours(16);
        
        var franja1 = new FranjaHoraria(inicio, fin);
        var r1 = new Reserva(docente.Email, aula.CodigoEspacio, franja1, 10);
        r1.Aceptar(); // Para que el AR lo considere activo
        
        // Simulamos hidratación del AR (en un repo real esto vendría cargado)
        // Usamos reflexión o un método de prueba si la lista es privada, 
        // pero aquí el método AddReserva ya la añade.
        aula.AddReserva(r1, new EdificioConfig("AdaByron", 100.0), docente); 

        // Segunda reserva solapada (15:00 - 17:00)
        var franja2 = new FranjaHoraria(inicio.AddHours(1), fin.AddHours(1));
        var r2 = new Reserva(docente.Email, aula.CodigoEspacio, franja2, 10);

        // Act & Assert
        Assert.Throws<ExcepcionConflictoReserva>(() => 
            aula.AddReserva(r2, new EdificioConfig("AdaByron", 100.0), docente));
    }
}
