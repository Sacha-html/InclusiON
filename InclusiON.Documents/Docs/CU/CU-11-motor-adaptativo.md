# Módulo 11 — Motor Adaptativo

---

## CU-48: Configurar motor adaptativo para una actividad

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | — |
| **HU de referencia** | HU-10 |
| **Prioridad** | Alta |

**Precondiciones**
- La actividad está en el roadmap de una Persona asignada al Profesional.

**Flujo principal**
1. El Profesional accede a la actividad en el editor del roadmap.
2. Selecciona "Configurar motor adaptativo".
3. El sistema muestra el panel de configuración con:
   - Activar / Desactivar el motor para esta actividad.
   - Rango de dificultad: mínimo y máximo (escala 1–5).
   - Rango de tiempo límite: mínimo y máximo (en segundos).
   - Umbral de éxitos consecutivos para escalar dificultad.
   - Umbral de fracasos consecutivos para desescalar.
   - Porcentaje de éxito mínimo aceptable.
   - Umbral de frustración (nivel de frustración que activa el estado crítico).
4. El Profesional configura los valores y guarda.
5. El sistema valida que el mínimo no supere el máximo en cada rango.

**Flujos alternativos**
- **4a. Motor desactivado:** El sistema no interviene en la ejecución de esa actividad.
- **5a. Validación de rangos:** Si mínimo > máximo, el sistema muestra error inline.
- **Restaurar valores por defecto:** El Profesional puede resetear toda la configuración a los valores predeterminados del sistema.

**Postcondiciones**
- El motor adaptativo queda configurado para esa actividad en el roadmap de esa Persona específica.
- El Sistema aplica la configuración en la próxima ejecución de la actividad.

---

## CU-49: Consultar historial de ajustes adaptativos

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | — |
| **HU de referencia** | HU-10 |
| **Prioridad** | Media |

**Precondiciones**
- El motor adaptativo está o estuvo activo para la actividad.
- Existen ajustes registrados (al menos una ejecución posterior a la activación).

**Flujo principal**
1. El Profesional accede a la actividad en el roadmap y selecciona "Ver ajustes adaptativos".
2. El sistema muestra el historial en formato timeline cronológico descendente:
   - **Verde** — Escalamiento (progreso): se subió dificultad / se redujo tiempo o pistas.
   - **Amarillo** — Desescalamiento (dificultad): se bajó dificultad / se aumentó tiempo o pistas.
   - **Rojo** — Frustración (alerta): se bajó todo al mínimo y se notificó al Profesional.
3. Cada entrada muestra: tipo de ajuste, valores anteriores y nuevos, motivo y fecha/hora.

**Flujos alternativos**
- **Sin ajustes:** El sistema muestra "El motor aún no realizó ningún ajuste para esta actividad."

**Postcondiciones**
- El Profesional puede evaluar si la configuración del motor es adecuada para la Persona.

---

## CU-50: Ajustar dificultad automáticamente según rendimiento

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Sistema |
| **Actores secundarios** | Profesional (notificado en caso de frustración) |
| **HU de referencia** | HU-10 |
| **Prioridad** | Alta |

**Precondiciones**
- El motor adaptativo está activo y configurado para la actividad ejecutada.
- Se acaba de registrar el resultado de una ejecución (CU-30).

**Flujo principal**
1. El sistema evalúa el rendimiento acumulado de la Persona en esa actividad.
2. Determina el estado adaptativo:

   | Estado | Condición | Acción del sistema |
   |--------|-----------|-------------------|
   | **Estable** | Rendimiento consistente dentro de rangos | Mantiene parámetros sin cambios |
   | **Progresando** | N éxitos consecutivos ≥ umbral configurado | Sube dificultad, reduce tiempo, reduce pistas e intentos |
   | **Dificultad** | N fracasos consecutivos ≥ umbral configurado | Baja dificultad, aumenta tiempo, agrega pistas e intentos |
   | **Frustración** | Nivel de frustración ≥ umbral o 3+ abandonos | Baja todo al mínimo, envía alerta al Profesional |

3. El sistema aplica el ajuste dentro de los rangos configurados (nunca supera mínimos ni máximos).
4. El sistema registra el ajuste en el historial con tipo, valores previos, valores nuevos, motivo, fecha y hora.

**Flujos alternativos**
- **Estado Frustración:** El sistema envía notificación al Profesional con alerta de nivel crítico de frustración de la Persona.
- **Sin configuración o motor desactivado:** El sistema no interviene.

**Postcondiciones**
- Los parámetros de la actividad quedan ajustados para la próxima ejecución.
- El ajuste queda registrado en el historial accesible para el Profesional (CU-49).
