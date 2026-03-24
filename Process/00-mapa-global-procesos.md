# Mapa Global de Procesos del Sistema InclusiON

## Visión general

El sistema InclusiON organiza sus procesos en 8 áreas funcionales que cubren el ciclo completo de trabajo con personas con discapacidad: desde la configuración inicial del sistema hasta el monitoreo continuo del progreso.

```mermaid
flowchart TB
    subgraph CONFIG["Configuración del Sistema"]
        P01[01 Instituciones]
        P02[02 Roles y Permisos]
        P03[03 Catálogos]
    end

    subgraph USUARIOS["Gestión de Usuarios"]
        P04[04 Profesionales]
        P05[05 Personas con Discapacidad]
        P06[06 Familiares]
        P07[07 Invitaciones]
    end

    subgraph ASIG["Asignaciones"]
        P08[08 Asignación de Profesionales]
    end

    subgraph EVAL["Evaluación y Planificación"]
        P09[09 Evaluación y Diagnóstico]
        P10[10 Gestión de Actividades]
        P11[11 Plan de Trabajo / Roadmap]
    end

    subgraph EJEC["Ejecución"]
        P12[12 Resolución de Actividades]
        P13[13 Dificultad Adaptativa MDA]
    end

    subgraph MONIT["Monitoreo y Reportes"]
        P14[14 Seguimiento de Avances]
        P15[15 Generación de Informes]
    end

    subgraph COM["Comunicación"]
        P16[16 Comunicación entre Actores]
    end

    subgraph ADMIN_CUENTAS["Administración de Cuentas"]
        P17[17 Gestión de Usuarios]
        P18[18 Onboarding]
    end

    subgraph SOPORTE["Soporte"]
        P19[19 Soporte y Ayuda]
    end

    CONFIG --> USUARIOS
    USUARIOS --> ASIG
    ASIG --> EVAL
    EVAL --> EJEC
    EJEC --> MONIT
    MONIT -->|Ajustar plan| EVAL
    COM -.->|Transversal| USUARIOS
    COM -.->|Transversal| MONIT
    ADMIN_CUENTAS -.->|Transversal| USUARIOS
    P18 -.->|Post primer login| USUARIOS
    SOPORTE -.->|Transversal| CONFIG & USUARIOS & EVAL & EJEC & MONIT

```

## Relación entre procesos

```mermaid
flowchart LR
    P01[01 Instituciones] -->|Agrupa| P04[04 Profesionales]
    P01 -->|Agrupa| P05[05 Personas]
    P02[02 Roles] -->|Controla acceso a| P01 & P03 & P04 & P05 & P06
    P03[03 Catálogos] -->|Alimenta formularios de| P05 & P09 & P10

    P04 -->|Se asigna vía| P08[08 Asignaciones]
    P05 -->|Se asigna vía| P08
    P06[06 Familiares] -->|Se registra vía| P07[07 Invitaciones]
    P07 -->|Vincula a| P05

    P08 -->|Habilita| P09[09 Evaluación]
    P09 -->|Define perfil para| P10[10 Actividades]
    P10 -->|Se organiza en| P11[11 Roadmap]

    P11 -->|Asigna a persona| P12[12 Resolución]
    P12 -->|Trigger| P13[13 MDA]
    P13 -->|Ajusta| P12

    P12 -->|Genera datos para| P14[14 Seguimiento]
    P14 -->|Consolida en| P15[15 Informes]
    P15 -->|Visible para| P06

    P16[16 Comunicación] -.->|Notifica| P04 & P06

    P14 -->|Retroalimenta| P09

    P17[17 Gestión Usuarios] -.->|Administra cuentas de| P04 & P05 & P06
    P18[18 Onboarding] -->|Post alta| P04 & P06
    P19[19 Soporte] -.->|Atiende a| P04 & P06
```

## Fases del sistema (DOCX)

El proyecto final define 4 fases secuenciales que agrupan los procesos:

### Fase 1 — Configuración y Onboarding
**Objetivo:** Preparar el sistema y dar de alta a todos los actores.

| Proceso | Descripción |
|---------|-------------|
| 01 Instituciones | Crear instituciones educativas |
| 02 Roles y Permisos | Configurar roles y crear admins institucionales |
| 03 Catálogos | Cargar tablas de referencia |
| 04 Profesionales | Alta de profesionales con credenciales |
| 05 Personas | Alta de personas con discapacidad |
| 06 Familiares | Alta directa o por invitación |
| 07 Invitaciones | Envío de email para registro de familiares |
| 08 Asignaciones | Vincular profesionales a instituciones y personas |
| 18 Onboarding | Guiar a cada usuario en su primer ingreso |

