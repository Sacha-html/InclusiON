-- ============================================================================
-- Corrección de Caracteres Especiales / Acentos (UTF-8)
-- Corrige los nombres y apellidos que sufrieron encoding artifacts (??)
-- sin alterar ningún ID, clave foránea, usuario ni relación de la base.
-- ============================================================================

BEGIN;

-- 1. Corrección en PersonsWithDisability (Alumnos)
UPDATE "PersonsWithDisability" SET "FirstName" = REPLACE("FirstName", '??lvarez', 'Álvarez') WHERE "FirstName" LIKE '%??%';
UPDATE "PersonsWithDisability" SET "FirstName" = REPLACE("FirstName", 'Joaqu??n', 'Joaquín') WHERE "FirstName" LIKE '%??%';
UPDATE "PersonsWithDisability" SET "FirstName" = REPLACE("FirstName", 'M??a', 'Mía') WHERE "FirstName" LIKE '%??%';
UPDATE "PersonsWithDisability" SET "FirstName" = REPLACE("FirstName", 'Sof??a', 'Sofía') WHERE "FirstName" LIKE '%??%';
UPDATE "PersonsWithDisability" SET "FirstName" = REPLACE("FirstName", 'Luc??a', 'Lucía') WHERE "FirstName" LIKE '%??%';
UPDATE "PersonsWithDisability" SET "FirstName" = REPLACE("FirstName", 'Benjam??n', 'Benjamín') WHERE "FirstName" LIKE '%??%';
UPDATE "PersonsWithDisability" SET "FirstName" = REPLACE("FirstName", 'Nicol??s', 'Nicolás') WHERE "FirstName" LIKE '%??%';
UPDATE "PersonsWithDisability" SET "FirstName" = REPLACE("FirstName", 'Tom??s', 'Tomás') WHERE "FirstName" LIKE '%??%';
UPDATE "PersonsWithDisability" SET "FirstName" = REPLACE("FirstName", 'Agust??n', 'Agustín') WHERE "FirstName" LIKE '%??%';
UPDATE "PersonsWithDisability" SET "FirstName" = REPLACE("FirstName", 'Sim??n', 'Simón') WHERE "FirstName" LIKE '%??%';

UPDATE "PersonsWithDisability" SET "LastName" = REPLACE("LastName", 'G??mez', 'Gómez') WHERE "LastName" LIKE '%??%';
UPDATE "PersonsWithDisability" SET "LastName" = REPLACE("LastName", 'Fern??ndez', 'Fernández') WHERE "LastName" LIKE '%??%';
UPDATE "PersonsWithDisability" SET "LastName" = REPLACE("LastName", 'D??az', 'Díaz') WHERE "LastName" LIKE '%??%';
UPDATE "PersonsWithDisability" SET "LastName" = REPLACE("LastName", '??lvarez', 'Álvarez') WHERE "LastName" LIKE '%??%';
UPDATE "PersonsWithDisability" SET "LastName" = REPLACE("LastName", 'Ben??tez', 'Benítez') WHERE "LastName" LIKE '%??%';
UPDATE "PersonsWithDisability" SET "LastName" = REPLACE("LastName", 'Acu??a', 'Acuña') WHERE "LastName" LIKE '%??%';
UPDATE "PersonsWithDisability" SET "LastName" = REPLACE("LastName", 'Dom??nguez', 'Domínguez') WHERE "LastName" LIKE '%??%';
UPDATE "PersonsWithDisability" SET "LastName" = REPLACE("LastName", 'R??os', 'Ríos') WHERE "LastName" LIKE '%??%';
UPDATE "PersonsWithDisability" SET "LastName" = REPLACE("LastName", 'Rold??n', 'Roldán') WHERE "LastName" LIKE '%??%';
UPDATE "PersonsWithDisability" SET "LastName" = REPLACE("LastName", 'Gim??nez', 'Giménez') WHERE "LastName" LIKE '%??%';

-- 2. Corrección en FamilyRepresentatives (Tutores)
UPDATE "FamilyRepresentatives" SET "FirstName" = REPLACE("FirstName", 'Luc??a', 'Lucía') WHERE "FirstName" LIKE '%??%';
UPDATE "FamilyRepresentatives" SET "FirstName" = REPLACE("FirstName", 'M??nica', 'Mónica') WHERE "FirstName" LIKE '%??%';
UPDATE "FamilyRepresentatives" SET "FirstName" = REPLACE("FirstName", 'H??ctor', 'Héctor') WHERE "FirstName" LIKE '%??%';
UPDATE "FamilyRepresentatives" SET "FirstName" = REPLACE("FirstName", 'Mart??n', 'Martín') WHERE "FirstName" LIKE '%??%';
UPDATE "FamilyRepresentatives" SET "FirstName" = REPLACE("FirstName", 'Ra??l', 'Raúl') WHERE "FirstName" LIKE '%??%';
UPDATE "FamilyRepresentatives" SET "FirstName" = REPLACE("FirstName", 'N??stor', 'Néstor') WHERE "FirstName" LIKE '%??%';

