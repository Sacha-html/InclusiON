# InclusiON — Estado de Historias de Usuario

**Última actualización:** 2026-04-17

Leyenda de estados:
- ✅ **HECHO** — Implementado y funcionando
- 🔧 **PARCIAL** — Entidades/migraciones existen pero falta lógica de negocio
- ⏳ **PENDIENTE** — No iniciado

---

## Resumen Rápido

| Track | Hechas | Parciales | Pendientes | Total |
|-------|--------|-----------|------------|-------|
| Backend | 9 | 2 | 9 | 20 |
| Frontend | 8 | 0 | 13 | 21 |

---

## Track Backend (BE)

### Sprint 1 — Bases

| ID | Historia | Estado | Notas |
|----|----------|--------|-------|
| BE-01 | Catálogos de Referencia (lectura) | ✅ HECHO | `CatalogsController` + queries + handlers para disability-types, autonomy-levels, activity-categories, skill-areas, activity-template-types |
| BE-02 | CRUD Profesionales | ✅ HECHO | `ProfessionalsController` + Create/Update/Deactivate commands + GetAll/GetById queries |
| BE-03 | Asignaciones Profesional-Institución-Persona | ✅ HECHO | `AssignmentsController` + `InstitutionsController` + handlers + repos para asignaciones profesional-persona-institución |

### Sprint 2 — Actividades y Roadmap

| ID | Historia | Estado | Notas |
|----|----------|--------|-------|
| BE-04 | Áreas de Habilidad (SkillAreas) | ✅ HECHO | Incluido en catálogos (BE-01) como `GET /api/catalogs/skill-areas` |
| BE-05 | Tipos de Template de Actividad | ✅ HECHO | Incluido en catálogos (BE-01) como `GET /api/catalogs/activity-template-types` |
| BE-06 | CRUD Actividades con Contenido Dinámico | ⏳ PENDIENTE | Entidades `Activity`, `ActivityContent` existen. Falta: controller, commands, queries, handlers |
| BE-07 | Perfil de Habilidades de la Persona | ✅ HECHO | `PersonSkillProfile` entity + endpoints GET/POST/PUT en PersonsController |
| BE-08 | Sistema de Invitaciones Familiares | ✅ HECHO | `InvitationsController` con 4 endpoints + email SMTP con MailKit + templates HTML |
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
| BE-13 | Diagnósticos Funcionales | ✅ HECHO | `DiagnosesController` + Create/Update commands + GetAll/GetById queries + `DiagnosesRepository`. Endpoints: GET /persons/{id}/diagnoses, GET /diagnoses/{id}, POST, PUT |
| BE-14 | Reportes de Progreso | ✅ HECHO | Flujo completo: Draft→Submitted→Approved/Rejected. Endpoints: GET, POST, PUT, PATCH submit/approve/reject, GET /family. Emails a familiar (aprobación) y profesional (rechazo). Ver [Features/reportes-flujo-aprobacion.md](./Features/reportes-flujo-aprobacion.md) |
| BE-15 | Mensajería Interna | ⏳ PENDIENTE | Entidad `Message` existe. Sin handlers |
| BE-16 | Búsqueda Semántica de Actividades | 🔧 PARCIAL | Entidades `ActivityEmbedding`, `ActivityResult` con migración. Library `SemanticSearch` existe. Falta: interfaces en Application, handler, endpoint. Ver [Features/integracion-semantic-search.md](./Features/integracion-semantic-search.md) |
| BE-17 | Motor de Dificultad Adaptativa (MDA) | 🔧 PARCIAL | Entidades `AdaptiveEngineConfig`, `AdaptiveAdjustmentLog` con migración. Falta: `IAdaptiveEngineService`, implementación, pipeline steps. Ver [Features/MDA_Especificacion_Tecnica.md](./Features/MDA_Especificacion_Tecnica.md) |

### Sprint 5 — Gestión de Usuarios, Onboarding y Soporte

| ID | Historia | Estado | Notas |
|----|----------|--------|-------|
| BE-18 | Gestión Centralizada de Usuarios | ✅ HECHO | `AdminUsersController` + handlers: listado paginado (raw SQL), reset password, deactivate, reactivate. Proceso 17 |
| BE-19 | Onboarding de Usuarios | ⏳ PENDIENTE | Completar perfil profesional, flags onboarding, endpoint completado. Proceso 18 |
| BE-20 | Soporte y Ayuda | ⏳ PENDIENTE | FAQ CRUD, tickets CRUD, respuestas. Proceso 19 |

