using AdaByron.Application.DTOs;
using AdaByron.Application.Ports.Out;
using AdaByron.Application.UseCases.Admin;
using AdaByron.Domain.Aggregates.PersonAggregate;
using AdaByron.Domain.Aggregates.ReservationAggregate;
using AdaByron.Domain.Aggregates.SpaceAggregate;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace AdaByron.Application.Tests;

public class UpdateBuildingConfigUseCaseTests
{
    private readonly Mock<IEdificioConfigRepository> _configRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly UpdateBuildingConfigUseCase _useCase;

    public UpdateBuildingConfigUseCaseTests()
    {
        _useCase = new UpdateBuildingConfigUseCase(_configRepoMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_CambiarAforo25Porciento_AplicaYLimitaSalas()
    {
        // 1. Arrange & Act: Simulamos caso de éxito, el Gerente cambia a 25% (PBI 6)
        var dto = new UpdateConfigDTO(25);
        await _useCase.ExecuteAsync(dto);

        // Verificamos que se han guardado los cambios en el caso de uso del Gerente
        _configRepoMock.Verify(c => c.UpdateConfigAsync(It.Is<EdificioConfig>(config => config.PorcentajeOcupacion == 25)), Times.Once);
        _uowMock.Verify(u => u.CommitAsync(), Times.Once);

        // 2. Comprobamos la integración en la regla de negocio del Agregado
        var sala = new Espacio("A-01", "Aula Magna", Planta.De(1), Aforo.De(40), TipoEspacio.Aula, null);
        var config = new EdificioConfig("AdaByron", 25);
        var docente = new Persona("docente@unizar.es", "Juan", "Perez", Rol.Docente, new Departamento("Informatica"));
        var reserva = new Reserva(docente.Email, sala.CodigoEspacio, new FranjaHoraria(DateTime.Now.AddHours(1), DateTime.Now.AddHours(2)), 11);

        // Assert: una sala de 40 plazas al 25% solo permite 10 personas, si metemos 11 debe petar
        Action reservaAction = () => sala.AddReserva(reserva, config, docente);
        reservaAction.Should().Throw<ExcepcionAforoSuperado>().WithMessage("*10*");
    }

    [Theory]
    [InlineData(-10)]
    [InlineData(150)]
    public async Task ExecuteAsync_ValorInvalido_LanzaExcepcionDominio(int porcentajeInvalido)
    {
        // Arrange
        var dto = new UpdateConfigDTO(porcentajeInvalido);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ExcepcionDominio>().WithMessage("*entre 0 y 100*");
        _uowMock.Verify(u => u.RollbackAsync(), Times.Once);
    }
}
