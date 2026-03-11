namespace AdaByron.Domain.Repositories;

// TODO: Interfaces de repositorio definidas en el DOMINIO (sin dependencias de EF Core)
// Las implementaciones viven en Infrastructure/Persistence/Repositories/
//
// IReservationRepository:
//   - Task<Reservation?> GetByIdAsync(Guid id)
//   - Task<IEnumerable<Reservation>> GetActiveAsync()
//   - Task<IEnumerable<Reservation>> GetInvalidAsync()
//   - Task<Guid>  AddAsync(Reservation reservation)
//   - Task        UpdateStatusAsync(Guid id, ReservationStatus status)
//   - Task        DeleteAsync(Guid id)
//
// ISpaceRepository:
//   - Task<Space?> GetByIdAsync(Guid id)
//   - Task<IEnumerable<Space>> GetAllAsync()
//   - Task<double> GetBuildingOccupancyAsync()   ← implementado con PostGIS en Infrastructure
