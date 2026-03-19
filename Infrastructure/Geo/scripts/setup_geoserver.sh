#!/usr/bin/env bash
set -euo pipefail

GEOSERVER_URL="${GEOSERVER_URL:-http://localhost:8080/geoserver}"
GEOSERVER_USER="${GEOSERVER_USER:-admin}"
GEOSERVER_PASSWORD="${GEOSERVER_PASSWORD:-geoserver}"

WORKSPACE="${WORKSPACE:-adabyron}"
STORE="${STORE:-adabyron_postgis_unified}"

# Como GeoServer y PostgreSQL están en el mismo docker-compose:
DB_HOST="${DB_HOST:-postgres}"
DB_PORT="${DB_PORT:-5432}"
DB_NAME="${DB_NAME:-adabyron}"
DB_USER="${DB_USER:-adabyron_user}"
DB_PASSWORD="${DB_PASSWORD:-adabyron_pass}"

STYLE_NAME="${STYLE_NAME:-spaces_ui_style}"
STYLE_FILE="${STYLE_FILE:-Infrastructure/Geo/styles/spaces_ui_style.sld}"

LAYERS=(
  "spaces_floor_s1_ada_byron_ui"
  "spaces_floor_0_ada_byron_ui"
  "spaces_floor_1_ada_byron_ui"
  "spaces_floor_2_ada_byron_ui"
  "spaces_floor_3_ada_byron_ui"
  "spaces_floor_4_ada_byron_ui"
  "spaces_floor_5_ada_byron_ui"
)

echo "Esperando a GeoServer..."
until curl -s -u "${GEOSERVER_USER}:${GEOSERVER_PASSWORD}" \
  "${GEOSERVER_URL}/rest/about/version.xml" >/dev/null; do
  sleep 3
done

echo "GeoServer disponible."

echo "1) Creando workspace '${WORKSPACE}'..."
curl -s -u "${GEOSERVER_USER}:${GEOSERVER_PASSWORD}" \
  -XPOST \
  -H "Content-type: text/xml" \
  -d "<workspace><name>${WORKSPACE}</name></workspace>" \
  "${GEOSERVER_URL}/rest/workspaces" || true

echo "2) Creando store PostGIS '${STORE}'..."
curl -s -u "${GEOSERVER_USER}:${GEOSERVER_PASSWORD}" \
  -XPOST \
  -H "Content-type: text/xml" \
  -d "
<dataStore>
  <name>${STORE}</name>
  <connectionParameters>
    <entry key=\"dbtype\">postgis</entry>
    <entry key=\"host\">${DB_HOST}</entry>
    <entry key=\"port\">${DB_PORT}</entry>
    <entry key=\"database\">${DB_NAME}</entry>
    <entry key=\"schema\">public</entry>
    <entry key=\"user\">${DB_USER}</entry>
    <entry key=\"passwd\">${DB_PASSWORD}</entry>
  </connectionParameters>
</dataStore>" \
  "${GEOSERVER_URL}/rest/workspaces/${WORKSPACE}/datastores" || true

echo "3) Creando style '${STYLE_NAME}'..."
curl -s -u "${GEOSERVER_USER}:${GEOSERVER_PASSWORD}" \
  -XPOST \
  -H "Content-type: text/xml" \
  -d "<style><name>${STYLE_NAME}</name><filename>${STYLE_NAME}.sld</filename></style>" \
  "${GEOSERVER_URL}/rest/workspaces/${WORKSPACE}/styles" || true

echo "4) Subiendo contenido SLD..."
curl -s -u "${GEOSERVER_USER}:${GEOSERVER_PASSWORD}" \
  -XPUT \
  -H "Content-type: application/vnd.ogc.sld+xml" \
  --data-binary @"${STYLE_FILE}" \
  "${GEOSERVER_URL}/rest/workspaces/${WORKSPACE}/styles/${STYLE_NAME}"

echo "5) Publicando layers..."
for LAYER in "${LAYERS[@]}"; do
  echo "   - Publicando ${LAYER}"
  curl -s -u "${GEOSERVER_USER}:${GEOSERVER_PASSWORD}" \
    -XPOST \
    -H "Content-type: text/xml" \
    -d "<featureType><name>${LAYER}</name></featureType>" \
    "${GEOSERVER_URL}/rest/workspaces/${WORKSPACE}/datastores/${STORE}/featuretypes" || true

  echo "   - Asignando style ${STYLE_NAME} a ${LAYER}"
  curl -s -u "${GEOSERVER_USER}:${GEOSERVER_PASSWORD}" \
    -XPUT \
    -H "Content-type: text/xml" \
    -d "<layer><defaultStyle><name>${STYLE_NAME}</name><workspace>${WORKSPACE}</workspace></defaultStyle></layer>" \
    "${GEOSERVER_URL}/rest/layers/${WORKSPACE}:${LAYER}" || true
done

echo "GeoServer configurado correctamente."