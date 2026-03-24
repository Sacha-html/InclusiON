# InclusiON — Documentación del Proyecto

**Institución Cervantes — Analista de Sistemas — Prácticas Profesionalizantes 2025/2026**

---

## Qué es InclusiON

Plataforma web de inclusión educativa para personas con discapacidad. Permite a profesionales crear actividades adaptativas, asignarlas a estudiantes, monitorear su progreso y ajustar la dificultad automáticamente.

**Stack:** Angular 20 (frontend) + .NET 8 con Clean Architecture (backend) + SQL Server

---

## Índice de Documentos

| Documento | Descripción | Audiencia |
|-----------|-------------|-----------|
| [HU_ESTADO.md](./HU_ESTADO.md) | Historias de usuario con estado actual (hecho/pendiente) | Todo el equipo |
| [Features/InclusiON_HUs_BEyFE.md](./Features/InclusiON_HUs_BEyFE.md) | Especificación completa de cada HU (endpoints, criterios, mocks) | Desarrolladores |
| [Features/CIF_ACCESIBILIDAD_ANGULAR.md](./Features/CIF_ACCESIBILIDAD_ANGULAR.md) | Referencia de accesibilidad CIF/ICF para Angular | Frontend devs |
| [CLAUDE_BACKEND.md](./CLAUDE_BACKEND.md) | Instrucciones para agentes/Claude trabajando en el backend .NET | Agentes AI + Backend devs |
| [CLAUDE_FRONTEND.md](./CLAUDE_FRONTEND.md) | Instrucciones para agentes/Claude trabajando en el frontend Angular | Agentes AI + Frontend devs |
| [Features/MDA_Especificacion_Tecnica.md](./Features/MDA_Especificacion_Tecnica.md) | Especificación del Motor de Dificultad Adaptativa | Todo el equipo |
| [Features/integracion-semantic-search.md](./Features/integracion-semantic-search.md) | Plan de integración de búsqueda semántica ONNX | Backend devs |
| [ARQUITECTURA.md](./ARQUITECTURA.md) | Visión general de la arquitectura y decisiones técnicas | Todo el equipo |
| [Process/](./Process/) | Procesos del sistema de información | Todo el equipo |
| [References/](./References/) | Documentación de capacidades transversales (accesibilidad, autenticación) | Todo el equipo |

---

## Procesos del Sistema de Información

**[00 — Mapa Global de Procesos](./Process/00-mapa-global-procesos.md)** — Visión general, fases del DOCX, relación entre procesos y estado por fase.

### Configuración del Sistema
| # | Proceso | Estado |
|---|---------|--------|
| 01 | [Gestión de Instituciones Educativas](./Process/01-gestion-instituciones.md) | ✅ Implementado |
| 02 | [Gestión de Roles y Permisos](./Process/02-gestion-roles-permisos.md) | ✅ Implementado |
| 03 | [Gestión de Catálogos](./Process/03-gestion-catalogos.md) | ✅ Implementado |

### Gestión de Usuarios
| # | Proceso | Estado |
|---|---------|--------|
| 04 | [Gestión de Profesionales](./Process/04-gestion-profesionales.md) | ✅ Implementado |
| 05 | [Gestión de Personas con Discapacidad](./Process/05-gestion-personas.md) | ✅ Implementado |
| 06 | [Gestión de Familiares](./Process/06-gestion-familiares.md) | ✅ Implementado |
| 07 | [Gestión de Invitaciones](./Process/07-gestion-invitaciones.md) | ✅ Implementado |

### Asignaciones y Vinculaciones
| # | Proceso | Estado |
|---|---------|--------|
| 08 | [Asignación de Profesionales](./Process/08-asignacion-profesionales.md) | ✅ Implementado |

### Evaluación y Planificación
| # | Proceso | Estado |
|---|---------|--------|
| 09 | [Evaluación y Diagnóstico](./Process/09-evaluacion-diagnostico.md) | ⏳ Parcial |
| 10 | [Gestión de Actividades](./Process/10-gestion-actividades.md) | ⏳ Pendiente |
| 11 | [Gestión del Plan de Trabajo (Roadmap)](./Process/11-gestion-plan-trabajo.md) | ⏳ Pendiente |

### Ejecución
| # | Proceso | Estado |
|---|---------|--------|
| 12 | [Resolución de Actividades](./Process/12-resolucion-actividades.md) | ⏳ Pendiente |
| 13 | [Dificultad Adaptativa (MDA)](./Process/13-dificultad-adaptativa.md) | ⏳ Pendiente |

### Monitoreo y Reportes
| # | Proceso | Estado |
|---|---------|--------|
| 14 | [Seguimiento de Avances](./Process/14-seguimiento-avances.md) | ⏳ Parcial |
| 15 | [Generación de Informes](./Process/15-generacion-informes.md) | ⏳ Pendiente |

### Comunicación
| # | Proceso | Estado |
|---|---------|--------|
| 16 | [Comunicación entre Actores](./Process/16-comunicacion-actores.md) | ⏳ Parcial |

### Referencias Transversales
```
References/
  REF-accesibilidad.md            Sistema de Accesibilidad (7 perfiles × 2 modos)
  REF-autenticacion.md            Autenticación multi-método y JWT
```

---

## Cómo Empezar

### Prerequisitos

- Node.js 20+ y npm
- .NET 8 SDK
- SQL Server (LocalDB o instancia completa)
- Git

### Levantar el Backend

```bash
cd InclusiON.Server
dotnet restore
dotnet run --project InclusiON.Api   # Migra la DB automáticamente
# API en https://localhost:7xxx
```

### Levantar el Frontend

```bash
cd InclusiON.Client
npm install
npm start
# App en http://localhost:4200
```

---

## Estructura del Repositorio

```
InclusiON.Client/          ← Frontend Angular 20
InclusiON.Server/          ← Backend .NET 8
├── InclusiON.Api/         ← Controllers, Program.cs
├── InclusiON.Application/ ← CQRS handlers, interfaces
├── InclusiON.Infrastructure/ ← JWT, repos, servicios
├── InclusiON.Data/        ← EF Core, migraciones
├── InclusiON.Domain/      ← Entidades del dominio
├── InclusiON.DTOs/        ← Request/Response DTOs
├── InclusiON.Shared/      ← Constantes, recursos
└── InclusiON.SemanticSearch/ ← Embeddings ONNX
Documentacion/             ← Este directorio
```

---

## Convención de Commits

Usamos la convención de Angular: `<tipo>(<alcance>): <descripción>`

Tipos: `feat`, `fix`, `perf`, `docs`, `chore`, `style`, `refactor`, `test`

Ejemplos:
- `feat(auth): agregar login visual con PIN`
- `fix(persons): corregir paginación en listado`
- `docs(mda): actualizar especificación técnica`
