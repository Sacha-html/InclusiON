# InclusiON — Motor de Dificultad Adaptativa (MDA)

## Especificación Técnica

**Institución Cervantes — Analista de Sistemas**
**Prácticas Profesionalizantes**
**Versión 1.1 — Marzo 2026**

---

## 1. Resumen Ejecutivo

El Motor de Dificultad Adaptativa (MDA) es un sistema de reglas que analiza el desempeño del estudiante en cada actividad y ajusta automáticamente los parámetros de configuración personalizada (dificultad, tiempo límite, pistas, intentos permitidos) dentro de rangos definidos por el profesional.

El motor opera como un bucle continuo: el estudiante ejecuta una actividad, el sistema evalúa el resultado, aplica reglas de adaptación y configura la siguiente ejecución. Esto traduce directamente la necesidad pedagógica de "objetivos pequeños y progresivos" a un mecanismo técnico determinista, sin dependencias externas.

### Objetivos del Motor

- Ajustar la dificultad en tiempo real para mantener al estudiante en su zona de desarrollo próximo.
- Prevenir frustración detectando patrones negativos y actuando antes de que el estudiante abandone.
- Respetar los límites definidos por el profesional: el motor nunca excede los rangos configurados.
- Generar trazabilidad completa de cada ajuste para análisis posterior del profesional.
- Operar de forma autocontenida: sin APIs externas, sin LLM, sin latencia ni costo adicional.

---

## 2. Integración con Entidades Existentes

El motor se integra con la jerarquía de tres niveles ya definida y la entidad de respuestas existente. No modifica `PersonRoadmapActivity`; agrega dos tablas nuevas: `AdaptiveEngineConfig` (configuración de rangos, relación 1:0..1) y `AdaptiveAdjustmentLog` (auditoría de ajustes).

### Entidades involucradas

| Entidad | Campos relevantes | Rol en el motor |
|---------|-------------------|-----------------|
| `PersonRoadmapActivity` | DifficultyLevel, TimeLimitSeconds, MaxAttempts, ShowHints, UnlockThresholdPercent | **Destino de los ajustes.** El motor modifica estos campos dentro de los rangos de AdaptiveEngineConfig. No se agregan columnas a esta tabla. |
| `AdaptiveEngineConfig` **NUEVA** | MinDifficultyLevel, MaxDifficultyLevel, Min/MaxTimeLimitSeconds, umbrales de éxito/fallo/frustración | **Reglas y rangos del motor.** Relación 1:0..1 con PersonRoadmapActivity. Si no existe, la actividad opera sin motor. |
| `AdaptiveAdjustmentLog` **NUEVA** | AdjustmentType, PreviousValue, NewValue, Reason, AdjustedAt | **Auditoría.** Cada ajuste del motor se registra aquí para trazabilidad y visualización de progreso. |
| `ActivityResponse` | SuccessPercentage, TimeSpentSeconds, AttemptCount, FrustrationLevel, ResponsePattern | **Fuente de datos.** El motor lee el historial de respuestas para decidir el ajuste. |
| `PersonRoadmapArea` | SkillAreaId, DisplayOrder | **Contexto.** Permite al motor evaluar progreso por área de habilidad. |
| `PersonRoadmap` | PersonId, CreatedByProfessionalId | **Cabecera.** Identifica a qué persona pertenece la configuración. |

---

## 3. Entidad Nueva: AdaptiveEngineConfig (1:0..1 con PersonRoadmapActivity)

La configuración del motor adaptativo se separa en su propia entidad con relación 1:0..1 contra `PersonRoadmapActivity`. Esta decisión responde a dos principios arquitectónicos:

- **Separación de responsabilidades:** `PersonRoadmapActivity` define qué actividad y cómo se configura para la persona (tiempo, hints, dificultad). `AdaptiveEngineConfig` define cómo el motor automático gestiona esos parámetros (rangos, umbrales, reglas).
- **Opcionalidad real:** No todas las actividades usan el motor. Si `AdaptiveConfig` es `null`, la actividad funciona con configuración estática. Sin columnas vacías, sin flags booleanos.

### Relación con PersonRoadmapActivity

