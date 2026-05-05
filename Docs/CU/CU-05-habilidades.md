# Módulo 5 — Perfil de Habilidades

---

## CU-21: Asignar áreas de habilidad a una persona

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | — |
| **HU de referencia** | HU-03 |
| **Prioridad** | Crítica |

**Precondiciones**
- El Profesional está autenticado.
- El Profesional tiene asignación activa con la Persona.
- Existen áreas de habilidad cargadas en el catálogo.

**Flujo principal**
1. El Profesional accede al perfil de la Persona y entra a la sección "Perfil de habilidades".
2. El sistema muestra las áreas disponibles del catálogo, excluyendo las ya asignadas.
3. El Profesional selecciona una o más áreas y confirma.
4. El sistema crea las relaciones `PersonSkillArea` con `IsActive = true`.
5. Las áreas se muestran como etiquetas con el color e ícono definido en el catálogo.

**Flujos alternativos**
- **3a. Área ya asignada:** El sistema la excluye de la lista de disponibles; no permite duplicados.
- **1a. Profesional sin asignación:** El sistema devuelve `403 Forbidden`.

**Postcondiciones**
- Las áreas asignadas aparecen en el roadmap de la Persona y en el radar chart de habilidades.
- El Profesional puede agregar actividades de esas áreas al roadmap.

---

## CU-22: Desactivar área de habilidad de una persona

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | — |
| **HU de referencia** | HU-03 |
| **Prioridad** | Media |

**Precondiciones**
- El Profesional está autenticado y tiene asignación activa con la Persona.
- El área está actualmente asignada a la Persona.

**Flujo principal**
1. El Profesional accede al perfil de habilidades de la Persona.
2. Selecciona "Desactivar" en el área que ya no se va a trabajar.
3. El sistema marca `PersonSkillArea.IsActive = false`.
4. El área deja de aparecer en el roadmap y en el radar chart activo.

**Flujos alternativos**
- **2a. Área con actividades en roadmap activo:** El sistema muestra aviso "Esta área tiene actividades en el roadmap. Desactivarla las ocultará del plan de trabajo." y solicita confirmación.

**Postcondiciones**
- El historial de actividades y respuestas de esa área se conserva (no se elimina).
- El área desaparece del radar chart y del roadmap activo.
