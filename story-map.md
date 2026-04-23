# InclusiON — Story Map

**Última actualización:** 2026-04-23
**Scope MVP:** Ciclo completo Profesional → Persona con discapacidad

---

## Backbone — Journey del usuario

```
Crear actividad → Armar roadmap → Asignar → [Persona] Ver roadmap → Ejecutar actividad → Responder
```

---

## Mapa completo

### Columna 1 — Crear Actividad *(Profesional)*

| Nivel | Historia | Jira | Estado |
|---|---|---|---|
| Esqueleto caminante | Crear actividad con wizard (área → template → contenido) | IN-105 | ⏳ |
| | Consultar catálogo de actividades propias | IN-107 | ⏳ |
| Mejora | Editar actividad propia | IN-108 | ⏳ |
| Mejora | Desactivar actividad | IN-109 | ⏳ |
| Post-MVP | Integrar pictogramas ARASAAC | IN-106 | ⏳ |
| Post-MVP | Búsqueda semántica por lenguaje natural | IN-135 | ⏳ |

### Columna 2 — Armar Roadmap *(Profesional)*

| Nivel | Historia | Jira | Estado |
|---|---|---|---|
| Esqueleto caminante | Crear roadmap por persona | IN-110 | ⏳ |
| | Agregar actividades al roadmap por área | IN-111 | ⏳ |
| Mejora | Definir orden secuencial y umbral de desbloqueo | IN-112 | ⏳ |
| Mejora | Reordenamiento de actividades drag-drop | IN-113 | ⏳ |
| Mejora | Desbloqueo manual de actividad | IN-114 | ⏳ |
| Mejora | Eliminar actividad del roadmap | IN-115 | ⏳ |
| Post-MVP | Configurar motor adaptativo por actividad | IN-116 | ⏳ |

### Columna 3 — Asignar *(Profesional)*

| Nivel | Historia | Jira | Estado |
|---|---|---|---|
| Esqueleto caminante | Cargar asignación con contenido completo | IN-118 | ⏳ |

### Columna 4 — Ver Roadmap *(Persona con discapacidad)*

| Nivel | Historia | Jira | Estado |
|---|---|---|---|
| Esqueleto caminante | Visualizar roadmap propio estilo Duolingo | IN-117 | ⏳ |
| Mejora | Actividades bloqueadas vs desbloqueadas según umbral | IN-112 | ⏳ |

### Columna 5 — Ejecutar Actividad *(Persona con discapacidad)*

| Nivel | Historia | Jira | Estado |
|---|---|---|---|
| Esqueleto caminante | Iniciar actividad (ActivityPlayerShell) | IN-119 | ⏳ |
| | Player: Selección de figuras | IN-120 | ⏳ |
| | Player: Completar letra | IN-124 | ⏳ |
| Mejora | Player: Suma visual | IN-121 | ⏳ |
| Mejora | Player: Emparejar imagen-palabra | IN-122 | ⏳ |
| Mejora | Player: Ordenar secuencia | IN-123 | ⏳ |

### Columna 6 — Responder *(Persona con discapacidad → sistema)*

| Nivel | Historia | Jira | Estado |
|---|---|---|---|
| Esqueleto caminante | Completar actividad y evaluar resultado | IN-126 | ⏳ |
| | Cifrado automático de respuesta clínica `[Encrypted]` | IN-173 | ✅ |
| Mejora | Desbloqueo automático de siguiente actividad si supera umbral | IN-127 | ⏳ |
| Mejora | Monitoreo de frustración (pausa tras 3+ intentos) | IN-128 | ⏳ |
| Mejora | Evaluación automática de rendimiento tras cada actividad | IN-129 | ⏳ |
| Post-MVP | Ajuste adaptativo de dificultad (cálculo + aplicación) | IN-130, IN-131 | ⏳ |

---

## Releases

### Release 1 — Admin MVP *(ya disponible)*

Todo lo que corre hoy: auth, usuarios, personas, instituciones, diagnósticos, reportes, seguridad.