```
PersonRoadmapActivity (existente, sin cambios)
├── SequenceOrder, IsUnlocked, UnlockedAt
├── TimeLimitSeconds, MaxAttempts, ShowHints, DifficultyLevel
└── AdaptiveConfig?  ──► AdaptiveEngineConfig (1:0..1)
                         ├── MinDifficultyLevel
                         ├── MaxDifficultyLevel
                         ├── MinTimeLimitSeconds
                         ├── MaxTimeLimitSeconds
                         ├── ConsecutiveSuccessToUpgrade
                         ├── ConsecutiveFailuresToDowngrade
                         ├── SuccessThresholdPercent
                         ├── FrustrationThreshold
                         └── IsEnabled
```

El motor lee los valores actuales (DifficultyLevel, TimeLimitSeconds, etc.) de `PersonRoadmapActivity`, y los rangos/reglas de `AdaptiveEngineConfig`. Cuando ajusta, modifica `PersonRoadmapActivity` dentro de los límites definidos en la config.

### Definición de campos

| Campo | Tipo | Default | Descripción |
|-------|------|---------|-------------|
| `Id` | int | PK | PK autoincremental. |
| `PersonRoadmapActivityId` | int | FK | FK Unique a PersonRoadmapActivity. Relación 1:0..1. |
| `IsEnabled` | bool | true | Master switch. Permite pausar el motor sin eliminar la config. |
| `MinDifficultyLevel` | int | 1 | Piso de dificultad. El motor nunca baja por debajo de este valor. |
| `MaxDifficultyLevel` | int | 5 | Techo de dificultad. El motor nunca sube por encima de este valor. |
| `MinTimeLimitSeconds` | int? | null | Tiempo mínimo. null = sin límite inferior (el motor puede quitar el timer). |
| `MaxTimeLimitSeconds` | int? | null | Tiempo máximo. null = sin límite de tiempo. |
| `ConsecutiveSuccessToUpgrade` | int | 3 | Aciertos consecutivos (>= SuccessThresholdPercent) para subir dificultad. |
| `ConsecutiveFailuresToDowngrade` | int | 2 | Fallos consecutivos (< SuccessThresholdPercent) para bajar dificultad. |
| `SuccessThresholdPercent` | int | 70 | Porcentaje mínimo de SuccessPercentage para considerar un intento como "éxito". |
| `FrustrationThreshold` | int | 3 | Nivel de FrustrationLevel (1-5) que dispara intervención inmediata. |

### Especificación de entidad C#

```csharp
public class AdaptiveEngineConfig : AuditableBaseEntity
{
    public int Id { get; set; }
    public int PersonRoadmapActivityId { get; set; }

    public bool IsEnabled { get; set; } = true;

    // ── Rangos de dificultad ──
    public int MinDifficultyLevel { get; set; } = 1;
    public int MaxDifficultyLevel { get; set; } = 5;

    // ── Rangos de tiempo ──
    public int? MinTimeLimitSeconds { get; set; }
    public int? MaxTimeLimitSeconds { get; set; }

    // ── Umbrales del motor ──
    public int ConsecutiveSuccessToUpgrade { get; set; } = 3;
    public int ConsecutiveFailuresToDowngrade { get; set; } = 2;
    public int SuccessThresholdPercent { get; set; } = 70;
    public int FrustrationThreshold { get; set; } = 3;

    // ── Navegación ──
    public virtual PersonRoadmapActivity PersonRoadmapActivity { get; set; } = null!;
}
```

### Configuración Fluent API

```csharp
public class AdaptiveEngineConfigConfiguration
    : IEntityTypeConfiguration<AdaptiveEngineConfig>
{
    public void Configure(EntityTypeBuilder<AdaptiveEngineConfig> b)
    {
        b.ToTable("AdaptiveEngineConfigs");
        b.HasKey(x => x.Id);

        // 1:0..1 con PersonRoadmapActivity
        b.HasOne(x => x.PersonRoadmapActivity)
         .WithOne(x => x.AdaptiveConfig)
         .HasForeignKey<AdaptiveEngineConfig>(x => x.PersonRoadmapActivityId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.PersonRoadmapActivityId).IsUnique();

        // Restricciones
        b.Property(x => x.MinDifficultyLevel).HasDefaultValue(1);
        b.Property(x => x.MaxDifficultyLevel).HasDefaultValue(5);
        b.Property(x => x.ConsecutiveSuccessToUpgrade).HasDefaultValue(3);
        b.Property(x => x.ConsecutiveFailuresToDowngrade).HasDefaultValue(2);
        b.Property(x => x.SuccessThresholdPercent).HasDefaultValue(70);
        b.Property(x => x.FrustrationThreshold).HasDefaultValue(3);
        b.Property(x => x.IsEnabled).HasDefaultValue(true);
    }
}
```

