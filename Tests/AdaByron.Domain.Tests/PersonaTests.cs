using AdaByron.Domain.Aggregates.PersonAggregate;
using AdaByron.Domain.Exceptions;
using Xunit;

namespace AdaByron.Domain.Tests;

public class PersonaTests
{
    [Fact]
    public void ActualizarDatosAdministrativos_A_DocenteSinDepartamento_LanzaExcepcion()
    {
        var persona = new Persona("conserje@unizar.es", "Ana", "Pérez", Rol.Conserje);

        Assert.Throws<ExcepcionDominio>(() =>
            persona.ActualizarDatosAdministrativos("Ana", "Pérez", Rol.DocenteInvestigador));
    }

    [Fact]
    public void ActualizarDatosAdministrativos_A_Gerente_PermiteDepartamentoVacio()
    {
        var persona = new Persona("docente@unizar.es", "Luis", "Sanz", Rol.DocenteInvestigador, Departamento.Informatica);

        persona.ActualizarDatosAdministrativos("Luis", "Sanz", Rol.Gerente);

        Assert.Equal(Rol.Gerente, persona.Rol);
        Assert.True(persona.Departamento.IsNull);
    }
}
