# HU-10 — Motor de Dificultad Adaptativa (MDA) y Búsqueda Semántica

| Campo | Contenido |
|---|---|
| ID | HU-10 |
| Épica | Motor Adaptativo |
| Título | Motor de Dificultad Adaptativa y Búsqueda Semántica |
| Prioridad | Alta |
| Estimación | 13 puntos de historia |
| Sprint asignado | Sprint 6 |
| Estado | Completada |

**Proceso relacionado:** 10, 11

---

## Historia de Usuario

**Como** profesional
**Quiero** que el sistema ajuste automáticamente la dificultad de las actividades según el desempeño del estudiante
**Para** mantenerlo en su zona de desarrollo próximo sin frustración ni aburrimiento

**Como** profesional
**Quiero** configurar los rangos del motor adaptativo y ver el historial de ajustes realizados
**Para** personalizar la adaptación según cada persona y tomar decisiones pedagógicas informadas

**Como** profesional
**Quiero** buscar actividades escribiendo en lenguaje natural
**Para** encontrar actividades relevantes sin conocer su nombre exacto

---

## Estado de implementación

| Módulo | Backend | Frontend |
|--------|---------|----------|
| Motor adaptativo (ajuste automático) | ✅ Completo | ✅ Completo |
| Configuración por actividad del roadmap | ✅ Completo | ✅ Completo |
| Historial de ajustes (timeline) | ✅ Completo | ✅ Completo |
| Búsqueda semántica | ✅ Completo | ✅ Completo |
| Regeneración nocturna de embeddings estándar | ✅ Completo | — |

---

## Descripción funcional

### 1. Motor de Dificultad Adaptativa

Después de cada actividad completada, el sistema evalúa el rendimiento del estudiante y determina uno de 4 estados:

| Estado | Cuándo se activa | Qué hace el sistema |
|--------|------------------|---------------------|
| **Estable** | Rendimiento consistente | Mantiene los parámetros sin cambios |
| **Progresando** | `ConsecutiveSuccessToUpgrade` éxitos consecutivos ≥ `SuccessThresholdPercent` | Sube `DifficultyLevel` en 1 (hasta `MaxDifficultyLevel`) |
| **Dificultad** | `ConsecutiveFailuresToDowngrade` fallos consecutivos | Baja `DifficultyLevel` en 1 (hasta `MinDifficultyLevel`) |
| **Frustración** | `FrustrationLevel` ≥ `FrustrationThreshold` | Baja dificultad + envía alerta push al profesional |

Los ajustes nunca exceden los rangos configurados por el profesional.

#### Configuración por el profesional

Para cada actividad del roadmap, el profesional puede:
- **Activar o desactivar** el motor adaptativo
- **Configurar rangos** de dificultad mínimo-máximo
- **Definir umbrales** de éxitos/fracasos consecutivos, porcentaje de éxito mínimo, umbral de frustración
- **Quitar configuración** (botón "Quitar motor", visible solo si config existe)

Si el motor no está configurado o está desactivado, no interviene.

#### Historial de ajustes

Cada ajuste registra: tipo, valores anteriores/nuevos, motivo, fecha/hora.
El profesional ve el timeline cronológico con colores:
- **Verde** — DifficultyUp (progreso)
- **Amarillo** — DifficultyDown (dificultad)
- **Rojo** — FrustrationIntervention (alerta)

Exportable a CSV (botón "⬇ Exportar CSV").

---

### 2. Búsqueda Semántica de Actividades

El profesional busca actividades en lenguaje natural. El sistema genera embedding del texto y compara coseno contra embeddings almacenados.

**Flujo:**
1. Profesional escribe en barra de búsqueda → debounce 400ms
2. Toggle "Búsqueda IA" activa modo semántico
3. Resultados reemplazan el listado con badge "Búsqueda IA"
4. Al limpiar vuelve al listado paginado estándar

**Stack:** ONNX `paraphrase-multilingual-MiniLM-L12-v2` (384 dims) + pgvector cosine similarity.

---

### 3. Regeneración Nocturna de Embeddings (`TemplateGenerationAgent`)

