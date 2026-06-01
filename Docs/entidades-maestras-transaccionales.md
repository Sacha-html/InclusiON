# Entidades Maestras y Transaccionales — InclusiON

**Generado:** 2026-05-05  
**Fuente:** `der.md` (39 entidades totales)

---

## Entidades Maestras

> Datos de referencia/configuración. Cambian poco, son referenciados por otras tablas.

### Catálogos puros

| Entidad | Rol |
|---|---|
| `DisabilityType` | Tipos de discapacidad reconocidos |
| `AutonomyLevel` | Niveles de autonomía (define si requiere supervisión) |
| `LoginMethod` | Métodos de autenticación disponibles (STANDARD=1, PIN=2, ASSISTED=3) |
| `ActivityCategory` | Categorías temáticas de actividades |
| `ReportType` | Tipos de reporte de progreso clínico |
| `SkillArea` | Áreas de habilidad (eje central del radar chart) |
| `ActivityTemplateType` | Templates con schema JSON y componente Angular asociado |

### Perfiles / Entidades de dominio

| Entidad | Rol |
|---|---|
| `User` | Identidad base de todos los actores (ASP.NET Identity) |
| `EducationalInstitution` | Instituciones educativas registradas |
| `Professional` | Perfil extendido del profesional (estado Pending/Approved/Rejected) |
| `PersonWithDisability` | Perfil central de la persona atendida (accesibilidad, auth, autonomía) |
| `FamilyRepresentative` | Perfil del familiar/tutor |
| `Activity` | Actividad educativa reutilizable creada por un profesional |
| `ActivityContent` | Contenido dinámico JSON de la actividad (1:1 con Activity) |
| `ActivityEmbedding` | Vector semántico para búsqueda por similaridad — pgvector (1:1 con Activity) |
| `PersonRoadmap` | Plan de aprendizaje personalizado (1:1 con PersonWithDisability) |
| `AdaptiveEngineConfig` | Configuración del motor de dificultad adaptativa por actividad del roadmap (1:1) |

**Total maestras: 17**

---

## Entidades Transaccionales

> Generadas durante la operación del sistema. Representan eventos, estados, resultados o trazabilidad.

### Sesión / Auth

| Entidad | Evento que representa |
|---|---|
| `RefreshToken` | Emisión y revocación de tokens JWT |
| `TrustedDevice` | Autorización de dispositivo para login asistido |

### Relaciones operativas

| Entidad | Evento que representa |
|---|---|
| `AdminInstitution` | Asignación de admin a institución |
| `ProfessionalInstitution` | Vínculo profesional ↔ institución |
| `ProfessionalPerson` | Vínculo profesional ↔ persona atendida |
| `PersonRepresentative` | Vínculo persona ↔ familiar (con fechas de vigencia y consentimiento) |
| `PersonSkillProfile` | Áreas de habilidad activas asignadas a una persona |
| `PersonRoadmapArea` | Sección del roadmap correspondiente a un área de habilidad |
| `PersonRoadmapActivity` | Actividad dentro del roadmap (desbloqueo, dificultad, límites) |
| `Invitation` | Código de un solo uso generado para registro de familiar |

### Ejecución / Respuestas

| Entidad | Evento que representa |
|---|---|
| `ActivityAssignment` | Asignación directa de actividad a una persona |
| `ActivityResponse` | Resultado de ejecución de actividad asignada (cifrado AES-256-GCM) |
| `ActivityResult` | Resultado por intento en roadmap (input principal del radar chart) |
| `AdaptiveAdjustmentLog` | Ajuste de dificultad realizado por el motor adaptativo |

### Clínico / Reportes

| Entidad | Evento que representa |
|---|---|
| `Diagnosis` | Diagnóstico funcional registrado por el profesional (texto cifrado) |
| `Report` | Reporte de progreso con flujo Draft → Submitted → Approved/Rejected |

### Historial / Auditoría

| Entidad | Evento que representa |
|---|---|
| `ProfessionalStatusHistory` | Cambios de estado del profesional (Pending → Approved/Rejected) |
| `FamilyStatusHistory` | Cambios de estado del familiar (Active/Terminated — sin flujo de aprobación) |
| `PersonRepresentativeHistory` | Altas, bajas y modificaciones del vínculo persona-familiar |
| `AccessAudit` | Registro de accesos a recursos (IN-172) — Allowed/Denied |
| `Message` | Mensajes internos entre usuarios con soporte de hilos |

**Total transaccionales: 22**

---

## Resumen

| Tipo | Cantidad |
|---|---|
| Maestras — catálogos puros | 7 |
| Maestras — perfiles/dominio | 10 |
| **Total maestras** | **17** |
| Transaccionales | 22 |
| **Total general** | **39** |

> **Nota:** `PersonRoadmapArea` y `PersonRoadmapActivity` son semi-maestras — se crean una vez por plan pero mutan en tiempo de ejecución (desbloqueo, nivel de dificultad). Se clasifican como transaccionales por su naturaleza dinámica.
