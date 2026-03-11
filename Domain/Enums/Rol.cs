namespace AdaByron.Domain.Enums;

// Roles del sistema. Determinan qué espacios puede reservar cada persona (Regla F).
public enum Rol
{
    Estudiante,  // Solo SalaComun
    TecnicoLab,  // Laboratorios de su departamento
    Docente,     // Laboratorios de su departamento + Aulas/Seminarios/SalasComunes
    Conserje,    // Todo excepto Despacho
    Gerente,     // Acceso total + gestión de reservas inválidas
}
