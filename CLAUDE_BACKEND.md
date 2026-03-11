# CLAUDE.md — Backend (.NET 8)

Instrucciones para agentes AI y desarrolladores trabajando en `InclusiON.Server/`.

---

## Proyecto y Stack

- **.NET 8** con Clean Architecture
- **EF Core** (SQL Server) para persistencia
- **Dapper** para consultas de solo lectura optimizadas
- **JWT Bearer** para autenticación
- **CQRS** con auto-registro por reflexión

---

## Comandos

```bash
cd InclusiON.Server
dotnet restore
dotnet build
dotnet run --project InclusiON.Api    # Migra la DB automáticamente al iniciar
dotnet test
```

### Migraciones EF Core (Package Manager Console)

```powershell
Add-Migration <NombreMigración> -Project InclusiON.Data -StartupProject InclusiON.Api
```

---

## Estructura de Proyectos

```
InclusiON.Api/              ← Entry point, controllers, Program.cs, middleware
InclusiON.Application/      ← CQRS handlers, interfaces, extensions
InclusiON.Infrastructure/   ← JWT, repos, servicios, Unit of Work
InclusiON.Data/             ← DbContext, configurations, migraciones, seeders
InclusiON.Domain/           ← Entidades del dominio y base classes
InclusiON.DTOs/             ← Request/Response DTOs, PagedRequest/Response
InclusiON.Shared/           ← Constantes, mensajes (resx)
InclusiON.SemanticSearch/   ← Library ONNX para embeddings (pendiente integración)
```

---

## Convenciones Clave

### Crear una nueva feature (flujo completo)

1. **Controller** en `InclusiON.Api/Controllers/` — solo recibe request y delega al handler
2. **Command/Query records** en `InclusiON.Application/UseCases/{Feature}/Commands/` o `Queries/`
3. **Handler** en `InclusiON.Application/UseCases/{Feature}/Handlers/` — implementa `ICommandHandler<TCommand, TResult>` o `IQueryHandler<TQuery, TResult>`
4. **Interface del repo** en `InclusiON.Application/Interfaces/Repositories/`
5. **Implementación del repo** en `InclusiON.Infrastructure/Data/Repositories/`
6. **DTOs** en `InclusiON.DTOs/Requests/{Feature}/` y `Responses/{Feature}/`

**IMPORTANTE:** Los handlers se auto-registran por reflexión. NO necesitan registro manual en DI.

### Crear una nueva entidad

1. Clase en `InclusiON.Domain/Models/` — heredar de `AuditableBaseEntity` (o `BaseEntity`, `IdentifiableEntity`, `NameableEntity`)
2. Configuración Fluent API en `InclusiON.Data/Configurations/`
3. `DbSet<T>` en `AppDbContext.cs`
4. Migración: `Add-Migration <Nombre> -Project InclusiON.Data -StartupProject InclusiON.Api`

### Paginación

Siempre usar las extensions, nunca Skip/Take manual:

```csharp
// Sin sorting
return await query.ToPagedAsync(page, pageSize, cancellationToken);

// Con sorting dinámico
var sortMappings = new Dictionary<SortField, Expression<Func<T, object>>>
{
    [SortField.Id] = p => p.Id,
    [SortField.FirstName] = p => p.FirstName,
    [SortField.CreatedAt] = p => p.CreatedAt
};
return await query.ToPagedAsync(page, pageSize, sortBy, sortDirection, sortMappings, ct);
```

### Filtros para entidades auditables

```csharp
query.WhereActive()                          // IsActive == true
query.WhereInactive()                        // IsActive == false
query.WhereIsActive(bool?)                   // Filtro condicional
query.WhereCreatedBetween(from?, to?)        // Rango de fechas
query.WhereCreatedBy(userId)                 // Creado por usuario
```

### SortField enum

Valores disponibles: `Id`, `CreatedAt`, `Name`, `FirstName`, `LastName`, `BirthDate`, `Email`. Default: `SortField.Id`.

---

## Entidades Base

