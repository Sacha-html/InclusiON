# IN-188 — Justificación de Roles del Sistema respecto a los Actores de Negocio

## Contexto

El análisis de actores de negocio identificó los 4 actores funcionales que interactúan con InclusiON:
Persona con Discapacidad (Alumno), Profesional (Docente / Terapeuta), Familia / Cuidador y Administrador Institucional (Equipo Directivo).
Este documento justifica cómo y por qué cada actor se traduce en un rol técnico del sistema.

---

## Mapeo actor → rol

| Actor de Negocio | Rol técnico | Claim JWT |
|---|---|---|
| Persona con Discapacidad (Alumno) | `persona` | `role: persona` |
| Profesional (Docente / Terapeuta) | `profesional` | `role: profesional` |
| Familia / Cuidador | `familia` | `role: familia` |
| Administrador Institucional (Equipo Directivo) | `admin` | `role: admin`, `institutionId: <id>` |
| Institución Educativa | — (sin rol) | — |

---

## Justificación por rol

### Rol `persona`

**Actor:** Persona con Discapacidad (Alumno)

La persona es el destinatario principal del sistema. Necesita una interfaz radicalmente distinta al resto de los actores: portal AAC con pictogramas, navegación simplificada, feedback visual/sonoro y métodos de login accesibles (PIN, ASSISTED, FAMILY). Separar este rol garantiza que sus vistas, permisos y flujo de autenticación se puedan configurar de forma independiente sin afectar a los demás actores. Compartir rol con otro actor implicaría exponer datos clínicos o funcionalidades administrativas a usuarios que no deben verlos.

**Decisiones técnicas que dependen de este rol:**
- Métodos de login: `PIN`, `ASSISTED`, `FAMILY` (adicionales al `STANDARD`)
- Vistas exclusivas: portal AAC, roadmap visual, players de actividad
- Sin acceso a datos de otros usuarios ni a paneles de gestión

---

### Rol `profesional`

**Actor:** Profesional (docente, terapeuta, fonoaudiólogo, psicólogo)

El profesional es el actor clínico/educativo del sistema. Crea contenido (actividades con templates dinámicos), define el plan de trabajo y accede a datos sensibles de personas bajo su cargo: diagnósticos funcionales, perfiles de habilidades, resultados de actividades. Separar este rol permite aplicar el principio de mínimo privilegio: el profesional solo ve personas que tiene asignadas, no a toda la institución.

**Decisiones técnicas que dependen de este rol:**
- Acceso restringido por asignación `profesional ↔ persona`
- Vistas exclusivas: Mi Aula, detalle de persona, creación de actividades, roadmap, reportes
- Puede invitar familiares (genera token de invitación vinculado a su persona asignada)

---

### Rol `familia`

**Actor:** Familia / Cuidador

El familiar o cuidador acompaña a la persona desde afuera del sistema terapéutico. Su acceso es de lectura y seguimiento: ve el progreso de su familiar, lee reportes generados por el profesional y se comunica con él. No tiene acceso a datos clínicos ni a la gestión del sistema. Se registra únicamente por invitación del profesional (no hay auto-registro libre), lo que garantiza que solo accede quien fue habilitado explícitamente.

**Decisiones técnicas que dependen de este rol:**
- Registro exclusivamente por invitación (`token` + email)
- Sin acceso a datos de diagnóstico ni de actividades en detalle clínico
- Vistas exclusivas: dashboard familiar, progreso, reportes, mensajería

---

### Rol `admin` — Administrador Institucional (Equipo Directivo)

**Actor:** Administrador Institucional / Dirección Escolar

Es la máxima autoridad operativa dentro del establecimiento educativo. Gestiona el personal escolar (alta de docentes y terapeutas, blanqueo de credenciales y revocación inmediata de accesos), matricula a los alumnos, vincula a los tutores legales y audita la trazabilidad integral de la institución para dar cumplimiento a la Ley 25.326 de Protección de Datos Personales.

**Decisiones técnicas que dependen de este rol:**
- `institutionId: <id>` en JWT — todas las operaciones directivas se acotan a su ámbito escolar
- Gestión de cuentas de usuario: reseteo de contraseñas, activación y revocación de accesos
- Matriculación formal de alumnos y asignaciones de docentes a salas/alumnos
- Consulta de catálogos estandarizados del sistema y aprobación formal de reportes de progreso

---

### Por qué Institución no tiene rol

La institución es una entidad organizativa, no un usuario. No inicia sesión ni ejecuta acciones. Agrupa profesionales y personas, y define el alcance del Administrador Institucional. Su presencia en el sistema es como dato (`institutionId` en JWT), no como actor con credenciales.

---

## Resumen de separación de responsabilidades

| Responsabilidad | persona | profesional | familia | admin inst. |
|---|:---:|:---:|:---:|:---:|
| Realizar actividades | ✓ | | | |
| Crear actividades y roadmap | | ✓ | | |
| Ver datos clínicos de persona | | ✓ (asignadas) | | |
| Ver progreso de familiar | | | ✓ | |
| Comunicación (mensajería) | | ✓ | ✓ | |
| Gestionar usuarios de la institución | | | | ✓ |
| Asignar profesionales a alumnos | | | | ✓ |
| Aprobar reportes oficiales | | | | ✓ |
| Configurar datos de la sede | | | | ✓ |
