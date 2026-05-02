# InclusiON — Arquitectura

Visión general de la arquitectura y decisiones técnicas del proyecto.

---

## Diagrama General

```
┌─────────────────────────────────────────────────────────┐
│                    FRONTEND                              │
│              Angular 20 + CoreUI                         │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐   │
│  │AAC Portal│ │Prof Portal│ │Fam Portal│ │  Admin   │   │
│  │/app/*    │ │/pro/*     │ │/family/* │ │/admin/*  │   │
│  └────┬─────┘ └────┬─────┘ └────┬─────┘ └────┬─────┘   │
│       └─────────────┴────────────┴────────────┘         │
│                  HTTP + JWT Bearer                       │
│               http://localhost:4200                      │
└──────────────────────────┬──────────────────────────────┘
                           │ CORS
┌──────────────────────────▼──────────────────────────────┐
│                   InclusiON.Api                          │
│              ASP.NET Core Web API                        │
│         Controllers → Command/Query → Handler           │
│              https://localhost:7xxx                      │
├─────────────────────────────────────────────────────────┤
│              InclusiON.Application                       │
│     CQRS Handlers (auto-registrados por reflexión)      │
│     ICommandHandler<TCmd, TResult>                      │
│     IQueryHandler<TQuery, TResult>                      │
│     Extensions: Paginación, Filtros Auditable           │
├─────────────────────────────────────────────────────────┤
│            InclusiON.Infrastructure                      │
│     JWT Service │ Password Hasher │ Repositories        │
│     Unit of Work │ Identity │ Connection Factory        │
├─────────────────────────────────────────────────────────┤
│               InclusiON.Data                             │
│     EF Core DbContext │ Configurations (36)             │
│     Migrations (13) │ DatabaseSeeder                    │
├─────────────────────────────────────────────────────────┤
│              InclusiON.Domain                            │
│     39 Entidades │ Base Classes │ Enums                 │
├──────────────┬──────────────────────────────────────────┤
│ InclusiON.DTOs │ InclusiON.Shared │ InclusiON.SemanticSearch │
│ Req/Res DTOs   │ Constantes/Resx  │ ONNX Embeddings         │
└──────────────┴──────────────┴───────────────────────────┘
                           │
                    ┌──────▼──────┐
                    │ PostgreSQL  │
                    └─────────────┘
```

---

## Patrón CQRS

Separamos las operaciones de lectura (Queries) de las de escritura (Commands):

```
Controller recibe HTTP request
  → Crea Command o Query record
  → Resuelve el Handler por reflexión (DI automático)
  → Handler ejecuta lógica de negocio
  → Retorna Result<TResponse>
```

### Interfaces base

```csharp
public interface ICommandHandler<TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken ct);
}

public interface IQueryHandler<TQuery, TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken ct);
}
```

### Auto-registro

En `Application/DependencyInjection.cs` se escanean los assemblies y se registran todos los handlers automáticamente. No hay que agregar nada al contenedor DI cuando se crea un handler nuevo.

---

## Patrón Repository + Unit of Work

```
Controller → Handler → IRepository (interface en Application)
                         ↓
               Repository (impl en Infrastructure)
                         ↓
                  DbContext (en Data)
```

- Las interfaces viven en `Application/Interfaces/Repositories/`
- Las implementaciones en `Infrastructure/Data/Repositories/`
- `IUnitOfWork` coordina el commit de la transacción

---

## Autenticación y Autorización

### Flujo JWT

```
1. POST /api/auth/login → AuthController → LoginCommandHandler
2. Handler verifica credenciales → genera AccessToken + RefreshToken
3. Frontend almacena en localStorage
4. auth.interceptor.ts agrega "Authorization: Bearer {token}" a cada request
5. Backend valida el token con middleware JWT
6. Cuando expira: POST /api/auth/refresh → nuevo par de tokens
```

### Roles del sistema

| Rol | Layout | Descripción |
|-----|--------|-------------|
| `Admin` | `/admin` | Administración completa |
| `Professional` | `/pro` | Profesionales que gestionan estudiantes |
| `PersonWithDisability` | `/app` (AAC) | Estudiantes que ejecutan actividades |
| `FamilyRepresentative` | `/family` | Familiares que ven progreso |

### Métodos de login

| Método | Ruta | Quién lo usa |
|--------|------|-------------|
| Estándar (email/pass) | `/admin-login` | Admin, Professional |
| Visual estándar | `/login/visual-standard` | PersonWithDisability |
| PIN | `/login/pin` | PersonWithDisability |
| Asistido | `/login/assisted` | PersonWithDisability (con supervisor) |
| Familiar | `/login/family` | FamilyRepresentative |

---

## Accesibilidad (WCAG 2.1 AA/AAA)

El sistema implementa accesibilidad como feature transversal, no como addon:

