#!/usr/bin/env bash
set -e

DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
DB_NAME="${DB_NAME:-adabyron}"
DB_USER="${DB_USER:-postgres}"

echo "Restaurando tablas base GIS..."
psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -f Infrastructure/Geo/sql/00_spaces_base_tables.sql

echo "Creando vistas Ada Byron..."
psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -f Infrastructure/Geo/sql/01_create_ada_byron_views.sql

echo "Creando vistas UI..."
psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -f Infrastructure/Geo/sql/02_create_ui_views.sql

echo "GIS restaurado correctamente."