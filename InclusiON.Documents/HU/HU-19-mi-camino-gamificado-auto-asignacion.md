# HU-19 — "Mi Camino" Gamificado, Aislamiento de Plantillas y Auto-Asignación

| Campo | Contenido |
|---|---|
| ID | HU-19 |
| Épica | Portal del Alumno & Gamificación de Trayectoria |
| Título | Visualización de "Mi Camino" (10 Niveles), Aislamiento de Plantillas de Profesionales y Auto-Asignación para Ejecución |
| Prioridad | Alta |
| Estimación | 8 puntos de historia |
| Sprint asignado | Sprint 12 |
| Estado | Completada |
| Códigos Jira | IN-320, IN-321, IN-322, IN-323 |

---

## Historia de Usuario 1 — Aislamiento de Datos de Actividades del Roadmap (IN-320)

**Como** profesional de la plataforma  
**Quiero** que las actividades oficiales del Roadmap no aparezcan en mi grilla de trabajo diario "Mis Actividades" ni en los botones de gestión de plantillas  
**Para** mantener limpio mi panel de trabajo y evitar la mezcla de plantillas globales del sistema con mis actividades personalizadas.

### Criterios de Aceptación
- [x] Las 10 actividades oficiales del Roadmap se configuran como plantillas globales (`IsTemplate = true`, `RoadmapOrder` definido y `ProfessionalId = null`).
- [x] El repositorio de backend (`ActivitiesRepository.cs`) filtra estrictamente las actividades de profesionales para que solo retorne `ProfessionalId == currentUserId && !IsTemplate`.
- [x] Se eliminan los botones "Ver Roadmap" y "Biblioteca de Plantillas" de la cabecera de "Mis Actividades" en el portal del profesional.
- [x] Las rutas de templates y roadmap se eliminan del módulo de navegación y enrutamiento del profesional.

---

## Historia de Usuario 2 — Experiencia Gamificada "Mi Camino" en Portal Alumno (IN-321)

**Como** alumno (persona con discapacidad)  
**Quiero** recorrer una trayectoria de aprendizaje gamificada de 10 niveles con diseño violeta, nodos circulares en zigzag, conectores luminosos y anillos de pulso  
**Para** motivar mi progreso pedagógico y acceder a mis actividades de manera intuitiva y adaptada.

### Criterios de Aceptación
- [x] Se reactiva la ruta `/app/roadmap` en el portal del Alumno con el diseño estético nativo violeta (`#673AB7` / `#5C6BC0`).
- [x] El componente `AacRoadmapComponent` muestra los 10 niveles oficiales en zigzag preservando las clases visuales (`.pulse-ring`, `.activity-node`, `.activity-connector`, etc.).
- [x] El Nivel 1 siempre está desbloqueado para iniciar el aprendizaje.
- [x] Cada nivel subsiguiente se desbloquea automáticamente si el nivel anterior fue superado con un puntaje mayor o igual al **60%** de aciertos.
- [x] Los niveles completados muestran el indicador de éxito (`✓` y porcentaje), permitiendo al alumno volver a jugarlos para mejorar su puntaje.

---

## Historia de Usuario 3 — Flujo de Auto-Asignación Transaccional para el Reproductor (IN-322)

**Como** alumno  
**Quiero** que al hacer clic en un nivel del Roadmap se genere o recupere automáticamente una asignación personal (`ActivityAssignment`)  
**Para** jugar directamente sin errores de permisos (403) ni recursos no encontrados (404) y registrar mi progreso en la base de datos.

### Criterios de Aceptación
- [x] Se implementa el endpoint backend `POST /api/activity-assignments/auto-assign/{activityId}` en `ActivityAssignmentsController`.
- [x] Si el alumno ya cuenta con una asignación activa (no cancelada) para la actividad seleccionada, el backend la devuelve inmediatamente.
- [x] Si no existe asignación previa, el backend crea un nuevo registro `ActivityAssignment` vinculado a su `PersonId` y a un profesional del sistema.
- [x] El reproductor (`ActivityPlayerShellComponent`) recibe un `assignmentId` encriptado válido y carga con código HTTP `200 OK`.
- [x] Al iniciar y terminar la actividad, `player-base.component.ts` persiste los resultados y tiempos a través de `activitiesService.completeResponse()`.

---

## Historia de Usuario 4 — Filtrado Exclusivo de "Mis Actividades" en Portal Alumno (IN-324)

