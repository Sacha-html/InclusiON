# Sprint 10 — Roadmap Estándar, Corrección de Players y Refinamiento de Modelo de Negocio

**Período:** Julio – Agosto 2026

**Objetivo:** Implementar el roadmap estándar de 10 niveles para nuevos alumnos, corregir el bug crítico de players de actividades sin contenido, agregar experiencia de celebración al completar actividades, y refinar el modelo de negocio eliminando funcionalidades que no corresponden a cada perfil de usuario.

---

## Tareas

### Roadmap Estándar (10 Niveles)

| Código | Task | Backend | Frontend | Estado |
|--------|------|---------|----------|--------|
| IN-200 | Implementar `RoadmapInitializer` con 10 actividades estándar por alumno nuevo | `RoadmapInitializer.cs` | — | ✅ DONE |
| IN-201 | Definir contenido JSON real por tipo de player para cada actividad | `RoadmapInitializer.cs` + `DatabaseSeeder.cs` | — | ✅ DONE |
| IN-202 | Patch de actividades existentes con `ContentJson = '{}'` al arranque | `DatabaseSeeder.PatchStandardActivitiesContentAsync` | — | ✅ DONE |
| IN-203 | Corrección de `TemplateTypeId` para actividades 3, 6 y 10 (OPTION_SELECT) | `DatabaseSeeder.cs` | — | ✅ DONE |

> **Detalle IN-202/203:** El bug raíz era doble: (1) `RoadmapInitializer` creaba actividades con `ContentJson = '{}'` por lo que ningún player tenía datos del juego. (2) La actividad 6 usaba `SOUND_RECOGNITION` que no tiene player implementado en el `PLAYER_REGISTRY`. La solución fue un método de parche idempotente que corre en cada arranque del servidor.

---

### Corrección de Bug Crítico — Players de Actividades

| Código | Task | Backend | Frontend | Estado |
|--------|------|---------|----------|--------|
| IN-204 | Fix: players muestran "actividad no disponible" por ContentJson vacío | `RoadmapInitializer.cs` + `DatabaseSeeder.cs` | — | ✅ DONE |
| IN-205 | Fix: `withViewTransitions()` causa `InvalidStateError` con HMR en desarrollo | — | `app.config.ts` | ✅ DONE |

---

### Experiencia del Alumno — Celebración al Completar Actividad

| Código | Task | Backend | Frontend | Estado |
|--------|------|---------|----------|--------|
| IN-206 | Animación de medalla dorada al completar actividad exitosamente | — | `player-result.component` CSS puro | ✅ DONE |

> 12 partículas de confetti multicolor con animaciones `medal-drop`, `burst`, `confetti`. Sin dependencias externas.

---

### Mejoras de UX — Chat y Sembrado

| Código | Task | Backend | Frontend | Estado |
|--------|------|---------|----------|--------|
| IN-207 | Auto-scroll al final del chat al recibir o enviar mensajes | — | `messages.component.ts` | ✅ DONE |
| IN-208 | Fix sembrado: supervisor de Benjamín Castro | `DatabaseSeeder.cs` | — | ✅ DONE |
| IN-209 | Fix sembrado: Laura González omitida en tabla Professionals | `DatabaseSeeder.cs` | — | ✅ DONE |

---

### Seguridad y Dependencias

| Código | Task | Estado | Notas |
|--------|------|--------|-------|
| IN-210 | Actualización `System.Security.Cryptography.Xml` → `10.0.10` | ✅ DONE | CVE resuelto. Aplicado en todos los proyectos del backend |
| IN-211 | Actualización `QuestPDF` → `2025.4.0` | ✅ DONE | Discrepancia NuGet resuelta |

---

### Modelo de Negocio — Ajuste de Perfiles de Usuario

| Código | Task | Backend | Frontend | Estado |
|--------|------|---------|----------|--------|
| IN-212 | Eliminar Calendario del perfil Persona (AAC) | — | `aac/routes.ts` + `aac-home.component.html` + `aac-nav.component.ts` | ✅ DONE |
| IN-213 | Eliminar módulo de Instituciones del dashboard Admin | — | `_nav.ts` + `app.routes.ts` + `default-layout.component.ts` | ✅ DONE |

> **IN-212:** Las personas con discapacidad tienen autonomía limitada. El calendario sigue disponible en perfiles Profesional y Familiar.
>
> **IN-213:** El administrador *es* la institución. Los endpoints `/api/institutions` y el modelo de datos siguen existiendo — solo se removió la UI.

---

## Resumen

| Métrica | Valor |
|---------|-------|
| Total tareas | 14 |
| Completadas | 14 |
| Pendientes | 0 |
| CVEs resueltos | 1 |
| Componentes Angular modificados | 4 |
| Sin migraciones nuevas de BD | ✅ |

---

## Backlog próximos candidatos (Sprint 11)

| Código | Task | Prioridad |
|--------|------|-----------|
| IN-139 | Exportación de reporte a PDF (QuestPDF) | Media |
| — | Player SOUND_RECOGNITION | Baja |
| IN-99/100 | Onboarding interactivo (tour guiado) | Media |
| IN-101/102 | Pantallas de bienvenida persona y familiar | Media |
| — | Modo oscuro accesible completo | Media |
| — | Paginación historial asignaciones (familia) | Baja |

---

## Épicas padre

- **IN-10:** Plan de Trabajo (Roadmap) ← cerrado
- **IN-11:** Player y Resolución de Actividades ← cerrado
- **IN-12:** Motor Adaptativo (MDA) y Reportes ← activo (PDF pendiente)
- **IN-13:** Mensajería y Portal Familiar ← activo