> Base sólida. Datos clínicos cifrados (IN-173), row-level auth (IN-172), flujo de invitaciones a familia funcionando.

---

### Release 2 — MVP Educativo *(próximo)*

**Objetivo:** un profesional puede crear una actividad y asignarla. Una persona puede verla y responderla.

**Esqueleto caminante:**

```
IN-105 → IN-110/111 → IN-118 → IN-126
IN-107 → IN-110/111 →         → IN-119 + IN-120/124 (2 players mínimo)
                      IN-117 ↗
```

**Criterio de salida:** un profesional crea una actividad de selección (IN-105), la agrega al roadmap (IN-111), la persona la ve en su portal AAC (IN-117), la ejecuta (IN-119 + IN-120) y el sistema registra el resultado (IN-126).

| Jira | Historia | Depende de |
|---|---|---|
| IN-105 | Crear actividad con wizard | — |
| IN-107 | Catálogo de actividades del profesional | IN-105 |
| IN-110 | Crear roadmap por persona | IN-105 |
| IN-111 | Agregar actividades al roadmap por área | IN-110 |
| IN-118 | Cargar asignación con contenido completo | IN-111 |
| IN-117 | Roadmap visual (persona, estilo Duolingo) | IN-110, IN-118 |
| IN-119 | Iniciar actividad — ActivityPlayerShell | IN-118 |
| IN-120 | Player: Selección de figuras | IN-119 |
| IN-124 | Player: Completar letra | IN-119 |
| IN-126 | Completar actividad y evaluar resultado | IN-119 |

---

### Release 3 — Medición y experiencia completa

| Jira | Historia |
|---|---|
| IN-90 | Radar chart de habilidades por área |
| IN-121 | Player: Suma visual |
| IN-122 | Player: Emparejar imagen-palabra |
| IN-123 | Player: Ordenar secuencia |
| IN-127 | Desbloqueo automático por umbral |
| IN-128 | Monitoreo de frustración |
| IN-129 | Evaluación automática de rendimiento |
| IN-140 | Bandeja de entrada de mensajes |
| IN-141 | Envío de mensajes |
| IN-142 | Hilos de conversación |
| IN-99 | Wizard de completado de perfil (profesional) |

---

### Release 4 — Inteligencia

| Jira | Historia | Estado base |
|---|---|---|
| IN-135 | Búsqueda semántica de actividades | 🔧 Entidades + migration listos |
| IN-130 | Cálculo de ajuste según estado | 🔧 Entidades + migration listos |
| IN-131 | Aplicación de ajuste dentro de rangos | 🔧 Entidades + migration listos |
| IN-132 | Registro de ajustes en historial | 🔧 Entidades + migration listos |
| IN-133 | Alerta al profesional en estado de frustración | ⏳ |
| IN-134 | Timeline de historial de ajustes | ⏳ |
| IN-116 | Configuración del motor por actividad (panel) | ⏳ |

---

## Muro actual

```
✅ Setup     ✅ Usuarios     🔴 [IN-105]     ⛔ ×5 columnas
```

**IN-105 es el único desbloqueo.** Todo el eje educativo depende de este ticket.
Una vez que IN-105 esté, IN-110 → IN-111 → IN-118 → IN-119/126 pueden encadenarse sin dependencias externas.

---

## Notas de arquitectura relevantes para el MVP

- `Activity` y `ActivityContent` ya tienen entidades y configuraciones EF. Falta: controller, commands, queries, handlers.
- `ActivityResponse` y `ActivityResult` tienen entidad + `[Encrypted]` ya aplicado (IN-173). Los resultados clínicos llegan cifrados automáticamente sin trabajo adicional.
- `PersonRoadmap`, `PersonRoadmapArea`, `PersonRoadmapActivity` existen como entidades. Falta: handlers.
- `ActivityAssignment` existe. Sin handlers.
- El player shell (IN-119) es un cargador dinámico — selecciona el componente según el `templateType` que devuelve el backend. Los tipos de template ya están expuestos como catálogo (BE-05 ✅).
