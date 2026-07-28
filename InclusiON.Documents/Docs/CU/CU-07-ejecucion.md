# Módulo 7 — Ejecución de Actividades

---

## CU-29: Ejecutar actividad interactiva

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Persona |
| **Actores secundarios** | Sistema |
| **HU de referencia** | HU-06 |
| **Prioridad** | Crítica |

**Precondiciones**
- La Persona está autenticada.
- La actividad está en estado "Desbloqueada" en el roadmap de la Persona.

**Flujo principal**
1. La Persona toca el nodo desbloqueado en su roadmap.
2. El sistema carga la actividad completa según su tipo de plantilla y registra el momento de inicio.
3. El sistema presenta el player correspondiente:

   | Tipo de plantilla | Interacción |
   |-------------------|-------------|
   | Selección de figuras | Instrucción con audio + grilla de pictogramas. Feedback verde/rojo al seleccionar. |
   | Suma visual | Pictogramas animados representando cantidades + botones numéricos con distractores. |
   | Emparejar imagen-palabra | Pictogramas a izquierda, palabras a derecha. La Persona une cada par. |
   | Ordenar secuencia | Cards arrastrables. La Persona ordena y presiona "Verificar". |
   | Completar letra | Pictograma + palabra con espacio en blanco + botones con letras posibles. |

4. La Persona realiza la actividad. El sistema registra cada intento, el tiempo y el patrón de respuesta.
5. Al completar todas las interacciones, el sistema evalúa el resultado (ver CU-30).

**Flujos alternativos**
- **4a. Más de 3 intentos fallidos consecutivos:** El sistema muestra pausa con estímulo positivo e incrementa el nivel de frustración registrado.
- **2a. Tipo de plantilla no reconocido:** El sistema muestra mensaje de error amigable y registra el incidente.
- **Accesibilidad:** La interfaz respeta el perfil de accesibilidad (contraste, fuente, movimiento reducido). Se soporta audio mediante síntesis de voz.

**Postcondiciones**
- El resultado de la actividad queda registrado en el sistema.
- El sistema evalúa si corresponde desbloquear la siguiente actividad (ver CU-31).

---

## CU-30: Registrar resultado de actividad

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Sistema |
| **Actores secundarios** | Persona (desencadenante) |
| **HU de referencia** | HU-06 |
| **Prioridad** | Crítica |

**Precondiciones**
- La Persona completó la ejecución de una actividad (CU-29).

**Flujo principal**
1. El sistema calcula el porcentaje de éxito basado en los intentos y respuestas correctas.
2. El sistema persiste el `ActivityResult` con: porcentaje de éxito, intentos totales, tiempo de ejecución, nivel de frustración, patrón de respuesta (`JsonResponse`).
3. El sistema actualiza el estado del nodo en el roadmap: "Completado" con el porcentaje de éxito.
4. El Profesional puede consultar el resultado desde su dashboard (ver CU-36).

**Postcondiciones**
- El resultado es visible para el Profesional.
- El `ActivityResult` queda almacenado cifrado si contiene datos sensibles.

---

## CU-31: Desbloquear siguiente actividad por umbral

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Sistema |
| **Actores secundarios** | Persona (beneficiaria) |
| **HU de referencia** | HU-06 |
| **Prioridad** | Crítica |

**Precondiciones**
- Se acaba de registrar el resultado de una actividad (CU-30).
- Existe una actividad siguiente en la secuencia del área.

**Flujo principal**
1. El sistema compara el porcentaje de éxito con el umbral de desbloqueo configurado en el roadmap.
2. Si `porcentajeÉxito >= umbral`:
   a. El sistema crea la asignación para la siguiente actividad.
   b. El nodo siguiente pasa a estado "Desbloqueado".
   c. El portal de la Persona muestra celebración con confetti.
3. Si `porcentajeÉxito < umbral`:
   a. La actividad se marca como completada pero la siguiente permanece bloqueada.
   b. La Persona puede volver a intentar la actividad actual.

**Flujos alternativos**
- **2a. No hay siguiente actividad en el área:** El sistema marca el área como completada en el radar chart.
- **Motor adaptativo activo (CU-50):** El sistema ejecuta el ajuste de dificultad antes de determinar el desbloqueo.

**Postcondiciones**
- El estado del roadmap queda actualizado para la Persona y el Profesional.
