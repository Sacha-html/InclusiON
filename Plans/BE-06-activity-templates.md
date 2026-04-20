# Plan — BE-06: Sistema de Templates y Alta de Actividades

**Sprint:** 7 (próximo)
**Épica:** IN-9 Gestión de Actividades
**HU de referencia:** HU-02 — Sistema de Actividades con Templates Dinámicos
**Bloqueado por:** nada (es el bloqueante principal del flujo educativo)
**Desbloquea:** BE-09 Roadmap, FE-04/05 Wizard de alta, IN-105..IN-113

---

## Contexto y decisiones de diseño

El sistema de actividades sigue el mismo patrón que ya existe en el proyecto:
`SkillTemplate → SkillProfile` (blueprint → instancia por persona).

Para actividades la cadena es:

```
ActivityTemplateType   →   Activity (instancia)   →   ActivityResponse (respuesta del alumno)
  (catálogo/sistema)       (creada por profesional,      (ejecución por persona,
                            desde un type)                score, intentos)
```

**Decisión clave — contenido en JSON por tipo:**
El contenido de cada actividad se almacena como `jsonb` (PostgreSQL). Cada `ActivityTemplateType` define su `ContentSchema` (estructura esperada). El frontend lo usa para generar el formulario dinámico del wizard. Esta es la misma estrategia que usan plataformas educativas similares (Moodle, Duolingo) y evita una tabla por tipo de actividad.

**Visibilidad compartida:**
Las actividades tienen tres niveles de visibilidad: `Private` (solo el creador), `Institution` (toda la institución del creador), `Public` (todos los profesionales). Las actividades del sistema (`IsSystem = true`) son visibles para todos pero no editables.

---

## Dominio — Entidades nuevas

### 1. `ActivityTemplateType` (catálogo del sistema)

Define el tipo de template: qué formulario genera, qué componente lo renderiza.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `int` | PK |
| `SkillAreaId` | `int` FK | Área de habilidad a la que pertenece |
| `Code` | `string(50)` unique | `SELECT_FIGURE`, `VISUAL_SUM`, `MATCH_IMAGE_WORD`, `ORDER_SEQUENCE`, `FILL_LETTER`, `OPEN_TEXT` |
| `Name` | `string(150)` | Nombre legible |
| `Description` | `string(500)?` | Descripción del tipo |
| `ContentSchema` | `string` (jsonb) | JSON Schema que describe los campos del contenido |
| `ComponentName` | `string(100)` | Nombre del componente Angular que renderiza la actividad |
| `UsesPictograms` | `bool` | Si el template integra ARASAAC |
| `HasAudio` | `bool` | Si el template soporta audio |
| `DisplayOrder` | `int` | Orden en el selector del wizard |
| `IsActive` | `bool` | Estado lógico |

**Tipos iniciales (primera iteración):**

| Code | Área | Descripción |
|------|------|-------------|
| `SELECT_FIGURE` | Comunicación | Elegir la figura correcta entre opciones con pictogramas |
| `MATCH_IMAGE_WORD` | Alfabetización | Emparejar imagen con su palabra escrita |
| `ORDER_SEQUENCE` | Lógica-Matemática | Ordenar elementos en la secuencia correcta |
| `VISUAL_SUM` | Lógica-Matemática | Sumar contando objetos visuales |
| `FILL_LETTER` | Alfabetización | Completar la letra/palabra que falta |
| `OPEN_TEXT` | Cualquiera | Respuesta de texto libre (sin corrección automática) |

---

### 2. `Activity` (actividad creada por un profesional)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `Guid` | PK |
| `ActivityTemplateTypeId` | `int` FK | Tipo de template base |
| `CreatedByProfessionalId` | `Guid` FK | Profesional creador |
| `InstitutionId` | `int?` FK | Institución del creador al momento de crear (para visibilidad) |
| `Title` | `string(200)` | Título de la actividad |
| `Description` | `string(500)?` | Descripción opcional |
| `Content` | `string` (jsonb) | Contenido concreto (preguntas, opciones, respuestas, imágenes) |
| `ComplexityLevel` | `int (1-5)` | Nivel de dificultad |
| `EstimatedMinutes` | `int?` | Duración estimada en minutos |
| `RequiresSupervision` | `bool` | Si necesita que un profesional esté presente |
| `Visibility` | `enum` | `Private`, `Institution`, `Public` |
| `IsSystem` | `bool` | Actividad estándar del sistema (no editable) |
| `IsActive` | `bool` | Estado lógico |
| `CreatedAt` | `DateTime` | Fecha de creación |
| `UpdatedAt` | `DateTime?` | Última modificación |

