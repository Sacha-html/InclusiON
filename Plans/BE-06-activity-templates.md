# Plan — BE-06: Sistema de Templates y Alta de Actividades

**Sprint:** 7 (próximo)
**Épica:** IN-9 Gestión de Actividades
**HU de referencia:** HU-02 — Sistema de Actividades con Templates Dinámicos
**Bloqueado por:** nada (es el bloqueante principal del flujo educativo)
**Desbloquea:** BE-09 Roadmap, FE-04/05 Wizard de alta, IN-105..IN-113

---

## Estado del dominio (ya existe en el código)

Las entidades de dominio **ya están definidas** en `InclusiON.Domain/Models/`. El trabajo de BE-06 es implementar los repositorios, queries, commands y endpoints — no crear entidades nuevas.

### Entidades existentes

| Entidad | Tabla | Descripción |
|---------|-------|-------------|
| `Activity` | `Activities` | Actividad pedagógica creada por un profesional |
| `ActivityContent` | `ActivityContents` | Contenido interactivo 1:1 con Activity. Tiene `TemplateTypeId` + `ContentJson` (jsonb) |
| `ActivityTemplateType` | `ActivityTemplateTypes` | Define la estructura del contenido. Vinculado a `SkillArea` |
| `ActivityCategory` | `ActivityCategories` | Clasificación temática independiente del SkillArea |
| `ActivityAssignment` | `ActivityAssignments` | Asignación de actividad a persona por un profesional |
| `ActivityResponse` | `ActivityResponses` | Respuesta/intento de una persona en una actividad |
| `ActivityEmbedding` | `ActivityEmbeddings` | Embedding semántico para búsqueda (BE-16) |
| `PersonRoadmapActivity` | `PersonRoadmapActivities` | Actividad dentro de un roadmap (BE-09) |

### DbSets registrados en AppDbContext

```csharp
DbSet<ActivityCategory>         ActivityCategories
DbSet<ActivityTemplateType>     ActivityTemplateTypes
DbSet<ActivityContent>          ActivityContents
DbSet<ActivityAssignment>       ActivityAssignments
DbSet<ActivityResponse>         ActivityResponses
DbSet<ActivityEmbedding>        ActivityEmbeddings
DbSet<PersonRoadmapActivity>    PersonRoadmapActivities
```

---

## Datos de seed (ya en la base)

### SkillAreas (3)

| Id | Nombre | Color | Ícono |
|----|--------|-------|-------|
| — | Comunicación | `#2E5FA3` | `chat` |
| — | Alfabetización | `#4CAF50` | `menu_book` |
| — | Lógico-matemático | `#FF9800` | `calculate` |

### ActivityTemplateTypes (8, agrupados por SkillArea)

| SkillArea | Code | Nombre | Pictogramas | Audio |
|-----------|------|--------|:-----------:|:-----:|
| Comunicación | `PICTOGRAM_SELECT` | Seleccionar pictograma | ✓ | ✓ |
| Comunicación | `OPTION_SELECT` | Selección de opciones | — | ✓ |
| Alfabetización | `GLOBAL_READING` | Lectura global | — | ✓ |
| Alfabetización | `SOUND_RECOGNITION` | Reconocer sonidos | — | ✓ |
| Alfabetización | `BUILD_WORD` | Armar palabras | — | — |
| Lógico-matemático | `CLASSIFY` | Clasificación | — | — |
| Lógico-matemático | `ORDER_SEQUENCE` | Ordenamiento | — | — |
| Lógico-matemático | `NUMERATION` | Numeración | — | — |

> ⚠️ Los campos `ContentSchema` y `ComponentName` están vacíos en el seed. Completarlos es parte de BE-06.

### ActivityCategories (8)

| Id | Nombre | Descripción resumida |
|----|--------|---------------------|
| 1 | Lectoescritura | Lectura, escritura, conciencia fonológica |
| 2 | Numeración y Matemática | Prenumeración, numeración, operaciones básicas |
| 3 | Habilidades Socioemocionales | Conducta, hábitos, rutinas, normas, historias sociales |
| 4 | Comunicación y Lenguaje | LSA, SAAC, sistemas aumentativos |
| 5 | Motricidad y Coordinación | Fina, gruesa, óculo-manual, orientación espacial |
| 6 | Creatividad y Expresión Artística | Música, plástica, dramatización |
| 7 | Autonomía y Vida Diaria | Vestimenta, higiene, dinero, noción del tiempo |
| 8 | Estimulación Cognitiva | Memoria, atención, percepción, resolución de problemas |

---

## Relaciones clave del modelo

