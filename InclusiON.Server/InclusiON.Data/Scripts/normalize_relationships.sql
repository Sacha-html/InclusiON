-- =========================================================================================
-- Script de Migración y Normalización de Parentescos / Relaciones de Alumnos y Familias
-- Base de datos: inclusion_dev (PostgreSQL)
-- Objetivo: Asegurar consistencia entre registros existentes y las opciones del combo frontend
-- =========================================================================================

-- 1. Normalizar minúsculas ('madre' -> 'Madre')
UPDATE "FamilyRepresentatives" 
SET "Relationship" = 'Madre' 
WHERE "Relationship" = 'madre';

-- 2. Unificar 'Tutor Legal' / 'Tutor' / 'tutor' a 'Tutor/a' en todas las tablas relacionales
UPDATE "FamilyRepresentatives" 
SET "Relationship" = 'Tutor/a' 
WHERE "Relationship" IN ('Tutor Legal', 'Tutor', 'tutor');

UPDATE "PersonRepresentatives" 
SET "Relationship" = 'Tutor/a' 
WHERE "Relationship" IN ('Tutor Legal', 'Tutor', 'tutor');

UPDATE "PersonRepresentativeHistories" 
SET "Relationship" = 'Tutor/a' 
WHERE "Relationship" IN ('Tutor Legal', 'Tutor', 'tutor');

UPDATE "Invitations" 
SET "Relationship" = 'Tutor/a' 
WHERE "Relationship" IN ('Tutor Legal', 'Tutor', 'tutor');
