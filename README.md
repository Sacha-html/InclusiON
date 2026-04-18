# InclusiON Backend

.NET 10 Web API con Entity Framework Core + PostgreSQL para el sistema de gestión inclusiva.

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
dotnet run
# API disponible en http://localhost:5000
# Documentación Scalar en http://localhost:5000/scalar
```

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

# Con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Solo unitarios
dotnet test InclusiON.Tests/Unit/InclusiON.Tests.Unit.csproj

# Solo integración
dotnet test InclusiON.Tests/Integration/InclusiON.Tests.Integration.csproj
```

Los tests de integración usan `WebApplicationFactory<Program>` con EF Core InMemory — no requieren PostgreSQL ni conexión externa.

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
