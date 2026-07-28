# Integración SemanticSearch en Clean Architecture

## Estado actual ✅ IMPLEMENTADO

`InclusiON.SemanticSearch/` es una library independiente con:
- `OnnxEmbeddingService` — implementa `IEmbeddingService` usando OnnxRuntime + SentencePieceTokenizer
- `SemanticSearchExtensions` — extension method `AddSemanticSearch(IConfiguration)`
- Archivos de modelo en `Model/`: `model.onnx` (448 MB), `sentencepiece.bpe.model` (5 MB)

La biblioteca está integrada y funcional. **Implementado:**
- Handler CQRS de búsqueda semántica
- Endpoints de actividades similares, personas compatibles y actividades recomendadas

---

## Stack de vectorización

| Componente | Tecnología |
|-----------|-----------|
| Modelo | `paraphrase-multilingual-MiniLM-L12-v2` (sentence-transformers) |
| Dimensiones | 384 |
| Idiomas | Multilingüe (incluye español) |
| Runtime | `Microsoft.ML.OnnxRuntime` 1.21.0 |
| Tokenizador | `Microsoft.ML.Tokenizers` 0.22.0 — `LlamaTokenizer` (SentencePiece BPE) |
| Archivo modelo | `Model/model.onnx` (~448 MB) |
| Archivo tokenizador | `Model/sentencepiece.bpe.model` (~5 MB) |
| Pooling | Mean pooling sobre `last_hidden_state` + normalización L2 |

> **Nota:** `BertOnnxTextEmbeddingGenerationService` de SemanticKernel solo soporta WordPiece (`vocab.txt`)
> y es incompatible con modelos SentencePiece. Por eso se usa OnnxRuntime directamente.

---

## Configuración (`appsettings.json`)

```json
"SemanticSearch": {
  "ModelPath": "Model/model.onnx",
  "SentencePieceModelPath": "Model/sentencepiece.bpe.model"
}
```

Los archivos se copian al output automáticamente desde `InclusiON.Api.csproj` (items con `CopyToOutputDirectory=PreserveNewest`).

---

## Interfaces en Application

- `IEmbeddingService` — `Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct)`
  - Ubicación: `InclusiON.Application/Interfaces/Infrastructure/IEmbeddingService.cs`

---

## Implementado

### Queries y Handlers

En `InclusiON.Application/UseCases/Activities/Queries/`:
- `GetSimilarActivitiesQuery.cs` — obtiene actividades similares a una dada
- `GetSimilarActivitiesQueryHandler.cs` — usa cosine similarity con pgvector
- `GetCompatiblePersonsQuery.cs` — busca personas compatibles para una actividad

En `InclusiON.Application/UseCases/Persons/Queries/`:
- `GetRecommendedActivitiesQuery.cs` — obtiene actividades recomendadas para una persona
- `GetRecommendedActivitiesQueryHandler.cs`

### Endpoints

| Endpoint | Descripción |
|----------|-------------|
| `GET /api/activities/{id}/similar` | Actividades similares (top 5) |
| `GET /api/activities/{id}/compatible-persons` | Personas compatibles para actividad (top 10) |
| `GET /api/persons/{id}/recommended-activities` | Actividades recomendadas para persona (top 10) |

### Repositorios

`IEmbeddingRepository` (en `Application/Interfaces/Infrastructure/`) con métodos:

| Método | Firma | Descripción |
|--------|-------|-------------|
| `GetByActivityIdAsync` | `Task<float[]?>` | Embedding de actividad por Id |
| `GetPersonEmbeddingAsync` | `Task<float[]?>` | Embedding de persona por Id |
| `SearchAsync` | `Task<List<(int Id, float Score)>>` | Búsqueda semántica — devuelve tuplas (Id, SimilarityScore coseno) ordenadas por similitud descendente |
| `SearchActivitiesForPersonAsync` | `Task<List<int>>` | Actividades recomendadas para persona |
| `SearchPersonsForActivityAsync` | `Task<List<Guid>>` | Personas compatibles con actividad |

**Parámetros de threshold en búsqueda:**

Los métodos de búsqueda aceptan `minSimilarity` con defaults centralizados en `EmbeddingThresholds.cs`:

```csharp
// InclusiON.Application/Constants/EmbeddingThresholds.cs
public static class EmbeddingThresholds
{
    public const float SemanticSearch  = 0.25f;  // búsqueda de texto libre
    public const float PersonActivity  = 0.20f;  // perfil persona ↔ actividad
    public const float SimilarActivity = 0.30f;  // actividad ↔ actividad similar
    public const float PersonForActivity = 0.20f; // actividad ↔ personas compatibles
}
```

**Score de similitud propagado al cliente:**

`ActivityListItemResponse` incluye `float? SimilarityScore` — solo presente en respuestas de búsqueda semántica, null en listados paginados normales.

### EmbeddingRepository

Implementación en `InclusiON.Infrastructure/Data/Repositories/EmbeddingRepository.cs` usa raw SQL con pgvector para similarity search.

- `SearchAsync` usa `= ANY($5::int[])` (parameterizado) para `excludeIds` — sin riesgo de SQL injection
- Retorna `(1 - (ae."Embedding" <=> $1::vector)) AS "Score"` para exponer similitud coseno al caller

---

## Dependencias entre proyectos

```
Api → SemanticSearch (para AddSemanticSearch en Program.cs)
SemanticSearch → Application (implementa IEmbeddingService)
Application → (solo interfaces, no referencia SemanticSearch)
```

---

## Cómo se genera un embedding al crear una actividad

La generación de embeddings se ejecuta via **Python Agent** en `Inclusion.Agent/main.py`:
- El backend encola un job de generación de embedding
- El agente Python (localhost:5050) procesa la cola y genera los vectores
- Los embeddings se almacenan en `ActivityEmbeddings` y `PersonEmbeddings`

### Flujo completo

1. **Backend** crea/actualiza actividad → encola job `JobTypes.Embedding`
2. **PendingJobsWorker** (BackgroundService) despacha a **EmbeddingAgent**
3. **EmbeddingAgent** llama `POST /embed` al agente Python
   - **4xx del agente Python** → `PermanentJobFailureException` → `JobExecutor` llama `FailAsync` inmediatamente (sin retry)
   - **5xx del agente Python** → retry hasta `MaxRetries = 3`
   - **Vector con dimensiones ≠ 384** → `PermanentJobFailureException` (dato corrupto, no vale reintentar)
4. **EmbeddingRepository** persiste en PostgreSQL via raw SQL
5. **Queries** de búsqueda semántica usan cosine similarity con pgvector

**PermanentJobFailureException** (`Application/Exceptions/`): excepción que señala al `JobExecutor` que el fallo no es transitorio — ir directo a `FailAsync` sin decrementar `RetryCount`.

> **Nota:** El modelo ONNX (`paraphrase-multilingual-MiniLM-L12-v2`) se ejecuta en el agente Python,
> no en el backend .NET. El backend solo almacena y consulta los vectores.
