namespace AdaByron.Domain.Policies;

// TODO: PermissionPolicy — Motor de reglas F de permisos por rol
// Implementa las Reglas de Autorización (DDD Policies):
//   CanBook(Role role, SpaceType type, string? personDept, string? spaceDept) → bool
//
// Reglas:
//   Student   → only CommonRoom
//   Concierge → all except Office
//   Technician/Lecturer → only Lab of same department
//   Manager   → all + InvalidReservation management


namespace AdaByron.Domain.Policies;

using AdaByron.Domain.Enums;

public static class PoliticaPermisos
{
    public static bool CanBook(
        Rol rol,
        TipoEspacio categoriaReserva,
        string? departamentoPersona,
        string? departamentoEspacio)
    {
        return rol switch
        {
            Rol.Estudiante => CanEstudianteBook(categoriaReserva),

            Rol.TecnicoLab => CanTecnicoLabBook(
                categoriaReserva,
                departamentoPersona,
                departamentoEspacio
            ),

            Rol.Docente => CanDocenteBook(
                categoriaReserva,
                departamentoPersona,
                departamentoEspacio
            ),

            Rol.Conserje => CanConserjeBook(categoriaReserva),

            Rol.Gerente => CanGerenteBook(categoriaReserva),

            _ => false
        };
    }

    private static bool CanEstudianteBook(TipoEspacio categoriaReserva)
    {
        return categoriaReserva == TipoEspacio.SalaComun;
    }

    private static bool CanTecnicoLabBook(
        TipoEspacio categoriaReserva,
        string? departamentoPersona,
        string? departamentoEspacio)
    {
        return categoriaReserva switch
        {
            TipoEspacio.SalaComun => true,
            TipoEspacio.Seminario => true,
            TipoEspacio.Laboratorio => MismoDepartamento(departamentoPersona, departamentoEspacio),
            _ => false
        };
    }

    private static bool CanDocenteBook(
        TipoEspacio categoriaReserva,
        string? departamentoPersona,
        string? departamentoEspacio)
    {
        return categoriaReserva switch
        {
            TipoEspacio.SalaComun => true,
            TipoEspacio.Aula => true,
            TipoEspacio.Seminario => true,
            TipoEspacio.Laboratorio => MismoDepartamento(departamentoPersona, departamentoEspacio),
            _ => false
        };
    }

    private static bool CanConserjeBook(TipoEspacio categoriaReserva)
    {
        return categoriaReserva != TipoEspacio.Despacho;
    }

    private static bool CanGerenteBook(TipoEspacio categoriaReserva)
    {
        return categoriaReserva != TipoEspacio.Despacho;
    }

    private static bool MismoDepartamento(string? departamentoPersona, string? departamentoEspacio)
    {
        if (string.IsNullOrWhiteSpace(departamentoPersona) ||
            string.IsNullOrWhiteSpace(departamentoEspacio))
        {
            return false;
        }

        return departamentoPersona.Trim().Equals(
            departamentoEspacio.Trim(),
            StringComparison.OrdinalIgnoreCase
        );
    }
}