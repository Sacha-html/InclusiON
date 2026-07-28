# Listado de ABMs — InclusiON

**Última actualización:** 2026-05-05

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

## ✅ MVP — Práctica II (ABM-01 a ABM-13)

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
---

## 🔜 Post-MVP — Práctica III

> El esquema de base de datos ya contempla estas entidades. Los ABMs se formalizarán en la siguiente iteración.

| # | ABM | Actor principal | Entidades del DER | Estado |
|---|-----|-----------------|-------------------|--------|
| 14 | Motor Adaptativo *(pendiente)* | Sistema / Profesional | `AdaptiveEngineConfig` (activo), `AdaptiveAdjustmentLog` | ⏳ |

*ABM-14 cubre los CU-48 (configurar), CU-49 (historial ajustes) y CU-50 (ajuste automático por rendimiento). La lógica de disparo es automática (Sistema); la configuración de umbrales la gestiona el Profesional.*

---

## Entidades del DER cubiertas

| # | Entidad (`DbSet`) | ABM que la cubre | Fase |
|---|-------------------|------------------|------|
| 1 | `DisabilityType` | 03 — Catálogos | MVP |
| 2 | `AutonomyLevel` | 03 — Catálogos | MVP |
| 3 | `ActivityCategory` | 03 — Catálogos | MVP |
| 4 | `SkillArea` | 03 — Catálogos | MVP |
| 5 | `ActivityTemplateType` | 03 — Catálogos | MVP |
| 6 | `LoginMethod` | 03 — Catálogos (solo-lectura — seed de sistema) | MVP |
| 7 | `ReportType` | 03 — Catálogos | MVP |
| 8 | `User` | 02 — Administradores / implícito en 04, 05, 06 | MVP |
| 9 | `RefreshToken` | Gestión de sesión — sin ABM de usuario | MVP |
| 10 | `TrustedDevice` | Gestión de sesión — sin ABM de usuario | MVP |
| 11 | `Professional` | 04 — Profesionales | MVP |
| 12 | `ProfessionalStatusHistory` | 04 — Profesionales (historial automático) | MVP |
| 13 | `PersonWithDisability` | 05 — Personas | MVP |
| 14 | `FamilyRepresentative` | 06 — Familiares | MVP |
| 15 | `FamilyStatusHistory` | 06 — Familiares (historial automático) | MVP |
| 16 | `EducationalInstitution` | 01 — Instituciones | MVP |
| 17 | `AdminInstitution` | 01 — Instituciones | MVP |
| 18 | `ProfessionalInstitution` | 04 — Profesionales | MVP |
| 19 | `ProfessionalPerson` | 07 — Asignación Profesional–Persona | MVP |
| 20 | `PersonRepresentative` | 06 — Familiares | MVP |
| 21 | `PersonRepresentativeHistory` | 06 — Familiares (historial automático) | MVP |
| 22 | `PersonSkillProfile` | 05 — Personas | MVP |
| 23 | `Invitation` | 06 — Familiares | MVP |
| 24 | `Activity` | 08 — Actividades | MVP |
| 25 | `ActivityContent` | 08 — Actividades | MVP |
| 26 | `ActivityEmbedding` | 08 — Actividades (generado automáticamente) | MVP |
| 27 | `PersonRoadmap` | 09 — Roadmap | MVP |
| 28 | `PersonRoadmapArea` | 09 — Roadmap | MVP |
| 29 | `PersonRoadmapActivity` | 09 — Roadmap | MVP |
| 30 | `ActivityAssignment` | 10 — Asignaciones | MVP |
| 31 | `ActivityResponse` | 10 — Asignaciones (registro automático) | MVP |
| 32 | `ActivityResult` | 10 — Asignaciones (calculado automáticamente) | MVP |
| 33 | `AdaptiveEngineConfig` | 14 — Motor Adaptativo | **Post-MVP** |
| 34 | `AdaptiveAdjustmentLog` | 14 — Motor Adaptativo (registro automático) | **Post-MVP** |
| 35 | `Diagnosis` | 11 — Diagnósticos | MVP |
| 36 | `Report` | 12 — Reportes | MVP |
| 37 | `Message` | 13 — Mensajes | MVP |
| 38 | `AccessAudit` | Auditoría (append-only — sin ABM de usuario) | MVP |

---

## Totales

| Fase | ABMs | Entidades DER |
|------|------|---------------|
| ✅ MVP | 13 | 36 |
| 🔜 Post-MVP | 1 | 2 |
| **Total** | **14** | **38** |

---

## Convenciones aplicadas en todos los ABMs

- **Baja lógica:** ninguna entidad se elimina físicamente. Todas tienen campo `Activo` (o `IsActive`). La baja establece `Activo = false`.
- **Listado con filtros:** todos los listados permiten filtrar por estado activo/inactivo y términos de búsqueda relevantes.
- **Persistencia real:** cada operación se confirma en PostgreSQL vía `AppDbContext.SaveChangesAsync()`.
- **Validaciones de integridad:** se validan unicidad, referencias existentes y reglas de negocio antes de pristir.
- **Auditoría:** las operaciones de baja y cambios de estado generan registros en las tablas de historial correspondientes.