```
Activity (int Id)
  ├── CategoryId → ActivityCategory       (clasificación temática)
  ├── SkillAreaId? → SkillArea            (área pedagógica, opcional)
  ├── ProfessionalId → Professional       (creador)
  ├── Content → ActivityContent (1:1)
  │     └── TemplateTypeId → ActivityTemplateType
  │           └── SkillAreaId → SkillArea
  ├── ActivityAssignments → ActivityAssignment[]
  │     ├── PersonId → PersonWithDisability
  │     └── Responses → ActivityResponse[]
  ├── RoadmapActivities → PersonRoadmapActivity[]
  └── Embedding → ActivityEmbedding (1:1, BE-16)
```

**Distinción CategoryId vs SkillAreaId:**
- `SkillAreaId` en Activity (y en ActivityTemplateType) define el **área pedagógica** que orienta qué templates están disponibles.
- `CategoryId` es la **clasificación temática** de la actividad (más granular, 8 opciones) y va en los metadatos del wizard.
- Un profesional que trabaja "Comunicación y Lenguaje" (categoría) puede usar templates de "Comunicación" (SkillArea).

---

## Arquitectura — lo que falta implementar

### 1. ContentSchema en ActivityTemplateTypes

El campo `ContentSchema` está vacío en el seed actual. Hay que completarlo con el JSON Schema de cada template. El frontend lo usará para generar el formulario del Paso 3.

**PICTOGRAM_SELECT / OPTION_SELECT:**
```json
{
  "instruction": "string",
  "options": [
    { "id": "string", "imageUrl": "string?", "label": "string", "isCorrect": "bool" }
  ],
  "maxSelections": "int"
}
```

**GLOBAL_READING:**
```json
{
  "word": "string",
  "imageUrl": "string?",
  "options": ["string"],
  "correctOption": "string"
}
```

**SOUND_RECOGNITION:**
```json
{
  "audioUrl": "string",
  "letter": "string",
  "options": ["string"],
  "correctOption": "string"
}
```

**BUILD_WORD:**
```json
{
  "imageUrl": "string?",
  "word": "string",
  "shuffledLetters": ["string"]
}
```

**CLASSIFY:**
```json
{
  "instruction": "string",
  "criteria": "string",
  "items": [{ "id": "string", "imageUrl": "string?", "label": "string", "group": "string" }],
  "groups": ["string"]
}
```

**ORDER_SEQUENCE / NUMERATION:**
```json
{
  "instruction": "string",
  "items": [{ "id": "string", "imageUrl": "string?", "label": "string", "correctPosition": "int" }]
}
```

---

## Backend — plan de implementación

### Paso 1 — Migración: completar ContentSchema + ComponentName
Crear migración de datos que actualice los 8 `ActivityTemplateTypes` con sus `ContentSchema` y `ComponentName`.

### Paso 2 — Repositorios

```
IActivitiesRepository
  GetPagedAsync(page, pageSize, search, categoryId?, skillAreaId?, complexityLevel?, isStandard?, professionalId?)
  GetByIdAsync(int id)
  CreateAsync(Activity activity)
  UpdateAsync(Activity activity)
  DeactivateAsync(int id) — valida que no tenga ActivityAssignments activas

IActivityContentsRepository
  GetByActivityIdAsync(int activityId)
  CreateAsync(ActivityContent content)
  UpdateAsync(ActivityContent content)
```

### Paso 3 — Queries

```
GetActivityTemplateTypesQuery     → lista activos, filtrable por skillAreaId
GetActivityCategoriesQuery        → lista activos (catálogo estático)
GetActivitiesPagedQuery           → catálogo paginado con filtros
GetActivityByIdQuery              → detalle + content + templateType
```

### Paso 4 — Commands

```
CreateActivityCommand
  → crea Activity + ActivityContent en una sola transacción
  → valida: categoryId existe, templateTypeId existe, ContentJson no vacío

UpdateActivityCommand
  → solo el creador puede actualizar
  → actualiza Activity + ActivityContent

DeactivateActivityCommand
  → solo el creador puede desactivar
  → falla si hay ActivityAssignments con Status != Cancelada
```

### Paso 5 — Controller y DTOs

```
GET  /api/activity-template-types                → GetActivityTemplateTypesQuery
GET  /api/activity-categories                    → GetActivityCategoriesQuery

GET  /api/activities                             → GetActivitiesPagedQuery
GET  /api/activities/{id}                        → GetActivityByIdQuery
POST /api/activities                             → CreateActivityCommand
PUT  /api/activities/{id}                        → UpdateActivityCommand
DELETE /api/activities/{id}                      → DeactivateActivityCommand
```

