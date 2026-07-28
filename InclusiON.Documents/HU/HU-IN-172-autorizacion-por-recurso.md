# HU-IN-172 — Autorización por Recurso (Row-Level Authorization)

| Campo | Contenido |
|---|---|
| ID | HU-IN-172 |
| Épica | Seguridad / Autorización |
| Título | Autorización por Recurso (Row-Level Authorization) |
| Prioridad | Alta |
| Estimación | 8 puntos de historia |
| Sprint asignado | Sprint 3 |
| Estado | Completada |

**Asignado a:** Mirko Ivo Wlk

---

## Historia de Usuario

**Como** responsable de la seguridad de la plataforma (rol transversal que aplica a Profesional, Familia, Institución y Administrador)

**Quiero** que el acceso a los datos sensibles de cada persona (perfil, diagnósticos, reportes, respuestas, roadmap, mensajes) quede restringido a los usuarios que tengan un vínculo explícito con esa persona — asignación profesional activa, vínculo familiar, o pertenencia a la institución correspondiente —

**Para** cumplir con el principio de mínimo privilegio y con la Ley 25.326 de Protección de Datos Personales (Art. 8 — datos sensibles de salud, sobre menores de edad y personas con discapacidad), evitar filtraciones entre profesionales/familias de la misma institución, y dejar trazabilidad de cada acceso a datos clínicos.

### Historias complementarias por rol

- **Como Profesional**, quiero que el sistema me impida consultar el perfil, diagnósticos, reportes o respuestas de una persona que no tengo asignada — aun perteneciendo a mi misma institución — para no exponer datos clínicos de terceros.
- **Como Familiar**, quiero que mi acceso (consulta y login asistido) quede limitado a la persona que tengo a cargo, para garantizar que no puedo ver datos de menores ajenos.
- **Como Admin Institucional**, quiero que mi vista esté limitada a las instituciones que me fueron asignadas, tanto en listados como en accesos directos por ID, para operar solo dentro de mi alcance.
- **Como Admin Global**, quiero que todos mis accesos queden auditados aunque no estén restringidos, para poder demostrar cumplimiento ante una auditoría.

---

## Criterios de Aceptación

> ⚑ Cada criterio debe ser verificable: o se cumple o no se cumple. Evitar redacciones vagas.

