# ABM — Actividades

**Actor:** Profesional  
**Justificación:** Las actividades son el contenido educativo central del sistema. El Profesional necesita crearlas, editarlas y organizarlas para luego asignarlas a sus personas en el roadmap. Sin este ABM, no hay contenido para trabajar, no hay roadmap posible y la plataforma no cumple su función pedagógica. El sistema también tiene actividades estándar (precargadas por el Admin), pero el Profesional puede crear actividades propias adaptadas a cada persona.

**Entidades:** `Activity`, `ActivityContent`

---

## Alta — Actividad

**Actor:** Profesional

### Paso 1: Datos generales

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Título | Texto (150) | Sí | No vacío |
| Categoría | Referencia | Sí | Debe existir y estar activa en catálogo |
| Área de habilidad | Referencia | No | Debe existir y estar activa si se ingresa |
| Descripción | Texto largo | No | — |
| Instrucciones | Texto largo | No | — |
| Soporte visual | Booleano | No | Por defecto: false |
| Soporte auditivo | Booleano | No | Por defecto: false |
| Lectura fácil | Booleano | No | Por defecto: false |
| Usa pictogramas | Booleano | No | Por defecto: false |
| URL de recursos | Texto (500) | No | URL válida si se ingresa |
| Duración estimada (min) | Entero | No | Positivo si se ingresa |
| Nivel de complejidad | Entero (1-5) | No | Entre 1 y 5 |
| Requiere supervisión | Booleano | Sí | Por defecto: false |

### Paso 2: Contenido interactivo (opcional)

Si la actividad tiene contenido interactivo (tipo template), se completa `ActivityContent`:

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Tipo de template | Referencia | Sí | Debe existir y estar activo |
| Contenido JSON | JSON | Sí | Debe ser un JSON válido que respete el esquema del template seleccionado |

**Validaciones de integridad:**
- Una actividad solo puede tener un `ActivityContent` (relación 1:1).
- El JSON de contenido debe validarse contra el `EsquemaDeContenido` del template.

**Resultado:**
- Se crea `Activity` con `EsActividadEstandar = false` (actividades creadas por profesionales nunca son estándar), `Activo = true`.
- Si se completó contenido interactivo, se crea `ActivityContent` vinculado.

---

## Baja — Actividad

**Actor:** Profesional (solo actividades propias, no estándar)

- Se establece `Activo = false` en `Activity` (baja lógica).
- **Validación:**
  - Solo se puede dar de baja una actividad cuyo `ProfesionalCreador` sea el profesional autenticado.
  - No se puede dar de baja si la actividad tiene `ActivityAssignment` con estado `Pendiente` o `EnProgreso`.
  - Las actividades estándar (`EsActividadEstandar = true`) solo pueden darse de baja por el Admin Global.

---

## Modificación — Actividad

**Actor:** Profesional (solo actividades propias)

Todos los campos del alta son editables.

**Restricción adicional:**
- Si la actividad ya fue asignada al menos una vez, el tipo de template de `ActivityContent` no puede cambiarse (el contenido JSON podría volverse inválido para respuestas ya registradas).
- El `ProfesionalCreador` no puede modificarse.

---

## Listado — Actividades

**Actor:** Profesional

| Columna | Descripción |
|---------|-------------|
| Título | Nombre de la actividad |
| Categoría | Del catálogo |
| Área de habilidad | Del catálogo |
| Nivel de complejidad | 1 a 5 |
| Duración estimada | En minutos |
| Tiene contenido interactivo | Sí / No |
| Estándar | Sí (del sistema) / No (propia) |
| Estado | Activo / Inactivo |

**Filtros disponibles:** título, categoría, área de habilidad, nivel de complejidad, tiene contenido interactivo, es estándar, estado.  
**Persistencia:** Consulta a `Activity`. El Profesional ve sus propias actividades + las estándar activas.  
**Admin Global:** ve todas las actividades del sistema.

---

## Players — Tipos de contenido interactivo

Cada `ActivityTemplateType` tiene un `Code` que mapea a un player Angular. La arquitectura es dinámica: el shell resuelve el componente en runtime vía `PLAYER_REGISTRY`.

### Arquitectura

