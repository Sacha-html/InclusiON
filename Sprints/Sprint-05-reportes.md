# Sprint 5 — Reportes y Dashboard Familiar (IN-137, IN-138, IN-155-IN-156, IN-91, IN-153)

**Período:** 

**Objetivo:** Completar sistema de reportes (profesional y familia), dashboard familiar

---

## Tareas

### Reportes - Profesional

| Código | Task | Backend | Frontend | Estado |
|--------|------|---------|----------|--------|
| IN-137 | Consulta de reportes por profesional | ✅ GET /api/reports | ⚠️ Ruta → dashboard (mock) | 🟡 Parcial |
| IN-155 | [Frontend] - Consulta de reportes profesional | - | ❌ No existe vista real | ⏳ Pendiente |
| IN-136 | Creación de reporte de progreso (tipo, período, contenido) | ❌ No existe | ❌ No existe | ⏳ Pendiente |
| IN-156 | Alta de Reportes (POST) | ❌ No existe | ❌ No existe | ⏳ Pendiente |
| IN-139 | Exportación de reporte a PDF | ❌ No existe | ❌ No existe | ⏳ Pendiente |

### Reportes - Familia

| Código | Task | Backend | Frontend | Estado |
|--------|------|---------|----------|--------|
| IN-138 | Consulta de reportes por familia | ✅ GET /api/reports (filtro personId) | ❌ No existe | ⏳ Pendiente |
| IN-153 | Panel de visualización progreso y reportes (Familiar-Tutor) | ❌ No existe | ❌ No existe | ⏳ Pendiente |
| IN-91 | Dashboard familiar (últimas actividades, mensajes, reportes) | ❌ No existe | ⚠️ Existe mock | ⏳ Pendiente |

---

## Resumen

| Métrica | Valor |
|---------|-------|
| Total tareas | 8 |
| Backend completado | 2 (IN-137, IN-138) |
| Frontend completado | 0 |
| Parcial (mock) | 2 |
| Pendientes | 6 |

---

## Validación código

### Reportes Backend
- ✅ `ReportsController.cs` - GET /api/reports
- ✅ Entity `Report.cs` con campos: Title, Content, PeriodStartDate, PeriodEndDate, AchievedGoals, AreasToReinforce, FutureRecommendations, NextObjectives
- ✅ `ReportsRepository.cs`
- ❌ Falta POST /api/reports (crear)
- ❌ Falta endpoint PDF

### Reportes Frontend
- ⚠️ Ruta `/pro/reports` existe pero carga `pro-dashboard.component` (mock)
- ⚠️ Ruta `/family` existe con mock (cards hardcodeadas)
- ❌ No existe ReportsService
- ❌ No existe vista de reportes real para ningún rol

### Familia Dashboard actual
- ✅ Existe `family-dashboard.component.ts`
- ⚠️ Tiene cards mock: "15/20 actividades", "Próxima cita mañana"

---

## Pendientes (próximos pasos)

1. Crear ReportsService en frontend
2. Crear vista `/pro/reports` real (tabla con filtros)
3. Crear vista `/family/reports` para familiar
4. Agregar POST /api/reports (backend)
5. Agregar generación PDF (backend + frontend)

---

## Épicas padre

- **IN-12:** Motor Adaptativo (MDA) y Reportes