| N° | El sistema debe… / Condición verificable |
|----|------------------------------------------|
| CA-01 | Un Profesional que invoca `GET /api/persons/{id}` sobre una persona que **no** tiene en `ProfessionalAssignments` (activa) recibe `403 Forbidden` aunque pertenezca a su institución. |
| CA-02 | Un Profesional que invoca `GET /api/persons` recibe únicamente las personas con `ProfessionalAssignment.IsActive = true` con su `ProfessionalId`. El filtro se aplica en el repositorio (no post-filtrado en memoria). |
| CA-03 | Un Profesional recibe `403` al invocar cualquier endpoint de `Diagnoses`, `Reports`, `Responses`, `PersonSkillProfile`, `Roadmap` de una persona no asignada. |
| CA-04 | Un Familiar que invoca endpoints de consulta (persona, reportes aprobados, roadmap lectura) sobre una persona sin `PersonRepresentative.IsActive = true` vinculado a su `FamilyRepresentative.UserId` recibe `404`. La relación es N:M: un Familiar puede tener varias personas a cargo y una persona puede tener varios representantes. |
| CA-05 | Un Familiar solo puede invocar `/Auth/login/assisted` y `/Auth/login/family` para personas donde exista `PersonRepresentative` con **`IsActive = true` Y `CanSuperviseLogin = true`**. Intento sobre otro `personId` → `404`. |
| CA-06 | Un Familiar **no** puede ver reportes en estado `Draft` o `Submitted`, solo `Approved`. |
| CA-07 | Un Admin Institucional recibe `403` al intentar acceder a un recurso cuya `InstitutionId` no está en su lista `AdminInstitutions`. El filtro se aplica también a entidades transitivas (reportes, asignaciones, invitaciones). |
| CA-08 | Un Admin Global puede acceder a todos los recursos (bypass de validación de vínculo) pero cada acceso queda registrado en `AccessAudit`. |
| CA-09 | Existe un servicio `IResourceAuthorizationService` con métodos tipados por entidad (`CanAccessPersonAsync`, `CanAccessReportAsync`, `CanAccessDiagnosisAsync`, `CanAccessResponseAsync`, `GetAccessiblePersonIdsAsync`, etc.) e inyectable en handlers. |
| CA-10 | La verificación es **fail-closed**: si no se puede determinar el vínculo (falla DB, usuario sin rol válido, recurso inexistente) la respuesta es deny, no allow. |
| CA-11 | Existe cache por request (scoped) que evita consultar la misma `ProfessionalAssignment` más de una vez dentro del mismo handler/pipeline. |
| CA-12 | Existe entidad y tabla `AccessAudit` con columnas `Id`, `UserId`, `Role`, `ResourceType`, `ResourceId`, `Action` (Read/Write), `Result` (Allowed/Denied), `Timestamp` (UTC), `IpAddress`, `CorrelationId`. La escritura es write-behind (no bloquea la respuesta). |
| CA-13 | Cada `QueryHandler` / `CommandHandler` sobre entidades sensibles (Person, SkillProfile, Diagnosis, Report, Response, Roadmap, Message) invoca la verificación antes de ejecutar la operación. |
| CA-14 | Existe suite de tests unitarios que cubre al menos 12 casos positivos y 12 negativos (matriz rol × entidad × acción). |
| CA-15 | Existen tests de integración que validan `403` en endpoints críticos: `GET /persons/{id}`, `GET /diagnoses/{id}`, `GET /reports/{id}`, `PUT /persons/{id}`, `POST /diagnoses`. |
| CA-16 | El frontend maneja `403` con interceptor HTTP mostrando toast "No tenés permiso para acceder a este recurso" y redirige al dashboard del rol. |
| CA-17 | Política de códigos de respuesta: **`404 Not Found`** para roles Familia y Estudiante (oculta existencia del recurso), **`403 Forbidden`** para Profesional y Admin (feedback claro para usuarios internos). Queda documentado en `References/REF-autenticacion.md`. |
| CA-18 | Un Profesional que invoca `GET /api/users/{id}` sobre un usuario que no es el propio, no es de un familiar de sus personas asignadas, ni es otro profesional asignado a sus personas, recibe `403`. |
| CA-19 | Un Profesional solo ve `Invitations` (pendientes o aceptadas) que creó él mismo o que corresponden a personas que tiene asignadas. |

---

## Estimación y Planificación

| Estimación (puntos) | Capa/s afectada/s | Fecha inicio | Fecha fin |
|---------------------|-------------------|--------------|-----------|
| 8 | Backend (Application, Infrastructure, Api, Database), Frontend (interceptor HTTP) | — | — |

**Desglose por fases:**

| Fase | Alcance | Estimación | Estado |
|------|---------|------------|--------|
| 1 | `IResourceAuthorizationService` + entidad `AccessAudit` + migración + unit tests del servicio | 3 pts | ✅ Completa |
| 2 | Aplicación a `PersonsController`, `DiagnosesController`, `ReportsController`, `InvitationsController`, `UsersController`, `AuthController` | 3 pts | ✅ Completa |
| 3 | Tests de integración (matriz rol × entidad × acción) | 1 pt | ✅ Completa |
| 4 | Interceptor HTTP frontend + manejo de `403` en UI | 1 pt | ✅ Completa |

---

## Definition of Done