**DTOs:**
```
ActivityTemplateTypeResponse  { Id, Code, Name, Description, SkillAreaId, SkillAreaName, SkillAreaColor, ContentSchema, ComponentName, UsesPictograms, HasAudio }
ActivityCategoryResponse      { Id, Name, Description }

ActivityListItemResponse      { Id, Title, CategoryId, CategoryName, SkillAreaId, SkillAreaName, SkillAreaColor, TemplateTypeCode, ComplexityLevel, EstimatedDurationMinutes, IsStandardActivity, ProfessionalName, CreatedAt }
ActivityDetailResponse        { ...ListItem, Description, Instructions, Content: { TemplateTypeId, ContentJson }, HasVisualSupport, HasAudioSupport, UsesEasyReading, UsesPictograms, RequiresSupervision, ResourcesUrl }

CreateActivityRequest         { Title, Description?, Instructions?, CategoryId, SkillAreaId?, ContentTemplateTypeId, ContentJson, ComplexityLevel?, EstimatedDurationMinutes?, RequiresSupervision, HasVisualSupport, HasAudioSupport, UsesEasyReading, UsesPictograms, ResourcesUrl?, IsStandardActivity }
UpdateActivityRequest         { Title, Description?, Instructions?, CategoryId, ContentJson, ComplexityLevel?, EstimatedDurationMinutes?, RequiresSupervision, HasVisualSupport, HasAudioSupport, UsesEasyReading, UsesPictograms, ResourcesUrl? }
```

**Permisos:**
- `activity-templates:read` → cualquier profesional autenticado
- `activities:read` → profesional autenticado (ve propias + estándar del sistema)
- `activities:create` → profesional
- `activities:update` → profesional (solo las propias, no las `IsStandardActivity`)
- `activities:delete` → profesional (solo las propias sin asignaciones activas)

---

## Frontend — Wizard (FE-04/05)

### Flujo de 4 pasos

```
Paso 1 — Área de habilidad (SkillArea)
  3 cards: Comunicación (#2E5FA3), Alfabetización (#4CAF50), Lógico-matemático (#FF9800)

Paso 2 — Tipo de template (ActivityTemplateType)
  Cards filtradas por SkillArea seleccionada
  Comunicación:      PICTOGRAM_SELECT, OPTION_SELECT
  Alfabetización:    GLOBAL_READING, SOUND_RECOGNITION, BUILD_WORD
  Lógico-matemático: CLASSIFY, ORDER_SEQUENCE, NUMERATION

Paso 3 — Contenido (ActivityContent.ContentJson)
  Formulario dinámico generado del ContentSchema del template seleccionado
  Componentes por template code:
    pictogram-select-form, option-select-form, global-reading-form,
    sound-recognition-form, build-word-form, classify-form,
    order-sequence-form, numeration-form

Paso 4 — Metadatos
  Título *, Instrucciones (textarea)
  Categoría * (dropdown 8 opciones de ActivityCategory)
  Complejidad 1-5 (estrellas)
  Duración estimada (minutos)
  Requiere supervisión (checkbox)
  Flags de accesibilidad:
    □ Soporte visual (imágenes/video)
    □ Soporte auditivo (audio/narración)
    □ Lectura fácil
    □ Pictogramas
```

### Estructura de componentes

```
activities/
  wizard/
    activity-wizard.component.ts
    steps/
      step-skill-area.component.ts
      step-template-type.component.ts
      step-content/
        activity-content-form.component.ts   ← dispatcher por code
        forms/
          pictogram-select-form.component.ts
          option-select-form.component.ts
          global-reading-form.component.ts
          sound-recognition-form.component.ts
          build-word-form.component.ts
          classify-form.component.ts
          order-sequence-form.component.ts
          numeration-form.component.ts
      step-metadata.component.ts
  list/
    activities-list.component.ts
  detail/
    activity-detail.component.ts
```

---

## Orden de implementación

```
[ ] 1. Migración de datos: completar ContentSchema + ComponentName en los 8 templates
[ ] 2. IActivitiesRepository + IActivityContentsRepository (interfaces)
[ ] 3. ActivitiesRepository + ActivityContentsRepository (implementaciones)
[ ] 4. GetActivityTemplateTypesQuery + GetActivityCategoriesQuery
[ ] 5. GetActivitiesPagedQuery + GetActivityByIdQuery
[ ] 6. CreateActivityCommand (Activity + ActivityContent en transacción)
[ ] 7. UpdateActivityCommand + DeactivateActivityCommand
[ ] 8. ActivitiesController + DTOs + registro en DI
[ ] 9. FE: wizard 4 pasos + catálogo
```
