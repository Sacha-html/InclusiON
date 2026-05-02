# InclusiON — Estado de Historias de Usuario

**Última actualización:** 2026-05-01

Leyenda de estados:
- ✅ **HECHO** — Implementado y funcionando
- 🔧 **PARCIAL** — Entidades/migraciones existen pero falta lógica de negocio
- ⏳ **PENDIENTE** — No iniciado

---

## Resumen Rápido

| Track | Hechas | Parciales | Pendientes | Total |
|-------|--------|-----------|------------|-------|
| Backend | 9 | 2 | 9 | 20 |
| Frontend | 9 | 0 | 12 | 21 |

---

## Track Backend (BE)

### Sprint 1 — Bases

| Historia | Estado | Notas |
|----------|--------|-------|
| Catálogos de Referencia (lectura) | ✅ HECHO | `CatalogsController` + queries + handlers para disability-types, autonomy-levels, activity-categories, skill-areas, activity-template-types |
| CRUD Profesionales | ✅ HECHO | `ProfessionalsController` + Create/Update/Deactivate commands + GetAll/GetById queries |
| Asignaciones Profesional-Institución-Persona | ✅ HECHO | `AssignmentsController` + `InstitutionsController` + handlers + repos para asignaciones profesional-persona-institución |

### Sprint 2 — Actividades y Roadmap

| Historia | Estado | Notas |
|----------|--------|-------|
| Áreas de Habilidad (SkillAreas) | ✅ HECHO | Incluido en catálogos como `GET /api/catalogs/skill-areas` |
| Tipos de Template de Actividad | ✅ HECHO | Incluido en catálogos como `GET /api/catalogs/activity-template-types` |
| CRUD Actividades con Contenido Dinámico | ✅ HECHO | `ActivitiesController` + Create/Update/PatchStatus commands + GetAll/GetById/SearchSemantic queries + handlers. Búsqueda semántica con ONNX embeddings. Permisos por ownership (propias) + lectura estándar. |
| Perfil de Habilidades de la Persona | ✅ HECHO | `PersonSkillProfile` entity + endpoints GET/POST/PUT en PersonsController |
| Sistema de Invitaciones Familiares | ✅ HECHO | `InvitationsController` con 4 endpoints + email SMTP con MailKit + templates HTML |
| Roadmap de la Persona | ✅ HECHO | `RoadmapController` + Create/AddArea/AddActivity/RemoveArea/RemoveActivity/UnlockActivity/UpdateNotes commands + GetPersonRoadmap query. `PersonAccess` enforced. Actividad 1 auto-desbloqueada. Validación ownership actividad. Pendiente: drag-drop reorder (IN-113). |

### Sprint 3 — Ejecución y Métricas

| Historia | Estado | Notas |
|----------|--------|-------|
| Asignaciones — Consulta y Gestión | ⏳ PENDIENTE | Entidad `ActivityAssignment` existe. Sin handlers |
| Respuestas — Ciclo de Vida de Actividad | ⏳ PENDIENTE | Entidad `ActivityResponse` existe. Sin handlers |
| Radar Chart y Dashboard con Datos Reales | ⏳ PENDIENTE | Sin implementación |

### Sprint 4 — Avanzados

| Historia | Estado | Notas |
|----------|--------|-------|
| Diagnósticos Funcionales | ✅ HECHO | `DiagnosesController` + Create/Update commands + GetAll/GetById queries + `DiagnosesRepository`. Endpoints: GET /persons/{id}/diagnoses, GET /diagnoses/{id}, POST, PUT |
| Reportes de Progreso | ✅ HECHO | Flujo completo: Draft→Submitted→Approved/Rejected. Endpoints: GET, POST, PUT, PATCH submit/approve/reject, GET /family. Emails a familiar (aprobación) y profesional (rechazo). Ver [Features/reportes-flujo-aprobacion.md](./Features/reportes-flujo-aprobacion.md) |
| Mensajería Interna | ⏳ PENDIENTE | Entidad `Message` existe. Sin handlers |
| Búsqueda Semántica de Actividades | 🔧 PARCIAL | Entidades `ActivityEmbedding`, `ActivityResult` con migración. Library `SemanticSearch` existe. Falta: interfaces en Application, handler, endpoint. Ver [Features/integracion-semantic-search.md](./Features/integracion-semantic-search.md) |
| Motor de Dificultad Adaptativa (MDA) | 🔧 PARCIAL | Entidades `AdaptiveEngineConfig`, `AdaptiveAdjustmentLog` con migración. Falta: `IAdaptiveEngineService`, implementación, pipeline steps. Ver [Features/MDA_Especificacion_Tecnica.md](./Features/MDA_Especificacion_Tecnica.md) |

