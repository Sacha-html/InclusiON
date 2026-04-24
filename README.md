# InclusiON

**Institución Cervantes — Analista de Sistemas — Prácticas Profesionalizantes 2025/2026**

---

## Qué es InclusiON

InclusiON es una plataforma web pensada para fortalecer la inclusión educativa de personas con discapacidad. Conecta a **profesionales** (docentes, terapeutas, psicólogos), **personas con discapacidad**, **familias** e **instituciones educativas** en un entorno digital accesible.

La plataforma permite:
- Diseñar **actividades educativas interactivas** adaptadas a cada persona
- Armar **planes de trabajo personalizados** con progresión automática
- **Monitorear el progreso** con dashboards, gráficos y reportes
- **Ajustar automáticamente la dificultad** según el rendimiento del estudiante
- Facilitar la **comunicación** entre profesionales y familias
- Garantizar la **accesibilidad** con 7 perfiles visuales y 4 métodos de login adaptativo

---

## Quiénes participan

| Actor | Qué hace en la plataforma |
|-------|---------------------------|
| **Administrador** | Configura el sistema: crea instituciones, gestiona roles, carga catálogos, da de alta usuarios |
| **Profesional** | Evalúa personas, crea actividades, arma planes de trabajo, monitorea progreso, genera reportes |
| **Persona con discapacidad** | Realiza actividades interactivas desde su portal, ve su progreso como un camino visual |
| **Familia** | Se registra por invitación, consulta el progreso de su familiar, se comunica con el profesional |

---

## Estructura de este repositorio

Este repositorio contiene toda la documentación del proyecto InclusiON. Está organizado así:

```
InclusiON.Documents/
│
├── README.md                    ← Este archivo: guía general del proyecto
│
├── Docs/                        ← Documentos del proyecto
│   ├── InclusiON Final (...).docx   Documento original entregado
│   ├── proyecto-final-actualizado.md
│   ├── story-map.md                 Story map (backbone → tareas → HUs, MVP marcado)
│   └── ui-patterns.md
│
├── State/                       ← Estado de avance del proyecto
│   ├── checklist-procesos.md        Qué está hecho y qué falta, por proceso
│   └── progreso-hu.md               Detalle por HU con código Jira y estado
│
├── Process/                     ← Procesos del sistema de información (19 procesos)
│   ├── 00-mapa-global-procesos.md    Mapa global con fases y relaciones
│   ├── 01-gestion-instituciones.md   Alta y edición de instituciones
│   ├── 02-gestion-roles-permisos.md  Roles, permisos y admins institucionales
│   ├── 03-gestion-catalogos.md       Tablas de referencia del sistema
│   ├── 04-gestion-profesionales.md   Alta y gestión de profesionales
│   ├── 05-gestion-personas.md        Alta y gestión de personas con discapacidad
│   ├── 06-gestion-familiares.md      Alta de familiares (directa o por invitación)
│   ├── 07-gestion-invitaciones.md    Invitaciones por email para familiares
│   ├── 08-asignacion-profesionales.md Vinculaciones entre actores
│   ├── 09-evaluacion-diagnostico.md  Evaluación y diagnóstico funcional
│   ├── 10-gestion-actividades.md     Creación de actividades con plantillas
│   ├── 11-gestion-plan-trabajo.md    Roadmap personalizado por persona
│   ├── 12-resolucion-actividades.md  Ejecución de actividades por el estudiante
│   ├── 13-dificultad-adaptativa.md   Motor de ajuste automático de dificultad
│   ├── 14-seguimiento-avances.md     Dashboard, Mi Aula y monitoreo
│   ├── 15-generacion-informes.md     Reportes formales de progreso
│   └── 16-comunicacion-actores.md    Mensajería interna y notificaciones
│
├── HU/                          ← Historias de usuario (10 HU unificadas)
│   ├── HU-01 a HU-10               Cada HU describe una funcionalidad completa
│   │                                con criterios de aceptación
│
├── References/                  ← Documentación de capacidades transversales
│   ├── REF-accesibilidad.md         7 perfiles visuales × 2 modos = 14 combinaciones
│   └── REF-autenticacion.md         5 métodos de login + gestión de sesiones
│
├── Features/                    ← Especificaciones técnicas detalladas
│   ├── InclusiON_HUs_BEyFE.md      HU técnicas con endpoints y criterios
│   ├── MDA_Especificacion_Tecnica.md Motor de Dificultad Adaptativa
│   ├── CIF_ACCESIBILIDAD_ANGULAR.md Referencia de accesibilidad CIF/ICF
│   └── integracion-semantic-search.md Búsqueda semántica con ONNX
│
├── Templates/                   ← Plantillas para documentación
│   └── InclusiON_HU_Plantilla.docx  Plantilla para nuevas HU
│
├── diagrama-contexto.md         ← Diagrama de contexto: actores, flujos y límites del sistema
├── diccionario-datos.md         ← Diccionario de datos (31 entidades del sistema)
├── der.md                       ← DER: tipos de datos, nulabilidad y responsabilidades por entidad
├── glosario.md                  ← Definición de todos los términos del dominio
├── ARQUITECTURA.md              ← Arquitectura técnica del sistema
├── HU_ESTADO.md                 ← Estado de implementación BE/FE
├── CLAUDE_BACKEND.md            ← Instrucciones para desarrollo backend
├── CLAUDE_FRONTEND.md           ← Instrucciones para desarrollo frontend
└── Test/                        ← Documentación del repositorio de tests E2E
    └── README.md                    Setup, scripts, CI y estructura de InclusiON.Testing
```

