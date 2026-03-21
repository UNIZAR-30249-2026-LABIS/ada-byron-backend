using AdaByron.Application.DTOs;

namespace AdaByron.Application.UseCases.Reservations;

public sealed class ReservationAppService
{
    private readonly MakeReservationUseCase _makeReservationUseCase;

    public ReservationAppService(MakeReservationUseCase makeReservationUseCase)
    {
        _makeReservationUseCase = makeReservationUseCase;
    }

    public Task<ReservationDto> MakeReservation(CreateReservationRequest request, CancellationToken cancellationToken = default)
    {
        return _makeReservationUseCase.Execute(request, cancellationToken);
    }
}
