CREATE OR REPLACE VIEW spaces_floor_s1_ada_byron_ui AS
SELECT
  *,
  CASE
    WHEN uso = 'AULA' THEN 'AULA'
    WHEN uso = 'LABORATORIO' THEN 'LABORATORIO'
    WHEN uso = 'SALA INFORMÁTICA' THEN 'SALA_INFORMATICA'
    WHEN uso = 'SALA REUNIONES' THEN 'SALA_REUNIONES'
    WHEN uso = 'SALÓN DE ACTOS' THEN 'SALON_ACTOS'
    WHEN uso = 'BIBLIOTECA' THEN 'BIBLIOTECA'
    WHEN uso = 'DESPACHO' THEN 'DESPACHO'
    ELSE 'CONTEXTO'
  END AS display_category,
  CASE
    WHEN uso IN (
      'AULA',
      'LABORATORIO',
      'SALA INFORMÁTICA',
      'SALA REUNIONES',
      'SALÓN DE ACTOS',
      'BIBLIOTECA',
      'DESPACHO'
    ) THEN TRUE
    ELSE FALSE
  END AS is_reservable_candidate
FROM spaces_floor_s1_ada_byron;

CREATE OR REPLACE VIEW spaces_floor_0_ada_byron_ui AS
SELECT
  *,
  CASE
    WHEN uso = 'AULA' THEN 'AULA'
    WHEN uso = 'LABORATORIO' THEN 'LABORATORIO'
    WHEN uso = 'SALA INFORMÁTICA' THEN 'SALA_INFORMATICA'
    WHEN uso = 'SALA REUNIONES' THEN 'SALA_REUNIONES'
    WHEN uso = 'SALÓN DE ACTOS' THEN 'SALON_ACTOS'
    WHEN uso = 'BIBLIOTECA' THEN 'BIBLIOTECA'
    WHEN uso = 'DESPACHO' THEN 'DESPACHO'
    ELSE 'CONTEXTO'
  END AS display_category,
  CASE
    WHEN uso IN (
      'AULA',
      'LABORATORIO',
      'SALA INFORMÁTICA',
      'SALA REUNIONES',
      'SALÓN DE ACTOS',
      'BIBLIOTECA',
      'DESPACHO'
    ) THEN TRUE
    ELSE FALSE
  END AS is_reservable_candidate
FROM spaces_floor_0_ada_byron;

CREATE OR REPLACE VIEW spaces_floor_1_ada_byron_ui AS
SELECT
  *,
  CASE
    WHEN uso = 'AULA' THEN 'AULA'
    WHEN uso = 'LABORATORIO' THEN 'LABORATORIO'
    WHEN uso = 'SALA INFORMÁTICA' THEN 'SALA_INFORMATICA'
    WHEN uso = 'SALA REUNIONES' THEN 'SALA_REUNIONES'
    WHEN uso = 'SALÓN DE ACTOS' THEN 'SALON_ACTOS'
    WHEN uso = 'BIBLIOTECA' THEN 'BIBLIOTECA'
    WHEN uso = 'DESPACHO' THEN 'DESPACHO'
    ELSE 'CONTEXTO'
  END AS display_category,
  CASE
    WHEN uso IN (
      'AULA',
      'LABORATORIO',
      'SALA INFORMÁTICA',
      'SALA REUNIONES',
      'SALÓN DE ACTOS',
      'BIBLIOTECA',
      'DESPACHO'
    ) THEN TRUE
    ELSE FALSE
  END AS is_reservable_candidate
FROM spaces_floor_1_ada_byron;

CREATE OR REPLACE VIEW spaces_floor_2_ada_byron_ui AS
SELECT
  *,
  CASE
    WHEN uso = 'AULA' THEN 'AULA'
    WHEN uso = 'LABORATORIO' THEN 'LABORATORIO'
    WHEN uso = 'SALA INFORMÁTICA' THEN 'SALA_INFORMATICA'
    WHEN uso = 'SALA REUNIONES' THEN 'SALA_REUNIONES'
    WHEN uso = 'SALÓN DE ACTOS' THEN 'SALON_ACTOS'
    WHEN uso = 'BIBLIOTECA' THEN 'BIBLIOTECA'
    WHEN uso = 'DESPACHO' THEN 'DESPACHO'
    ELSE 'CONTEXTO'
  END AS display_category,
  CASE
    WHEN uso IN (
      'AULA',
      'LABORATORIO',
      'SALA INFORMÁTICA',
      'SALA REUNIONES',
      'SALÓN DE ACTOS',
      'BIBLIOTECA',
      'DESPACHO'
    ) THEN TRUE
    ELSE FALSE
  END AS is_reservable_candidate