Cada noche `MidnightCleanupWorker` encola un job `TemplateGeneration`. `TemplateGenerationAgent` lo procesa:
1. Obtiene todas las actividades estándar activas (`IsStandardActivity = true`)
2. Encola un job `Embedding` por cada una → `EmbeddingAgent` los procesa en ciclos siguientes
3. Garantiza que los embeddings de actividades estándar estén siempre actualizados

---

## Criterios de Aceptación

### Motor adaptativo ✅ Completo

- [x] Después de cada actividad completada, el sistema evalúa y ajusta automáticamente si el motor está activo
- [x] Los ajustes nunca exceden los rangos mínimo-máximo configurados por el profesional
- [x] Si no hay configuración o está desactivada, el sistema no interviene
- [x] Cada ajuste queda registrado en el historial con tipo, valores y motivo
- [x] En estado de frustración se envía una alerta push al profesional

### Configuración del motor ✅ Completo

- [x] El profesional puede activar/desactivar el motor por actividad del roadmap
- [x] Form con rangos de dificultad, umbrales de éxito/fallo/frustración, tiempo límite
- [x] Botón "Quitar motor" visible solo si config existe
- [x] La configuración se puede guardar sin activar el motor

### Historial de ajustes ✅ Completo

- [x] El timeline muestra los ajustes en orden cronológico descendente
- [x] Cada entrada muestra tipo con color, fecha y descripción legible
- [x] Las alertas de frustración se destacan visualmente (borde rojo)
- [x] Si no hay ajustes, se muestra "El motor aún no ha realizado ajustes"
- [x] Exportable a CSV con BOM UTF-8 y formato es-AR

### Búsqueda semántica ✅ Completo

- [x] El backend expone `GET /api/Activities/search?text=...&limit=N` con autenticación
- [x] Los resultados se ordenan por relevancia semántica (menor distancia coseno)
- [x] El servicio Angular `searchSemantic(text, limit)` consume el endpoint
- [x] La barra de búsqueda alterna entre filtro texto y búsqueda semántica
- [x] Al usar búsqueda semántica se muestra badge "Búsqueda IA"
- [x] Debounce 400ms para no spamear el endpoint
- [x] Al limpiar la búsqueda vuelve al listado paginado estándar
- [x] Si no hay resultados se muestra "Sin actividades relacionadas"

### Regeneración nocturna ✅ Completo

- [x] `TemplateGenerationAgent` registrado como `IJobHandler` con `JobTypeId = 5`
- [x] Encola jobs `Embedding` para cada actividad estándar activa
- [x] Tolerante a fallos individuales (continúa con la siguiente actividad si una falla)
- [x] Logea conteo de jobs encolados vs total

---

## Notas técnicas

### Implementación backend

```
GET    /api/Activities/search?text=...&limit=N   → búsqueda semántica
GET    .../areas/{areaId}/activities/{entryId}/adaptive-config   → config MDA
PUT    .../areas/{areaId}/activities/{entryId}/adaptive-config   → crear/actualizar
DELETE .../areas/{areaId}/activities/{entryId}/adaptive-config   → quitar motor
GET    .../areas/{areaId}/activities/{entryId}/adjustment-history → timeline
```

### Workers relevantes

| Worker/Agent | Rol |
|---|---|
| `MidnightCleanupWorker` | Dispara `GenerateTemplateCentroidsStep` cada noche |
| `GenerateTemplateCentroidsStep` | Encola job `TemplateGeneration` (JobType=5) |
| `TemplateGenerationAgent` | Procesa job → encola `Embedding` por cada actividad estándar |
| `EmbeddingAgent` | Llama Python `/embed` → guarda vector en `ActivityEmbeddings` |
| `AdaptiveAdjustmentAgent` | Evalúa rendimiento post-actividad → ajusta dificultad |

### Modelo de dificultad

`DifficultyLevel` es un entero dentro de `[MinDifficultyLevel, MaxDifficultyLevel]`. El contenido concreto de cada nivel lo interpreta el player de cada tipo de actividad.
