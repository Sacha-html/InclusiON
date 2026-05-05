# Módulo 12 — Onboarding

---

## CU-51: Completar wizard de perfil en primer ingreso

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | — |
| **HU de referencia** | HU-12 |
| **Prioridad** | Alta |

**Precondiciones**
- El Profesional cambió su contraseña temporal por primera vez (CU-15).
- El perfil tiene campos obligatorios vacíos (especialidad, teléfono, matrícula profesional).
- `hasCompletedOnboarding = false`.

**Flujo principal**
1. Tras el cambio de contraseña, el sistema detecta el perfil incompleto y redirige al wizard.
2. El wizard solicita en pasos: especialidad (del catálogo), teléfono, matrícula profesional.
3. El Profesional completa los campos y avanza.
4. Al finalizar, el sistema guarda el perfil completo y muestra el tour guiado (CU-52).

**Flujos alternativos**
- **2a. Campos opcionales vacíos:** El sistema permite omitirlos y avanzar.
- **Perfil ya completo al momento del primer ingreso:** El wizard no se muestra; se pasa directamente al tour.

**Postcondiciones**
- El perfil del Profesional queda completo.
- El wizard no vuelve a mostrarse.

---

## CU-52: Realizar tour guiado del portal

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | — |
| **HU de referencia** | HU-12 |
| **Prioridad** | Media |

**Precondiciones**
- El Profesional completó el wizard de perfil o el perfil ya estaba completo.
- `hasCompletedOnboarding = false`.

**Flujo principal**
1. El sistema muestra tooltips superpuestos sobre las secciones principales del portal: dashboard, Mi Aula, actividades, roadmap, mensajes.
2. El Profesional navega por los tooltips con "Siguiente / Anterior / Saltar".
3. Al completar o saltar el tour, el sistema marca `hasCompletedOnboarding = true`.

**Flujos alternativos**
- **Relanzar tour:** El Profesional puede volver a iniciar el tour desde la sección Configuración / Ayuda.

**Postcondiciones**
- `hasCompletedOnboarding = true`. El tour no se muestra automáticamente en futuros ingresos.

---

## CU-53: Ver pantalla de bienvenida tras registro

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Familiar / Persona |
| **Actores secundarios** | — |
| **HU de referencia** | HU-12 |
| **Prioridad** | Media |

**Precondiciones — Familiar**
- El Familiar completó su registro por invitación (CU-09) y accede por primera vez.

**Precondiciones — Persona**
- La Persona realiza su primer login (PIN, asistido o visual).

**Flujo principal — Familiar**
1. Tras el primer login exitoso, el sistema muestra pantalla de bienvenida.
2. La pantalla muestra: datos del Familiar, nombre de la persona vinculada y resumen de qué puede hacer en el portal.
3. El Familiar confirma y accede al dashboard.

**Flujo principal — Persona**
1. Tras el primer login exitoso, el sistema muestra pantalla de bienvenida simple.
2. La pantalla muestra: avatar de la Persona y su nombre en tipografía grande.
3. La Persona es dirigida a su roadmap.

**Postcondiciones**
- La pantalla de bienvenida se muestra una sola vez.