### Navegación inversa en PersonRoadmapActivity

Agregar en `PersonRoadmapActivity.cs`:

```csharp
public virtual AdaptiveEngineConfig? AdaptiveConfig { get; set; }
```

---

## 4. Entidad Nueva: AdaptiveAdjustmentLog

Cada ajuste que realiza el motor se registra en esta tabla para trazabilidad completa. El profesional puede ver el historial de adaptaciones, entender cómo evolucionó la configuración, y visualizarlo en gráficas de progreso.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | int | PK autoincremental. |
| `PersonRoadmapActivityId` | int | FK a la actividad del roadmap que fue ajustada. |
| `ActivityResponseId` | int | FK a la respuesta que disparó el ajuste. |
| `AdjustmentType` | string | "DifficultyUp", "DifficultyDown", "HintsEnabled", "HintsDisabled", "TimeLimitIncreased", "TimeLimitDecreased", "AttemptsIncreased", "FrustrationIntervention" |
| `PreviousValue` | string | Valor anterior serializado como JSON. Ej: `{"DifficultyLevel": 2}` |
| `NewValue` | string | Valor nuevo serializado como JSON. Ej: `{"DifficultyLevel": 3}` |
| `Reason` | string | Explicación legible. Ej: "3 aciertos consecutivos con SuccessPercentage >= 70%" |
| `AdjustedAt` | DateTime | Timestamp UTC del ajuste. |

---

## 5. Máquina de Estados del Motor

El motor opera con una máquina de estados simple que clasifica la tendencia del estudiante en una actividad específica. Los estados determinan qué reglas de ajuste se aplican.

| Estado | Condición de entrada | Acción del motor |
|--------|---------------------|------------------|
| **ESTABLE** | Estado inicial, o cuando no se cumplen condiciones de los otros estados. | Sin cambios. El motor mantiene la configuración actual. |
| **PROGRESANDO** | N respuestas consecutivas con SuccessPercentage >= SuccessThresholdPercent (N = ConsecutiveSuccessToUpgrade). | Subir DifficultyLevel (+1, máx MaxDifficultyLevel). Opcionalmente: reducir TimeLimitSeconds, desactivar ShowHints, reducir MaxAttempts. |
| **DIFICULTAD** | N respuestas consecutivas con SuccessPercentage < SuccessThresholdPercent (N = ConsecutiveFailuresToDowngrade). | Bajar DifficultyLevel (-1, mín MinDifficultyLevel). Opcionalmente: aumentar TimeLimitSeconds, activar ShowHints, aumentar MaxAttempts. |
| **FRUSTRACIÓN** | FrustrationLevel de la última respuesta >= FrustrationThreshold, O 3+ abandonos consecutivos (TimeSpentSeconds < 10s con SuccessPercentage = 0). | Intervención inmediata: bajar dificultad al mínimo, activar hints, maximizar tiempo, maximizar intentos. Generar alerta al profesional. |

### Diagrama de transiciones

```
                    ┌──────────────────────┐
                    │       ESTABLE        │
                    │  (sin cambios)       │
                    └──────┬───────┬───────┘
              éxitos ≥ N   │       │   fallos ≥ N
                    ┌──────▼──┐  ┌─▼────────┐
                    │PROGRESANDO│  │DIFICULTAD│
                    │ (sube)   │  │ (baja)   │
                    └──────┬──┘  └──┬───────┘
                           │        │
                    primer fallo /  primer éxito
                    ───────► ESTABLE ◄───────
                                │
                    frustración detectada
                    ┌───────────▼──────────┐
                    │     FRUSTRACIÓN      │
                    │ (intervención total)  │
                    └──────────────────────┘
```

---

## 6. Reglas de Ajuste Detalladas

### 6.1 Regla de Escalamiento (PROGRESANDO)

