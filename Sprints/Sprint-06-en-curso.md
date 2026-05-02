# Sprint 6 — En Curso

**Período:** Abril 2026

**Objetivo:** Migración de stack (.NET 10 + PostgreSQL), flujo de auto-registro profesional, documentación y mocks de actividades

---

## Tareas (14 ítems originales + extensión actividades)

Leyenda: ✅ En Revisión · 🔄 En Curso · ⏳ Pendiente · ✔️ Completado fuera de Jira

| Código | Task | Tipo | Estado |
|--------|------|------|--------|
| IN-171 | [Backend] Migrar a PostgreSQL | Task | ✅ En Revisión |
| IN-147 | Migrar .NET 8 a .NET 10 | Story | ✅ En Revisión |
| IN-149 | Implementar flujo de auto-registro (Sign-up) para el rol Profesional | Story | ✅ En Revisión |
| IN-150 | Implementar flujo de selección institucional y validación por Administrador | Story | ✅ En Revisión |
| IN-148 | Implementar permiso de agrupación del núcleo familiar para el rol Profesional | Story | ✅ En Revisión |
| IN-165 | Crear HU Sprint 2 | Task | ✅ En Revisión |
| IN-166 | Crear HU's Sprint 3 | Task | ✅ En Revisión |
| IN-167 | Crear Historias de Usuario para Sprint 1 en Confluence | Task | ✅ En Revisión |
| IN-168 | Documentar procesos en Confluence | Task | ✅ En Revisión |
| IN-169 | Documentar WBS del proyecto | Task | ✅ En Revisión |
| IN-152 | Sprint y cuestionario (armar cuestionario para institución y familia) | Task | ✅ En Revisión |
| IN-170 | [Frontend] Revisar accesibilidad en componentes | Task | ✅ En Revisión |
| IN-151 | Tratar con Mauricio los requerimientos | Story | 🔄 En Curso |
| IN-163 | [Mock] Generar interfaz de alta de actividad | Task | ✔️ Completado |

---

## Extensión Sprint 6 — MVP Actividades (completado fuera de Jira)

| Ítem | Descripción | Estado |
|------|-------------|--------|
| — | pgvector + docker image | ✔️ Completado |
| — | Migración EF AddPgvectorExtension | ✔️ Completado |
| — | Stack vectorización: OnnxRuntime + SentencePiece (paraphrase-multilingual-MiniLM-L12-v2) | ✔️ Completado |
| IN-105 | Activity CRUD backend (handlers + controller + repositorio) | ✔️ Completado |
| IN-106 | Integración ARASAAC (ArasaacService + picker en wizard) | ✔️ Completado |
| IN-107 | Lista actividades con filtros (`/pro/activities`) | ✔️ Completado |
| IN-108 | Edición de actividad propia | ✔️ Completado |
| IN-109 | Desactivación/activación de actividad | ✔️ Completado |
| — | Assignment + Response backend (asignar, start, complete) | ✔️ Completado |
| — | `GET /api/my/activity-assignments` (endpoint para estudiante sin GUID) | ✔️ Completado |
| — | Modal asignar actividad desde lista profesional | ✔️ Completado |
| — | Lista asignaciones estudiante (`/app/activities`) | ✔️ Completado |
| — | ActivityPlayerShell + SELECT_FIGURE player | ✔️ Completado |
| — | Arquitectura dinámica de players: PlayerBaseComponent + registry + NgComponentOutlet + primitivas (PlayerIntro, PlayerResult, PictogramCard) | ✔️ Completado |
| — | Players ORDER_SEQUENCE, MATCH_IMAGE_WORD, VISUAL_SUM | ✔️ Completado |
| — | Tab "Actividades" en person-detail del profesional (historial de asignaciones + resultados) | ✔️ Completado |
| — | ABM 11 Diagnósticos — baja lógica BE + FE (ya existía, fix TS2322 + SCSS faltante) | ✔️ Completado |
| — | Embedding al crear/editar actividad (fire-and-forget en CreateActivity + UpdateActivity handlers) | ✔️ Completado |
| — | Búsqueda semántica: `GET /api/activities/search?text=` + SearchActivitiesSemanticQueryHandler + toggle IA en lista FE | ✔️ Completado |
| IN-110 | Roadmap backend: CreateRoadmap, AddActivity, SetOrder, ManualUnlock, RemoveActivity handlers | ✔️ Completado |
| IN-113 | Drag-drop reorder: ReorderRoadmapActivitiesCommandHandler + PUT .../reorder + CDK drag en FE | ✔️ Completado |
| IN-117 | Vista roadmap estudiante (AacRoadmapComponent) — zigzag estilo Duolingo, señales computadas, enrichedAreas | ✔️ Completado |
| IN-127 | Auto-unlock en CompleteActivityResponseCommandHandler si successPercentage ≥ umbral | ✔️ Completado |
| IN-170 | Accesibilidad players: foco tras phase change, WCAG 4.1.1 letter-slot, aria-labels, role="region" | ✔️ Completado |

---

## Resumen

| Métrica | Valor |
|---------|-------|
| Total tareas originales | 14 |
| En Revisión (For Review) | 13 |
| En Curso (In Progress) | 1 |
| Completadas fuera de Jira | 1 + 22 extensión |

---

## Backlog sin sprint asignado — próximos candidatos

| Código | Task | Dependencia | Estado |
|--------|------|-------------|--------|
| IN-110 | Creación del roadmap por persona | IN-105 ✔️ | ✔️ Completado |
| IN-111 | Agregar actividades al roadmap por área | IN-110 | ✔️ Completado |
| IN-112 | Definir orden secuencial y umbral de desbloqueo | IN-110 | ✔️ Completado |
| IN-113 | Reordenamiento drag-drop | IN-110 | ✔️ Completado |
| IN-117 | Visualización del roadmap (vista estudiante, estilo Duolingo) | IN-110 | ✔️ Completado |
| — | Vista resultados profesional (historial asignaciones por persona) | IN-105 ✔️ | ✔️ Completado |
| — | Búsqueda semántica actividades (handler + endpoint + FE) | modelo listo | ✔️ Completado |
| — | Embedding al crear actividad (fire-and-forget handler) | — | ✔️ Completado |
| IN-86 | Timeline de diagnósticos en perfil de persona (vista admin) | ✅ listo | Cerrar en Jira |
| IN-136 | Creación de reporte de progreso | ✅ listo | Cerrar en Jira |

---

## Épicas padre

- **IN-4:** Gestión de Usuarios
- **IN-6:** Autenticación y Accesibilidad
- **IN-9:** Gestión de Actividades ← en curso
- **IN-10:** Plan de Trabajo (Roadmap) ← próxima prioridad
