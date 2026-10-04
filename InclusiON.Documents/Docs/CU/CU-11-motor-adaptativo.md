# Módulo 11 — Motor Adaptativo (MDA)

El Motor de Dificultad Adaptativa (MDA) calibra de forma automatizada y dinámica el nivel de exigencia de cada actividad dentro de la trayectoria pedagógica (Roadmap) del estudiante. Opera como un agente en segundo plano que analiza el desempeño en tiempo real tras cada ejecución, ajustando la dificultad y activando intervenciones inmediatas ante señales de sobrecarga o frustración para resguardar la experiencia de aprendizaje.

---

## CU-48: Configurar Motor Adaptativo para una Actividad

| Campo | Detalle |
|---|---|
| **Actor principal** | Profesional Docente / Terapeuta |
| **Actores secundarios** | Sistema |
| **HU de referencia** | HU-10 / HU-IN-116 |
| **Prioridad** | Alta |
| **Precondiciones** | La actividad forma parte del Roadmap del estudiante asignado al profesional (`[PersonAccess(AccessMode.Write)]`). |

**Flujo principal**
1. El Profesional accede a la ficha del estudiante en "Mi Aula" y selecciona la solapa "Roadmap".
2. En la tarjeta de la actividad deseada, presiona la acción **"⚙ Motor MDA"**.
3. El sistema recupera la configuración vigente (o valores por defecto si aún no fue configurado) y despliega el modal interactivo con los siguientes parámetros:
   - **Habilitar motor:** switch de activación/desactivación (`isEnabled`).
   - **Rango de dificultad:** nivel mínimo y nivel máximo permitidos (escala del 1 al 10).
   - **Racha de éxitos para escalar:** cantidad de éxitos consecutivos requeridos para subir la dificultad (1 a 20).
   - **Racha de fallos para desescalar:** cantidad de intentos fallidos consecutivos para reducir la dificultad (1 a 20).
   - **Umbral de logro mínimo:** porcentaje de precisión requerido para computar un intento como exitoso (1% a 100%, por defecto 70%).
   - **Umbral de frustración:** nivel de frustración que activa la intervención de contingencia (escala 1 a 5, por defecto 3).
   - **Rango de tiempo límite:** tiempo mínimo y máximo sugerido en segundos (opcional).
4. El Profesional ajusta los parámetros según las necesidades del alumno y presiona "Guardar configuración".
5. El sistema valida que los límites mínimos no superen a los máximos en cada rango y persiste la configuración en la tabla `AdaptiveEngineConfigs`.
6. El sistema notifica al docente: *"Configuración del motor adaptativo guardada exitosamente"*.

**Flujos alternativos**
- **3a. Desactivar motor adaptativo:** El docente desmarca el switch o pulsa "Restablecer / Deshabilitar motor". El sistema desactiva las intervenciones automáticas sobre esa actividad (`DELETE /api/.../adaptive-config`).
- **4a. Inconsistencia de rangos:** Si el nivel mínimo configurado es superior al máximo, el sistema resalta el error inline y bloquea la persistencia.

**Postcondiciones**
- El motor adaptativo queda configurado a medida para esa actividad en el Roadmap específico del alumno.
- El agente en segundo plano evaluará las próximas ejecuciones bajo estos umbrales.

---

## CU-49: Consultar y Exportar Historial de Ajustes Adaptativos

| Campo | Detalle |
|---|---|
| **Actor principal** | Profesional Docente / Terapeuta |
| **Actores secundarios** | Sistema (generación de exportable CSV) |
| **HU de referencia** | HU-10 / HU-IN-134 |
| **Prioridad** | Media |
| **Precondiciones** | La actividad se encuentra en el Roadmap del estudiante asignado al Profesional (`[PersonAccess(AccessMode.Read)]`). |

