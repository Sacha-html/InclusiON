# ABM — Diagnósticos Funcionales

**Actor:** Profesional  
**Justificación:** El Profesional necesita registrar evaluaciones formales sobre la persona con discapacidad: cuáles son sus capacidades, sus desafíos, qué apoyos requiere y qué objetivos pedagógicos se plantean. Estos diagnósticos son el punto de partida clínico-pedagógico del trabajo con cada persona y la base sobre la que se construye el roadmap. Sin este ABM, el sistema carece de trazabilidad clínica y los reportes de progreso no tienen contexto inicial de referencia.

**Entidades:** `Diagnosis`

---

## Alta — Diagnóstico Funcional

**Actor:** Profesional

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Persona con discapacidad | Referencia | Sí | Debe existir y estar activa; el profesional debe tener asignada a la persona |
| Fecha del diagnóstico | Fecha | Sí | No puede ser futura |
| Diagnóstico principal | Texto (255) | Sí | No vacío |
| Observaciones iniciales | Texto largo | No | — |
| Capacidades identificadas | Texto largo | No | — |
| Desafíos identificados | Texto largo | No | — |
| Apoyos requeridos | Texto largo | No | — |
| Objetivos pedagógicos | Texto largo | No | — |
| Estrategias recomendadas | Texto largo | No | — |

**Resultado:** Se crea `Diagnosis` con `Activo = true`. El profesional autenticado queda registrado como profesional que registra.

---

## Baja — Diagnóstico

**Actor:** Profesional (solo diagnósticos propios, es decir, registrados por el mismo profesional)

- Se establece `Activo = false` (baja lógica).
- **Validación:** No se puede dar de baja si el diagnóstico es el más reciente de la persona y existe un roadmap activo (el roadmap puede estar basado en este diagnóstico).

---

## Modificación — Diagnóstico

**Actor:** Profesional (solo diagnósticos propios)

Todos los campos son editables excepto `PersonaConDiscapacidad` y el `Profesional` que lo registró.

| Campo | Validaciones |
|-------|--------------|
| Fecha del diagnóstico | No puede ser futura |
| Diagnóstico principal | No vacío |
| Resto de campos | Texto libre |

---

## Listado — Diagnósticos de una Persona

**Actor:** Profesional

| Columna | Descripción |
|---------|-------------|
| Fecha | Fecha del diagnóstico |
| Diagnóstico principal | Resumen |
| Profesional | Quién lo registró |
| Estado | Activo / Inactivo |

Ordenado por fecha descendente (el más reciente primero).

**Filtros:** profesional, estado, rango de fechas.  
**Persistencia:** Consulta a `Diagnosis` filtrado por `PersonaConDiscapacidadId`. El profesional autenticado solo puede ver diagnósticos de personas bajo su cargo.