Cuando el estudiante demuestra dominio sostenido, el motor escala la complejidad de forma gradual. Los ajustes se aplican en orden de prioridad: primero dificultad, luego tiempo, luego hints, luego intentos.

| Prioridad | Ajuste | Condición adicional |
|-----------|--------|---------------------|
| 1 | `DifficultyLevel += 1` (máx MaxDifficultyLevel) | Siempre se intenta primero. Si ya está en el máximo, pasa a prioridad 2. |
| 2 | `TimeLimitSeconds -= 10%` (mín MinTimeLimitSeconds) | Solo si TimeLimitSeconds no es null y el estudiante completó con >30% del tiempo sobrante. |
| 3 | `ShowHints = false` | Solo si ShowHints es true y las últimas 3 respuestas no usaron hints (RequiredSupport = false). |
| 4 | `MaxAttempts -= 1` (mín 1) | Solo si las últimas N respuestas tuvieron AttemptCount = 1 (resolvió al primer intento). |

### 6.2 Regla de Desescalamiento (DIFICULTAD)

Cuando el estudiante muestra dificultad sostenida, el motor facilita la experiencia de forma gradual. Prioridad inversa: primero da más soporte, luego baja dificultad.

| Prioridad | Ajuste | Condición adicional |
|-----------|--------|---------------------|
| 1 | `ShowHints = true` | Si ShowHints es false, se activa primero como soporte básico. |
| 2 | `MaxAttempts += 1` (máx 5) | Dar más oportunidades antes de reducir dificultad. |
| 3 | `TimeLimitSeconds += 15%` (máx MaxTimeLimitSeconds) | Solo si TimeLimitSeconds no es null. |
| 4 | `DifficultyLevel -= 1` (mín MinDifficultyLevel) | Último recurso: solo si los ajustes 1-3 ya fueron aplicados previamente sin mejora. |

### 6.3 Regla de Intervención por Frustración

Cuando se detecta frustración, la intervención es inmediata y total. No es gradual: se restablece todo al nivel más accesible dentro de los rangos permitidos.

1. `DifficultyLevel = MinDifficultyLevel`
2. `ShowHints = true`
3. `TimeLimitSeconds = MaxTimeLimitSeconds` (o null si no hay límite)
4. `MaxAttempts = 5` (o null para ilimitado)
5. Se genera una notificación al profesional vía SignalR (si está conectado) o flag en dashboard.

---

## 7. Arquitectura del Pipeline (CQRS)

El handler de completar respuesta ejecuta múltiples operaciones que deben correr en la misma transacción. En vez de acumular lógica en un solo método, se usa un patrón de pipeline con steps ordenados.

### Flujo completo

```
Controller recibe POST /complete
  → Command llega al Handler (auto-registrado por reflexión)
    → Handler carga datos + arma contexto
    → Pipeline (steps resueltos por DI, ordenados por Order):
        Step 1: PersistResponseStep         ← Crea/completa el ActivityResponse
        Step 2: AdaptiveEngineStep          ← Evalúa historial, ajusta config, escribe log
        Step 3: UnlockNextActivityStep      ← Desbloquea la siguiente actividad si corresponde
        Step 4: FrustrationAlertStep        ← Genera notificación si hubo frustración
    → UoW.CommitAsync() (1 sola transacción)
    → Post-commit: SignalR si hay alerta
  → Response al frontend con resultado + ajuste
```

### Ubicación en la arquitectura

```
InclusiON.ApplicationBusiness/
├── Interfaces/
│   └── Pipeline/
│       ├── IPostCompletionStep.cs           ← contrato del step
│       └── ActivityCompletionContext.cs      ← contexto compartido
├── Handlers/
│   └── ActivityResponses/
│       └── CompleteActivityResponseCommandHandler.cs
└── Steps/
    └── ActivityCompletion/
        ├── PersistResponseStep.cs           ← Order: 1
        ├── AdaptiveEngineStep.cs            ← Order: 2
        ├── UnlockNextActivityStep.cs        ← Order: 3
        └── FrustrationAlertStep.cs          ← Order: 4

InclusiON.Infrastructure/
└── Services/
    └── AdaptiveEngineService.cs             ← implementación del motor

InclusiON.Api/
└── Extensions/
    └── ServiceCollectionExtensions.cs       ← registro DI
```

