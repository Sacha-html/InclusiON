# ABM — Representantes Familiares

**Actor:** Profesional (inicia el proceso con invitación); el Familiar completa su propio registro  
**Justificación:** Los familiares o tutores son actores clave: participan del proceso de la persona con discapacidad, pueden recibir reportes, enviar mensajes y supervisar el login asistido. El Profesional necesita incorporarlos al sistema de forma controlada (vía invitación) para garantizar que solo accedan personas vinculadas a la persona bajo su cargo.

**Entidades:** `Invitation`, `FamilyRepresentative`, `FamilyStatusHistory`, `PersonRepresentative`, `PersonRepresentativeHistory`, `User`

---

## Alta — Invitación a Familiar

**Actor:** Profesional

El proceso de alta de un familiar comienza con una invitación por email.

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Email del familiar | Texto (100) | Sí | Formato válido |
| Nombre del familiar | Texto (100) | No | — |
| Apellido del familiar | Texto (100) | No | — |
| Relación con la persona | Texto (50) | No | Madre, Padre, Tutor, etc. |
| Persona asociada | Referencia | No | Debe existir y estar activa; el profesional debe tener asignada a esa persona |

**Validaciones de integridad:**
- No puede existir una invitación activa (no expirada, no usada) para el mismo email + persona.

**Resultado:**
- Se crea `Invitation` con código único, `Usada = false` y fecha de expiración = 7 días.
- Se envía email con link de registro.

---

## Registro — Familiar (flujo post-invitación)

**Actor:** Representante Familiar (usando el link de invitación)

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Nombre | Texto (100) | Sí | No vacío |
| Apellido | Texto (100) | Sí | No vacío |
| DNI | Texto (20) | No | Único si se ingresa |
| Teléfono | Texto (20) | No | — |
| Contraseña | Texto | Sí | Mínimo 8 caracteres |

**Validaciones de integridad:**
- El código de invitación debe existir, no estar expirado y no haber sido ya usado.
- El email ya está fijado por la invitación; no se puede modificar en este paso.

**Resultado:**
- Se crea `User` y `FamilyRepresentative` con `Estado = Active`, `OnboardingCompletado = false`.
- Se marca la invitación como `Usada = true`.
- Si la invitación tenía `PersonaAsociada`, se crea `PersonRepresentative` vinculando al familiar con la persona.
- Se registra entrada inicial en `FamilyStatusHistory`.

---

## Alta — Vínculo Persona–Familiar

**Actor:** Profesional

Permite vincular un familiar ya registrado con una persona con discapacidad (o agregar un vínculo adicional).

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Persona | Referencia | Sí | Debe existir y estar activa |
| Familiar | Referencia | Sí | Debe existir y estar activo |
| Relación | Texto (50) | No | — |
| Es primario | Booleano | Sí | — |
| Tiene consentimiento informado | Booleano | Sí | — |
| Fecha de consentimiento | Fecha | Condicional | Obligatoria si `TieneConsentimiento = true` |
| Puede supervisar login | Booleano | Sí | — |

**Validaciones de integridad:**
- No puede existir ya un vínculo activo entre la misma persona y el mismo familiar.
- Si `EsPrimario = true`, el vínculo primario anterior se actualiza a `EsPrimario = false`.

**Resultado:** Registro en `PersonRepresentative` + entrada en `PersonRepresentativeHistory` (tipo: `Linked`).

---

## Baja — Vínculo Persona–Familiar

**Actor:** Profesional

- Se establece `Activo = false` en `PersonRepresentative`, se registra `FechaDesvinculación`.
- Se crea entrada en `PersonRepresentativeHistory` (tipo: `Unlinked`) con observación obligatoria.
- El `FamilyRepresentative` y su `User` no se eliminan.

---

## Baja — Familiar

**Actor:** Profesional / Administrador Institucional

- Cambia `Estado = Terminated` en `FamilyRepresentative` e `IsActive = false` en `User`.
- Se desactivan todos los `PersonRepresentative` activos del familiar.
- Se registra en `FamilyStatusHistory`.

---

## Modificación — Vínculo Persona–Familiar

**Actor:** Profesional

| Campo | Validaciones |
|-------|--------------|
| Relación | — |
| Es primario | Si se activa, el anterior primario se desactiva |
| Tiene consentimiento informado | — |
| Fecha de consentimiento | Obligatoria si `TieneConsentimiento = true` |
| Puede supervisar login | — |

**Resultado:** Se actualiza `PersonRepresentative` + entrada en `PersonRepresentativeHistory` (tipo: `Updated`).

---

## Listado — Invitaciones

**Actor:** Profesional

| Columna | Descripción |
|---------|-------------|
| Email | Destinatario de la invitación |
| Nombre y Apellido | Si se cargaron al crear la invitación |
| Persona asociada | Nombre de la persona con discapacidad |
| Fecha de expiración | Hasta cuándo es válida |
| Estado | Pendiente / Usada / Expirada |

**Filtros:** persona asociada, estado.

---

## Listado — Familiares

**Actor:** Profesional

| Columna | Descripción |
|---------|-------------|
| Nombre y Apellido | Identidad del familiar |
| DNI | Documento |
| Relación | Vínculo con la persona |
| Personas vinculadas | Personas con discapacidad que representa |
| Puede supervisar login | Sí / No |
| Estado | Active / Terminated |

**Filtros:** nombre/DNI, estado, persona vinculada.  
**Persistencia:** Consulta a `FamilyRepresentative` con join a `PersonRepresentative`, filtrado por personas asignadas al profesional autenticado.
