# CHECKLIST — Tests pendientes

**Proyecto:** InclusiON.Server  
**Fecha:** 2026-04-28  
**Referencia:** análisis de cobertura post IN-185

Estado actual: **178 tests unitarios, 0 tests de arquitectura**  
Cobertura de handlers: **9 / 93 (9.7%)**

---

## Leyenda

- 🔴 Alta — lógica de negocio compleja, flujos de estado, seguridad crítica
- 🟡 Media — lógica moderada o patrón ya establecido en el proyecto
- 🟢 Baja — query handlers simples (fetch + map), valor limitado
- `[comando]` — handler de comando (tiene lógica de negocio)
- `[query]` — handler de consulta (generalmente fetch + map)

---

## 1. Tests unitarios — Handlers

### 1.1 Reports

Los command handlers de reportes tienen la lógica de la máquina de estados más compleja del sistema.

| # | Clase de test | Handler | Prioridad | Casos clave |
|---|--------------|---------|-----------|-------------|
| [ ] | `SubmitReportCommandHandlerTests` | `SubmitReportCommandHandler` [comando] | 🔴 | solo Draft→Submitted; profesional creador; informe no encontrado |
| [ ] | `ApproveReportCommandHandlerTests` | `ApproveReportCommandHandler` [comando] | 🔴 | solo desde Submitted; informe no encontrado; éxito graba ApprovedAt |
| [ ] | `RejectReportCommandHandlerTests` | `RejectReportCommandHandler` [comando] | 🔴 | solo desde Submitted; comment requerido; informe no encontrado |
| [ ] | `UpdateReportCommandHandlerTests` | `UpdateReportCommandHandler` [comando] | 🔴 | permite Draft y Rejected; bloquea Submitted y Approved; solo creador |
| [ ] | `CreateReportCommandHandlerTests` | `CreateReportCommandHandler` [comando] | 🟡 | profesional no encontrado; profesional no aprobado; persona no encontrada; creación exitosa |
| [ ] | `GetReportByIdQueryHandlerTests` | `GetReportByIdQueryHandler` [query] | 🟢 | no encontrado; encontrado y mapeado |
| [ ] | `GetReportsQueryHandlerTests` | `GetReportsQueryHandler` [query] | 🟢 | filtros opcionales; paginación |
| [ ] | `GetFamilyReportsQueryHandlerTests` | `GetFamilyReportsQueryHandler` [query] | 🟡 | solo devuelve reportes aprobados; filtrado por familiar autenticado |

---

### 1.2 AdminUsers

| # | Clase de test | Handler | Prioridad | Casos clave |
|---|--------------|---------|-----------|-------------|
| [ ] | `AdminDeactivateUserCommandHandlerTests` | `AdminDeactivateUserCommandHandler` [comando] | 🔴 | no puede desactivarse a sí mismo; ya inactivo (no-op); usuario no encontrado; éxito |
| [ ] | `AdminReactivateUserCommandHandlerTests` | `AdminReactivateUserCommandHandler` [comando] | 🔴 | ya activo (no-op); usuario no encontrado; éxito |
| [ ] | `AdminResetPasswordCommandHandlerTests` | `AdminResetPasswordCommandHandler` [comando] | 🟡 | usuario no encontrado; falla de identity; email enviado; éxito |
| [ ] | `GetAdminUsersQueryHandlerTests` | `GetAdminUsersQueryHandler` [query] | 🟢 | lista vacía; mapeo de campos |
| [ ] | `GetAdminUserDetailQueryHandlerTests` | `GetAdminUserDetailQueryHandler` [query] | 🟢 | no encontrado; encontrado y mapeado |

---

### 1.3 Professionals

