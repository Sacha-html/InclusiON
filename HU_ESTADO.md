# InclusiON — Estado de Historias de Usuario

**Última actualización:** 2026-03-08

Leyenda de estados:
- ✅ **HECHO** — Implementado y funcionando
- 🔧 **PARCIAL** — Entidades/migraciones existen pero falta lógica de negocio
- ⏳ **PENDIENTE** — No iniciado

---

## Resumen Rápido

| Track | Hechas | Parciales | Pendientes | Total |
|-------|--------|-----------|------------|-------|
| Backend | 4 | 2 | 11 | 17 |
| Frontend | 3 | 0 | 15 | 18 |

---

## Track Backend (BE)

### Sprint 1 — Bases

| ID | Historia | Estado | Notas |
|----|----------|--------|-------|
| BE-01 | Catálogos de Referencia (lectura) | ✅ HECHO | `CatalogsController` + queries + handlers para disability-types, autonomy-levels, activity-categories, skill-areas, activity-template-types |
| BE-02 | CRUD Profesionales | ✅ HECHO | `ProfessionalsController` + Create/Update/Deactivate commands + GetAll/GetById queries |
| BE-03 | Asignaciones Profesional-Institución-Persona | ⏳ PENDIENTE | Entidades `ProfessionalInstitution`, `ProfessionalPerson` existen en Domain pero no hay handlers ni endpoints |

### Sprint 2 — Actividades y Roadmap

| ID | Historia | Estado | Notas |
|----|----------|--------|-------|
| BE-04 | Áreas de Habilidad (SkillAreas) | ✅ HECHO | Incluido en catálogos (BE-01) como `GET /api/catalogs/skill-areas` |
| BE-05 | Tipos de Template de Actividad | ✅ HECHO | Incluido en catálogos (BE-01) como `GET /api/catalogs/activity-template-types` |
| BE-06 | CRUD Actividades con Contenido Dinámico | ⏳ PENDIENTE | Entidades `Activity`, `ActivityContent` existen. Falta: controller, commands, queries, handlers |
| BE-07 | Perfil de Habilidades de la Persona | ⏳ PENDIENTE | Sin implementación |
| BE-08 | Sistema de Invitaciones Familiares | ⏳ PENDIENTE | Entidad `Invitation` existe. Falta todo el flujo |
| BE-09 | Roadmap de la Persona | ⏳ PENDIENTE | Entidades `PersonRoadmap`, `PersonRoadmapArea`, `PersonRoadmapActivity` existen. Falta: controller, handlers |

### Sprint 3 — Ejecución y Métricas

| ID | Historia | Estado | Notas |
|----|----------|--------|-------|
| BE-10 | Asignaciones — Consulta y Gestión | ⏳ PENDIENTE | Entidad `ActivityAssignment` existe. Sin handlers |
| BE-11 | Respuestas — Ciclo de Vida de Actividad | ⏳ PENDIENTE | Entidad `ActivityResponse` existe. Sin handlers |
| BE-12 | Radar Chart y Dashboard con Datos Reales | ⏳ PENDIENTE | Sin implementación |

### Sprint 4 — Avanzados

| ID | Historia | Estado | Notas |
|----|----------|--------|-------|
| BE-13 | Diagnósticos Funcionales | ⏳ PENDIENTE | Entidad `Diagnosis` existe. Sin handlers |
| BE-14 | Reportes de Progreso | ⏳ PENDIENTE | Entidades `Report`, `ReportType` existen. Sin handlers |
| BE-15 | Mensajería Interna | ⏳ PENDIENTE | Entidad `Message` existe. Sin handlers |
| BE-16 | Búsqueda Semántica de Actividades | 🔧 PARCIAL | Entidades `ActivityEmbedding`, `ActivityResult` con migración. Library `SemanticSearch` existe. Falta: interfaces en Application, handler, endpoint. Ver [Features/integracion-semantic-search.md](./Features/integracion-semantic-search.md) |
| BE-17 | Motor de Dificultad Adaptativa (MDA) | 🔧 PARCIAL | Entidades `AdaptiveEngineConfig`, `AdaptiveAdjustmentLog` con migración. Falta: `IAdaptiveEngineService`, implementación, pipeline steps. Ver [Features/MDA_Especificacion_Tecnica.md](./Features/MDA_Especificacion_Tecnica.md) |

---

## Track Frontend (FE)

### Sprint 1 — Bases

| ID | Historia | Depende de (BE) | Estado | Notas |
|----|----------|-----------------|--------|-------|
| FE-01 | CatalogsService y Conexión con Form de Persona | BE-01 | ✅ HECHO | `persons.service.ts` conectado, formularios funcionando |
| FE-02 | Listado y Formulario de Profesionales | BE-02 | ✅ HECHO | Vistas en `views/professional/` |
| FE-03 | Asignaciones Institución/Persona en Detalle Profesional | BE-03 | ⏳ PENDIENTE | Depende de BE-03 |

### Sprint 2 — Actividades y Roadmap

