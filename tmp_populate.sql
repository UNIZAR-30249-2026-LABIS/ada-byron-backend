INSERT INTO espacios ("CodigoEspacio", "Nombre", "altura", "aforo", "TipoFisico", "CategoriaReserva")
SELECT
    id_espacio AS "CodigoEspacio",
    nombre AS "Nombre",
    CASE WHEN altura = 'S1' THEN -1 ELSE CAST(altura AS integer) END AS "altura",
    20 AS "aforo",
    CASE
        WHEN uso = 'AULA' THEN 'Aula'
        WHEN uso = 'LABORATORIO' THEN 'Laboratorio'
        WHEN uso = 'SALA INFORMÁTICA' THEN 'Laboratorio'
        WHEN uso = 'SEMINARIO' THEN 'Seminario'
        WHEN uso = 'SALA REUNIONES' THEN 'Seminario'
        WHEN uso = 'SALÓN DE ACTOS' THEN 'SalaComun'
        WHEN uso = 'BIBLIOTECA' THEN 'SalaComun'
        WHEN uso = 'DESPACHO' THEN 'Despacho'
        ELSE 'SalaComun'
    END AS "TipoFisico",
    CASE
        WHEN uso = 'AULA' THEN 'Aula'
        WHEN uso = 'LABORATORIO' THEN 'Laboratorio'
        WHEN uso = 'SALA INFORMÁTICA' THEN 'Laboratorio'
        WHEN uso = 'SEMINARIO' THEN 'Seminario'
        WHEN uso = 'SALA REUNIONES' THEN 'Seminario'
        WHEN uso = 'SALÓN DE ACTOS' THEN 'SalaComun'
        WHEN uso = 'BIBLIOTECA' THEN 'SalaComun'
        WHEN uso = 'DESPACHO' THEN 'Despacho'
        ELSE 'SalaComun'
    END AS "CategoriaReserva"
FROM (
    SELECT id_espacio, nombre, uso, altura FROM spaces_floor_s1_ada_byron_ui WHERE is_reservable_candidate = true
    UNION
    SELECT id_espacio, nombre, uso, altura FROM spaces_floor_0_ada_byron_ui WHERE is_reservable_candidate = true
    UNION
    SELECT id_espacio, nombre, uso, altura FROM spaces_floor_1_ada_byron_ui WHERE is_reservable_candidate = true
    UNION
    SELECT id_espacio, nombre, uso, altura FROM spaces_floor_2_ada_byron_ui WHERE is_reservable_candidate = true
    UNION
    SELECT id_espacio, nombre, uso, altura FROM spaces_floor_3_ada_byron_ui WHERE is_reservable_candidate = true
    UNION
    SELECT id_espacio, nombre, uso, altura FROM spaces_floor_4_ada_byron_ui WHERE is_reservable_candidate = true
    UNION
    SELECT id_espacio, nombre, uso, altura FROM spaces_floor_5_ada_byron_ui WHERE is_reservable_candidate = true
) t
WHERE NOT EXISTS (
    SELECT 1 FROM espacios e WHERE e."CodigoEspacio" = t.id_espacio
);
