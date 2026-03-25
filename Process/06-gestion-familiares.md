# Proceso 06 — Gestión de Familiares

**Área:** Gestión de Usuarios

## Descripción
Proceso de alta, edición y desactivación de los representantes familiares en la plataforma. Los familiares son los acompañantes de la persona con discapacidad: monitorean su progreso, reciben reportes y se comunican con los profesionales. Pueden darse de alta por dos vías: CRUD directo del admin o registro vía invitación.

## Participantes
- **Admin Global** — CRUD completo de familiares
- **Admin Institucional** — CRUD dentro de su institución
- **Profesional** — Invita familiares vía email (ver Proceso 07)

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

## Diagrama de flujo

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

    FAM -->|PersonRepresentative| PCD[Persona con Discapacidad]
```


