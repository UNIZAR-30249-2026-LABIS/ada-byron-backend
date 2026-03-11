namespace AdaByron.Infrastructure.Identity;

// TODO: Gestión de autenticación e identificación
//
// UniversityDirectoryService:
//   - ResolveUserIdentityAsync(email) → UserIdentity { Email, Role, Department }
//   Consulta el directorio universitario (LDAP / REST) para resolver el rol del usuario.
//   El sistema NO gestiona contraseñas. La identificación es stateless por e-mail.
