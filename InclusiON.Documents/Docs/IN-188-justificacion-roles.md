# IN-188 — Justificación de Roles del Sistema respecto a los Actores de Negocio

## Contexto

El análisis de actores de negocio (IN-187) identificó 5 actores que interactúan con InclusiON:
Persona con Discapacidad, Profesional, Familia/Cuidador, Admin Institucional y Admin Global.
Este documento justifica cómo y por qué cada actor se traduce en un rol técnico del sistema.

---

## Mapeo actor → rol

| Actor de Negocio | Rol técnico | Claim JWT |
|---|---|---|
| Persona con Discapacidad | `persona` | `role: persona` |
| Profesional | `profesional` | `role: profesional` |
| Familia / Cuidador | `familia` | `role: familia` |
| Admin Institucional | `admin` | `role: admin`, `isGlobalAdmin: false`, `institutionId: <id>` |
| Admin Global | `admin` | `role: admin`, `isGlobalAdmin: true` |
| Institución | — (sin rol) | — |

---

## Justificación por rol

### Rol `persona`

**Actor:** Persona con Discapacidad

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

### Rol `admin` — Admin Institucional

**Actor:** Admin Institucional

Gestiona operativamente los usuarios de su institución: alta de profesionales y personas, asignación de profesionales, reset de contraseñas y desactivación de cuentas. Su alcance está limitado por `institutionId` en el JWT, por lo que no puede ver ni modificar datos de otras instituciones. Este límite es técnico, no solo de UI: los endpoints filtran por `institutionId` extraído del token.

**Decisiones técnicas que dependen de este rol:**
- `isGlobalAdmin: false` + `institutionId: <id>` en JWT
- Todos los queries de gestión filtran por `institutionId`
- No puede crear instituciones ni acceder a configuración global

---

### Rol `admin` — Admin Global

**Actor:** Admin Global

Tiene acceso sistémico completo: crea y gestiona instituciones, configura roles y permisos, carga catálogos maestros y puede operar sobre cualquier usuario del sistema. Es el único actor que puede crear admins institucionales. Se diferencia del admin institucional por el flag `isGlobalAdmin: true` en el JWT, que bypasea los filtros por institución.

**Decisiones técnicas que dependen de este rol:**
- `isGlobalAdmin: true` en JWT — sin filtro por institución
- Acceso a endpoints de configuración global (`/admin/institutions`, `/admin/catalogs`, `/admin/roles`)
- Puede ver y actuar sobre todas las instituciones simultáneamente

---

### Por qué `admin` es un único rol técnico con dos alcances

Unificar el rol técnico en `admin` con un flag `isGlobalAdmin` en lugar de crear dos roles separados (`admin-global` y `admin-inst`) responde a que:

1. Las operaciones son las mismas, solo cambia el filtro de alcance.
2. Un único rol simplifica la asignación de permisos: todas las políticas de `admin` aplican a ambos; el filtro por institución es responsabilidad del middleware, no de la capa de autorización.
3. Escala mejor: si en el futuro se agregan sub-instituciones o jerarquías, el modelo de flag + claim es más flexible que multiplicar roles.

---

### Por qué Institución no tiene rol

La institución es una entidad organizativa, no un usuario. No inicia sesión ni ejecuta acciones. Agrupa profesionales y personas, y define el alcance del admin institucional. Su presencia en el sistema es como dato (`institutionId` en JWT), no como actor con credenciales.

---

## Resumen de separación de responsabilidades

| Responsabilidad | persona | profesional | familia | admin inst. | admin global |
|---|:---:|:---:|:---:|:---:|:---:|
| Realizar actividades | ✓ | | | | |
| Crear actividades y roadmap | | ✓ | | | |
| Ver datos clínicos de persona | | ✓ (asignadas) | | | |
| Ver progreso de familiar | | | ✓ | | |
| Comunicación (mensajería) | | ✓ | ✓ | | |
| Gestionar usuarios de institución | | | | ✓ | ✓ |
| Gestionar instituciones | | | | | ✓ |
| Configurar sistema global | | | | | ✓ |