| # | Clase de test | Handler | Prioridad | Casos clave |
|---|--------------|---------|-----------|-------------|
| [ ] | `ValidateProfessionalCommandHandlerTests` | `ValidateProfessionalCommandHandler` [comando] | 🔴 | Pending→Approved; Pending→Rejected; estado inválido para validar; ya aprobado |
| [ ] | `DeactivateProfessionalCommandHandlerTests` | `DeactivateProfessionalCommandHandler` [comando] | 🔴 | ya inactivo (no-op); en estado Pending bloqueado o permitido; éxito |
| [ ] | `ReactivateProfessionalCommandHandlerTests` | `ReactivateProfessionalCommandHandler` [comando] | 🔴 | ya activo (no-op); profesional no encontrado; éxito |
| [ ] | `RegisterProfessionalCommandHandlerTests` | `RegisterProfessionalCommandHandler` [comando] | 🟡 | email duplicado; creación de usuario ASP.NET Identity; envío de email |
| [ ] | `CreateProfessionalCommandHandlerTests` | `CreateProfessionalCommandHandler` [comando] | 🟡 | profesional ya vinculado al usuario; institución no encontrada; éxito |
| [ ] | `UpdateProfessionalCommandHandlerTests` | `UpdateProfessionalCommandHandler` [comando] | 🟡 | solo el propio profesional puede editar; profesional no encontrado; éxito |
| [ ] | `SuspendInactiveProfessionalsCommandHandlerTests` | `SuspendInactiveProfessionalsCommandHandler` [comando] | 🟡 | ningún profesional elegible; múltiples profesionales suspendidos |
| [ ] | `GetProfessionalByIdQueryHandlerTests` | `GetProfessionalByIdQueryHandler` [query] | 🟢 | no encontrado; encontrado y mapeado |
| [ ] | `GetProfessionalsQueryHandlerTests` | `GetProfessionalsQueryHandler` [query] | 🟢 | filtros; paginación |
| [ ] | `GetPendingProfessionalsQueryHandlerTests` | `GetPendingProfessionalsQueryHandler` [query] | 🟢 | lista vacía; solo devuelve Pending |
| [ ] | `GetProfessionalStatusHistoryQueryHandlerTests` | `GetProfessionalStatusHistoryQueryHandler` [query] | 🟢 | no encontrado; historial ordenado |

---

### 1.4 Persons

| # | Clase de test | Handler | Prioridad | Casos clave |
|---|--------------|---------|-----------|-------------|
| [ ] | `CreatePersonCommandHandlerTests` | `CreatePersonCommandHandler` [comando] | 🔴 | institución no encontrada; DNI duplicado; creación con nivel de autonomía opcional |
| [ ] | `UpdatePersonCommandHandlerTests` | `UpdatePersonCommandHandler` [comando] | 🟡 | persona no encontrada; solo profesional asignado puede editar; éxito |
| [ ] | `DeactivatePersonCommandHandlerTests` | `DeactivatePersonCommandHandler` [comando] | 🔴 | ya inactiva; tiene asignaciones activas (bloqueo o advertencia); éxito |
| [ ] | `UpdateLoginMethodCommandHandlerTests` | `UpdateLoginMethodCommandHandler` [comando] | 🟡 | persona no encontrada; método inválido; PIN requerido si método PIN |
| [ ] | `GetPersonByIdQueryHandlerTests` | `GetPersonByIdQueryHandler` [query] | 🟢 | no encontrado; encontrado |
| [ ] | `GetPersonsQueryHandlerTests` | `GetPersonsQueryHandler` [query] | 🟢 | filtros; paginación |
| [ ] | `GetSupervisorCandidatesQueryHandlerTests` | `GetSupervisorCandidatesQueryHandler` [query] | 🟢 | devuelve solo profesionales Approved asignados |

---

### 1.5 Family

