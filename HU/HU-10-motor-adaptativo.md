# HU-10 — Motor de Dificultad Adaptativa (MDA) y Búsqueda Semántica

**Proceso relacionado:** 10, 11, 13
**Prioridad:** Alta
**Última modificación:** 2026-05-16 — Búsqueda semántica completa: toggle IA + barra + debounce 400ms. Eliminada sección de UI pendiente.

---

## Historia de Usuario

**Como** profesional
**Quiero** que el sistema ajuste automáticamente la dificultad, tiempo, pistas e intentos de las actividades según el desempeño del estudiante
**Para** mantenerlo en su zona de desarrollo próximo sin frustración ni aburrimiento

**Como** profesional
**Quiero** configurar los rangos del motor adaptativo y ver el historial de ajustes realizados
**Para** personalizar la adaptación según cada persona y tomar decisiones pedagógicas informadas

**Como** profesional
**Quiero** buscar actividades escribiendo en lenguaje natural
**Para** encontrar actividades relevantes sin conocer su nombre exacto

---

## Descripción funcional

### 1. Motor de Dificultad Adaptativa

> **Estado de implementación:**
> - Backend: 🔧 Schema en base de datos listo (`AdaptiveEngineConfig`, `AdaptiveAdjustmentLog`). Sin handlers ni endpoints.
> - Frontend: ⏳ No iniciado.

Después de cada actividad completada, el sistema evalúa el rendimiento del estudiante y determina uno de 4 estados:

| Estado | Cuándo se activa | Qué hace el sistema |
|--------|------------------|---------------------|
| **Estable** | Rendimiento consistente | Mantiene los parámetros sin cambios |
| **Progresando** | Varios éxitos consecutivos por encima del umbral | Sube la dificultad, reduce el tiempo, reduce las pistas y los intentos disponibles |
| **Dificultad** | Varios fracasos consecutivos por debajo del umbral | Baja la dificultad, aumenta el tiempo, agrega pistas e intentos |
| **Frustración** | Nivel de frustración alto o 3+ abandonos | Baja todo al mínimo y envía una alerta al profesional |

Los ajustes nunca exceden los rangos configurados por el profesional.

#### Configuración por el profesional

Para cada actividad del roadmap, el profesional puede:
- **Activar o desactivar** el motor adaptativo
- **Configurar rangos** de dificultad (mínimo-máximo), tiempo límite (mínimo-máximo)
- **Definir umbrales** de éxitos/fracasos consecutivos para escalar o desescalar, porcentaje de éxito mínimo, umbral de frustración
- **Restaurar valores por defecto**

Si el motor no está configurado o está desactivado, no interviene.

#### Historial de ajustes

Cada ajuste realizado por el motor se registra con:
- Tipo de ajuste (escalamiento, desescalamiento, frustración)
- Valores anteriores y nuevos
- Motivo del ajuste
- Fecha y hora

El profesional puede ver este historial como timeline cronológico con colores:
- **Verde** — Escalamiento (progreso)
- **Amarillo** — Desescalamiento (dificultad)
- **Rojo** — Frustración (alerta)

---

### 2. Búsqueda Semántica de Actividades

> **Estado de implementación:**
> - Backend: ✅ Completamente implementado. Endpoint `GET /api/Activities/search?text=...&limit=N`. ONNX `paraphrase-multilingual-MiniLM-L12-v2` (384 dims) + pgvector cosine similarity. Filtrado por profesional y actividades estándar.
> - Frontend (servicio): ✅ `ActivitiesService.searchSemantic(text, limit)` implementado.
> - Frontend (UI): ⏳ Sin interfaz visual — barra de búsqueda no conectada aún en lista de actividades.

El profesional puede buscar actividades usando lenguaje natural. El sistema genera un embedding del texto ingresado y compara semánticamente contra todos los embeddings almacenados, devolviendo los más similares ordenados por relevancia.

**Flujo:**
1. Profesional escribe en la barra de búsqueda de actividades
2. Con debounce de 400ms se llama al endpoint semántico
3. Los resultados reemplazan el listado estándar con badge "Búsqueda IA"
4. Al limpiar la búsqueda vuelve al listado paginado normal

**Diferencias con búsqueda de texto:**

| Característica | Texto exacto | Semántica |
|---|---|---|
| Búsqueda por sinónimos | ❌ | ✅ |
| Búsqueda por concepto | ❌ | ✅ |
| Ordena por relevancia | ❌ | ✅ |
| Funciona offline | ✅ | ❌ (requiere modelo ONNX cargado) |

---

## Criterios de Aceptación

### Motor adaptativo

- [ ] Después de cada actividad completada, el sistema evalúa y ajusta automáticamente si el motor está activo
- [ ] Los ajustes nunca exceden los rangos mínimo-máximo configurados por el profesional
- [ ] Si no hay configuración o está desactivada, el sistema no interviene
- [ ] Cada ajuste queda registrado en el historial con tipo, valores y motivo
- [ ] La evaluación y el ajuste se realizan de forma atómica (todo o nada)
- [ ] En estado de frustración se envía una alerta al profesional (mensaje interno)

### Configuración del motor

- [ ] El profesional puede activar/desactivar el motor por actividad del roadmap
- [ ] Los sliders de rango validan que el mínimo no supere al máximo
- [ ] Se pueden restaurar los valores por defecto con un botón
- [ ] La configuración se puede guardar sin activar el motor (draft)

### Historial de ajustes

- [ ] El timeline muestra los ajustes en orden cronológico descendente
- [ ] Cada entrada muestra el tipo con color, fecha y descripción legible (ej: "Dificultad subió de 2 a 3")
- [ ] Las alertas de frustración se destacan visualmente con borde rojo
- [ ] Si no hay ajustes, se muestra "El motor aún no ha realizado ajustes"
- [ ] Se puede filtrar por tipo de ajuste o rango de fechas

### Búsqueda semántica ✅ Completo

- [x] El backend expone `GET /api/Activities/search?text=...&limit=N` con autenticación
- [x] Los resultados se ordenan por relevancia semántica (menor distancia coseno)
- [x] El servicio Angular `searchSemantic(text, limit)` consume el endpoint
- [x] La barra de búsqueda en lista de actividades alterna entre filtro texto y búsqueda semántica
- [x] Al usar búsqueda semántica se muestra badge "Búsqueda IA" en el resultado
- [x] Con debounce de 400ms para no spamear el endpoint
- [x] Al limpiar la búsqueda vuelve al listado paginado estándar
- [x] Si no hay resultados se muestra mensaje "Sin actividades relacionadas"

---

## Notas técnicas

### Motor adaptativo — implementación backend pendiente

```
POST /api/AdaptiveConfig            → crear/actualizar config para un nodo
GET  /api/Persons/{id}/adaptive     → historial de ajustes de la persona
```

El handler `TriggerAdaptiveAdjustmentCommand` se dispara en el mismo pipeline que `CompleteActivityResponseCommandHandler`. Debe ser idempotente: si ya se ajustó para esa respuesta, no vuelve a ajustar.