- [x] Código implementado en `InclusiON.Server` siguiendo Clean Architecture (servicio en Application, implementación en Infrastructure, inyectado en handlers vía DI reflection).
- [x] Migración EF Core creada y aplicada para la tabla `AccessAudit` (`20260418062012_ExtendAccessAuditResources`).
- [x] Tests unitarios happy-path para los casos canónicos (Professional con/sin asignación, GlobalAdmin bypass, FamilyRepresentative con vínculo activo, modo Write).
- [x] Bug de seguridad en login asistido corregido: `IsAuthorizedSupervisorAsync` ahora verifica `CanSuperviseLogin` sobre la persona específica, no solo que el supervisor sea profesional/familiar del sistema.
- [x] Tests de integración verdes para los 5 endpoints críticos listados en CA-15 (23 tests — `ResourceAuthorizationIntegrationTests`).
- [x] `403` / `404` manejado en `authInterceptor` del frontend: toast `"No tenés permiso para acceder a este recurso"` + redirección al dashboard del rol vía `RoleRoutes`.
- [x] Tests unitarios con cobertura de la matriz completa rol × entidad × acción (≥ 24 casos) — cubierto por 23 tests de integración en `ResourceAuthorizationIntegrationTests` + 5 unit tests en `ResourceAuthorizationServiceTests`.
- [x] Entrada actualizada en `HU_ESTADO.md` con estado ✅ y notas de implementación.
- [x] Decisión `403` vs `404` documentada en `References/REF-autenticacion.md` (sección "Autorización por Recurso — Política de Códigos de Respuesta").
- [x] Actualizado `Process/02-gestion-roles-permisos.md` con sección "Capa 3 — Autorización por Recurso".
- [x] Actualizado `diccionario-datos.md` con la entidad `AccessAudit` (atributos completos + notas de retención).
- [ ] Revisión por pares (code review) completada.
- [ ] Validación manual QA cubriendo: profesional intentando ver persona no asignada, familiar intentando login asistido de persona ajena, admin institucional intentando ver recurso fuera de scope.

---

## Notas y Observaciones

### Contexto

Hoy la autorización se resuelve con dos capas: **roles + permisos** (claims JWT, policies `[Authorize]`) y **filtro institucional** en listados (claim `institutionId`). Falta la tercera capa: **validación de propiedad del recurso** al acceder por ID. Un profesional con `persons:read` puede hoy invocar `GET /api/persons/{id}` para cualquier persona de su institución, aun sin asignación. Un familiar con token válido podría (conociendo el ID) acceder a otra persona.

Esta HU cubre esa tercera capa. La motivación legal es la Ley 25.326 Art. 8 (datos sensibles de salud) que exige que el acceso esté limitado a quien tenga "relación jurídica" directa con el titular — más estricto que el filtro institucional.

### Entidades en scope (sensibles)

`Person`, `PersonSkillProfile`, `Diagnosis`, `Report`, `ActivityResponse`, `PersonRoadmap` (+ áreas + actividades), `ActivityAssignment`, `Invitation`, `User` (consulta de terceros), `Message` (cuando BE-15 se implemente).

### Fuera de scope

Catálogos (lectura pública autenticada), actividades estándar (lectura para profesionales aprobados), Instituciones (ya resuelto por filtro institucional), usuarios propios (un usuario siempre puede ver su propio perfil).

### Tablas de vínculo (fuentes de verdad)

| Rol | Tabla | Campo que vincula |
|-----|-------|-------------------|
| Profesional | `ProfessionalAssignments` | `ProfessionalId` + `PersonId` + `IsActive` |
| Familiar | `PersonRepresentatives` (N:M) vía `FamilyRepresentatives.UserId` | `PersonId` + `RepresentativeId` + `IsActive` (+ `CanSuperviseLogin` para login asistido) |
| Admin Institucional | `AdminInstitutions` | `AdminUserId` + `InstitutionId` |

**Modelo confirmado (2026-04-17):** la cadena es `User → FamilyRepresentative (1:1 vía UserId) → PersonRepresentative (N:M) → PersonWithDisability`. No hay que crear nada nuevo. El flag `CanSuperviseLogin` ya modela el permiso granular para login asistido, y `HasInformedConsent` + `ConsentDate` cubren consentimiento informado.

**Relación N:M:** una persona puede tener varios representantes activos (Madre, Padre, Tutor, Abuelo) y un representante puede tener varias personas a cargo. El índice único es `(PersonId, RepresentativeId)`. El flag `IsPrimary` distingue al representante principal.

### Endpoints afectados

