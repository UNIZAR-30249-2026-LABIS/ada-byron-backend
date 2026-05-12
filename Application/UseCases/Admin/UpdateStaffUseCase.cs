using AdaByron.Application.DTOs;
using AdaByron.Application.Ports.Out;
using AdaByron.Domain.Aggregates.PersonAggregate;
using AdaByron.Domain.Aggregates.ReservationAggregate;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Interfaces;
using AdaByron.Domain.Services;

namespace AdaByron.Application.UseCases.Admin;

/// <summary>
/// Actualiza los datos administrativos (rol, departamento, flag gerente) de una persona existente.
/// Si el departamento cambia, marca como potencialmente inválidas las reservas futuras que ya no son accesibles
/// para la nueva configuración y notifica al usuario vía SignalR.
/// </summary>
public class UpdateStaffUseCase(
    IPersonaRepository personas,
    IReservaRepository reservas,
    IEspacioRepository espacios,
    INotificationService notifications)
{
    public async Task<StaffDTO> ExecuteAsync(string email, UpdateStaffRequestDTO dto)
    {
        var persona = await personas.GetByEmailAsync(email)
            ?? throw new ExcepcionDominio($"No existe ninguna persona con email '{email}'.");

        var rol          = ParseRol(dto.Rol);
        var departamento = ParseDepartamento(dto.Departamento);

        // Capturar estado previo para detectar cambios
        var deptAnterior = persona.Departamento;
        var rolAnterior  = persona.Rol;

        persona.ActualizarDatosAdministrativos(dto.Nombre, dto.Apellidos, rol, departamento, dto.EsGerente);

        await personas.UpdateAsync(persona);

        // Si cambia el departamento O el rol, reevaluar permisos de reservas futuras activas
        bool deptCambio = !deptAnterior.Equals(persona.Departamento);
        bool rolCambio  = rolAnterior != persona.Rol;

        if (deptCambio || rolCambio)
            await MarcarReservasInvalidadasAsync(persona);

        return CreateStaffUseCase.ToDTO(persona);
    }

    private async Task MarcarReservasInvalidadasAsync(Persona persona)
    {
        var politica     = new PoliticaReserva();
        var misReservas  = await reservas.GetByPersonaAsync(persona.Email);
        var ahora        = DateTime.UtcNow;

        var activas = misReservas
            .Where(r => r.Estado is EstadoReserva.Pendiente
                                 or EstadoReserva.Aceptada
                                 or EstadoReserva.PotencialmenteInvalida)
            .Where(r => r.Franja.Fin > ahora)
            .ToList();

        if (activas.Count == 0) return;

        var marcadas = new List<Reserva>();

        foreach (var reserva in activas)
        {
            var espacio = await espacios.GetByCodigoAsync(reserva.EspacioId);
            if (espacio is null) continue;

            try
            {
                politica.VerificarPermisos(persona, espacio);
            }
            catch (ExcepcionPermisos)
            {
                reserva.MarcarComoPotencialmenteInvalida();
                marcadas.Add(reserva);
            }
        }

        if (marcadas.Count == 0) return;

        await reservas.UpdateRangeAsync(marcadas);

        foreach (var reserva in marcadas)
        {
            await notifications.NotifyCancellationAsync(
                persona.Email,
                $"Tu reserva en el espacio {reserva.EspacioId} ha quedado marcada como potencialmente inválida " +
                "porque ya no tienes permiso de acceso tras el cambio de departamento.");
        }
    }

    private static Rol ParseRol(string rol)
    {
        if (!Enum.TryParse<Rol>(rol, ignoreCase: true, out var parsed))
            throw new ExcepcionDominio($"Rol '{rol}' no reconocido.");
        return parsed;
    }

    private static Departamento? ParseDepartamento(string? departamento)
        => string.IsNullOrWhiteSpace(departamento) ? Departamento.Null : Departamento.From(departamento.Trim());
}
