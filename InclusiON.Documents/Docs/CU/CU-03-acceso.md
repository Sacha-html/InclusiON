# Módulo 3 — Acceso al Sistema

---

## CU-11: Iniciar sesión estándar (email y contraseña)

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional / Admin / Familiar |
| **Actores secundarios** | Sistema |
| **HU de referencia** | HU-01 |
| **Prioridad** | Crítica |

**Precondiciones**
- El usuario existe en el sistema con `IsActive = true`.
- La cuenta no está bloqueada por rate limiting.

**Flujo principal**
1. El usuario accede a la pantalla de login e ingresa email y contraseña.
2. El sistema valida las credenciales contra el hash almacenado (BCrypt o Argon2id).
3. Si el hash es BCrypt, el sistema lo migra transparentemente a Argon2id tras la verificación exitosa.
4. El sistema emite un `AccessToken` (JWT) y un `RefreshToken`.
5. El sistema registra el acceso en `AccessAudit`.
6. El sistema redirige al portal correspondiente según el rol.

**Flujos alternativos**
- **2a. Credenciales inválidas:** El sistema incrementa el contador de intentos fallidos y muestra mensaje genérico (sin especificar si falló email o contraseña).
- **2b. Límite de intentos superado (10 intentos/minuto):** El sistema devuelve `429 Too Many Requests` con mensaje "Demasiados intentos. Esperá unos minutos antes de reintentar."
- **1a. `MustChangePassword = true`:** Tras autenticación exitosa, el sistema redirige obligatoriamente al formulario de cambio de contraseña.

**Postcondiciones**
- El usuario accede a su portal con token válido.
- El acceso queda registrado en el audit log.

---

## CU-12: Iniciar sesión por PIN

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Persona |
| **Actores secundarios** | Sistema |
| **HU de referencia** | HU-01 |
| **Prioridad** | Crítica |

**Precondiciones**
- La Persona existe en el sistema con método de login configurado como PIN.
- El PIN fue configurado por el Profesional durante el alta.

**Flujo principal**
1. El usuario (Persona o quien la asiste) accede a la pantalla de login de Persona.
2. Ingresa el identificador de la persona y el PIN de 4 dígitos.
3. El sistema valida el PIN contra el hash Argon2id almacenado.
4. El sistema emite tokens y redirige al portal de la Persona.

**Flujos alternativos**
- **3a. PIN incorrecto:** El sistema incrementa el contador. Tras 5 intentos en 5 minutos desde la misma IP devuelve `429`.
- **3b. Cuenta inactiva:** El sistema muestra mensaje de cuenta desactivada sin revelar el motivo.

**Postcondiciones**
- La Persona accede a su roadmap y actividades disponibles.

---

## CU-13: Iniciar sesión asistido

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Persona |
| **Actores secundarios** | Profesional (autoriza), Sistema |
| **HU de referencia** | HU-01 |
| **Prioridad** | Alta |

**Precondiciones**
- La Persona tiene método de login configurado como "asistido".
- El Profesional que autoriza tiene `CanSuperviseLogin = true` en la asignación con esa Persona.
- El Profesional está autenticado en su propio portal.

**Flujo principal**
1. El Profesional accede a la vista de login asistido desde el perfil de la Persona.
2. El Profesional confirma la identidad de la Persona y autoriza el acceso.
3. El sistema emite un token de sesión para la Persona.
4. El sistema abre el portal de la Persona en el dispositivo correspondiente.

**Flujos alternativos**
- **2a. Profesional sin permiso `CanSuperviseLogin`:** El sistema devuelve `404` al intentar la operación.

**Postcondiciones**
- La Persona accede a su portal sin necesitar credenciales propias.

---

## CU-14: Iniciar sesión familiar

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Familiar |
| **Actores secundarios** | Sistema |
| **HU de referencia** | HU-04 |
| **Prioridad** | Alta |

**Precondiciones**
- El Familiar completó el registro por invitación (CU-09).
- Tiene `PersonRepresentative.IsActive = true` para al menos una persona.

**Flujo principal**
1. El Familiar accede a la pantalla de login e ingresa email y contraseña.
2. El sistema valida credenciales y verifica que el rol sea `FamilyRepresentative`.
3. El sistema emite tokens y redirige al portal familiar.

**Flujos alternativos**
- Iguales a CU-11 (intentos fallidos, rate limiting, `MustChangePassword`).

**Postcondiciones**
- El Familiar accede solo a los datos de su persona vinculada.

---

## CU-15: Cambiar contraseña temporal en primer ingreso

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional / Familiar |
| **Actores secundarios** | Sistema |
| **HU de referencia** | HU-12 |
| **Prioridad** | Crítica |

**Precondiciones**
- El usuario se autenticó exitosamente con una contraseña temporal (`MustChangePassword = true`).

**Flujo principal**
1. El sistema redirige al formulario de cambio de contraseña.
2. El usuario ingresa la contraseña temporal actual y la nueva contraseña (dos veces).
3. El sistema valida: mínimo 8 caracteres, 1 mayúscula, 1 número, distinta a la anterior.
4. El sistema actualiza el hash, desactiva `MustChangePassword` y revoca el token temporal.
5. El sistema redirige al portal o al wizard de onboarding si aplica.

**Flujos alternativos**
- **3a. Nueva contraseña no cumple requisitos:** El sistema muestra errores inline.
- **3b. Nueva contraseña igual a la temporal:** El sistema exige que sea diferente.

**Postcondiciones**
- `MustChangePassword = false`. El usuario opera con su propia contraseña.

---

## CU-16: Configurar perfil de accesibilidad

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional (configura para Persona) / Profesional o Familiar (para sí mismo) |
| **Actores secundarios** | — |
| **HU de referencia** | HU-01 |
| **Prioridad** | Alta |

**Precondiciones**
- Usuario autenticado.
- Si es Profesional configurando para una Persona: debe tener asignación activa con esa persona.

**Flujo principal**
1. El Actor accede al panel de accesibilidad del perfil.
2. Configura las opciones disponibles: modo claro/oscuro, tamaño de fuente, alto contraste, movimiento reducido, síntesis de voz.
3. El sistema persiste la configuración en el perfil del usuario.
4. La interfaz aplica los cambios inmediatamente.

**Flujos alternativos**
- **Movimiento reducido del SO:** Si el sistema operativo tiene activada la preferencia de movimiento reducido, el sistema la respeta aunque el perfil no la tenga configurada explícitamente.

**Postcondiciones**
- El perfil de accesibilidad queda guardado y se aplica en cada sesión.
