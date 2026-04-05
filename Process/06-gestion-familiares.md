# Proceso 06 — Gestión de Familiares

**Área:** Gestión de Usuarios

## Descripción
Proceso de alta, edición y desactivación de los representantes familiares en la plataforma. Los familiares son los acompañantes de la persona con discapacidad: monitorean su progreso, reciben reportes y se comunican con los profesionales. Pueden darse de alta por dos vías: CRUD directo del admin o registro vía invitación.

## Participantes
- **Admin Global** — CRUD completo de familiares, vinculación/desvinculación
- **Admin Institucional** — CRUD dentro de su institución
- **Profesional** — Invita familiares vía email (ver Proceso 07), vinculación/desvinculación de familiares a sus personas asignadas

## Pasos del proceso

### 1. Alta de Familiar (vía Admin)
El admin crea al familiar con datos personales (nombre, apellido, email, documento, teléfono, relación). La vinculación con una persona con discapacidad se realiza como paso separado (ver Proceso 08 — Asignación de Profesionales).
- **Endpoint:** `POST /api/family`
- **Campos:** FirstName, LastName, Email, DocumentNumber (opcional), Phone (opcional), Relationship (opcional)
- **Nota:** El request NO incluye PersonId; la vinculación persona-familiar se establece por separado
- **Frontend:** `/admin/family` (formulario)

### 2. Alta de Familiar (vía Invitación)
El familiar se registra desde una ruta pública usando un código de invitación generado por un profesional. Se crea automáticamente el usuario, el perfil de familiar y la vinculación con la persona.
- **Validar código:** `GET /api/invitations/{code}`
- **Aceptar:** `POST /api/invitations/{code}/accept`
- **Frontend:** `/invite/:code` (ruta pública)
- Ver detalle completo en **Proceso 07 — Gestión de Invitaciones**

### 3. Consulta de Familiares
Listado paginado con búsqueda. Filtrado por institución para admins institucionales. El detalle incluye las personas vinculadas vía `PersonRepresentative`.
- **Listado:** `GET /api/family` (paginado)
- **Detalle:** `GET /api/family/{id}` — incluye `linkedPersons` con nombre, tipo de discapacidad y si es representante primario
- **Frontend:** `/admin/family` (DataTable)

### 4. Edición de Familiar
El admin modifica los datos personales del familiar.
- **Endpoint:** `PUT /api/family/{id}`

### 5. Desactivación de Familiar
El admin desactiva al familiar (soft-delete).
- **Endpoint:** `PUT /api/family/{id}/deactivate`

### 6. Vinculación de Familiar a Persona (HU IN-151)
El profesional o admin puede vincular familiares registrados a una persona con discapacidad. Esta funcionalidad permite gestionar los vínculos familiar-persona independientemente de las invitaciones.

#### 6.1 Listar Familiares Disponibles
Obtiene lista de familiares activos que pueden ser vinculados a una persona. **Excluye familias ya vinculadas activamente** a la persona y marca las que fueron previamente desvinculadas.
- **Endpoint:** `GET /api/family/available?search={nombre}&personId={personId}`
- **Permiso:** `family:read` (admin), `family:link` (profesional)
- **Query params:**
  - `search` (opcional) — filtro por nombre
  - `personId` (opcional) — ID de la persona para filtrar ya vinculados y marcar previamente desvinculados
- **Respuesta:** Lista de `FamilyResponse` con campo adicional `wasPreviouslyLinked` (boolean) que indica si el familiar fue desvinculado previamente de esa persona.

#### 6.2 Listar Familiares Vinculados a una Persona
Obtiene los familiares actualmente vinculados a una persona.
- **Endpoint:** `GET /api/persons/{personId}/representatives`
- **Permiso:** `persons:read`
- **Respuesta:** Lista de `PersonRepresentativeResponse` con nombre, relación, es principal, estado

#### 6.3 Vincular Familiar a Persona
Vincula un familiar activo a una persona con discapacidad. Si el familiar fue previamente desvinculado de esa persona, **se reactiva el registro existente** (UPDATE) en lugar de crear uno nuevo.
- **Endpoint:** `POST /api/family/{familyId}/link/{personId}`
- **Permiso:** `family:link`
- **Request Body:**
  ```json
  {
    "relationship": "Madre",
    "isPrimary": true
  }
  ```
