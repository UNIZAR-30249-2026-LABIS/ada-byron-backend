using AdaByron.Application.DTOs;
using AdaByron.Application.Ports.Out;
using AdaByron.Domain.Entities;
using AdaByron.Domain.Exceptions;
using AdaByron.Domain.Interfaces;
using AdaByron.Domain.Policies;
using AdaByron.Domain.ValueObjects;

namespace AdaByron.Application.UseCases.Reservas;

/// <summary>
/// Caso de uso: crear una reserva pasando el Motor de Reglas F1-F6 dentro de
/// una transacción ACID con bloqueo consultivo de PostgreSQL (HU-T2).
/// </summary>
public class CrearReservaUseCase(
    IPersonaRepository     personas,
    IEspacioRepository     espacios,
    IReservaRepository     reservas,
    IUnitOfWork            uow,
    IAforoEdificioService  aforoService)
{
    public async Task<ReservaResponseDTO> ExecuteAsync(CrearReservaRequestDTO request)
    {
        // 1. Cargar entidades (fuera de la TX: lectura rápida sin bloquear)
        var persona = await personas.GetByEmailAsync(request.Email)
            ?? throw new ExcepcionUsuarioNoRegistrado(request.Email);

        var espacio = await espacios.GetByCodigoAsync(request.CodigoEspacio)
            ?? throw new ExcepcionDominio(
                $"No se encontró el espacio con código '{request.CodigoEspacio}'.");

        var franja = new FranjaHoraria(request.Inicio, request.Fin);

        // 2. Obtener el porcentaje de ocupación dinámico del edificio
        var porcentajeOcupacion = await aforoService.GetPorcentajeAsync();

        // ── Inicio de Transacción ACID ────────────────────────────────────────
        await uow.BeginTransactionAsync();
        try
        {
            // 3. Bloqueo consultivo en PostgreSQL: evita que dos peticiones al
            //    mismo espacio/hora entren en la sección crítica a la vez (HU-T2)
            await uow.AcquireEspacioLockAsync(request.CodigoEspacio);

            // 4. Re-leer las reservas del espacio DENTRO de la TX para tener
            //    una vista consistente (evita phantom reads)
            var reservasExistentes = await reservas.GetByEspacioAsync(request.CodigoEspacio);

            // 5. Motor de Reglas F1-F6 (lanza ExcepcionDominio si algo falla)
            PoliticaReserva.ValidarCreacion(
                persona,
                espacio,
                franja,
                request.NumeroAsistentes,
                reservasExistentes,
                porcentajeOcupacion);   // F5 con porcentaje dinámico

            // 6. Crear la reserva y auto-aceptar (flujo normal)
            var reserva = new Reserva(
                personaId:        persona.Email,
                espacioId:        espacio.CodigoEspacio,
                franja:           franja,
                numeroAsistentes: request.NumeroAsistentes);

            reserva.Aceptar();

            // 7. Persistir dentro de la TX — el Commit incluye SaveChanges
            await reservas.AddAsync(reserva);
            await uow.CommitAsync();

            return new ReservaResponseDTO(
                Id:               reserva.Id,
                Email:            reserva.PersonaId,
                CodigoEspacio:    reserva.EspacioId,
                Inicio:           reserva.Franja.Inicio,
                Fin:              reserva.Franja.Fin,
                NumeroAsistentes: reserva.NumeroAsistentes,
                Estado:           reserva.Estado.ToString());
        }
        catch
        {
            await uow.RollbackAsync();
            throw;      // Se propaga al middleware de excepciones de la API
        }
    }
}
