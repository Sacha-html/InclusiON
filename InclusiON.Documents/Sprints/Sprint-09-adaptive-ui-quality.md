# Sprint 9 — Adaptive Engine UI, Radar Chart, UX de Calidad, Tipos de Actividad (IN-90, IN-116, IN-150+)

**Período:** Mayo 2026

**Objetivo:** UI completa del motor adaptativo, radar de habilidades, mejoras UX cross-portal (notificación en tiempo real, completitud de perfil, asignación desde roadmap, radar en familia), completar tipos de actividad faltantes (OPTION_SELECT, GLOBAL_READING)

---

## Tareas

| Código | Task | Estado | Notas |
|--------|------|--------|-------|
| IN-116 | Configuración del motor adaptativo por actividad (UI GET/PUT/DELETE) | ✅ DONE | Modal en roadmap tab + 3 endpoints + handlers + validación |
| IN-90  | Radar chart de habilidades (sin librería externa) | ✅ DONE | Pure SVG, `SkillRadarChartComponent`, `GetSkillRadarQueryHandler`, `IAdaptiveEngineRepository.GetSkillRadarAsync` |
| IN-150 | Asignar actividad directamente desde roadmap | ✅ DONE | `AssignFromRoadmapCommand` + handler + endpoint + modal FE |
| —      | Notification bell con badge en tiempo real (SignalR) | ✅ DONE | `NotificationBellComponent`, `SignalrService.notification$`, reemplaza placeholder en navbar |
| —      | Dashboard profesional real-time (contador unread) | ✅ DONE | `DetailComponent` suscribe `notification$` → incrementa `unreadMessages` live |
| —      | Radar en portal familiar | ✅ DONE | `FamilyProgressComponent` carga `getSkillRadar()` al seleccionar persona |
| —      | Completitud de perfil funcional | ✅ DONE | `profileCompletion` getter (7 campos), badge coloreado en `ProfessionalFunctionalProfileComponent` |
| —      | Exportar historial MDA a CSV | ✅ DONE | `exportHistoryToCsv()` en roadmap tab. BOM UTF-8, formato es-AR, botón "⬇ Exportar CSV" en timeline |
| —      | Tipo actividad OPTION_SELECT — editor + player | ✅ DONE | Opción múltiple: texto + pictograma opcional por opción. Editor y player completos, registrados en ambos registries |
| —      | Tipo actividad GLOBAL_READING — editor + player | ✅ DONE | Lectura global: palabra prominente + grid de pictogramas. Editor y player completos, registrados en ambos registries |

---

## Detalle técnico

### IN-116 — Adaptive Engine Config UI

**Backend (nuevos):**
- `GetAdaptiveEngineConfigQuery` / `GetAdaptiveEngineConfigQueryHandler`
- `UpsertAdaptiveEngineConfigCommand` / `UpsertAdaptiveEngineConfigCommandHandler`
- `DeleteAdaptiveEngineConfigCommand` / `DeleteAdaptiveEngineConfigCommandHandler`
- `IAdaptiveEngineRepository`: `GetConfigAsync`, `UpsertConfigAsync`, `DeleteConfigAsync`
- Endpoints: `GET/PUT/DELETE .../areas/{areaId}/activities/{entryId}/adaptive-config`

**Frontend:**
- Modal "⚙ Motor MDA" en cada actividad del roadmap tab
- Form con: isEnabled toggle, rangos de dificultad, umbrales de éxito/fallo/frustración, tiempo límite
- Botón "Quitar motor" (solo visible si config existe)
- `RoadmapService`: `getAdaptiveConfig()`, `upsertAdaptiveConfig()`, `deleteAdaptiveConfig()`

### IN-90 — Radar Chart de Habilidades

**Backend:**
- `GetSkillRadarQuery(Guid PersonId)` / `GetSkillRadarQueryHandler`
- `IAdaptiveEngineRepository.GetSkillRadarAsync`: agrupación por área → promedio de éxito ponderado
- `SkillRadarPointResponse` DTO: `AreaName`, `Color`, `Icon`, `AvgSuccessPercent?`, `TotalResponses`
- Endpoint: `GET /api/persons/{personId}/roadmap/skill-radar`

**Frontend:**
- `SkillRadarChartComponent`: SVG puro, `cx=140 cy=140 r=95`, grillas al 25/50/75/100%, polígono de datos, labels truncados, leyenda tabular debajo
- `computed` signals: `n()`, `angles()`, `hasAnyData()`
- Fallback: mensaje si hay < 3 áreas
- Integrado en `ProfessionalSkillsComponent` (vista profesional) y `FamilyProgressComponent` (portal familia)

### IN-150 — Asignar desde Roadmap

**Backend:**
- `AssignFromRoadmapCommand(PersonRoadmapActivityId, PersonId, AssignedByProfessionalId, DueDate?, IsEvaluationActivity)`
- `AssignFromRoadmapCommandHandler`: resuelve `activityId` desde `PersonRoadmapActivity`, crea `ActivityAssignment`, push notification fire-and-forget
- `AssignFromRoadmapRequest` DTO
- Endpoint: `POST .../areas/{areaId}/activities/{entryId}/assign`

**Frontend:**
- Botón "📋 Asignar" en cada actividad del roadmap (visible con permiso `Roadmap.Update`)
- Modal: fecha límite opcional + checkbox isEvaluationActivity
- `RoadmapService.assignFromRoadmap()`

### Notification Bell

