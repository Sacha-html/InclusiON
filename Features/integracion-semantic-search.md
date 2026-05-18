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

`IEmbeddingRepository` con métodos:
- `GetByActivityIdAsync` — obtener embedding de actividad
- `GetPersonEmbeddingAsync` — obtener embedding de persona
- `SearchPersonsForActivityAsync` — buscar personas por compatibilidad
- `SearchActivitiesAsync` — buscar actividades similares

### EmbeddingRepository

Implementación en `InclusiON.Infrastructure/Data/Repositories/EmbeddingRepository.cs` usa raw SQL con pgvector para similarity search.

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

1. **Backend** crea/actualiza actividad → encola job
2. **BackgroundJob** procesa y genera embedding
3. **EmbeddingRepository** persiste en PostgreSQL via raw SQL
4. **Queries** de búsqueda semántica usan cosine similarity con pgvector

> **Nota:** El modelo ONNX (`paraphrase-multilingual-MiniLM-L12-v2`) se ejecuta en el agente Python,
> no en el backend .NET. El backend solo almacena y consulta los vectores.
