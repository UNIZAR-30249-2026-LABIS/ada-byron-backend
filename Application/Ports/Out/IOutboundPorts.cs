namespace AdaByron.Application.Ports.Out;

// TODO: Puerto de SALIDA — contratos que Application necesita de Infrastructure
// Principio: Application define QUÉ necesita; Infrastructure sabe CÓMO.
//
// Interfaces a definir aquí:
//   IReservationRepository:
//     - Task<Reservation?> GetByIdAsync(Guid id)
//     - Task<IEnumerable<Reservation>> GetActiveAsync()
//     - Task<IEnumerable<Reservation>> GetInvalidAsync()
//     - Task<Guid>  AddAsync(Reservation reservation)
//     - Task        UpdateStatusAsync(Guid id, ReservationStatus status)
//     - Task        DeleteAsync(Guid id)
//
//   ISpaceRepository:
//     - Task<Space?> GetByIdAsync(Guid id)
//     - Task<IEnumerable<Space>> GetAllAsync()
//     - Task<double> GetBuildingOccupancyAsync()  ← PostGIS
//
//   INotificationService:
//     - Task NotifyApprovedAsync(Guid reservationId)
//     - Task NotifyRejectedAsync(Guid reservationId, string reason)
//     - Task NotifyInvalidatedAsync(Guid reservationId)
