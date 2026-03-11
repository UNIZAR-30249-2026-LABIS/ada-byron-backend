namespace AdaByron.Application.Ports.In;

// TODO: Puerto de ENTRADA — contratos que los UseCases exponen hacia AdaByron.API
// Principio: los Controllers solo conocen estos puertos, nunca los UseCases directamente.
//
// Interfaces a definir aquí:
//   IReservationUseCases:
//     - Task<Guid>   MakeReservationAsync(MakeReservationCommand cmd)
//     - Task<IEnumerable<ReservationDto>> GetActiveReservationsAsync()
//     - Task          ApproveReservationAsync(Guid id)
//     - Task          RejectReservationAsync(Guid id, string reason)
//
//   ISpaceUseCases:
//     - Task<IEnumerable<SpaceDto>> GetSpacesAsync()
//     - Task<double>  GetBuildingOccupancyAsync()
