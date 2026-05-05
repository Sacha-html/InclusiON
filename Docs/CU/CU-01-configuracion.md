# Módulo 1 — Configuración del Sistema

---

## CU-01: Gestionar catálogos del sistema

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Admin Global |
| **Actores secundarios** | — |
| **HU de referencia** | HU-01 |
| **Prioridad** | Crítica |

**Precondiciones**
- Usuario autenticado con rol Admin Global.

**Flujo principal**
1. El Admin accede a la sección Catálogos.
2. El sistema muestra los 6 catálogos disponibles: Tipos de discapacidad, Niveles de autonomía, Categorías de actividad, Áreas de habilidad, Tipos de template, Métodos de login.
3. El Admin selecciona un catálogo.
4. El sistema lista los ítems existentes.
5. El Admin elige crear, editar o desactivar un ítem.
6. El sistema valida que el nombre no esté duplicado dentro del mismo catálogo.
7. El sistema guarda el cambio y lo refleja en todos los formularios que usan ese catálogo.

**Flujos alternativos**
- **6a. Nombre duplicado:** El sistema muestra error "Ya existe un ítem con ese nombre" y bloquea el guardado.
- **5a. Desactivar ítem en uso:** El sistema advierte que el ítem está referenciado y solicita confirmación.

**Postcondiciones**
- El catálogo queda actualizado.
- Los dropdowns del sistema reflejan el cambio inmediatamente.

---

## CU-02: Registrar institución

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Admin Global |
| **Actores secundarios** | — |
| **HU de referencia** | HU-01 |
| **Prioridad** | Crítica |

**Precondiciones**
- Usuario autenticado con rol Admin Global.

**Flujo principal**
1. El Admin accede a la sección Instituciones.
2. El Admin selecciona "Nueva institución".
3. El sistema muestra formulario: nombre, dirección, teléfono, email de contacto.
4. El Admin completa los datos y confirma.
5. El sistema valida unicidad de nombre.
6. El sistema crea la institución y la deja disponible para asignaciones.

**Flujos alternativos**
- **5a. Nombre duplicado:** El sistema muestra error y bloquea el guardado.

**Postcondiciones**
- La institución queda registrada y disponible para asignar profesionales y admins institucionales.

---

## CU-03: Asignar profesional a institución

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Admin Global / Admin Institucional |
| **Actores secundarios** | Profesional (receptor) |
| **HU de referencia** | HU-01 |
| **Prioridad** | Crítica |

**Precondiciones**
- El profesional existe en el sistema con estado activo.
- La institución existe en el sistema.
- El Admin tiene acceso a la institución (Admin Global: todas; Admin Institucional: solo las suyas).

**Flujo principal**
1. El Admin accede al perfil del profesional o a la vista de la institución.
2. El Admin selecciona "Asignar a institución" e indica la institución.
3. El sistema verifica que el vínculo no exista ya.
4. El sistema crea la relación `ProfessionalInstitution` con `IsActive = true`.

**Flujos alternativos**
- **3a. Vínculo ya existe:** El sistema informa que el profesional ya está asignado a esa institución.
- **1a. Desvinculación:** El Admin desactiva el vínculo existente; el historial se conserva (soft-delete lógico).

**Postcondiciones**
- El profesional aparece en la institución.
- El Admin Institucional de esa institución puede ver al profesional en su panel.

---

## CU-04: Asignar persona a profesional

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Admin / Profesional |
| **Actores secundarios** | Persona (receptor) |
| **HU de referencia** | HU-01 |
| **Prioridad** | Crítica |

**Precondiciones**
- La persona existe en el sistema.
- El profesional existe y está activo.

**Flujo principal**
1. El Admin o Profesional accede a la vista de asignaciones de la persona.
2. Selecciona el profesional a asignar e indica si es profesional principal.
3. Indica si ese profesional puede autorizar el login asistido de la persona.
4. El sistema crea la relación `ProfessionalAssignment` con `IsActive = true`.

**Flujos alternativos**
- **2a. Vínculo ya existe:** El sistema informa del vínculo existente.
- **Desvinculación:** La desvinculación es lógica; el historial de actividades y respuestas se conserva.

**Postcondiciones**
- El profesional puede ver a la persona en su panel.
- La persona aparece en el roadmap y Mi Aula del profesional.
