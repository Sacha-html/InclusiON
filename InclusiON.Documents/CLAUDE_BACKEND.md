# CLAUDE.md — Backend (.NET 10)

Instrucciones para agentes AI y desarrolladores trabajando en `InclusiON.Server/`.

---

## Proyecto y Stack

- **.NET 10** con Clean Architecture
- **EF Core** (PostgreSQL / Npgsql) para persistencia
- **IRawDbExecutor** para consultas SQL raw optimizadas (usado en AdminUsers)
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
InclusiON.SemanticSearch/   ← Library ONNX para embeddings con búsqueda semántica implementada
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

Valores disponibles: `Id`, `CreatedAt`, `Name`, `FirstName`, `LastName`, `BirthDate`, `Email`, `Title`, `ReportDate`, `Specialty`, `LicenseNumber`, `Status`. Default: `SortField.Id`.

**Nota PostgreSQL:** usar siempre `EF.Functions.ILike()` para búsqueda case-insensitive, nunca `EF.Functions.Like()` ni `.ToLower()`.

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

| Controller | Endpoints |
|-----------|-----------|
| `AuthController` | identify, login (standard/PIN/assisted/family), refresh, register, change-login-method |
| `CatalogsController` | GET disability-types, autonomy-levels, activity-categories, skill-areas, template-types, login-methods, avatar-colors, report-types |
| `CatalogAdminController` | CRUD de 6 tipos de catálogo (admin) |
| `PersonsController` | CRUD personas, supervisor-candidates, professional-assignments, login methods |
| `ProfessionalsController` | CRUD profesionales, pending, validate, reactivate, deactivate, status-history, reset-password |
| `ProfessionalValidationController` | Validación/rechazo de profesionales pendientes por admin |
| `FamilyController` | CRUD representantes familiares |
| `InstitutionsController` | CRUD instituciones educativas, asignaciones profesional-institución |
| `AdminInstitutionsController` | Asignación/desasignación de admin institucional a institución |
| `AssignmentsController` | Asignaciones profesional-persona |
| `InvitationsController` | Crear invitación, aceptar, validar código |
| `ReportsController` | CRUD informes, submit, approve, reject, GET /family |
| `ActivitiesController` | GET (list paginado + filtros), GET (by id), POST, PUT, PATCH /status |
| `ActivityAssignmentsController` | POST (asignar), GET /persons/{id}/activity-assignments, GET /my/activity-assignments, POST .../responses/start, POST .../responses/{resId}/complete |
| `DiagnosesController` | GET /persons/{id}/diagnoses, GET /diagnoses/{id}, POST, PUT |
| `AdminUsersController` | Listado paginado, reset-password, deactivate, reactivate |
| `AdminDashboardController` | GET /api/admin/dashboard — 8 KPIs (profesionales, validaciones pendientes, familias, personas, instituciones, asignaciones activas, reportes pendientes/aprobados) |
| `RolesController` | Listado de roles y permisos, asignar permisos |
| `UsersController` | GetById, GetProfile |
| `MessagesController` | GET inbox, GET sent, GET unread-count, GET {id}, POST (send), POST {id}/reply, PUT {id}/read, DELETE {id} |
| `RoadmapController` | GET /persons/{id}/roadmap, POST (create), PATCH notes, POST areas, DELETE areas/{id}, POST areas/{id}/activities, DELETE activities/{id}, PATCH activities/{id}/unlock |

---

## Handlers Existentes (Application/UseCases/)

### Auth
- `LoginCommandHandler`, `PinLoginCommandHandler`, `VisualStandardLoginCommandHandler` (identifica por nombre + contraseña)
- `AssistedLoginCommandHandler`, `FamilyLoginCommandHandler`
- `RefreshTokenCommandHandler`, `RegisterUserCommandHandler`
- `IdentifyUserQueryHandler`, `GetLoginMethodsQueryHandler`

### Catalogs
- `GetActivityCategoriesQueryHandler`, `GetActivityTemplateTypesQueryHandler`
- `GetAutonomyLevelsQueryHandler`, `GetDisabilityTypesQueryHandler`, `GetSkillAreasQueryHandler`
- `GetAvatarColorsQueryHandler`, `GetLoginMethodsQueryHandler`
- `GetReportTypesQueryHandler`

### Persons
- `CreatePersonCommandHandler`, `UpdatePersonCommandHandler`
- `GetPersonsQueryHandler`, `GetPersonByIdQueryHandler`
- `GetPersonProfessionalsQueryHandler`, `GetSupervisorCandidatesQueryHandler`
- `UpdateLoginMethodCommandHandler`

### Professionals
- `CreateProfessionalCommandHandler`, `UpdateProfessionalCommandHandler`
- `DeactivateProfessionalCommandHandler`, `ReactivateProfessionalCommandHandler`
- `ValidateProfessionalCommandHandler`, `AdminResetPasswordCommandHandler`
- `GetProfessionalsQueryHandler`, `GetProfessionalByIdQueryHandler`
- `GetPendingProfessionalsQueryHandler`, `GetProfessionalStatusHistoryQueryHandler`

### Family
- `CreateFamilyCommandHandler`, `UpdateFamilyCommandHandler`, `DeactivateFamilyCommandHandler`
- `GetFamilyQueryHandler`, `GetFamilyByIdQueryHandler`

### Institutions
- `CreateInstitutionCommandHandler`, `UpdateInstitutionCommandHandler`
- `GetInstitutionsQueryHandler`, `GetInstitutionByIdQueryHandler`

### Assignments
- `AssignProfessionalToPersonCommandHandler`, `RemoveAssignmentCommandHandler`
- `GetAssignmentsQueryHandler`

