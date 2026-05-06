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

    // ── Dual-role Gerente + DocenteInvestigador ───────────────────────────────

    /// <summary>La única combinación de dos roles permitida es Gerente+DocenteInvestigador.</summary>
    [Fact]
    public void Crear_GerenteYDocenteInvestigador_TieneAmbosRoles()
    {
        var persona = new Persona("dual@unizar.es", "María", "López",
            Rol.DocenteInvestigador, Departamento.Informatica, esGerente: true);

        Assert.Contains(Rol.Gerente,            persona.Roles);
        Assert.Contains(Rol.DocenteInvestigador, persona.Roles);
        Assert.Equal(2, persona.Roles.Count);
        Assert.True(persona.EsGerente);
    }

    /// <summary>Si se actualiza a Gerente+DocenteInvestigador, la persona refleja ambos roles.</summary>
    [Fact]
    public void Actualizar_A_GerenteYDocenteInvestigador_Exito()
    {
        var persona = new Persona("docente@unizar.es", "Luis", "Sanz",
            Rol.DocenteInvestigador, Departamento.Informatica);

        persona.ActualizarDatosAdministrativos("Luis", "Sanz",
            Rol.DocenteInvestigador, Departamento.Informatica, esGerente: true);

        Assert.True(persona.EsGerente);
        Assert.Contains(Rol.DocenteInvestigador, persona.Roles);
    }

    /// <summary>Cualquier otra combinación de dos roles distintos a Gerente+DocenteInvestigador
    /// debe lanzar ExcepcionDominio.</summary>
    [Theory]
    [InlineData(Rol.Gerente,              Rol.Estudiante)]
    [InlineData(Rol.Gerente,              Rol.Conserje)]
    [InlineData(Rol.Gerente,              Rol.TecnicoLaboratorio)]
    [InlineData(Rol.Gerente,              Rol.InvestigadorContratado)]
    [InlineData(Rol.DocenteInvestigador,  Rol.Estudiante)]
    [InlineData(Rol.Estudiante,           Rol.Conserje)]
    public void Crear_CombinacionInvalidaDeDosRoles_LanzaExcepcion(Rol rol1, Rol rol2)
    {
        Assert.Throws<ExcepcionDominio>(() =>
            new Persona("test@unizar.es", "Test", "User",
                new[] { rol1, rol2 }));
    }
}