| Clase | Campos |
|-------|--------|
| `BaseEntity` | `Id` (int) |
| `IdentifiableEntity` | + `Guid` |
| `NameableEntity` | + `Name`, `Description` |
| `AuditableBaseEntity` | + `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`, `IsActive` |

---

## Controllers Existentes

| Controller | Endpoints | HU |
|-----------|-----------|-----|
| `AuthController` | Login, PIN login, visual login, assisted, family, refresh, register | Transversal |
| `CatalogsController` | GET disability-types, autonomy-levels, activity-categories, skill-areas, template-types | BE-01, BE-04, BE-05 |
| `PersonsController` | CRUD personas, login methods | Transversal |
| `ProfessionalsController` | CRUD profesionales, deactivate | BE-02 |
| `UsersController` | GetById, GetProfile | Transversal |

---

## Handlers Existentes (Application/UseCases/)

### Auth
- `LoginCommandHandler`, `PinLoginCommandHandler`, `VisualStandardLoginCommandHandler`
- `AssistedLoginCommandHandler`, `FamilyLoginCommandHandler`
- `RefreshTokenCommandHandler`, `RegisterUserCommandHandler`
- `IdentifyUserQueryHandler`, `GetLoginMethodsQueryHandler`

### Catalogs
- `GetActivityCategoriesQueryHandler`, `GetActivityTemplateTypesQueryHandler`
- `GetAutonomyLevelsQueryHandler`, `GetDisabilityTypesQueryHandler`, `GetSkillAreasQueryHandler`

### Persons
- `CreatePersonCommandHandler`, `UpdatePersonCommandHandler`, `UpdateLoginMethodCommandHandler`
- `GetPersonsQueryHandler`, `GetPersonByIdQueryHandler`

### Professionals
- `CreateProfessionalCommandHandler`, `UpdateProfessionalCommandHandler`, `DeactivateProfessionalCommandHandler`
- `GetProfessionalsQueryHandler`, `GetProfessionalByIdQueryHandler`

### Users
- `GetUserProfileQueryHandler`

---

## Repositorios Existentes

| Interface | Implementación | Entidad principal |
|-----------|---------------|-------------------|
| `IPersonsRepository` | `PersonsRepository` | `PersonWithDisability` |
| `IProfessionalsRepository` | `ProfessionalsRepository` | `Professional` |
| `IUsersRepository` | `UsersRepository` | `User` |
| `IRefreshTokensRepository` | `RefreshTokensRepository` | `RefreshToken` |
| `IVisualLoginRepository` | `VisualLoginRepository` | Login visual data |

---

## Entidades del Dominio (39 entidades)

### Implementadas con handlers
`User`, `PersonWithDisability`, `Professional`, `RefreshToken`, `LoginMethod`, `TrustedDevice`, `AccessAudit`

### Con migración pero sin handlers (pendientes)
`Activity`, `ActivityContent`, `ActivityAssignment`, `ActivityResponse`, `ActivityEmbedding`, `ActivityResult`, `ActivityCategory`, `ActivityTemplateType`, `AdaptiveEngineConfig`, `AdaptiveAdjustmentLog`, `AutonomyLevel`, `Diagnosis`, `DisabilityType`, `EducationalInstitution`, `FamilyRepresentative`, `Invitation`, `Message`, `PersonRepresentative`, `PersonRoadmap`, `PersonRoadmapActivity`, `PersonRoadmapArea`, `ProfessionalInstitution`, `ProfessionalPerson`, `Report`, `ReportType`, `SkillArea`

---

## Features Complejos (documentación aparte)

- **Búsqueda Semántica:** Ver `Documentacion/Features/integracion-semantic-search.md`
- **Motor Adaptativo (MDA):** Ver `Documentacion/Features/MDA_Especificacion_Tecnica.md`

---

## Lo que NO hacer

- No registrar handlers manualmente en DI — se auto-registran por reflexión
- No usar Skip/Take manual — usar `ToPagedAsync()`
- No poner lógica de negocio en controllers — solo delegar al handler
- No modificar entidades base sin consultar al equipo
- No crear migraciones sin verificar que el modelo compila (`dotnet build`)