- **Validaciones:**
  - El familiar debe estar activo (`User.IsActive = true` y `FamilyRepresentative.Status = Active`)
  - Solo una "Madre" por persona
  - Solo un "Padre" por persona
  - Si `isPrimary = true`, se quitan los otros familiares principales
- **Comportamiento de re-vinculación:**
  - Si existe un `PersonRepresentative` inactivo para el par `(personId, familyId)`, se reactiva con `IsActive = true`, se limpian `EndedAt` y `UnlinkObservation`, y se actualiza `UpdatedAt`.
  - Si no existe registro previo, se crea uno nuevo con `CreatedAt = DateTime.UtcNow`.
  - En ambos casos se registra un evento en `PersonRepresentativeHistory` con `ChangeType = Linked`.
- **Respuestas de error:**
  - `404` — Familiar no encontrado
  - `409` — Familiar ya está vinculado activamente a esta persona
  - `409` — Ya existe un familiar con la misma relación (Madre/Padre)
  - `500` — Error interno

#### 6.3.1 Endpoint Profesional
El profesional tiene sus propios endpoints con permisos específicos:
- **Listar disponibles:** `GET /api/family/professional/available?search={nombre}&personId={personId}`
- **Vincular:** `POST /api/family/professional/link/{familyId}/{personId}`
- **Desvincular:** `DELETE /api/family/professional/unlink/{familyId}/{personId}`
- **Permisos requeridos:** `family:link` y `family:unlink`

#### 6.4 Desvincular Familiar de Persona
Desvincula un familiar de una persona. Requiere motivo obligatorio.
- **Endpoint:** `DELETE /api/family/{familyId}/unlink/{personId}`
- **Permiso:** `family:unlink`
- **Request Body:**
  ```json
  {
    "observation": "Motivo de la desvinculación"
  }
  ```
- **Comportamiento:** No se borra el registro de `PersonRepresentatives`. Se establece `IsActive = false`, `EndedAt = DateTime.UtcNow`, y se guarda `UnlinkObservation`. Se registra evento en `PersonRepresentativeHistory` con `ChangeType = Unlinked`.

#### 6.5 Historial de Cambios de Vinculación
Obtiene el historial de cambios de vinculación para un familiar.
- **Endpoint:** `GET /api/family/{familyId}/link-history`
- **Permiso:** `family:read`

### 7. Estados del Familiar
- **Active** — Familiar activo en el sistema
- **Terminated** — Familiar dado de baja

### 8. Historial de Estados del Familiar
Registra cada cambio de estado del familiar.
- **Endpoint:** `GET /api/family/{familyId}/status-history`
- **Permiso:** `family:read`

### 9. Modelo de datos — PersonRepresentative

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `Guid` | PK |
| `PersonId` | `Guid` | FK a PersonsWithDisability |
| `RepresentativeId` | `Guid` | FK a FamilyRepresentatives |
| `Relationship` | `string?` | Parentesco (Madre, Padre, Hermano/a, Abuelo/a, Tío/a, Tutor Legal, Otro) |
| `IsPrimary` | `bool` | Si es el representante principal |
| `HasInformedConsent` | `bool` | Si firmó consentimiento informado |
| `CanSuperviseLogin` | `bool` | Si puede supervisar el login |
| `IsActive` | `bool` | Estado del vínculo |
| `CreatedAt` | `DateTime` | Fecha de creación del vínculo |
| `UpdatedAt` | `DateTime?` | Fecha de última modificación |
| `EndedAt` | `DateTime?` | Fecha de desvinculación (null si activo) |
| `UnlinkObservation` | `string?` | Motivo de desvinculación |

### 10. Modelo de datos — PersonRepresentativeHistory

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `Guid` | PK |
| `PersonRepresentativeId` | `Guid` | FK al registro vinculado |
| `PersonId` | `Guid` | Persona con discapacidad |
| `RepresentativeId` | `Guid` | Familiar |
| `ChangeType` | `enum` | `Linked` / `Unlinked` |
| `Relationship` | `string?` | Parentesco al momento del cambio |
| `WasPrimary` | `bool` | Si era principal |
| `Observation` | `string?` | Observación del cambio |
| `ChangedByUserId` | `Guid?` | Usuario que realizó el cambio |
| `CreatedAt` | `DateTime` | Fecha del evento |