**Regla de negocio:** no se puede desactivar una `Activity` que tenga asignaciones activas en un roadmap.

---

### `ContentSchema` por tipo — estructura JSON de referencia

El `ContentSchema` del `ActivityTemplateType` define qué campos debe tener el `Content` de cada `Activity`. El frontend lee este schema para generar el formulario del wizard paso 3.

**SELECT_FIGURE / MATCH_IMAGE_WORD:**
```json
{
  "instruction": "string",
  "options": [
    { "id": "string", "imageUrl": "string", "label": "string", "isCorrect": "bool" }
  ],
  "maxSelections": "int"
}
```

**ORDER_SEQUENCE:**
```json
{
  "instruction": "string",
  "items": [
    { "id": "string", "imageUrl": "string?", "label": "string", "correctPosition": "int" }
  ]
}
```

**VISUAL_SUM:**
```json
{
  "instruction": "string",
  "operandA": { "value": "int", "imageUrl": "string?" },
  "operandB": { "value": "int", "imageUrl": "string?" },
  "options": ["int"]
}
```

**FILL_LETTER:**
```json
{
  "word": "string",
  "missingIndex": "int",
  "imageUrl": "string?",
  "hint": "string?"
}
```

**OPEN_TEXT:**
```json
{
  "prompt": "string",
  "imageUrl": "string?",
  "maxLength": "int?"
}
```

---

## Backend — Plan de implementación (Clean Architecture)

### Capa Domain

```
InclusiON.Domain/Models/
  ActivityTemplateType.cs
  Activity.cs
```

Nada especial en la capa de dominio más allá de las propiedades. Las validaciones de negocio van en los command handlers.

### Capa Application

**Queries:**
```
GetActivityTemplateTypesQuery       → lista de types activos (filtrable por SkillAreaId)
GetActivitiesPagedQuery             → catálogo paginado (search, templateTypeId, skillAreaId, visibility, complexityLevel)
GetActivityByIdQuery                → detalle de una actividad
```

**Commands:**
```
CreateActivityCommand               → crea Activity desde un templateType
UpdateActivityCommand               → edita una Activity propia
DeactivateActivityCommand           → desactiva (valida que no tenga asignaciones activas)
```

**Interfaces:**
```
IActivitiesRepository
IActivityTemplateTypesRepository
```

### Capa Infrastructure

```
InclusiON.Infrastructure/Data/Repositories/
  ActivitiesRepository.cs
  ActivityTemplateTypesRepository.cs
```

`Content` y `ContentSchema` se mapean como `string` en EF Core con conversión de tipo, almacenados como `jsonb` en PostgreSQL.

**Configuración EF:**
```csharp
// En AppDbContext o IEntityTypeConfiguration<Activity>:
builder.Property(a => a.Content).HasColumnType("jsonb");
builder.Property(a => a.Visibility).HasConversion<string>();
```

**Migración:**
```
20260418_AddActivityTemplatesAndActivities
```
Incluye seed de los 6 `ActivityTemplateType` iniciales con sus `ContentSchema`.

### Capa API

```
Controllers/ActivitiesController.cs
  GET  /api/activity-template-types              → GetActivityTemplateTypesQuery
  GET  /api/activity-template-types/{id}/schema  → schema JSON del tipo

  GET  /api/activities                           → GetActivitiesPagedQuery (con filtros)
  GET  /api/activities/{id}                      → GetActivityByIdQuery
  POST /api/activities                           → CreateActivityCommand
  PUT  /api/activities/{id}                      → UpdateActivityCommand
  DELETE /api/activities/{id}                    → DeactivateActivityCommand
```

**Permisos:**
- `activity-templates:read` → cualquier profesional autenticado
- `activities:read` → profesional (ve propias + institución + públicas según visibilidad)
- `activities:create` → profesional
- `activities:update` → profesional (solo las propias)
- `activities:delete` → profesional (solo las propias, sin asignaciones activas)

