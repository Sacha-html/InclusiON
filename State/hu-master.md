# InclusiON — HU Master (Única Fuente de Verdad)

**Última actualización:** 2026-05-03  
**Fuente Jira:** `Sprints/Jira-CSV.csv` (export 2026-05)  
**Reemplaza:** `HU_ESTADO.md` y `progreso-hu.md`

> **Nota:** Muchas HUs están en Backlog en Jira aunque ya implementadas — el equipo no actualizó el tablero. El estado aquí refleja el **estado real del código**, no el estado Jira.

---

## Leyenda

| Símbolo | Significado |
|---------|-------------|
| ✅ | Hecho — Done en Jira + funcionando |
| 🔵 | Implementado — For Review en Jira o código completo |
| 🔄 | En progreso |
| 🔧 | Parcial — entidades/base lista, lógica incompleta |
| ⏳ | Pendiente — no iniciado |

---

## Resumen

| Sprint | HUs | ✅ | 🔵 | 🔧 | ⏳ |
|--------|-----|----|----|----|----|
| Sprint 0 | 7 | 7 | — | — | — |
| Sprint 1 | 15 | — | 15 | — | — |
| Sprint 2 | 29 | — | 29 | — | — |
| Sprint 3 | 16 | — | 16 | — | — |
| Sprint 4 | 14 | — | 14 | — | — |
| Sprint 5 | 2 | — | 2 | — | — |
| Sprint 6 | 7 | — | 6 | — | 1 |
| Sprint 7 | 7 | — | — | — | 7 |
| Sin sprint — implementado | 30 | — | 30 | 0 | — |
| Sin sprint — pendiente | 18 | — | — | — | 18 |
| **Total** | **145** | **7** | **106** | **4** | **28** |

---

## Sprint 0 — Arranque

| IN | Historia | Estado |
|----|----------|--------|
| IN-14 | Definición de roles del equipo | ✅ |
| IN-15 | Elección y prueba de herramientas (Teams, GitHub, VS Code, Figma) | ✅ |
| IN-16 | Creación de repositorios GitHub | ✅ |
| IN-17 | Elaboración del Product Backlog inicial | ✅ |
| IN-18 | Definición de ceremonias Scrum (Daily, Planning, Review, Retro) | ✅ |
| IN-19 | Selección de plataforma tecnológica | ✅ |
| IN-20 | Modelo de datos base iniciales | ✅ |

---

## Sprint 1 — Configuración del Sistema

| IN | Historia | Estado | Notas |
|----|----------|--------|-------|
| IN-21 | Registrar institución | 🔵 | |
| IN-22 | Consultar instituciones | 🔵 | |
| IN-23 | Editar institución | 🔵 | |
| IN-24 | Consultar roles | 🔵 | |
| IN-25 | Asignar permisos por módulo | 🔵 | |
| IN-26 | Crear administrador institucional | 🔵 | |
| IN-27 | Asignar institución a administrador | 🔵 | |
| IN-28 | Filtrar datos por institución | 🔵 | |
| IN-29 | Enforcement de aislamiento por institución (InstitutionAccessFilter) | 🔵 | |
| IN-30 | Confirmar al guardar permisos con aviso de cierre de sesiones | 🔵 | |
| IN-31 | Revocar tokens al cambiar permisos de un rol | 🔵 | |
| IN-32 | Invalidar caché de permisos | 🔵 | |
| IN-33 | Consultar catálogos del sistema (6 tipos) | 🔵 | |
| IN-34 | Registrar ítem en catálogo | 🔵 | |
| IN-35 | Editar ítem en catálogo | 🔵 | |

---

## Sprint 2 — Gestión de Usuarios

