# Plan MVP — Actividades + Embeddings + Player

**Objetivo:** Profesional crea actividad con contenido ARASAAC → sistema genera embedding → estudiante juega → sistema registra resultado.

---

## Paso 1 — pgvector + infraestructura de embeddings ✅

### 1.1 Docker
- [x] Cambiar imagen `postgres:17-alpine` → `pgvector/pgvector:pg17` en `docker/postgres/docker-compose.yml`

### 1.2 Paquetes NuGet
- [x] Agregar `Pgvector` (0.3.2) a `InclusiON.Data.csproj`
- [x] ~~`Microsoft.SemanticKernel` + `Microsoft.SemanticKernel.Connectors.Onnx`~~ **reemplazado por:**
  - `Microsoft.ML.OnnxRuntime` (1.21.0) — inferencia ONNX directa
  - `Microsoft.ML.Tokenizers` (0.22.0) — tokenización con `LlamaTokenizer` / `SentencePieceTokenizer`
- [x] Agregar referencia de `InclusiON.SemanticSearch` → `InclusiON.Application`
- [x] Remover referencia incorrecta `InclusiON.Application` → `InclusiON.SemanticSearch`

### 1.3 Modelo y configuración
- [x] Actualizar `ActivityEmbedding.cs`: `EmbeddingJson` (string) → `float[] Embedding` con `[NotMapped]`
- [x] Actualizar `ActivityEmbeddingConfiguration.cs`: ignorar `Embedding` en EF
- [x] Actualizar `DependencyInjection.cs`: `NpgsqlDataSourceBuilder` + `UseVector()`

### Nota de arquitectura — Modelo de embeddings
> **Modelo:** `paraphrase-multilingual-MiniLM-L12-v2` (sentence-transformers)
> - 384 dimensiones, multilingüe (funciona en español)
> - Orientado a similitud semántica de oraciones
> - Archivo ONNX: `model.onnx` (~448 MB)
> - Tokenizador: `sentencepiece.bpe.model` (~5 MB, SentencePiece BPE)
>
> **Por qué NO `BertOnnxTextEmbeddingGenerationService` de SemanticKernel:**
> Esa clase requiere WordPiece (`vocab.txt`). El modelo multilingüe usa SentencePiece.
> Incompatibilidad técnica que obliga a implementar el pipeline manualmente.
>
> **Implementación custom en `OnnxEmbeddingService`:**
> `LlamaTokenizer.Create(stream, addBOS: true, addEOS: true)` → ids → tensores → `InferenceSession.Run()` → mean pooling → normalización L2

### Nota de arquitectura — EF Core y pgvector
> **Por qué `[NotMapped]`:** `Pgvector.EntityFrameworkCore` solo soporta hasta Npgsql EF 8.x.
> Con Npgsql EF 10 la columna `vector(384)` se crea y escribe via raw SQL.
> EF gestiona todas las demás columnas normalmente.

### 1.4 Migración EF Core
- [x] Migración `AddPgvectorExtension`:
  - `CREATE EXTENSION IF NOT EXISTS vector;`
  - Drop columna `EmbeddingJson`
  - `ALTER TABLE "ActivityEmbeddings" ADD COLUMN "Embedding" vector(384);`
  - `CREATE INDEX USING hnsw ("Embedding" vector_cosine_ops)`

### 1.5 IEmbeddingService + archivos de modelo
- [x] `IEmbeddingService` en `InclusiON.Application/Interfaces/Infrastructure/`
- [x] `OnnxEmbeddingService` reescrito con OnnxRuntime + SentencePieceTokenizer (mean pooling + L2 norm)
- [x] Archivos de modelo descargados en `InclusiON.SemanticSearch/Model/`:
  - `model.onnx` (448 MB) — desde HuggingFace `paraphrase-multilingual-MiniLM-L12-v2/onnx/model.onnx`
  - `sentencepiece.bpe.model` (5 MB) — desde HuggingFace `paraphrase-multilingual-MiniLM-L12-v2/sentencepiece.bpe.model`
  - `tokenizer.json` (8.8 MB) — descargado pero no usado (referencia)
- [x] `InclusiON.Api.csproj`: los dos archivos de modelo se copian al output con `CopyToOutputDirectory=PreserveNewest`
- [x] Registrar `IEmbeddingService` → `OnnxEmbeddingService` (Singleton) en `SemanticSearchExtensions.cs`
- [x] Registrar en `Program.cs`: `builder.Services.AddSemanticSearch(builder.Configuration)`
- [x] Configurar en `appsettings.json`:
  ```json
  "SemanticSearch": {
    "ModelPath": "Model/model.onnx",
    "SentencePieceModelPath": "Model/sentencepiece.bpe.model"
  }
  ```

---

## Paso 2 — Backend Activity CRUD ✅

### 2.1 Comandos y queries
- [x] `CreateActivityCommand` + `CreateActivityCommandHandler`
- [x] `GetActivitiesQuery` + `GetActivitiesQueryHandler` (paginado, propias + estándar, filtros)
- [x] `GetActivityByIdQuery` + `GetActivityByIdQueryHandler`
- [x] `UpdateActivityCommand` + `UpdateActivityCommandHandler`
- [x] `PatchActivityStatusCommand` + `PatchActivityStatusCommandHandler`

