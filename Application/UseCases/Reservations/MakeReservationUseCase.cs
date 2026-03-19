using AdaByron.Application.DTOs;
using AdaByron.Domain.Entities;
using AdaByron.Domain.Interfaces;
using AdaByron.Domain.ValueObjects;

namespace AdaByron.Application.UseCases.Reservations;

public sealed class MakeReservationUseCase
{
    private readonly IPersonaRepository _personaRepository;
    private readonly IEspacioRepository _espacioRepository;
    private readonly IReservaRepository _reservaRepository;

    public MakeReservationUseCase(
        IPersonaRepository personaRepository,
        IEspacioRepository espacioRepository,
        IReservaRepository reservaRepository)
    {
        _personaRepository = personaRepository;
        _espacioRepository = espacioRepository;
        _reservaRepository = reservaRepository;
    }

    public async Task<ReservationDto> Execute(CreateReservationRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Validaciones básicas
        if (request.AttendeeCount <= 0)
            throw new ArgumentException("El número de asistentes debe ser mayor que 0.");

        if (request.StartTime.Date != request.EndTime.Date)
            throw new ArgumentException("La reserva debe realizarse dentro del mismo día.");

        // FranjaHoraria ya valida que StartTime < EndTime y duración máxima
        var franja = new FranjaHoraria(request.StartTime, request.EndTime);

        // 2. Verificar existencia de usuario
        var persona = await _personaRepository.GetByEmailAsync(request.RequesterEmail);
        if (persona == null)
            throw new InvalidOperationException($"No se encontró la persona con email '{request.RequesterEmail}'.");

        // 3. Verificar existencia de espacio
        var espacio = await _espacioRepository.GetByCodigoAsync(request.SpaceId);
        if (espacio == null)
            throw new InvalidOperationException($"No se encontró el espacio con código '{request.SpaceId}'.");

        // 4. Comprobar solapamiento
        var existeSolapamiento = await _reservaRepository.ExisteSolapamientoAsync(request.SpaceId, franja);
        if (existeSolapamiento)
            throw new InvalidOperationException("El espacio ya tiene una reserva en la franja horaria solicitada.");

        // 5. Crear la reserva
        var reserva = new Reserva(request.RequesterEmail, request.SpaceId, franja);

        // 6. Persistir la reserva
        await _reservaRepository.AddAsync(reserva);

        return new ReservationDto
        {
            Id = reserva.Id,
            RequesterEmail = reserva.PersonaId,
            SpaceName = espacio.Nombre,
            StartTime = franja.Inicio,
            EndTime = franja.Fin,
            Status = "Confirmed"
        };
    }
}