using AdaByron.Application.DTOs;
using AdaByron.Application.Ports.Out;
using AdaByron.Domain.Aggregates.SpaceAggregate;
using AdaByron.Domain.Aggregates.ReservationAggregate;
using AdaByron.Domain.Aggregates.PersonAggregate;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Interfaces;

namespace AdaByron.Application.UseCases.Reservas;

/// <summary>
/// Caso de uso: crear una reserva mediante el Aggregate Root 'Espacio' (AR).
/// Encapsula la lógica de negocio (HU-13, HU-14, HU-15) delegando en el modelo de dominio.
/// </summary>
public class CrearReservaUseCase(
    IPersonaRepository     personas,
    IEspacioRepository     espacios,
    IReservaRepository     reservas,
    IUnitOfWork            uow,
    IEdificioConfigRepository configRepo)
{
    public async Task<ReservaResponseDTO> ExecuteAsync(CrearReservaRequestDTO request)
    {
        // 1. Cargar entidades base
        var persona = await personas.GetByEmailAsync(request.Email)
            ?? throw new ExcepcionUsuarioNoRegistrado(request.Email);

        var franja = new FranjaHoraria(request.Inicio, request.Fin);
        var configEdificio = await configRepo.GetConfigAsync() 
                             ?? new EdificioConfig("AdaByron", 100); // 100% por defecto si no hay nada en BD

        // ── Inicio de Transacción ACID ────────────────────────────────────────
        await uow.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        try
        {
            // Bloqueo consultivo para evitar concurrencia en el mismo espacio (HU-T2)
            await uow.AcquireEspacioLockAsync(request.CodigoEspacio);

            // Se debe cargar o recargar el Agregado DESPUÉS de adquirir el Lock 
            // para que un hilo vea los check-ins/commits realizados por hilos anteriores que soltaron su Lock
            var espacio = await espacios.GetByCodigoAsync(request.CodigoEspacio)
                ?? throw new ExcepcionDominio($"Espacio '{request.CodigoEspacio}' no encontrado.");

            // 2. Crear objeto de intención
            var nuevaReserva = new Reserva(
                personaId:        persona.Email,
                espacioId:        espacio.CodigoEspacio,
                franja:           franja,
                numeroAsistentes: request.NumeroAsistentes);

            // 3. El AR gestiona la creación y valida todas las Reglas F (HU-13, 14, 15)
            // Se le pasan las reservas existentes para la validación horaria local.
            // Para esta iteración, el AR valida internamente con su lógica encapsulada.
            espacio.AddReserva(nuevaReserva, configEdificio, persona);

            // Si pasa las reglas, autoconfírmamos (según flujo actual)
            nuevaReserva.Aceptar();

            // 4. Persistir
            await reservas.AddAsync(nuevaReserva);
            await uow.CommitAsync();

            return new ReservaResponseDTO(
                Id:               nuevaReserva.Id,
                Email:            nuevaReserva.PersonaId,
                CodigoEspacio:    nuevaReserva.EspacioId,
                Inicio:           nuevaReserva.Franja.Inicio,
                Fin:              nuevaReserva.Franja.Fin,
                NumeroAsistentes: nuevaReserva.NumeroAsistentes,
                Estado:           nuevaReserva.Estado.ToString());
        }
        catch
        {
            await uow.RollbackAsync();
            throw;
        }
    }
}