| IN | Historia | Estado | Notas |
|----|----------|--------|-------|
| IN-36 | Alta de profesional con contraseña temporal y envío de email | 🔵 | |
| IN-37 | Consulta paginada de profesionales con filtros | 🔵 | |
| IN-38 | Edición de profesional | 🔵 | |
| IN-39 | Desactivación de profesional | 🔵 | |
| IN-40 | Alta de persona con perfil funcional | 🔵 | |
| IN-41 | Consulta paginada de personas con filtros | 🔵 | |
| IN-42 | Edición de datos personales y funcionales de persona | 🔵 | |
| IN-43 | Configuración del método de login con confirm popup | 🔵 | |
| IN-44 | Desactivación de persona (soft-delete + revocación de tokens) | 🔵 | |
| IN-45 | Alta directa de familiar con selector de persona | 🔵 | |
| IN-46 | Alta de familiar por invitación (auto-registro) | 🔵 | |
| IN-47 | Consulta paginada de familiares | 🔵 | |
| IN-48 | Detalle de familiar con personas vinculadas | 🔵 | |
| IN-49 | Edición de familiar | 🔵 | |
| IN-50 | Desactivación de familiar | 🔵 | |
| IN-51 | Vinculación automática persona-familiar en alta directa | 🔵 | |
| IN-52 | Envío de email con contraseña temporal en alta directa de familiar | 🔵 | |
| IN-53 | Crear invitación y enviar email | 🔵 | |
| IN-54 | Validación de código de invitación | 🔵 | |
| IN-55 | Aceptación y registro automático de invitación | 🔵 | |
| IN-56 | Consulta de invitaciones por profesional | 🔵 | |
| IN-57 | Consulta de invitaciones por admin | 🔵 | |
| IN-58 | Asignar profesional a institución | 🔵 | |
| IN-59 | Desasignar profesional de institución | 🔵 | |
| IN-60 | Asignar persona a profesional | 🔵 | |
| IN-61 | Desactivar asignación persona-profesional | 🔵 | |
| IN-62 | Vinculación familiar automática por invitación | 🔵 | |
| IN-63 | Configuración de perfil de habilidades (selección múltiple) | 🔵 | |
| IN-64 | Desvinculación lógica (soft-delete) de asignaciones | 🔵 | |

---

## Sprint 3 — Autenticación y Accesibilidad

| IN | Historia | Estado | Notas |
|----|----------|--------|-------|
| IN-65 | Login estándar (email + contraseña) | 🔵 | |
| IN-66 | Login visual estándar (identificación por nombre + contraseña) | 🔵 | |
| IN-67 | Login por PIN (4 dígitos) | 🔵 | |
| IN-68 | Login asistido (supervisor autoriza) | 🔵 | |
| IN-69 | Login familiar | 🔵 | |
| IN-70 | Identificación de usuario por nombre | 🔵 | |
| IN-71 | Refresh de token automático | 🔵 | |
| IN-72 | Cambio de contraseña obligatorio en primer login | 🔵 | |
| IN-73 | Redirección por rol al portal correspondiente | 🔵 | |
| IN-74 | Validación de rol en login admin/profesional (allowedRoles) | 🔵 | |
| IN-75 | 7 perfiles visuales de accesibilidad | 🔵 | alto contraste, dislexia, low-vision, deuteranopia, protanopia, tritanopia |
| IN-76 | Modo claro y oscuro (14 combinaciones) | 🔵 | |
| IN-77 | Panel de accesibilidad (Alt+A) | 🔵 | |
| IN-78 | Toasts con colores de accesibilidad | 🔵 | |
| IN-79 | Guards de ruta por rol y permiso con toast de aviso | 🔵 | |
| IN-80 | Directivas de permisos en interfaz | 🔵 | |

---

## Sprint 4 — Evaluación, Dashboard y Usuarios

| IN | Historia | Estado | Notas |
|----|----------|--------|-------|
| IN-81 | Configuración del perfil de habilidades | 🔵 | |
| IN-82 | Edición del perfil funcional | 🔵 | |
| IN-83 | Registro de diagnóstico funcional | 🔵 | |
| IN-84 | Consulta de historial de diagnósticos (lista por fecha desc) | 🔵 | |
| IN-85 | Edición de diagnóstico por su creador | 🔵 | |
| IN-87 | Dashboard del profesional con contadores reales | 🔵 | |
| IN-88 | Mi Aula (cards de personas asignadas) | 🔵 | |
| IN-93 | Listado centralizado de usuarios con filtros (rol, estado, institución) | 🔵 | |
| IN-94 | Detalle de usuario con entidad asociada | 🔵 | |
| IN-95 | Reseteo de contraseña | 🔵 | |
| IN-96 | Desactivación de cuenta (soft-delete + revocación de tokens) | 🔵 | |
| IN-97 | Reactivación de cuenta (genera temporal + envío email) | 🔵 | |
| IN-103 | Consulta de tipos de template (catálogo) | 🔵 | |
| IN-104 | Consulta de categorías de actividad (catálogo) | 🔵 | |

---

## Sprint 5 — Reportes

| IN | Historia | Estado | Notas |
|----|----------|--------|-------|
| IN-138 | Consultar reportes como familiar | 🔵 | |
| IN-164 | Consultar reportes como profesional | 🔵 | |

---

