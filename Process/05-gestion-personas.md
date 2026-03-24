# Proceso 05 — Gestión de Personas con Discapacidad

**Área:** Gestión de Usuarios

## Descripción
Proceso de alta, edición y gestión de las personas con discapacidad en la plataforma. La persona es el destinatario central del sistema: recibe planes de trabajo, realiza actividades y su progreso es monitoreado por profesionales y familiares. Se configura su tipo de discapacidad, nivel de autonomía, perfil de accesibilidad y método de login adaptativo.

## Participantes
- **Admin Global** — CRUD completo
- **Admin Institucional** — CRUD dentro de su institución
- **Profesional** — Edita datos funcionales de sus personas asignadas

## Pasos del proceso

### 1. Alta de Persona ✅ Implementado
El admin registra la persona con datos personales, tipo de discapacidad (del catálogo), nivel de autonomía y perfil de accesibilidad.
- **Endpoint:** `POST /api/persons`
- **Frontend:** `/admin/persons` (formulario)

### 2. Consulta de Personas ✅ Implementado
Listado paginado con búsqueda. Admins institucionales ven solo personas de sus instituciones.
- **Listado:** `GET /api/persons` (paginado)
- **Detalle:** `GET /api/persons/{id}`
- **Frontend admin:** `/admin/persons` (DataTable)
- **Frontend profesional:** `/pro/persons/{id}` (detalle con edición inline)

### 3. Edición de Persona ✅ Implementado
El admin o profesional modifica datos personales y funcionales: tipo de discapacidad, nivel de autonomía, perfil de accesibilidad.
- **Endpoint:** `PUT /api/persons/{id}`
- **Frontend:** Edición inline en detalle de persona

### 4. Configuración del Método de Login ✅ Implementado
Se asigna el método de login según el nivel de autonomía de la persona (Standard, PIN, Assisted, Family).
- **Endpoint:** `PUT /api/persons/{id}/login-method`
- **Métodos disponibles:** `GET /api/catalogs/login-methods`

## Diagrama de flujo

```mermaid
flowchart TD
    ADMIN[Admin] -->|POST /api/persons| ALTA[Alta de Persona]
    ALTA -->|Selecciona del catálogo| CAT[Tipo discapacidad + Nivel autonomía]

    ADMIN -->|GET /api/persons| LIST[Listado paginado]
    ADMIN -->|PUT /api/persons/id| EDIT[Editar datos]

    PROF[Profesional] -->|PUT /api/persons/id| EDIT_F[Editar perfil funcional]
    PROF -->|PUT /api/persons/id/login-method| LOGIN[Configurar método de login]
    LOGIN --> METHOD{Método}
    METHOD -->|Standard| STD[Contraseña visual]
    METHOD -->|PIN| PIN[PIN 4 dígitos]
    METHOD -->|Assisted| ASS[Login asistido]
    METHOD -->|Family| FAM[Contraseña familiar]
```

## Estado resumen

| Paso | Estado |
|------|--------|
| Alta de persona | ✅ Implementado |
| Consulta de personas | ✅ Implementado |
| Edición de persona | ✅ Implementado |
| Configuración de login | ✅ Implementado |
