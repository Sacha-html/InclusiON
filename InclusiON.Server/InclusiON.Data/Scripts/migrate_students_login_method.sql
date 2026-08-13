-- Script SQL de Migración para Alumnos (PersonsWithDisability)
-- Actualiza a todos los alumnos existentes que tengan configurado el login por email (LoginMethodId = 1 o NULL)
-- Estableciendo su LoginMethodId a 2 (PIN) y su PIN por defecto a '1234' (Hash BCrypt legacy/Argon2id compatible).

UPDATE "PersonsWithDisability"
SET "LoginMethodId" = 2,
    "PinCodeHash" = '$2a$12$761XF8Q01rB2g3q.l9vY9.xS107gE.n9B72Y.E43d04p/h1m54W6O' -- Hash de PIN '1234'
WHERE "LoginMethodId" = 1 OR "LoginMethodId" IS NULL;
