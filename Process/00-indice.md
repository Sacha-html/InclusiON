# Índice de Procesos — InclusiON

**Plataforma:** InclusiON — Inclusión Educativa para Personas con Discapacidad  
**Arquitectura:** Angular + .NET Clean Architecture + PostgreSQL

## Procesos documentados

| # | Proceso | Área | BPMN |
|---|---------|------|------|
| [01](./01-gestion-instituciones.md) | Gestión de Instituciones Educativas | Configuración | [diagram](./BPMN/01-gestion-instituciones.drawio) |
| [02](./02-autenticacion-registro.md) | Autenticación y Registro | Seguridad | [diagram](./BPMN/02-autenticacion-registro.drawio) |
| [03](./03-gestion-catalogos.md) | Gestión de Catálogos | Configuración | [diagram](./BPMN/03-gestion-catalogos.drawio) |
| [04](./04-gestion-personas.md) | Gestión de Personas con Discapacidad | Personas | [diagram](./BPMN/04-gestion-personas.drawio) |
| [05](./05-gestion-profesionales.md) | Gestión de Profesionales | Personas | [diagram](./BPMN/05-gestion-profesionales.drawio) |
| [06](./06-gestion-familiares.md) | Gestión de Familiares / Representantes | Personas | [diagram](./BPMN/06-gestion-familiares.drawio) |
| [07](./07-gestion-invitaciones.md) | Gestión de Invitaciones | Usuarios | [diagram](./BPMN/07-gestion-invitaciones.drawio) |
| [08](./08-asignacion-profesionales.md) | Asignación de Profesionales | Asignaciones | [diagram](./BPMN/08-asignacion-profesionales.drawio) |
| [09](./09-gestion-actividades.md) | Gestión de Actividades (Catálogo) | Actividades | [diagram](./BPMN/09-gestion-actividades.drawio) |
| **[10](./10-proceso-core.md)** | **PROCESO CORE: Ejecución de Actividades** | **Core** | **[diagram](./BPMN/10-proceso-core.drawio)** |
| [11](./11-roadmap-motor-adaptativo.md) | Roadmap y Motor Adaptativo | Progreso | [diagram](./BPMN/11-roadmap-motor-adaptativo.drawio) |
| [12](./12-reportes.md) | Reportes de Progreso | Reportes | [diagram](./BPMN/12-reportes.drawio) |
| [13](./13-mensajeria.md) | Mensajería Interna | Comunicación | [diagram](./BPMN/13-mensajeria.drawio) |

## Documentación complementaria (sin diagrama BPMN)

| Archivo | Contenido |
|---------|-----------|
| [02-gestion-roles-permisos.md](./02-gestion-roles-permisos.md) | Roles del sistema, permisos por módulo, admins institucionales, autorización por recurso |
| [14-seguimiento-avances.md](./14-seguimiento-avances.md) | Dashboard profesional, Mi Aula, progreso familiar, vistas de seguimiento |
| [15-generacion-informes.md](./15-generacion-informes.md) | Ciclo completo de reportes con detalle de estados, modal post-creación, exportación PDF |
| [00-mapa-global-procesos.md](./00-mapa-global-procesos.md) | Mapa visual de las 8 áreas funcionales del sistema |
| [09b-evaluacion-diagnostico.md](./09b-evaluacion-diagnostico.md) | Evaluación clínica y diagnóstico de personas (proceso paralelo a gestión de actividades) |
| [16-comunicacion-actores.md](./16-comunicacion-actores.md) | Comunicación entre actores del sistema |
| [17-gestion-usuarios.md](./17-gestion-usuarios.md) | Administración de cuentas de usuario |
| [18-onboarding.md](./18-onboarding.md) | Onboarding de nuevos usuarios a la plataforma |
| [19-soporte.md](./19-soporte.md) | Soporte y ayuda dentro del sistema |

## Proceso core

El proceso central de la plataforma es la **Ejecución de Actividades** (Proceso 10): el profesional asigna actividades terapéuticas a personas con discapacidad, la persona las ejecuta mediante el portal AAC accesible, el sistema evalúa los resultados y actualiza el roadmap de progreso, y el motor adaptativo ajusta la dificultad automáticamente.

```
Profesional → crea actividad → asigna a persona
Persona (PCD) → ejecuta en AAC Player → completa
Sistema → evalúa → actualiza roadmap → motor adaptativo
Profesional → revisa resultados → genera reporte
```

## Roles del sistema

| Rol | Código | Alcance |
|-----|--------|---------|
| Admin Global | `global-admin` | Todo el sistema |
| Admin Institucional | `admin` | Su institución |
| Profesional | `professional` | Sus personas asignadas |
| Familiar | `family` | Sus personas a cargo |
| Persona (PCD) | `person` | Sus propias actividades |

## Notación BPMN

Los diagramas siguen BPMN 2.0:
- **Círculo fino** = Evento de inicio
- **Círculo grueso** = Evento de fin
- **Rectángulo redondeado** = Tarea
- **Diamante** = Gateway (decisión)
- **Flecha continua** = Flujo de secuencia
- **Flecha punteada** = Flujo de mensaje
- **Carril** = Participante / responsable
