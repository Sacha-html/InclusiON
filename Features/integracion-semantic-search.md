# Integración SemanticSearch en Clean Architecture

## Contexto

Ya existe `InclusiON.SemanticSearch/` como library separada con OnnxEmbeddingProvider, CosineSimilarityCalculator y WordPieceTokenizer. Ya existen las entidades ActivityEmbedding y ActivityResult con su migración en Data. Falta conectar todo al resto de la solución.

## Agregar en `ApplicationBusiness/Abstractions/`

- `IEmbeddingService.cs` — interfaz con `Task<float[]> GenerateEmbeddingAsync(string text)` y `Task<float[][]> GenerateEmbeddingsAsync(IEnumerable<string> texts)`
- `ISimilarityCalculator.cs` — interfaz con `float Calculate(float[] a, float[] b)`

## Agregar en `DTOs/Activities/`

- `ActivitySearchResultDto.cs` — Id, Name, Description, Score (similaridad), Tags

## Agregar en `ApplicationBusiness/Queries/Activities/SearchSemantic/`

- `SearchActivitiesSemanticQuery.cs` — query con SearchText, TopN, MinScore
- `SearchActivitiesSemanticQueryHandler.cs` — embebe el texto, compara contra ActivityEmbeddings via coseno, devuelve top N

## Modificar `CreateActivityCommandHandler`

- Después de guardar la Activity, generar embedding con IEmbeddingService y guardar en ActivityEmbedding

## Modificar `Api/Program.cs`

- Agregar `builder.Services.AddSemanticSearch(builder.Configuration)` (el extension method ya existe en SemanticSearch)

## Modificar `appsettings.json`

- Agregar sección:

```json
"SemanticSearch": {
  "Provider": "Onnx",
  "ModelPath": "Resources/Models/all-MiniLM-L6-v2.onnx",
  "VocabPath": "Resources/Models/vocab.txt",
  "EmbeddingDimension": 384
}
```

## Agregar endpoint en controller

- `GET /api/activities/search?text=...&topN=10` que ejecute el query handler

## Referencias de proyecto

- `Api` → referencia `SemanticSearch`
- `SemanticSearch` → referencia `ApplicationBusiness` (para implementar las interfaces)

## Patrón

Seguir el CQRS existente con ICommandHandler/IQueryHandler auto-registrados por reflection.