### Dónde encontrar cada cosa

| Necesito... | Ir a... |
|-------------|---------|
| Entender de qué se trata el proyecto | Este README |
| Ver el story map (backbone, tareas, HUs del MVP) | `Docs/story-map.md` |
| Ver qué está hecho y qué falta (por proceso) | `State/checklist-procesos.md` |
| Ver el detalle de avance por HU con código Jira | `State/progreso-hu.md` |
| Ver cómo funcionan los procesos del sistema | `Process/00-mapa-global-procesos.md` |
| Ver un proceso específico (ej: cómo se registra un familiar) | `Process/07-gestion-invitaciones.md` |
| Entender qué funcionalidad hace falta desarrollar | Sección "Historias de Usuario" de este README |
| Leer los criterios de aceptación de una funcionalidad | `HU/HU-XX-nombre.md` |
| Ver el DER con tipos de datos y nulabilidad | `der.md` |
| Conocer las entidades y datos que maneja el sistema | `diccionario-datos.md` |
| Ver el sistema desde afuera (actores, flujos, límites) | `diagrama-contexto.md` |
| Entender un término que no conozco | `glosario.md` |
| Consultar especificaciones técnicas de una feature | `Features/` |
| Entender la accesibilidad o la autenticación | `References/` |

---

## Cómo funciona el sistema: las 4 fases

El sistema opera en un ciclo de 4 fases que se repiten a lo largo del acompañamiento educativo de cada persona:

### Fase 1 — Configuración y Onboarding
Se prepara el sistema y se dan de alta todos los actores.

```
Admin crea institución
  → Admin registra profesional
    → Admin registra persona con discapacidad
      → Admin asigna profesional a persona
        → Profesional invita familiar por email
          → Familiar se registra desde el link
```

**Procesos involucrados:** 01 Instituciones, 02 Roles, 03 Catálogos, 04 Profesionales, 05 Personas, 06 Familiares, 07 Invitaciones, 08 Asignaciones

### Fase 2 — Evaluación y Diagnóstico
El profesional evalúa a la persona para conocer su punto de partida antes de planificar la intervención.

```
Profesional configura perfil de habilidades (qué áreas trabajar)
  → Profesional edita perfil funcional (capacidades, nivel de autonomía)
    → Profesional registra diagnóstico formal (observaciones, objetivos, estrategias)
```

**Procesos involucrados:** 09 Evaluación y Diagnóstico

### Fase 3 — Intervención y Personalización
El profesional diseña el plan de trabajo basado en la evaluación.

```
Profesional crea actividades educativas (con plantillas interactivas y pictogramas)
  → Profesional arma el roadmap por área de habilidad (secuencia de actividades)
    → Profesional configura el motor adaptativo (rangos de dificultad, umbrales)
```

**Procesos involucrados:** 10 Actividades, 11 Plan de Trabajo, 13 Dificultad Adaptativa (config)

### Fase 4 — Seguimiento y Mejora Continua
Ciclo operativo principal. El estudiante trabaja, el sistema adapta y los actores monitorean.

```
Persona ve su roadmap visual (estilo Duolingo) y elige una actividad desbloqueada
  → Persona realiza la actividad interactiva (5 tipos de juegos)
    → Sistema registra tiempos, aciertos, errores y frustración
      → Motor adaptativo evalúa rendimiento y ajusta dificultad automáticamente
        → Si supera el umbral, la siguiente actividad se desbloquea
          → Profesional monitorea desde su dashboard y "Mi Aula"
            → Profesional genera reporte formal de progreso
              → Familia consulta el progreso desde su portal
                → Profesional decide: ¿reforzar, avanzar o mantener?
                  → Vuelve a Fase 3 si necesita ajustar el plan
```

