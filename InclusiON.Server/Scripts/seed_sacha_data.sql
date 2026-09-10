-- Script para poblar datos de prueba en el perfil de Sacha Del Barrio en Docker Local
DO $$
DECLARE
    v_sacha_prof_id UUID := '00000000-0000-0000-0000-000000000203';
    v_sacha_user_id UUID := '00000000-0000-0000-0000-000000000023';
    v_aula_manana_id UUID := '17a9dcfd-c19a-44c9-a99d-c61cedd364d8';
    v_aula_tarde_id UUID := '08914083-17eb-4776-a2cf-6dc7172f0ca8';
    v_student_rec RECORD;
    v_act_id INT;
BEGIN
    -- 1. Asignar alumnos a Sacha y sus aulas
    -- Alumnos para Aula Mañana
    FOR v_student_rec IN 
        SELECT "Id" FROM "PersonsWithDisability" WHERE "FirstName" IN ('Tomás', 'Sofía', 'Mateo', 'Juan')
    LOOP
        INSERT INTO "ProfessionalPersons" ("ProfessionalId", "PersonId", "AssignedAt", "IsPrimaryProfessional", "CanSuperviseLogin", "IsActive", "ClassroomId")
        VALUES (v_sacha_prof_id, v_student_rec."Id", NOW(), true, true, true, v_aula_manana_id)
        ON CONFLICT ("ProfessionalId", "PersonId") 
        DO UPDATE SET "ClassroomId" = v_aula_manana_id, "IsActive" = true, "IsPrimaryProfessional" = true;
    END LOOP;

    -- Alumnos para Aula Tarde
    FOR v_student_rec IN 
        SELECT "Id" FROM "PersonsWithDisability" WHERE "FirstName" IN ('Valentina', 'Benjamín', 'Maria', 'Carlos')
    LOOP
        INSERT INTO "ProfessionalPersons" ("ProfessionalId", "PersonId", "AssignedAt", "IsPrimaryProfessional", "CanSuperviseLogin", "IsActive", "ClassroomId")
        VALUES (v_sacha_prof_id, v_student_rec."Id", NOW(), true, true, true, v_aula_tarde_id)
        ON CONFLICT ("ProfessionalId", "PersonId") 
        DO UPDATE SET "ClassroomId" = v_aula_tarde_id, "IsActive" = true, "IsPrimaryProfessional" = true;
    END LOOP;

    -- 2. Crear actividades propias de Sacha si no existen
    IF NOT EXISTS (SELECT 1 FROM "Activities" WHERE "Title" = 'Identificación de Emociones Básicas' AND "ProfessionalId" = v_sacha_prof_id) THEN
        INSERT INTO "Activities" ("ProfessionalId", "CategoryId", "Title", "Description", "Instructions", "HasVisualSupport", "HasAudioSupport", "UsesEasyReading", "UsesPictograms", "SkillAreaId", "EstimatedDurationMinutes", "ComplexityLevel", "RequiresSupervision", "IsStandardActivity", "CreatedAt", "CreatedBy", "IsActive", "IsTemplate")
        VALUES (v_sacha_prof_id, 1, 'Identificación de Emociones Básicas', 'Actividad visual interactiva para reconocer alegría, tristeza, enojo y calma.', 'Observa los pictogramas y selecciona la emoción correspondiente a la situación planteada.', true, true, true, true, 1, 15, 1, false, false, NOW(), v_sacha_user_id, true, false);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "Activities" WHERE "Title" = 'Secuencia Temporal: Rutina Matutina' AND "ProfessionalId" = v_sacha_prof_id) THEN
        INSERT INTO "Activities" ("ProfessionalId", "CategoryId", "Title", "Description", "Instructions", "HasVisualSupport", "HasAudioSupport", "UsesEasyReading", "UsesPictograms", "SkillAreaId", "EstimatedDurationMinutes", "ComplexityLevel", "RequiresSupervision", "IsStandardActivity", "CreatedAt", "CreatedBy", "IsActive", "IsTemplate")
        VALUES (v_sacha_prof_id, 2, 'Secuencia Temporal: Rutina Matutina', 'Ordenar los pasos para prepararse antes de ir a la escuela.', 'Arrastra las tarjetas en el orden correcto desde despertarse hasta salir.', true, false, true, true, 2, 20, 2, true, false, NOW(), v_sacha_user_id, true, false);
    END IF;

    -- 3. Asignar actividades a los alumnos de Sacha
    FOR v_student_rec IN 
        SELECT pp."PersonId" FROM "ProfessionalPersons" pp WHERE pp."ProfessionalId" = v_sacha_prof_id
    LOOP
        FOR v_act_id IN 
            SELECT "Id" FROM "Activities" WHERE "ProfessionalId" IN (v_sacha_prof_id, '00000000-0000-0000-0000-000000000200') LIMIT 3
        LOOP
            IF NOT EXISTS (
                SELECT 1 FROM "ActivityAssignments" 
                WHERE "PersonId" = v_student_rec."PersonId" AND "ActivityId" = v_act_id AND "AssignedByProfessionalId" = v_sacha_prof_id
            ) THEN
                INSERT INTO "ActivityAssignments" ("ActivityId", "PersonId", "AssignedByProfessionalId", "AssignedAt", "DueDate", "IsEvaluationActivity", "CreatedAt", "CreatedBy", "IsActive", "StatusId")
                VALUES (v_act_id, v_student_rec."PersonId", v_sacha_prof_id, NOW(), NOW() + INTERVAL '7 days', false, NOW(), v_sacha_user_id, true, 1);
            END IF;
        END LOOP;
    END LOOP;

    RAISE NOTICE '¡Datos de prueba para Sacha Del Barrio asignados exitosamente!';
END $$;
