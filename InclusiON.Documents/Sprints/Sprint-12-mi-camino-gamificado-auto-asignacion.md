# Sprint 12 — Aislamiento de Plantillas, "Mi Camino" Gamificado y Auto-Asignación

**Período:** Agosto 2026

**Objetivo:** Establecer el aislamiento estricto de datos de actividades del profesional eliminando plantillas del panel diario, restaurar la trayectoria gamificada de 10 niveles ("Mi Camino") en el portal del alumno preservando la estética nativa violeta e implementar el flujo de auto-asignación transaccional para eliminar errores 404/403 en el reproductor.

---

## Tareas

### Aislamiento de Plantillas y Limpieza Profesional (HU-19 / IN-320)

| Código | Task | Backend | Frontend | Estado |
|--------|------|---------|----------|--------|
| IN-320a | Migración de actividades oficiales del Roadmap a plantillas globales (`ProfessionalId = null`, `IsTemplate = true`) | Scripts DB / Migración | — | ✅ DONE |
| IN-320b | Filtrado estricto en repositorio para profesionales (`ProfessionalId == currentId && !IsTemplate`) | `ActivitiesRepository.cs` | — | ✅ DONE |
| IN-320c | Purga de botones "Ver Roadmap" y "Biblioteca de Plantillas" en vista de profesional | — | `activities-list.component.html` | ✅ DONE |
| IN-320d | Eliminación de rutas residuales de templates y roadmap en módulo profesional | — | `professional.routes.ts` | ✅ DONE |

---

### "Mi Camino" Gamificado (Portal Alumno) (HU-19 / IN-321)

| Código | Task | Backend | Frontend | Estado |
|--------|------|---------|----------|--------|
| IN-321a | Restauración de interfaz nativa violeta (`#673AB7`) con nodos circulares y zigzag | — | `aac-roadmap.component.ts` / `.html` / `.scss` | ✅ DONE |
| IN-321b | Integración dinámica de los 10 niveles oficiales del Roadmap vía `GET /api/Activities/roadmap` | `ActivitiesController.cs` | `activities.service.ts` | ✅ DONE |
| IN-321c | Regla de desbloqueo progresivo basado en umbral de éxito del 60% | — | `aac-roadmap.component.ts` | ✅ DONE |
| IN-321d | Soporte para repetición de niveles completados para mejora de puntaje | — | `aac-roadmap.component.ts` | ✅ DONE |

---

### Auto-Asignación y Reproductor de Actividades (HU-19 / IN-322 & IN-323)

| Código | Task | Backend | Frontend | Estado |
|--------|------|---------|----------|--------|
| IN-322a | Endpoint de auto-asignación `POST /api/activity-assignments/auto-assign/{activityId}` | `ActivityAssignmentsController.cs` | `activities.service.ts` | ✅ DONE |
| IN-322b | Comando y handler de auto-asignación con resolución de profesional fallback | `AutoAssignActivityCommand.cs`, `AutoAssignActivityCommandHandler.cs` | — | ✅ DONE |
| IN-322c | Flexibilización de lectura de actividades plantilla para alumnos (`IsTemplate = true`) | `GetActivityByIdQueryHandler.cs`, `ActivitiesController.cs` | — | ✅ DONE |
| IN-322d | Redirección sin errores a `ActivityPlayerShellComponent` con ID de asignación real | — | `aac-roadmap.component.ts` | ✅ DONE |
| IN-322e | Persistencia híbrida de progreso (Caché local + guardado en DB) | — | `player-base.component.ts` | ✅ DONE |

---

### Catálogo Compartido y Filtrado Exclusivo (HU-19 / IN-324 & IN-325)

| Código | Task | Backend | Frontend | Estado |
|--------|------|---------|----------|--------|
| IN-324 | Filtrado reactivo en "Mis Actividades" del alumno para mostrar únicamente las tareas de su profesor | `ActivityAssignmentResponse.cs` | `aac-activities.component.ts` | ✅ DONE |
| IN-325a | Catálogo compartido colaborativo visible para todos los profesionales de la institución | `ActivitiesRepository.cs` | `list.component.ts` | ✅ DONE |
| IN-325b | Habilitación de consulta de detalle y asignación cruzada entre profesionales | `GetActivityByIdQueryHandler.cs`, `CreateActivityAssignmentCommandHandler.cs` | `list.component.ts` | ✅ DONE |

---

## Resumen del Sprint

1. **Aislamiento Estricto de Datos:** Los profesionales no ven contaminada su grilla con las 10 actividades oficiales del sistema y no tienen acceso a rutas de edición de plantillas globales.
2. **"Mi Camino" Intuitivo y Accesible:** Los alumnos disfrutan de una experiencia de gamificación con estética violeta nativa, animaciones de pulso y avance progresivo al superar cada nivel con 60% o más de aciertos.
3. **Resiliencia y Cero Errores 404/403:** Al hacer clic en un nivel, la auto-asignación genera o reutiliza una asignación válida para el alumno, permitiendo que el reproductor interactivo cargue con código 200 OK y registre el progreso pedagógico en la base de datos.
4. **Filtrado Exclusivo en "Mis Actividades":** En el portal del alumno, "Mis Actividades" contiene únicamente las asignaciones manuales del profesor a cargo, evitando la duplicación de los niveles de "Mi Camino".
5. **Catálogo Colaborativo de Profesionales:** Las actividades creadas por cualquier profesional son visibles, consultables y asignables por toda la planta docente de la institución.