| # | Clase de test | Handler | Prioridad | Casos clave |
|---|--------------|---------|-----------|-------------|
| [ ] | `LinkFamilyToPersonCommandHandlerTests` | `LinkFamilyToPersonCommandHandler` [comando] | 🔴 | familiar no encontrado; persona no encontrada; ya vinculado activo; éxito |
| [ ] | `UnlinkFamilyFromPersonCommandHandlerTests` | `UnlinkFamilyFromPersonCommandHandler` [comando] | 🔴 | vínculo no encontrado; ya inactivo; éxito |
| [ ] | `CreateFamilyCommandHandlerTests` | `CreateFamilyCommandHandler` [comando] | 🟡 | email duplicado; creación de usuario ASP.NET Identity; éxito |
| [ ] | `DeactivateFamilyCommandHandlerTests` | `DeactivateFamilyCommandHandler` [comando] | 🟡 | ya inactivo; tiene vínculo activo (bloqueo); éxito |
| [ ] | `UpdateFamilyCommandHandlerTests` | `UpdateFamilyCommandHandler` [comando] | 🟡 | familiar no encontrado; email duplicado; éxito |
| [ ] | `GetFamilyQueryHandlerTests` | `GetFamilyQueryHandler` [query] | 🟢 | lista vacía; mapeo |
| [ ] | `GetFamilyByIdQueryHandlerTests` | `GetFamilyByIdQueryHandler` [query] | 🟢 | no encontrado; encontrado |
| [ ] | `GetFamilyLinkHistoryQueryHandlerTests` | `GetFamilyLinkHistoryQueryHandler` [query] | 🟢 | sin historial; historial ordenado |
| [ ] | `GetPersonLinkHistoryQueryHandlerTests` | `GetPersonLinkHistoryQueryHandler` [query] | 🟢 | sin historial; historial ordenado |
| [ ] | `GetAvailableFamiliesQueryHandlerTests` | `GetAvailableFamiliesQueryHandler` [query] | 🟢 | filtra ya vinculados |
| [ ] | `GetFamilyStatusHistoryQueryHandlerTests` | `GetFamilyStatusHistoryQueryHandler` [query] | 🟢 | historial ordenado |

---

### 1.6 Auth

| # | Clase de test | Handler | Prioridad | Casos clave |
|---|--------------|---------|-----------|-------------|
| [ ] | `LoginCommandHandlerTests` | `LoginCommandHandler` [comando] | 🔴 | usuario no encontrado; contraseña incorrecta; cuenta bloqueada; cuenta inactiva; éxito con token |
| [ ] | `PinLoginCommandHandlerTests` | `PinLoginCommandHandler` [comando] | 🔴 | PIN incorrecto; persona no encontrada; PIN no configurado; éxito |
| [ ] | `FamilyLoginCommandHandlerTests` | `FamilyLoginCommandHandler` [comando] | 🔴 | email no encontrado; método no permitido; contraseña incorrecta; éxito |
| [ ] | `AssistedLoginCommandHandlerTests` | `AssistedLoginCommandHandler` [comando] | 🔴 | supervisor no autorizado; persona no encontrada; éxito |
| [ ] | `VisualStandardLoginCommandHandlerTests` | `VisualStandardLoginCommandHandler` [comando] | 🔴 | contraseña visual incorrecta; persona no encontrada; éxito |
| [ ] | `RefreshTokenCommandHandlerTests` | `RefreshTokenCommandHandler` [comando] | 🔴 | token inválido; token expirado; usuario inactivo; éxito con nuevos tokens |
| [ ] | `ChangePasswordCommandHandlerTests` | `ChangePasswordCommandHandler` [comando] | 🔴 | contraseña actual incorrecta; nueva contraseña débil; éxito |
| [ ] | `RegisterUserCommandHandlerTests` | `RegisterUserCommandHandler` [comando] | 🟡 | email duplicado; contraseña débil; éxito |
| [ ] | `GetLoginMethodsQueryHandlerTests` | `GetLoginMethodsQueryHandler` [query] | 🟡 | usuario no encontrado; devuelve métodos configurados |
| [ ] | `IdentifyUserQueryHandlerTests` | `IdentifyUserQueryHandler` [query] | 🟡 | email no encontrado; devuelve datos de identificación |

---

### 1.7 Diagnoses (completa el módulo)

| # | Clase de test | Handler | Prioridad | Casos clave |
|---|--------------|---------|-----------|-------------|
| [ ] | `CreateDiagnosisCommandHandlerTests` | `CreateDiagnosisCommandHandler` [comando] | 🟡 | profesional no encontrado; profesional no aprobado; persona no encontrada; éxito |
| [ ] | `UpdateDiagnosisCommandHandlerTests` | `UpdateDiagnosisCommandHandler` [comando] | 🟡 | diagnóstico no encontrado; solo creador puede editar; profesional no aprobado; éxito |
| [ ] | `GetDiagnosesByPersonQueryHandlerTests` | `GetDiagnosesQueryHandler` [query] | 🟢 | lista vacía; devuelve solo activos; mapeo de campos |
| [ ] | `GetDiagnosisByIdQueryHandlerTests` | `GetDiagnosisByIdQueryHandler` [query] | 🟢 | no encontrado; encontrado y mapeado |

