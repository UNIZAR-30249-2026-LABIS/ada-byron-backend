# GIS Setup - Ada Byron

Este directorio contiene los recursos necesarios para reconstruir la parte GIS del proyecto de forma reproducible sobre la base de datos unificada del proyecto.

## Arquitectura actual

El proyecto usa una única base PostgreSQL/PostGIS para:

- **Tablas de negocio** (creadas por EF Core):
  - `personas`
  - `espacios`
  - `reservas`

- **Tablas GIS**:
  - `spaces_floor_s1`
  - `spaces_floor_0`
  - `spaces_floor_1`
  - `spaces_floor_2`
  - `spaces_floor_3`
  - `spaces_floor_4`
  - `spaces_floor_5`

- **Vistas GIS**:
  - `spaces_floor_*_ada_byron`
  - `spaces_floor_*_ada_byron_ui`

## Qué incluye

- `sql/00_spaces_base_tables.sql`  
  Dump de las tablas base importadas en PostGIS:
  - `spaces_floor_s1` a `spaces_floor_5`

- `sql/01_create_ada_byron_views.sql`  
  Crea las vistas filtradas solo al edificio Ada Byron:
  - `spaces_floor_*_ada_byron`

- `sql/02_create_ui_views.sql`  
  Crea las vistas UI para el mapa (`spaces_floor_*_ada_byron_ui`).  
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
```

## Requisitos

- Docker y Docker Compose.
- Base PostgreSQL/PostGIS levantada con el `docker-compose.yml` del backend.
- GeoServer levantado en el mismo docker-compose.
- Migraciones EF Core aplicadas previamente.

## Servicios esperados

**Desde el host:**
- PostgreSQL/PostGIS: `localhost:5433`
- pgAdmin: `localhost:5050`
- GeoServer: `localhost:8080`

**Desde GeoServer hacia PostgreSQL:**
Como ambos servicios están en el mismo docker-compose, GeoServer debe conectarse a PostgreSQL usando:
- **host:** `postgres`
- **port:** `5432`

## Flujo de inicialización recomendado

1. **Levantar infraestructura**
   ```bash
   docker compose up -d
   ```

2. **Aplicar migraciones EF Core**
   ```bash
   dotnet ef database update --project Infrastructure --startup-project AdaByron.API
   ```

3. **Restaurar GIS en la base unificada**
   ```bash
   ./Infrastructure/Geo/scripts/restore_gis.sh
   ```
   *Esto crea las tablas base GIS (`spaces_floor_*`), y las vistas `*_ada_byron` y `*_ada_byron_ui`.*

4. **Configurar GeoServer**
   ```bash
   ./Infrastructure/Geo/scripts/setup_geoserver.sh
   ```
   *Esto crea el workspace `adabyron`, el store `adabyron_postgis_unified`, el estilo `spaces_ui_style` y publica todas las layers UI correspondientes a cada planta.*

## Scripts

### `restore_gis.sh`
Se ejecuta desde el host y se conecta a PostgreSQL usando el host `localhost` y el puerto `5433`.  
Este script restaura las tablas GIS base desde `00_spaces_base_tables.sql` y ejecuta secuencialmente `01_create_ada_byron_views.sql` y `02_create_ui_views.sql`.

### `setup_geoserver.sh`
Configura GeoServer por REST API y crea el store apuntando a PostgreSQL usando el host `postgres` y puerto `5432` (porque GeoServer y PostgreSQL comparten red Docker dentro del mismo docker-compose).  
Este script automatiza la creación del workspace, store, carga del estilo SLD, publicación de las capas y asignación visual.

## Notas importantes

- `restore_gis.sh` está pensado para una base limpia. Si se ejecuta varias veces sobre la misma base, el dump puede dar errores de objetos ya existentes.
- Las vistas se recrean a partir de los scripts SQL mencionados.
- GeoServer no almacena los datos; los lee desde la base PostgreSQL/PostGIS unificada.
- El frontend debe consumir siempre las layers `_ui`.

## Capas usadas por el frontend

- `adabyron:spaces_floor_s1_ada_byron_ui`
- `adabyron:spaces_floor_0_ada_byron_ui`
- `adabyron:spaces_floor_1_ada_byron_ui`
- `adabyron:spaces_floor_2_ada_byron_ui`
- `adabyron:spaces_floor_3_ada_byron_ui`
- `adabyron:spaces_floor_4_ada_byron_ui`
- `adabyron:spaces_floor_5_ada_byron_ui`

## Objetivo de esta estructura

Disponer de una única base de datos para:
- La lógica de negocio del backend.
- Los espacios cartográficos del edificio.
- La publicación WMS en GeoServer.
- El consumo desde el frontend.

---