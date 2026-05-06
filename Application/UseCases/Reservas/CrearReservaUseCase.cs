using AdaByron.Application.DTOs;
using AdaByron.Application.Ports.Out;
using AdaByron.Domain.Aggregates.SpaceAggregate;
using AdaByron.Domain.Aggregates.ReservationAggregate;
using AdaByron.Domain.Aggregates.PersonAggregate;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Interfaces;
using DomainTipoUso = AdaByron.Domain.Aggregates.ReservationAggregate.TipoUso;

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
    public async Task<ReservaResponseDTO> ExecuteAsync(string emailFromJwt, CrearReservaRequestDTO request)
    {
        // 1. Cargar entidades base — email procede del JWT, no del cuerpo HTTP
        var persona = await personas.GetByEmailAsync(emailFromJwt)
            ?? throw new ExcepcionUsuarioNoRegistrado(emailFromJwt);

        // El frontend envía hora local sin zona horaria (Kind=Unspecified).
        // La validación del horario del espacio ya usa esa hora local correctamente.
        // Antes de persistir, se marca como UTC para que Npgsql pueda escribir
        // en la columna timestamptz sin error (el valor no cambia, solo el Kind).
        var inicio = DateTime.SpecifyKind(request.Inicio, DateTimeKind.Utc);
        var fin    = DateTime.SpecifyKind(request.Fin,    DateTimeKind.Utc);
        var franja = new FranjaHoraria(inicio, fin);
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

            // Parsear TipoUso desde string (viene del frontend como texto)
            DomainTipoUso? tipoUso = null;
            if (!string.IsNullOrWhiteSpace(request.TipoUso) &&
                Enum.TryParse<DomainTipoUso>(request.TipoUso, ignoreCase: true, out var parsed))
            {
                tipoUso = parsed;
            }

            // 2. Crear objeto de intención
            var nuevaReserva = new Reserva(
                personaId:        persona.Email,
                espacioId:        espacio.CodigoEspacio,
                franja:           franja,
                numeroAsistentes: request.NumeroAsistentes,
                tipoUso:          tipoUso,
                descripcion:      request.Descripcion);

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
                Estado:           nuevaReserva.Estado.ToString(),
                TipoUso:          nuevaReserva.TipoUso?.ToString(),
                Descripcion:      nuevaReserva.Descripcion);
        }
        catch
        {
            await uow.RollbackAsync();
            throw;
        }
    }
}
