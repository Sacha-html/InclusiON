# HU-05 — Roadmap: Plan de Trabajo Personalizado

| Campo | Contenido |
|---|---|
| ID | HU-05 |
| Épica | Roadmap y Progreso |
| Título | Roadmap: Plan de Trabajo Personalizado |
| Prioridad | Crítica |
| Estimación | 8 puntos de historia |
| Sprint asignado | Sprint 6 |
| Estado | Completada |

**Proceso relacionado:** 10, 11

---

## Historia de Usuario

**Como** profesional
**Quiero** armar un plan de trabajo con actividades secuenciadas por área de habilidad para cada persona
**Para** que el aprendizaje sea progresivo y las actividades se desbloqueen según el rendimiento del estudiante

**Como** persona con discapacidad
**Quiero** ver mi progreso como un camino visual con nodos por área
**Para** entender intuitivamente dónde estoy y qué me falta por hacer

---

## Descripción funcional

### Vista del profesional
El profesional arma el roadmap desde el perfil de la persona:
- **Agrega actividades** a cada área de habilidad del perfil, definiendo el orden secuencial y el umbral de desbloqueo (porcentaje mínimo de éxito para avanzar a la siguiente)
- **Reordena actividades** mediante arrastrar y soltar
- **Fuerza el desbloqueo** manual de una actividad (override del umbral automático)
- **Elimina actividades** del roadmap (solo si no tienen respuestas registradas)

La primera actividad de cada área se desbloquea automáticamente.

### Vista del estudiante
La persona ve su roadmap como un camino visual estilo Duolingo:
- **Nodos completados** — Check verde con porcentaje de éxito
- **Nodo desbloqueado** — Pulso brillante, se puede tocar para iniciar la actividad
- **Nodos bloqueados** — Candado gris, sin título visible, no interactivo
- Al volver de completar una actividad se muestra una celebración con confetti

---

## Criterios de Aceptación

### Profesional
- [ ] Solo se pueden agregar actividades de las áreas del perfil de habilidades de la persona
- [x] El orden dentro de cada área es único y secuencial
- [x] La primera actividad de cada área se desbloquea automáticamente al agregarla
- [ ] No se puede reordenar ni eliminar una actividad que ya tiene respuestas
- [ ] El desbloqueo manual también crea la asignación de actividad correspondiente
- [x] No se puede agregar la misma actividad dos veces en la misma área

### Estudiante
- [x] Los nodos bloqueados muestran solo "Bloqueada" sin revelar el título
- [x] Al tocar un nodo desbloqueado se abre la actividad correspondiente
- [x] La interfaz respeta el perfil de accesibilidad configurado (contraste, tamaño de fuente)
- [x] Las animaciones respetan la preferencia de movimiento reducido del sistema operativo
