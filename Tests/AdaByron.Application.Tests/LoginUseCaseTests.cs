using AdaByron.Application.DTOs;
using AdaByron.Application.Ports.Out;
using AdaByron.Application.UseCases.Auth;
using AdaByron.Domain.Entities;
using AdaByron.Domain.Enums;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Interfaces;
using Moq;
using Xunit;

namespace AdaByron.Application.Tests;

public class LoginUseCaseTests
{
    private readonly Mock<IPersonaRepository> _repoMock  = new();
    private readonly Mock<ITokenService>      _tokenMock = new();
    private readonly LoginUseCase             _useCase;

    public LoginUseCaseTests() => _useCase = new(_repoMock.Object, _tokenMock.Object);

    [Fact]
    public async Task Login_ConEmailInexistente_LanzaExcepcion()
    {
        // Arrange: el repositorio devuelve null (usuario no registrado)
        _repoMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Persona)null!);

        // Act & Assert: debe lanzar ExcepcionUsuarioNoRegistrado
        await Assert.ThrowsAsync<ExcepcionUsuarioNoRegistrado>(() =>
            _useCase.ExecuteAsync(new LoginRequestDTO("fantasma@unizar.es")));
    }

    [Fact]
    public async Task Login_ConEmailValido_DevuelveToken()
    {
        // Arrange: la persona existe en la BD
        // Constructor real: (email, nombre, apellidos, rol, departamento?)
        var persona = new Persona("miguel@unizar.es", "Miguel", "López", Rol.Gerente);

        _repoMock
            .Setup(r => r.GetByEmailAsync("miguel@unizar.es"))
            .ReturnsAsync(persona);

        _tokenMock
            .Setup(t => t.GenerarToken(persona))
            .Returns("token-de-mentira");

        // Act
        var resultado = await _useCase.ExecuteAsync(new LoginRequestDTO("miguel@unizar.es"));

        // Assert
        Assert.Equal("token-de-mentira", resultado.Token);
        Assert.Equal("miguel@unizar.es", resultado.Email);
        Assert.Equal(Rol.Gerente, resultado.Rol);
    }
}