**Procesos involucrados:** 12 Resolución, 13 MDA (auto), 14 Seguimiento, 15 Informes, 16 Comunicación

---

## Los 19 procesos del sistema

Cada proceso describe **qué hace el sistema, quién lo usa y cómo funciona**. Todos incluyen diagramas de flujo y estado de implementación.

**[Ver mapa global de procesos](./Process/00-mapa-global-procesos.md)** con diagrama de relaciones entre procesos.

### Configuración del Sistema
| # | Proceso | Qué resuelve |
|---|---------|--------------|
| 01 | [Instituciones](./Process/01-gestion-instituciones.md) | Dar de alta las escuelas y centros donde trabajan los profesionales |
| 02 | [Roles y Permisos](./Process/02-gestion-roles-permisos.md) | Definir qué puede hacer cada tipo de usuario en la plataforma |
| 03 | [Catálogos](./Process/03-gestion-catalogos.md) | Cargar las listas de referencia: tipos de discapacidad, áreas de habilidad, etc. |

### Gestión de Usuarios
| # | Proceso | Qué resuelve |
|---|---------|--------------|
| 04 | [Profesionales](./Process/04-gestion-profesionales.md) | Registrar a los profesionales que van a usar la plataforma |
| 05 | [Personas](./Process/05-gestion-personas.md) | Registrar a las personas con discapacidad con su perfil funcional |
| 06 | [Familiares](./Process/06-gestion-familiares.md) | Registrar a los representantes familiares (directo o por invitación) |
| 07 | [Invitaciones](./Process/07-gestion-invitaciones.md) | Que el profesional invite al familiar por email y este se registre solo |

### Asignaciones
| # | Proceso | Qué resuelve |
|---|---------|--------------|
| 08 | [Asignaciones](./Process/08-asignacion-profesionales.md) | Vincular profesionales con instituciones, personas y áreas de habilidad |

### Evaluación y Planificación
| # | Proceso | Qué resuelve |
|---|---------|--------------|
| 09 | [Evaluación](./Process/09-evaluacion-diagnostico.md) | Que el profesional evalúe a la persona y registre su diagnóstico funcional |
| 10 | [Actividades](./Process/10-gestion-actividades.md) | Que el profesional cree actividades educativas con plantillas interactivas |
| 11 | [Plan de Trabajo](./Process/11-gestion-plan-trabajo.md) | Que el profesional arme un camino de aprendizaje secuenciado por área |

### Ejecución
| # | Proceso | Qué resuelve |
|---|---------|--------------|
| 12 | [Resolución](./Process/12-resolucion-actividades.md) | Que la persona realice las actividades y el sistema registre su progreso |
| 13 | [Dificultad Adaptativa](./Process/13-dificultad-adaptativa.md) | Que el sistema suba o baje la dificultad automáticamente según el rendimiento |

### Monitoreo y Reportes
| # | Proceso | Qué resuelve |
|---|---------|--------------|
| 14 | [Seguimiento](./Process/14-seguimiento-avances.md) | Que el profesional vea el progreso de sus personas en un dashboard visual |
| 15 | [Informes](./Process/15-generacion-informes.md) | Que el profesional genere reportes formales y la familia los consulte |

### Comunicación
| # | Proceso | Qué resuelve |
|---|---------|--------------|
| 16 | [Comunicación](./Process/16-comunicacion-actores.md) | Que profesionales y familias se comuniquen dentro de la plataforma |

### Administración de Cuentas
| # | Proceso | Qué resuelve |
|---|---------|--------------|
| 17 | [Gestión de Usuarios](./Process/17-gestion-usuarios.md) | Que el admin gestione cuentas de forma centralizada: resetear contraseñas, desactivar, reactivar |
| 18 | [Onboarding](./Process/18-onboarding.md) | Que cada usuario nuevo sea guiado en su primer ingreso al sistema |

### Soporte
| # | Proceso | Qué resuelve |
|---|---------|--------------|
| 19 | [Soporte y Ayuda](./Process/19-soporte.md) | Que los usuarios consulten ayuda y reporten problemas desde la plataforma |

El avance de implementación de cada proceso se puede consultar en el [checklist de procesos](./State/checklist-procesos.md).

---

## Historias de Usuario

Las funcionalidades del sistema están documentadas como **historias de usuario**: describen qué necesita cada actor, cómo funciona y los criterios para darla por terminada. Cada HU unifica lo que se necesita construir tanto en el servidor como en la interfaz visual.

