# InclusiON — Listado de Casos de Uso

**Práctica Profesionalizante II — Institución Cervantes**

---

## Actores del Sistema

### Identificación de Actores

| ID | Actor | Tipo | Descripción | Cómo accede |
|----|-------|------|-------------|-------------|
| A-01 | **Usuario** | Abstracto (base) | Actor base del que heredan todos los roles autenticados. No interactúa directamente con el sistema. | — |
| A-02 | **Admin Global** | Primario | Gestiona el sistema completo: instituciones, catálogos, cuentas, reportes. Sin restricción de institución. | Email + contraseña |
| A-03 | **Admin Institucional** | Primario | Igual que Admin Global pero limitado a las instituciones que tiene asignadas. | Email + contraseña |
| A-04 | **Profesional** | Primario | Docente, terapeuta o psicólogo. Crea actividades, gestiona roadmaps y hace seguimiento de personas asignadas. | Email + contraseña |
| A-05 | **Persona** | Primario | Persona con discapacidad. Ejecuta actividades de su roadmap. Interfaz AAC adaptada. | PIN / login asistido / login visual |
| A-06 | **Familiar** | Primario | Representante o tutor. Acceso de solo lectura al progreso de su persona vinculada. | Email + contraseña (por invitación) |
| A-07 | **Sistema** | Secundario | Componente automatizado que ejecuta acciones sin intervención humana (ajuste adaptativo, desbloqueos, auditoría). | — |

### Jerarquía de Actores

```
Usuario (A-01)
├── Admin Global (A-02)
│   └── Admin Institucional (A-03)  [hereda permisos, restringido por institución]
├── Profesional (A-04)
├── Persona (A-05)
└── Familiar (A-06)

Sistema (A-07)  [actor secundario, sin herencia]
```

### Relaciones entre Actores

| Relación | Descripción |
|----------|-------------|
| Admin Global → Admin Institucional | Herencia. Admin Institucional tiene las mismas capacidades pero acotadas a sus instituciones. |
| Profesional → Familiar | El Profesional genera la invitación que permite al Familiar registrarse. |
| Profesional → Persona | El Profesional configura el método de login y accesibilidad de la Persona. |
| Familiar ↔ Persona | Vínculo N:M. Un Familiar puede tener varias personas; una persona puede tener varios representantes. |
| Sistema → Persona | El Sistema ajusta la dificultad y desbloquea actividades en base al rendimiento de la Persona. |

---

## Archivos detallados por módulo

| Módulo | Archivo |
|--------|---------|
| 1 — Configuración del Sistema | [CU/CU-01-configuracion.md](CU/CU-01-configuracion.md) |
| 2 — Gestión de Usuarios | [CU/CU-02-gestion-usuarios.md](CU/CU-02-gestion-usuarios.md) |
| 3 — Acceso al Sistema | [CU/CU-03-acceso.md](CU/CU-03-acceso.md) |
| 4 — Actividades | [CU/CU-04-actividades.md](CU/CU-04-actividades.md) |
| 5 — Perfil de Habilidades | [CU/CU-05-habilidades.md](CU/CU-05-habilidades.md) |
| 6 — Roadmap | [CU/CU-06-roadmap.md](CU/CU-06-roadmap.md) |
| 7 — Ejecución de Actividades | [CU/CU-07-ejecucion.md](CU/CU-07-ejecucion.md) |
| 8 — Seguimiento y Resultados | [CU/CU-08-seguimiento.md](CU/CU-08-seguimiento.md) |
| 9 — Diagnósticos y Reportes | [CU/CU-09-diagnosticos-reportes.md](CU/CU-09-diagnosticos-reportes.md) |
| 10 — Mensajería | [CU/CU-10-mensajeria.md](CU/CU-10-mensajeria.md) |
| 11 — Motor Adaptativo | [CU/CU-11-motor-adaptativo.md](CU/CU-11-motor-adaptativo.md) |
| 12 — Onboarding | [CU/CU-12-onboarding.md](CU/CU-12-onboarding.md) |
| 13 — Soporte | [CU/CU-13-soporte.md](CU/CU-13-soporte.md) |

