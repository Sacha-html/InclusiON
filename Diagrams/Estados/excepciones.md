# Lista de Excepciones del Sistema

Situaciones que el sistema debe rechazar, derivadas de los diagramas de estado, el DER y los ABMs. Organizadas por categoría.

---

## 1. Transiciones de estado inválidas

Intentos de mover una entidad a un estado que no existe en su máquina de estados.

| # | Entidad | Situación rechazada | Por qué |
|---|---------|---------------------|---------|
| E-01 | `Professional` | Transición desde `Rejected` a cualquier estado | Estado terminal sin salida documentada |
| E-02 | `Professional` | Transición desde `Terminated` a cualquier estado | Estado terminal irreversible |
| E-03 | `Professional` | Transición inversa `Approved → Pending` o `Suspended → Pending` | El diagrama no define retroceso; Pending es solo estado inicial |
| E-04 | `Report` | Editar (`PUT`) un reporte que no está en `Draft` | Solo `Draft` es editable. `Submitted`, `Approved` y `Rejected` son de solo lectura |
| E-05 | `Report` | Enviar (`submit`) un reporte que no está en `Draft` | La transición `Submitted` solo parte desde `Draft` |
| E-06 | `Report` | Aprobar o rechazar un reporte que no está en `Submitted` | Las transiciones `Approved` y `Rejected` solo parten desde `Submitted` |
| E-07 | `Report` | Rechazar sin `AdminComment` | La transición `Submitted → Rejected` requiere motivo obligatorio |
| E-08 | `Report` | Reabrir un reporte `Rejected` | Estado terminal; la corrección implica crear un nuevo `Draft` |
| E-09 | `ActivityAssignment` | Cancelar una asignación en estado `Completed` | `Completed` es terminal; no hay transición de salida |
| E-10 | `Invitation` | Usar un código de invitación con estado `Expired` | Pasó el TTL de 7 días; la transición `Used` solo es válida desde `Pending` |
| E-11 | `Invitation` | Usar un código de invitación con estado `Used` | Ya fue consumida; no puede usarse dos veces |
| E-12 | `Invitation` | Usar un código de invitación con estado `Cancelled` | Fue cancelada por el profesional |

---

## 2. Violaciones de unicidad

Intentos de crear registros duplicados donde el modelo exige singularidad.

| # | Entidad | Situación rechazada | Por qué |
|---|---------|---------------------|---------|
| E-13 | `User` | Email ya registrado | `User.Email` es `UNIQUE` en el DER |
| E-14 | `Professional` | DNI ya registrado en otro `Professional` | `Professional.DocumentNumber` es `UNIQUE` |
| E-15 | `Professional` | Matrícula ya registrada en otro `Professional` | `Professional.LicenseNumber` es `UNIQUE` si se ingresa |
| E-16 | `PersonWithDisability` | DNI ya registrado en otra persona | `PersonWithDisability.DocumentNumber` es `UNIQUE` |
| E-17 | `PersonRoadmap` | Crear un segundo roadmap activo para la misma persona | Relación 1:1 entre `PersonWithDisability` y `PersonRoadmap` activo |
| E-18 | `PersonRoadmapArea` | Agregar la misma área de habilidad dos veces al mismo roadmap | Área no puede repetirse en el roadmap |
| E-19 | `PersonRoadmapActivity` | Agregar la misma actividad dos veces en la misma área del roadmap | Actividad no puede repetirse dentro del área |
| E-20 | `AdaptiveEngineConfig` | Crear una segunda config para la misma `PersonRoadmapActivity` | Relación 1:1 entre `PersonRoadmapActivity` y `AdaptiveEngineConfig` |
| E-21 | `ActivityAssignment` | Asignar la misma actividad a la misma persona cuando ya existe una asignación activa | No puede haber dos asignaciones activas del mismo par actividad+persona |
| E-22 | `ProfessionalPerson` | Asignar el mismo profesional a la misma persona cuando ya existe vínculo activo | Duplicado en `ProfessionalPerson` |
| E-23 | `PersonRepresentative` | Vincular el mismo familiar con la misma persona cuando ya existe vínculo activo | Duplicado en `PersonRepresentative` |
| E-24 | `Invitation` | Crear invitación cuando ya existe una `Pending` para el mismo `Email + ForPersonId` | Invitación activa duplicada para el mismo destino |
| E-25 | `ActivityContent` | Crear un segundo `ActivityContent` para la misma actividad | Relación 1:1 entre `Activity` y `ActivityContent` |

