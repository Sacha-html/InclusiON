# HU-IN-173 — Hardening de Seguridad de Datos Sensibles

| Campo | Contenido |
|---|---|
| ID | HU-IN-173 |
| Épica | Seguridad |
| Título | Hardening de Seguridad de Datos Sensibles |
| Prioridad | Alta |
| Estimación | 8 puntos de historia |
| Sprint asignado | Sprint 6 |
| Estado | Completada |

**Asignado a:** Mirko Ivo Wlk

---

## Historia de Usuario

**Como** responsable de la seguridad de la plataforma

**Quiero** que los datos de autenticación y los datos clínicos sensibles estén protegidos contra ataques de fuerza bruta, exposición en base de datos y uso de algoritmos obsoletos

**Para** cumplir con buenas prácticas de seguridad (OWASP), proteger la privacidad de las personas con discapacidad, y reducir el impacto de una eventual filtración de la base de datos.

---

## Criterios de Aceptación

| N° | El sistema debe… / Condición verificable |
|----|------------------------------------------|
| CA-01 | El endpoint `POST /api/auth/login/pin` devuelve `429 Too Many Requests` al superar 5 intentos en 5 minutos desde la misma IP. |
| CA-02 | Los endpoints `POST /api/auth/login`, `login/visual-standard`, `login/family`, `login/assisted` e `identify` devuelven `429` al superar 10 intentos por minuto desde la misma IP. |
| CA-03 | El endpoint `POST /api/auth/refresh` devuelve `429` al superar 20 intentos por minuto desde la misma IP. |
| CA-04 | La respuesta 429 incluye `{ "success": false, "message": "Demasiados intentos. Esperá unos minutos antes de reintentar." }`. |
| CA-05 | Los PINs nuevos se hashean con Argon2id (parámetros OWASP: 64 MB memoria, 3 iteraciones, paralelismo 1). El hash almacenado comienza con `$argon2id$`. |
| CA-06 | Al verificar un PIN almacenado con BCrypt (`$2b$` / `$2a$`), el sistema lo acepta y migra transparentemente a Argon2id en el mismo login exitoso (lazy migration). El hash viejo nunca se usa de nuevo. |
| CA-07 | Las propiedades marcadas con `[Encrypted]` en los modelos de dominio se cifran automáticamente al persistir y se descifran al leer, sin cambios en los handlers ni repositorios. |
| CA-08 | Los campos cifrados se almacenan en PostgreSQL con el prefijo `ENC:` seguido del payload AES-256-GCM en base64. Un valor sin ese prefijo se devuelve tal cual (fallback para datos pre-migración). |
| CA-09 | Los siguientes campos están cifrados: `Diagnosis` (7 campos clínicos), `Report` (5 campos narrativos), `ActivityResponse` (`ResponsePattern`, `Observations`), `ActivityResult` (`JsonResponse`). |
| CA-10 | Al arrancar la API, `SensitiveDataEncryptor` detecta y cifra cualquier registro con campos en texto plano. La operación es idempotente (re-ejecutar no corrompe datos ya cifrados). |
| CA-11 | El seeder usa `IPinHasher` (vía `PinHashAccessor`) para hashear PINs de datos de prueba, en lugar de llamar a BCrypt directamente. |
| CA-12 | Existen tests unitarios que cubren: roundtrip encrypt/decrypt, nonces aleatorios, fallback plaintext, claves inválidas, hash Argon2id, verify correcto/incorrecto, migración BCrypt. |

---

## Notas Técnicas

### Rate Limiting
- Implementado con `Microsoft.AspNetCore.RateLimiting` (nativo .NET 8+).
- Partición por `RemoteIpAddress` — detrás de reverse proxy considerar `X-Forwarded-For` en producción.
- Tres políticas: `auth-pin` (sliding window), `auth-login` (sliding window), `auth-refresh` (fixed window).

### Argon2id
- Librería: `Isopoh.Cryptography.Argon2` v2.0.0 (pure C#, sin dependencias nativas).
- Interfaz: `IPinHasher` con `Hash(pin)` y `Verify(storedHash, pin, out needsRehash)`.
- Migración lazy: `needsRehash = true` cuando el hash almacenado es BCrypt. El caller rehashea en background (fire-and-forget).

### Cifrado AES-256-GCM
- Clave: 32 bytes (256 bits) configurada en `EncryptionSettings:Key` (base64).
- Formato almacenado: `ENC:<base64(nonce[12] + ciphertext[N] + tag[16])>`.
- Detección automática de campos a cifrar: reflexión sobre `[Encrypted]` en `OnModelCreating`.
- Puente entre proyectos: `EncryptionAccessor` (static delegates) y `PinHashAccessor` — evitan referencias circulares entre `InclusiON.Data` e `InclusiON.Infrastructure`.

---

## Definition of Done

- [x] Rate limiting configurado y aplicado en todos los endpoints de auth
- [x] `IPinHasher` + `Argon2idPinHasher` implementados con soporte BCrypt legacy
- [x] Handlers `CreatePerson`, `UpdateLoginMethod`, `PinLogin` actualizados a `IPinHasher`
- [x] Seeder actualiza a `PinHashAccessor` (Argon2id)
- [x] `IEncryptionService` + `AesGcmEncryptionService` implementados
- [x] `[Encrypted]` attribute en `InclusiON.Domain`
- [x] `EncryptedStringConverter` + `EncryptionAccessor` en `InclusiON.Data`
- [x] `AppDbContext.OnModelCreating` aplica converter automáticamente por reflexión
- [x] Campos `[Encrypted]` anotados en `Diagnosis`, `Report`, `ActivityResponse`, `ActivityResult`
- [x] `SensitiveDataEncryptor` implementado e integrado en startup
- [x] 21 tests unitarios: `AesGcmEncryptionServiceTests` (7), `EncryptedStringConverterTests` (5), `Argon2idPinHasherTests` (9)
- [x] Build sin errores
- [ ] Code review
- [ ] QA manual (plan de pruebas en sesión 2026-04-18)
