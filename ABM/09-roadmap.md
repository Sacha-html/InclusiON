# ABM — Roadmap (Plan de Trabajo)

**Actor:** Profesional  
**Justificación:** El roadmap es el plan de trabajo personalizado de cada persona con discapacidad. El Profesional necesita crearlo y mantenerlo para organizar las actividades por área de habilidad, definir el orden de avance y configurar el motor de dificultad adaptativa. Sin el roadmap, la persona no tiene un recorrido pedagógico estructurado y no puede avanzar de forma secuencial.

**Entidades:** `PersonRoadmap`, `PersonRoadmapArea`, `PersonRoadmapActivity`, `AdaptiveEngineConfig`

---

## Alta — Roadmap de la Persona

**Actor:** Profesional

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Persona con discapacidad | Referencia | Sí | Debe existir, estar activa y NO tener ya un roadmap activo |
| Notas | Texto (2000) | No | — |

**Validaciones de integridad:**
- Cada persona puede tener solo un `PersonRoadmap` activo (relación 1:1).
- El profesional debe tener asignada a la persona.

**Resultado:** Se crea `PersonRoadmap` con `Activo = true`. El profesional creador queda registrado.

---

## Alta — Área en el Roadmap

**Actor:** Profesional

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Roadmap | Referencia | Sí | Debe estar activo |
| Área de habilidad | Referencia | Sí | Debe estar activa; no puede repetirse en el mismo roadmap |
| Orden de visualización | Entero | Sí | Positivo; único dentro del roadmap |

**Resultado:** Se crea `PersonRoadmapArea`.

---

## Alta — Actividad en el Roadmap

**Actor:** Profesional

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Área del roadmap | Referencia | Sí | Debe estar activa |
| Actividad | Referencia | Sí | Debe estar activa; no puede repetirse en la misma área |
| Orden secuencial | Entero | Sí | Positivo; único dentro del área |
| Desbloqueada | Booleano | Sí | La primera actividad de cada área suele empezar desbloqueada |
| Umbral de desbloqueo (%) | Entero | Sí | Entre 0 y 100; por defecto: 60 |
| Tiempo límite (seg) | Entero | No | Positivo si se ingresa |
| Máximo de intentos | Entero | No | Positivo si se ingresa |
| Mostrar pistas | Booleano | Sí | Por defecto: true |
| Nivel de dificultad | Entero (1-3) | Sí | Por defecto: 1 |

**Resultado:** Se crea `PersonRoadmapActivity`.

---

## Alta — Configuración Adaptativa

**Actor:** Profesional (al configurar una actividad del roadmap)

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Actividad del roadmap | Referencia | Sí | 1:1, no puede tener ya una config |
| Habilitado | Booleano | Sí | — |
| Dificultad mínima | Entero | Sí | ≥ 1; menor que máxima |
| Dificultad máxima | Entero | Sí | ≤ 5; mayor que mínima |
| Tiempo mínimo (seg) | Entero | No | — |
| Tiempo máximo (seg) | Entero | No | Mayor que mínimo |
| Éxitos consecutivos para subir | Entero | Sí | ≥ 1; por defecto: 3 |
| Fracasos consecutivos para bajar | Entero | Sí | ≥ 1; por defecto: 2 |
| Umbral de éxito (%) | Entero | Sí | Entre 0 y 100; por defecto: 70 |
| Umbral de frustración | Entero (1-5) | Sí | Por defecto: 3 |

**Resultado:** Se crea `AdaptiveEngineConfig`.

---

## Baja — Actividad del Roadmap

**Actor:** Profesional

- Se establece `Activo = false` en `PersonRoadmapActivity`.
- **Validación:** No puede darse de baja si es la única actividad desbloqueada del área (la persona quedaría sin poder avanzar).

---

## Baja — Área del Roadmap

**Actor:** Profesional

- Se establece `Activo = false` en `PersonRoadmapArea`.
- **Validación:** Se deben dar de baja primero todas las actividades del área.

---

## Modificación — Roadmap

| Entidad | Campos editables |
|---------|-----------------|
| `PersonRoadmap` | Notas |
| `PersonRoadmapArea` | Orden de visualización |
| `PersonRoadmapActivity` | Orden, umbral de desbloqueo, tiempo límite, máximo de intentos, mostrar pistas, nivel de dificultad |
| `AdaptiveEngineConfig` | Todos los campos excepto `ActividadDelRoadmap` |

---

## Listado — Roadmap de una Persona

**Actor:** Profesional / Persona con Discapacidad (vista simplificada)

Vista anidada por área:

```
Área: Comunicación
  └─ Actividad 1 (Desbloqueada) — Nivel 1 — Umbral 60%
  └─ Actividad 2 (Bloqueada)
Área: Lógica-Matemática
  └─ Actividad 1 (Desbloqueada)
```

| Columna por área | Descripción |
|-----------------|-------------|
| Área de habilidad | Nombre e ícono del área |
| Actividades | Cantidad total / completadas |
| Progreso | Porcentaje de avance |

| Columna por actividad | Descripción |
|----------------------|-------------|
| Título | Nombre de la actividad |
| Orden | Posición en la secuencia |
| Desbloqueada | Sí / No |
| Nivel de dificultad | 1 a 3 |
| Motor adaptativo | Activo / Inactivo |
| Umbral de desbloqueo | % |

**Persistencia:** Consulta a `PersonRoadmap` → `PersonRoadmapArea` → `PersonRoadmapActivity` + `AdaptiveEngineConfig`, filtrado por persona.
