-- ============================================================================
-- Carga Automatizada de 40 Alumnos y Tutores
-- 10 por aula distribuidos equitativamente entre Sofía Gutiérrez y Pedro Martínez:
--   - Sofía Gutiérrez -> Aula A (10 alumnos)
--   - Sofía Gutiérrez -> Aula B (10 alumnos)
--   - Pedro Martínez   -> Aula A (10 alumnos)
--   - Pedro Martínez   -> Aula B (10 alumnos)
--
-- Cumple con todas las reglas de negocio del sistema:
--   - Edades entre 12 y 40 años (nacidos entre 2009 y 2013).
--   - Parentescos del combo oficial ('Madre', 'Padre', 'Tutor/a', 'Abuelo/a').
--   - Documentos (DNI) y correos únicos sin duplicación.
--   - Integridad referencial completa (Users, AspNetUserRoles, FamilyRepresentatives,
--     PersonsWithDisability, PersonRepresentatives, ProfessionalPersons, PersonEmbeddings).
-- ============================================================================

BEGIN;

DO $$
DECLARE
    v_admin_user_id UUID := '00000000-0000-0000-0000-000000000001';

    v_sofia_id UUID := 'be6af429-f630-45d4-8bb8-2b55a39f3f80';
    v_sofia_aula_a UUID := '620f9bd9-6d58-47db-9ac5-c68fa9fba425';
    v_sofia_aula_b UUID := '9d58bcd7-eaad-4ecf-81ff-d4986cc6cdc8';

    v_pedro_id UUID := '7bf53b93-0979-4cca-8902-b6561882898d';
    v_pedro_aula_a UUID := '428b97ee-caf3-405e-b493-e26f60e5f9d4';
    v_pedro_aula_b UUID := '1d1fe65f-1dcc-41a8-9b2c-cbb028e4f92b';

    v_tutor_role_id UUID := '33333333-3333-3333-3333-333333333333';
    v_student_role_id UUID := '44444444-4444-4444-4444-444444444444';

    -- Hashes estándar de ASP.NET Identity para 'Student123!' y 'Tutor123!'
    v_student_pwd_hash TEXT := 'AQAAAAIAAYagAAAAEG2sz3MvQvnF9f7zQRvmJ1RcPjZkMhOf0SQUQxD3GsfH9sa8HaC8OcA1avMOnJDYTA==';
    v_tutor_pwd_hash TEXT := 'AQAAAAIAAYagAAAAECkq8gRB2gNVjHRzxcjGKZ5sXfSiVHfy/cHredgMBhPmWcfSNQ2AlxejDOjzUP8GGw==';

    v_rec RECORD;
    v_student_user_id UUID;
    v_tutor_user_id UUID;
    v_student_id UUID;
    v_tutor_id UUID;
    v_rep_link_id UUID;