| ID | Historia | Depende de (BE) | Estado | Notas |
|----|----------|-----------------|--------|-------|
| FE-04 | Formulario de Actividad con Template Dinámico | BE-06 | ⏳ PENDIENTE | Depende de BE-06 |
| FE-05 | Catálogo de Actividades del Profesional | BE-06 | ⏳ PENDIENTE | Depende de BE-06 |
| FE-06 | Sección Perfil de Habilidades en Perfil Persona | BE-07 | ⏳ PENDIENTE | Depende de BE-07 |
| FE-07 | Gestor de Roadmap (vista Profesional) | BE-09 | ⏳ PENDIENTE | Depende de BE-09 |
| FE-08 | Registro por Invitación (página pública Familia) | BE-08 | ⏳ PENDIENTE | Depende de BE-08 |

### Sprint 3 — Experiencia del Estudiante

| ID | Historia | Depende de (BE) | Estado | Notas |
|----|----------|-----------------|--------|-------|
| FE-09 | Roadmap Visual (vista Estudiante, estilo Duolingo) | BE-09, BE-10 | ⏳ PENDIENTE | AAC portal, componente principal |
| FE-10 | ActivityPlayerShell (cargador dinámico) | BE-11 | ⏳ PENDIENTE | Shell que carga el player correcto según template |
| FE-11 | Activity Players (5 componentes) | BE-11 | ⏳ PENDIENTE | Selección, emparejamiento, secuencia, completar, respuesta libre |
| FE-12 | Radar Chart de Habilidades | BE-12 | ⏳ PENDIENTE | Gráfica radar por persona |
| FE-13 | Dashboards con Datos Reales | BE-12 | ✅ HECHO | Dashboards base existen (pro, family, admin) — falta conectar datos reales |

### Sprint 4 — Avanzados

| ID | Historia | Depende de (BE) | Estado | Notas |
|----|----------|-----------------|--------|-------|
| FE-14 | Timeline de Diagnósticos y Formulario | BE-13 | ⏳ PENDIENTE | |
| FE-15 | Creación de Reportes y Vista Familia | BE-14 | ⏳ PENDIENTE | |
| FE-16 | Mensajería Interna — Inbox y Redactar | BE-15 | ⏳ PENDIENTE | |
| FE-17 | Panel de Configuración del Motor Adaptativo | BE-17 | ⏳ PENDIENTE | Config de rangos para el profesional |
| FE-18 | Timeline de Ajustes Adaptativos | BE-17 | ⏳ PENDIENTE | Gráficas de evolución |

---

## Lo que SÍ está implementado (transversal)

Estos componentes no son HUs específicas pero están funcionando:

| Componente | Estado | Descripción |
|-----------|--------|-------------|
| Autenticación JWT | ✅ | Login estándar, PIN, visual, asistido, familiar, refresh tokens |
| CRUD Personas | ✅ | `PersonsController` + handlers completos |
| CRUD Usuarios | ✅ | `UsersController` + handlers |
| Sistema de Login Visual | ✅ | 7 componentes Angular (identify, role-select, PIN, standard, assisted, family, method-selector) |
| Layouts por Rol | ✅ | AAC (estudiante), Professional, Family, Admin — con navegación y guards |
| Sistema de Accesibilidad | ✅ | 7 perfiles (default, high-contrast, dyslexia, low-vision, deuteranopia, protanopia, tritanopia) + light/dark |
| Paginación genérica | ✅ | `ToPagedAsync()` extensions con sorting dinámico |
| Filtros para AuditableEntity | ✅ | `WhereActive()`, `WhereCreatedBetween()`, etc. |
| Seeder de base de datos | ✅ | `DatabaseSeeder` con datos iniciales |
| EF Core Configurations | ✅ | 36 archivos de configuración de entidades |
| Migraciones | ✅ | 13 migraciones aplicadas (hasta Add-AdaptiveEngine) |

---

## Orden de Implementación Recomendado

Para avanzar con las HU pendientes, este es el orden óptimo respetando dependencias:

### Próximo bloque (Sprint 2 BE)
1. **BE-06** — CRUD Actividades (desbloquea FE-04, FE-05, y es prerequisito de todo lo que viene)
2. **BE-03** — Asignaciones Profesional-Institución-Persona (desbloquea FE-03)
3. **BE-07** — Perfil de Habilidades (desbloquea FE-06)
4. **BE-08** — Invitaciones (desbloquea FE-08)
5. **BE-09** — Roadmap (desbloquea FE-07, FE-09)

### Segundo bloque (Sprint 3 BE)
6. **BE-10** — Asignaciones de actividades
7. **BE-11** — Respuestas y ciclo de vida (desbloquea FE-10, FE-11)
8. **BE-12** — Radar y Dashboard (desbloquea FE-12)

### Tercer bloque (Sprint 4 BE)
9. **BE-13** a **BE-15** — Diagnósticos, Reportes, Mensajería
10. **BE-16** — Búsqueda Semántica
11. **BE-17** — Motor Adaptativo (depende de BE-11)
