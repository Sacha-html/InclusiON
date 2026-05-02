# Integración SemanticSearch en Clean Architecture

## Estado actual

`InclusiON.SemanticSearch/` es una library independiente con:
- `OnnxEmbeddingService` — implementa `IEmbeddingService` usando OnnxRuntime + SentencePieceTokenizer
- `SemanticSearchExtensions` — extension method `AddSemanticSearch(IConfiguration)`
- Archivos de modelo en `Model/`: `model.onnx` (448 MB), `sentencepiece.bpe.model` (5 MB)

La biblioteca está integrada y funcional. **Falta:** el handler CQRS de búsqueda semántica y el endpoint.

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

## Lo que falta implementar

### Handler CQRS

En `InclusiON.Application/UseCases/Activities/Queries/`:

```
SearchActivitiesSemanticQuery.cs        — SearchText, TopN, MinScore
SearchActivitiesSemanticQueryHandler.cs — embebe texto, consulta ActivityEmbeddings via coseno, devuelve top N
```

El handler inyecta `IEmbeddingService` (para vectorizar la consulta) y usa raw SQL:

```sql
SELECT a.*, 1 - (ae."Embedding" <=> $1::vector) AS "Score"
FROM "ActivityEmbeddings" ae
JOIN "Activities" a ON a."Id" = ae."ActivityId"
WHERE 1 - (ae."Embedding" <=> $1::vector) > {minScore}
ORDER BY ae."Embedding" <=> $1::vector
LIMIT {topN};
```

### DTOs

`ActivitySearchResultDto` — Id, Title, CategoryName, TemplateTypeName, Score

### Endpoint

```
GET /api/activities/search?text=...&topN=10&minScore=0.6
```

En `ActivitiesController` (o nuevo `ActivitySearchController`).

---

## Dependencias entre proyectos

```
Api → SemanticSearch (para AddSemanticSearch en Program.cs)
SemanticSearch → Application (implementa IEmbeddingService)
Application → (solo interfaces, no referencia SemanticSearch)
```

---

## Cómo se genera un embedding al crear una actividad

En `CreateActivityCommandHandler`, después de guardar la actividad:

```csharp
var embedding = await _embeddingService.GenerateEmbeddingAsync(
    $"{activity.Title} {activity.Description} {contentJson}",
    cancellationToken);

// Persistir via raw SQL (pgvector no soporta EF 10)
await _rawDb.ExecuteAsync(
    "INSERT INTO \"ActivityEmbeddings\" (\"ActivityId\", \"Embedding\") VALUES ($1, $2::vector) ON CONFLICT (\"ActivityId\") DO UPDATE SET \"Embedding\" = $2::vector",
    activity.Id, $"[{string.Join(",", embedding)}]");
```

> **Estado:** la generación de embedding en `CreateActivityCommandHandler` está pendiente de implementación.
> El resto del pipeline (modelo, tokenizador, DI) está operativo.
