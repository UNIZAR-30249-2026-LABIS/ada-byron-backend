# GIS Setup - Ada Byron

Este directorio contiene los recursos necesarios para reconstruir la parte GIS del proyecto de forma reproducible.

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
  Restaura el dump base y ejecuta los SQL de vistas.

- `scripts/setup_geoserver.sh`  
  Configura GeoServer automáticamente:
  - crea el workspace
  - crea el store PostGIS
  - publica las layers `_ui`
  - sube el estilo SLD
  - asigna el estilo a las layers

## Flujo de datos

```text
GeoJSON -> PostGIS (tablas base) -> vistas Ada Byron -> vistas UI -> GeoServer -> WMS -> Frontend