## Sprint 6 — En Curso

| IN | Historia | Estado | Notas |
|----|----------|--------|-------|
| IN-86 | Timeline de diagnósticos en perfil de persona (Profesional) | 🔵 | Filtro por fecha, computed() client-side |
| IN-148 | Permiso de agrupación del núcleo familiar | 🔵 | |
| IN-149 | Auto-registro (Sign-up) para rol Profesional | 🔵 | Validación async email + matrícula |
| IN-150 | Selección institucional y validación por administrador | 🔵 | Pending→Approved/Rejected + emails |
| IN-151 | Tratar con Mauricio los requerimientos | 🔄 | En progreso — reunión de relevamiento |
| IN-152 | Sprint y cuestionario | 🔵 | |
| IN-172 | Autorización por recurso (row-level authorization) | 🔵 | 4 fases, 28 tests. Ver HU/HU-IN-172 |

---

## Sprint 7 — Documentación (En Curso)

> Tasks de documentación — todas en Backlog en Jira.

| IN | Tarea | Estado | Notas |
|----|-------|--------|-------|
| IN-179 | Ceremonias: Daily y retro | ⏳ | Documentar ceremonias Scrum del equipo |
| IN-180 | Diccionario de datos | ⏳ | Ya existe `diccionario-datos.md` — pasar a Confluence |
| IN-181 | Listados de ABMs | ⏳ | Ya existe `ABM/` completo — revisar y actualizar |
| IN-182 | Listado de Casos de Uso | ⏳ | Ya existe `Process/` — alinear con HUs |
| IN-183 | Pasar DER a Confluence | ⏳ | Usar `Docs/der.md` como base |
| IN-184 | Revisar Sprint 1 | ⏳ | Revisión de documentación Sprint 1 |
| IN-185 | Revisar ABMs (Test) | ⏳ | Validar ABMs contra código implementado |

---

## Sin Sprint — Implementado (Jira Backlog desactualizado)

> Implementadas en código pero nunca movidas en Jira.

| IN | Historia | Estado | Notas |
|----|----------|--------|-------|
| IN-89 | Detalle de persona con edición inline | 🔵 | Click por campo → input → blur/Enter guarda, Escape cancela |
| IN-98 | Consulta de actividad reciente del usuario | 🔵 | Historial de login via refresh tokens — `GET /admin/users/{id}/activity` |
| IN-136 | Creación de reporte de progreso (tipo, período, contenido) | 🔵 | Jira Backlog — implementado |
| IN-91 | Dashboard familiar (últimas actividades, mensajes, reportes) | 🔵 | |
| IN-92 | Portal familia con progreso completo | 🔵 | |
| IN-105 | Creación de actividad con wizard (área, template, contenido, metadatos) | 🔵 | |
| IN-106 | Integración de pictogramas ARASAAC | 🔵 | |
| IN-107 | Consulta del catálogo de actividades (propias + estándar) | 🔵 | Búsqueda semántica IA, filtros, paginación |
| IN-108 | Edición de actividad propia | 🔵 | |
| IN-109 | Desactivación de actividad | 🔵 | Valida asignaciones activas |
| IN-110 | Creación del roadmap por persona | 🔵 | |
| IN-111 | Agregar actividades al roadmap por área | 🔵 | |
| IN-112 | Definir orden secuencial y umbral de desbloqueo | 🔵 | |
| IN-113 | Reordenamiento de actividades drag-drop | 🔵 | CDK drag-drop + PUT /areas/{id}/activities/reorder |
| IN-114 | Desbloqueo manual de actividad | 🔵 | |
| IN-115 | Eliminación de actividad del roadmap | 🔵 | |
| IN-117 | Visualización del roadmap (vista estudiante, estilo Duolingo) | 🔵 | |
| IN-118 | Carga de asignación con contenido completo | 🔵 | |
| IN-119 | Inicio de actividad (registro de respuesta) | 🔵 | ActivityPlayerShell despacha por templateTypeCode |
| IN-120 | Player: Selección de figuras | 🔵 | Completo: intro→playing→result, ARASAAC |
| IN-121 | Player: Suma visual | 🔵 | Completo: intro→playing→result, editor con ARASAAC opcional, registry wired |
| IN-122 | Player: Emparejar imagen-palabra | 🔵 | Completo: intro→playing→result, editor con ARASAAC por par, undo, registry wired |
| IN-123 | Player: Ordenar secuencia | 🔵 | Completo: intro→playing→result, editor drag-like con ▲▼, ARASAAC por ítem, registry wired |
| IN-124 | Player: Completar letra | 🔵 | Completo: intro→playing→result, editor con toggle de huecos y distractores, registry wired |
| IN-126 | Completar actividad y evaluar resultado | 🔵 | CompleteActivityResponseCommandHandler, calcula successPercentage |
| IN-127 | Desbloqueo automático si supera umbral | 🔵 | |
| IN-135 | Búsqueda semántica de actividades por lenguaje natural | 🔵 | ONNX embeddings, endpoint GET /api/activities/search |
| IN-140 | Bandeja de entrada de mensajes | 🔵 | |
| IN-141 | Envío de mensajes con asunto y contenido | 🔵 | |
| IN-142 | Hilos de conversación (respuestas) | 🔵 | |
| IN-143 | Indicador de mensajes no leídos en sidebar | 🔵 | Badge dinámico en layout |
| IN-144 | Marcado automático como leído al abrir | 🔵 | |

