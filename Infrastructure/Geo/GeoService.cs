namespace AdaByron.Infrastructure.Geo;

// TODO: Capa geoespacial — lógica PostGIS y (futuro) integración GeoServer
//
// GeoService:
//   - CalculateBuildingOccupancyAsync() → usa ST_Area/ST_Within sobre geometrías de Space
//   - FindSpacesWithinBoundsAsync(geometry bounds) → consulta espacial PostGIS
//
// GeoJsonConverter:
//   - ToGeoJson(Geometry geometry) → string GeoJSON para el mapa Leaflet del frontend
