namespace AdaByron.Application.DTOs;

public sealed class StaffDTO
{
    public string Email { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string[] Roles { get; set; } = [];
    public bool EsGerente { get; set; }
    public string? Departamento { get; set; }
}

public sealed class CreateStaffRequestDTO
{
    public string Email { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string? Departamento { get; set; }
    public bool EsGerente { get; set; }
}

public sealed class UpdateStaffRequestDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string? Departamento { get; set; }
    public bool EsGerente { get; set; }
}
