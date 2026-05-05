# Módulo 2 — Gestión de Usuarios

---

## CU-05: Auto-registrarse como profesional

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional (no autenticado) |
| **Actores secundarios** | Sistema (envío de notificación al admin) |
| **HU de referencia** | HU-IN-149 |
| **Prioridad** | Alta |

**Precondiciones**
- El formulario de registro está disponible públicamente en `/register-professional`.

**Flujo principal**
1. El Profesional accede a la página pública de registro.
2. Completa el formulario: nombre, apellido, email, especialidad, fecha de nacimiento (obligatorios); DNI, teléfono, matrícula, institución (opcionales).
3. El sistema valida en tiempo real que el email y la matrícula no estén en uso (debounce 800 ms).
4. El Profesional envía el formulario.
5. El sistema crea el `User` con `IsActive = false` y el `Professional` con `Status = Pending`.
6. Si seleccionó institución, el sistema crea `ProfessionalInstitution`.
7. El sistema muestra un modal de confirmación con botón "Aceptar" que redirige al login.

**Flujos alternativos**
- **3a. Email ya registrado:** El sistema muestra error inline y bloquea el envío.
- **3b. Matrícula ya registrada:** El sistema muestra error inline y bloquea el envío.
- **2a. Menor de 18 años:** El sistema muestra error en el campo de fecha de nacimiento.

**Postcondiciones**
- El profesional queda en estado `Pending`, sin acceso al sistema.
- El Admin puede ver la solicitud en el tab "Validaciones".

---

## CU-06: Validar solicitud de registro de profesional

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Admin Global / Admin Institucional |
| **Actores secundarios** | Sistema (envío de email), Profesional (receptor) |
| **HU de referencia** | HU-IN-150 |
| **Prioridad** | Alta |

**Precondiciones**
- Existe al menos una solicitud en estado `Pending`.
- Admin Institucional: solo ve solicitudes de su institución.
- Admin Global: ve todas las solicitudes.

**Flujo principal — Aprobar**
1. El Admin accede al tab "Validaciones" en la sección Profesionales.
2. Selecciona una solicitud pendiente y revisa los datos.
3. Selecciona "Aprobar".
4. El sistema activa el `User` (`IsActive = true`), genera contraseña temporal y activa `MustChangePassword`.
5. El sistema envía email al profesional con credenciales en segundo plano.
6. La solicitud desaparece del tab "Validaciones" y aparece en "Activos".

**Flujo alternativo — Rechazar**
3a. El Admin selecciona "Rechazar" e ingresa el motivo (obligatorio).
4a. El sistema desactiva la relación `ProfessionalInstitution` si existía.
5a. El sistema envía email al profesional con el motivo del rechazo.

**Postcondiciones (aprobación)**
- El Profesional puede iniciar sesión con las credenciales temporales.
- En el primer login es forzado a cambiar la contraseña.

---

## CU-07: Registrar persona con discapacidad

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Admin / Profesional |
| **Actores secundarios** | — |
| **HU de referencia** | HU-01 |
| **Prioridad** | Crítica |

**Precondiciones**
- Usuario autenticado con rol Admin o Profesional.

**Flujo principal**
1. El Actor accede a la sección Personas y selecciona "Nueva persona".
2. Completa los datos personales: nombre, apellido, fecha de nacimiento, tipo de discapacidad (del catálogo), nivel de autonomía (del catálogo).
3. Configura el método de login: PIN, visual estándar o asistido.
4. El sistema crea la `Person` y el `User` asociado.
5. El sistema muestra el perfil de la persona recién creada.

**Flujos alternativos**
- **3a. Login por PIN:** El sistema solicita definir un PIN de 4 dígitos que se hashea con Argon2id antes de persistir.

**Postcondiciones**
- La persona está registrada y disponible para ser asignada a profesionales.
- El perfil de habilidades queda vacío hasta que el Profesional lo configure.

---

