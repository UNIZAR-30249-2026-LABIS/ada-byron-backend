using AdaByron.Domain.Aggregates.SpaceAggregate;
using FluentAssertions;
using Xunit;

namespace AdaByron.Domain.Tests;

public class EdificioConfigTests
{
    [Theory]
    [InlineData(100, 100)] // 100% de 100 = 100
    [InlineData(50, 50)]   // 50% de 100 = 50
    [InlineData(0, 0)]     // 0% de 100 = 0
    [InlineData(25, 10)]   // 25% de 40 = 10
    public void Edificio_CalculaCapacidad_Correctamente(double porcentaje, int expectedAsistentes)
    {
        // Arrange
        var edificio = Edificio.AdaByron;
        int aforoSala = porcentaje == 25 ? 40 : 100;
        
        // Act
        var maxCapacidad = edificio.CalcularCapacidadPermitida(aforoSala, porcentaje);
        
        // Assert
        maxCapacidad.Should().Be(expectedAsistentes);
    }
    
    [Fact]
    public void EdificioConfig_PorcentajeMenorQueCero_LanzaArgumentException()
    {
        // Act
        Action act = () => new EdificioConfig("AdaByron", -10);
        
        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*entre 0 y 100*");
    }
    
    [Fact]
    public void EdificioConfig_PorcentajeMayorQue100_LanzaArgumentException()
    {
        // Act
        Action act = () => new EdificioConfig("AdaByron", 150);
        
        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*entre 0 y 100*");
    }
}