| HU | Funcionalidad |
|----|---------------|
| [HU-01](./HU/HU-01-catalogos-configuracion.md) | Catálogos y configuración inicial del sistema |
| [HU-02](./HU/HU-02-actividades-templates.md) | Creación de actividades con plantillas dinámicas |
| [HU-03](./HU/HU-03-perfil-habilidades.md) | Perfil de habilidades del estudiante |
| [HU-04](./HU/HU-04-acceso-familiar.md) | Acceso familiar por invitación |
| [HU-05](./HU/HU-05-roadmap.md) | Roadmap: plan de trabajo personalizado |
| [HU-06](./HU/HU-06-ejecucion-actividades.md) | Ejecución de actividades interactivas |
| [HU-07](./HU/HU-07-dashboard-radar.md) | Dashboard y radar chart de habilidades |
| [HU-08](./HU/HU-08-diagnosticos-reportes.md) | Diagnósticos funcionales y reportes de progreso |
| [HU-09](./HU/HU-09-mensajeria.md) | Mensajería interna entre actores |
| [HU-10](./HU/HU-10-motor-adaptativo.md) | Motor de dificultad adaptativa |
| [HU-11](./HU/HU-11-gestion-usuarios.md) | Gestión centralizada de usuarios |
| [HU-12](./HU/HU-12-onboarding.md) | Onboarding de usuarios |
| [HU-13](./HU/HU-13-soporte.md) | Soporte y ayuda |

---

## Accesibilidad

La plataforma está diseñada para ser usada por personas con diferentes tipos de discapacidad:

- **7 perfiles visuales** (estándar, alto contraste, dislexia, baja visión, deuteranopía, protanopía, tritanopía) con modo claro y oscuro = **14 combinaciones**
- **4 métodos de login** adaptados al nivel de autonomía: contraseña visual, PIN numérico, login asistido por supervisor y login familiar
- **4 portales diferenciados** por rol: Admin, Profesional, Familia y AAC (persona con discapacidad)

Más detalle en [REF-accesibilidad.md](./References/REF-accesibilidad.md) y [REF-autenticacion.md](./References/REF-autenticacion.md).

---

## Datos del sistema

El sistema gestiona información sensible de personas con discapacidad, profesionales y familias. El [diccionario de datos](./diccionario-datos.md) describe las 31 entidades organizadas en 11 áreas: catálogos, usuarios, instituciones, actividades, planes de trabajo, respuestas, diagnósticos, reportes, mensajería y auditoría.

---

## Documentación técnica

Orientada al equipo de desarrollo:

| Documento | Para qué sirve |
|-----------|-----------------|
| [ARQUITECTURA.md](./ARQUITECTURA.md) | Entender cómo está construido el sistema |
| [HU_ESTADO.md](./HU_ESTADO.md) | Ver qué está hecho y qué falta por implementar (BE/FE) |
| [State/checklist-procesos.md](./State/checklist-procesos.md) | Checklist de avance por proceso (MVP vs Post-MVP) |
| [State/progreso-hu.md](./State/progreso-hu.md) | Progreso detallado por HU con código Jira y estado |
| [Docs/story-map.md](./Docs/story-map.md) | Story map: backbone, tareas y HUs del MVP |
| [der.md](./der.md) | DER con tipos PostgreSQL, nulabilidad y responsabilidades |
| [Features/InclusiON_HUs_BEyFE.md](./Features/InclusiON_HUs_BEyFE.md) | Especificaciones técnicas de cada funcionalidad |
| [Features/MDA_Especificacion_Tecnica.md](./Features/MDA_Especificacion_Tecnica.md) | Motor de dificultad adaptativa |
| [Features/CIF_ACCESIBILIDAD_ANGULAR.md](./Features/CIF_ACCESIBILIDAD_ANGULAR.md) | Referencia de accesibilidad CIF/ICF |
| [CLAUDE_BACKEND.md](./CLAUDE_BACKEND.md) | Instrucciones para trabajar en el backend (.NET 10) |
| [CLAUDE_FRONTEND.md](./CLAUDE_FRONTEND.md) | Instrucciones para trabajar en el frontend (Angular 20) |
| [Test/README.md](./Test/README.md) | Setup, scripts, CI y estructura de InclusiON.Testing |

---

## Equipo

| Rol | Integrante |
|-----|------------|
| Product Owner | Ferreyra Candelaria, Vettorazzi Catalina |
| Scrum Master | Decalli Mariano |
| Desarrollo | Aparicio Fernando, Cochis German, Del Barrio Sacha, Wlk Mirko |
| Profesor | Loza, Fernando Hugo |