---

## Sin Sprint — Pendiente (Post-MVP)

| IN | Historia | Notas |
|----|----------|-------|
| IN-90 | Radar chart de habilidades (promedio de éxito por área) | Post-MVP |
| IN-99 | Wizard de completado de perfil (profesional) | Post-MVP |
| IN-100 | Tour guiado del portal (profesional) | Post-MVP |
| IN-101 | Pantalla de bienvenida (familiar) | Post-MVP |
| IN-102 | Pantalla de bienvenida (persona con discapacidad) | Post-MVP |
| IN-116 | Configuración del motor adaptativo por actividad | Post-MVP — MDA |
| IN-125 | Registro de progreso durante ejecución (intentos, frustración) | Post-MVP |
| IN-128 | Monitoreo de frustración (pausa tras 3+ intentos) | Post-MVP — MDA |
| IN-129 | Evaluación automática de rendimiento | Post-MVP — MDA |
| IN-130 | Cálculo de ajuste según estado (Estable/Progresando/Dificultad) | Post-MVP — MDA |
| IN-131 | Aplicación de ajuste dentro de rangos configurados | Post-MVP — MDA |
| IN-132 | Registro de cada ajuste en historial de auditoría | Post-MVP — MDA |
| IN-133 | Alerta al profesional en estado de frustración | Post-MVP — MDA |
| IN-134 | Consulta del historial de ajustes (timeline) | Post-MVP — MDA |
| IN-139 | Exportación de reporte a PDF | Post-MVP |
| IN-145 | Notificaciones automáticas de eventos del sistema | Post-MVP |
| IN-153 | Panel de visualización de progreso y reportes (familiar) | Post-MVP |
| IN-154 | Canal de sugerencias sobre perfiles de accesibilidad | Post-MVP |

---

## Sin HU Jira — Implementado

Features implementadas sin Story formal en Jira:

| Feature | Estado | Notas |
|---------|--------|-------|
| Hardening de Seguridad | 🔵 | Argon2id para PINs, AES-256-GCM datos clínicos, rate limiting auth. Ver `HU/HU-IN-173-hardening-seguridad.md`. **Sin HU Jira** — IN-173 en Jira es tarea de docs. Pendiente crear Story. |
| IDateTimeProvider Argentina | 🔵 | Infraestructura — UTC-3 sin DST, singleton. Sin HU propia. |
| UtcDateTimeConverter | 🔵 | Infraestructura — normaliza DateTime Kind=Unspecified a UTC. Sin HU propia. |

---

## Epics de Referencia

| IN | Epic | Sprints relacionados |
|----|------|---------------------|
| IN-2 | Planificación del inicio | Sprint 0 |
| IN-3 | Configuración del Sistema | Sprint 1 |
| IN-4 | Gestión de Usuarios | Sprint 2 |
| IN-5 | Invitaciones y Asignaciones | Sprint 2 |
| IN-6 | Autenticación y Accesibilidad | Sprint 3 |
| IN-7 | Evaluación, Diagnóstico y Dashboard | Sprint 4 |
| IN-8 | Administración de Cuentas y Onboarding | Sprint 5 |
| IN-9 | Gestión de Actividades | Sin sprint (Backlog) |
| IN-10 | Plan de Trabajo (Roadmap) | Sin sprint (Backlog) |
| IN-11 | Resolución de Actividades | Sin sprint (Backlog) |
| IN-12 | Motor Adaptativo (MDA) y Reportes | Sin sprint (Backlog) |
| IN-13 | Mensajería y Portal Familiar | Sin sprint (Backlog) |
