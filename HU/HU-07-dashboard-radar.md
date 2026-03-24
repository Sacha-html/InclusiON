# HU-07 — Dashboard y Radar Chart de Habilidades

**Proceso relacionado:** 14
**Prioridad:** Alta

---

## Historia de Usuario

**Como** profesional
**Quiero** ver un dashboard con indicadores del estado de mis personas asignadas y un gráfico radar de sus habilidades
**Para** tener una vista rápida del progreso y tomar decisiones pedagógicas informadas

**Como** familiar
**Quiero** ver un resumen de las últimas actividades, mensajes y reportes de mi familiar
**Para** estar al tanto de su progreso sin depender de reuniones presenciales

---

## Descripción funcional

### Dashboard del Profesional
Al ingresar al portal, el profesional ve:
- **Contadores** — Total de personas asignadas, asignaciones pendientes
- **Últimas 5 actividades completadas** — Con nombre de la persona, actividad y resultado
- **Próximas 5 actividades por vencer** — Con fecha límite
- **Mi Aula** — Cards visuales con avatar coloreado de cada persona asignada, acceso rápido al detalle

### Radar Chart de Habilidades
En el perfil de cada persona, el profesional ve un gráfico tipo radar/araña:
- Cada eje representa un área de habilidad activa
- El puntaje de cada eje es el promedio de éxito de las actividades completadas en esa área
- Los ejes usan los colores del catálogo de áreas
- Si no hay datos para un área, se muestra en gris con indicador "Sin datos"

### Dashboard Familiar
Al ingresar al portal, el familiar ve:
- Nombre de la persona vinculada
- Últimas 3 actividades realizadas con resultado
- Indicador de mensajes no leídos
- Acceso a reportes de progreso

---

## Criterios de Aceptación

### Dashboard Profesional
- [ ] Muestra contadores reales calculados desde las asignaciones y actividades
- [ ] Si no hay datos, cada sección muestra un estado vacío con acción sugerida
- [ ] La carga es rápida (< 2 segundos) con indicadores de carga visual

### Radar Chart
- [ ] Solo muestra áreas de habilidad activas del perfil de la persona
- [ ] El puntaje se calcula como promedio de los porcentajes de éxito de actividades completadas
- [ ] Sin respuestas en un área: puntaje 0, indicador visual de "Sin datos"
- [ ] El gráfico se adapta a perfiles de alto contraste

### Dashboard Familiar
- [ ] El familiar solo ve datos de su persona vinculada
- [ ] Muestra estados vacíos cuando no hay actividades, mensajes o reportes