- `NotificationBellComponent`: `unreadCount = signal(0)`, fetch inicial desde `MessagesService.getUnreadCount()`
- Suscribe `SignalrService.notification$` → incrementa badge en tiempo real
- Click: reset a 0, navega a `/pro/messages`
- Badge `99+` si count > 99, `aria-label` dinámico
- Reemplaza icono estático en `DefaultHeaderComponent`

### Dashboard Real-Time

- `DetailComponent` (dashboard profesional) inyecta `SignalrService`
- `ngOnInit`: suscribe `notification$` → `unreadMessages++`
- `ngOnDestroy`: unsubscribe limpio

### Completitud de Perfil Funcional

- `ProfessionalFunctionalProfileComponent.profileCompletion`: cuenta 7 campos informativos
  - Niveles: `attentionLevel`, `communicationLevel`, `motorSkillLevel` (> 0)
  - Texto: `interestsAndMotivators`, `learningStyle`, `availableResources`, `additionalTherapies` (non-empty)
- Badge `"Perfil: X% completo"` con color dinámico: verde ≥80%, amarillo ≥40%, rojo <40%

### OPTION_SELECT — Editor + Player

**Modelo (`player.models.ts`):**
```typescript
interface OptionSelectOption  { id: string; text: string; pictogramId?: number; }
interface OptionSelectContent { instruction: string; question: string; options: OptionSelectOption[]; correctOptionId: string; }
```

**Editor (`option-select-editor.component`):**
- Campo instrucción + campo pregunta principal
- Lista de opciones: texto editable, pictograma ARASAAC opcional por opción
- Click en opción → marcar como correcta (highlight verde)
- Overlay picker ARASAAC por opción individual (botón `＋🖼`)
- Válido: instrucción + pregunta + ≥2 opciones + correctOptionId

**Player (`option-select-player.component`):**
- Fase `playing`: pregunta prominente (`game-question`) + grid `auto-fit minmax(180px,1fr)`
- Cada botón muestra pictograma (si existe) + texto
- `selectOption()`: evalúa, 900 ms delay → fase `result`
- Estados: `none / correct / wrong / reveal / dimmed` con colores `--a11y-*`
- Score: 0 o 100

### GLOBAL_READING — Editor + Player

**Modelo (`player.models.ts`):**
```typescript
interface GlobalReadingContent {
  instruction: string;
  word:        string;
  items:       Array<{ id: string; pictogramId: number; label: string }>;
  correctItemId: string;
}
```

**Editor (`global-reading-editor.component`):**
- Campo instrucción + campo `word` (palabra a leer)
- Misma lógica de items + buscador ARASAAC que `SelectFigureEditorComponent`
- Válido: instrucción + word ≥2 chars + ≥2 items + correctItemId

**Player (`global-reading-player.component`):**
- Fase `playing`: instrucción pequeña → `.word-display` (3rem, uppercase, letra destacada) → grid `PictogramCardComponent`
- `selectItem()`: evalúa, 900 ms delay → fase `result`
- `itemState()` / `itemBadge()`: mismos estados que `SelectFigurePlayer`
- Score: 0 o 100

**Registries actualizados:**
- `CONTENT_EDITOR_REGISTRY`: agregados `OPTION_SELECT` + `GLOBAL_READING`
- `PLAYER_REGISTRY`: agregados `OPTION_SELECT` + `GLOBAL_READING`
- Único tipo aún sin implementar: `SOUND_RECOGNITION` (requiere MediaRecorder API)

---

## Tests

| Clase | Tests | Estado |
|-------|-------|--------|
| `AdaptiveAdjustmentAgentTests` | 13 | ✅ Passing |
| `GetAdaptiveEngineConfigQueryHandlerTests` | 2 | ✅ Passing |
| `UpsertAdaptiveEngineConfigCommandHandlerTests` | 3 | ✅ Passing |
| `DeleteAdaptiveEngineConfigCommandHandlerTests` | 1 | ✅ Passing |
| `GetSkillRadarQueryHandlerTests` | 3 | ✅ Passing |
| `RoadmapControllerTests` (nuevos) | 4 | ✅ Passing |
| **Total acumulado** | **648** | **0 fallos** |

---

## Resumen

| Métrica | Valor |
|---------|-------|
| Total tareas | 10 |
| Completadas | 10 |
| Tests nuevos | 26 |
| Endpoints nuevos | 6 |
| Componentes Angular nuevos | 4 (`SkillRadarChartComponent`, `NotificationBellComponent`, `OptionSelectEditorComponent` + player, `GlobalReadingEditorComponent` + player) |
| Tipos de actividad cubiertos | 7 / 8 (pendiente: `SOUND_RECOGNITION`) |

---

## Backlog próximos candidatos

| Código | Task | Prioridad | Notas |
|--------|------|-----------|-------|
| IN-139 | Exportación de reporte a PDF | Media | `QuestPDF` o `Puppeteer` headless |
| — | Tipo actividad SOUND_RECOGNITION — editor + player | Media | Requiere MediaRecorder API, grabación de voz, comparación fonética o ASR |
| — | Onboarding interactivo (tour guiado) | Media | Shepherd.js o similar, primer login de profesional |
| — | Alertas configurables (email semanal de progreso) | Baja | Job semanal `WeeklyProgressReportAgent` |
| — | Paginación en historial de asignaciones (portal familia) | Baja | Actualmente carga todo de una vez |
| — | Modo oscuro accesible (14 combinaciones tema) | Media | Variables `--a11y-*` ya definidas, falta toggle UI |

---

## Épicas padre

- **IN-10:** Plan de Trabajo (Roadmap) ← activo
- **IN-12:** Motor Adaptativo (MDA) ← cerrado
- **IN-13:** Mensajería y Portal Familiar ← activo