- **7 perfiles** de accesibilidad que cambian variables CSS
- **2 modos** de color (light/dark) independientes del perfil
- **Atributos HTML** (`data-color-mode`, `data-profile`) controlan todo
- **Focus visible**, skip links, reduced motion, forced colors
- **TTS** con Web Speech API
- **Reading mode** que oculta sidebar/header

Ver detalle completo en `CLAUDE_FRONTEND.md`.

---

## Features Complejos

### Búsqueda Semántica

```
Texto de búsqueda → ONNX (paraphrase-multilingual-MiniLM-L12-v2) → Embedding 384D
                                                    ↓
                                    pgvector cosine similarity (<=>)
                                                    ↓
                                         Top N actividades similares
```

- Library en `InclusiON.SemanticSearch/`
- Runtime: `Microsoft.ML.OnnxRuntime` + `Microsoft.ML.Tokenizers` (SentencePiece BPE)
- Modelo: `paraphrase-multilingual-MiniLM-L12-v2` — multilingüe, 384 dims
- Entidades: `ActivityEmbedding`, `ActivityResult`
- Estado: modelo + tokenizador + DI listos; falta handler CQRS de búsqueda y embedding al crear actividad
- Doc: `Features/integracion-semantic-search.md`

### Motor de Dificultad Adaptativa — MDA

```
Estudiante completa actividad
  → ActivityResponse guardada
  → MDA evalúa historial reciente
  → Máquina de estados: ESTABLE | PROGRESANDO | DIFICULTAD | FRUSTRACIÓN
  → Ajusta parámetros dentro de rangos del profesional
  → Log de auditoría en AdaptiveAdjustmentLog
  → Si frustración: alerta al profesional via SignalR
```

- Entidades: `AdaptiveEngineConfig`, `AdaptiveAdjustmentLog`
- Estado: entidades y migraciones existen, falta implementación del servicio
- Doc: `Documentacion/Features/MDA_Especificacion_Tecnica.md`

### Pipeline de Completar Respuesta (diseño futuro)

```
POST /complete
  → CompleteActivityResponseCommandHandler
    → Step 1: PersistResponseStep
    → Step 2: AdaptiveEngineStep (MDA)
    → Step 3: UnlockNextActivityStep
    → Step 4: FrustrationAlertStep
  → UoW.CommitAsync() (1 transacción)
  → Post-commit: SignalR si hay alerta
```

---

## Dependencias Clave entre Proyectos

```
Api → Application, Infrastructure, Data, DTOs, Shared
Application → Domain, DTOs, Shared (interfaces solamente)
Infrastructure → Application, Domain, Data, DTOs (implementaciones)
Data → Domain (EF Core configs y DbContext)
DTOs → (sin dependencias)
Domain → (sin dependencias)
Shared → (sin dependencias)
SemanticSearch → Application (implementa IEmbeddingService)
```

---

## Base de Datos

- **PostgreSQL** con Npgsql + EF Core 10
- **13+ migraciones** aplicadas (Enero-Abril 2026)
- **39 entidades** mapeadas con Fluent API
- **DatabaseSeeder** con datos iniciales (roles, catálogos)
- Auto-migración en `Program.cs` al iniciar la API
- Usar `EF.Functions.ILike()` para búsquedas case-insensitive (no `Like()`)

### Entidades principales y sus relaciones

```
User (1) ──── (0..1) PersonWithDisability
User (1) ──── (0..1) Professional
User (1) ──── (0..1) FamilyRepresentative
User (1) ──── (N) RefreshToken
User (1) ──── (N) TrustedDevice

Professional (N) ── ProfessionalInstitution ── (N) EducationalInstitution
Professional (N) ── ProfessionalPerson ── (N) PersonWithDisability
Professional (1) ── (N) ProfessionalStatusHistory

PersonWithDisability (1) ── (0..1) PersonRoadmap
PersonWithDisability (N) ── PersonRepresentative ── (N) FamilyRepresentative
PersonWithDisability (1) ── (N) PersonSkillProfile ── (N) SkillArea
PersonRepresentative (1) ── (N) PersonRepresentativeHistory
FamilyRepresentative (1) ── (N) FamilyStatusHistory

PersonRoadmap (1) ── (N) PersonRoadmapArea
PersonRoadmapArea (1) ── (N) PersonRoadmapActivity
PersonRoadmapActivity (1) ── (0..1) AdaptiveEngineConfig
PersonRoadmapActivity (1) ── (N) AdaptiveAdjustmentLog
PersonRoadmapActivity (1) ── (N) ActivityResult

Activity (1) ── (0..1) ActivityContent
Activity (1) ── (0..1) ActivityEmbedding
Activity (1) ── (N) PersonRoadmapActivity
Activity (1) ── (N) ActivityAssignment

ActivityAssignment (1) ── (N) ActivityResponse
ActivityResponse (1) ── (N) AdaptiveAdjustmentLog
```
