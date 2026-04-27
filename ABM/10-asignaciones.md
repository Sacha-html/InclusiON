# ABM — Asignaciones de Actividad

**Actor:** Profesional  
**Justificación:** Además del roadmap (secuencia estructurada), el Profesional necesita asignar actividades individuales a una persona con discapacidad fuera del roadmap: tareas para trabajar en casa, actividades de evaluación inicial, refuerzo puntual. Sin este ABM, las personas solo pueden acceder a lo que está en su roadmap y el profesional pierde flexibilidad pedagógica. También es aquí donde se registra la ejecución y los resultados de cada intento.

**Entidades:** `ActivityAssignment`, `ActivityResponse`, `ActivityResult`

---

## Alta — Asignación de Actividad

**Actor:** Profesional

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Actividad | Referencia | Sí | Debe existir y estar activa |
| Persona con discapacidad | Referencia | Sí | Debe existir, estar activa; el profesional debe tener asignada a la persona |
| Fecha límite | Fecha/hora | No | Debe ser futura si se ingresa |
| Orden secuencial | Entero | No | Positivo si se ingresa |
| Es actividad de evaluación | Booleano | Sí | Por defecto: false |

**Validaciones de integridad:**
- No puede existir ya una asignación activa de la misma actividad para la misma persona.

**Resultado:** Se crea `ActivityAssignment` con `Estado = Pendiente`, `Activo = true`.

---

## Baja — Asignación

**Actor:** Profesional

- Se establece `Estado = Cancelada` y `Activo = false`.
- **Validación:** No se puede cancelar si el estado es `Completada`.

---

## Modificación — Asignación

**Actor:** Profesional

| Campo | Validaciones |
|-------|--------------|
| Fecha límite | Futura; solo si estado es `Pendiente` |
| Orden secuencial | Positivo |

El estado se actualiza automáticamente por el sistema cuando la persona realiza la actividad; no se modifica directamente por el profesional.

---

## Registro de Respuesta (automático — por la Persona)

**Actor:** Persona con Discapacidad (al ejecutar la actividad)

Cada intento de resolución genera un `ActivityResponse`. No es un ABM de usuario directo; se registra automáticamente por el sistema.

| Campo | Descripción |
|-------|-------------|
| Asignación | Asignación que se está respondiendo |
| Fecha de inicio | Cuándo comenzó el intento |
| Fecha de finalización | Cuándo terminó |
| Tiempo empleado (seg) | Duración del intento |
| Resultado | Éxito, Parcial, Fallido, Abandonado |
| Porcentaje de éxito | 0 a 100 |
| Cantidad de intentos | Número del intento |
| Patrón de respuesta | JSON con las respuestas individuales |
| Requirió soporte | Si el supervisor intervino |
| Nivel de frustración | 1 a 5 (detectado por el sistema) |
| Observaciones | Notas del profesional post-sesión |

---

## Modificación — Observaciones del Profesional en una Respuesta

**Actor:** Profesional

El profesional puede agregar o editar observaciones en un `ActivityResponse` ya registrado.

| Campo | Validaciones |
|-------|--------------|
| Observaciones | Texto libre hasta 1000 caracteres |

---

## Listado — Asignaciones de una Persona

**Actor:** Profesional

| Columna | Descripción |
|---------|-------------|
| Actividad | Título |
| Fecha de asignación | Cuándo se asignó |
| Fecha límite | Vencimiento (si tiene) |
| Estado | Pendiente / EnProgreso / Completada / Cancelada |
| Intentos realizados | Cantidad de `ActivityResponse` |
| Último resultado | Resultado del último intento |
| Es evaluación | Sí / No |

**Filtros:** estado, es evaluación, actividad.

---

## Listado — Respuestas de una Asignación

**Actor:** Profesional

| Columna | Descripción |
|---------|-------------|
| N° intento | Número de intento |
| Fecha | Cuándo realizó la actividad |
| Tiempo empleado | En minutos:segundos |
| Porcentaje de éxito | % |
| Resultado | Éxito / Parcial / Fallido / Abandonado |
| Requirió soporte | Sí / No |
| Nivel de frustración | 1 a 5 |

**Persistencia:** Consulta a `ActivityAssignment` → `ActivityResponse`, filtrado por persona y/o profesional.
