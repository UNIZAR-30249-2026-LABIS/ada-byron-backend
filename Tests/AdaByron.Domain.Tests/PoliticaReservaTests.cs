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
        // Fecha fija en el futuro para evitar que cruzar medianoche falle la validación de horario
        var inicio = new DateTime(2030, 6, 16, 10, 0, 0);
        var franja = new FranjaHoraria(inicio, inicio.AddHours(1));
        return new Reserva("test@unizar.es", espacio.CodigoEspacio, franja, asistentes);
    }

    // ── Pruebas de mutabilidad de categoría (Regla C) ───────────────────────

    [Theory]
    [InlineData(TipoEspacio.Aula, TipoEspacio.Aula)]
    [InlineData(TipoEspacio.Aula, TipoEspacio.Laboratorio)]
    [InlineData(TipoEspacio.Aula, TipoEspacio.Seminario)]
    [InlineData(TipoEspacio.Aula, TipoEspacio.SalaComun)]
    [InlineData(TipoEspacio.Laboratorio, TipoEspacio.Aula)]
    [InlineData(TipoEspacio.Laboratorio, TipoEspacio.Laboratorio)]
    [InlineData(TipoEspacio.Laboratorio, TipoEspacio.Seminario)]
    [InlineData(TipoEspacio.Seminario, TipoEspacio.Aula)]
    [InlineData(TipoEspacio.Seminario, TipoEspacio.Laboratorio)]
    [InlineData(TipoEspacio.Seminario, TipoEspacio.Seminario)]
    [InlineData(TipoEspacio.Seminario, TipoEspacio.SalaComun)]
    [InlineData(TipoEspacio.SalaComun, TipoEspacio.Seminario)]
    [InlineData(TipoEspacio.SalaComun, TipoEspacio.SalaComun)]
    public void Actualizar_CambioCategoriaPermitido_ActualizaCategoriaReserva(TipoEspacio tipoFisico, TipoEspacio categoriaReserva)
    {
        var espacio = CrearEspacio(tipoFisico, 30);

        espacio.Actualizar(
            "Sala Test",
            Aforo.De(30),
            Planta.De(1),
            categoriaReserva,
            true,
            HorarioReservaDia.CrearHorarioPorDefecto());

        Assert.Equal(categoriaReserva, espacio.CategoriaReserva);
        Assert.Equal(tipoFisico, espacio.TipoFisico);
    }

    [Theory]
    [InlineData(TipoEspacio.Aula, TipoEspacio.Despacho)]
    [InlineData(TipoEspacio.Laboratorio, TipoEspacio.SalaComun)]
    [InlineData(TipoEspacio.Laboratorio, TipoEspacio.Despacho)]
    [InlineData(TipoEspacio.Seminario, TipoEspacio.Despacho)]
    [InlineData(TipoEspacio.SalaComun, TipoEspacio.Aula)]
    [InlineData(TipoEspacio.SalaComun, TipoEspacio.Laboratorio)]
    [InlineData(TipoEspacio.SalaComun, TipoEspacio.Despacho)]
    [InlineData(TipoEspacio.Despacho, TipoEspacio.Aula)]
    [InlineData(TipoEspacio.Despacho, TipoEspacio.Laboratorio)]
    [InlineData(TipoEspacio.Despacho, TipoEspacio.Seminario)]
    [InlineData(TipoEspacio.Despacho, TipoEspacio.SalaComun)]
    public void Actualizar_CambioCategoriaNoPermitido_LanzaExcepcionCambioCategoria(TipoEspacio tipoFisico, TipoEspacio categoriaReserva)
    {
        var espacio = CrearEspacio(tipoFisico, 30);

        Assert.Throws<ExcepcionCambioCategoria>(() => espacio.Actualizar(
            "Sala Test",
            Aforo.De(30),
            Planta.De(1),
            categoriaReserva,
            tipoFisico != TipoEspacio.Despacho,
            HorarioReservaDia.CrearHorarioPorDefecto()));
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
        var tecnico = CrearPersona(Rol.TecnicoLaboratorio, "Informática");
        var lab     = CrearEspacio(TipoEspacio.Laboratorio, 20, "Informática");
        var reserva = CrearReservaDeIntento(lab, 5);

        lab.AddReserva(reserva, new EdificioConfig("AdaByron", 100.0), tecnico);
        Assert.Single(lab.Reservas);
    }

    [Fact]
    public void AddReserva_TecnicoLabReservaLaboratorioDistintoDepartamento_Lanza()
    {
        var tecnico = CrearPersona(Rol.TecnicoLaboratorio, "Informática");
        var lab     = CrearEspacio(TipoEspacio.Laboratorio, 20, "Ing. de Sistemas e Ing. Electrónica y Comunicaciones");
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
        var docente = CrearPersona(Rol.DocenteInvestigador, "Informática");
        var aula    = CrearEspacio(TipoEspacio.Aula, capacidad, "Informática");
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
        var docente = CrearPersona(Rol.DocenteInvestigador, "Informática");
        var aula    = CrearEspacio(TipoEspacio.Aula, capacidad, "Informática");
        var reserva = CrearReservaDeIntento(aula, asistentes);

        Assert.Throws<ExcepcionAforoSuperado>(() => 
            aula.AddReserva(reserva, new EdificioConfig("AdaByron", porcentaje), docente));
    }

    // ── Pruebas de Disponibilidad (F6 / HU-15) ───────────────────────────────

    [Fact]
    public void AddReserva_HorarioSolapado_LanzaExcepcionConflicto()
    {
        // Arrange
        var docente = CrearPersona(Rol.DocenteInvestigador, "Informática");
        var aula    = CrearEspacio(TipoEspacio.Aula, 100, "Informática");
        
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

    [Fact]
    public void AddReserva_EspacioNoReservable_LanzaExcepcion()
    {
        var gerente = CrearPersona(Rol.Gerente);
        var seminario = CrearEspacio(TipoEspacio.Seminario, 30, "Informatica");
        seminario.ActualizarConfiguracionReserva(false, HorarioReservaDia.CrearHorarioPorDefecto());
        var reserva = CrearReservaDeIntento(seminario, 5);

        Assert.Throws<ExcepcionDominio>(() =>
            seminario.AddReserva(reserva, new EdificioConfig("AdaByron", 100.0), gerente));
    }

    [Fact]
    public void AddReserva_FueraDelHorarioPermitido_LanzaExcepcion()
    {
        var gerente = CrearPersona(Rol.Gerente);
        var aula = CrearEspacio(TipoEspacio.Aula, 30, "Informatica");
        var horario = Enumerable.Range(0, 7)
            .Select(dia => new HorarioReservaDia
            {
                DiaSemana = dia,
                Activo = dia == (int)DateTime.Today.DayOfWeek,
                HoraInicio = "08:00",
                HoraFin = "12:00"
            })
            .ToList();
        aula.ActualizarConfiguracionReserva(true, horario);

        var franja = new FranjaHoraria(DateTime.Today.AddHours(13), DateTime.Today.AddHours(14));
        var reserva = new Reserva("test@unizar.es", aula.CodigoEspacio, franja, 5);

        Assert.Throws<ExcepcionDominio>(() =>
            aula.AddReserva(reserva, new EdificioConfig("AdaByron", 100.0), gerente));
    }
}