---

## Listado de Casos de Uso

### Módulo 1 — Configuración del Sistema

| ID | Nombre | Actor principal | HU de referencia |
|----|--------|----------------|------------------|
| CU-01 | Gestionar catálogos del sistema | Admin Global | HU-01 |
| CU-02 | Registrar institución | Admin Global | HU-01 |
| CU-03 | Asignar profesional a institución | Admin | HU-01 |
| CU-04 | Asignar persona a profesional | Admin / Profesional | HU-01 |

---

### Módulo 2 — Gestión de Usuarios

| ID | Nombre | Actor principal | HU de referencia |
|----|--------|----------------|------------------|
| CU-05 | Auto-registrarse como profesional | Profesional (público) | HU-IN-149 |
| CU-06 | Validar solicitud de registro de profesional | Admin | HU-IN-150 |
| CU-07 | Registrar persona con discapacidad | Admin / Profesional | HU-01 |
| CU-08 | Invitar familiar al sistema | Profesional | HU-04 |
| CU-09 | Completar registro por invitación | Familiar | HU-04 |
| CU-10 | Gestionar cuentas de usuario (reset, baja, alta) | Admin | HU-11 |

---

### Módulo 3 — Acceso al Sistema

| ID | Nombre | Actor principal | HU de referencia |
|----|--------|----------------|------------------|
| CU-11 | Iniciar sesión estándar (email y contraseña) | Profesional / Admin / Familiar | HU-01 |
| CU-12 | Iniciar sesión por PIN | Persona | HU-01 |
| CU-13 | Iniciar sesión asistido (supervisado por profesional) | Persona | HU-01 |
| CU-14 | Iniciar sesión familiar | Familiar | HU-04 |
| CU-15 | Cambiar contraseña temporal en primer ingreso | Profesional / Familiar | HU-12 |
| CU-16 | Configurar perfil de accesibilidad | Persona / Profesional | HU-01 |

---

### Módulo 4 — Actividades

| ID | Nombre | Actor principal | HU de referencia |
|----|--------|----------------|------------------|
| CU-17 | Crear actividad desde plantilla dinámica | Profesional | HU-02 |
| CU-18 | Editar actividad propia | Profesional | HU-02 |
| CU-19 | Desactivar actividad | Profesional | HU-02 |
| CU-20 | Consultar catálogo de actividades | Profesional | HU-02 |

---

### Módulo 5 — Perfil de Habilidades

| ID | Nombre | Actor principal | HU de referencia |
|----|--------|----------------|------------------|
| CU-21 | Asignar áreas de habilidad a una persona | Profesional | HU-03 |
| CU-22 | Desactivar área de habilidad de una persona | Profesional | HU-03 |

---

### Módulo 6 — Roadmap

| ID | Nombre | Actor principal | HU de referencia |
|----|--------|----------------|------------------|
| CU-23 | Crear roadmap de una persona | Profesional | HU-05 |
| CU-24 | Agregar actividad al roadmap | Profesional | HU-05 |
| CU-25 | Reordenar actividades del roadmap | Profesional | HU-05 |
| CU-26 | Forzar desbloqueo manual de actividad | Profesional | HU-05 |
| CU-27 | Eliminar actividad del roadmap | Profesional | HU-05 |
| CU-28 | Consultar roadmap propio | Persona | HU-05 |

---

### Módulo 7 — Ejecución de Actividades

| ID | Nombre | Actor principal | HU de referencia |
|----|--------|----------------|------------------|
| CU-29 | Ejecutar actividad interactiva | Persona | HU-06 |
| CU-30 | Registrar resultado de actividad | Sistema | HU-06 |
| CU-31 | Desbloquear siguiente actividad por umbral | Sistema | HU-06 |

---

### Módulo 8 — Seguimiento y Resultados

