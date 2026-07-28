# Sprint 6 — Gestión de Actividades, Roadmap y Players (IN-105 a IN-128)

**Período:** 

**Objetivo:** Gestión de actividades, roadmap y players de resolución

---

## Tareas

### Gestión de Actividades

| Código | Task | Backend | Frontend | Estado |
|--------|------|---------|----------|--------|
| IN-105 | Creación de actividad con wizard | ⚠️ Entidad existe, no controller | ❌ No existe | ⏳ Pendiente |
| IN-106 | Integración de pictogramas ARASAAC | ❌ No existe | ❌ No existe | ⏳ Pendiente |
| IN-107 | Consulta del catálogo de actividades | ⚠️ Entidad existe | ❌ No existe | ⏳ Pendiente |
| IN-108 | Edición de actividad propia | ⚠️ Entidad existe | ❌ No existe | ⏳ Pendiente |
| IN-109 | Desactivación de actividad | ⚠️ Entidad existe | ❌ No existe | ⏳ Pendiente |

### Roadmap (Plan de Trabajo)

| Código | Task | Backend | Frontend | Estado |
|--------|------|---------|----------|--------|
| IN-110 | Creación del roadmap por persona | ⚠️ Entidad PersonRoadmap existe | ❌ No existe | ⏳ Pendiente |
| IN-111 | Agregar actividades al roadmap por área | ⚠️ Entidad existe | ❌ No existe | ⏳ Pendiente |
| IN-112 | Definir orden secuencial y umbral de desbloqueo | ⚠️ Entidad existe | ❌ No existe | ⏳ Pendiente |
| IN-113 | Reordenamiento de actividades drag-drop | ❌ No existe | ❌ No existe | ⏳ Pendiente |
| IN-114 | Desbloqueo manual de actividad | ❌ No existe | ❌ No existe | ⏳ Pendiente |
| IN-115 | Eliminación de actividad del roadmap | ❌ No existe | ❌ No existe | ⏳ Pendiente |
| IN-116 | Configuración del motor adaptativo por actividad | ⚠️ Entidad existe | ❌ No existe | ⏳ Pendiente |
| IN-117 | Visualización del roadmap (estilo Duolingo) | ❌ No existe | ❌ No existe | ⏳ Pendiente |

### Players y Resolución de Actividades

| Código | Task | Backend | Frontend | Estado |
|--------|------|---------|----------|--------|
| IN-118 | Carga de asignación con contenido completo | ⚠️ Entidad ActivityAssignment existe | ❌ No existe | ⏳ Pendiente |
| IN-119 | Inicio de actividad (registro de respuesta) | ⚠️ Entidad existe | ❌ No existe | ⏳ Pendiente |
| IN-120 | Player: Selección de figuras | ❌ No existe | ❌ No existe | ⏳ Pendiente |
| IN-121 | Player: Suma visual | ❌ No existe | ❌ No existe | ⏳ Pendiente |
| IN-122 | Player: Emparejar imagen-palabra | ❌ No existe | ❌ No existe | ⏳ Pendiente |
| IN-123 | Player: Ordenar secuencia | ❌ No existe | ❌ No existe | ⏳ Pendiente |
| IN-124 | Player: Completar letra | ❌ No existe | ❌ No existe | ⏳ Pendiente |
| IN-125 | Registro de progreso (intentos, frustración) | ⚠️ Entidad ActivityResult existe | ❌ No existe | ⏳ Pendiente |
| IN-126 | Completar actividad y evaluar resultado | ❌ No existe | ❌ No existe | ⏳ Pendiente |
| IN-127 | Desbloqueo automático si supera umbral | ❌ No existe | ❌ No existe | ⏳ Pendiente |
| IN-128 | Monitoreo de frustración (pausa tras 3+ intentos) | ❌ No existe | ❌ No existe | ⏳ Pendiente |

---

> **Nota:** El Motor Adaptativo (MDA) IN-129 a IN-135 fue movido a Sprint 5.

---

## Resumen

| Métrica | Valor |
|---------|-------|
| Total tareas | 24 |
| Entidades existentes | 10 |
| Controllers existentes | 0 |
| Frontend completado | 0 |

---

## Validación código

### Entidades existentes (Backend)
- ✅ `Activity.cs` - Actividad principal
- ✅ `ActivityContent.cs` - Contenido de actividad
- ✅ `ActivityAssignment.cs` - Asignación de actividad
- ✅ `PersonRoadmap.cs` - Roadmap por persona
- ✅ `PersonRoadmapArea.cs` - Áreas del roadmap
- ✅ `PersonRoadmapActivity.cs` - Actividades del roadmap
- ✅ `ActivityResult.cs` - Resultado de actividad
- ✅ `ActivityResponse.cs` - Respuesta de actividad
- ✅ `ActivityCategory.cs` - Categoría de actividad
- ✅ `ActivityTemplateType.cs` - Tipos de template

### Controllers faltantes
- ❌ `ActivityController` - No existe
- ❌ `RoadmapController` - No existe

### Frontend
- ❌ No existe vista de gestión de actividades
- ❌ No existe componente de roadmap (estilo Duolingo)
- ❌ No existen players de resolución

---

## Épicas padre

- **IN-9:** Gestión de Actividades
- **IN-10:** Plan de Trabajo (Roadmap)
- **IN-11:** Player y Resolución de Actividades