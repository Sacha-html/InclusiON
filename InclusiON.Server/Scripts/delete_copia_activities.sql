-- =====================================================================
-- Script: delete_copia_activities.sql
-- Descripción: Elimina las actividades de prueba que contienen '(Copia)'
--              y sus registros dependientes en cascada.
-- =====================================================================

DELETE FROM "ActivityResponses"
WHERE "AssignmentId" IN (
    SELECT a."Id"
    FROM "ActivityAssignments" a
    INNER JOIN "Activities" act ON a."ActivityId" = act."Id"
    WHERE act."Title" ILIKE '%copia%'
);

DELETE FROM "ActivityAssignments"
WHERE "ActivityId" IN (
    SELECT "Id" FROM "Activities" WHERE "Title" ILIKE '%copia%'
);

DELETE FROM "ActivityContents"
WHERE "ActivityId" IN (
    SELECT "Id" FROM "Activities" WHERE "Title" ILIKE '%copia%'
);

DELETE FROM "ActivityEmbeddings"
WHERE "ActivityId" IN (
    SELECT "Id" FROM "Activities" WHERE "Title" ILIKE '%copia%'
);

DELETE FROM "Activities"
WHERE "Title" ILIKE '%copia%';