---

### 1.8 Institutions (completa el módulo)

| # | Clase de test | Handler | Prioridad | Casos clave |
|---|--------------|---------|-----------|-------------|
| [ ] | `CreateInstitutionCommandHandlerTests` | `CreateInstitutionCommandHandler` [comando] | 🟡 | nombre duplicado; éxito |
| [ ] | `UpdateInstitutionCommandHandlerTests` | `UpdateInstitutionCommandHandler` [comando] | 🟡 | institución no encontrada; inactiva; nombre duplicado; éxito |
| [ ] | `GetInstitutionsQueryHandlerTests` | `GetInstitutionsQueryHandler` [query] | 🟢 | lista vacía; filtros; paginación |

---

### 1.9 Assignments

| # | Clase de test | Handler | Prioridad | Casos clave |
|---|--------------|---------|-----------|-------------|
| [ ] | `AssignPersonCommandHandlerTests` | `AssignPersonCommandHandler` [comando] | 🔴 | profesional no encontrado; persona no encontrada; ya asignado; éxito |
| [ ] | `DeactivatePersonAssignmentCommandHandlerTests` | `DeactivatePersonAssignmentCommandHandler` [comando] | 🔴 | asignación no encontrada; ya inactiva; éxito |
| [ ] | `AssignInstitutionCommandHandlerTests` | `AssignInstitutionCommandHandler` [comando] | 🟡 | ya asignado; institución no encontrada; éxito |
| [ ] | `RemoveInstitutionAssignmentCommandHandlerTests` | `RemoveInstitutionAssignmentCommandHandler` [comando] | 🟡 | asignación no encontrada; éxito |
| [ ] | `GetPersonsByProfessionalQueryHandlerTests` | `GetPersonsByProfessionalQueryHandler` [query] | 🟢 | lista vacía; solo devuelve activos |
| [ ] | `GetInstitutionsByProfessionalQueryHandlerTests` | `GetInstitutionsByProfessionalQueryHandler` [query] | 🟢 | lista vacía; mapeo |

---

### 1.10 Invitations

| # | Clase de test | Handler | Prioridad | Casos clave |
|---|--------------|---------|-----------|-------------|
| [ ] | `CreateInvitationCommandHandlerTests` | `CreateInvitationCommandHandler` [comando] | 🟡 | profesional no encontrado; invitación duplicada activa; expira en N días; éxito |
| [ ] | `AcceptInvitationCommandHandlerTests` | `AcceptInvitationCommandHandler` [comando] | 🟡 | token no encontrado; expirada; ya usada; éxito vincula y marca como usada |
| [ ] | `GetInvitationsQueryHandlerTests` | `GetInvitationsQueryHandler` [query] | 🟢 | lista vacía; mapeo |
| [ ] | `ValidateInvitationQueryHandlerTests` | `ValidateInvitationQueryHandler` [query] | 🟡 | no encontrada; expirada; ya usada; válida |

---

### 1.11 Catalogs

| # | Clase de test | Handler | Prioridad | Casos clave |
|---|--------------|---------|-----------|-------------|
| [ ] | `GetCatalogsQueryHandlersTests` | todos los 6 GetXxx query handlers | 🟢 | lista vacía; devuelve solo activos; mapeo de campos |

> Pueden agruparse en un solo archivo de test ya que todos siguen el mismo patrón simple.

---

### 1.12 Users

| # | Clase de test | Handler | Prioridad | Casos clave |
|---|--------------|---------|-----------|-------------|
| [ ] | `GetUserProfileQueryHandlerTests` | `GetUserProfileQueryHandler` [query] | 🟢 | usuario no encontrado; encontrado y mapeado |

---

## 2. Tests unitarios — Controllers

Los tests de controllers existentes validan: política de autorización, extracción de `entityId` del JWT, y delegación al handler. Faltan los siguientes controllers con ese mismo patrón.