FROM spaces_floor_2_ada_byron;

CREATE OR REPLACE VIEW spaces_floor_3_ada_byron_ui AS
SELECT
  *,
  CASE
    WHEN uso = 'AULA' THEN 'AULA'
    WHEN uso = 'LABORATORIO' THEN 'LABORATORIO'
    WHEN uso = 'SALA INFORMÁTICA' THEN 'SALA_INFORMATICA'
    WHEN uso = 'SALA REUNIONES' THEN 'SALA_REUNIONES'
    WHEN uso = 'SALÓN DE ACTOS' THEN 'SALON_ACTOS'
    WHEN uso = 'BIBLIOTECA' THEN 'BIBLIOTECA'
    WHEN uso = 'DESPACHO' THEN 'DESPACHO'
    ELSE 'CONTEXTO'
  END AS display_category,
  CASE
    WHEN uso IN (
      'AULA',
      'LABORATORIO',
      'SALA INFORMÁTICA',
      'SALA REUNIONES',
      'SALÓN DE ACTOS',
      'BIBLIOTECA',
      'DESPACHO'
    ) THEN TRUE
    ELSE FALSE
  END AS is_reservable_candidate
FROM spaces_floor_3_ada_byron;

CREATE OR REPLACE VIEW spaces_floor_4_ada_byron_ui AS
SELECT
  *,
  CASE
    WHEN uso = 'AULA' THEN 'AULA'
    WHEN uso = 'LABORATORIO' THEN 'LABORATORIO'
    WHEN uso = 'SALA INFORMÁTICA' THEN 'SALA_INFORMATICA'
    WHEN uso = 'SALA REUNIONES' THEN 'SALA_REUNIONES'
    WHEN uso = 'SALÓN DE ACTOS' THEN 'SALON_ACTOS'
    WHEN uso = 'BIBLIOTECA' THEN 'BIBLIOTECA'
    WHEN uso = 'DESPACHO' THEN 'DESPACHO'
    ELSE 'CONTEXTO'
  END AS display_category,
  CASE
    WHEN uso IN (
      'AULA',
      'LABORATORIO',
      'SALA INFORMÁTICA',
      'SALA REUNIONES',
      'SALÓN DE ACTOS',
      'BIBLIOTECA',
      'DESPACHO'
    ) THEN TRUE
    ELSE FALSE
  END AS is_reservable_candidate
FROM spaces_floor_4_ada_byron;

CREATE OR REPLACE VIEW spaces_floor_5_ada_byron_ui AS
SELECT
  *,
  CASE
    WHEN uso = 'AULA' THEN 'AULA'
    WHEN uso = 'LABORATORIO' THEN 'LABORATORIO'
    WHEN uso = 'SALA INFORMÁTICA' THEN 'SALA_INFORMATICA'
    WHEN uso = 'SALA REUNIONES' THEN 'SALA_REUNIONES'
    WHEN uso = 'SALÓN DE ACTOS' THEN 'SALON_ACTOS'
    WHEN uso = 'BIBLIOTECA' THEN 'BIBLIOTECA'
    WHEN uso = 'DESPACHO' THEN 'DESPACHO'
    ELSE 'CONTEXTO'
  END AS display_category,
  CASE
    WHEN uso IN (
      'AULA',
      'LABORATORIO',
      'SALA INFORMÁTICA',
      'SALA REUNIONES',
      'SALÓN DE ACTOS',
      'BIBLIOTECA',
      'DESPACHO'
    ) THEN TRUE
    ELSE FALSE
  END AS is_reservable_candidate
FROM spaces_floor_5_ada_byron;