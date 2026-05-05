# Módulo 6 — Roadmap

---

## CU-23: Crear roadmap de una persona

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | — |
| **HU de referencia** | HU-05 |
| **Prioridad** | Crítica |

**Precondiciones**
- El Profesional tiene asignación activa con la Persona.
- La Persona tiene al menos un área de habilidad asignada en su perfil.

**Flujo principal**
1. El Profesional accede al perfil de la Persona y entra a "Roadmap".
2. El sistema muestra las áreas de habilidad activas del perfil de la Persona.
3. El Profesional agrega la primera actividad a un área (ver CU-24).
4. El sistema crea el roadmap con la primera actividad desbloqueada automáticamente.

**Flujos alternativos**
- **2a. Sin áreas de habilidad:** El sistema muestra estado vacío con acción sugerida "Configurar perfil de habilidades primero".

**Postcondiciones**
- El roadmap existe para la Persona.
- La Persona puede ver el nodo desbloqueado en su portal.

---

## CU-24: Agregar actividad al roadmap

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | — |
| **HU de referencia** | HU-05 |
| **Prioridad** | Crítica |

**Precondiciones**
- Existe un roadmap para la Persona.
- La actividad pertenece a un área activa en el perfil de habilidades de la Persona.
- La actividad no fue agregada anteriormente en esa área.

**Flujo principal**
1. El Profesional selecciona un área del roadmap y elige "Agregar actividad".
2. El sistema muestra el catálogo de actividades filtrado por el área seleccionada.
3. El Profesional selecciona la actividad y define el umbral de desbloqueo (porcentaje mínimo de éxito para avanzar, 0–100%).
4. El sistema agrega la actividad al final de la secuencia del área.
5. Si es la primera actividad de esa área, el sistema la desbloquea automáticamente creando la asignación.

**Flujos alternativos**
- **3a. Actividad ya agregada en esa área:** El sistema bloquea y muestra "Esta actividad ya está en el roadmap de esta área".
- **3b. Actividad de área diferente:** El sistema filtra y no la muestra.

**Postcondiciones**
- La actividad aparece en el roadmap de la Persona.
- Si es la primera del área, la Persona ya puede iniciarla.

---

## CU-25: Reordenar actividades del roadmap

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | — |
| **HU de referencia** | HU-05 |
| **Prioridad** | Media |

**Precondiciones**
- El roadmap existe y el área tiene más de una actividad.
- La actividad a mover **no** tiene respuestas registradas.

**Flujo principal**
1. El Profesional accede al roadmap y entra al editor del área.
2. Arrastra y suelta la actividad a la posición deseada.
3. El sistema actualiza el orden secuencial de las actividades del área.
4. El sistema recalcula el estado de desbloqueo de los nodos afectados.

**Flujos alternativos**
- **2a. Actividad con respuestas:** El sistema bloquea el movimiento y muestra "No se puede reordenar una actividad que ya tiene respuestas registradas".

**Postcondiciones**
- El orden actualizado se refleja en el roadmap visual de la Persona.

---

## CU-26: Forzar desbloqueo manual de actividad

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | — |
| **HU de referencia** | HU-05 |
| **Prioridad** | Media |

**Precondiciones**
- La actividad está en estado bloqueado en el roadmap.
- El Profesional tiene asignación activa con la Persona.

**Flujo principal**
1. El Profesional selecciona la actividad bloqueada en el editor del roadmap.
2. Elige "Desbloquear manualmente".
3. El sistema omite el umbral automático y crea la asignación de la actividad para la Persona.
4. El nodo pasa a estado "Desbloqueado" en el portal de la Persona.

**Postcondiciones**
- La Persona puede iniciar la actividad aunque no haya alcanzado el umbral de la anterior.

---

## CU-27: Eliminar actividad del roadmap

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | — |
| **HU de referencia** | HU-05 |
| **Prioridad** | Baja |

**Precondiciones**
- La actividad **no** tiene respuestas registradas.
- El Profesional tiene asignación activa con la Persona.

**Flujo principal**
1. El Profesional selecciona la actividad en el editor del roadmap.
2. Elige "Eliminar del roadmap".
3. El sistema verifica que no existan respuestas asociadas.
4. El sistema elimina la actividad del roadmap y reordena la secuencia del área.

**Flujos alternativos**
- **3a. Actividad con respuestas:** El sistema bloquea la eliminación. "No se puede eliminar una actividad con respuestas registradas. Podés desactivarla desde el catálogo."

**Postcondiciones**
- La actividad desaparece del roadmap y del portal de la Persona.

---

## CU-28: Consultar roadmap propio

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Persona |
| **Actores secundarios** | — |
| **HU de referencia** | HU-05 |
| **Prioridad** | Crítica |

**Precondiciones**
- La Persona está autenticada.
- Tiene un roadmap con al menos una actividad.

**Flujo principal**
1. La Persona accede a su portal. El roadmap se muestra como pantalla principal.
2. El sistema presenta un camino visual estilo Duolingo con nodos por área:
   - **Completado:** check verde con porcentaje de éxito.
   - **Desbloqueado:** pulso brillante, tocable para iniciar la actividad.
   - **Bloqueado:** candado gris, sin título visible, no interactivo.
3. La Persona toca un nodo desbloqueado para iniciar la actividad (ver CU-29).
4. Al volver tras completar una actividad, el sistema muestra celebración con confetti si se desbloqueó la siguiente.

**Flujos alternativos**
- **2a. Sin roadmap:** El sistema muestra pantalla de bienvenida indicando que su profesional pronto le asignará actividades.

**Postcondiciones**
- La Persona visualiza su progreso e inicia actividades disponibles.
- Las animaciones respetan la preferencia de movimiento reducido del sistema operativo.