---

## Track Frontend (FE)

### Sprint 1 — Bases

| ID | Historia | Depende de (BE) | Estado | Notas |
|----|----------|-----------------|--------|-------|
| FE-01 | CatalogsService y Conexión con Form de Persona | BE-01 | ✅ HECHO | `persons.service.ts` conectado, formularios funcionando |
| FE-02 | Listado y Formulario de Profesionales | BE-02 | ✅ HECHO | Vistas en `views/professional/` |
| FE-03 | Asignaciones Institución/Persona en Detalle Profesional | BE-03 | ✅ HECHO | Sección personas a cargo e instituciones en detalle profesional con tabs |

### Sprint 2 — Actividades y Roadmap

| ID | Historia | Depende de (BE) | Estado | Notas |
|----|----------|-----------------|--------|-------|
| FE-04 | Formulario de Actividad con Template Dinámico | BE-06 | ⏳ PENDIENTE | Depende de BE-06 |
| FE-05 | Catálogo de Actividades del Profesional | BE-06 | ⏳ PENDIENTE | Depende de BE-06 |
| FE-06 | Sección Perfil de Habilidades en Perfil Persona | BE-07 | ✅ HECHO | Sección perfil de habilidades con chips coloreados en detalle persona |
| FE-07 | Gestor de Roadmap (vista Profesional) | BE-09 | ⏳ PENDIENTE | Depende de BE-09 |
| FE-08 | Registro por Invitación (página pública Familia) | BE-08 | ✅ HECHO | Registro por invitación (página pública /invite/:code) |

### Sprint 3 — Experiencia del Estudiante

| ID | Historia | Depende de (BE) | Estado | Notas |
|----|----------|-----------------|--------|-------|
| FE-09 | Roadmap Visual (vista Estudiante, estilo Duolingo) | BE-09, BE-10 | ⏳ PENDIENTE | AAC portal, componente principal |
| FE-10 | ActivityPlayerShell (cargador dinámico) | BE-11 | ⏳ PENDIENTE | Shell que carga el player correcto según template |
| FE-11 | Activity Players (5 componentes) | BE-11 | ⏳ PENDIENTE | Selección, emparejamiento, secuencia, completar, respuesta libre |
| FE-12 | Radar Chart de Habilidades | BE-12 | ⏳ PENDIENTE | Gráfica radar por persona |
| FE-13 | Dashboards con Datos Reales | BE-12 | ✅ HECHO | Dashboard profesional con datos reales (personas, invitaciones, saludo personalizado) |

### Sprint 4 — Avanzados

| ID | Historia | Depende de (BE) | Estado | Notas |
|----|----------|-----------------|--------|-------|
| FE-14 | Timeline de Diagnósticos y Formulario | BE-13 | ⏳ PENDIENTE | Backend listo. Falta: vistas Angular en perfil de persona (formulario + timeline). Jira IN-86 |
| FE-15 | Creación de Reportes y Vista Familia | BE-14 | ✅ HECHO | Profesional: lista + alta con modal + submit. Admin: lista global + tab en detalle profesional. Familiar: lista + detalle en `/family/reports`. Jira IN-138 |
| FE-16 | Mensajería Interna — Inbox y Redactar | BE-15 | ⏳ PENDIENTE | |
| FE-17 | Panel de Configuración del Motor Adaptativo | BE-17 | ⏳ PENDIENTE | Config de rangos para el profesional |
| FE-18 | Timeline de Ajustes Adaptativos | BE-17 | ⏳ PENDIENTE | Gráficas de evolución |

### Sprint 5 — Gestión de Usuarios, Onboarding y Soporte

| ID | Historia | Depende de (BE) | Estado | Notas |
|----|----------|-----------------|--------|-------|
| FE-19 | Panel de Gestión de Usuarios (Admin) | BE-18 | ✅ HECHO | `/admin/users` listado paginado con filtros, sort, reset password, deactivate, reactivate |
| FE-20 | Onboarding Wizard (Profesional y Familiar) | BE-19 | ⏳ PENDIENTE | `/pro/onboarding/profile`, `/family/onboarding/welcome`, tour guiado |
| FE-21 | Centro de Ayuda y Tickets de Soporte | BE-20 | ⏳ PENDIENTE | `/help` FAQ, `/help/tickets` mis tickets, `/admin/support/*` gestión |

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

