-- =====================================================================
-- Script: cleanup_templates.sql
-- Descripción: Limpia los registros de plantillas globales (IsTemplate = true)
--              de la tabla Activities y sus contenidos relacionados.
-- 
-- IMPORTANTE: Ejecutar manualmente en tu cliente de BD (DBeaver, pgAdmin)
--             una sola vez antes de probar la Biblioteca de Plantillas dinámica.
-- =====================================================================

-- Opción 1: Eliminar en cascada (si la BD tiene ON DELETE CASCADE configurado)
DELETE FROM "Activities" WHERE "IsTemplate" = true;

-- Opción 2: Eliminar en orden explícito (si no hay CASCADE)
-- DELETE FROM "ActivityContents"
-- WHERE "ActivityId" IN (SELECT "Id" FROM "Activities" WHERE "IsTemplate" = true);
-- DELETE FROM "Activities" WHERE "IsTemplate" = true;

-- Verificación (ejecutar después para confirmar)
SELECT COUNT(*) AS plantillas_restantes FROM "Activities" WHERE "IsTemplate" = true;
