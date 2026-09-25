-- Official standard roadmap seed.
-- Identity is StandardKey; this script never relies on activity, category, or
-- template-type IDs and never updates activities without an official key.
-- ROADMAP_STANDARD_03_QUIEN_SE_RIE keeps SOUND_RECOGNITION for player compatibility;
-- no real audio is played yet, so it currently evaluates the association between
-- laughter and emotion through visual options.

BEGIN;

DO $$
DECLARE
    v_skill_area_id integer;
    v_activity_id integer;
    v_template_type_id integer;
    v_definition record;
    v_definitions jsonb := $defs$
[
  {"seq":1,"standard_key":"ROADMAP_STANDARD_01_ARMANDO_EL_VASO","title":"Armando el vaso","description":"Rompecabezas básico de un vaso de agua.","category":"Motricidad y Coordinación","template":"PUZZLE","instructions":"Armá el vaso","content":{"instruction":"Armá el vaso","pictogramId":2610,"label":"Vaso de agua","rows":2,"cols":1,"showGhostGuide":true}},
  {"seq":2,"standard_key":"ROADMAP_STANDARD_02_NECESITO_IR_AL_BANO","title":"Necesito ir al baño","description":"Comunicación de una necesidad básica mediante pictogramas.","category":"Comunicación y Lenguaje","template":"PICTOGRAM_SELECT","instructions":"Elegí el pictograma del baño","content":{"instruction":"Elegí el pictograma del baño","correctItemId":"bano","items":[{"id":"bano","label":"Baño","pictogramId":6929},{"id":"pelota","label":"Pelota","pictogramId":3241}]}},
  {"seq":3,"standard_key":"ROADMAP_STANDARD_03_QUIEN_SE_RIE","title":"¿Quién se ríe?","description":"Discriminación auditiva de una emoción básica.","category":"Habilidades Socioemocionales","template":"SOUND_RECOGNITION","instructions":"Escuchá y elegí la cara alegre","content":{"instruction":"Escuchá el sonido y elegí la cara alegre","question":"¿Quién se ríe?","options":[{"id":"alegre","text":"La cara alegre","pictogramId":8582},{"id":"triste","text":"La cara triste","pictogramId":16371}],"correctOptionId":"alegre"}},
  {"seq":4,"standard_key":"ROADMAP_STANDARD_04_BUSCANDO_LA_MESA","title":"Buscando la MESA","description":"Lectura global de la palabra MESA.","category":"Lectoescritura","template":"GLOBAL_READING","instructions":"Leé MESA y elegí el pictograma correcto","content":{"instruction":"Leé MESA y elegí el pictograma correcto","word":"MESA","items":[{"id":"mesa","pictogramId":3129,"label":"Mesa"},{"id":"silla","pictogramId":3155,"label":"Silla"},{"id":"cama","pictogramId":25900,"label":"Cama"}],"correctItemId":"mesa"}},
  {"seq":5,"standard_key":"ROADMAP_STANDARD_05_CONTANDO_GALLETAS","title":"Contando galletas","description":"Conteo visual de tres galletas.","category":"Numeración y Matemática","template":"NUMERATION","instructions":"Contá las galletas y elegí el número correcto","content":{"instruction":"Contá las galletas y elegí el número correcto","operandA":3,"operandB":0,"pictogramId":8312,"options":[{"id":"dos","value":2},{"id":"tres","value":3},{"id":"cuatro","value":4}]}},
  {"seq":6,"standard_key":"ROADMAP_STANDARD_06_ME_SIRVO_AGUA","title":"Me sirvo agua","description":"Secuencia de acciones para servirse agua.","category":"Autonomía y Vida Diaria","template":"ORDER_SEQUENCE","instructions":"Ordená los pasos para servirte agua","content":{"instruction":"Ordená los pasos para servirte agua","items":[{"id":"1","label":"Agarrar el vaso","pictogramId":2610,"correctPosition":0},{"id":"2","label":"Abrir la canilla","pictogramId":11737,"correctPosition":1},{"id":"3","label":"Tomar agua","pictogramId":6061,"correctPosition":2}]}},
  {"seq":7,"standard_key":"ROADMAP_STANDARD_07_DONDE_HACEMOS_PIS","title":"¿Dónde hacemos pis?","description":"Comprensión del contexto de una necesidad básica.","category":"Habilidades Socioemocionales","template":"OPTION_SELECT","instructions":"Elegí dónde hacemos pis","content":{"instruction":"Si tenés ganas de hacer pis, ¿adónde vas?","question":"¿Dónde hacemos pis?","options":[{"id":"cama","text":"La cama","pictogramId":25900},{"id":"bano","text":"El baño","pictogramId":6929},{"id":"cocina","text":"La cocina","pictogramId":10752}],"correctOptionId":"bano"}},
  {"seq":8,"standard_key":"ROADMAP_STANDARD_08_ORDENAR_LA_COCINA","title":"A ordenar la cocina","description":"Clasificación de alimentos y utensilios.","category":"Numeración y Matemática","template":"CLASSIFY","instructions":"Uní cada imagen con su grupo","content":{"instruction":"Uní cada imagen con su grupo","pairs":[{"id":"galleta","label":"Alimentos: galleta","pictogramId":8312},{"id":"manzana","label":"Alimentos: manzana","pictogramId":2462},{"id":"plato","label":"Utensilios: plato","pictogramId":16857},{"id":"cuchara","label":"Utensilios: cuchara","pictogramId":2362}]}},
  {"seq":9,"standard_key":"ROADMAP_STANDARD_09_LETRAS_DE_MESA","title":"Las letras de MESA","description":"Conciencia fonológica para completar M _ S _.","category":"Lectoescritura","template":"BUILD_WORD","instructions":"Completá las letras de MESA","content":{"instruction":"Completá las letras de MESA","word":"MESA","hiddenIndices":[1,3],"options":[["E","A","O"],["A","E","I"]]}},
  {"seq":10,"standard_key":"ROADMAP_STANDARD_10_RUTINA_DE_LA_MANANA","title":"La rutina de la mañana","description":"Secuencia de cuatro acciones de la rutina matutina.","category":"Autonomía y Vida Diaria","template":"ORDER_SEQUENCE","instructions":"Ordená la rutina de la mañana","content":{"instruction":"Ordená la rutina de la mañana","items":[{"id":"1","label":"Despertarse","pictogramId":8989,"correctPosition":0},{"id":"2","label":"Ir al baño","pictogramId":6929,"correctPosition":1},{"id":"3","label":"Lavarse los dientes","pictogramId":6971,"correctPosition":2},{"id":"4","label":"Desayunar","pictogramId":28667,"correctPosition":3}]}}
]
$defs$::jsonb;
BEGIN
    SELECT "Id" INTO v_skill_area_id FROM "SkillAreas" WHERE "Name" = 'Trayectoria' LIMIT 1;
    IF v_skill_area_id IS NULL THEN
        RAISE EXCEPTION 'Skill area Trayectoria is required before seeding the standard roadmap';
    END IF;

    FOR v_definition IN
        SELECT * FROM jsonb_to_recordset(v_definitions) AS d(
            seq integer, standard_key text, title text, description text,
            category text, template text, instructions text, content jsonb)
    LOOP
        SELECT "Id" INTO v_template_type_id
        FROM "ActivityTemplateTypes"
        WHERE "Code" = v_definition.template;
        IF v_template_type_id IS NULL THEN
            RAISE EXCEPTION 'Template type % is required before seeding the standard roadmap', v_definition.template;
        END IF;

        IF NOT EXISTS (SELECT 1 FROM "ActivityCategories" WHERE "Name" = v_definition.category) THEN
            RAISE EXCEPTION 'Activity category % is required before seeding the standard roadmap', v_definition.category;
        END IF;

        INSERT INTO "Activities" (
            "StandardKey", "Title", "Description", "Instructions", "CategoryId", "SkillAreaId",
            "ProfessionalId", "HasVisualSupport", "HasAudioSupport", "UsesEasyReading", "UsesPictograms",
            "RequiresSupervision", "IsStandardActivity", "IsTemplate", "RoadmapOrder", "ComplexityLevel",
            "EstimatedDurationMinutes", "CreatedAt", "CreatedBy", "IsActive")
        VALUES (
            v_definition.standard_key, v_definition.title, v_definition.description, v_definition.instructions,
            (SELECT "Id" FROM "ActivityCategories" WHERE "Name" = v_definition.category), v_skill_area_id,
            NULL, true, true, true, true, false, true, true, v_definition.seq, 1, 2, NOW(),
            '00000000-0000-0000-0000-000000000001'::uuid, true)
        ON CONFLICT ("StandardKey") DO UPDATE SET
            "Title" = EXCLUDED."Title", "Description" = EXCLUDED."Description", "Instructions" = EXCLUDED."Instructions",
            "CategoryId" = EXCLUDED."CategoryId", "SkillAreaId" = EXCLUDED."SkillAreaId", "ProfessionalId" = NULL,
            "IsStandardActivity" = true, "IsTemplate" = true, "RoadmapOrder" = EXCLUDED."RoadmapOrder",
            "UpdatedAt" = NOW(), "UpdatedBy" = EXCLUDED."CreatedBy"
        RETURNING "Id" INTO v_activity_id;

        INSERT INTO "ActivityContents" ("ActivityId", "TemplateTypeId", "ContentJson", "CreatedAt", "CreatedBy", "IsActive")
        VALUES (v_activity_id, v_template_type_id, v_definition.content::text, NOW(), '00000000-0000-0000-0000-000000000001'::uuid, true)
        ON CONFLICT ("ActivityId") DO UPDATE SET
            "TemplateTypeId" = EXCLUDED."TemplateTypeId", "ContentJson" = EXCLUDED."ContentJson",
            "UpdatedAt" = NOW(), "UpdatedBy" = EXCLUDED."CreatedBy";
    END LOOP;
END $$;

COMMIT;
