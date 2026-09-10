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

## CU-34: Consultar y gestionar Mi Aula (`Classrooms`)

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | Sistema |
| **HU de referencia** | HU-07, HU-17 (IN-301, IN-304) |
| **Prioridad** | Alta |

**Precondiciones**
- El Profesional está autenticado.

**Flujo principal**
1. El Profesional accede a la sección "Mi Aula" (`/pro/aulas`).
2. El sistema lista las aulas creadas por el profesional (`GetClassroomsByProfessionalQuery`).
3. Para cada aula, muestra los alumnos asignados con avatar, nombre, nivel de autonomía y estado del roadmap.
4. El Profesional puede:
   - Crear una nueva aula indicando su nombre (`CreateClassroomCommand` / IN-301). El aula se crea vacía sin obligar alumnos.
   - Modificar la denominación de un aula existente (`UpdateClassroomCommand`).
   - Reasignar o mover un alumno a otra de sus aulas (`MovePersonToClassroomCommand`).
   - Dar de baja un aula desocupada (`DeactivateClassroomCommand`).
5. Al seleccionar la card de un alumno, el sistema redirige a su perfil clínico/pedagógico detallado.

**Flujos alternativos**
- **Sin aulas creadas:** El sistema ofrece crear la primera aula o muestra alumnos sin aula asignada.
- **Intento de desactivar aula con alumnos:** El sistema exige reasignar o desvincular a los alumnos previamente.

**Postcondiciones**
- La estructura de aulas y las asignaciones alumno-profesional persisten en la base de datos relacional.

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

---

## CU-37: Consultar métricas analíticas y KPIs pedagógicos (GAS)

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | Sistema |
| **HU de referencia** | HU-07, HU-19 |
| **Prioridad** | Alta |

**Precondiciones**
- El Profesional está autenticado.
- Existen sesiones analíticas registradas en la base de datos (`ActivitySession`).

**Flujo principal**
1. El Profesional ingresa a la sección de Analítica (`/pro/analytics`).
2. El sistema consume el endpoint `GET /api/analytics/professional`.
3. El Profesional puede aplicar filtros:
   - Aula específica (`classroomId` / `aulaId`).
   - Rango de fechas (`desde` y `hasta`).
4. El sistema consolida y renderiza los siguientes indicadores:
   - **Puntuación GAS (*Goal Attainment Scaling*):** Promedio en escala continua [-2, +2] que refleja si los objetivos pedagógicos están por debajo (-2, -1), en el nivel esperado (0) o superados (+1, +2).
   - **Tasa de éxito promedio (`SuccessRate`):** Porcentaje global y desglose por área de habilidad.
   - **Tasa de error promedio (`ErrorCount`):** Cantidad de fallas por sesión (rango 0 a 6).
   - **Tiempo dedicado (`TimeSpentSeconds`):** Duración media por sesión interactiva.
   - **Distribución de competencias:** Gráfico de rendimiento comparado por áreas de habilidad.
   - **Detección de alertas:** Indicadores de frustración o estancamiento para actividades con alta tasa de fallas repetidas.

**Flujos alternativos**
- **Sin registros en el rango seleccionado:** Muestra estado informativo "No se registraron sesiones en el período indicado" invitando a ampliar el filtro temporal.
- **Profesional sin alumnos vinculados:** Muestra panel vacío con invitación a matricular alumnos o crear aulas.

**Postcondiciones**
- El Profesional dispone de métricas cuantitativas y estandarizadas (GAS) para seguimiento clínico y emisión de reportes.