```
ActivityPlayerShellComponent          ← carga asignación, resuelve player via ViewContainerRef
  └── PLAYER_REGISTRY[templateCode]   ← mapa Code → Type<PlayerBaseComponent>
        ├── PlayerBaseComponent        ← base abstracta (startResponse, completeResponse, timer)
        ├── PlayerIntroComponent       ← pantalla intro reutilizable
        ├── PlayerResultComponent      ← pantalla resultado reutilizable
        └── PictogramCardComponent     ← tarjeta imagen/picto con estados visuales
```

### Templates disponibles

| Code | Nombre | ContentJson shape | Estado |
|------|--------|-------------------|--------|
| `SELECT_FIGURE` | Seleccionar figura | `{ instruction, correctItemId, items: [{id, pictogramId, label}] }` | ✅ Player completo |
| `ORDER_SEQUENCE` | Ordenar secuencia | `{ instruction, items: [{id, label, pictogramId?, correctPosition}] }` | ✅ Player completo |
| `MATCH_IMAGE_WORD` | Emparejar imagen-palabra | `{ instruction, pairs: [{id, label, pictogramId}] }` | ✅ Player completo |
| `VISUAL_SUM` | Suma visual | `{ instruction, operandA, operandB, pictogramId?, options: [{id, value}] }` | ✅ Player completo |
| `COMPLETE_LETTER` | Completar letra | `{ instruction, word, hiddenIndices: number[], options: string[][] }` | ✅ Player completo |

### Agregar nuevo tipo

1. Definir `ContentJson` shape en `player.models.ts`
2. Crear componente en `views/aac/activities/player/<nuevo>/`  extendiendo `PlayerBaseComponent`
3. Agregar entrada en `player-registry.ts`
4. Agregar seed en `DatabaseSeeder` con `Code` y `ContentSchema`

El shell no necesita modificación.

---

## Wizard de creación — Arquitectura de editores

El paso 2 del wizard de alta de actividad resuelve el editor de contenido en runtime usando el mismo patrón registry que los players.

### Arquitectura

```
NewComponent (wizard shell)             ← monta editor via ViewContainerRef
  └── CONTENT_EDITOR_REGISTRY[code]     ← mapa Code → Type<ContentEditorBaseComponent>
        └── ContentEditorBaseComponent  ← base abstracta (@Directive)
              @Input()  initialJson: string    ← JSON actual (para edición futura)
              @Output() contentChange          ← emite JSON actualizado en cada cambio
              @Output() validChange            ← emite boolean de validez
```

El shell escucha `contentChange` y `validChange` para actualizar `editorContentJson` y `isEditorValid`. Al submit, usa `editorContentJson()` directamente como `contentJson` del request.

### Editores disponibles

| Code | Editor | Campos principales | Válido cuando |
|------|--------|-------------------|---------------|
| `SELECT_FIGURE` | `SelectFigureEditorComponent` | Instrucción + ítems (ARASAAC picker) + marcar correcta | ≥ 2 ítems, correcta marcada |
| `ORDER_SEQUENCE` | `OrderSequenceEditorComponent` | Instrucción + ítems con orden correcto (▲▼) + picto opcional | ≥ 2 ítems con etiqueta |
| `MATCH_IMAGE_WORD` | `MatchImageWordEditorComponent` | Instrucción + pares imagen–palabra (ARASAAC por par) | ≥ 2 pares completos |
| `VISUAL_SUM` | `VisualSumEditorComponent` | Instrucción + operandoA + operandoB + opciones auto-generadas | Instrucción + opciones incluyen respuesta correcta |
| `COMPLETE_LETTER` | `CompleteLetterEditorComponent` | Instrucción + palabra + toggle letras ocultas + distractores por hueco | ≥ 1 hueco, distractores completos |

### Agregar nuevo editor

1. Crear componente en `views/professional/activities/new/editors/<nuevo>/` extendiendo `ContentEditorBaseComponent`
2. Implementar `ngOnInit()` leyendo `this.initialJson`, y `emit()` emitiendo `contentChange` + `validChange`
3. Agregar entrada en `content-editor-registry.ts`

El wizard shell no necesita modificación.

---

## Búsqueda semántica

`GET /api/activities/search?text=<texto>&limit=10`

- Genera embedding del texto con `paraphrase-multilingual-MiniLM-L12-v2` (384 dims)
- Cosine similarity contra `ActivityEmbeddings` vía pgvector (`<=>` operator)
- Filtra activas + propias/estándar del profesional
- Devuelve `ActivityListItemResponse[]` ordenada por similitud
- FE: toggle "⚡ Búsqueda semántica" en lista de actividades, desactiva filtros tradicionales