| # | Clase de test | Controller | Prioridad | Qué validar |
|---|--------------|-----------|-----------|-------------|
| [ ] | `AuthControllerTests` | `AuthController` | 🔴 | rutas anónimas vs. protegidas; rate limiting en login; `[AllowAnonymous]` en endpoints correctos |
| [ ] | `AssignmentsControllerTests` | `AssignmentsController` | 🟡 | profesional obtiene su entityId; BadRequest si entityId null |
| [ ] | `PersonsControllerTests` | `PersonsController` | 🟡 | profesional obtiene entityId para crear/actualizar; admin obtiene userId |
| [ ] | `FamilyControllerTests` | `FamilyController` | 🟡 | admin: GetCurrentUserId no null; familiar: GetCurrentEntityId no null |
| [ ] | `InstitutionsControllerTests` | `InstitutionsController` | 🟡 | admin solo: política de autorización; PATCH status delega comando correcto |
| [ ] | `CatalogsControllerTests` | `CatalogsController` | 🟢 | endpoints públicos o autenticados; no requieren tests exhaustivos |
| [ ] | `UsersControllerTests` | `UsersController` | 🟢 | GetCurrentUserId no null; respuesta mapeada |
| [ ] | `RolesControllerTests` | `RolesController` | 🟢 | política de autorización; solo admin global |

---

## 3. Tests de integración

La infraestructura ya existe: `WebApplicationFactory` + `InMemoryDatabase` + `TokenHelper` + `AuthorizationTestFixture`.

Los tests de integración existentes cubren: autorización de recursos (5 endpoints) y smoke test del pipeline.

### 3.1 Flujos de estado — Reports

| # | Test | Escenario | Prioridad |
|---|------|-----------|-----------|
| [ ] | `ReportWorkflowIntegrationTests` | Draft → Submit → Approve: flujo completo HTTP real | 🔴 |
| [ ] | `ReportWorkflowIntegrationTests` | Draft → Submit → Reject → Edit → Submit: re-envío post rechazo | 🔴 |
| [ ] | `ReportWorkflowIntegrationTests` | Intento de editar un Submitted devuelve 422 | 🟡 |
| [ ] | `ReportWorkflowIntegrationTests` | Solo el creador puede editar/dar de baja; otro profesional recibe 403 | 🟡 |
| [ ] | `ReportWorkflowIntegrationTests` | Familiar solo ve reportes Approved vía GET /reports/family | 🟡 |

### 3.2 Flujos de estado — Diagnoses

| # | Test | Escenario | Prioridad |
|---|------|-----------|-----------|
| [ ] | `DiagnosisWorkflowIntegrationTests` | Profesional crea, actualiza y da de baja su propio diagnóstico | 🟡 |
| [ ] | `DiagnosisWorkflowIntegrationTests` | Profesional B no puede editar/dar de baja diagnóstico de profesional A | 🟡 |
| [ ] | `DiagnosisWorkflowIntegrationTests` | Admin puede dar de baja cualquier diagnóstico | 🟡 |

### 3.3 Flujos de estado — Professionals

| # | Test | Escenario | Prioridad |
|---|------|-----------|-----------|
| [ ] | `ProfessionalWorkflowIntegrationTests` | Pending → Approved → Deactivated → Reactivated | 🟡 |
| [ ] | `ProfessionalWorkflowIntegrationTests` | Pending → Rejected: no puede acceder a endpoints de profesional aprobado | 🟡 |

### 3.4 Acceso familiar

| # | Test | Escenario | Prioridad |
|---|------|-----------|-----------|
| [ ] | `FamilyAccessIntegrationTests` | Familiar con vínculo activo accede a datos de su persona | 🟡 |
| [ ] | `FamilyAccessIntegrationTests` | Familiar sin vínculo recibe 404 en datos de persona ajena | 🟡 |
| [ ] | `FamilyAccessIntegrationTests` | Vínculo desactivado revoca acceso inmediatamente | 🔴 |

### 3.5 Auth flows

| # | Test | Escenario | Prioridad |
|---|------|-----------|-----------|
| [ ] | `AuthIntegrationTests` | Login válido devuelve access token + refresh token | 🔴 |
| [ ] | `AuthIntegrationTests` | Refresh token inválido devuelve 401 | 🔴 |
| [ ] | `AuthIntegrationTests` | Access token expirado + refresh token válido → nuevos tokens | 🔴 |
| [ ] | `AuthIntegrationTests` | Login incorrecto 5 veces bloquea la cuenta | 🟡 |

---

## 4. Tests de arquitectura

