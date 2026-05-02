namespace AdaByron.Application.DTOs;

/// <summary>
/// Request para actualizar los datos de un espacio desde el panel de Gerente (HU-XX Admin).
/// </summary>
public sealed class UpdateSpaceRequestDTO
{
    /// <summary>Nombre/Designación del espacio (máx. 200 caracteres).</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Número máximo de personas (> 0).</summary>
    public int Aforo { get; set; }

    /// <summary>Número de planta (-1 = sótano 1, 0 = PB, 1..9).</summary>
    public int Planta { get; set; }

    /// <summary>Categoría funcional para reservas: Aula, Laboratorio, Seminario, SalaComun, Despacho.</summary>
    public string Categoria { get; set; } = string.Empty;

    /// <summary>Indica si el espacio admite reservas puntuales.</summary>
    public bool EsReservable { get; set; }

    /// <summary>Horario semanal de reserva del espacio.</summary>
    public List<UpdateSpaceScheduleDayDTO> HorarioReserva { get; set; } = new();
}

public sealed class UpdateSpaceScheduleDayDTO
{
    public int DiaSemana { get; set; }
    public bool Activo { get; set; }
    public string HoraInicio { get; set; } = "00:00";
    public string HoraFin { get; set; } = "23:59";
}

/// <summary>
/// DTO de respuesta con las reservas de un usuario autenticado (HU-18).
/// </summary>
public sealed class MiReservaDTO
{
    public Guid Id { get; set; }
    public string EspacioId { get; set; } = string.Empty;
    public string NombreEspacio { get; set; } = string.Empty;
    public DateTime Inicio { get; set; }
    public DateTime Fin { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int NumeroAsistentes { get; set; }
}

/// <summary>
/// Request para definir o eliminar el porcentaje de ocupación específico de un espacio (PBI-12 / HU-O1).
/// PorcentajeEspecifico = null → hereda el porcentaje global del edificio.
/// PorcentajeEspecifico ∈ [0, 100] → aplica la restricción específica de este espacio.
/// </summary>
public sealed class SetSpaceOccupancyDTO
{
    /// <summary>
    /// Porcentaje entre 0 y 100, o null para revertir al porcentaje global del edificio.
    /// </summary>
    public double? PorcentajeEspecifico { get; set; }
}