## CU-08: Invitar familiar al sistema

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | Sistema (envío de email), Familiar (receptor) |
| **HU de referencia** | HU-04 |
| **Prioridad** | Alta |

**Precondiciones**
- El Profesional tiene al menos una persona asignada.
- El Profesional está autenticado.

**Flujo principal**
1. El Profesional accede al perfil de una persona asignada y selecciona "Invitar familiar".
2. Completa: email, nombre, apellido y relación del familiar (padre, madre, tutor, etc.).
3. El sistema verifica que no exista ya una invitación activa para ese email y persona.
4. El sistema genera un link único de registro con token y TTL de 7 días.
5. El sistema envía el email con el link al familiar.
6. La invitación queda en estado `Enviada` en el listado del Profesional.

**Flujos alternativos**
- **3a. Invitación activa ya existe:** El sistema informa al Profesional y ofrece reenviar.
- **3b. Email ya registrado como Familiar vinculado a esa persona:** El sistema informa que el familiar ya tiene acceso.

**Postcondiciones**
- La invitación existe en estado `Enviada`.
- El familiar recibe el email con el link de registro.

---

## CU-09: Completar registro por invitación

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Familiar (no autenticado) |
| **Actores secundarios** | Sistema |
| **HU de referencia** | HU-04 |
| **Prioridad** | Alta |

**Precondiciones**
- La invitación existe en estado `Enviada`.
- El link no ha expirado (menos de 7 días desde el envío).
- El link no fue usado previamente.

**Flujo principal**
1. El Familiar abre el link de invitación.
2. El sistema valida el token y muestra el formulario con datos pre-llenados en solo lectura: nombre, apellido, relación con la persona.
3. El Familiar elige una contraseña (mínimo 8 caracteres, 1 mayúscula, 1 número).
4. El Familiar confirma el registro.
5. El sistema crea el `User` activo, el perfil `FamilyRepresentative` y la relación `PersonRepresentative`.
6. La invitación pasa a estado `Aceptada`.
7. El sistema redirige al Familiar al login.

**Flujos alternativos**
- **1a. Link expirado:** El sistema muestra mensaje "Esta invitación ha expirado" y no permite el registro.
- **1b. Link ya usado:** El sistema muestra mensaje "Esta invitación ya fue utilizada".
- **3a. Contraseña no cumple requisitos:** El sistema muestra error inline.

**Postcondiciones**
- El Familiar puede iniciar sesión y acceder al portal con los datos de su persona vinculada.
- La invitación queda en estado `Aceptada` y no puede reutilizarse.

---

## CU-10: Gestionar cuentas de usuario

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Admin Global / Admin Institucional |
| **Actores secundarios** | Sistema |
| **HU de referencia** | HU-11 |
| **Prioridad** | Alta |

**Precondiciones**
- Usuario autenticado con rol Admin.
- Admin Institucional: solo gestiona usuarios de sus instituciones.

**Flujo principal — Resetear contraseña**
1. El Admin accede a la lista centralizada de usuarios y aplica filtros (rol, estado, institución).
2. Selecciona un usuario y elige "Resetear contraseña".
3. El sistema genera una contraseña temporal, activa `MustChangePassword` y revoca todas las sesiones activas del usuario.
4. La contraseña temporal se muestra una sola vez con botón de copiar.

**Flujo alternativo — Desactivar cuenta**
2a. El Admin selecciona "Desactivar cuenta".
3a. El sistema aplica soft-delete, revoca tokens activos y corta el acceso inmediatamente.
4a. El sistema registra la operación en `AccessAudit`.

**Flujo alternativo — Reactivar cuenta**
2b. El Admin selecciona "Reactivar cuenta".
3b. El sistema reactiva el usuario, genera contraseña temporal y activa `MustChangePassword`.

**Restricciones**
- El Admin no puede desactivar su propio usuario.
- Todas las operaciones quedan registradas en `AccessAudit`.

**Postcondiciones**
- El estado del usuario queda actualizado inmediatamente.
- El usuario afectado pierde o recupera el acceso según la operación realizada.
