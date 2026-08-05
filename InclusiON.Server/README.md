# InclusiON Backend

.NET 10 Web API con Entity Framework Core + PostgreSQL para el sistema de gestión inclusiva.

## Modelo de Búsqueda Semántica

Los archivos del modelo **no están en el repositorio** (>100 MB). Descargarlos manualmente:

| Archivo | Fuente |
|---------|--------|
| `InclusiON.SemanticSearch/Model/model.onnx` | [HuggingFace — paraphrase-multilingual-MiniLM-L12-v2 (onnx)](https://huggingface.co/sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2/resolve/main/onnx/model.onnx) |
| `InclusiON.SemanticSearch/Model/sentencepiece.bpe.model` | [HuggingFace — sentencepiece.bpe.model](https://huggingface.co/sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2/resolve/main/sentencepiece.bpe.model) |

Si los archivos no están presentes, la app arranca igual con `NullEmbeddingService` (búsqueda semántica deshabilitada).

---

## Estructura del Proyecto

```
InclusiON.Server/
├── InclusiON.Api/                    # API REST + Controllers + Filters
├── InclusiON.Application/            # Use Cases, Queries, Commands (CQRS custom)
├── InclusiON.Domain/                 # Entidades y modelos de dominio
├── InclusiON.Infrastructure/         # Repositorios, servicios externos, autorización
├── InclusiON.Infrastructure.Telemetry/ # OpenTelemetry, métricas
├── InclusiON.Data/                   # DbContext, Configurations, Migrations
├── InclusiON.DTOs/                   # Data Transfer Objects
├── InclusiON.Shared/                 # Recursos compartidos
├── InclusiON.SemanticSearch/         # Búsqueda semántica (opcional)
└── InclusiON.Tests/
    ├── Unit/                         # Tests unitarios (xUnit + NSubstitute + FluentAssertions)
    └── Integration/                  # Tests de integración (WebApplicationFactory + InMemory DB)
```

## Requisitos

- .NET 10 SDK
- PostgreSQL 14+ (local o Docker)

## Configuración

### Variables de Entorno

```bash
# Connection String (Npgsql / PostgreSQL)
ConnectionStrings__PostgreSqlConn=Host=localhost;Port=5432;Database=inclusion_dev;Username=postgres;Password=Tu_Password_Segura123!

# JWT
Jwt__Key=your-256-bit-secret-key-here
Jwt__Issuer=InclusiON
Jwt__Audience=InclusiON

# Email (SMTP)
Smtp__Host=localhost
Smtp__Port=587
Smtp__User=
Smtp__Password=

# Azure AI (opcional, para búsqueda semántica)
AzureAI__Endpoint=
AzureAI__Key=
```

### Base de Datos

#### Setup inicial — usuarios y bases de datos

El script `InclusiON.Data/Scripts/db-users-setup.sql` crea los usuarios de PostgreSQL y las bases de datos por ambiente. **Ejecutar una sola vez como superusuario** (o al recrear la DB desde cero).

Con Docker (configuración local):

```powershell
# Copiar el script al contenedor y ejecutarlo
docker cp InclusiON.Data/Scripts/db-users-setup.sql postgres:/tmp/setup.sql
docker exec postgres psql -U postgres -f /tmp/setup.sql
```

Con psql local:

```bash
psql -U postgres -f InclusiON.Data/Scripts/db-users-setup.sql
```

> **Importante:** El script usa `DROP DATABASE IF EXISTS` para `inclusion_dev` e `inclusion_test` — borra y recrea esas bases. No toca staging ni producción.
>
> La extensión `vector` (pgvector) requiere superusuario y **no está incluida en el script**. Ejecutar manualmente después del setup:
> ```sql
> -- conectado a inclusion_dev
> CREATE EXTENSION IF NOT EXISTS vector;
> ```

#### Recrear bases desde cero

```powershell
# 1. Bajar el contenedor (si está corriendo)
docker stop postgres && docker rm postgres

# 2. Levantar docker compose de infra (pgvector)
docker compose up -d

# 3. Setup usuarios y bases
docker cp InclusiON.Data/Scripts/db-users-setup.sql postgres:/tmp/setup.sql
docker exec postgres psql -U postgres -f /tmp/setup.sql

# 4. Extensión pgvector (una vez por base, requiere superusuario)
docker exec postgres psql -U postgres -d inclusion_dev  -c "CREATE EXTENSION IF NOT EXISTS vector;"
docker exec postgres psql -U postgres -d inclusion_test -c "CREATE EXTENSION IF NOT EXISTS vector;"

# 5. Migraciones + seed (desde InclusiON.Api)
dotnet ef database update
# o simplemente levantar la app — MigrateAsync() corre al iniciar
```

#### Migraciones EF Core

```bash
# Crear migración
dotnet ef migrations add NombreMigracion --project InclusiON.Data --startup-project InclusiON.Api

# Aplicar migraciones
dotnet ef database update --project InclusiON.Data --startup-project InclusiON.Api
```

## Comandos

### Desarrollo

```bash
cd InclusiON.Api

# Normal (rate limiter activo)
dotnet run

# Con rate limiter deshabilitado — útil para tests E2E o desarrollo intensivo
dotnet run -- --RateLimiter:Disabled=true

# API disponible en http://localhost:5000 / https://localhost:5001
# Documentación Scalar en http://localhost:5000/scalar
```

> **Rate Limiter**: habilitado por defecto en todos los entornos. Para deshabilitarlo sin tocar archivos de config, pasarlo como argumento CLI (`--RateLimiter:Disabled=true`). El flag CLI tiene mayor prioridad que `appsettings.json`.

### Build

```bash
# Production
dotnet publish -c Release -o ./publish

# Development
dotnet build
```

### Testing

```bash
# Todos los tests
dotnet test

# Solo unitarios
dotnet test InclusiON.Tests/Unit/InclusiON.Tests.Unit.csproj

# Solo integración
dotnet test InclusiON.Tests/Integration/InclusiON.Tests.Integration.csproj
```

Los tests de integración usan `WebApplicationFactory<Program>` con EF Core InMemory — no requieren PostgreSQL ni conexión externa.

### Cobertura de código

El script `coverage.ps1` ejecuta los tests con cobertura, genera un reporte HTML y lo abre en el browser:

```powershell
.\coverage.ps1
```

**Prerequisito** — instalar `reportgenerator` como herramienta global (una sola vez):

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

El reporte se genera en `coverage-report/index.html`. Ambas carpetas (`coverage/` y `coverage-report/`) están en `.gitignore`.

### Limpieza

```bash
dotnet clean
dotnet restore
```

## Patrones de Código

### CQRS (IQueryHandler / ICommandHandler)

El proyecto usa un patrón CQRS custom — **no usa MediatR**. Los handlers se registran automáticamente por reflexión en `AddApplicationServices()`.

```csharp
// Query
public record GetPersonsQuery(int Page, int PageSize, string? Search)
    : PagedRequest;

public class GetPersonsQueryHandler
    : IQueryHandler<GetPersonsQuery, ApiResponse<PagedResponse<PersonListItemResponse>>>
{
    public async Task<ApiResponse<PagedResponse<PersonListItemResponse>>> HandleAsync(
        GetPersonsQuery query, CancellationToken cancellationToken)
    {
        // Implementación
    }
}
```

```csharp
// Command
public record CreatePersonCommand(CreatePersonRequest Request)
    : ICommand<ApiResponse<PersonResponse>>;

public class CreatePersonCommandHandler
    : ICommandHandler<CreatePersonCommand, ApiResponse<PersonResponse>>
{
    public async Task<ApiResponse<PersonResponse>> HandleAsync(
        CreatePersonCommand command, CancellationToken cancellationToken)
    {
        // Implementación
    }
}
```

### Registro de Dependencias

Los handlers se registran automáticamente — no hace falta registrar manualmente:

```csharp
// Program.cs
builder.Services.AddApplicationServices(); // registra todos los handlers por reflexión

// Para repositorios, sí se registran explícitamente:
builder.Services.AddScoped<IPersonsRepository, PersonsRepository>();
```

### Repositorios

```csharp
public interface IPersonsRepository
{
    Task<PersonWithDisability?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PersonWithDisability> CreateAsync(PersonWithDisability person, CancellationToken ct = default);
    Task UpdateAsync(PersonWithDisability person, CancellationToken ct = default);
    Task<PagedResponse<PersonWithDisability>> GetPagedAsync(int page, int pageSize, ...);
    Task<bool> ExistsDocumentAsync(string documentNumber, Guid? excludePersonId = null, CancellationToken ct = default);
}
```

### Optimización de Queries (PostgreSQL)

```csharp
// ✅ ILike para búsqueda case-insensitive en PostgreSQL
var searchPattern = $"%{query}%";
var results = await _context.Persons
    .Where(p => EF.Functions.ILike(p.FirstName, searchPattern) ||
                EF.Functions.ILike(p.LastName, searchPattern))
    .AsNoTracking()
    .ToListAsync(cancellationToken);

// ✅ Subqueries como IQueryable (se ejecutan en una sola query SQL)
var personIdsInInstitution = _context.ProfessionalInstitutions
    .Where(pi => pi.InstitutionId == id && pi.IsActive)
    .Select(pi => pi.ProfessionalId);

var professionals = await _context.Professionals
    .Where(p => personIdsInInstitution.Contains(p.Id))
    .ToListAsync(cancellationToken);
```

## Seguridad

La autorización opera en tres capas apiladas:

```
[1] Autenticación JWT          → ¿quién sos?
[2] Política de rol/permiso    → ¿podés llegar a este endpoint?
[3] Autorización por recurso   → ¿tenés vínculo con este dato específico?
```

### Capa 1 y 2 — Políticas de rol/permiso

```csharp
[Authorize(Policy = "persons:read")]
public async Task<ActionResult<...>> GetPersonById(Guid personId, ...)
```

### Capa 3 — Row-Level Authorization (HU-IN-172)

Implementada en `IResourceAuthorizationService` (Application) / `ResourceAuthorizationService` (Infrastructure).

**Para endpoints con `personId` en la ruta** se usan atributos-filtro declarativos:

```csharp
[HttpGet("{personId:guid}")]
[Authorize(Policy = "persons:read")]
[PersonAccess(AccessMode.Read)]          // ← row-level check
public async Task<ActionResult<...>> GetPersonById(Guid personId, ...)
```

Filtros disponibles en `InclusiON.Api/Filters/`:

| Atributo | Parámetro de ruta | Recurso |
|---|---|---|
| `[PersonAccess(mode)]` | `{personId:guid}` | `PersonWithDisability` |
| `[DiagnosisAccess(mode)]` | `{id:int}` | `Diagnosis` |
| `[ReportAccess(mode)]` | `{reportId:int}` | `Report` |

**Reglas por rol:**

| Rol | Acceso permitido |
|---|---|
| GlobalAdmin | Todos los recursos (bypass, pero auditado) |
| Professional | Solo personas con `ProfessionalPerson.IsActive = true` |
| FamilyRepresentative | Solo personas con `PersonRepresentative.IsActive = true` |
| Admin institucional | Solo personas de sus instituciones asignadas |
| PersonWithDisability | Solo sus propios datos |

**Política de respuesta (CA-17):**
- `FamilyRepresentative` / `PersonWithDisability` → **404** (oculta existencia del recurso)
- `Professional` / `Admin` → **403** (feedback claro para usuarios internos)

**Auditoría:** cada acceso (permitido o denegado) queda registrado en la tabla `AccessAudit` con `UserId`, `Role`, `Result`, `ActionType`, `IpAddress` y `CorrelationId`.

**Listados:** el scoping se aplica en el repositorio vía `GetAccessiblePersonIdsAsync()` — no hay post-filtrado en memoria.

```csharp
// PersonsController.GetPersons — GlobalAdmin no aplica filtro
var accessibleIds = _httpContextService.IsGlobalAdmin()
    ? null
    : await _resourceAuthz.GetAccessiblePersonIdsAsync(cancellationToken);
```

**Reglas de negocio** (distinto de acceso): viven en los command handlers. Ej: solo el profesional autor puede editar un reporte en estado `Draft` — validado en `UpdateReportCommandHandler`, no en el filtro.

## Mensajería

### ¿Quién puede enviar mensajes a quién?

Solo se permiten mensajes entre **Profesional ↔ Familiar**, siempre que compartan al menos una persona con discapacidad con asignaciones activas en ambos lados.

| Combinación | ¿Permitido? | Motivo |
|---|---|---|
| Profesional → Familiar (vínculo activo) | ✅ | Regla principal |
| Familiar → Profesional (vínculo activo) | ✅ | Bidireccional |
| Profesional → Familiar (sin vínculo) | ❌ 403 | No comparten persona activa |
| Profesional → Profesional | ❌ 403 | Mismo tipo de usuario |
| Familiar → Familiar | ❌ 403 | Mismo tipo de usuario |
| Cualquiera → Persona con discapacidad | ❌ 403 | Canal no habilitado para este rol |
| Persona con discapacidad → Cualquiera | ❌ 403 | Canal no habilitado para este rol |
| Usuario → sí mismo | ❌ 400 | Auto-mensaje no permitido |

El vínculo se valida vía `HaveSharedPersonAsync`: join entre `ProfessionalPersons`, `Professionals`, `PersonRepresentatives` y `FamilyRepresentatives`, filtrando `IsActive = true` en ambos lados.

### Comportamiento adicional

- `GET /messages/{id}` auto-marca como leído si el usuario autenticado es el receptor.
- Respuestas (`/reply`) heredan `Subject` y `RelatedPersonId` del mensaje padre. No re-validan la relación (el vínculo original fue válido al enviar).
- Soft delete: el mensaje se desactiva (`IsActive = false`), no se borra físicamente.
- Bandeja de entrada y enviados listan solo mensajes raíz (`ParentMessageId == null`); las respuestas se cargan anidadas en `GET /messages/{id}`.

## Errores Comunes

### API no refleja cambios tras compilar

El proceso anterior sigue corriendo con el binario viejo:

```bash
# Windows
netstat -ano | findstr :5000
taskkill /PID <PID> /F
dotnet run
```

### Migración falla por schema existente

```bash
dotnet ef database drop --project InclusiON.Data --startup-project InclusiON.Api
dotnet ef database update --project InclusiON.Data --startup-project InclusiON.Api
```

---

## Endpoints Principales

| Endpoint | Método | Descripción |
|----------|--------|-------------|
| `/auth/identify` | POST | Identificar usuario (retorna candidatos si hay homónimos) |
| `/auth/login/visual-standard` | POST | Login con contraseña |
| `/auth/login/visual-pin` | POST | Login con PIN |
| `/auth/login/visual-assisted` | POST | Login asistido por supervisor |
| `/auth/refresh` | POST | Renovar token JWT |
| `/auth/users/{userId}/login-method` | PUT | Cambiar método de login |
| `/persons` | GET | Listar personas (paginado, filtros, sort) |
| `/persons` | POST | Crear persona |
| `/persons/{id}` | GET | Obtener persona |
| `/persons/{id}` | PUT | Actualizar persona |
| `/persons/{id}/professionals` | GET | Profesionales asignados |
| `/persons/{id}/supervisor-candidates` | GET | Candidatos a supervisor de login |
| `/professionals` | GET | Listar profesionales (paginado, filtros, sort) |
| `/professionals/pending` | GET | Solicitudes pendientes de validación |
| `/professionals/{id}` | GET/PUT | Obtener / actualizar profesional |
| `/professionals/{id}/validate` | POST | Aprobar o rechazar profesional |
| `/family` | GET | Listar representantes familiares (paginado, filtros, sort) |
| `/family/{id}` | GET/PUT | Obtener / actualizar familiar |
| `/reports` | GET | Listar informes (paginado, filtros, sort) |
| `/reports/family` | GET | Informes accesibles por familiar |
| `/institutions` | GET/POST | Listar / crear instituciones |
| `/institutions/{id}` | GET/PUT | Obtener / actualizar institución |
| `/admin/users` | GET | Listar usuarios del sistema |
| `/catalogs/login-methods` | GET | Métodos de login activos |
| `/catalogs/avatar-colors` | GET | Colores de avatar disponibles |
| `/messages/inbox` | GET | Bandeja de entrada (paginado) |
| `/messages/sent` | GET | Mensajes enviados (paginado) |
| `/messages/unread-count` | GET | Cantidad de mensajes no leídos |
| `/messages/{id}` | GET | Obtener mensaje (marca leído si receptor) |
| `/messages` | POST | Enviar mensaje |
| `/messages/{id}/reply` | POST | Responder mensaje |
| `/messages/{id}/read` | PATCH | Marcar como leído |
| `/messages/{id}` | DELETE | Eliminar mensaje (soft delete) |
| `/activities/{id}/similar` | GET | Actividades similares (búsqueda semántica) |
| `/activities/{id}/compatible-persons` | GET | Personas compatibles para una actividad |
| `/persons/{id}/recommended-activities` | GET | Actividades recomendadas para una persona |
| `/my/activity-assignments` | GET | Asignaciones activas del alumno autenticado |
| `/persons/{id}/activity-assignments` | GET | Asignaciones activas de una persona |
| `/activity-assignments` | POST | Crear asignación de actividad |
| `/activity-assignments/{id}` | GET | Obtener asignación con ContentJson y TemplateTypeCode |
| `/activity-assignments/{id}/cancel` | PATCH | Cancelar asignación pendiente |
| `/activity-assignments/{id}/responses/start` | POST | Iniciar intento de actividad (el alumno empieza a jugar) |
| `/activity-assignments/{id}/responses/{responseId}/complete` | POST | Completar intento con resultados (desbloquea siguiente nivel) |
| `/calendar` | GET | Listar eventos del calendario (filtrado por rol) |
| `/calendar` | POST | Crear o actualizar evento de calendario |
| `/calendar/{id}` | DELETE | Eliminar evento de calendario |

---

## Datos de Prueba (Seed)

| Email | Tipo | Método login | Credencial |
|-------|------|-------------|------------|
| maria@test.com | Persona | PIN | 1234 |
| juan@test.com | Persona | Estándar | Juan123! |
| ana@test.com | Persona | Asistido | (supervisado) |
| carlos@test.com | Persona | PIN | 5678 |
| profesional@test.com | Profesional | Estándar | Prof123! |
| docente@test.com | Profesional | Estándar | Doc123! |
| admin@inclusion.com | Admin | Estándar | Admin123! |

> **Nota**: Al iniciar el backend, `DatabaseSeeder` corre `PatchStandardActivitiesContentAsync` que verifica y corrige el `ContentJson` de las 10 actividades estándar del roadmap si está vacío (`'{}'`). Esto garantiza que los players del cliente siempre tengan datos del juego.
