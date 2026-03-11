namespace AdaByron.Infrastructure.Persistence.DbContext;

// TODO: AdaByronDbContext — configuración de EF Core
// - UseNpgsql(...).UseNetTopologySuite()  →  activa PostGIS para geometrías (Persistence/Geo)
// - DbSets: Reservations, Spaces, Persons
// - ApplyConfigurationsFromAssembly → carga automáticamente /Configurations/*.cs
// - Control de concurrencia optimista: columna xmin (RowVersion nativo de PostgreSQL)
