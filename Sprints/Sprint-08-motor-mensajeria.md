# Sprint 8 — Motor Adaptativo, Reportes y Mensajería (IN-129 a IN-145)

**Período:** 

**Objetivo:** Motor adaptativo (MDA), reportes avanzada y sistema de mensajería

---

## Tareas

| Código | Task | Estado | Notas |
|--------|------|--------|-------|
| IN-129 | Evaluación automática de rendimiento tras cada actividad | ✅ DONE | `AdaptiveAdjustmentAgent` (PendingJobsWorker + IJobHandler) |
| IN-130 | Cálculo de ajuste según estado | ✅ DONE | Lógica en `AdaptiveAdjustmentAgent`: success streak / failure streak / frustration |
| IN-131 | Aplicación de ajuste dentro de rangos configurados | ✅ DONE | Clamp a [MinDifficultyLevel, MaxDifficultyLevel] |
| IN-132 | Registro de cada ajuste en historial de auditoría | ✅ DONE | `AddAdjustmentLogAsync` → tabla `AdaptiveAdjustmentLogs` |
| IN-133 | Alerta al profesional en estado de frustración | ✅ DONE | `FrustrationIntervention` → JobTypes.Push al profesional |
| IN-134 | Consulta del historial de ajustes (timeline) | ✅ DONE | `GetAdaptiveAdjustmentHistoryQuery` + endpoint + timeline en FE |
| IN-135 | Búsqueda semántica de actividades por lenguaje natural | ✅ DONE | Handler + endpoint + FE toggle IA |
| IN-136 | Creación de reporte de progreso | ✅ DONE | |
| IN-139 | Exportación de reporte a PDF | ⏳ PENDIENTE | |
| IN-140 | Bandeja de entrada de mensajes | ✅ DONE | BE + FE (MessagesComponent, inbox/sent tabs) |
| IN-141 | Envío de mensajes con asunto y contenido | ✅ DONE | BE + FE (compose modal, contactos) |
| IN-142 | Hilos de conversación (respuestas) | ✅ DONE | BE + FE (reply inline en detalle) |
| IN-143 | Indicador de mensajes no leídos en sidebar | ✅ DONE | BE + FE (badge sidebar professional + family) |
| IN-144 | Marcado automático como leído al abrir | ✅ DONE | BE + FE (auto-mark en openMessage) |
| IN-145 | Notificaciones automáticas de eventos del sistema | ✅ DONE | SignalR `NotificationHub` + `IRealTimeNotifier` + bell badge en navbar |

---

## Resumen

| Métrica | Valor |
|---------|-------|
| Total tareas | 15 |
| Completadas | 14 |
| Pendientes | 1 (IN-139 PDF export) |

---

## Notas

- Mensajería: `MessagesController` (8 endpoints) + `MessagesComponent` FE completo. Ruta `/pro/messages` y `/family/messages` activas. Badge sidebar en ambos portales.
- Búsqueda semántica: handler + endpoint + toggle FE completados.
- MDA: `AdaptiveAdjustmentAgent` completo. Flujo: `ActivityResponse` creada → background job `AdaptiveAdjustment` → agent lee historial, evalúa success streak / failure streak / frustration level, ajusta `DifficultyLevel` dentro de rangos, registra `AdaptiveAdjustmentLog`, notifica profesional si frustración.
- NotificationHub SignalR activo. `NotificationBellComponent` en navbar con badge en tiempo real. Dashboard del profesional incrementa contador unread por SignalR.
- Único pendiente: IN-139 export PDF de reportes.

---

## Épicas padre

- **IN-12:** Motor Adaptativo (MDA) y Reportes
- **IN-13:** Mensajería y Portal Familiar