using AdaByron.Application.DTOs;
using AdaByron.Application.Ports.Out;
using AdaByron.Application.UseCases.Reservas;
using AdaByron.Domain.Aggregates.PersonAggregate;
using AdaByron.Domain.Aggregates.SpaceAggregate;
using AdaByron.Domain.Aggregates.ReservationAggregate;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Interfaces;
using Moq;
using Xunit;

namespace AdaByron.Application.Tests;

public class CrearReservaUseCaseTests
{
    private readonly Mock<IPersonaRepository> _personasMock = new();
    private readonly Mock<IEspacioRepository> _espaciosMock = new();
    private readonly Mock<IReservaRepository> _reservasMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IEdificioConfigRepository> _configMock = new();

    private readonly CrearReservaUseCase _useCase;

    public CrearReservaUseCaseTests()
    {
        _useCase = new CrearReservaUseCase(
            _personasMock.Object,
            _espaciosMock.Object,
            _reservasMock.Object,
            _uowMock.Object,
            _configMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_EspacioNoExiste_LanzaExcepcionDominio()
    {
        _personasMock.Setup(p => p.GetByEmailAsync("test@unizar.es"))
            .ReturnsAsync(new Persona("test@unizar.es", "Test", "User", Rol.Docente, new Departamento("Informatica")));
        
        _espaciosMock.Setup(e => e.GetByCodigoAsync("A-01")).ReturnsAsync((Espacio)null!);

        var req = new CrearReservaRequestDTO(
            Email: "test@unizar.es",
            CodigoEspacio: "A-01",
            NumeroAsistentes: 10,
            Inicio: DateTime.Now.AddHours(1),
            Fin: DateTime.Now.AddHours(2)
        );

        await Assert.ThrowsAsync<ExcepcionDominio>(() => _useCase.ExecuteAsync(req));
    }

    [Fact]
    public async Task ExecuteAsync_HappyPath_GeneraCommitYToken()
    {
        var persona = new Persona("docente@unizar.es", "Juan", "Perez", Rol.Docente, new Departamento("Informatica"));
        var espacio = new Espacio("A-01", "Aula", Planta.De(1), Aforo.De(50), TipoEspacio.Aula, null);
        
        _personasMock.Setup(p => p.GetByEmailAsync("docente@unizar.es")).ReturnsAsync(persona);
        _espaciosMock.Setup(e => e.GetByCodigoAsync("A-01")).ReturnsAsync(espacio);
        _configMock.Setup(a => a.GetConfigAsync()).ReturnsAsync(new EdificioConfig("AdaByron", 100.0));
        _reservasMock.Setup(r => r.GetByEspacioAsync("A-01")).ReturnsAsync(new List<Reserva>());

        var req = new CrearReservaRequestDTO(
            Email: "docente@unizar.es",
            CodigoEspacio: "A-01",
            NumeroAsistentes: 20,
            Inicio: DateTime.Now.AddDays(1),
            Fin: DateTime.Now.AddDays(1).AddHours(2)
        );

        var response = await _useCase.ExecuteAsync(req);

        Assert.Equal("A-01", response.CodigoEspacio);
        Assert.Equal(EstadoReserva.Aceptada.ToString(), response.Estado);

        // Verifica que se haya guardado y commiteado en la BD
        _uowMock.Verify(u => u.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted), Times.Once);
        _uowMock.Verify(u => u.AcquireEspacioLockAsync("A-01"), Times.Once);
        _reservasMock.Verify(r => r.AddAsync(It.IsAny<Reserva>()), Times.Once);
        _uowMock.Verify(u => u.CommitAsync(), Times.Once);
    }
}