**Flujo principal**
1. El Profesional ingresa a la solapa "Roadmap" en el perfil del estudiante.
2. En la actividad correspondiente, presiona la acción **"Ver historial MDA"**.
3. El sistema consulta `GET /api/persons/{id}/roadmap/areas/{areaId}/activities/{actId}/adjustments` y despliega la lista cronológica descendente de ajustes:
   - Badge **Verde (`DifficultyUp`)**: escalamiento de dificultad por consolidación de aprendizaje.
   - Badge **Amarillo (`DifficultyDown`)**: desescalamiento pedagógico preventivo por reiteración de fallos.
   - Badge **Rojo (`FrustrationIntervention`)**: intervención crítica ante detección de sobrecarga o frustración del alumno.
4. Cada entrada expone: tipo de ajuste, valor previo, nuevo valor asignado, motivo pedagógico registrado por el agente y fecha/hora exacta.
5. El Profesional puede presionar **"⬇ Exportar CSV"** para descargar la planilla de auditoría evolutiva de la actividad.

**Flujos alternativos**
- **Sin ajustes registrados:** El sistema informa en el panel: *"Sin ajustes registrados. El motor aún no intervino en esta actividad."*

**Postcondiciones**
- El docente cuenta con evidencia cuantitativa sobre la respuesta del estudiante a la dificultad propuesta, permitiendo calibrar apoyos o derivar a revisión interdisciplinaria.

---

## CU-50: Ajustar Dificultad Automáticamente según Rendimiento y Frustración

| Campo | Detalle |
|---|---|
| **Actor principal** | Sistema (Agente `AdaptiveAdjustmentAgent` en segundo plano) |
| **Actores secundarios** | Profesional Docente (notificado en caso de frustración) |
| **HU de referencia** | HU-10 / HU-IN-128–131 |
| **Prioridad** | Crítica |
| **Precondiciones** | El alumno completó una sesión de actividad vinculada a su Roadmap y el motor adaptativo está habilitado para dicha actividad. |

**Flujo principal**
1. Al registrarse la finalización de una actividad (`CompleteActivityResponseCommand`), el sistema persiste el resultado y encola de forma asíncrona un trabajo de fondo tipo `JobTypes.AdaptiveAdjustment`.
2. El agente `AdaptiveAdjustmentAgent` toma el trabajo y recupera las respuestas recientes y la configuración activa del alumno.
3. El agente evalúa las métricas según las reglas pedagógicas:
   - **Caso Frustración:** Si el nivel de frustración reportado alcanza o supera el umbral (`latestResponse.FrustrationLevel >= config.FrustrationThreshold`), clasifica como `FrustrationIntervention`.
   - **Caso Progreso:** Si se alcanza la racha de éxitos requerida con porcentaje de logro suficiente y el nivel actual es menor al máximo, clasifica como `DifficultyUp`.
   - **Caso Dificultad persistente:** Si se acumulan los fallos consecutivos configurados y el nivel actual es mayor al mínimo, clasifica como `DifficultyDown`.
4. El agente calcula el nuevo nivel de dificultad respetando estrictamente los topes mínimo y máximo configurados por el docente.
5. El agente persiste el cambio de dificultad en `PersonRoadmapActivities` y asienta el registro con motivo detallado en `AdaptiveAdjustmentLogs`.
6. Si el ajuste fue motivado por frustración (`FrustrationIntervention`), el agente encola inmediatamente una alerta push en tiempo real para el Profesional asignado (*"Alerta de frustración: El alumno presentó nivel de frustración elevado. Se redujo la dificultad automáticamente"*).

**Flujos alternativos**
- **Rendimiento estable dentro de parámetros:** Si no se cumplen las condiciones de subida, bajada ni frustración, el agente finaliza sin alterar la dificultad.
- **Motor desactivado o ausente:** El agente no realiza ninguna acción.

**Postcondiciones**
- La próxima sesión de la actividad se iniciará automáticamente con el nivel de dificultad adaptado.
- Queda registrado el evento en el historial del Roadmap (CU-49) y el docente queda alertado si hubo signos de sobrecarga emocional.
