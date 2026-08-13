# Resumen Ejecutivo de Sprint - 2026-08-10

## Proyecto: InclusiON
**Sprint:** Sprint 10 — Roadmap Estándar, Corrección de Players y Refinamiento de Modelo de Negocio  
**Fecha de Reporte:** 10 de agosto de 2026  
**Responsable:** Sacha Del Barrio (Ingeniero de Sistemas Sénior)

---

## 1. Tabla Comparativa de Tickets en Desarrollo

A continuación se detallan los tickets que se encuentran activos o en etapa de estabilización/corrección por parte del equipo en el actual sprint:

| Ticket ID | Título del Ticket | Prioridad | Estado Jira | Bloqueador Principal / Detalle |
| :--- | :--- | :---: | :---: | :--- |
| **IN-203** | Corrección de plantillas de OPTION_SELECT | Media | En curso | **Falta de interactividad y accesibilidad en el player:** El componente carecía de respuesta auditiva de accesibilidad y lógica fluida de reintentos para alumnos. |
| **IN-204** | Fix: players muestran "actividad no disponible" por ContentJson vacío | Crítica | En curso | **Conflictos de tracking en EF Core:** Las transacciones de cambio de estado a completado fallaban o no se guardaban debido al uso de consultas sin tracking (`AsNoTracking`). |

---

## 2. Notas Técnicas Extraídas de los Comentarios y Cambios del Sprint

### Componente Frontend (Angular) — Player de Selección de Opciones (IN-203)
* **Síntesis de Voz (TTS):** Se integró `AccessibilityService` en la carga del player (`option-select-player.component.ts`) para reproducir mediante voz la pregunta de la actividad (`speakQuestion`), mejorando la autonomía de los alumnos con discapacidad visual o cognitiva.
* **Efectos de Sonido Puros:** Implementación de la API Web Audio de HTML5 (`AudioContext`) para reproducir efectos de sonido dinámicos para respuestas correctas (secuencia armónica ascendente en triángulo) e incorrectas (onda de sierra descendente), eliminando dependencias externas de archivos multimedia.
* **Lógica Anti-Frustración:** Cuando el alumno comete un error, el sistema entra en un estado temporal de error (`wrongOptionId`) durante 700ms, tras el cual limpia la selección para permitir un reintento inmediato sin interrumpir el flujo ni penalizar al alumno.
* **Lógica de Acierto:** Al seleccionar la opción correcta, se activa `dockedOptionId` para mostrar la animación de éxito, se reproduce el sonido armónico y se lee "¡Excelente!" antes de redirigir a los resultados a los 1800ms.

### Componente Backend (.NET 10) y Persistencia (IN-204)
* **Solución de Seguimiento en EF Core:** Se removió `.AsNoTracking()` de `GetByIdAsync` en `ActivityAssignmentRepository.cs` para asegurar que Entity Framework haga seguimiento a la entidad. Adicionalmente, en `UpdateAsync` se implementó una verificación de estado de la entidad (`Detached`) para aplicar `.Update()` únicamente si la entidad fue desconectada, resolviendo conflictos de concurrencia.
* **Ajuste Adaptativo Robustecido:** En `CompleteActivityResponseCommandHandler.cs`, se modificaron las llamadas asíncronas de tipo "fire-and-forget" (`Task.Run`). Ahora, el registro del ajuste adaptativo en la cola de trabajos (`JobTypes.AdaptiveAdjustment`) y el envío de notificaciones push (`JobTypes.Push`) se realizan de forma explícita y awaitable utilizando el token de cancelación de la petición principal. Esto previene pérdidas de contexto o excepciones de base de datos dispuesta prematuramente.

### Sembrado de Datos (IN-201)
* **Actualización del Nivel 3:** Modificación del archivo `RoadmapInitializer.cs` para redefinir el "Concepto 'Muchos / Pocos'" de la trayectoria estándar. Se cambió la definición anterior de "manzanas en canastas" por un esquema genérico de "grupos de frutas" con pictogramas actualizados (ID de acierto `muchas` con el pictograma 28339 e ID de error `pocas` con el pictograma 3247).

---

## 3. Evaluación de Progreso y Cierre

El Sprint 10 cuenta con las **14 historias y tareas de su planificación original completadas formalmente** (entre ellas, la migración del calendario, la inicialización automática de la trayectoria de 10 niveles y la eliminación de módulos redundantes de la UI bajo directivas de modelo de negocio). 

El trabajo de desarrollo restante consiste exclusivamente en pulir la robustez de la API en el backend y mejorar la experiencia interactiva de accesibilidad en los players del frontend Angular. Considerando que aún restan días para la fecha límite del sprint y que el avance en código ya cubre los objetivos de negocio principales, **el sprint se encuentra en una situación excelente y en camino a un cierre exitoso sin desviaciones de cronograma**.