UPDATE "FamilyRepresentatives" SET "LastName" = REPLACE("LastName", 'G??mez', 'Gómez') WHERE "LastName" LIKE '%??%';
UPDATE "FamilyRepresentatives" SET "LastName" = REPLACE("LastName", 'Fern??ndez', 'Fernández') WHERE "LastName" LIKE '%??%';
UPDATE "FamilyRepresentatives" SET "LastName" = REPLACE("LastName", 'D??az', 'Díaz') WHERE "LastName" LIKE '%??%';
UPDATE "FamilyRepresentatives" SET "LastName" = REPLACE("LastName", '??lvarez', 'Álvarez') WHERE "LastName" LIKE '%??%';
UPDATE "FamilyRepresentatives" SET "LastName" = REPLACE("LastName", 'Ben??tez', 'Benítez') WHERE "LastName" LIKE '%??%';
UPDATE "FamilyRepresentatives" SET "LastName" = REPLACE("LastName", 'Acu??a', 'Acuña') WHERE "LastName" LIKE '%??%';
UPDATE "FamilyRepresentatives" SET "LastName" = REPLACE("LastName", 'Dom??nguez', 'Domínguez') WHERE "LastName" LIKE '%??%';
UPDATE "FamilyRepresentatives" SET "LastName" = REPLACE("LastName", 'R??os', 'Ríos') WHERE "LastName" LIKE '%??%';
UPDATE "FamilyRepresentatives" SET "LastName" = REPLACE("LastName", 'Rold??n', 'Roldán') WHERE "LastName" LIKE '%??%';
UPDATE "FamilyRepresentatives" SET "LastName" = REPLACE("LastName", 'Gim??nez', 'Giménez') WHERE "LastName" LIKE '%??%';

-- 3. Corrección en Users (Cuentas de Acceso)
UPDATE "Users" SET "Name" = REPLACE("Name", '??lvarez', 'Álvarez') WHERE "Name" LIKE '%??%';
UPDATE "Users" SET "Name" = REPLACE("Name", 'Joaqu??n', 'Joaquín') WHERE "Name" LIKE '%??%';
UPDATE "Users" SET "Name" = REPLACE("Name", 'M??a', 'Mía') WHERE "Name" LIKE '%??%';
UPDATE "Users" SET "Name" = REPLACE("Name", 'Sof??a', 'Sofía') WHERE "Name" LIKE '%??%';
UPDATE "Users" SET "Name" = REPLACE("Name", 'Luc??a', 'Lucía') WHERE "Name" LIKE '%??%';
UPDATE "Users" SET "Name" = REPLACE("Name", 'Benjam??n', 'Benjamín') WHERE "Name" LIKE '%??%';
UPDATE "Users" SET "Name" = REPLACE("Name", 'Nicol??s', 'Nicolás') WHERE "Name" LIKE '%??%';
UPDATE "Users" SET "Name" = REPLACE("Name", 'Tom??s', 'Tomás') WHERE "Name" LIKE '%??%';
UPDATE "Users" SET "Name" = REPLACE("Name", 'Agust??n', 'Agustín') WHERE "Name" LIKE '%??%';
UPDATE "Users" SET "Name" = REPLACE("Name", 'Sim??n', 'Simón') WHERE "Name" LIKE '%??%';
UPDATE "Users" SET "Name" = REPLACE("Name", 'M??nica', 'Mónica') WHERE "Name" LIKE '%??%';
UPDATE "Users" SET "Name" = REPLACE("Name", 'H??ctor', 'Héctor') WHERE "Name" LIKE '%??%';
UPDATE "Users" SET "Name" = REPLACE("Name", 'Mart??n', 'Martín') WHERE "Name" LIKE '%??%';
UPDATE "Users" SET "Name" = REPLACE("Name", 'Ra??l', 'Raúl') WHERE "Name" LIKE '%??%';
UPDATE "Users" SET "Name" = REPLACE("Name", 'N??stor', 'Néstor') WHERE "Name" LIKE '%??%';

UPDATE "Users" SET "Surname" = REPLACE("Surname", 'G??mez', 'Gómez') WHERE "Surname" LIKE '%??%';
UPDATE "Users" SET "Surname" = REPLACE("Surname", 'Fern??ndez', 'Fernández') WHERE "Surname" LIKE '%??%';
UPDATE "Users" SET "Surname" = REPLACE("Surname", 'D??az', 'Díaz') WHERE "Surname" LIKE '%??%';
UPDATE "Users" SET "Surname" = REPLACE("Surname", '??lvarez', 'Álvarez') WHERE "Surname" LIKE '%??%';
UPDATE "Users" SET "Surname" = REPLACE("Surname", 'Ben??tez', 'Benítez') WHERE "Surname" LIKE '%??%';
UPDATE "Users" SET "Surname" = REPLACE("Surname", 'Acu??a', 'Acuña') WHERE "Surname" LIKE '%??%';
UPDATE "Users" SET "Surname" = REPLACE("Surname", 'Dom??nguez', 'Domínguez') WHERE "Surname" LIKE '%??%';
UPDATE "Users" SET "Surname" = REPLACE("Surname", 'R??os', 'Ríos') WHERE "Surname" LIKE '%??%';
UPDATE "Users" SET "Surname" = REPLACE("Surname", 'Rold??n', 'Roldán') WHERE "Surname" LIKE '%??%';
UPDATE "Users" SET "Surname" = REPLACE("Surname", 'Gim??nez', 'Giménez') WHERE "Surname" LIKE '%??%';

COMMIT;
