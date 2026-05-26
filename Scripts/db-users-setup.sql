-- =============================================================================
-- InclusiON — Script de usuarios de base de datos PostgreSQL
-- IN-196: Generar script de usuarios de bbdd para cadenas de conexion
--
-- Ejecutar como superusuario (postgres) en cada ambiente.
-- =============================================================================


-- =============================================================================
-- DESARROLLO + TESTING (usuario compartido, bases separadas)
-- =============================================================================

-- Crear usuario de aplicación para desarrollo y testing local
DO $$
BEGIN
  IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'inclusion_dev_app') THEN
    CREATE ROLE inclusion_dev_app WITH LOGIN PASSWORD 'Inclusion_Dev_2025_!';
  END IF;
END$$;

-- Base de datos de desarrollo
-- CREATE DATABASE inclusion_dev OWNER postgres;

\c inclusion_dev
GRANT CONNECT ON DATABASE inclusion_dev TO inclusion_dev_app;
GRANT USAGE, CREATE ON SCHEMA public TO inclusion_dev_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO inclusion_dev_app;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO inclusion_dev_app;

ALTER DEFAULT PRIVILEGES IN SCHEMA public
  GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO inclusion_dev_app;
ALTER DEFAULT PRIVILEGES IN SCHEMA public
  GRANT USAGE, SELECT ON SEQUENCES TO inclusion_dev_app;

-- Base de datos de testing (Playwright E2E — separada para poder limpiar entre runs)
-- CREATE DATABASE inclusion_test OWNER postgres;

\c inclusion_test
GRANT CONNECT ON DATABASE inclusion_test TO inclusion_dev_app;
GRANT USAGE, CREATE ON SCHEMA public TO inclusion_dev_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO inclusion_dev_app;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO inclusion_dev_app;

ALTER DEFAULT PRIVILEGES IN SCHEMA public
  GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO inclusion_dev_app;
ALTER DEFAULT PRIVILEGES IN SCHEMA public
  GRANT USAGE, SELECT ON SEQUENCES TO inclusion_dev_app;


-- =============================================================================
-- UAT
-- =============================================================================

DO $$
BEGIN
  IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'inclusion_uat_app') THEN
    CREATE ROLE inclusion_uat_app WITH LOGIN PASSWORD 'REEMPLAZAR_UAT_PASSWORD';
  END IF;
END$$;

\c inclusion_uat
GRANT CONNECT ON DATABASE inclusion_uat TO inclusion_uat_app;
GRANT USAGE ON SCHEMA public TO inclusion_uat_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO inclusion_uat_app;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO inclusion_uat_app;

ALTER DEFAULT PRIVILEGES IN SCHEMA public
  GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO inclusion_uat_app;
ALTER DEFAULT PRIVILEGES IN SCHEMA public
  GRANT USAGE, SELECT ON SEQUENCES TO inclusion_uat_app;


-- =============================================================================
-- PRODUCCION
-- =============================================================================

DO $$
BEGIN
  IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'inclusion_prod_app') THEN
    CREATE ROLE inclusion_prod_app WITH LOGIN PASSWORD 'REEMPLAZAR_PROD_PASSWORD';
  END IF;
END$$;

\c inclusion_prod
GRANT CONNECT ON DATABASE inclusion_prod TO inclusion_prod_app;
GRANT USAGE ON SCHEMA public TO inclusion_prod_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO inclusion_prod_app;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO inclusion_prod_app;

ALTER DEFAULT PRIVILEGES IN SCHEMA public
  GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO inclusion_prod_app;
ALTER DEFAULT PRIVILEGES IN SCHEMA public
  GRANT USAGE, SELECT ON SEQUENCES TO inclusion_prod_app;


-- =============================================================================
-- USUARIO DE MIGRACIONES (solo CI/CD — NO usar en runtime)
-- Necesita CREATE TABLE para ejecutar EF Core migrations
-- =============================================================================

DO $$
BEGIN
  IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'inclusion_migrations') THEN
    CREATE ROLE inclusion_migrations WITH LOGIN PASSWORD 'REEMPLAZAR_MIGRATIONS_PASSWORD';
  END IF;
END$$;

-- Descomentar y ejecutar en la DB correspondiente:
-- GRANT CONNECT ON DATABASE inclusion_dev TO inclusion_migrations;
-- GRANT ALL PRIVILEGES ON SCHEMA public TO inclusion_migrations;
-- GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO inclusion_migrations;
-- GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO inclusion_migrations;
-- ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO inclusion_migrations;
-- ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO inclusion_migrations;


-- =============================================================================
-- CADENAS DE CONEXION RESULTANTES
-- =============================================================================

-- Development  → appsettings.Development.json
-- Host=localhost;Port=5432;Database=inclusion_dev;Username=inclusion_dev_app;Password=Inclusion_Dev_2025_!

-- Testing (Playwright E2E) → appsettings.Testing.json
-- Host=localhost;Port=5432;Database=inclusion_test;Username=inclusion_dev_app;Password=Inclusion_Dev_2025_!

-- UAT          → appsettings.UAT.json
-- Host=uat-server;Port=5432;Database=inclusion_uat;Username=inclusion_uat_app;Password=REEMPLAZAR_UAT_PASSWORD;SSL Mode=Require

-- Production   → appsettings.Production.json
-- Host=prod-server;Port=5432;Database=inclusion_prod;Username=inclusion_prod_app;Password=REEMPLAZAR_PROD_PASSWORD;SSL Mode=Require

-- Migrations (CI/CD)
-- Host=...;Port=5432;Database=inclusion_dev;Username=inclusion_migrations;Password=REEMPLAZAR_MIGRATIONS_PASSWORD