**Estado actual: 0 tests de arquitectura.**  
Librería recomendada: **NetArchTest.Rules** (NuGet).

Agregar al proyecto de tests unitarios:

```xml
<PackageReference Include="NetArchTest.Rules" Version="1.3.2" />
```

### 4.1 Dependencias entre capas

| # | Test | Regla | Prioridad |
|---|------|-------|-----------|
| [ ] | `LayerDependencyTests` | `InclusiON.Domain` no referencia `Application`, `Infrastructure` ni `Api` | 🔴 |
| [ ] | `LayerDependencyTests` | `InclusiON.Application` no referencia `Infrastructure` ni `Api` | 🔴 |
| [ ] | `LayerDependencyTests` | `InclusiON.Application` no instancia `DbContext` directamente | 🔴 |
| [ ] | `LayerDependencyTests` | `InclusiON.Api` no instancia repositorios directamente (sin `new XxxRepository`) | 🔴 |

### 4.2 Handlers — convenciones de implementación

| # | Test | Regla | Prioridad |
|---|------|-------|-----------|
| [ ] | `HandlerConventionTests` | Toda clase que termine en `CommandHandler` implementa `ICommandHandler<,>` | 🔴 |
| [ ] | `HandlerConventionTests` | Toda clase que termine en `QueryHandler` implementa `IQueryHandler<,>` | 🔴 |
| [ ] | `HandlerConventionTests` | Los handlers no son públicos desde `InclusiON.Api` (solo desde `Application`) | 🟡 |

### 4.3 Repositorios — convenciones de implementación

| # | Test | Regla | Prioridad |
|---|------|-------|-----------|
| [ ] | `RepositoryConventionTests` | Toda clase que termine en `Repository` implementa una interfaz `IXxxRepository` | 🟡 |
| [ ] | `RepositoryConventionTests` | Las implementaciones de repositorios están solo en `InclusiON.Infrastructure` | 🟡 |
| [ ] | `RepositoryConventionTests` | Ningún controller usa `AppDbContext` directamente | 🔴 |

### 4.4 Dominio — inmutabilidad y pureza

| # | Test | Regla | Prioridad |
|---|------|-------|-----------|
| [ ] | `DomainModelTests` | Toda entidad hereda de `AuditableBaseEntity` o `BaseEntity` | 🟡 |
| [ ] | `DomainModelTests` | Las entidades del dominio no tienen dependencias a namespaces de `Application` ni `Infrastructure` | 🔴 |

### 4.5 Controllers — seguridad

| # | Test | Regla | Prioridad |
|---|------|-------|-----------|
| [ ] | `ControllerSecurityTests` | Todo controller tiene al menos un `[Authorize]` (a nivel clase o todos sus métodos) | 🔴 |
| [ ] | `ControllerSecurityTests` | Ningún endpoint marcado `[AllowAnonymous]` está en un controller que maneje datos de usuarios | 🟡 |

### 4.6 DTOs — convenciones

| # | Test | Regla | Prioridad |
|---|------|-------|-----------|
| [ ] | `DtoConventionTests` | Clases en `InclusiON.DTOs.Requests` no tienen referencias a entidades de dominio | 🟡 |
| [ ] | `DtoConventionTests` | Clases en `InclusiON.DTOs.Responses` no tienen referencias a entidades de dominio | 🟡 |

---

## Resumen

| Tipo | Total items | 🔴 Alta | 🟡 Media | 🟢 Baja |
|------|------------|---------|---------|---------|
| Unit — Handlers | 61 | 22 | 23 | 16 |
| Unit — Controllers | 8 | 1 | 4 | 3 |
| Integración | 17 | 6 | 11 | 0 |
| Arquitectura | 18 | 9 | 8 | 0 |
| **Total** | **104** | **38** | **46** | **19** |

### Orden de ataque sugerido para la tesis

1. **Arquitectura** — rendimiento alto, se escriben rápido, demuestran madurez técnica
2. **Reports handlers** — módulo central, lógica de estado compleja, ya testeamos deactivate
3. **Auth handlers** — críticos para seguridad, valor demostrativo alto en la tesis
4. **Professionals handlers** — módulo más complejo del sistema
5. **Integración** — flujos de Reports y Auth (mayor valor narrativo)
6. Resto según tiempo disponible