**DTOs nuevos:**
```
ActivityTemplateTypeResponse        { Id, Code, Name, Description, SkillAreaId, SkillAreaName, ContentSchema, ComponentName, UsesPictograms, HasAudio }
ActivityListItemResponse            { Id, Title, TemplateTypeCode, TemplateTypeName, SkillAreaName, SkillAreaColor, ComplexityLevel, EstimatedMinutes, Visibility, IsSystem, CreatedByName }
ActivityDetailResponse              { ...ListItem, Description, Content, RequiresSupervision, CreatedAt, UpdatedAt }
CreateActivityRequest               { ActivityTemplateTypeId, Title, Description?, Content, ComplexityLevel, EstimatedMinutes?, RequiresSupervision, Visibility }
UpdateActivityRequest               { Title, Description?, Content, ComplexityLevel, EstimatedMinutes?, RequiresSupervision, Visibility }
```

---

## Orden de implementación

```
[ ] 1. Entidades Domain + configuración EF (Activity, ActivityTemplateType)
[ ] 2. Migración + seed de ActivityTemplateTypes con ContentSchema
[ ] 3. Repositorios (IActivitiesRepository, IActivityTemplateTypesRepository)
[ ] 4. Query: GetActivityTemplateTypesQuery
[ ] 5. Query: GetActivitiesPagedQuery + GetActivityByIdQuery
[ ] 6. Command: CreateActivityCommand (con validación de ContentSchema opcional en v1)
[ ] 7. Command: UpdateActivityCommand + DeactivateActivityCommand
[ ] 8. Controller + DTOs + permisos
[ ] 9. Registro DI en InfrastructureDependencyInjection.cs
```

---

## Frontend — Plan de implementación (FE-04/05, IN-105..109)

### Wizard de alta (IN-105)

4 pasos siguiendo la HU-02:

```
Paso 1 — Área de habilidad
  Grilla de cards con ícono y color por área (datos del catálogo /api/skill-areas)

Paso 2 — Tipo de template
  Cards filtradas por área, con nombre, descripción, íconos UsesPictograms/HasAudio

Paso 3 — Contenido
  Formulario dinámico generado a partir del ContentSchema del type seleccionado.
  Campos: texto, opciones con imágenes, buscador ARASAAC (integración futura IN-106).

Paso 4 — Metadatos
  Título, descripción, complejidad (1-5 estrellas), duración estimada,
  requiere supervisión, visibilidad (Privada / Institución / Pública)
```

**Componentes:**
```
activities/
  wizard/
    activity-wizard.component.ts          ← stepper principal
    steps/
      step-skill-area.component.ts
      step-template-type.component.ts
      step-content/
        activity-content-form.component.ts  ← dispatcher por type code
        forms/
          select-figure-form.component.ts
          match-image-word-form.component.ts
          order-sequence-form.component.ts
          visual-sum-form.component.ts
          fill-letter-form.component.ts
          open-text-form.component.ts
      step-metadata.component.ts
  list/
    activities-list.component.ts          ← catálogo con filtros
  detail/
    activity-detail.component.ts
```

### Catálogo (IN-107)

Tabla/grilla paginada con filtros: área de habilidad, tipo de template, nivel de complejidad, visibilidad. Badge de área con el color del catálogo. Estrellas para complejidad. Indicador "Sistema" para actividades no editables.

---

## Notas de arquitectura

- **Validación de Content en v1:** no se valida el JSON contra el schema en el backend (demasiada complejidad sin beneficio inmediato). El frontend garantiza la estructura mediante los formularios tipados por type. En v2 se puede agregar un `IContentValidator` por estrategia.
- **ARASAAC:** la integración de pictogramas (IN-106) va en un sprint posterior. En v1 se soporta `imageUrl` como campo de texto libre (URL externa o base64).
- **Seed de ActivityTemplateTypes:** va hardcodeado en una migración de datos (no en `HasData` de EF, para poder usar `jsonb` limpiamente).
- **RoadmapActivity** (BE-09): cuando se implemente el roadmap, agrega la tabla `RoadmapActivities` con FK a `Activity.Id`. La restricción de "no desactivar con asignaciones activas" se amplía para incluir esa tabla.