### Contrato del Step

```csharp
// ApplicationBusiness/Interfaces/Pipeline/IPostCompletionStep.cs

public interface IPostCompletionStep
{
    int Order { get; }
    Task ExecuteAsync(ActivityCompletionContext context, CancellationToken ct);
}
```

### Contexto compartido

```csharp
// ApplicationBusiness/Interfaces/Pipeline/ActivityCompletionContext.cs

public class ActivityCompletionContext
{
    // ── Entrada (el handler la arma antes del pipeline) ──
    public int AssignmentId { get; set; }
    public int ResponseId { get; set; }
    public ActivityResponse Response { get; set; } = null!;
    public PersonRoadmapActivity RoadmapActivity { get; set; } = null!;
    public AdaptiveEngineConfig? AdaptiveConfig { get; set; }

    // ── Lo que aporta el command ──
    public decimal SuccessPercentage { get; set; }
    public int TimeSpentSeconds { get; set; }
    public string? ResponsePattern { get; set; }
    public int? FrustrationLevel { get; set; }

    // ── Resultados intermedios (cada step escribe) ──
    public AdaptiveAdjustmentResult? AdaptiveResult { get; set; }
    public bool NextActivityUnlocked { get; set; }
    public int? UnlockedActivityId { get; set; }
    public bool FrustrationAlertGenerated { get; set; }
}
```

### Handler orquestador

```csharp
// ApplicationBusiness/Handlers/ActivityResponses/
//   CompleteActivityResponseCommandHandler.cs

public class CompleteActivityResponseCommandHandler
    : ICommandHandler<CompleteActivityResponseCommand,
                      CompleteActivityResponseResult>
{
    private readonly IUnitOfWork _uow;
    private readonly IEnumerable<IPostCompletionStep> _steps;
    private readonly INotificationService _notifications;

    public CompleteActivityResponseCommandHandler(
        IUnitOfWork uow,
        IEnumerable<IPostCompletionStep> steps,
        INotificationService notifications)
    {
        _uow = uow;
        _steps = steps;
        _notifications = notifications;
    }

    public async Task<CompleteActivityResponseResult> Handle(
        CompleteActivityResponseCommand command,
        CancellationToken ct)
    {
        // ── Cargar datos necesarios ──
        var assignment = await _uow.ActivityAssignments
            .GetWithRoadmapAndConfig(command.AssignmentId, ct);

        // ── Armar contexto ──
        var context = new ActivityCompletionContext
        {
            AssignmentId = command.AssignmentId,
            ResponseId = command.ResponseId,
            RoadmapActivity = assignment.RoadmapActivity,
            AdaptiveConfig = assignment.RoadmapActivity?.AdaptiveConfig,
            SuccessPercentage = command.SuccessPercentage,
            TimeSpentSeconds = command.TimeSpentSeconds,
            ResponsePattern = command.ResponsePattern,
            FrustrationLevel = command.FrustrationLevel
        };

        // ── Ejecutar pipeline en orden ──
        foreach (var step in _steps.OrderBy(s => s.Order))
            await step.ExecuteAsync(context, ct);

        // ── Commit (todo en una transacción) ──
        await _uow.CommitAsync(ct);

        // ── Post-commit: notificaciones fuera de transacción ──
        if (context.FrustrationAlertGenerated)
            await _notifications.NotifyProfessionalAsync(
                assignment.RoadmapActivity.PersonRoadmap.CreatedByProfessionalId,
                "Frustración detectada en actividad",
                ct);

        return new CompleteActivityResponseResult(
            ResponseId: context.ResponseId,
            AdaptiveAdjustment: context.AdaptiveResult,
            NextUnlocked: context.NextActivityUnlocked
        );
    }
}
```

### Step de ejemplo: AdaptiveEngineStep

```csharp
// ApplicationBusiness/Steps/ActivityCompletion/AdaptiveEngineStep.cs

public class AdaptiveEngineStep : IPostCompletionStep
{
    private readonly IAdaptiveEngineService _engine;

    public int Order => 2;

    public AdaptiveEngineStep(IAdaptiveEngineService engine)
    {
        _engine = engine;
    }

    public async Task ExecuteAsync(
        ActivityCompletionContext context,
        CancellationToken ct)
    {
        if (context.AdaptiveConfig is null) return;
        if (!context.AdaptiveConfig.IsEnabled) return;

        context.AdaptiveResult = await _engine.EvaluateAndAdjustAsync(
            context.RoadmapActivity.Id,
            context.ResponseId,
            ct);
    }
}
```