BEGIN
    -- Tabla temporal con los 40 alumnos y sus respectivos tutores
    CREATE TEMP TABLE _students_data (
        idx INT,
        prof_id UUID,
        classroom_id UUID,
        s_fn TEXT,
        s_ln TEXT,
        s_dni TEXT,
        s_birthdate TIMESTAMPTZ,
        s_avatar TEXT,
        t_fn TEXT,
        t_ln TEXT,
        t_email TEXT,
        t_dni TEXT,
        t_phone TEXT,
        t_rel TEXT
    ) ON COMMIT DROP;

    -- =========================================================================
    -- BLOQUE 1: Sofía Gutiérrez — Aula A (10 Alumnos)
    -- =========================================================================
    INSERT INTO _students_data VALUES
    (1, v_sofia_id, v_sofia_aula_a, 'Mateo', 'Rossi', '52001001', '2012-05-15 00:00:00+00', '#2E5FA3', 'Lucía', 'Rossi', 'tutor.sofia.a1@test.com', '27001001', '+54 9 11 4511-2001', 'Madre'),
    (2, v_sofia_id, v_sofia_aula_a, 'Valentina', 'Gómez', '52001002', '2011-08-20 00:00:00+00', '#4CAF50', 'Carlos', 'Gómez', 'tutor.sofia.a2@test.com', '27001002', '+54 9 11 4511-2002', 'Padre'),
    (3, v_sofia_id, v_sofia_aula_a, 'Lucas', 'Fernández', '52001003', '2013-03-10 00:00:00+00', '#FF9800', 'Mariana', 'Fernández', 'tutor.sofia.a3@test.com', '27001003', '+54 9 11 4511-2003', 'Madre'),
    (4, v_sofia_id, v_sofia_aula_a, 'Camila', 'Díaz', '52001004', '2010-11-25 00:00:00+00', '#E91E63', 'Jorge', 'Díaz', 'tutor.sofia.a4@test.com', '27001004', '+54 9 11 4511-2004', 'Padre'),
    (5, v_sofia_id, v_sofia_aula_a, 'Santiago', 'Romero', '52001005', '2012-09-05 00:00:00+00', '#9C27B0', 'Elena', 'Romero', 'tutor.sofia.a5@test.com', '27001005', '+54 9 11 4511-2005', 'Tutor/a'),
    (6, v_sofia_id, v_sofia_aula_a, 'Martina', 'Sosa', '52001006', '2011-04-12 00:00:00+00', '#00BCD4', 'Roberto', 'Sosa', 'tutor.sofia.a6@test.com', '27001006', '+54 9 11 4511-2006', 'Padre'),
    (7, v_sofia_id, v_sofia_aula_a, 'Bautista', 'Torres', '52001007', '2013-07-18 00:00:00+00', '#3F51B5', 'Patricia', 'Torres', 'tutor.sofia.a7@test.com', '27001007', '+54 9 11 4511-2007', 'Madre'),
    (8, v_sofia_id, v_sofia_aula_a, 'Emma', 'Álvarez', '52001008', '2012-01-30 00:00:00+00', '#8BC34A', 'Diego', 'Álvarez', 'tutor.sofia.a8@test.com', '27001008', '+54 9 11 4511-2008', 'Padre'),
    (9, v_sofia_id, v_sofia_aula_a, 'Joaquín', 'Benítez', '52001009', '2010-06-22 00:00:00+00', '#795548', 'Claudia', 'Benítez', 'tutor.sofia.a9@test.com', '27001009', '+54 9 11 4511-2009', 'Madre'),
    (10, v_sofia_id, v_sofia_aula_a, 'Mía', 'Castro', '52001010', '2011-12-14 00:00:00+00', '#009688', 'Fernando', 'Castro', 'tutor.sofia.a10@test.com', '27001010', '+54 9 11 4511-2010', 'Padre');

    -- =========================================================================
    -- BLOQUE 2: Sofía Gutiérrez — Aula B (10 Alumnos)
    -- =========================================================================
    INSERT INTO _students_data VALUES
    (11, v_sofia_id, v_sofia_aula_b, 'Thiago', 'Morales', '52001011', '2012-03-08 00:00:00+00', '#FF5722', 'Silvia', 'Morales', 'tutor.sofia.b1@test.com', '27001011', '+54 9 11 4511-2011', 'Madre'),
    (12, v_sofia_id, v_sofia_aula_b, 'Sofía', 'Ortiz', '52001012', '2013-10-15 00:00:00+00', '#673AB7', 'Marcelo', 'Ortiz', 'tutor.sofia.b2@test.com', '27001012', '+54 9 11 4511-2012', 'Padre'),
    (13, v_sofia_id, v_sofia_aula_b, 'Facundo', 'Ruiz', '52001013', '2011-02-19 00:00:00+00', '#03A9F4', 'Andrea', 'Ruiz', 'tutor.sofia.b3@test.com', '27001013', '+54 9 11 4511-2013', 'Madre'),
    (14, v_sofia_id, v_sofia_aula_b, 'Lucía', 'Acuña', '52001014', '2010-08-11 00:00:00+00', '#F44336', 'Gustavo', 'Acuña', 'tutor.sofia.b4@test.com', '27001014', '+54 9 11 4511-2014', 'Padre'),
    (15, v_sofia_id, v_sofia_aula_b, 'Benjamín', 'Navarro', '52001015', '2012-07-24 00:00:00+00', '#4CAF50', 'Mónica', 'Navarro', 'tutor.sofia.b5@test.com', '27001015', '+54 9 11 4511-2015', 'Madre'),
    (16, v_sofia_id, v_sofia_aula_b, 'Julieta', 'Domínguez', '52001016', '2011-05-30 00:00:00+00', '#FFC107', 'Pablo', 'Domínguez', 'tutor.sofia.b6@test.com', '27001016', '+54 9 11 4511-2016', 'Padre'),
    (17, v_sofia_id, v_sofia_aula_b, 'Nicolás', 'Medina', '52001017', '2013-09-17 00:00:00+00', '#607D8B', 'Graciela', 'Medina', 'tutor.sofia.b7@test.com', '27001017', '+54 9 11 4511-2017', 'Abuelo/a'),
    (18, v_sofia_id, v_sofia_aula_b, 'Delfina', 'Flores', '52001018', '2012-11-03 00:00:00+00', '#E91E63', 'Sergio', 'Flores', 'tutor.sofia.b8@test.com', '27001018', '+54 9 11 4511-2018', 'Padre'),
    (19, v_sofia_id, v_sofia_aula_b, 'Tomás', 'Herrera', '52001019', '2011-01-28 00:00:00+00', '#2196F3', 'Valeria', 'Herrera', 'tutor.sofia.b9@test.com', '27001019', '+54 9 11 4511-2019', 'Madre'),
    (20, v_sofia_id, v_sofia_aula_b, 'Victoria', 'Vega', '52001020', '2010-04-16 00:00:00+00', '#9C27B0', 'Daniel', 'Vega', 'tutor.sofia.b10@test.com', '27001020', '+54 9 11 4511-2020', 'Padre');

    -- =========================================================================
    -- BLOQUE 3: Pedro Martínez — Aula A (10 Alumnos)
    -- =========================================================================
    INSERT INTO _students_data VALUES
    (21, v_pedro_id, v_pedro_aula_a, 'Ignacio', 'Ramos', '52001021', '2012-06-12 00:00:00+00', '#2E5FA3', 'Laura', 'Ramos', 'tutor.pedro.a1@test.com', '27001021', '+54 9 11 4522-2021', 'Madre'),
    (22, v_pedro_id, v_pedro_aula_a, 'Catalina', 'Cabrera', '52001022', '2011-09-04 00:00:00+00', '#4CAF50', 'Héctor', 'Cabrera', 'tutor.pedro.a2@test.com', '27001022', '+54 9 11 4522-2022', 'Padre'),
    (23, v_pedro_id, v_pedro_aula_a, 'Santino', 'Ríos', '52001023', '2013-02-17 00:00:00+00', '#FF9800', 'Cecilia', 'Ríos', 'tutor.pedro.a3@test.com', '27001023', '+54 9 11 4522-2023', 'Madre'),
    (24, v_pedro_id, v_pedro_aula_a, 'Zoe', 'Mendoza', '52001024', '2012-10-29 00:00:00+00', '#E91E63', 'Javier', 'Mendoza', 'tutor.pedro.a4@test.com', '27001024', '+54 9 11 4522-2024', 'Padre'),
    (25, v_pedro_id, v_pedro_aula_a, 'Felipe', 'Duarte', '52001025', '2010-07-08 00:00:00+00', '#9C27B0', 'Rosa', 'Duarte', 'tutor.pedro.a5@test.com', '27001025', '+54 9 11 4522-2025', 'Tutor/a'),
    (26, v_pedro_id, v_pedro_aula_a, 'Abril', 'Godoy', '52001026', '2013-05-21 00:00:00+00', '#00BCD4', 'Martín', 'Godoy', 'tutor.pedro.a6@test.com', '27001026', '+54 9 11 4522-2026', 'Padre'),
    (27, v_pedro_id, v_pedro_aula_a, 'Lautaro', 'Ponce', '52001027', '2011-11-14 00:00:00+00', '#3F51B5', 'Natalia', 'Ponce', 'tutor.pedro.a7@test.com', '27001027', '+54 9 11 4522-2027', 'Madre'),
    (28, v_pedro_id, v_pedro_aula_a, 'Pilar', 'Luna', '52001028', '2012-04-03 00:00:00+00', '#8BC34A', 'Claudio', 'Luna', 'tutor.pedro.a8@test.com', '27001028', '+54 9 11 4522-2028', 'Padre'),
    (29, v_pedro_id, v_pedro_aula_a, 'Agustín', 'Ferreyra', '52001029', '2011-08-19 00:00:00+00', '#795548', 'Viviana', 'Ferreyra', 'tutor.pedro.a9@test.com', '27001029', '+54 9 11 4522-2029', 'Madre'),
    (30, v_pedro_id, v_pedro_aula_a, 'Malena', 'Paz', '52001030', '2010-12-07 00:00:00+00', '#009688', 'Mario', 'Paz', 'tutor.pedro.a10@test.com', '27001030', '+54 9 11 4522-2030', 'Padre');

    -- =========================================================================
    -- BLOQUE 4: Pedro Martínez — Aula B (10 Alumnos)
    -- =========================================================================
    INSERT INTO _students_data VALUES
    (31, v_pedro_id, v_pedro_aula_b, 'Simón', 'Molina', '52001031', '2012-02-11 00:00:00+00', '#FF5722', 'Sandra', 'Molina', 'tutor.pedro.b1@test.com', '27001031', '+54 9 11 4522-2031', 'Madre'),
    (32, v_pedro_id, v_pedro_aula_b, 'Juana', 'Aguilar', '52001032', '2013-12-01 00:00:00+00', '#673AB7', 'Raúl', 'Aguilar', 'tutor.pedro.b2@test.com', '27001032', '+54 9 11 4522-2032', 'Padre'),
    (33, v_pedro_id, v_pedro_aula_b, 'Bruno', 'Quiroga', '52001033', '2011-06-25 00:00:00+00', '#03A9F4', 'Marcela', 'Quiroga', 'tutor.pedro.b3@test.com', '27001033', '+54 9 11 4522-2033', 'Madre'),
    (34, v_pedro_id, v_pedro_aula_b, 'Bianca', 'Roldán', '52001034', '2010-09-18 00:00:00+00', '#F44336', 'Gabriel', 'Roldán', 'tutor.pedro.b4@test.com', '27001034', '+54 9 11 4522-2034', 'Padre'),
    (35, v_pedro_id, v_pedro_aula_b, 'Gael', 'Serrano', '52001035', '2012-08-14 00:00:00+00', '#4CAF50', 'Alicia', 'Serrano', 'tutor.pedro.b5@test.com', '27001035', '+54 9 11 4522-2035', 'Abuelo/a'),
    (36, v_pedro_id, v_pedro_aula_b, 'Morena', 'Cardozo', '52001036', '2011-03-29 00:00:00+00', '#FFC107', 'Esteban', 'Cardozo', 'tutor.pedro.b6@test.com', '27001036', '+54 9 11 4522-2036', 'Padre'),
    (37, v_pedro_id, v_pedro_aula_b, 'Dante', 'Giménez', '52001037', '2013-01-09 00:00:00+00', '#607D8B', 'Fabiana', 'Giménez', 'tutor.pedro.b7@test.com', '27001037', '+54 9 11 4522-2037', 'Madre'),
    (38, v_pedro_id, v_pedro_aula_b, 'Alma', 'Villalba', '52001038', '2012-10-02 00:00:00+00', '#E91E63', 'Néstor', 'Villalba', 'tutor.pedro.b8@test.com', '27001038', '+54 9 11 4522-2038', 'Padre'),
    (39, v_pedro_id, v_pedro_aula_b, 'Ramiro', 'Bustos', '52001039', '2011-07-16 00:00:00+00', '#2196F3', 'Sonia', 'Bustos', 'tutor.pedro.b9@test.com', '27001039', '+54 9 11 4522-2039', 'Madre'),
    (40, v_pedro_id, v_pedro_aula_b, 'Paula', 'Carrizo', '52001040', '2010-05-27 00:00:00+00', '#9C27B0', 'Alberto', 'Carrizo', 'tutor.pedro.b10@test.com', '27001040', '+54 9 11 4522-2040', 'Padre');

    -- =========================================================================
    -- BUCLE DE CREACIÓN TRANSACCIONAL
    -- =========================================================================
    FOR v_rec IN SELECT * FROM _students_data ORDER BY idx LOOP
        v_tutor_user_id := gen_random_uuid();
        v_student_user_id := gen_random_uuid();
        v_student_id := gen_random_uuid();
        v_tutor_id := gen_random_uuid();
        v_rep_link_id := gen_random_uuid();

        -- 1. Usuario Tutor
        INSERT INTO "Users" (
            "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
            "Name", "Surname", "EmailConfirmed", "PhoneNumberConfirmed", "TwoFactorEnabled",
            "LockoutEnabled", "MustChangePassword", "AccessFailedCount", "IsActive",
            "CreatedAt", "SecurityStamp", "ConcurrencyStamp", "PasswordHash"
        ) VALUES (
            v_tutor_user_id, v_rec.t_email, UPPER(v_rec.t_email), v_rec.t_email, UPPER(v_rec.t_email),
            v_rec.t_fn, v_rec.t_ln, true, false, false,
            true, true, 0, true,
            NOW(), gen_random_uuid()::text, gen_random_uuid()::text, v_tutor_pwd_hash
        );

        -- Rol Tutor
        INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
        VALUES (v_tutor_user_id, v_tutor_role_id);

        -- Ficha Familiar
        INSERT INTO "FamilyRepresentatives" (
            "Id", "UserId", "FirstName", "LastName", "DocumentNumber",
            "Relationship", "Phone", "IsActive", "Status", "CreatedAt", "CreatedBy"
        ) VALUES (
            v_tutor_id, v_tutor_user_id, v_rec.t_fn, v_rec.t_ln, v_rec.t_dni,
            v_rec.t_rel, v_rec.t_phone, true, 0, NOW(), v_admin_user_id
        );

        -- 2. Usuario Alumno
        INSERT INTO "Users" (
            "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
            "Name", "Surname", "EmailConfirmed", "PhoneNumberConfirmed", "TwoFactorEnabled",
            "LockoutEnabled", "MustChangePassword", "AccessFailedCount", "IsActive",
            "CreatedAt", "SecurityStamp", "ConcurrencyStamp", "PasswordHash"
        ) VALUES (
            v_student_user_id,
            LOWER(v_rec.s_fn || '.' || v_rec.s_ln || v_rec.idx),
            UPPER(v_rec.s_fn || '.' || v_rec.s_ln || v_rec.idx),
            LOWER(v_rec.s_fn || '.' || v_rec.s_ln || v_rec.idx || '@inclusion.local'),
            UPPER(v_rec.s_fn || '.' || v_rec.s_ln || v_rec.idx || '@inclusion.local'),
            v_rec.s_fn, v_rec.s_ln, true, false, false,
            true, false, 0, true,
            NOW(), gen_random_uuid()::text, gen_random_uuid()::text, v_student_pwd_hash
        );

        -- Rol Alumno
        INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
        VALUES (v_student_user_id, v_student_role_id);

        -- Ficha Persona con Discapacidad
        INSERT INTO "PersonsWithDisability" (
            "Id", "UserId", "FirstName", "LastName", "DocumentNumber",
            "BirthDate", "SupervisorUserId", "AvatarColor", "IsActive", "CreatedAt", "CreatedBy"
        ) VALUES (
            v_student_id, v_student_user_id, v_rec.s_fn, v_rec.s_ln, v_rec.s_dni,
            v_rec.s_birthdate, v_tutor_user_id, v_rec.s_avatar, true, NOW(), v_admin_user_id
        );

        -- 3. Vinculación Alumno - Tutor (PersonRepresentative)
        INSERT INTO "PersonRepresentatives" (
            "Id", "PersonId", "RepresentativeId", "Relationship",
            "IsPrimary", "HasInformedConsent", "CanSuperviseLogin", "IsActive", "CreatedAt"
        ) VALUES (
            v_rep_link_id, v_student_id, v_tutor_id, v_rec.t_rel,
            true, false, false, true, NOW()
        );

        -- 4. Asignación al Docente y al Aula (ProfessionalPerson)
        INSERT INTO "ProfessionalPersons" (
            "ProfessionalId", "PersonId", "ClassroomId",
            "IsPrimaryProfessional", "CanSuperviseLogin", "IsActive", "AssignedAt"
        ) VALUES (
            v_rec.prof_id, v_student_id, v_rec.classroom_id,
            true, true, true, NOW()
        );

        -- 5. Vector de embedding (PersonEmbeddings)
        INSERT INTO "PersonEmbeddings" (
            "PersonId", "Model", "Dimensions", "IsActive", "CreatedAt", "CreatedBy"
        ) VALUES (
            v_student_id, 'paraphrase-multilingual-MiniLM-L12-v2', 384, true, NOW(), v_admin_user_id
        );

    END LOOP;

    RAISE NOTICE '¡40 alumnos y sus respectivos tutores y asignaciones de aula creados exitosamente!';
END $$;

COMMIT;
