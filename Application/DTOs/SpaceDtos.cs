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