## Diagramas de flujo

```mermaid
flowchart TD
    ADMIN[Admin] -->|POST /api/family| ALTA_D[Alta directa]
    PROF[Profesional] -->|POST /api/invitations| INV[Invitación email]
    INV -->|/invite/:code| ALTA_I[Auto-registro]

    ALTA_D --> FAM[Familiar]
    ALTA_I --> FAM

    ADMIN -->|GET /api/family| LIST[Listado paginado]
    ADMIN -->|PUT /api/family/id| EDIT[Editar datos]
    ADMIN -->|PUT /api/family/id/deactivate| DEACT[Desactivar]

    PROF -->|GET /api/family/professional/available| DISP[Familiares disponibles]
    PROF -->|POST /api/family/professional/link/{id}/{personId}| VINCULAR[Vincular familiar]
    PROF -->|DELETE /api/family/professional/unlink/{id}/{personId}| DESVINCULAR[Desvincular familiar]
    ADMIN --> VINCULAR
    ADMIN --> DESVINCULAR

    FAM -->|PersonRepresentative| PCD[Persona con Discapacidad]
```

```mermaid
flowchart TD
    PCD[Persona con Discapacidad] -->|GET /api/persons/{id}/representatives| REPRESENTATIVOS[Familiares vinculados]
    
    REPRESENTATIVOS -->|Ver| LIST_VINC[Mostrar listado DataTable]
    REPRESENTATIVOS -->|Vincular| BOTON_VINC[Botón Vincular en header]
    REPRESENTATIVOS -->|Desvincular| BOTON_DESVINC[Menú hamburguesa → Desvincular]

    BOTON_VINC -->|Abrir modal| MODAL_LINK[Modal seleccionar familiar]
    MODAL_LINK -->|Buscar| SEARCH[Buscar por nombre]
    MODAL_LINK -->|Seleccionar| SELECT[Seleccionar familiar]
    SELECT -->|Si fue desvinculado antes| CONFIRM[Modal confirmar revinculación]
    SELECT -->|Si es nuevo| FILL[Completar relación y principal]
    CONFIRM -->|Confirmar| FILL
    FILL -->|Vincular| API_LINK[POST /api/family/professional/link/{id}/{personId}]
    API_LINK -->|Reactiva si existía inactivo| UPDATE[UPDATE PersonRepresentative]
    API_LINK -->|Si no existía| CREATE[INSERT PersonRepresentative]
    API_LINK -->|Siempre| HIST_LINK[INSERT PersonRepresentativeHistory Linked]
    
    BOTON_DESVINC -->|Ingresar motivo| MODAL_UNLINK[Modal desvincular]
    MODAL_UNLINK -->|Confirmar| API_UNLINK[DELETE /api/family/professional/unlink/{id}/{personId}]
    API_UNLINK -->|Soft-delete| SOFT[IsActive=false, EndedAt=now, UnlinkObservation]
    API_UNLINK -->|Siempre| HIST_UNLINK[INSERT PersonRepresentativeHistory Unlinked]
```

## Frontend — Componentes

| Componente | Ubicación | Descripción |
|---|---|---|
| `ProfessionalFamilyTabComponent` | `views/professional/person-detail/components/family-tab.component.ts` | Tab de familiares en detalle de persona (profesional) |
| `FamilyService` | `services/family.service.ts` | Servicio con métodos profesionales: `getAvailableFamiliesForProfessional`, `linkFamilyToPersonAsProfessional`, `unlinkFamilyFromPersonAsProfessional` |
| `DataTableComponent` | `shared/components/data-table/` | Tabla reutilizable usada para mostrar familiares vinculados |

### Flujo de UI del profesional
1. El profesional abre el detalle de una persona asignada.
2. Selecciona el tab "Familiares".
3. Ve un DataTable con los familiares vinculados (o "Sin registros" si no hay).
4. Botón "+ Vincular" en el header abre modal de selección.
5. El modal muestra lista de familiares disponibles (excluye ya vinculados, marca desvinculados con badge amarillo).
6. Al seleccionar un familiar, se muestran campos de relación (dropdown) y checkbox "Familiar principal".
7. Si el familiar fue previamente desvinculado, aparece alerta y se solicita confirmación.
8. Al confirmar, se llama al endpoint. Los errores del backend se muestran en toast.
