using AdaByron.Application.DTOs;
using AdaByron.Domain.Interfaces;
using AdaByron.Domain.Aggregates.ReservationAggregate;

namespace AdaByron.Application.UseCases.Reservations;

public class GetLiveReservationsUseCase(IReservaRepository reservas)
{
    public async Task<IEnumerable<LiveReservationDTO>> ExecuteAsync()
    {
        var data = await reservas.GetLiveWithDetailsAsync();

        return data.Select(item => new LiveReservationDTO(
            Id: item.Item1.Id,
            EspacioId: item.Item1.EspacioId,
            NombreEspacio: item.NombreEspacio,
            Solicitante: item.NombreUsuario,
            Inicio: item.Item1.Franja.Inicio,
            Fin: item.Item1.Franja.Fin,
            Estado: item.Item1.Estado.ToString(),
            EsPotencialmenteInvalida: item.Item1.Estado == EstadoReserva.PotencialmenteInvalida
        ));
    }
}