### Registro en DI (reflexión automática)

```csharp
// InclusiON.Api/Extensions/ServiceCollectionExtensions.cs

public static IServiceCollection AddPipelineSteps(
    this IServiceCollection services)
{
    var stepType = typeof(IPostCompletionStep);
    var assembly = typeof(AdaptiveEngineStep).Assembly;

    var steps = assembly.GetTypes()
        .Where(t => stepType.IsAssignableFrom(t)
                  && t is { IsInterface: false, IsAbstract: false });

    foreach (var step in steps)
        services.AddScoped(stepType, step);

    return services;
}

// En Program.cs:
// builder.Services.AddPipelineSteps();
```

---

## 8. Interfaz del Servicio Adaptativo (CQRS)

### Interfaces

```csharp
// ApplicationBusiness/Interfaces/IAdaptiveEngineService.cs

public interface IAdaptiveEngineService
{
    Task<AdaptiveAdjustmentResult> EvaluateAndAdjustAsync(
        int personRoadmapActivityId,
        int activityResponseId,
        CancellationToken ct = default);
}

public record AdaptiveAdjustmentResult(
    bool WasAdjusted,
    string? AdjustmentType,
    string? Reason,
    AdaptiveState CurrentState);

public enum AdaptiveState
{ Stable, Progressing, Struggling, Frustrated }
```

**Proyecto de implementación:** `InclusiON.Infrastructure`. La clase `AdaptiveEngineService` implementa `IAdaptiveEngineService`, recibe `IUnitOfWork` por DI y accede a los repositorios de `ActivityResponse` y `PersonRoadmapActivity`. Se registra como Scoped.

---

## 9. Pseudocódigo del Algoritmo

```
function EvaluateAndAdjust(activityId, responseId):
    activity = LoadPersonRoadmapActivity(activityId, include: AdaptiveConfig)
    if activity.AdaptiveConfig is null: return NoAdjustment
    config = activity.AdaptiveConfig
    if NOT config.IsEnabled: return NoAdjustment

    response = LoadResponse(responseId)
    history = LoadRecentResponses(activityId, count: max(
        config.ConsecutiveSuccessToUpgrade,
        config.ConsecutiveFailuresToDowngrade) + 1)

    // ── FRUSTRACIÓN (prioridad máxima) ──
    if response.FrustrationLevel >= config.FrustrationThreshold
       OR DetectAbandonment(history):
        return ApplyFrustrationIntervention(config)

    // ── PROGRESANDO ──
    successStreak = CountConsecutiveSuccesses(history,
        config.SuccessThresholdPercent)
    if successStreak >= config.ConsecutiveSuccessToUpgrade:
        return ApplyEscalation(config, history)

    // ── DIFICULTAD ──
    failStreak = CountConsecutiveFailures(history,
        config.SuccessThresholdPercent)
    if failStreak >= config.ConsecutiveFailuresToDowngrade:
        return ApplyDeescalation(config, history)

    // ── ESTABLE ──
    return NoAdjustment(state: Stable)


function DetectAbandonment(history):
    recent3 = history.Take(3)
    return recent3.All(r =>
        r.TimeSpentSeconds < 10 AND r.SuccessPercentage == 0)
```

---

## 10. Visualización de Progreso desde AdaptiveAdjustmentLog

El `AdaptiveAdjustmentLog` no solo sirve como auditoría — es la fuente de datos para gráficas de evolución que el profesional ve en el perfil del estudiante.

### Endpoint

```
GET /api/persons/{personId}/roadmap-activities/{activityId}/adaptive-history
    ?from=2026-01-01&to=2026-03-08
```

### DTO de respuesta

