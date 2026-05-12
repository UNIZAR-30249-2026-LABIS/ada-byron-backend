using AdaByron.Application.UseCases.Reservations;
using AdaByron.Domain.Aggregates.ReservationAggregate;
using AdaByron.Domain.Aggregates.SpaceAggregate;
using AdaByron.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace AdaByron.Application.Tests;

public class ReevaluarReservasEspacioUseCaseTests
{
    private readonly Mock<IReservaRepository> _reservas = new();
    private readonly Mock<IEdificioConfigRepository> _config = new();

    public ReevaluarReservasEspacioUseCaseTests()
    {
        _config.Setup(c => c.GetConfigAsync())
            .ReturnsAsync(new EdificioConfig("AdaByron", 100));
        _reservas.Setup(r => r.GetPotencialmenteInvalidasFuturasByEspacioAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Reserva>());
    }

    [Fact]
    public async Task ExecuteAsync_EspacioDejaDeSerReservable_MarcaReservaAceptadaComoPotencialmenteInvalida()
    {
        var espacio = CrearEspacio();
        var reserva = CrearReservaAceptada(espacio, new DateTime(2030, 6, 17, 10, 0, 0));
        espacio.ActualizarConfiguracionReserva(false, HorarioReservaDia.CrearHorarioPorDefecto());
        _reservas.Setup(r => r.GetAceptadasFuturasByEspacioAsync(espacio.CodigoEspacio))
            .ReturnsAsync(new[] { reserva });

        var result = await CrearUseCase().ExecuteAsync(espacio);

        result.Should().Be(1);
        reserva.Estado.Should().Be(EstadoReserva.PotencialmenteInvalida);
        _reservas.Verify(r => r.UpdateRangeAsync(It.Is<IEnumerable<Reserva>>(items => items.Single() == reserva)), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DiaDeLaReservaDesactivado_MarcaReservaAceptadaComoPotencialmenteInvalida()
    {
        var espacio = CrearEspacio();
        var inicioMartes = new DateTime(2030, 6, 18, 10, 0, 0);
        var reserva = CrearReservaAceptada(espacio, inicioMartes);
        var horarioSinMartes = HorarioReservaDia.CrearHorarioPorDefecto()
            .Select(h => h.DiaSemana == (int)inicioMartes.DayOfWeek
                ? new HorarioReservaDia { DiaSemana = h.DiaSemana, Activo = false }
                : h)
            .ToList();
        espacio.ActualizarConfiguracionReserva(true, horarioSinMartes);
        _reservas.Setup(r => r.GetAceptadasFuturasByEspacioAsync(espacio.CodigoEspacio))
            .ReturnsAsync(new[] { reserva });

        var result = await CrearUseCase().ExecuteAsync(espacio);

        result.Should().Be(1);
        reserva.Estado.Should().Be(EstadoReserva.PotencialmenteInvalida);
    }

    [Fact]
    public async Task ExecuteAsync_ReservaQuedaFueraDelNuevoHorario_MarcaReservaAceptadaComoPotencialmenteInvalida()
    {
        var espacio = CrearEspacio();
        var inicio = new DateTime(2030, 6, 17, 10, 0, 0);
        var reserva = CrearReservaAceptada(espacio, inicio, inicio.AddHours(2));
        var horarioHastaLasOnce = HorarioReservaDia.CrearHorarioPorDefecto()
            .Select(h => h.DiaSemana == (int)inicio.DayOfWeek
                ? new HorarioReservaDia { DiaSemana = h.DiaSemana, Activo = true, HoraInicio = "09:00", HoraFin = "11:00" }
                : h)
            .ToList();
        espacio.ActualizarConfiguracionReserva(true, horarioHastaLasOnce);
        _reservas.Setup(r => r.GetAceptadasFuturasByEspacioAsync(espacio.CodigoEspacio))
            .ReturnsAsync(new[] { reserva });

        var result = await CrearUseCase().ExecuteAsync(espacio);

        result.Should().Be(1);
        reserva.Estado.Should().Be(EstadoReserva.PotencialmenteInvalida);
    }

    private ReevaluarReservasEspacioUseCase CrearUseCase()
        => new(_reservas.Object, _config.Object);

    private static Espacio CrearEspacio()
        => new("A-01", "Aula Test", Planta.De(1), Aforo.De(40), TipoEspacio.Aula);

    private static Reserva CrearReservaAceptada(Espacio espacio, DateTime inicio, DateTime? fin = null)
    {
        var reserva = new Reserva(
            "docente@unizar.es",
            espacio.CodigoEspacio,
            new FranjaHoraria(inicio, fin ?? inicio.AddHours(1)),
            10);
        reserva.Aceptar();
        return reserva;
    }
}