## Features extras (no incluidas en HUs originales)

Funcionalidades implementadas que no estaban planificadas en las historias de usuario:

| Feature | Estado | Descripción |
|---------|--------|-------------|
| Auto-registro Profesional (IN-149) | ✅ | Formulario público de registro, validación async de email y matrícula, creación atómica User+Professional. Ver [HU/HU-IN-149-auto-registro-profesional.md](./HU/HU-IN-149-auto-registro-profesional.md) |
| Validación por Administrador (IN-150) | ✅ | Tab "Pendientes" en lista de profesionales, flujo Pending→Approved/Rejected, emails de notificación. Ver [HU/HU-IN-150-validacion-admin.md](./HU/HU-IN-150-validacion-admin.md) |
| CRUD Familiares | ✅ | `FamilyController` + admin UI completa (list/new/edit/detail) |
| CRUD Instituciones Educativas | ✅ | `InstitutionsController` + admin UI completa |
| ABM Catálogos (6 tipos) | ✅ | `CatalogAdminController` + UI con submenú por tipo |
| Panel de Roles y Permisos | ✅ | `RolesController` + UI con checkboxes agrupados por módulo |
| CRUD Admins Institucionales | ✅ | Crear admins vinculados a instituciones desde admin global |
| Admin Global vs Institucional | ✅ | Claim JWT `isGlobalAdmin`, policy `global-admin`, filtrado por institución en profesionales/personas/familiares/invitaciones |
| Email SMTP + Templates | ✅ | MailKit + `EmailTemplateService` con templates HTML para invitaciones (Ethereal para dev) |
| Mi Aula (Portal Profesional) | ✅ | Cards visuales con avatar coloreado de personas asignadas |
| Dashboard Profesional Real | ✅ | Contadores reales (personas, invitaciones) + tablas resumen |
| DataTableComponent Mejorado | ✅ | Título, botones header, debounce 400ms en búsqueda, fila vacía |
| Directivas de Permisos | ✅ | `*appHasPermission`, `*appIfGlobalAdmin`, `*appIfInstitutionalAdmin` |
| Guards de Rutas | ✅ | `globalAdminGuard`, `permissionGuard` en rutas admin |
| Toasts con Colores e Iconos | ✅ | Success (verde), error (rojo), warning (naranja), info (celeste) |
| Accesibilidad CoreUI | ✅ | Variables `--cui-*` sobreescritas por perfil en 14 combinaciones |
| Fix Login Profesional | ✅ | Flujo directo a email+password sin pasar por identificación visual |
| Sidebar Dinámico por Rol | ✅ | Admin global ve todo, institucional ve solo su scope |
| Edición Perfil Funcional (Pro) | ✅ | Profesional edita datos y perfil funcional de sus personas |
| Migraciones | ✅ | AddAdminInstitution, AddPersonSkillProfile |
| IDateTimeProvider (Argentina) | ✅ | Abstracción del reloj del sistema. `ArgentinaDateTimeProvider` (UTC-3, sin DST) inyectado en todos los handlers. Singleton. `UtcNow` para DB, `Now` / `Today` para lógica de negocio local |
| UtcDateTimeConverter | ✅ | Converter JSON global que normaliza `DateTime Kind=Unspecified` a UTC para Npgsql. Evita errores en campos fecha enviados desde el frontend |

---

## Orden de Implementación Recomendado

### Próximo bloque
1. **BE-06** — CRUD Actividades (desbloquea FE-04, FE-05, y es prerequisito de todo lo que viene)
2. **BE-09** — Roadmap (desbloquea FE-07, FE-09)

### Segundo bloque
3. **BE-10** — Asignaciones de actividades
4. **BE-11** — Respuestas y ciclo de vida (desbloquea FE-10, FE-11)
5. **BE-12** — Radar y Dashboard (desbloquea FE-12)

### Tercer bloque
6. **BE-15** — Mensajería Interna (BE-13 y BE-14 ya hechos)
7. **BE-16** — Búsqueda Semántica
8. **BE-17** — Motor Adaptativo (depende de BE-11)