### Sprint 5 — Gestión de Usuarios, Onboarding y Soporte

| Historia | Estado | Notas |
|----------|--------|-------|
| Gestión Centralizada de Usuarios | ✅ HECHO | `AdminUsersController` + handlers: listado paginado (raw SQL), reset password, deactivate, reactivate. Proceso 17 |
| Onboarding de Usuarios | ⏳ PENDIENTE | Completar perfil profesional, flags onboarding, endpoint completado. Proceso 18 |
| Soporte y Ayuda | ⏳ PENDIENTE | FAQ CRUD, tickets CRUD, respuestas. Proceso 19 |

---

## Track Frontend (FE)

### Sprint 1 — Bases

| Historia | Estado | Notas |
|----------|--------|-------|
| CatalogsService y Conexión con Form de Persona | ✅ HECHO | `persons.service.ts` conectado, formularios funcionando |
| Listado y Formulario de Profesionales | ✅ HECHO | Vistas en `views/professional/` |
| Asignaciones Institución/Persona en Detalle Profesional | ✅ HECHO | Sección personas a cargo e instituciones en detalle profesional con tabs |

### Sprint 2 — Actividades y Roadmap

| Historia | Estado | Notas |
|----------|--------|-------|
| Formulario de Actividad con Template Dinámico | ✅ HECHO | `new.component` + `edit.component` en `/pro/activities`. Wizard multi-paso con ARASAAC, template dinámico SelectFigure. Rutas protegidas por `permissionGuard`. |
| Catálogo de Actividades del Profesional | ✅ HECHO | `list.component` con filtros, búsqueda semántica IA, asignación modal, activar/desactivar. Permisos `activities:create/update` aplicados en UI. |
| Sección Perfil de Habilidades en Perfil Persona | ✅ HECHO | Sección perfil de habilidades con chips coloreados en detalle persona |
| Gestor de Roadmap (vista Profesional) | ✅ HECHO | Tab "Hoja de Ruta" en `person-detail`. Crear roadmap, agregar/eliminar áreas y actividades, desbloqueo manual. Permisos `roadmap:create/update/delete` aplicados. Pendiente: drag-drop reorder (IN-113). |
| Registro por Invitación (página pública Familia) | ✅ HECHO | Registro por invitación (página pública /invite/:code) |

### Sprint 3 — Experiencia del Estudiante

| Historia | Estado | Notas |
|----------|--------|-------|
| Roadmap Visual (vista Estudiante, estilo Duolingo) | ⏳ PENDIENTE | AAC portal, componente principal |
| ActivityPlayerShell (cargador dinámico) | ⏳ PENDIENTE | Shell que carga el player correcto según template |
| Activity Players (5 componentes) | ⏳ PENDIENTE | Selección, emparejamiento, secuencia, completar, respuesta libre |
| Radar Chart de Habilidades | ⏳ PENDIENTE | Gráfica radar por persona |
| Dashboards con Datos Reales | ✅ HECHO | Dashboard profesional con datos reales (personas, invitaciones, saludo personalizado) |

### Sprint 4 — Avanzados

