# Listado de ABMs — InclusiON

**Última actualización:** 2026-05-03

Cada ABM está justificado por el actor del sistema que lo necesita para cumplir su función. Todos los ABMs tienen persistencia real en base de datos (PostgreSQL vía EF Core) y validaciones básicas de integridad.

---

## Actores del sistema

| Actor | Descripción |
|-------|-------------|
| **Administrador Global** | Gestiona la plataforma completa; no pertenece a ninguna institución específica |
| **Administrador Institucional** | Gestiona profesionales y configuración de su institución |
| **Profesional** | Docente, terapeuta o psicólogo que trabaja con personas con discapacidad |
| **Representante Familiar** | Familiar o tutor que acompaña a la persona con discapacidad |
| **Persona con Discapacidad** | Destinatario central del sistema; realiza actividades |

---

## Resumen — ABMs por actor

Leyenda: ✅ Completo · 🔄 Parcial · ⏳ Pendiente

| # | ABM | Actor principal | Entidades del DER | Estado |
|---|-----|-----------------|-------------------|--------|
| 01 | [Instituciones](./01-instituciones.md) | Admin Global | `EducationalInstitution`, `AdminInstitution` | ✅ |
| 02 | [Administradores](./02-administradores.md) | Admin Global | `User` (rol admin) | ✅ |
| 03 | [Catálogos](./03-catalogos.md) | Admin Global | `DisabilityType`, `AutonomyLevel`, `ActivityCategory`, `SkillArea`, `ActivityTemplateType`, `ReportType` | ✅ |
| 04 | [Profesionales](./04-profesionales.md) | Admin Institucional | `Professional`, `ProfessionalStatusHistory`, `ProfessionalInstitution` | ✅ |
| 05 | [Personas con Discapacidad](./05-personas.md) | Profesional | `PersonWithDisability`, `PersonSkillProfile` | ✅ |
| 06 | [Representantes Familiares](./06-familiares.md) | Profesional | `FamilyRepresentative`, `FamilyStatusHistory`, `Invitation`, `PersonRepresentative`, `PersonRepresentativeHistory` | ✅ |
| 07 | [Asignación Profesional–Persona](./07-asignacion-profesional-persona.md) | Profesional / Admin Institucional | `ProfessionalPerson` | ✅ |
| 08 | [Actividades](./08-actividades.md) | Profesional | `Activity`, `ActivityContent`, `ActivityEmbedding` | ✅ |
| 09 | [Roadmap](./09-roadmap.md) | Profesional | `PersonRoadmap`, `PersonRoadmapArea`, `PersonRoadmapActivity`, `AdaptiveEngineConfig` | ✅ |
| 10 | [Asignaciones de Actividad](./10-asignaciones.md) | Profesional | `ActivityAssignment`, `ActivityResponse`, `ActivityResult` | ✅ |
| 11 | [Diagnósticos](./11-diagnosticos.md) | Profesional | `Diagnosis` | ✅ |
| 12 | [Reportes de Progreso](./12-reportes.md) | Profesional | `Report` | ✅ |
| 13 | [Mensajes](./13-mensajes.md) | Profesional / Representante Familiar | `Message` | ✅ |

---

## Entidades del DER cubiertas

| # | Entidad (`DbSet`) | ABM que la cubre |
|---|-------------------|------------------|
| 1 | `DisabilityType` | 03 — Catálogos |
| 2 | `AutonomyLevel` | 03 — Catálogos |
| 3 | `ActivityCategory` | 03 — Catálogos |
| 4 | `SkillArea` | 03 — Catálogos |
| 5 | `ActivityTemplateType` | 03 — Catálogos |
| 6 | `LoginMethod` | 03 — Catálogos (solo-lectura — seed de sistema) |
| 7 | `ReportType` | 03 — Catálogos |
| 8 | `User` | 02 — Administradores / implícito en 04, 05, 06 |
| 9 | `RefreshToken` | Gestión de sesión — sin ABM de usuario |
| 10 | `TrustedDevice` | Gestión de sesión — sin ABM de usuario |
| 11 | `Professional` | 04 — Profesionales |
| 12 | `ProfessionalStatusHistory` | 04 — Profesionales (historial automático) |
| 13 | `PersonWithDisability` | 05 — Personas |
| 14 | `FamilyRepresentative` | 06 — Familiares |
| 15 | `FamilyStatusHistory` | 06 — Familiares (historial automático) |
| 16 | `EducationalInstitution` | 01 — Instituciones |
| 17 | `AdminInstitution` | 01 — Instituciones |
| 18 | `ProfessionalInstitution` | 04 — Profesionales |
| 19 | `ProfessionalPerson` | 07 — Asignación Profesional–Persona |
| 20 | `PersonRepresentative` | 06 — Familiares |
| 21 | `PersonRepresentativeHistory` | 06 — Familiares (historial automático) |
| 22 | `PersonSkillProfile` | 05 — Personas |
| 23 | `Invitation` | 06 — Familiares |
| 24 | `Activity` | 08 — Actividades |
| 25 | `ActivityContent` | 08 — Actividades |
| 26 | `ActivityEmbedding` | 08 — Actividades (generado automáticamente) |
| 27 | `PersonRoadmap` | 09 — Roadmap |
| 28 | `PersonRoadmapArea` | 09 — Roadmap |
| 29 | `PersonRoadmapActivity` | 09 — Roadmap |
| 30 | `ActivityAssignment` | 10 — Asignaciones |
| 31 | `ActivityResponse` | 10 — Asignaciones (registro automático) |
| 32 | `ActivityResult` | 10 — Asignaciones (calculado automáticamente) |
| 33 | `AdaptiveEngineConfig` | 09 — Roadmap |
| 34 | `AdaptiveAdjustmentLog` | Motor adaptativo (registro automático) |
| 35 | `Diagnosis` | 11 — Diagnósticos |
| 36 | `Report` | 12 — Reportes |
| 37 | `Message` | 13 — Mensajes |
| 38 | `AccessAudit` | Auditoría (append-only — sin ABM de usuario) |

---

## Convenciones aplicadas en todos los ABMs

- **Baja lógica:** ninguna entidad se elimina físicamente. Todas tienen campo `Activo` (o `IsActive`). La baja establece `Activo = false`.
- **Listado con filtros:** todos los listados permiten filtrar por estado activo/inactivo y términos de búsqueda relevantes.
- **Persistencia real:** cada operación se confirma en PostgreSQL vía `AppDbContext.SaveChangesAsync()`.
- **Validaciones de integridad:** se validan unicidad, referencias existentes y reglas de negocio antes de persistir.
- **Auditoría:** las operaciones de baja y cambios de estado generan registros en las tablas de historial correspondientes.
