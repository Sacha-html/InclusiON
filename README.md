# InclusiON Backend

.NET 8 Web API con Entity Framework Core para el sistema de gestión inclusiva.

## Estructura del Proyecto

```
InclusiON.Server/
├── InclusiON.Api/              # API REST + Controllers
├── InclusiON.Application/     # Use Cases, Queries, Commands (MediatR)
├── InclusiON.Domain/         # Entidades y modelos de dominio
├── InclusiON.Infrastructure/  # Repositorios, servicios externos
├── InclusiON.Data/          # DbContext, Configurations, Migrations
├── InclusiON.DTOs/            # Data Transfer Objects
├── InclusiON.Shared/         # Recursos compartilhados
└── InclusiON.SemanticSearch/ # Búsqueda semántica (opcional)
```

## Requisitos

- .NET 8 SDK
- SQL Server (local o Docker)
- Node.js 18+ (para crear migraciones con EF Core)

## Configuración

### Variables de Entorno

```bash
# Connection String
ConnectionStrings__DefaultConnection=Server=localhost;Database=InclusiON;Trusted_Connection=True;TrustServerCertificate=True

# JWT
Jwt__Key=your-256-bit-secret-key-here
Jwt__Issuer=InclusiON
Jwt__Audience=InclusiON

# Email (SMTP)
Smtp__Host=localhost
Smtp__Port=587
Smtp__User=
Smtp__Password=

# Azure AI (opcional)
AzureAI__Endpoint=
AzureAI__Key=
```

### Base de Datos

```bash
# Crear migración inicial
dotnet ef migrations add InitialCreate --project InclusiON.Data --startup-project InclusiON.Api

# Aplicar migraciones
dotnet ef database update --project InclusiON.Data --startup-project InclusiON.Api
```

## Comandos

### Desarrollo

```bash
cd InclusiON.Api
dotnet run
# API disponible en http://localhost:5000
# Swagger en http://localhost:5000/swagger
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
# Con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Un proyecto específico
dotnet test InclusiON.Application.Tests
```

### Limpieza

```bash
# Limpiar bin/obj
dotnet clean

# Restaurar paquetes
dotnet restore
```

## Patrones de Código

### CQRS con MediatR

```csharp
// Query
public record GetPersonsQuery : IRequest<ApiResponse<List<PersonResponse>>>;

public class GetPersonsQueryHandler : IRequestHandler<GetPersonsQuery, ApiResponse<List<PersonResponse>>>
{
    public async Task<ApiResponse<List<PersonResponse>>> Handle(GetPersonsQuery request, CancellationToken cancellationToken)
    {
        // Implementación
    }
}
```

```csharp
// Command
public record CreatePersonCommand : IRequest<ApiResponse<PersonResponse>>;

public class CreatePersonCommandHandler : IRequestHandler<CreatePersonCommand, ApiResponse<PersonResponse>>
{
    public async Task<ApiResponse<PersonResponse>> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
    {
        // Implementación
    }
}
```

### Repositorios

```csharp
public interface IPersonsRepository
{
    Task<Person?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IQueryable<Person>> GetAllAsync();
    Task<Person> CreateAsync(Person entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Person entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
```

### Optimización de Queries

```csharp
// ✅ Con AsNoTracking al final
var persons = await _context.Persons
    .Include(p => p.User)
    .Include(p => p.LoginMethod)
    .AsNoTracking()
    .ToListAsync(cancellationToken);

// ✅ Contains sin ToLower (SQL Server es case-insensitive por defecto)
var searchPattern = $"%{query}%";
var results = await _context.Persons
    .Where(p => EF.Functions.Like(p.Name, searchPattern))
    .ToListAsync(cancellationToken);

// ✅ Subqueries como IQueryable (ejecutar al final)
var activeProfessionalIds = _context.Professionals
    .Where(p => p.IsActive)
    .Select(p => p.UserId);

var persons = await _context.Persons
    .Where(p => activeProfessionalIds.Contains(p.UserId))
    .ToListAsync(cancellationToken);
```

### Inyección de Dependencias

```csharp
// Registrar repositorio
builder.Services.AddScoped<IPersonsRepository, PersonsRepository>();

// Registrar servicio
builder.Services.AddScoped<IAuthService, AuthService>();

// Registrar handler
builder.Services.AddTransient<IRequestHandler<GetPersonsQuery, ApiResponse<List<PersonResponse>>>, GetPersonsQueryHandler>();
```

## Seguridad

### Permisos

Los permisos se gestionan en `Application.Constants.Permissions.cs`:

```csharp
public static class Permissions
{
    public static class Persons
    {
        public const string View = "persons:view";
        public const string Create = "persons:create";
        public const string Edit = "persons:edit";
        public const string Delete = "persons:delete";
    }
    // ... más permisos
}
```

### claim-based Authorization

```csharp
[Authorize(Policy = "persons:view")]
public async Task<ActionResult<ApiResponse<List<PersonResponse>>>> GetPersons(...)
```

## Testing

### Unit Tests

```bash
# Ejecutar todos los tests
dotnet test

# Con verbose
dotnet test --verbosity detailed

# Coverage
dotnet test --collect:"XPlat Code Coverage" --settings:coverage.config
```

## Errores Comunes

### "The database is locked"

El proceso del API está corriendo y tiene bloqueado el DLL. Matar el proceso:

```bash
# Windows
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

### "There is already an object..."

Eliminar la base de datos y recrear:

```bash
dotnet ef database drop --project InclusiON.Data --startup-project InclusiON.Api
dotnet ef database update --project InclusiON.Data --startup-project InclusiON.Api
```

---

## Endpoints Principales

| Endpoint | Método | Descripción |
|----------|--------|-------------|
| `/api/auth/identify` | POST | Identificar usuario antes del login |
| `/api/auth/login/visual-standard` | POST | Login con contraseña |
| `/api/auth/login/visual-pin` | POST | Login con PIN |
| `/api/auth/login/visual-assisted` | POST | Login asistido |
| `/api/persons` | GET | Listar personas |
| `/api/persons` | POST | Crear persona |
| `/api/persons/{id}` | GET | Obtener persona |
| `/api/persons/{id}` | PUT | Actualizar persona |
| `/api/persons/{id}/professionals` | GET | Profesionales asignados |
| `/api/professionals` | GET | Listar profesionales |
| `/api/family` | GET | Listar representantes familiares |
| `/api/reports` | GET | Generar reportes |

---

## Datos de Prueba (Seed)

El proyecto incluye un seeder que crea usuarios de prueba:

| Email | Tipo | Login Method | Password/PIN |
|-------|------|-------------|--------------|
| maria@test.com | Persona | PIN | PIN: 1234 |
| juan@test.com | Persona | Estándar | Password: Juan123! |
| ana@test.com | Persona | Asistido | (supervisado) |
| carlos@test.com | Persona | PIN | PIN: 5678 |
| profesional@test.com | Profesional | Estándar | Password: Prof123! |
| docente@test.com | Profesional | Estándar | Password: Doc123! |
| admin@inclusion.com | Admin | Estándar | Password: Admin123! |