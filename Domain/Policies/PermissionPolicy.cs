namespace AdaByron.Domain.Policies;

// TODO: PermissionPolicy — Motor de reglas F de permisos por rol
// Implementa las Reglas de Autorización (DDD Policies):
//   CanBook(Role role, SpaceType type, string? personDept, string? spaceDept) → bool
//
// Reglas:
//   Student   → only CommonRoom
//   Concierge → all except Office
//   Technician/Lecturer → only Lab of same department
//   Manager   → all + InvalidReservation management