| Historia | Estado | Notas |
|----------|--------|-------|
| Timeline de Diagnósticos y Formulario (IN-86) | ✅ HECHO | Timeline + formulario create/edit en tab del perfil de persona. Filtro por fecha con `computed()` client-side. |
| Creación de Reportes y Vista Familia (IN-138) | ✅ HECHO | Profesional: lista + alta con modal + submit. Admin: lista global + tab en detalle profesional. Familiar: lista + detalle en `/family/reports`. |
| Mensajería Interna — Inbox y Redactar | ⏳ PENDIENTE | Requiere Mensajería en backend |
| Panel de Configuración del Motor Adaptativo | ⏳ PENDIENTE | Config de rangos para el profesional |
| Timeline de Ajustes Adaptativos | ⏳ PENDIENTE | Gráficas de evolución |

### Sprint 5 — Gestión de Usuarios, Onboarding y Soporte

| Historia | Estado | Notas |
|----------|--------|-------|
| Panel de Gestión de Usuarios (Admin) | ✅ HECHO | `/admin/users` listado paginado con filtros, sort, reset password, deactivate, reactivate |
| Onboarding Wizard (Profesional y Familiar) | ⏳ PENDIENTE | `/pro/onboarding/profile`, `/family/onboarding/welcome`, tour guiado |
| Centro de Ayuda y Tickets de Soporte | ⏳ PENDIENTE | `/help` FAQ, `/help/tickets` mis tickets, `/admin/support/*` gestión |

---

---

## Sprint 6 — En Curso (2026-04-18)

### Features extras Sprint 6

| ID | Historia | Estado | Notas |
|----|----------|--------|-------|
| IN-86 | Timeline de Diagnósticos en Perfil de Persona (Profesional) | ✅ HECHO | Timeline + formulario create/edit como tab en detalle de persona. Filtro por fecha (desde/hasta) con `computed()` client-side. Contador de resultados filtrados. Ver [HU/HU-IN-86-timeline-diagnosticos.md](./HU/HU-IN-86-timeline-diagnosticos.md) |
| IN-173 | Hardening de Seguridad de Datos Sensibles | ✅ HECHO | Rate limiting auth (PIN 5/5min, login 10/min, refresh 20/min). Argon2id para PINs con migración lazy desde BCrypt. Cifrado AES-256-GCM en datos clínicos con `[Encrypted]` annotation automática (Diagnosis, Report, ActivityResponse, ActivityResult). `SensitiveDataEncryptor` al arranque. 21 unit tests. Ver [HU/HU-IN-173-hardening-seguridad.md](./HU/HU-IN-173-hardening-seguridad.md) |

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
| Autorización por Recurso (IN-172) | ✅ | Row-level authorization completo (4 fases): `IResourceAuthorizationService` + `AccessAudit` + filtros declarativos `[PersonAccess]` / `[DiagnosisAccess]` / `[ReportAccess]` + 28 tests (5 unit + 23 integración). Frontend: `authInterceptor` redirige al dashboard del rol en 403. Ver [HU/HU-IN-172-autorizacion-por-recurso.md](./HU/HU-IN-172-autorizacion-por-recurso.md) |
| Hardening de Seguridad (IN-173) | ✅ | Rate limiting por IP en endpoints auth. Argon2id (OWASP) para PINs + migración lazy desde BCrypt legacy. AES-256-GCM para datos clínicos vía `[Encrypted]` annotation + EF Core value converter automático. `SensitiveDataEncryptor` idempotente al arranque. 21 unit tests nuevos. Ver [HU/HU-IN-173-hardening-seguridad.md](./HU/HU-IN-173-hardening-seguridad.md) |
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
1. **CRUD Actividades** (IN-105..IN-109) — prerequisito de todo el flujo educativo
2. **Roadmap** (IN-110..IN-113) — plan de trabajo por persona

### Segundo bloque
3. **Asignaciones de actividades** — consulta y gestión
4. **Respuestas y ciclo de vida** — ejecutar actividad, registrar resultado
5. **Radar y Dashboard** — visualización de progreso

### Tercer bloque
6. **Mensajería Interna** — inbox profesional ↔ familia
7. **Búsqueda Semántica**
8. **Motor de Dificultad Adaptativa** — depende de respuestas y roadmap
