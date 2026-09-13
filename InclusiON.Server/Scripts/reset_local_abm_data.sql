-- ============================================================================
-- Limpieza LOCAL para probar y recargar manualmente los ABM de catálogos.
-- Base permitida: inclusion_dev. Este archivo NO debe ejecutarse en producción.
--
-- El script es transaccional y termina con ROLLBACK intencional. Para aplicar
-- la limpieza, revisar el archivo, cambiar únicamente la última instrucción a
-- COMMIT y ejecutarlo explícitamente sobre la base local Docker.
-- ============================================================================

BEGIN;

DO $$
BEGIN
    IF current_database() <> 'inclusion_dev' THEN
        RAISE EXCEPTION 'ABORTADO: la base actual es %, se esperaba inclusion_dev', current_database();
    END IF;
END $$;

DO $$
BEGIN
    IF to_regclass('public."Users"') IS NULL
       OR to_regclass('public."Professionals"') IS NULL
       OR to_regclass('public."PersonsWithDisability"') IS NULL
       OR to_regclass('public."FamilyRepresentatives"') IS NULL THEN
        RAISE EXCEPTION 'ABORTADO: faltan tablas principales del modelo actual';
    END IF;
END $$;

-- El único usuario permitido es el Admin sembrado por DatabaseSeeder.
CREATE TEMP TABLE _reset_users ON COMMIT DROP AS
SELECT "Id" AS id
FROM "Users"
WHERE "Id" <> '00000000-0000-0000-0000-000000000001'::uuid;

DO $$
BEGIN
    IF (SELECT count(*) FROM "Users"
        WHERE "Id" = '00000000-0000-0000-0000-000000000001'::uuid) <> 1 THEN
        RAISE EXCEPTION 'ABORTADO: no se encontró exactamente el Admin esperado';
    END IF;
END $$;

-- 1) Borrar primero las hojas de las relaciones con personas, actividades y usuarios.
-- Esto incluye trabajos pendientes, configuraciones adaptativas y tokens: son
-- datos operativos, no catálogos, y no deben quedar apuntando a datos eliminados.
DELETE FROM "AdaptiveAdjustmentLogs";
DELETE FROM "ActivityResults";
DELETE FROM "ActivityResponses";
DELETE FROM "ActivitySessions";
DELETE FROM "ActivityAssignments";
DELETE FROM "AdaptiveEngineConfigs";
DELETE FROM "PersonRoadmapActivities";
DELETE FROM "PersonRoadmapAreas";
DELETE FROM "PersonRoadmaps";
DELETE FROM "ActivityContents";
DELETE FROM "ActivityEmbeddings";
DELETE FROM "Activities";

DELETE FROM "ProfessionalPersons";
DELETE FROM "ProfessionalInstitutions";
DELETE FROM "ProfessionalStatusHistories";
DELETE FROM "FamilyStatusHistories";
DELETE FROM "PersonRepresentativeHistories";
DELETE FROM "PersonRepresentatives";
DELETE FROM "PersonSkillProfiles";
DELETE FROM "PersonEmbeddings";
DELETE FROM "Diagnoses";
DELETE FROM "Reports";
DELETE FROM "CalendarEvents";
DELETE FROM "Invitations";
DELETE FROM "AccessAudits";
DELETE FROM "Messages";
DELETE FROM "BackgroundJobs";
DELETE FROM "AdminInstitutions";
DELETE FROM "TrustedDevices";

-- Las FK de estos perfiles son restrictivas; se eliminan después de sus hijos.
DELETE FROM "Classrooms";
DELETE FROM "Professionals";
DELETE FROM "FamilyRepresentatives";
DELETE FROM "PersonsWithDisability";