```csharp
public record AdaptiveHistoryResponse(
    int PersonRoadmapActivityId,
    string ActivityName,
    string SkillAreaName,
    AdaptiveCurrentState CurrentState,
    List<AdaptiveHistoryEntry> Entries);

public record AdaptiveHistoryEntry(
    DateTime AdjustedAt,
    string AdjustmentType,
    string PreviousValue,    // JSON
    string NewValue,         // JSON
    string Reason,
    // Datos de la response que disparó el ajuste
    decimal? SuccessPercentage,
    int? TimeSpentSeconds,
    int? FrustrationLevel);

public record AdaptiveCurrentState(
    int DifficultyLevel,
    int? TimeLimitSeconds,
    int? MaxAttempts,
    bool ShowHints,
    AdaptiveState State);
```

### Tipos de gráficas posibles

**Timeline de Dificultad** — Gráfica de línea. Eje X = `AdjustedAt`, eje Y = `DifficultyLevel` extraído de `NewValue`. Cada punto tiene tooltip con `Reason`. Muestra cómo fue subiendo y bajando la dificultad. De un vistazo el profesional ve si el alumno progresó, si tuvo mesetas, o si rebotó entre niveles.

**Vista Multi-Parámetro** — Gráfica de líneas múltiples en paralelo. Una línea por parámetro: dificultad (1-5), hints (on/off como 0/1), tiempo límite (segundos), intentos (1-5). El profesional ve que en la semana 1 el alumno necesitaba hints y 60 segundos, y en la semana 3 ya opera sin hints en 40 segundos.

**Correlación con Rendimiento** — Gráfica combinada: línea de `DifficultyLevel` superpuesta con barras de `SuccessPercentage` de cada `ActivityResponse`. Muestra la relación causa-efecto: "bajé la dificultad → el porcentaje de éxito subió → el motor volvió a subir la dificultad".

**Mapa de Calor de Intervenciones** — Vista calendario tipo heatmap donde cada día tiene un color según el tipo de ajuste predominante: verde (escalamiento), naranja (desescalamiento), rojo (frustración), gris (estable). Permite al profesional detectar patrones temporales ("los lunes siempre hay frustración").

### Implementación frontend sugerida

Recharts (ya disponible en el stack Angular) con los componentes `LineChart`, `ComposedChart` y `Tooltip`. El endpoint devuelve los datos ya ordenados; el frontend solo los mapea a las series del chart. La selección de rango temporal (from/to) permite al profesional hacer zoom en períodos específicos.

---

## 11. Sinergia con Búsqueda Semántica

El Motor Adaptativo y la Búsqueda Semántica (ONNX + paraphrase-multilingual-MiniLM-L12-v2) forman un ciclo integrado donde cada componente potencia al otro:

| Fase | Búsqueda Semántica | Motor Adaptativo |
|------|-------------------|------------------|
| **Descubrimiento** | Encuentra actividades relevantes para el área de habilidad y perfil del estudiante. | — |
| **Asignación** | Sugiere la actividad con mayor afinidad semántica al objetivo pedagógico. | Configura los parámetros iniciales según el nivel actual del estudiante. |
| **Ejecución** | — | Monitorea el resultado y ajusta parámetros para la próxima ejecución. |
| **Recomendación** | Usa el historial de respuestas para recomendar actividades similares a las exitosas, o diferentes a las frustrantes. | Provee los datos de rendimiento que alimentan los filtros de la búsqueda. |

La búsqueda semántica responde "qué actividad asignar". El motor adaptativo responde "cómo configurarla para esta persona, ahora". Juntos cierran el ciclo: buscar, asignar, ejecutar, analizar, adaptar, buscar nuevamente.

---

## 12. Escenarios de Ejemplo

### Escenario A: Estudiante progresa bien

Juan tiene asignada "Seleccionar figura correcta" en Comunicación, con DifficultyLevel=1, ShowHints=true, TimeLimitSeconds=60.

1. Intento 1: SuccessPercentage=100%, TimeSpent=25s → Motor: **ESTABLE** (1 éxito, necesita 3)
2. Intento 2: SuccessPercentage=85%, TimeSpent=30s → Motor: **ESTABLE** (2 éxitos consecutivos)
3. Intento 3: SuccessPercentage=90%, TimeSpent=22s → Motor: **PROGRESANDO** → DifficultyLevel=2
4. Se logea: `AdaptiveAdjustmentLog(Type="DifficultyUp", Previous={"DifficultyLevel":1}, New={"DifficultyLevel":2}, Reason="3 aciertos ≥70%")`