| Controller | Endpoints con cambio |
|------------|---------------------|
| `PersonsController` | `GET /{id}`, `PUT /{id}`, `DELETE /{id}`, `GET` (listado con filtro) |
| `DiagnosesController` | `GET /persons/{id}/diagnoses`, `GET /diagnoses/{id}`, `POST`, `PUT` |
| `ReportsController` | `GET /{id}`, `GET`, `POST`, `PUT`, `PATCH submit/approve/reject`, `GET /family` |
| `PersonSkillProfileController` | todos |
| `AssignmentsController` | consultar asignaciones de otros profesionales requiere vínculo |
| `UsersController` | `GET /users/{id}` de terceros (propio siempre permitido) |
| `InvitationsController` | listar/consultar invitaciones — solo propias o de personas asignadas |
| `RoadmapController` (futuro BE-09) | todos |
| `ResponsesController` (futuro BE-11) | todos |

### Diseño técnico propuesto

**Interfaz:**

```csharp
public interface IResourceAuthorizationService
{
    Task<bool> CanAccessPersonAsync(Guid userId, Guid personId, AccessMode mode, CancellationToken ct);
    Task<bool> CanAccessReportAsync(Guid userId, Guid reportId, AccessMode mode, CancellationToken ct);
    Task<bool> CanAccessDiagnosisAsync(Guid userId, Guid diagnosisId, AccessMode mode, CancellationToken ct);
    Task<bool> CanAccessResponseAsync(Guid userId, Guid responseId, AccessMode mode, CancellationToken ct);
    Task<IReadOnlyList<Guid>> GetAccessiblePersonIdsAsync(Guid userId, CancellationToken ct);
}

public enum AccessMode { Read, Write }
```

**Estrategia por rol:**
- Resuelve el rol desde `ClaimsPrincipal` inyectado
- Salta todo si `isGlobalAdmin == true` (pero audita)
- Admin Institucional: valida `InstitutionId` del recurso vs claim
- Profesional: consulta `ProfessionalAssignments`
- Familiar: consulta `FamilyAssignments`

**Uso en handlers:**

```csharp
public async Task<ApiResponse<PersonDto>> Handle(GetPersonByIdQuery query, CancellationToken ct)
{
    if (!await _authz.CanAccessPersonAsync(_currentUser.Id, query.PersonId, AccessMode.Read, ct))
        return ApiResponse<PersonDto>.Forbidden();
    // ...
}
```

**Auditoría:** entidad `AccessAudit` + `IAccessAuditLogger` + migración. Escritura write-behind (fire-and-forget con canal en memoria) para no impactar latencia.

### Decisiones tomadas (2026-04-17)

1. ✅ **`403` vs `404`:** `404` para Familia/Estudiante (oculta existencia), `403` para Profesional/Admin (feedback claro). Ver CA-17.
2. ✅ **Tabla de vínculo familia-persona:** confirmada como `PersonRepresentatives` (ya existe, no hay que crearla).
3. ✅ **Scope ampliado:** se incluyen `Invitations` y `UsersController` (ver CA-18 y CA-19).
4. ✅ **`AccessAudit` en Fase 1:** migración combinada con el servicio, no se pospone.

### Decisiones pendientes

1. **Evaluar atributo declarativo** `[ResourceAuthorize(typeof(Person), "id")]` como alternativa a la invocación imperativa desde el handler.
2. **Retención de `AccessAudit`:** ¿cuánto tiempo se guarda? Propuesta inicial: 2 años para accesos a datos clínicos, conforme Ley 25.326.
3. **Política de notificaciones multi-representante** (fuera de scope IN-172, pero surge del análisis): ¿notificar a todos los representantes activos o solo al `IsPrimary`? Reservar para HU de notificaciones.

### Relación con otras iniciativas de seguridad

Esta HU es la **primera parte** del hardening de seguridad discutido el 2026-04-17. Las siguientes partes se abrirán como HUs separadas:
- Cifrado a nivel columna para diagnósticos y notas clínicas
- Rate-limiting en endpoints de login (especialmente PIN — 10k combinaciones)
- Rehash de PIN con Argon2id + salt único
- Registro de consentimiento versionado (especialmente para menores)
- Endpoint de derecho al olvido (borrado/anonimización)

### Dependencias

- No bloquea ningún sprint en curso; se aplica incrementalmente
- **Recomendado antes** de BE-11 (Respuestas) y BE-15 (Mensajería) para no crear más endpoints sin el patrón aplicado

### Jira

- ID Jira: **IN-172**
- Épica: Seguridad (transversal)
