using AdaByron.Application.DTOs;
using AdaByron.Application.Ports.Out;
using AdaByron.Domain.Entities;
using AdaByron.Domain.Interfaces;
using AdaByron.Domain.Policies;
using AdaByron.Domain.ValueObjects;

namespace AdaByron.Application.UseCases.Reservations;

public sealed class MakeReservationUseCase
{
    private readonly IPersonaRepository _personaRepository;
    private readonly IEspacioRepository _espacioRepository;
    private readonly IReservaRepository _reservaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAforoEdificioService _aforoService;

    public MakeReservationUseCase(
        IPersonaRepository personaRepository,
        IEspacioRepository espacioRepository,
        IReservaRepository reservaRepository,
        IUnitOfWork unitOfWork,
        IAforoEdificioService aforoService)
    {
        _personaRepository = personaRepository;
        _espacioRepository = espacioRepository;
        _reservaRepository = reservaRepository;
        _unitOfWork = unitOfWork;
        _aforoService = aforoService;
    }

    public async Task<ReservationDto> Execute(CreateReservationRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Cargar entidades fuera de la transacción para mantenerla corta
        var persona = await _personaRepository.GetByEmailAsync(request.RequesterEmail);
        if (persona == null)
            throw new InvalidOperationException($"No se encontró la persona con email '{request.RequesterEmail}'.");

        var espacio = await _espacioRepository.GetByCodigoAsync(request.SpaceId);
        if (espacio == null)
            throw new InvalidOperationException($"No se encontró el espacio con código '{request.SpaceId}'.");

        var franja = new FranjaHoraria(request.StartTime, request.EndTime);
        var porcentajeEdificio = await _aforoService.GetPorcentajeAsync();

        // 2. Transacción ACID: RepeatableRead + Lock Consultivo (HU-T2)
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.AcquireEspacioLockAsync(request.SpaceId);

            // Re-chequeo del solapamiento DENTRO del lock para evitar dobles reservas concurrentes
            var existeSolapamiento = await _reservaRepository.ExisteSolapamientoAsync(request.SpaceId, franja);

            // 3. Aplicar Política de Reservas (Dominio: Permisos HU-13, Disponibilidad HU-15, Aforo HU-14)
            // ValidarCreacion lanzará ExcepcionPermisos, ExcepcionConflictoReserva o ExcepcionAforoSuperado (HU-16)
            PoliticaReserva.ValidarCreacion(
                persona,
                espacio,
                departamentoEspacio: null, // Dependerá de si Espacio.Departamento es implementado, pasamos null por ahora, o podría ser espacio.Nombre u otro
                franja,
                request.AttendeeCount,
                existeSolapamiento,
                porcentajeEdificio);

            // 4. Crear y persistir la entidad Reserva
            var reserva = new Reserva(request.RequesterEmail, request.SpaceId, franja);
            
            // Nota: Podría establecerse su Estado o AttendeeCount si la entidad Reservation estuviera actualizada con esos campos
            // Como el usuario eliminó dichos campos de la última versión de Reserva.cs, pasamos solo los exigidos por el constructor
            
            await _reservaRepository.AddAsync(reserva);
            
            // 5. Commit de la Transacción
            await _unitOfWork.CommitAsync();

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
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw; // Excepciones de Dominio (ExcepcionPermisos, etc) suben y son capturadas por ExceptionHandlingMiddleware
        }
    }
}