```mermaid
flowchart LR
    F1A[Admin crea institución] --> F1B[Admin crea profesional]
    F1B --> F1C[Admin crea persona]
    F1C --> F1D[Admin asigna profesional ↔ persona]
    F1D --> F1E[Profesional invita familiar]
    F1E --> F1F[Familiar se registra]
    F1F --> F1G[Onboarding de cada usuario]
    F1G --> LISTO[Sistema listo para operar]
```

---

### Fase 2 — Evaluación y Diagnóstico
**Objetivo:** Establecer el punto de partida del alumno antes de la intervención.

| Proceso | Descripción |
|---------|-------------|
| 09 Evaluación | Configurar perfil de habilidades y perfil funcional |
| 09 Diagnóstico | Registrar diagnóstico funcional formal |

```mermaid
flowchart LR
    F2A[Profesional configura perfil de habilidades] --> F2B[Profesional edita perfil funcional]
    F2B --> F2C[Profesional registra diagnóstico]
    F2C --> EVAL[Evaluación inicial completa]
```

---

### Fase 3 — Intervención y Personalización
**Objetivo:** Diseñar el plan de trabajo personalizado basado en la evaluación.

| Proceso | Descripción |
|---------|-------------|
| 10 Actividades | Crear actividades con templates dinámicos |
| 11 Roadmap | Armar secuencia de actividades por área |
| 13 MDA config | Configurar parámetros del motor adaptativo |

```mermaid
flowchart LR
    F3A[Profesional crea actividades] --> F3B[Profesional arma roadmap]
    F3B --> F3C[Profesional configura MDA]
    F3C --> PLAN[Plan de trabajo listo]
```

---

### Fase 4 — Ciclo de Seguimiento y Mejora Continua
**Objetivo:** Ciclo operativo principal. El alumno trabaja, el sistema adapta, los actores monitorean.

| Proceso | Descripción |
|---------|-------------|
| 12 Resolución | La persona realiza actividades en el portal AAC |
| 13 MDA auto | El sistema ajusta dificultad automáticamente |
| 14 Seguimiento | Profesional y familia monitorean progreso |
| 15 Informes | Generación de reportes formales |
| 16 Comunicación | Mensajería entre profesional y familia |

```mermaid
flowchart TD
    F4A[Persona realiza actividad] --> F4B[Sistema registra respuesta]
    F4B --> F4C{MDA evalúa rendimiento}
    F4C -->|Ajusta dificultad| F4A
    F4C -->|Registra datos| F4D[Dashboard profesional]
    F4D --> F4E[Profesional monitorea]
    F4E -->|Genera| F4F[Reporte de progreso]
    F4F -->|Visible para| F4G[Familia consulta]
    F4E -->|Decide| F4H{¿Ajustar plan?}
    F4H -->|Reforzar| F4I[Volver a Fase 3: más actividades del mismo nivel]
    F4H -->|Avanzar| F4J[Volver a Fase 3: actividades de mayor complejidad]
    F4H -->|Mantener| F4A
```

El estado de avance de cada proceso se puede consultar en el [checklist de procesos](../Estado/checklist-procesos.md).

---

## Actores y sus procesos

| Actor | Procesos en los que participa |
|-------|------------------------------|
| **Admin Global** | 01, 02, 03, 04, 05, 06, 08, 17, 19 |
| **Admin Institucional** | 04, 05, 06, 08, 17, 19 |
| **Profesional** | 07, 08, 09, 10, 11, 14, 15, 16, 18, 19 |
| **Persona con Discapacidad** | 12, 18 |
| **Familia** | 07, 14, 15, 16, 18, 19 |
| **Sistema (automático)** | 13 |

## Referencias transversales

Estos documentos describen capacidades que soportan todos los procesos:

| Referencia | Descripción | Ubicación |
|------------|-------------|-----------|
| Accesibilidad | 7 perfiles × 2 modos = 14 combinaciones visuales | `References/REF-accesibilidad.md` |
| Autenticación | 5 métodos de login + JWT + refresh tokens | `References/REF-autenticacion.md` |
| Gestión de Usuarios | Administración centralizada de cuentas | `Process/17-gestion-usuarios.md` |
| Onboarding | Primer ingreso y configuración por rol | `Process/18-onboarding.md` |
| Soporte | Centro de ayuda, FAQ y tickets | `Process/19-soporte.md` |