**Como** alumno (persona con discapacidad)  
**Quiero** que al ingresar a la solapa "Mis Actividades" solo se muestren las tareas asignadas directamente por mi profesor a cargo  
**Para** no confundir las tareas personalizadas de clase con los niveles globales de la trayectoria de "Mi Camino".

### Criterios de Aceptación
- [x] El listado de asignaciones en `aac-activities.component.ts` filtra reactivamente las actividades descartando plantillas (`!a.isTemplate && a.roadmapOrder == null`).
- [x] Los niveles del Roadmap no aparecen duplicados en la grilla de actividades asignadas.
- [x] Si el profesor crea una actividad personalizada y la asigna al alumno, esta se muestra de inmediato con su estado (Pendiente, En Progreso, Completada).

---

## Historia de Usuario 5 — Catálogo Colaborativo Compartido entre Profesionales (IN-325)

**Como** profesional de la institución  
**Quiero** visualizar en mi catálogo de actividades todas las actividades creadas por cualquier colega profesional  
**Para** reutilizar recursos pedagógicos, ahorrar tiempo de diseño y asignar actividades enriquecidas a mis alumnos.

### Criterios de Aceptación
- [x] `ActivitiesRepository.cs` en `GetPagedAsync` retorna todas las actividades activas creadas por cualquier profesional (`!a.IsTemplate`).
- [x] La columna "Creado por" (`authorName`) identifica con nombre y apellido al profesional autor de cada actividad.
- [x] Cualquier profesional puede consultar el detalle (`GetActivityByIdQueryHandler.cs`) y asignar (`CreateActivityAssignmentCommandHandler.cs`) cualquier actividad del catálogo a sus alumnos a cargo.

---

## Casos de Borde y Escenarios de Resiliencia (IN-323)

### Caso de Borde 1: Alumno sin Profesional Directamente Vinculado
* **Escenario:** Un alumno creado recientemente aún no tiene registros en la tabla `ProfessionalPersons`.
* **Comportamiento implementado:** `AutoAssignActivityCommandHandler` consulta los profesionales activos del sistema (`GetAllActiveAsync` / `GetPagedAsync`) y selecciona automáticamente un profesional activo como fallback seguro en `AssignedByProfessionalId`, evitando excepciones de clave foránea o bloqueos de ejecución.

### Caso de Borde 2: Reingreso a Nivel Ya Jugado o en Progreso
* **Escenario:** El alumno hace clic en un nivel que ya completó o que dejó en progreso.
* **Comportamiento implementado:** El backend busca primero asignaciones existentes `StatusId != Cancelada` para esa combinación `PersonId` + `ActivityId`. Si existe, devuelve la asignación existente sin duplicar registros en base de datos.

### Caso de Borde 3: Alumno Accediendo a Actividad Plantilla Directa
* **Escenario:** Consulta de detalle de actividad (`GET /api/Activities/{id}`) por parte de un usuario con rol `PersonWithDisability` (donde `_httpContextService.GetCurrentEntityId()` no es un profesional).
* **Comportamiento implementado:** En `GetActivityByIdQueryHandler`, se permite la lectura a cualquier usuario autenticado si la actividad es una plantilla (`activity.IsTemplate == true`) o estándar (`activity.IsStandardActivity == true`).

### Caso de Borde 4: Sincronización Híbrida de Progreso (Caché Local + Base de Datos)
* **Escenario:** El alumno completa una actividad y regresa a la vista de "Mi Camino".
* **Comportamiento implementado:** `AacRoadmapComponent` y `PlayerBaseComponent` evalúan tanto las respuestas registradas en base de datos como las guardadas en `localStorage` (`roadmap_progress_{activityId}`). Si cualquiera de las dos fuentes confirma un puntaje $\ge 60\%$, el nivel se marca como superado y se desbloquea de inmediato el siguiente nivel en la interfaz.

### Caso de Borde 5: Manejo de Nivel Bloqueado
* **Escenario:** El alumno intenta hacer clic en un nivel bloqueado (ej. Nivel 5 sin haber superado el Nivel 4).
* **Comportamiento implementado:** La acción `onNodeClick` intercepta la llamada, comprueba `node.status === 'locked'` y no dispara peticiones de red ni navegación, mostrando visualmente el icono de candado (`🔒`) y manteniendo la accesibilidad con `aria-label`.