### Escenario B: Estudiante muestra dificultad

María tiene "Emparejar palabra-imagen" con DifficultyLevel=3, ShowHints=false, MaxAttempts=2.

1. Intento 1: SuccessPercentage=40% → Motor: **ESTABLE** (1 fallo, necesita 2)
2. Intento 2: SuccessPercentage=30% → Motor: **DIFICULTAD** → ShowHints=true (prioridad 1)
3. Intento 3: SuccessPercentage=50% (con hints) → Motor: **ESTABLE** (se cortó la racha)
4. Intento 4: SuccessPercentage=35% → Motor: **ESTABLE** (1 fallo)
5. Intento 5: SuccessPercentage=25% → Motor: **DIFICULTAD** → MaxAttempts=3 (prioridad 2, hints ya activados)

### Escenario C: Frustración detectada

Pedro tiene "Ordenar secuencia" con DifficultyLevel=2. Se frustra rápidamente.

1. Intento 1: TimeSpent=5s, Success=0%, FrustrationLevel=2 → Motor: **ESTABLE**
2. Intento 2: TimeSpent=3s, Success=0%, FrustrationLevel=4 → Motor: **FRUSTRACIÓN** (≥ threshold 3)
3. Intervención: DifficultyLevel=1, ShowHints=true, TimeLimitSeconds=máx, MaxAttempts=5
4. Se notifica al profesional: "Pedro muestra frustración en Ordenar secuencia"

---

## 13. Plan de Implementación

| Paso | Tarea | Proyecto | Dependencias |
|------|-------|----------|--------------|
| 1 | Crear entidad AdaptiveEngineConfig + configuración Fluent API + migración (1:0..1 con PersonRoadmapActivity) | Entities + Data | Migración existente de roadmaps |
| 2 | Crear entidad AdaptiveAdjustmentLog + configuración + migración | Entities + Data | Paso 1 |
| 3 | Agregar navegación `AdaptiveConfig?` a PersonRoadmapActivity | Entities | Paso 1 |
| 4 | Definir IAdaptiveEngineService + DTOs de resultado | ApplicationBusiness + DTOs | Ninguna |
| 5 | Definir IPostCompletionStep + ActivityCompletionContext + 4 steps | ApplicationBusiness | Paso 4 |
| 6 | Implementar AdaptiveEngineService con reglas y máquina de estados | Infrastructure | Pasos 1-5 |
| 7 | Integrar pipeline en CompleteActivityResponseCommandHandler | ApplicationBusiness | Paso 6 + Handler existente |
| 8 | Endpoints GET adaptive-history + adaptive-log para consulta del profesional | Api + AppBusiness | Paso 2 |
| 9 | Tests unitarios del motor (escenarios A/B/C + casos borde) | Tests | Paso 6 |
| 10 | Componente Angular: panel de configuración de rangos para el profesional | InclusiON.Client | Paso 8 |
| 11 | Componente Angular: gráficas de evolución adaptativa en perfil del estudiante | InclusiON.Client | Paso 8 |

---

## 14. Evoluciones Futuras (Post-Tesis)

Las siguientes líneas de innovación están identificadas como extensiones naturales del motor adaptativo y pueden mencionarse en la defensa como trabajo futuro:

**Detección de Patrones de Interacción** — Análisis de señales pasivas (tiempos de respuesta, secuencias de error, abandonos, pausas prolongadas) para inferir estados como frustración, desatención o fatiga. Complementa el FrustrationLevel explícito con detección implícita.

**Auto-configuración de Accesibilidad desde Perfil CIF** — Dado el perfil funcional CIF del estudiante, el sistema deriva automáticamente la configuración de UI (tamaño de targets, pictogramas, TTS, contraste). Conecta el modelo de datos CIF ya diseñado con la capa de accesibilidad Angular existente.

**Dashboard Predictivo de Progreso** — Visualización de curvas de avance por área con detección de mesetas y sugerencias al profesional. Basado en estadística descriptiva sobre ventanas temporales de ActivityResponse.

**MCP (Model Context Protocol)** — El motor adaptativo y la búsqueda semántica podrían exponerse como tools via MCP Server, permitiendo que un LLM orqueste recomendaciones pedagógicas más sofisticadas. Diferido por complejidad y dependencia externa.
