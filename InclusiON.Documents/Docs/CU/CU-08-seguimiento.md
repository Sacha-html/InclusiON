# Módulo 8 — Seguimiento y Resultados

---

## CU-32: Consultar dashboard del profesional

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | — |
| **HU de referencia** | HU-07 |
| **Prioridad** | Alta |

**Precondiciones**
- El Profesional está autenticado.

**Flujo principal**
1. El Profesional accede a su portal. El sistema carga el dashboard como pantalla principal.
2. El sistema muestra en menos de 2 segundos:
   - Contadores: total de personas asignadas, asignaciones pendientes.
   - Últimas 5 actividades completadas (persona, actividad, resultado).
   - Próximas 5 actividades por vencer (con fecha límite).
3. Si alguna sección no tiene datos, muestra estado vacío con acción sugerida.

**Postcondiciones**
- El Profesional tiene una vista rápida del estado de sus personas asignadas.

---

## CU-33: Consultar radar chart de habilidades

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | — |
| **HU de referencia** | HU-07 |
| **Prioridad** | Alta |

**Precondiciones**
- El Profesional tiene asignación activa con la Persona.
- La Persona tiene al menos un área de habilidad en su perfil.

**Flujo principal**
1. El Profesional accede al perfil de una Persona asignada.
2. El sistema muestra el gráfico radar/araña:
   - Cada eje = un área de habilidad activa del perfil.
   - El puntaje del eje = promedio de éxito de actividades completadas en esa área.
   - Los colores de los ejes corresponden al catálogo de áreas.
3. Áreas sin respuestas registradas se muestran en gris con indicador "Sin datos" (puntaje 0).

**Flujos alternativos**
- **Alto contraste activo:** El gráfico adapta la paleta de colores al perfil de accesibilidad.

**Postcondiciones**
- El Profesional puede leer el progreso global de la Persona por área de habilidad.

---

## CU-34: Consultar Mi Aula

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | — |
| **HU de referencia** | HU-07 |
| **Prioridad** | Alta |

**Precondiciones**
- El Profesional está autenticado y tiene al menos una persona asignada.

**Flujo principal**
1. El Profesional accede a la sección "Mi Aula" desde el dashboard o el menú lateral.
2. El sistema muestra una card visual por cada persona asignada: avatar coloreado, nombre, estado del roadmap (último acceso, actividades completadas).
3. El Profesional selecciona una card para acceder al perfil completo de esa persona.

**Flujos alternativos**
- **Sin personas asignadas:** El sistema muestra estado vacío "Aún no tenés personas asignadas."

**Postcondiciones**
- El Profesional accede al perfil completo de la persona seleccionada.

---

## CU-35: Consultar dashboard familiar

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Familiar |
| **Actores secundarios** | — |
| **HU de referencia** | HU-07 |
| **Prioridad** | Alta |

**Precondiciones**
- El Familiar está autenticado.
- Tiene `PersonRepresentative.IsActive = true` para al menos una persona.

**Flujo principal**
1. El Familiar accede a su portal. El sistema carga el dashboard.
2. El sistema muestra:
   - Nombre de la persona vinculada.
   - Últimas 3 actividades realizadas con resultado.
   - Indicador de mensajes no leídos.
   - Acceso a reportes de progreso aprobados.
3. El Familiar solo ve datos de su propia persona vinculada.

**Flujos alternativos**
- **Sin actividades, mensajes o reportes:** Cada sección muestra estado vacío.

**Postcondiciones**
- El Familiar tiene acceso al progreso de su persona vinculada sin depender de reuniones presenciales.

---

## CU-36: Consultar respuestas de una actividad asignada

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | — |
| **HU de referencia** | HU-07 |
| **Prioridad** | Alta |

**Precondiciones**
- El Profesional tiene asignación activa con la Persona.
- La actividad fue ejecutada al menos una vez.

**Flujo principal**
1. El Profesional accede al perfil de la Persona y selecciona una actividad completada.
2. El sistema muestra el detalle de la última ejecución: porcentaje de éxito, intentos, tiempo, nivel de frustración y patrón de respuesta.
3. El Profesional puede ver el historial de todas las ejecuciones anteriores de esa actividad.

**Flujos alternativos**
- **Sin ejecuciones:** El sistema muestra "Esta actividad aún no fue realizada."
- **Profesional sin asignación:** El sistema devuelve `403 Forbidden`.

**Postcondiciones**
- El Profesional puede tomar decisiones pedagógicas basadas en el rendimiento real.
