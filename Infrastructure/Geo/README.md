# GIS Setup - Ada Byron

Este directorio contiene los recursos necesarios para reconstruir la parte GIS del proyecto de forma reproducible sobre la base de datos unificada del proyecto.

## Arquitectura actual

El proyecto usa una única base PostgreSQL/PostGIS para:

- tablas de negocio creadas por EF Core:
  - `personas`
  - `espacios`
  - `reservas`

- tablas GIS:
  - `spaces_floor_s1`
  - `spaces_floor_0`
  - `spaces_floor_1`
  - `spaces_floor_2`
  - `spaces_floor_3`
  - `spaces_floor_4`
  - `spaces_floor_5`

- vistas GIS:
  - `spaces_floor_*_ada_byron`
  - `spaces_floor_*_ada_byron_ui`

## Qué incluye

- `sql/00_spaces_base_tables.sql`  
  Dump de las tablas base importadas en PostGIS:
  - `spaces_floor_s1`
  - `spaces_floor_0`
  - `spaces_floor_1`
  - `spaces_floor_2`
  - `spaces_floor_3`
  - `spaces_floor_4`
  - `spaces_floor_5`

- `sql/01_create_ada_byron_views.sql`  
  Crea las vistas filtradas solo al edificio Ada Byron:
  - `spaces_floor_*_ada_byron`

- `sql/02_create_ui_views.sql`  
  Crea las vistas UI para el mapa:
  - `spaces_floor_*_ada_byron_ui`

  Estas vistas añaden:
  - `display_category`
  - `is_reservable_candidate`

- `styles/spaces_ui_style.sld`  
  Estilo cartográfico aplicado a las capas `_ui` en GeoServer.

- `scripts/restore_gis.sh`  
  Restaura las tablas base GIS en la base unificada y ejecuta los SQL de vistas.

- `scripts/setup_geoserver.sh`  
  Configura GeoServer automáticamente:
  - crea el workspace
  - crea el store PostGIS
  - publica las layers `_ui`
  - sube el estilo SLD
  - asigna el estilo a las layers

## Flujo de datos

```text
Dump GIS -> PostgreSQL/PostGIS unificado -> vistas Ada Byron -> vistas UI -> GeoServer -> WMS -> Frontend