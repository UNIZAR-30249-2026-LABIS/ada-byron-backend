namespace AdaByron.Infrastructure.Persistence.Configurations;

// TODO: Configuraciones Fluent API de EF Core (IEntityTypeConfiguration<T>)
// ReservationConfiguration — tabla "reservations", concurrencia xmin, FK a Space
// SpaceConfiguration        — tabla "spaces", columna "geometry(Polygon,4326)" para PostGIS
// PersonConfiguration       — tabla "persons", PK = Email (string)