### 2.2 DTOs
- [x] `CreateActivityRequest`, `UpdateActivityRequest`
- [x] `ActivityResponse` (detalle con ContentJson), `ActivityListItemResponse`

### 2.3 Endpoint
- [x] `ActivitiesController`: `POST`, `GET` (list), `GET` (id), `PUT`, `PATCH /status`

### 2.4 Repositorio
- [x] `IActivitiesRepository` + `ActivitiesRepository`

---

## Paso 3 — Backend Assignment + Response ✅

### 3.1 Asignación
- [x] `CreateActivityAssignmentCommand` + handler → `POST /api/activity-assignments`
- [x] `GetPersonActivityAssignmentsQuery` + handler → `GET /api/persons/{id}/activity-assignments`
- [x] Endpoint adicional → `GET /api/my/activity-assignments` (usa entityId del JWT — el estudiante no necesita conocer su propio GUID)

### 3.2 Ejecución (ActivityResponse lifecycle)
- [x] `StartActivityResponseCommand` + handler → `POST /api/activity-assignments/{id}/responses/start`
- [x] `CompleteActivityResponseCommand` + handler → `POST /api/activity-assignments/{id}/responses/{resId}/complete`

### Controller
- [x] `ActivityAssignmentsController` con todos los endpoints

---

## Paso 4 — Frontend Profesional ✅

### 4.1 ARASAAC Service
- [x] `ArasaacService` en `src/app/services/arasaac.service.ts`
  - `search(term): Observable<ArasaacPictogram[]>` → `GET https://api.arasaac.org/api/pictograms/es/search/{term}`
  - `getPictogramUrl(id): string` → `https://static.arasaac.org/pictograms/{id}/{id}_500.png`
- [x] Exportado desde `src/app/services/index.ts`

### 4.2 Modelos TS
- [x] `GetActivitiesRequest`, `CreateAssignmentRequest` en `models/requests/activities/`
- [x] `ActivityListItemResponse`, `ActivityAssignmentResponse`, `SelectFigureContent`, `SelectFigureItem` en `models/responses/activity.response.ts`

### 4.3 Actividades — gestión (`/pro/activities`)
- [x] Lista de actividades con filtros (categoría, tipo, estado, origen)
- [x] Wizard de alta — Paso 1: metadatos (título, categoría, área, complejidad, etc.)
- [x] Wizard de alta — Paso 2: template + contenido dinámico con picker ARASAAC para SELECT_FIGURE
- [x] Edición de actividad (ruta `/pro/activities/:id/edit`)
- [x] Botón "Asignar" en la tabla → `AssignActivityModalComponent`
  - Selector de persona/estudiante activo
  - Fecha límite (opcional, mínimo hoy)
  - Checkbox "Es actividad de evaluación"
  - Llama a `POST /api/activity-assignments`

### 4.4 Rutas profesional
- [x] `/pro/activities` → `ListComponent`
- [x] `/pro/activities/new` → `NewComponent`
- [x] `/pro/activities/:id/edit` → `EditComponent`

---

## Paso 5 — Frontend Estudiante (Player) ✅

### 5.1 Shell y lista
- [x] Lista de asignaciones (`/app/activities`) — llama a `GET /api/my/activity-assignments`
  - Muestra tarjetas por asignación con estado (coloreado)
  - Botón "Jugar" navega a `/app/activities/:assignmentId`
- [x] `ActivityPlayerShellComponent` — carga componente según `templateTypeCode`:
  - `SELECT_FIGURE` → `SelectFigurePlayerComponent`
  - otros tipos → mensaje "próximamente"

### 5.2 SELECT_FIGURE player
- [x] `SelectFigurePlayerComponent` — 3 fases: `intro` | `playing` | `result`
  - Fase `intro`: muestra título y botón "Empezar"
  - Fase `playing`: muestra instrucción + imágenes ARASAAC en grid
  - Selección: anima correcto (verde) / incorrecto (rojo), después de 900ms pasa a `result`
  - Fase `result`: feedback visual claro + botón "Continuar"
- [x] Lógica de respuesta:
  - `startResponse()` → `POST .../responses/start` → guarda `responseId`
  - `selectItem()` → compara con `content.correctItemId`, marca `isCorrect`
  - `finishActivity()` → `POST .../responses/{id}/complete` con `successPercentage: 100|0`, `timeSpentSeconds`
  - Al completar emite `completed` → `ActivityPlayerShellComponent` navega de vuelta a la lista

### 5.3 Ruta AAC
- [x] `/app/activities/:assignmentId` → `ActivityPlayerShellComponent`

---

## Resumen

| Paso | Descripción | Estado |
|------|-------------|--------|
| 1 | pgvector + IEmbeddingService + ONNX multilingüe | ✅ |
| 2 | Activity CRUD (BE) | ✅ |
| 3 | Assignment + Response (BE) | ✅ |
| 4 | FE Profesional (gestión + wizard ARASAAC + modal asignar) | ✅ |
| 5 | FE Estudiante (player SELECT_FIGURE) | ✅ |

---

## Pendiente (post-MVP)

- Búsqueda semántica: handler + endpoint + FE (`GET /api/activities/search?text=...`)
- Vista de resultados del profesional: tab en person-detail con historial de asignaciones
- Players adicionales: MATCH_PAIRS, ORDER_SEQUENCE, FILL_BLANK, etc.
- Roadmap visual estilo Duolingo