-- 2) Dependencias de usuarios. Se conserva el Admin y no se tocan roles ni
-- claims de roles: AspNetRoles/AspNetRoleClaims son infraestructura de Identity.
DELETE FROM "AspNetUserClaims" WHERE "UserId" IN (SELECT id FROM _reset_users);
DELETE FROM "AspNetUserLogins" WHERE "UserId" IN (SELECT id FROM _reset_users);
DELETE FROM "AspNetUserTokens" WHERE "UserId" IN (SELECT id FROM _reset_users);
DELETE FROM "AspNetUserRoles" WHERE "UserId" IN (SELECT id FROM _reset_users);
DELETE FROM "RefreshTokens" WHERE "UserId" IN (SELECT id FROM _reset_users);
DELETE FROM "PasswordResetTokens" WHERE "UserId" IN (SELECT id FROM _reset_users);
DELETE FROM "Users" WHERE "Id" IN (SELECT id FROM _reset_users);

-- 3) Catálogos ABM vaciables. No quedan actividades que mantengan referencias
-- a categorías, áreas o tipos de plantilla, por eso se eliminan todas sus filas.
-- SkillAreas y ActivityTemplateTypes se cargarán manualmente desde el ABM y ya no
-- son recreados automáticamente por DatabaseSeeder ni RoadmapInitializer.
DELETE FROM "Specialties";
DELETE FROM "DisabilityTypes";
DELETE FROM "AutonomyLevels";
DELETE FROM "ActivityCategories";
-- ActivityTemplateTypes depende de SkillAreas mediante FK RESTRICT.
DELETE FROM "ActivityTemplateTypes";
DELETE FROM "SkillAreas";
DELETE FROM "ReportTypes";
DELETE FROM "EducationalInstitutions";

-- LoginMethods se conserva deliberadamente: no hay ABM de alta/baja para este
-- catálogo y es necesario para que el login y el modelo de personas arranquen.
-- También se conservan ActivityAssignmentStatuses, BackgroundJobStatuses y
-- JobTypes: son estados/tipos técnicos requeridos por el backend, no ABM.

-- Validación posterior: el estado resultante debe ser Admin + infraestructura.
DO $$
BEGIN
    IF (SELECT count(*) FROM "Users") <> 1
       OR NOT EXISTS (SELECT 1 FROM "Users" WHERE "Id" = '00000000-0000-0000-0000-000000000001'::uuid)
       OR (SELECT count(*) FROM "Professionals") <> 0
       OR (SELECT count(*) FROM "PersonsWithDisability") <> 0
       OR (SELECT count(*) FROM "FamilyRepresentatives") <> 0
       OR (SELECT count(*) FROM "Classrooms") <> 0
       OR (SELECT count(*) FROM "ProfessionalPersons") <> 0
       OR (SELECT count(*) FROM "ActivityAssignments") <> 0
       OR (SELECT count(*) FROM "Reports") <> 0
       OR (SELECT count(*) FROM "Activities") <> 0
       OR (SELECT count(*) FROM "ActivityContents") <> 0
       OR (SELECT count(*) FROM "ActivityEmbeddings") <> 0
       OR (SELECT count(*) FROM "AdaptiveEngineConfigs") <> 0
       OR (SELECT count(*) FROM "BackgroundJobs") <> 0
       OR (SELECT count(*) FROM "AspNetUserTokens") <> 0 THEN
        RAISE EXCEPTION 'Validación fallida: quedaron datos de usuarios, actividades u operación';
    END IF;

    IF (SELECT count(*) FROM "Specialties") <> 0
       OR (SELECT count(*) FROM "DisabilityTypes") <> 0
       OR (SELECT count(*) FROM "AutonomyLevels") <> 0
       OR (SELECT count(*) FROM "ActivityCategories") <> 0
       OR (SELECT count(*) FROM "SkillAreas") <> 0
       OR (SELECT count(*) FROM "ActivityTemplateTypes") <> 0
       OR (SELECT count(*) FROM "ReportTypes") <> 0
       OR (SELECT count(*) FROM "EducationalInstitutions") <> 0 THEN
        RAISE EXCEPTION 'Validación fallida: quedaron filas en catálogos ABM vaciables';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "AspNetRoles")
       OR NOT EXISTS (SELECT 1 FROM "AspNetRoleClaims") THEN
        RAISE EXCEPTION 'Validación fallida: faltan roles o claims de Identity';
    END IF;
END $$;

-- IMPORTANTE: modo seguro por defecto. Para aplicar, reemplazar ROLLBACK por COMMIT
-- después de revisar el plan y contar con un backup de la base local.
COMMIT;
