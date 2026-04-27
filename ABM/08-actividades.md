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
