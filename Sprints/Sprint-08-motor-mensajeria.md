# Sprint 8 — Motor Adaptativo, Reportes y Mensajería (IN-129 a IN-145)

**Período:** 

**Objetivo:** Motor adaptativo (MDA), reportes avanzada y sistema de mensajería

---

## Tareas

| Código | Task | Estado | Notas |
|--------|------|--------|-------|
| IN-129 | Evaluación automática de rendimiento tras cada actividad | ⏳ PENDIENTE | MDA |
| IN-130 | Cálculo de ajuste según estado | ⏳ PENDIENTE | Estable/Progresando/Dificultad/Frustración |
| IN-131 | Aplicación de ajuste dentro de rangos configurados | ⏳ PENDIENTE | |
| IN-132 | Registro de cada ajuste en historial de auditoría | ⏳ PENDIENTE | |
| IN-133 | Alerta al profesional en estado de frustración | ⏳ PENDIENTE | |
| IN-134 | Consulta del historial de ajustes (timeline) | ⏳ PENDIENTE | |
| IN-135 | Búsqueda semántica de actividades por lenguaje natural | ✅ DONE | Handler + endpoint + FE toggle IA |
| IN-136 | Creación de reporte de progreso | ✅ DONE | |
| IN-139 | Exportación de reporte a PDF | ⏳ PENDIENTE | |
| IN-140 | Bandeja de entrada de mensajes | ✅ DONE | Backend completo |
| IN-141 | Envío de mensajes con asunto y contenido | ✅ DONE | Restricción prof↔familiar por vínculo activo |
| IN-142 | Hilos de conversación (respuestas) | ✅ DONE | ReplyToMessage + nested replies |
| IN-143 | Indicador de mensajes no leídos en sidebar | ✅ DONE | GET /messages/unread-count |
| IN-144 | Marcado automático como leído al abrir | ✅ DONE | Auto-mark en GetById |
| IN-145 | Notificaciones automáticas de eventos del sistema | ⏳ PENDIENTE | |

---

## Resumen

| Métrica | Valor |
|---------|-------|
| Total tareas | 15 |
| Completadas | 7 |
| Pendientes | 8 |

---

## Notas

- Entidades existen: AdaptiveEngineConfig, ActivityEmbedding, Message
- Library SemanticSearch existe pero falta integración
- Backend: `MessagesController` implementado con 8 endpoints y restricción por vínculo activo. `AdaptiveEngineController` pendiente.

---

## Épicas padre

- **IN-12:** Motor Adaptativo (MDA) y Reportes
- **IN-13:** Mensajería y Portal Familiar