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
| [Process/](./Process/) | Documentación de procesos del sistema con diagramas Mermaid | Todo el equipo |

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
