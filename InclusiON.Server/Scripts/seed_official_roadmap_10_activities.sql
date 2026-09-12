-- =====================================================================
-- Script: seed_official_roadmap_10_activities.sql
-- Descripción: Elimina cualquier rastro de las 10 actividades de muestra
--              anteriores y establece definitivamente las 10 actividades
--              oficiales del Roadmap con sus contenidos completos.
-- =====================================================================

BEGIN;

-- 1. Asegurar la existencia del SkillArea 'Trayectoria'
INSERT INTO "SkillAreas" ("Name", "Description", "Icon", "Color", "DisplayOrder", "IsActive", "CreatedAt", "CreatedBy")
SELECT 'Trayectoria', 'Camino de aprendizaje estándar anti-frustración.', 'map', '#673AB7', 4, true, NOW(), '00000000-0000-0000-0000-000000000001'::uuid
WHERE NOT EXISTS (SELECT 1 FROM "SkillAreas" WHERE "Name" = 'Trayectoria');

-- Obtener el Id de Trayectoria
DO $$
DECLARE
    v_trayectoria_id integer;
    v_default_prof_id uuid;
BEGIN
    SELECT "Id" INTO v_trayectoria_id FROM "SkillAreas" WHERE "Name" = 'Trayectoria' LIMIT 1;
    SELECT "Id" INTO v_default_prof_id FROM "Professionals" LIMIT 1;
    IF v_default_prof_id IS NULL THEN
        v_default_prof_id := '00000000-0000-0000-0000-000000000200'::uuid;
    END IF;

    -- =========================================================================
    -- NIVEL 1: ¿Qué dice aquí? (GLOBAL_READING, Cat 1 - Lectoescritura)
    -- =========================================================================
    UPDATE "Activities" SET
        "Title" = '¿Qué dice aquí?',
        "Description" = 'Lectura global de palabras cotidianas asociadas a su pictograma.',
        "Instructions" = 'Lee la palabra y elige el dibujo correcto',
        "CategoryId" = 1,
        "SkillAreaId" = v_trayectoria_id,
        "ProfessionalId" = v_default_prof_id,
        "HasVisualSupport" = true,
        "HasAudioSupport" = true,
        "UsesEasyReading" = true,
        "UsesPictograms" = true,
        "RequiresSupervision" = false,
        "IsStandardActivity" = true,
        "IsTemplate" = true,
        "RoadmapOrder" = 1,
        "ComplexityLevel" = 1,
        "EstimatedDurationMinutes" = 2,
        "UpdatedAt" = NOW(),
        "UpdatedBy" = '00000000-0000-0000-0000-000000000001'::uuid
    WHERE "Id" = 1;

    UPDATE "ActivityContents" SET
        "TemplateTypeId" = 3, -- GLOBAL_READING
        "ContentJson" = '{"instruction":"Lee la palabra y elige el dibujo correcto","word":"GATO","items":[{"id":"gato","pictogramId":9214,"label":"Gato"},{"id":"perro","pictogramId":9217,"label":"Perro"},{"id":"pajaro","pictogramId":9174,"label":"Pájaro"}],"correctItemId":"gato"}',
        "UpdatedAt" = NOW(),
        "UpdatedBy" = '00000000-0000-0000-0000-000000000001'::uuid
    WHERE "ActivityId" = 1;

    -- =========================================================================
    -- NIVEL 2: ¿Cómo se siente el niño? (OPTION_SELECT, Cat 3 - Socioemocional)
    -- =========================================================================
    UPDATE "Activities" SET
        "Title" = '¿Cómo se siente el niño?',
        "Description" = 'Identificación de emociones básicas a partir de una situación visual.',
        "Instructions" = 'Mira el dibujo del niño llorando. ¿Qué emoción está sintiendo?',
        "CategoryId" = 3,
        "SkillAreaId" = v_trayectoria_id,
        "ProfessionalId" = v_default_prof_id,
        "HasVisualSupport" = true,
        "HasAudioSupport" = true,
        "UsesEasyReading" = true,
        "UsesPictograms" = true,
        "RequiresSupervision" = false,
        "IsStandardActivity" = true,
        "IsTemplate" = true,
        "RoadmapOrder" = 2,
        "ComplexityLevel" = 1,
        "EstimatedDurationMinutes" = 2,
        "UpdatedAt" = NOW(),
        "UpdatedBy" = '00000000-0000-0000-0000-000000000001'::uuid
    WHERE "Id" = 2;

    UPDATE "ActivityContents" SET
        "TemplateTypeId" = 2, -- OPTION_SELECT
        "ContentJson" = '{"instruction":"Mira el dibujo del niño llorando. ¿Qué emoción está sintiendo?","question":"¿Qué emoción está sintiendo el niño?","questionPictogramId":24200,"options":[{"id":"triste","text":"Tristeza","pictogramId":24200},{"id":"alegre","text":"Alegría","pictogramId":24194},{"id":"enojado","text":"Enojo","pictogramId":24198}],"correctOptionId":"triste"}',
        "UpdatedAt" = NOW(),
        "UpdatedBy" = '00000000-0000-0000-0000-000000000001'::uuid
    WHERE "ActivityId" = 2;

    -- =========================================================================
    -- NIVEL 3: Armando la palabra Sol (BUILD_WORD, Cat 1 - Lectoescritura)
    -- =========================================================================
    UPDATE "Activities" SET
        "Title" = 'Armando la palabra Sol',
        "Description" = 'Construcción guiada de palabras simples letra por letra.',
        "Instructions" = 'Completa las letras que faltan para formar la palabra SOL',
        "CategoryId" = 1,
        "SkillAreaId" = v_trayectoria_id,
        "ProfessionalId" = v_default_prof_id,
        "HasVisualSupport" = true,
        "HasAudioSupport" = true,
        "UsesEasyReading" = true,
        "UsesPictograms" = true,
        "RequiresSupervision" = false,
        "IsStandardActivity" = true,
        "IsTemplate" = true,
        "RoadmapOrder" = 3,
        "ComplexityLevel" = 1,
        "EstimatedDurationMinutes" = 2,
        "UpdatedAt" = NOW(),
        "UpdatedBy" = '00000000-0000-0000-0000-000000000001'::uuid
    WHERE "Id" = 3;

    UPDATE "ActivityContents" SET
        "TemplateTypeId" = 5, -- BUILD_WORD
        "ContentJson" = '{"instruction":"Completa las letras que faltan para formar la palabra SOL","word":"SOL","hiddenIndices":[0,1,2],"options":[["S","M","P"],["O","A","E"],["L","R","T"]]}',
        "UpdatedAt" = NOW(),
        "UpdatedBy" = '00000000-0000-0000-0000-000000000001'::uuid
    WHERE "ActivityId" = 3;

    -- =========================================================================
    -- NIVEL 4: Contando las manzanas (NUMERATION, Cat 2 - Numeración)
    -- =========================================================================
    UPDATE "Activities" SET
        "Title" = 'Contando las manzanas',
        "Description" = 'Conteo visual y numeración sumando elementos ilustrados.',
        "Instructions" = '¿Cuántas manzanas hay en total?',
        "CategoryId" = 2,
        "SkillAreaId" = v_trayectoria_id,
        "ProfessionalId" = v_default_prof_id,
        "HasVisualSupport" = true,
        "HasAudioSupport" = true,
        "UsesEasyReading" = true,
        "UsesPictograms" = true,
        "RequiresSupervision" = false,
        "IsStandardActivity" = true,
        "IsTemplate" = true,
        "RoadmapOrder" = 4,
        "ComplexityLevel" = 1,
        "EstimatedDurationMinutes" = 2,
        "UpdatedAt" = NOW(),
        "UpdatedBy" = '00000000-0000-0000-0000-000000000001'::uuid
    WHERE "Id" = 4;

    UPDATE "ActivityContents" SET
        "TemplateTypeId" = 8, -- NUMERATION
        "ContentJson" = '{"instruction":"¿Cuántas manzanas hay en total?","operandA":3,"operandB":2,"pictogramId":2462,"options":[{"id":"opt1","value":4},{"id":"opt2","value":5},{"id":"opt3","value":6}]}',
        "UpdatedAt" = NOW(),
        "UpdatedBy" = '00000000-0000-0000-0000-000000000001'::uuid
    WHERE "ActivityId" = 4;

    -- =========================================================================
    -- NIVEL 5: Pasos para lavarse las manos (ORDER_SEQUENCE, Cat 7 - Autonomía)
    -- =========================================================================
    UPDATE "Activities" SET
        "Title" = 'Pasos para lavarse las manos',
        "Description" = 'Secuencia cronológica de hábitos de higiene personal paso a paso.',
        "Instructions" = 'Ordena los pasos correctos para lavarse las manos',
        "CategoryId" = 7,
        "SkillAreaId" = v_trayectoria_id,
        "ProfessionalId" = v_default_prof_id,
        "HasVisualSupport" = true,
        "HasAudioSupport" = true,
        "UsesEasyReading" = true,
        "UsesPictograms" = true,
        "RequiresSupervision" = false,
        "IsStandardActivity" = true,
        "IsTemplate" = true,
        "RoadmapOrder" = 5,
        "ComplexityLevel" = 1,
        "EstimatedDurationMinutes" = 2,
        "UpdatedAt" = NOW(),
        "UpdatedBy" = '00000000-0000-0000-0000-000000000001'::uuid
    WHERE "Id" = 5;

    UPDATE "ActivityContents" SET
        "TemplateTypeId" = 7, -- ORDER_SEQUENCE
        "ContentJson" = '{"instruction":"Ordena los pasos correctos para lavarse las manos","items":[{"id":"1","label":"Mojarse las manos","pictogramId":32442,"correctPosition":0},{"id":"2","label":"Ponerse jabón","pictogramId":32443,"correctPosition":1},{"id":"3","label":"Enjuagarse con agua","pictogramId":32444,"correctPosition":2},{"id":"4","label":"Secarse con toalla","pictogramId":32445,"correctPosition":3}]}',
        "UpdatedAt" = NOW(),
        "UpdatedBy" = '00000000-0000-0000-0000-000000000001'::uuid
    WHERE "ActivityId" = 5;

    -- =========================================================================
    -- NIVEL 6: Tengo hambre (PICTOGRAM_SELECT, Cat 4 - Comunicación)
    -- =========================================================================
    UPDATE "Activities" SET
        "Title" = 'Tengo hambre',
        "Description" = 'Selección de pictograma de necesidad básica de alimentación.',
        "Instructions" = 'Si tienes hambre, ¿qué pictograma debes presionar para comunicarte?',
        "CategoryId" = 4,
        "SkillAreaId" = v_trayectoria_id,
        "ProfessionalId" = v_default_prof_id,
        "HasVisualSupport" = true,
        "HasAudioSupport" = true,
        "UsesEasyReading" = true,
        "UsesPictograms" = true,
        "RequiresSupervision" = false,
        "IsStandardActivity" = true,
        "IsTemplate" = true,
        "RoadmapOrder" = 6,
        "ComplexityLevel" = 1,
        "EstimatedDurationMinutes" = 2,
        "UpdatedAt" = NOW(),
        "UpdatedBy" = '00000000-0000-0000-0000-000000000001'::uuid
    WHERE "Id" = 6;

    UPDATE "ActivityContents" SET
        "TemplateTypeId" = 1, -- PICTOGRAM_SELECT
        "ContentJson" = '{"instruction":"Si tienes hambre, ¿qué pictograma debes presionar para comunicarte?","correctItemId":"comer","items":[{"id":"comer","label":"Quiero comer","pictogramId":28667},{"id":"dormir","label":"Quiero dormir","pictogramId":32448},{"id":"jugar","label":"Quiero jugar","pictogramId":23392}]}',
        "UpdatedAt" = NOW(),
        "UpdatedBy" = '00000000-0000-0000-0000-000000000001'::uuid
    WHERE "ActivityId" = 6;

    -- =========================================================================
    -- NIVEL 7: Armando la palabra Casa (BUILD_WORD, Cat 1 - Lectoescritura)
    -- =========================================================================
    UPDATE "Activities" SET
        "Title" = 'Armando la palabra Casa',
        "Description" = 'Construcción guiada de la palabra CASA completando las vocales faltantes.',
        "Instructions" = 'Completa las letras que faltan para formar la palabra CASA',
        "CategoryId" = 1,
        "SkillAreaId" = v_trayectoria_id,
        "ProfessionalId" = v_default_prof_id,
        "HasVisualSupport" = true,
        "HasAudioSupport" = true,
        "UsesEasyReading" = true,
        "UsesPictograms" = true,
        "RequiresSupervision" = false,
        "IsStandardActivity" = true,
        "IsTemplate" = true,
        "RoadmapOrder" = 7,
        "ComplexityLevel" = 1,
        "EstimatedDurationMinutes" = 2,
        "UpdatedAt" = NOW(),
        "UpdatedBy" = '00000000-0000-0000-0000-000000000001'::uuid
    WHERE "Id" = 7;

    UPDATE "ActivityContents" SET
        "TemplateTypeId" = 5, -- BUILD_WORD
        "ContentJson" = '{"instruction":"Completa las letras que faltan para formar la palabra CASA","word":"CASA","hiddenIndices":[1,3],"options":[["A","E","O"],["A","I","U"]]}',
        "UpdatedAt" = NOW(),
        "UpdatedBy" = '00000000-0000-0000-0000-000000000001'::uuid
    WHERE "ActivityId" = 7;

    -- =========================================================================
    -- NIVEL 8: ¿Para qué sirve? (OPTION_SELECT, Cat 8 - Estimulación Cognitiva)
    -- =========================================================================
    UPDATE "Activities" SET
        "Title" = '¿Para qué sirve?',
        "Description" = 'Asociación funcional de objetos de la vida cotidiana.',
        "Instructions" = '¿Para qué sirve una cuchara?',
        "CategoryId" = 8,
        "SkillAreaId" = v_trayectoria_id,
        "ProfessionalId" = v_default_prof_id,
        "HasVisualSupport" = true,
        "HasAudioSupport" = true,
        "UsesEasyReading" = true,
        "UsesPictograms" = true,
        "RequiresSupervision" = false,
        "IsStandardActivity" = true,
        "IsTemplate" = true,
        "RoadmapOrder" = 8,
        "ComplexityLevel" = 1,
        "EstimatedDurationMinutes" = 2,
        "UpdatedAt" = NOW(),
        "UpdatedBy" = '00000000-0000-0000-0000-000000000001'::uuid
    WHERE "Id" = 8;

    UPDATE "ActivityContents" SET
        "TemplateTypeId" = 2, -- OPTION_SELECT
        "ContentJson" = '{"instruction":"¿Para qué sirve una cuchara?","question":"¿Para qué usamos la cuchara?","questionPictogramId":2452,"options":[{"id":"comer","text":"Para comer sopa o yogur","pictogramId":28667},{"id":"dibujar","text":"Para pintar en la hoja","pictogramId":2600},{"id":"dormir","text":"Para acostarse en la cama","pictogramId":32448}],"correctOptionId":"comer"}',
        "UpdatedAt" = NOW(),
        "UpdatedBy" = '00000000-0000-0000-0000-000000000001'::uuid
    WHERE "ActivityId" = 8;

    -- =========================================================================
    -- NIVEL 9: Contando los lápices (NUMERATION, Cat 2 - Numeración)
    -- =========================================================================
    UPDATE "Activities" SET
        "Title" = 'Contando los lápices',
        "Description" = 'Conteo y suma visual de útiles escolares.',
        "Instructions" = '¿Cuántos lápices hay en la cartuchera?',
        "CategoryId" = 2,
        "SkillAreaId" = v_trayectoria_id,
        "ProfessionalId" = v_default_prof_id,
        "HasVisualSupport" = true,
        "HasAudioSupport" = true,
        "UsesEasyReading" = true,
        "UsesPictograms" = true,
        "RequiresSupervision" = false,
        "IsStandardActivity" = true,
        "IsTemplate" = true,
        "RoadmapOrder" = 9,
        "ComplexityLevel" = 1,
        "EstimatedDurationMinutes" = 2,
        "UpdatedAt" = NOW(),
        "UpdatedBy" = '00000000-0000-0000-0000-000000000001'::uuid
    WHERE "Id" = 9;

    UPDATE "ActivityContents" SET
        "TemplateTypeId" = 8, -- NUMERATION
        "ContentJson" = '{"instruction":"¿Cuántos lápices hay en la cartuchera?","operandA":4,"operandB":2,"pictogramId":3028,"options":[{"id":"opt1","value":5},{"id":"opt2","value":6},{"id":"opt3","value":7}]}',
        "UpdatedAt" = NOW(),
        "UpdatedBy" = '00000000-0000-0000-0000-000000000001'::uuid
    WHERE "ActivityId" = 9;

    -- =========================================================================
    -- NIVEL 10: Ropa de verano e invierno (CLASSIFY, Cat 7 - Autonomía)
    -- =========================================================================
    UPDATE "Activities" SET
        "Title" = 'Ropa de verano e invierno',
        "Description" = 'Clasificación y emparejamiento de prendas de vestir según la estación climática.',
        "Instructions" = 'Empareja cada prenda con la estación del año correspondiente',
        "CategoryId" = 7,
        "SkillAreaId" = v_trayectoria_id,
        "ProfessionalId" = v_default_prof_id,
        "HasVisualSupport" = true,
        "HasAudioSupport" = true,
        "UsesEasyReading" = true,
        "UsesPictograms" = true,
        "RequiresSupervision" = false,
        "IsStandardActivity" = true,
        "IsTemplate" = true,
        "RoadmapOrder" = 10,
        "ComplexityLevel" = 1,
        "EstimatedDurationMinutes" = 2,
        "UpdatedAt" = NOW(),
        "UpdatedBy" = '00000000-0000-0000-0000-000000000001'::uuid
    WHERE "Id" = 10;

    UPDATE "ActivityContents" SET
        "TemplateTypeId" = 6, -- CLASSIFY
        "ContentJson" = '{"instruction":"Empareja cada prenda con la estación del año correspondiente","pairs":[{"id":"1","label":"Invierno (Gorro)","pictogramId":5992},{"id":"2","label":"Invierno (Bufanda)","pictogramId":5996},{"id":"3","label":"Verano (Malla)","pictogramId":6010},{"id":"4","label":"Verano (Gorra de sol)","pictogramId":6015}]}',
        "UpdatedAt" = NOW(),
        "UpdatedBy" = '00000000-0000-0000-0000-000000000001'::uuid
    WHERE "ActivityId" = 10;

END $$;

COMMIT;