| ID | Nombre | Actor principal | HU de referencia |
|----|--------|----------------|------------------|
| CU-32 | Consultar dashboard del profesional | Profesional | HU-07 |
| CU-33 | Consultar radar chart de habilidades | Profesional | HU-07 |
| CU-34 | Consultar Mi Aula (vista de personas asignadas) | Profesional | HU-07 |
| CU-35 | Consultar dashboard familiar | Familiar | HU-07 |
| CU-36 | Consultar respuestas de una actividad asignada | Profesional | HU-07 |

---

### Módulo 9 — Diagnósticos y Reportes

| ID | Nombre | Actor principal | HU de referencia |
|----|--------|----------------|------------------|
| CU-37 | Registrar diagnóstico funcional | Profesional | HU-08 |
| CU-38 | Editar diagnóstico propio | Profesional | HU-08 |
| CU-39 | Consultar historial de diagnósticos | Profesional | HU-08 / HU-IN-86 |
| CU-40 | Crear reporte de progreso | Profesional | HU-08 |
| CU-41 | Enviar reporte para aprobación | Profesional | HU-08 |
| CU-42 | Aprobar reporte de progreso | Admin | HU-08 |
| CU-43 | Rechazar reporte de progreso (con comentario) | Admin | HU-08 |
| CU-44 | Consultar reportes aprobados | Familiar | HU-08 |

---

### Módulo 10 — Mensajería

| ID | Nombre | Actor principal | HU de referencia |
|----|--------|----------------|------------------|
| CU-45 | Enviar mensaje interno | Profesional / Familiar | HU-09 |
| CU-46 | Responder mensaje (hilo) | Profesional / Familiar | HU-09 |
| CU-47 | Consultar bandeja de entrada | Profesional / Familiar | HU-09 |

---

### Módulo 11 — Motor Adaptativo

| ID | Nombre | Actor principal | HU de referencia |
|----|--------|----------------|------------------|
| CU-48 | Configurar motor adaptativo para una actividad | Profesional | HU-10 |
| CU-49 | Consultar historial de ajustes adaptativos | Profesional | HU-10 |
| CU-50 | Ajustar dificultad automáticamente según rendimiento | Sistema | HU-10 |

---

### Módulo 12 — Onboarding

| ID | Nombre | Actor principal | HU de referencia |
|----|--------|----------------|------------------|
| CU-51 | Completar wizard de perfil en primer ingreso | Profesional | HU-12 |
| CU-52 | Realizar tour guiado del portal | Profesional | HU-12 |
| CU-53 | Ver pantalla de bienvenida tras registro | Familiar / Persona | HU-12 |

---

### Módulo 13 — Soporte

| ID | Nombre | Actor principal | HU de referencia |
|----|--------|----------------|------------------|
| CU-54 | Consultar FAQ | Todos | HU-13 |
| CU-55 | Reportar problema técnico (ticket) | Todos | HU-13 |
| CU-56 | Gestionar tickets de soporte | Admin | HU-13 |
| CU-57 | Gestionar entradas de FAQ | Admin | HU-13 |

---

## Resumen por Actor

| Actor | Casos de uso |
|-------|-------------|
| Admin Global / Institucional | CU-01, CU-02, CU-03, CU-04, CU-06, CU-10, CU-42, CU-43, CU-56, CU-57 |
| Profesional | CU-05, CU-08, CU-11, CU-15, CU-16, CU-17, CU-18, CU-19, CU-20, CU-21, CU-22, CU-23, CU-24, CU-25, CU-26, CU-27, CU-32, CU-33, CU-34, CU-36, CU-37, CU-38, CU-39, CU-40, CU-41, CU-45, CU-46, CU-47, CU-48, CU-49, CU-51, CU-52, CU-54, CU-55 |
| Persona | CU-12, CU-13, CU-16, CU-28, CU-29, CU-53 |
| Familiar | CU-09, CU-14, CU-15, CU-35, CU-44, CU-45, CU-46, CU-47, CU-53, CU-54, CU-55 |
| Sistema (automático) | CU-30, CU-31, CU-50 |

---

*Documento generado en base a las Historias de Usuario del MVP — InclusiON v2, Mayo 2026.*