### Invitations
- `CreateInvitationCommandHandler`, `AcceptInvitationCommandHandler`
- `ValidateInvitationCodeQueryHandler`

### Reports
- `CreateReportCommandHandler`, `UpdateReportCommandHandler`
- `SubmitReportCommandHandler`, `ApproveReportCommandHandler`, `RejectReportCommandHandler`
- `GetReportsQueryHandler`, `GetReportByIdQueryHandler`, `GetFamilyReportsQueryHandler`

### Diagnoses
- `CreateDiagnosisCommandHandler`, `UpdateDiagnosisCommandHandler`
- `GetDiagnosesQueryHandler`, `GetDiagnosisByIdQueryHandler`

### AdminUsers
- `GetAdminUsersQueryHandler`, `GetAdminUserDetailQueryHandler`
- `GetAdminDashboardQueryHandler`, `GetUserActivityQueryHandler`
- `AdminDeactivateUserCommandHandler`, `AdminReactivateUserCommandHandler`
- `AdminResetPasswordCommandHandler`

### Activities
- `CreateActivityCommandHandler`, `UpdateActivityCommandHandler`, `PatchActivityStatusCommandHandler`
- `GetActivitiesQueryHandler`, `GetActivityByIdQueryHandler`

### ActivityAssignments
- `CreateActivityAssignmentCommandHandler`
- `GetPersonActivityAssignmentsQueryHandler`
- `StartActivityResponseCommandHandler`, `CompleteActivityResponseCommandHandler`

### Users
- `GetUserProfileQueryHandler`

### Messages
- `GetInboxQueryHandler`, `GetSentQueryHandler`, `GetMessageByIdQueryHandler`, `GetUnreadCountQueryHandler`
- `GetMessageContactsQueryHandler`
- `SendMessageCommandHandler`, `ReplyToMessageCommandHandler`
- `MarkMessageReadCommandHandler`, `DeleteMessageCommandHandler`

### Roadmap
- `GetPersonRoadmapQueryHandler`
- `CreateRoadmapCommandHandler`, `UpdateRoadmapNotesCommandHandler`
- `AddRoadmapAreaCommandHandler`, `RemoveRoadmapAreaCommandHandler`
- `AddRoadmapActivityCommandHandler`, `RemoveRoadmapActivityCommandHandler`, `UnlockRoadmapActivityCommandHandler`
- `ReorderRoadmapActivitiesCommandHandler`

---

## Repositorios Existentes

| Interface | Implementación | Entidad principal |
|-----------|---------------|-------------------|
| `IAssignmentsRepository` | `AssignmentsRepository` | `ProfessionalPerson`, `ProfessionalInstitution` (`HaveSharedPersonAsync` valida vínculo prof↔familiar) |
| `IPersonsRepository` | `PersonsRepository` | `PersonWithDisability` |
| `IProfessionalsRepository` | `ProfessionalsRepository` | `Professional` |
| `IFamilyRepository` | `FamilyRepository` | `FamilyRepresentative` |
| `IInstitutionsRepository` | `InstitutionsRepository` | `EducationalInstitution` |
| `IReportsRepository` | `ReportsRepository` | `Report` |
| `IDiagnosesRepository` | `DiagnosesRepository` | `Diagnosis` |
| `IUsersRepository` | `UsersRepository` | `User` (`GetByIdWithProfileAsync` incluye Professional/FamilyRepresentative/PersonWithDisability) |
| `IMessagesRepository` | `MessagesRepository` | `Message` |
| `IRoadmapRepository` | `RoadmapRepository` | `PersonRoadmap`, `PersonRoadmapArea`, `PersonRoadmapActivity` |
| `IRefreshTokensRepository` | `RefreshTokensRepository` | `RefreshToken` |
| `IVisualLoginRepository` | `VisualLoginRepository` | `TrustedDevice`, `LoginMethod` (login visual estándar y PIN) |

---

## Entidades del Dominio (39 entidades)

### Implementadas con handlers
`User`, `PersonWithDisability`, `Professional`, `FamilyRepresentative`, `EducationalInstitution`, `Report`, `Invitation`, `Diagnosis`, `RefreshToken`, `LoginMethod`, `TrustedDevice`, `AccessAudit`, `ProfessionalInstitution`, `ProfessionalPerson`, `PersonRepresentative`, `PersonSkillProfile`, `ProfessionalStatusHistory`, `FamilyStatusHistory`, `PersonRepresentativeHistory`, `Activity`, `ActivityContent`, `ActivityAssignment`, `ActivityResponse`, `Message`, `PersonRoadmap`, `PersonRoadmapArea`, `PersonRoadmapActivity`

### Con migración pero sin handlers (pendientes)
`ActivityEmbedding` (implementado con búsqueda semántica), `ActivityResult`, `ActivityCategory`\*, `ActivityTemplateType`\*, `AdaptiveEngineConfig`, `AdaptiveAdjustmentLog`, `AutonomyLevel`\*, `DisabilityType`\*, `ReportType`, `SkillArea`\*

\* Catálogos leídos solo por el `CatalogsController`, no necesitan handlers de escritura propios.

---

## Features Complejos (documentación aparte)

- **Búsqueda Semántica:** Ver `Features/integracion-semantic-search.md`
- **Motor Adaptativo (MDA):** Ver `Features/MDA_Especificacion_Tecnica.md`
- **Plan MVP Actividades:** Ver `Plans/actividades-embeddings-mvp.md`

---

## Lo que NO hacer

- No registrar handlers manualmente en DI — se auto-registran por reflexión
- No usar Skip/Take manual — usar `ToPagedAsync()`
- No poner lógica de negocio en controllers — solo delegar al handler
- No modificar entidades base sin consultar al equipo
- No crear migraciones sin verificar que el modelo compila (`dotnet build`)