---

## 3. Dependencia en estado inválido

Intentos de operar sobre una entidad que referencia otra en un estado que lo impide.

| # | Entidad | Situación rechazada | Por qué |
|---|---------|---------------------|---------|
| E-26 | `ProfessionalPerson` | Crear asignación con un `Professional` cuyo `Status != Approved` | Solo profesionales aprobados pueden ser asignados a personas |
| E-27 | `ProfessionalPerson` | Crear asignación si el profesional y la persona no pertenecen a la misma institución | El scope institucional debe ser compartido |
| E-28 | `Activity` | Dar de baja una actividad con `ActivityAssignment` en estado `Pending` o `InProgress` | Desactivar la actividad dejaría asignaciones activas apuntando a contenido inactivo |
| E-29 | `Activity` | Cambiar el `TemplateType` de `ActivityContent` si la actividad ya fue asignada al menos una vez | El cambio invalidaría los `ActivityResponse` ya registrados para esa actividad |
| E-30 | `PersonRoadmapActivity` | Dar de baja la única actividad desbloqueada de un área | La persona quedaría sin poder avanzar en esa área |
| E-31 | `PersonRoadmapArea` | Dar de baja un área que aún tiene actividades activas | Se deben dar de baja primero todas sus actividades |
| E-32 | `PersonSkillProfile` | Eliminar un área de habilidad que tiene un `PersonRoadmapArea` activo | El área está en uso en el roadmap de la persona |

---

## 4. Violaciones de configuración

Valores que individualmente son válidos por tipo pero son inconsistentes entre sí.

| # | Entidad | Situación rechazada | Por qué |
|---|---------|---------------------|---------|
| E-33 | `AdaptiveEngineConfig` | `MinDifficultyLevel >= MaxDifficultyLevel` | El rango de dificultad debe ser positivo para que el motor pueda ajustar |
| E-34 | `PersonWithDisability` | `LoginMethod` incompatible con `AutonomyLevel` | El método de autenticación debe ser coherente con el nivel de autonomía de la persona |
| E-35 | `Professional` | Modificar el email si `Status != Pending` | El email solo puede cambiarse mientras la cuenta no fue activada |
---

## 5. Violaciones de autenticación y sesión

| # | Entidad | Situación rechazada | Por qué |
|---|---------|---------------------|---------|
| E-36 | `RefreshToken` | Usar un token con `RevokedAt IS NOT NULL` | El token fue revocado explícitamente (logout o forzado) |
| E-37 | `RefreshToken` | Usar un token con `ExpiresAt ≤ now()` | El TTL del token venció |

---

## 6. Violaciones de scope del actor

Intentos de operar sobre recursos fuera del alcance autorizado del actor autenticado.

| # | Actor | Situación rechazada | Por qué |
|---|-------|---------------------|---------|
| E-38 | Profesional | Acceder o modificar datos de una persona a la que no está asignado | `ProfessionalPerson` activo es la condición de acceso |
| E-39 | Profesional | Dar de baja una actividad estándar (`IsStandardActivity = true`) | Las actividades estándar solo puede darlas de baja el Admin Global |
| E-40 | Profesional | Dar de baja o editar una actividad cuyo `ProfessionalId` no le pertenece | Solo el creador puede modificar su actividad |
| E-41 | Admin Institucional | Gestionar profesionales o personas de una institución a la que no está asignado | `AdminInstitution` define el scope del admin |
| E-42 | Familiar | Acceder a un `Report` que no está en estado `Approved` | Familiares solo ven reportes aprobados |
| E-43 | Familiar | Acceder a datos de una persona con la que no tiene `PersonRepresentative` activo | El vínculo activo define el scope del familiar |
